using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddNodeCommand : IRootCauseCommand
    {
        private Node _ParentNode;
        private Node _NewNode;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public Node NewNode { get { return _NewNode; } }

        public AddNodeCommand(IRootCauseDb db,Node parentNode, string text) : this(db,parentNode, text, false) { }

        public AddNodeCommand(IRootCauseDb db, Node parentNode,string text,bool executeImmediately)
        {
            _ParentNode = parentNode;
            _NewNode = new Cause(text);
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.AddNode(_ParentNode, _NewNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.AddChild(_NewNode);
            _NewNode.AddParent(_ParentNode);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.RemoveNode(_NewNode)) { throw new CommandFailedDbWriteException(); }
            _ParentNode.RemoveChild(_NewNode);
            _NewNode.RemoveParent(_ParentNode);
            Executed = false;
        }
    }
}
