using System.Reflection;
using Luban.CodeFormat;
using Luban.CodeFormat.CodeStyles;
using Luban.CodeTarget;
using Luban.Datas;
using Luban.DataVisitors;
using Luban.Defs;
using Luban.TemplateExtensions;
using Luban.Tmpl;
using Luban.Types;
using Luban.TypeVisitors;
using Scriban;
using Scriban.Runtime;

namespace Luban.Extend;

public record class ConstInfo(string Type,string Name,string Value, string? Comment);

public abstract class ConstTargetBase : IConstTarget
{
    protected string TargetName { get; private set; }
    protected string Directory { get; private set; }
    protected string ClassNameFormat { get; private set; }
    protected GenerationContext Ctx { get; private set; }
    protected virtual ICodeStyle CodeStyle => _codeStyle ??= CreateConfigurableCodeStyle();

    private ICodeStyle _codeStyle;
    protected virtual string Header { get; } = string.Empty;
    protected abstract string OutputFileExt { get; }
    protected abstract DecoratorFuncVisitor<string> DeclaringTypeNameVisitor { get; }
    protected virtual IDataFuncVisitor<string> ValueVisitor => ConstValueVisitor.Ins;
    protected abstract string GetUnionIndexType(List<DefField> fields);
    protected abstract string GetUnionIndexValue(List<DType> values);
    
    private ICodeStyle CreateConfigurableCodeStyle()
    {
        var baseStyle = GenerationContext.Current.GetCodeStyle(TargetName) ?? CodeFormatManager.Ins.NoneCodeStyle;

        var env = EnvManager.Current;
        string namingKey = BuiltinOptionNames.NamingConvention;
        return new OverlayCodeStyle(baseStyle,
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "namespace", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "type", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "method", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "property", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "field", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{TargetName}", "enumItem", true, "")
        );
    }
    
    public virtual List<ConstInfo>? GetConstInfos(DefTable table, List<Record> records)
    {
        var constFields = table.ValueTType.DefBean.Fields.FindAll(field => field.Groups.FindIndex(group => group == "const") > -1 && field.CType is TString);
        if (constFields.Count == 0)
        {
            return null;
        }

        if (table.IndexList.Count == 0)
        {
            throw new Exception($"{table.Name} 不存在索引字段，无法生成常量数据");
        }

        var infos = new List<ConstInfo>();
        if (table.IsUnionIndex)
        {
            var types = new List<DefField>();
            foreach (var indexInfo in table.IndexList)
            {
                types.Add(indexInfo.IndexField);
            }

            var type = GetUnionIndexType(types);
            foreach (var field in constFields)
            {
                int commentIndex = GetCommentIndex(table,field);
                var indexValues = new List<DType>();
                var constIndex = table.ValueTType.DefBean.Fields.IndexOf(field);
                foreach (var record in records)
                {
                    indexValues.Clear();
                    foreach (var indexInfo in table.IndexList)
                    {
                        indexValues.Add(record.Data.Fields[indexInfo.IndexFieldIdIndex]);
                    }

                    var name = (record.Data.Fields[constIndex] as DString)?.Value ?? string.Empty;
                    var value = GetUnionIndexValue(indexValues);
                    var comment = GetComment(record, commentIndex);
                    infos.Add(new ConstInfo(type,name,value,comment));
                }
            }
        }
        else
        {
            foreach (var field in constFields)
            {
                int commentIndex = GetCommentIndex(table,field);
                var indexFieldIndex = table.IndexList[0].IndexFieldIdIndex;
                var constIndex = table.ValueTType.DefBean.Fields.IndexOf(field);
                if (field.Tags.TryGetValue("index", out var indexName))
                {
                    indexFieldIndex = GetIndexFieldIndex(table,indexName);
                    if (indexFieldIndex == -1)
                    {
                        throw new Exception($"{table.Name} 中不存在字段 {indexName} 无法作为常量 {field.Name} 的索引字段");
                    }
                }
                var type = table.ValueTType.DefBean.Fields[indexFieldIndex].CType.Apply(DeclaringTypeNameVisitor);
                foreach (var record in records)
                {
                    var name = (record.Data.Fields[constIndex] as DString)?.Value ?? string.Empty;
                    var value = record.Data.Fields[indexFieldIndex].Apply(ValueVisitor);
                    var comment = GetComment(record, commentIndex);
                    infos.Add(new ConstInfo(type,name,value,comment));
                }
            }
        }
        
        return infos;
    }

    private int GetCommentIndex(DefTable table,DefField field)
    {
        if (field.Tags.TryGetValue("comment", out var commentField))
        {
            var commentIndex = table.ValueTType.DefBean.Fields.FindIndex(field => field.CurrentVariantNameWithFieldNameOrOrigin == commentField);
            if (table.ValueTType.DefBean.Fields[commentIndex].CType is not TString)
            {
                return -1;
            }

            return commentIndex;
        }

        return -1;
    }
    
    private int GetIndexFieldIndex(DefTable table,string name)
    {
        return table.ValueTType.DefBean.Fields.FindIndex(field => field.CurrentVariantNameWithFieldNameOrOrigin == name);
    }

    private string? GetComment(Record record, int index)
    {
        if (index > -1)
        {
            return (record.Data.Fields[index] as DString)?.Value;
        }

        return null;
    }

    protected virtual void Render(string className,List<ConstInfo> infos,CodeWriter writer)
    {
        var templatePath = $"const/{GetType().GetCustomAttribute<ConstTargetAttribute>()?.Name ?? string.Empty}/const";
        if (!TemplateManager.Ins.TryGetTemplate(templatePath, out var template))
        {
            throw new Exception($"template:{templatePath}.sbn not found");
        }
        
        var ctx = new TemplateContext
        {
            LoopLimit = 0,
            NewLine = "\n",
        };
        var extraEnvs = new ScriptObject
        {
            {"__namespace",Ctx.Target.TopModule},
            {"__name",className},
            {"__infos",infos},
            {"__code_style", CodeStyle},
        };
        ctx.PushGlobal(new ContextTemplateExtension());
        ctx.PushGlobal(new TypeTemplateExtension());
        ctx.PushGlobal(extraEnvs);
        PushEnvs(ctx);
        writer.Write(template.Render(ctx));
    }

    protected virtual void PushEnvs(TemplateContext context)
    {
        
    }

    protected virtual void Handle(GenerationContext ctx,List<DefTable> tables,OutputFileManifest manifest)
    {
        var tasks = tables.Select(table => Task.Run(() =>
        {
            var infos = GetConstInfos(table, ctx.GetTableExportDataList(table));
            if (infos == null || infos.Count == 0)
            {
                return;
            }

            var writer = new CodeWriter();
            var className = string.Format(ClassNameFormat, table.Name);
            Render(className,infos,writer);
            manifest.AddFile(new OutputFile
            {
                Content = writer.ToResult(Header),
                File = $"{Directory}/{className}.{OutputFileExt}",
            });
        })).ToArray();
        Task.WaitAll(tasks);
    }
    
    public void Handle(GenerationContext ctx,string targetName,OutputFileManifest manifest)
    {
        TargetName = targetName;
        Ctx = ctx;
        Directory = EnvManager.Current.GetOptionOrDefault(targetName, "constDirectory", true, "Const");
        ClassNameFormat = EnvManager.Current.GetOptionOrDefault(targetName, "classNameFormat", true, "{0}Const");
        Handle(ctx,ctx.ExportTables,manifest);
    }
}
