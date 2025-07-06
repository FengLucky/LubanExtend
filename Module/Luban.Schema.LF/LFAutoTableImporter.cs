using System.Text;
using Luban.Defs;
using Luban.RawDefs;
using Luban.Utils;
namespace Luban.Schema.Builtin;

[TableImporter("lf")]
public class LFAutoTableImporter : ITableImporter
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, (string name, string mode,string index, string tags)> _tableMetas = new();
    private readonly List<string> _validTableModes = ["map", "list", "one"];
    
    public LFAutoTableImporter()
    {
        if (EnvManager.Current.TryGetOption("tableImporter", "tableMeta", false, out var metaFilePath))
        {
            metaFilePath = Path.Combine(GenerationContext.GlobalConf.InputDataDir, metaFilePath);
            if (!File.Exists(metaFilePath))
            {
                throw new ArgumentException($"指定的 table meta 文件不存在:{metaFilePath}");
            }
            var allLines = File.ReadAllLines(metaFilePath);
            for(int i=0;i<allLines.Length;i++)
            {
                var line = allLines[i].Trim();
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                (string name,string mode,string index,string tags) meta = ("","map", "","");
                var lineConfigs = line.Split("@", 2);
                if (lineConfigs.Length != 2 || string.IsNullOrWhiteSpace(lineConfigs[0]) || string.IsNullOrWhiteSpace(lineConfigs[1]))
                {
                    throw new Exception($"Table Meta: 第 {i+1} 行错误的格式配置:{line} ,应为 {{文件名}}@{{配置项}}");
                }
                var fileName = lineConfigs[0].Trim();
                var parts = lineConfigs[1].Trim().Split('@');
                foreach (var part in parts)
                {
                    var block = part.Split('=',2);
                    if (block.Length != 2)
                    {
                        throw new Exception($"Table Meta: 第 {i+1} 行错误的格式配置:{part} ,应包含 '='");
                    }

                    if (string.IsNullOrWhiteSpace(block[1]))
                    {
                        throw new Exception($"Table Meta: 第 {i+1} 行错误的格式配置:{part} ,配置内容不能为空");
                    }
                    var key = block[0].Trim();
                    var value = block[1].Trim();

                    switch (key.ToLower())
                    {
                        case "name":
                            meta.name = value;
                            break;
                        case "mode":
                            if (!_validTableModes.Contains(value))
                            {
                                throw new Exception($"Table Meta: 第 {i+1} 不能识别的表格模式:{part} ,应为 'map'、'one'、'list' 中的一个");
                            }
                            meta.mode = value;
                            break;
                        case "index":
                            meta.index = value;
                            break;
                        case "tags":
                            meta.tags = value;
                            break;
                        default:
                            throw new Exception($"Table Meta: 第 {i+1} 行不能识别的配置项:{key}");
                    }
                }

                if (!_tableMetas.TryAdd(fileName, meta))
                {
                    throw new Exception($"Table Meta: 第 {i+1} 表格名重复配置");
                }
            }
        }
    }

    public List<RawTable> LoadImportTables()
    {
        string dataDir = GenerationContext.GlobalConf.InputDataDir;

        string fileNamePrefixStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "filePrefix", false, "#");
        string tableNamespaceFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNamespaceFormat", false, "");
        string tableNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNameFormat", false, "{0}Table");
        string valueTypeNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "valueTypeNameFormat", false, "{0}Bean");
        var excelExts = new HashSet<string> { "xlsx", "xls", "xlsm", "csv" };

        var tables = new List<RawTable>();
        var logStr = new StringBuilder();
        foreach (string file in Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(file);
            string ext = Path.GetExtension(fileName).TrimStart('.');
            if (!excelExts.Contains(ext))
            {
                continue;
            }

            if (!fileNamePrefixStr.Contains(fileName[0]))
            {
                continue;
            }

            string relativePath = file.Substring(dataDir.Length + 1).TrimStart('\\').TrimStart('/');
            string namespaceFromRelativePath = Path.GetDirectoryName(relativePath).Replace('/', '.').Replace('\\', '.');
            
            var fileWithoutPrefixExt = Path.GetFileNameWithoutExtension(fileName.Substring(1));
            var mode = TableMode.MAP;
            var tags = new Dictionary<string, string>();
            var index = "";
            var originName = fileWithoutPrefixExt;
            var tableName = string.Format(tableNameFormatStr, originName);
            if (_tableMetas.TryGetValue(fileWithoutPrefixExt, out var meta))
            {
                originName = !string.IsNullOrWhiteSpace(meta.name) ? meta.name :originName;
                tableName = string.Format(tableNameFormatStr, originName);
                index = meta.index;
                mode = SchemaLoaderUtil.ConvertMode(file,tableName,meta.mode,meta.index);
                tags = DefUtil.ParseAttrs(meta.tags);
            }
            
            string tableNamespace = string.Format(tableNamespaceFormatStr, namespaceFromRelativePath,tableName);
            string valueTypeFullName = TypeUtil.MakeFullName(tableNamespace, string.Format(valueTypeNameFormatStr, originName));
            
            var table = new RawTable
            {
                Namespace = tableNamespace,
                Name = tableName,
                Index = index,
                ValueType = valueTypeFullName,
                ReadSchemaFromFile = true,
                Mode = mode,
                Comment = "",
                Groups = new List<string> { },
                InputFiles = new List<string> { relativePath },
                OutputFile = "",
                Tags = tags
            };
            logStr.Clear();
            logStr.Append($"import table file [{{table}}]:name={tableName};mode={mode};");
            if (!string.IsNullOrWhiteSpace(meta.index))
            {
                logStr.Append($"index={meta.index};");
            }
            
            if (!string.IsNullOrWhiteSpace(meta.tags))
            {
                logStr.Append($"tags={meta.tags};");
            }
            s_logger.Info(logStr);
            tables.Add(table);
        }
        return tables;
    }
}
