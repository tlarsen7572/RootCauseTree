using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeChainCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Parents;

        public RemoveNodeChainCommand(Node removeNode) : this(removeNode, false) { }

        public RemoveNodeChainCommand(Node removeNode, bool executeImmediately)
        {
            _RemoveNode = removeNode;
            _Parents = new HashSet<Node>();
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            foreach (var parent in _RemoveNode.ParentNodes)
            {
                _Parents.Add(parent);
                parent.RemoveNode(_RemoveNode);
            }
            foreach (var parent in _Parents)
            {
                _RemoveNode.RemoveParent(parent);
            }
        }

        public void Undo()
        {
            foreach (var parent in _Parents)
            {
                _RemoveNode.AddParent(parent);
                parent.AddNode(_RemoveNode);
            }
            _Parents.Clear();
        }
    }
}
