using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Hamstory.Editor
{
    public class StoryNode : GraphNode<StoryNodeData>
    {
        private ObjectField storyField;
        private List<VisualElement> mutableEls = new();
        private List<Port> mutablePorts = new();
        private VisualElement mutableContainer = new();

        private Port flowIn, flowOut;

        private Button btnReparse;

        public override Port FlowIn => flowIn;

        public override Port FlowOut => flowOut;

        public StoryNode(StoryGraphView view, StoryNodeData data) : base(view, data)
        {
            title = "故事";
            BuildLayout();
        }

        protected virtual void BuildLayout()
        {
            btnReparse = new Button(() => ParseStory(storyField.value));
            btnReparse.text = "重新解析";

            storyField = new ObjectField("脚本");
            storyField.objectType = typeof(TextAsset);
            storyField.RegisterValueChangedCallback(i => ParseStory(i.newValue));
            inputContainer.Add(storyField);

            storyField.value = Data.StoryText;
            ParseStory(storyField.value);

            mutableContainer.style.paddingLeft = new StyleLength(4);
            mutableContainer.style.marginTop = new StyleLength(8);
            inputContainer.Add(mutableContainer);

            Refresh();
        }

        protected virtual void ParseStory(UnityEngine.Object newFile)
        {
            RemoveMutableElements();
            if (newFile == null)
            {
                RemoveIOPorts();
                RemoveExtraOutPorts();
                if (inputContainer.Contains(btnReparse)) inputContainer.Remove(btnReparse);

                Data.StoryText = null;
                Data.Characters.Clear();

                Refresh();
                return;
            }
            else if (!inputContainer.Contains(btnReparse))
                inputContainer.Insert(1, btnReparse);

            var storyFile = newFile as TextAsset;
            Data.StoryText = storyFile;
            if (!StoryParser.Parse(storyFile.name, storyFile.text, out var story))
            {
                RemoveIOPorts();
                RemoveExtraOutPorts();
                AddMutableElement(new Label("解析出错，请查看控制台！"));

                Data.Characters.Clear();
                view.Save();
            }
            else
            {
                CreateIOPorts();
                AddMutableElement(new Label("角色配置"));
                for (int i = 0; i < story.Characters.Count; i++)
                {
                    int idx = i;
                    var field = new ObjectField(story.Characters[idx]);
                    field.objectType = typeof(CharacterConfig);
                    field.value = Data.Characters.Count > i ? Data.Characters[i] : null;
                    field.RegisterValueChangedCallback(j =>
                    {
                        if (Data.Characters.Count <= idx)
                        {
                            var cnt = idx - Data.Characters.Count + 1;
                            for (int k = 0; k < cnt; k++) Data.Characters.Add(null);
                        }
                        Data.Characters[idx] = j.newValue as CharacterConfig;
                        view.Save();
                    });
                    AddMutableElement(field);
                }
                var saved = RemoveExtraOutPorts(story.Jumps.ToArray());
                story.Jumps.ForEach(i =>
                {
                    if (saved.Contains(i)) return;
                    CreateExtraOutPort(i);
                });
            }
            Refresh();
            view.Save();
        }

        protected void AddMutableElement(VisualElement element)
        {
            mutableContainer.Add(element);
            mutableEls.Add(element);
        }

        protected void RemoveMutableElements()
        {
            mutableEls.ForEach(i => mutableContainer.Remove(i));
            mutableEls.Clear();
        }

        protected void CreateIOPorts()
        {
            if (flowIn == null) flowIn = CreateFlowInPort(0);
            if (flowOut == null) flowOut = CreateFlowOutPort(0);
        }

        protected void RemoveIOPorts()
        {
            var removeList = new List<GraphElement>();
            if (flowIn != null)
            {
                removeList.AddRange(flowIn.connections);
                removeList.Add(flowIn);
                flowIn = null;
            }
            if (flowOut != null)
            {
                removeList.AddRange(flowOut.connections);
                removeList.Add(flowOut);
                flowOut = null;
            }
            view.DeleteElements(removeList);
        }

        protected void CreateExtraOutPort(string label)
        {
            var port = CreateOutPort(label, Port.Capacity.Single, typeof(Flow));
            port.portColor = Flow.Color;
            outputContainer.Add(port);
            mutablePorts.Add(port);

            Refresh();
        }

        protected List<string> RemoveExtraOutPorts(params string[] remains)
        {
            var removeList = new List<GraphElement>();
            var savedList = new List<string>();
            mutablePorts.ForEach(i =>
            {
                if (remains.Contains(i.portName))
                {
                    savedList.Add(i.portName);
                    return;
                }
                removeList.Add(i);
                removeList.AddRange(i.connections);
            });
            view.DeleteElements(removeList);
            return savedList;
        }

        public void SetStory(TextAsset story)
        {
            storyField.value = story;
            Data.StoryText = story;
            ParseStory(story);
        }
    }
}