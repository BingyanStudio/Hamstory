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
  

## [0.0.1-beta4] - 2023-12-17
### 新增
* 现在可以直接从文件中拖拽故事脚本或节点图进入节点图编辑器了
  

### 修改
* 使用 MVVC 架构重构了大部分节点图编辑器
* 修改了部分不必要成员的可见性为 `internal`
  

### 修复
* 修复了部分操作的撤销不正确的问题
  

## [0.0.1-beta5] - 2023-12-30
### 新增
* 增加了针对单个故事脚本的执行器 `SingleStoryExecutor`
* 增加了自定义脚本解析器的添加接口
  
### 修改
* 为 `TraditionalDialogProvider` 暴露更多可调节参数
* 重构了解析器类的继承树
* 现在节点图内的脚本在修改后会自动重新解析，无需手动更新了
* 如果解析时有多个解析器请求解析同一语句，则会发出警告
* 现在菜单选项内的变量也会被解析为动态值了
  
  
### 修复
* 修复了指令解析器不区分大小写的bug

  
## [0.0.1-beta6] - 2024-2-21
### 新增
* 增加了 `StoryExecutorBase.Finished` 事件，用于在故事执行完毕后执行回调
* 为 `StoryExecutorBase.Execute` 及其子类的重载增加了参数 callback，用于在故事执行完毕后执行回调
* 增加了 `SingleStoryExecutor.SetVisual(VisualProvider)` 用于动态设置 `VisualProvider`
* 增加了 `SingleStoryExecutor.SetData(DataProvider)` 用于动态设置 `DataProvider`

  
## [0.0.1-beta7] - 2024-2-22
### 新增
* 增加了 `StoryExecutorBase.CharacterChanged` 事件，用于提示角色的切换，从而实现镜头切换等效果
* 增加了 `StoryExecutorBase.GetCharacter(string)` 用于统一获取角色配置  

  
## [0.0.1-beta8] - 2024-2-23
### 新增
* [End] 语句块，用于在中途结束一段对话
* [Return] 语句块，用于结束一段对话的同时，返回一些信息，以供代码逻辑实现不同选项的实际效果
* 带有返回值的对话结束回调


### 修复
* 修复了 `StoryExecutorBase.End()` 与 `StoryExecutorBase.End()` 主次关系颠倒的问题  

  
## [0.0.1-beta9] - 2024-2-24
### 新增
* 为各个组件添加了组件菜单  

  
## [0.0.1-beta10] - 2024-3-1
### 新增
* `BasicDataProvider`: 提供了基础的比较与变量解析接口的 `DataProvider`
* 添加对布尔值的支持