# 更新日志 / ChangeLog

这里是 **Hamstory** 的更新日志！

## [0.0.1-beta1] - 2023-12-12
### 新增
* 故事脚本的解析器 `StoryParser` 以及相关的子解析器
* 故事脚本的语句 `Sentence`
* 故事脚本的解释执行器 `StoryExecutorBase`
* 与之配套的，用于控制具体表现形式的模块：
  * 故事展现模块 `VisualProvider`，以及其对于传统的“对话框展示在屏幕最底下或最顶上的”的实现子类 `TraditionalDialogProvider`
  * 数据解释模块 `DataProvider`，以及其使用了 `DevKit` 内的 `Archive` 存档工具进行数据IO的实现子类 `ArchiveDataProvider`
* 故事节点图 `StoryGraph` 以及相关的编辑器拓展支持
* 对故事节点图的解析数据结构 `StoryChain`
* 用于执行 `StoryChain` 的执行器实现子类 `StoryChainExecutor`

## [0.0.1-beta2] - 2023-12-15
### 新增
* 专用于编写剧情脚本的文件格式 `.hamstory` ，以及对应的 Unity 导入支持

### 修复
* 节点图内通过拖拽连线到空位置，以选择资源并创建节点时，节点的资源属性框没有正确赋值的问题

## [0.0.1-beta3] - 2023-12-15
### 新增
* `StoryGraphExecutor` 的“初始时执行”属性
* 【搜索并添加节点】窗口对 `.hamstory` 后缀的支持

### 修改
* 现在 `StoryGraph` 将统称为 "故事节点图"
* `StoryChainExecutor` 改名为 `StoryGraphExecutor`
* 现在 `DataProvider` 为 `StoryExecutorBase` 的非必须参数
* 创建 `.hamstory` 文件时，现在默认使用 `TextAsset` 的图标了