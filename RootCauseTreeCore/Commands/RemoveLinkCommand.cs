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
            if(_Db.RemoveLink(_StartNode, _EndNode))
            {
                _StartNode.RemoveNode(_EndNode);
                _EndNode.RemoveParent(_StartNode);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if (_Db.AddLink(_StartNode, _EndNode))
            {
                _StartNode.AddNode(_EndNode);
                _EndNode.AddParent(_StartNode);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }
    }
}
