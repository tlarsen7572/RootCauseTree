using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeChainCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Parents;
        private IRootCauseDb _Db;

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
            if (_Db.RemoveNodeChain(_RemoveNode))
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
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if(_Db.UndoRemoveNodeChain(_RemoveNode, _Parents))
            {
                foreach (var parent in _Parents)
                {
                    _RemoveNode.AddParent(parent);
                    parent.AddNode(_RemoveNode);
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
