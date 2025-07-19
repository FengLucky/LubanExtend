## 构建方法

运行 `build.bat` 或 `build.sh` <br/>
Build/publish 目录下会生成一份 `dll` 文件

## 生成绑定索引的代码常量

在同一个表中通过额外配置一列或多列字段生成和索引字段绑定的常量代码

#### 命令行参数:

| 参数          | 必选 | 默认值      | 示例                                         |
|-------------|----|----------|--------------------------------------------|
| 使用扩展管线      | 是  |          | -p extend                                  |
| 目标代码使用的常量模板 | 是  |          | -x cs-dotnet-json.const=csharp             |
| 常量代码子目录     | 否  | Const    | -x cs-dotnet-json.constDirectory=Const     |
| 常量类格式化字符串   | 否  | {0}Const | -x cs-dotnet-json.constNameFormat={0}Const |

#### bean 定义

| 属性    | 必选 | 值             | 描述                 | 
|-------|----|---------------|--------------------|
| group | 是  | const         |                    |
| type  | 是  | string        |                    |
| tags  | 否  | comment={字段名} | 常量绑定的注释字段          |
| tags  | 否  | index={字段名}   | 常量绑定的索引字段,默认为第一个索引 |

#### 示例：

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
## 自动导入 table

| 参数                                 | 默认值      | 描述                                                      | 示例                                            |
|------------------------------------|----------|---------------------------------------------------------|-----------------------------------------------|
| tableImporter.name                 | extend   | 使用的导入器名称，该值必须配置且为 `extend`                              | -x tableImporter.name=extend                  |
| tableImporter.filePrefix           | #        | 自动导入文件前缀，支持配置多个前缀，但是文件名只识别第一个字符                         | -x tableImporter.filePrefix=#$_               |
| tableImporter.tableNamespaceFormat |          | 命名空间格式字符串，第一个参数为文件夹路径，第二个参数为文件名                         | -x tableImporter.tableNamespaceFormat={0}.{1} |
| tableImporter.tableNameFormat      | {0}Table | 表名格式字符串                                                 | -x tableImporter.tableNameFormat=Table{0}     |
| tableImporter.valueTypeNameFormat  | {0}Bean  | 值类型名格式字符串                                               | -x tableImporter.valueTypeNameFormat={0}VO    |
| tableImporter.tableMeta            |          | 表的额外信息配置文件，可配置文件名到 table 名的映射、mode、index、tags、group 等信息 | -x tableImporter.tableMeta=TableMeta.ini      |

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
# group 为分组

道具表@name = Item@mode=list@index=id@tags=index=multiple
```

## 代码模板
通过代码模板实现的扩展方法，仅支持 C#
- 一个值索引多条配置：字段 `tags` 添加 `index=multiple`，仅在表的 `mode` 为 `list` 时生效。可给多个字段配置`tags`
- C# json 代码支持运行时热重载,需自己实现文件变化监听方法，然后调用 Tables.IncrementalUpdate 方法进行增量更新

## 测试工程使用方法

- 自行构建鲁班可执行文件放到`Sample/Luban/Bin`目录下
- 双击运行 `打表-bin.bat`或`打表-json.bat`