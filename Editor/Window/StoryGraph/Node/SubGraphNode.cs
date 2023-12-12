using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    public class SubGraphNode : GraphNode<SubGraphNodeData>
    {
        private ObjectField graphField;

        private Port flowIn, flowOut;
        private Button btnEdit;

        public override Port FlowIn => flowIn;
        public override Port FlowOut => flowOut;


        public SubGraphNode(StoryGraphView view, SubGraphNodeData data) : base(view, data)
        {
            title = "故事链";
            BuildLayout();
        }

        private void BuildLayout()
        {
            graphField = new("配置");
            graphField.objectType = typeof(StoryGraph);
            graphField.RegisterValueChangedCallback(i => UpdateGraph(i.previousValue as StoryGraph, i.newValue as StoryGraph));
            graphField.value = Data.Subgraph;
            inputContainer.Add(graphField);

            btnEdit = new Button(() => StoryGraphWindow.ShowWindow(graphField.value as StoryGraph));
            btnEdit.text = "编辑";

            UpdateGraph(null, Data.Subgraph);
        }

        private void UpdateGraph(StoryGraph oldVal, StoryGraph newVal)
        {
            Data.Subgraph = newVal;
            if (oldVal ^ newVal)
            {
                if (newVal)
                {
                    flowIn = CreateFlowInPort(0);
                    flowOut = CreateFlowOutPort(0);

                    if (!inputContainer.Contains(btnEdit)) inputContainer.Add(btnEdit);
                }
                else
                {
                    var removeList = new List<GraphElement>();

                    removeList.AddRange(flowIn.connections);
                    removeList.Add(flowIn);
                    flowIn = null;
                    removeList.AddRange(flowOut.connections);
                    removeList.Add(flowOut);
                    flowOut = null;

                    view.DeleteElements(removeList);

                    if (inputContainer.Contains(btnEdit)) inputContainer.Remove(btnEdit);
                }
            }
        }

        public void SetSubgraph(StoryGraph subgraph)
        {
            UpdateGraph(Data.Subgraph, subgraph);
        }
    }
}