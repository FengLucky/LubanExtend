using Luban.Datas;
using Luban.DataVisitors;

namespace Luban.Extend;

public class ConstValueVisitor: IDataFuncVisitor<string>
{
    public static ConstValueVisitor Ins = new();
    public string Accept(DBool type)
    {
        return type.Value.ToString().ToLower();
    }

    public string Accept(DByte type)
    {
        return type.Value.ToString();
    }

    public string Accept(DShort type)
    {
        return type.Value.ToString();
    }

    public string Accept(DInt type)
    {
        return type.Value.ToString();
    }

    public string Accept(DLong type)
    {
        return type.Value + "L";
    }

    public string Accept(DFloat type)
    {
        return type.Value + "f";
    }

    public string Accept(DDouble type)
    {
        return type.Value.ToString();
    }

    public string Accept(DEnum type)
    {
        return type.Type.DefEnum.Name + "." + type.Type.DefEnum.Items[type.Value].Name;
    }

    public string Accept(DString type)
    {
        return "\"" + type.Value + "\"";
    }

    public string Accept(DDateTime type)
    {
        throw new NotImplementedException();
    }

    public string Accept(DBean type)
    {
        throw new NotImplementedException();
    }

    public string Accept(DArray type)
    {
        throw new NotImplementedException();
    }

    public string Accept(DList type)
    {
        throw new NotImplementedException();
    }

    public string Accept(DSet type)
    {
        throw new NotImplementedException();
    }

    public string Accept(DMap type)
    {
        throw new NotImplementedException();
    }
}
