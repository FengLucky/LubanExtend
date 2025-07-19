using System.Text;
using Luban.CodeFormat;
using Luban.CSharp.TemplateExtensions;
using Luban.Datas;
using Luban.Defs;
using Luban.TemplateExtensions;
using Luban.TypeVisitors;
using Scriban;

namespace Luban.Extend;

[ConstTarget("csharp")]
public class CsharpConstTarget: ConstTargetBase
{
    protected override string OutputFileExt => "cs";
    protected override DecoratorFuncVisitor<string> DeclaringTypeNameVisitor => CSharp.TypeVisitors.DeclaringTypeNameVisitor.Ins;

    [ThreadStatic]
    private static StringBuilder? _sb;
    protected override string GetUnionIndexType(List<DefField> fields)
    {
        if (_sb == null)
        {
            _sb = new();
        }
        else
        {
            _sb.Clear();
        }

        _sb.Append('(');
        var first = true;
        foreach (var field in fields)
        {
            if (!first)
            {
                _sb.Append(',');
            }
            first = false;
            _sb.Append(field.CType.Apply(DeclaringTypeNameVisitor)).Append(' ').Append(TypeTemplateExtension.FormatFieldName(CodeFormatManager.Ins.CsharpDefaultCodeStyle, field.Name));
        }
        _sb.Append(')');
        return _sb.ToString();
    }

    protected override string GetUnionIndexValue(List<DType> values)
    {
        if (_sb == null)
        {
            _sb = new();
        }
        else
        {
            _sb.Clear();
        }
        
        _sb.Append('(');
        var first = true;
        foreach (var value in values)
        {
            if (!first)
            {
                _sb.Append(',');
            }
            first = false;
            _sb.Append(value.Apply(ConstValueVisitor.Ins));
        }
        _sb.Append(')');
        return _sb.ToString();
    }

    protected override void PushEnvs(TemplateContext context)
    {
        base.PushEnvs(context);
        context.PushGlobal(new CsharpTemplateExtension());
    }
}
