using System.Collections;
using System.Collections.Generic;
using Bingyan;
using UnityEngine;
using UnityEngine.UI;

namespace Hamstory
{
    /// <summary>
    /// 传统的方框型对话框，没有提供“头像”。<br/>
    /// 如果需要头像（或更高级的功能，如头像的变化），可以继承这个类型并重写 SetCharacter 方法。
    /// </summary>
    public class TraditionalDialogProvider : VisualProvider
    {
        [SerializeField, Title("界面父物体")] private GameObject dialogPanel;
        [SerializeField, Title("菜单按钮容器")] private Transform menuParent;
        [SerializeField, Title("菜单按钮")] private GameObject menuBtn;
        [SerializeField, Title("角色名称文字")] private Text charNameText;
        [SerializeField, Title("内容文字")] private Text contentText;

        public override GameObject GetDialogPanel() => dialogPanel;

        public override void SetText(StoryExecutorBase executor, string content)
        {
            StartCoroutine(_SetText(executor, content));
        }

        private IEnumerator _SetText(StoryExecutorBase executor, string content)
        {
            var text = contentText;

            int charIdx = 0;
            float delta = 0.1f, lastTime = Time.timeSinceLevelLoad;

            while (charIdx < content.Length)
            {
                if (Time.timeSinceLevelLoad - lastTime <= delta)
                {
                    lastTime += delta;
                    charIdx++;
                }

                text.text = content.Substring(0, charIdx);
                yield return 0;
            }

            yield return new WaitUntil(() => Input.anyKeyDown);
            executor.Continue();
        }

        public override void SetCharacter(CharacterConfig config, string extraArgs)
        {
            charNameText.text = config.CharName;
        }

        public override void CreateMenu(StoryExecutorBase executor, List<MenuOption> options)
        {
            foreach (var item in options)
            {
                if (Instantiate(menuBtn, Vector3.zero, Quaternion.identity, menuParent)
                    .TryGetComponent<IMenuButton>(out var btn))
                    btn.SetContent(item.Content, () => { executor.Goto(item.SentenceIndex); ClearMenu(); });
                else Debug.LogError("TraditionalDialogProvider 需要一个具有【实现了 IMenuButton 接口的组件】的按钮！");
            }
        }

        public override void ClearMenu()
        {
            for (int i = 0; i < menuParent.childCount; i++)
                Destroy(menuParent.GetChild(i).gameObject);
        }

        public override void ClearCharacter()
        {
            charNameText.text = "";
        }
    }
}