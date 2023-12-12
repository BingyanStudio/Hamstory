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