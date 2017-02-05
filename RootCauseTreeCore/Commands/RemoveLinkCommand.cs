using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveLinkCommand : IRootCauseCommand
    {
        private Node _ParentNode;
        private Node _ChildNode;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public RemoveLinkCommand(IRootCauseDb db,Node parentNode, Node childNode) : this(db,parentNode, childNode, false) { }

        public RemoveLinkCommand(IRootCauseDb db, Node parentNode, Node childNode,bool executeImmediately)
        {
            _ParentNode = parentNode;
            _ChildNode = childNode;
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if(!_Db.RemoveLink(_ParentNode, _ChildNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.RemoveChild(_ChildNode);
            _ChildNode.RemoveParent(_ParentNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.UndoRemoveLink(_ParentNode, _ChildNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.AddChild(_ChildNode);
            _ChildNode.AddParent(_ParentNode);
            Executed = false;
        }
    }
}
