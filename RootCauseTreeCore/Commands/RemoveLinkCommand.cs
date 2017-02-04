using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveLinkCommand : IRootCauseCommand
    {
        private Node _StartNode;
        private Node _EndNode;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public RemoveLinkCommand(IRootCauseDb db,Node startNode, Node endNode) : this(db,startNode, endNode, false) { }

        public RemoveLinkCommand(IRootCauseDb db, Node startNode, Node endNode,bool executeImmediately)
        {
            _StartNode = startNode;
            _EndNode = endNode;
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if(!_Db.RemoveLink(_StartNode, _EndNode)) { throw new CommandFailedDbWriteException(); }
            _StartNode.RemoveNode(_EndNode);
            _EndNode.RemoveParent(_StartNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.UndoRemoveLink(_StartNode, _EndNode)) { throw new CommandFailedDbWriteException(); }
            _StartNode.AddNode(_EndNode);
            _EndNode.AddParent(_StartNode);
            Executed = false;
        }
    }
}
