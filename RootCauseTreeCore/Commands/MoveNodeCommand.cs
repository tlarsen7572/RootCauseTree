using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class MoveNodeCommand : IRootCauseCommand
    {
        private Node _MovingNode;
        private Node _TargetNode;
        private HashSet<Node> _Parents;

        public MoveNodeCommand(Node movingNode, Node targetNode) : this(movingNode, targetNode, false) { }

        public MoveNodeCommand(Node movingNode, Node targetNode,bool executeImmediately)
        {
            _MovingNode = movingNode;
            _TargetNode = targetNode;
            _Parents = new HashSet<Node>();
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            foreach (var parent in _MovingNode.ParentNodes)
            {
                _Parents.Add(parent);
            }

            foreach(var parent in _Parents)
            {
                _MovingNode.RemoveParent(parent);
                parent.RemoveNode(_MovingNode);
            }
            _MovingNode.AddParent(_TargetNode);
            _TargetNode.AddNode(_MovingNode);
        }

        public void Undo()
        {
            _MovingNode.RemoveParent(_TargetNode);
            _TargetNode.RemoveNode(_MovingNode);
            foreach (var parent in _Parents)
            {
                parent.AddNode(_MovingNode);
                _MovingNode.AddParent(parent);
            }
            _Parents.Clear();
        }
    }
}
