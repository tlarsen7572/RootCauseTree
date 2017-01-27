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
        private IRootCauseDb _Db;

        public MoveNodeCommand(IRootCauseDb db,Node movingNode, Node targetNode) : this(db,movingNode, targetNode, false) { }

        public MoveNodeCommand(IRootCauseDb db, Node movingNode, Node targetNode,bool executeImmediately)
        {
            _MovingNode = movingNode;
            _TargetNode = targetNode;
            _Parents = new HashSet<Node>();
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (_Db.MoveNode(_MovingNode, _TargetNode))
            {
                foreach (var parent in _MovingNode.ParentNodes)
                {
                    _Parents.Add(parent);
                }

                foreach (var parent in _Parents)
                {
                    _MovingNode.RemoveParent(parent);
                    parent.RemoveNode(_MovingNode);
                }
                _MovingNode.AddParent(_TargetNode);
                _TargetNode.AddNode(_MovingNode);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if (_Db.UndoMoveNode(_MovingNode, _TargetNode, _Parents))
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
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }
    }
}
