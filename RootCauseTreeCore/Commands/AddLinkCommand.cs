using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddLinkCommand : IRootCauseCommand
    {
        private Node _ParentNode;
        private Node _ChildNode;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public AddLinkCommand(IRootCauseDb db, Node parentNode, Node childNode) : this(db,parentNode, childNode, false) { }

        public AddLinkCommand(IRootCauseDb db, Node parentNode,Node childNode,bool executeImmediately)
        {
            _Db = db;
            _ParentNode = parentNode;
            _ChildNode = childNode;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.AddLink(_ParentNode, _ChildNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.AddChild(_ChildNode);
            _ChildNode.AddParent(_ParentNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.RemoveLink(_ParentNode, _ChildNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.RemoveChild(_ChildNode);
            _ChildNode.RemoveParent(_ParentNode);
            Executed = false;
        }
    }
}
