using System;

namespace Hamstory
{
    /// <summary>
    /// 一个用于实现选项按钮的接口。<br/>
    /// 实现这个接口需要让该物体可以被点击，且在点击后执行在 SetContent 中传入的 callback 方法
    /// </summary>
    public interface IMenuButton
    {
        /// <summary>
        /// 设置按钮的内容。此处应当保存 callback 引用，并在按钮按下时调用它。
        /// </summary>
        /// <param name="content">按钮的文本内容</param>
        /// <param name="callback">按钮按下时需要调用的回调</param>
        void SetContent(string content, Action callback);
    }
}