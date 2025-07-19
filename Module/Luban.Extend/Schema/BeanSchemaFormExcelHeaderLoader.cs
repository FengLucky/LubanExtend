using Luban.RawDefs;
using Luban.Schema;
using Luban.Schema.Builtin;

namespace Luban.Extend;

[BeanSchemaLoader("extend")]
public class BeanSchemaFormExcelHeaderLoader:IBeanSchemaLoader
{
    public RawBean Load(string fileName, string beanFullName, RawTable table)
    {
        var bean = BeanSchemaFromExcelHeaderLoader.LoadTableValueTypeDefineFromFile(fileName, beanFullName, table);
        bean.Groups = table.Groups;
        return bean;
    }
}
