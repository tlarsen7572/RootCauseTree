using System.Collections.Generic;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class MoveNodeCommand : IRootCauseCommand
    {
        private Node _MovingNode;
        private Node _TargetNode;
        private HashSet<Node> _Parents;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

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
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.MoveNode(_MovingNode, _TargetNode)) { throw new CommandFailedDbWriteException(); }
            foreach (var parent in _MovingNode.ParentNodes)
            {
                _Parents.Add(parent);
            }

            foreach (var parent in _Parents)
            {
                _MovingNode.RemoveParent(parent);
                parent.RemoveChild(_MovingNode);
            }
            _MovingNode.AddParent(_TargetNode);
            _TargetNode.AddChild(_MovingNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.UndoMoveNode(_MovingNode, _Parents)) { throw new CommandFailedDbWriteException(); }
            _MovingNode.RemoveParent(_TargetNode);
            _TargetNode.RemoveChild(_MovingNode);
            foreach (var parent in _Parents)
            {
                parent.AddChild(_MovingNode);
                _MovingNode.AddParent(parent);
            }
            _Parents.Clear();
            Executed = false;
        }
    }
}
