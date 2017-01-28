using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddLinkCommand : IRootCauseCommand
    {
        private Node _StartNode;
        private Node _EndNode;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public AddLinkCommand(IRootCauseDb db, Node startNode, Node endNode) : this(db,startNode, endNode, false) { }

        public AddLinkCommand(IRootCauseDb db, Node startNode,Node endNode,bool executeImmediately)
        {
            _Db = db;
            _StartNode = startNode;
            _EndNode = endNode;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.AddLink(_StartNode, _EndNode)) { throw new CommandFailedDbWriteException(); }
            _StartNode.AddNode(_EndNode);
            _EndNode.AddParent(_StartNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.RemoveLink(_StartNode, _EndNode)) { throw new CommandFailedDbWriteException(); }
            _StartNode.RemoveNode(_EndNode);
            _EndNode.RemoveParent(_StartNode);
            Executed = false;
        }
    }
}
