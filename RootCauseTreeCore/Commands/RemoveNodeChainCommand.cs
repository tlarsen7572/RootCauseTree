using System.Collections.Generic;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeChainCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Parents;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public RemoveNodeChainCommand(IRootCauseDb db,Node removeNode) : this(db,removeNode, false) { }

        public RemoveNodeChainCommand(IRootCauseDb db, Node removeNode, bool executeImmediately)
        {
            _RemoveNode = removeNode;
            _Parents = new HashSet<Node>();
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.RemoveNodeChain(_RemoveNode)) { throw new CommandFailedDbWriteException(); }
            foreach (var parent in _RemoveNode.ParentNodes)
            {
                _Parents.Add(parent);
                parent.RemoveChild(_RemoveNode);
            }
            foreach (var parent in _Parents)
            {
                _RemoveNode.RemoveParent(parent);
            }
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.UndoRemoveNodeChain(_RemoveNode, _Parents)) { throw new CommandNotExecutedException(); }
            foreach (var parent in _Parents)
            {
                _RemoveNode.AddParent(parent);
                parent.AddChild(_RemoveNode);
            }
            _Parents.Clear();
            Executed = false;
        }
    }
}
