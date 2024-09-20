using System.Text;
using Luban.Datas;
using Luban.Defs;
using NLog;

namespace Luban.DataTarget.Const;

public sealed class PageTableConst
{
    public const string Family = "page";
}

[DataTarget("const-cs")]
public class CsharpConstTarget: ConstTargetBase
{
    protected override string DefaultOutputFileExt => "cs";
    private readonly string[] _validIndexTypes = { "short","int", "long", "string" };

    protected override string? GenCode(DefTable table, List<Record> records, int constIndex)
    {
        var logger = LogManager.GetCurrentClassLogger();
        if (table.IndexList.Count != 1)
        {
            logger.Error($"{table.FullName} 表中索引有多个，不支持生成 const 代码");
            return null;
        }
        
        if (!_validIndexTypes.Contains(table.IndexField.CType.TypeName))
        {
            logger.Error($"{table.FullName} 表中索引类型为 {table.IndexField.CType.TypeName}，不支持生成 const 代码");
            return null;
        }
        
        var indent = 0;
        var sb = new StringBuilder();
      
        if (!string.IsNullOrWhiteSpace(table.NamespaceWithTopModule))
        {
            sb.Append("namespace ").Append(table.NamespaceWithTopModule).AppendLine();
            sb.Append("{").AppendLine();
            indent++;
        }

        sb.Append('\t', indent).Append("public sealed class ").Append(table.Name).Append("Const").AppendLine();
        sb.Append('\t', indent).Append("{").AppendLine();
        indent++;
        bool hasField = false;
        foreach (var record in records)
        {
            if (record.Data.Fields[constIndex] is not DString dString || string.IsNullOrWhiteSpace(dString.Value))
            {
                continue;
            }
            sb.Append('\t', indent).Append("public const ").Append(table.IndexField.CType.TypeName).Append(" ").Append(dString.Value).Append(" = ")
                .Append(record.Data.Fields[table.IndexFieldIdIndex]).Append(";").AppendLine();
            hasField = true;
        }

        indent--;
        sb.Append('\t', indent).Append("}").AppendLine();
        if (!string.IsNullOrWhiteSpace(table.NamespaceWithTopModule))
        {
            sb.Append("}").AppendLine();
        }

        if (!hasField)
        {
            return null;
        }

        return sb.ToString();
    }
}
