## 构建方法
运行 `build.bat` 或 `build.sh` <br/>
Build/publish 目录下会生成一份 `dll` 文件
## `Luban.Data.Target.Const` 构建数据同时构建和索引绑定的常量代码
仅支持单索引，且索引类型为`short` `int` `long` `string`
#### 使用方法
- 添加字段(不导出，如`_const`)，字段类型为 `string`
- 必选：字段 `group` 分组为 `const`，自动跳过内容为空的记录，同一个配置类型中只能有一个分组为 `const` 的字段
- 可选：字段 `tags` 添加 `comment={字段名}` 生成常量时会自动将指定字段数据作为注释生成
- 命令行添加参数 `-d const-cs` `-x const-cs.outputDataDir={常量代码导出路径}`
- 常量代码导出路径需要单独一个文件夹，不能和代码导出路径或者数据导出路径在同一个目录，否则会被鲁班自动删除

`xml` 定义如下：
```
<bean name="ClassBean" comment="class 类型">
    <var name="id" comment="id" type="int"/>
    <var name="_const" comment="常量字段" type="string" group="const" tags="comment=commentField"/>
    <var name="commentField" comment="常量注释字段" type="string" group="const"/>
</bean>
```
`luban.conf` 分组定义 `const` :
```
"groups":
[
    {"names":["c"], "default":true},
    {"names":["s"], "default":true},
    {"names":["const"],"default":true}
]
```
## `Luban.Schema.LF`
#### 自动导入 table

| 参数                 | 默认值 | 描述                              | 示例                                              |
|--------------------|-----|---------------------------------|-------------------------------------------------|
| tableImporter.name | lf  | 使用的导入器名称，该值必须配置且为 `lf`          | -x tableImporter.name=lf                        |
| tableImporter.filePrefix | #   | 自动导入文件前缀，支持配置多个前缀，但是文件名只识别第一个字符 | -x tableImporter.filePrefix=#$_                 |
| tableImporter.tableNamespaceFormat | | 命名空间格式字符串，第一个参数为文件路径，第二个参数为文件名 | -x tableImporter.tableNamespaceFormat={0}.{1}   |
| tableImporter.tableNameFormat | {0}Table | 表名格式字符串 | -x tableImporter.tableNameFormat=Table{0}       |
| tableImporter.valueTypeNameFormat | {0}Bean | 值类型名格式字符串 | -x tableImporter.valueTypeNameFormat={0}VO      |
| tableImporter.tableMeta | | 表的额外信息配置文件，可配置文件名到 table 名的映射、mode、index、tags 等信息 | -x tableImporter.tableMeta=TableMeta.ini        |

`table meta` 配置格式如下:
```ini
# 注释行，仅支持 # 开头的行作为注释
# 空行自动跳过
# 使用 @ 作为配置项的分隔符
# 第一项为原始表名，不需要路径、前缀和扩展名
# name 为生成的 Table 名
# mode 为 表模式,可选项 map、list、one
# index 为索引字段
# tags 为自定义标签

道具表=name=Item@mode=list@index=id@tags=index=multiple
```
## 代码模板
- 一个索引值索引多条配置：字段 `tags` 添加 `index=multiple`，仅在表的 `mode` 为 `list` 时生效。可给多个字段配置`tags`
## 测试工程使用方法
- 自行构建鲁班可执行文件放到`Sample/Luban/Bin`目录下
- 双击运行 `打表-bin.bat`或`打表-json.bat`