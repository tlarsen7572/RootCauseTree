using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Nodes;
        private HashSet<Node> _Parents;

        public RemoveNodeCommand(Node removeNode)
        {
            _Nodes = new HashSet<Node>();
            _Parents = new HashSet<Node>();
            _RemoveNode = removeNode;
        }

        public void Execute()
        {
            foreach (var node in _RemoveNode.Nodes)
            {
                _Nodes.Add(node);
                foreach (var parent in _RemoveNode.ParentNodes)
                {
                    node.AddParent(parent);
                }
                node.RemoveParent(_RemoveNode);
            }

            foreach (var parent in _RemoveNode.ParentNodes)
            {
                _Parents.Add(parent);
                foreach (var node in _RemoveNode.Nodes)
                {
                    parent.AddNode(node);
                }
                parent.RemoveNode(_RemoveNode);
            }

            foreach (var node in _Nodes)
            {
                _RemoveNode.RemoveNode(node);
            }

            foreach (var parent in _Parents)
            {
                _RemoveNode.RemoveParent(parent);
            }
        }

        public void Undo()
        {

        }
    }
}
