## 构建方法
- 将`Module`目录下下文件复制到 `Luban/src/` 目录下
- `Luban` 项目添加`Moduel`目录下各项目的引用
- `Module`目录下各项目添加对 `Luban.Core` 项目的引用
- 修改鲁班 `OutputFileManifest.cs` 文件 `public void AddFile(OutputFile file)` 方法如下
```c#
public void AddFile(OutputFile file)
{
    if (file == null)
    {
        return;
    }
    lock (this)
    {
        _dataFiles.Add(file);
    }
}
```
### `Luban.Data.Target.Const` 构建数据同时构建和索引绑定的常量代码
仅支持单索引，且索引类型为`short` `int` `long` `string`
#### 使用方法
- 添加字段(不导出，如`_const`)，字段类型为 `string`
- 必选：字段 `group` 分组为 `const`，自动跳过内容为空的记录，同一个配置类型中只能有一个分组为 `const` 的字段
- 可选：字段 `tags` 添加 `comment={字段名}` 生成常量时会自动将指定字段数据作为注释生成
- 命令行添加参数 `-d const-cs` `-x const-cs.outputDataDir={常量代码导出路径}`
- 常量代码导出路径需要单独一个文件夹，不能和代码导出路径或者数据导出路径在同一个目录，否则会被鲁班自动删除
## 代码模板
- 一个索引值索引多条配置：字段 `tags` 添加 `index=multiple`，仅在表的 `mode` 为 `list` 时生效。可给多个字段配置`tags`
## 测试工程使用方法
- 自行构建鲁班可执行文件放到`Sample/Luban/Bin`目录下
- 双击运行 `打表-bin.bat`或`打表-json.bat`