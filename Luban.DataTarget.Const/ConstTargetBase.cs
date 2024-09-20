using Luban.Defs;

namespace Luban.DataTarget.Const;

public abstract class ConstTargetBase : DataTargetBase
{
    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        //  var constIndex = table.fi.IndexOf(record=> record)
        var constIndex = table.ValueTType.DefBean.Fields.FindIndex(field => field.Groups.FindIndex(group => group == "const") > -1 && field.CType.TypeName == "string");
        if (constIndex == -1)
        {
            
            return null;
        }

        var content = GenCode(table, records, constIndex);
        if (content == null)
        {
            return null;
        }
        return new OutputFile { File = $"{table.Name}Const.{OutputFileExt}", Content = content };
    }

    protected abstract string? GenCode(DefTable table, List<Record> records, int constIndex);
}
