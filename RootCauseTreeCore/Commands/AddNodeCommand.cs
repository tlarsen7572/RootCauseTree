using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddNodeCommand : IRootCauseCommand
    {
        private Node _StartNode;
        private Node _NewNode;
        private IRootCauseDb _Db;

        public Node NewNode { get { return _NewNode; } }

        public AddNodeCommand(IRootCauseDb db,Node startNode, string text) : this(db,startNode, text, false) { }

        public AddNodeCommand(IRootCauseDb db, Node startNode,string text,bool executeImmediately)
        {
            _StartNode = startNode;
            _NewNode = new Cause(text);
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (_Db.AddNode(_StartNode, _NewNode))
            {
                _StartNode.AddNode(_NewNode);
                _NewNode.AddParent(_StartNode);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if (_Db.RemoveNode(_NewNode))
            {
                _StartNode.RemoveNode(_NewNode);
                _NewNode.RemoveParent(_StartNode);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }
    }
}
