using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class ChangeNodeTextCommand : IRootCauseCommand
    {
        private Node _Node;
        private string _NewText;
        private string _OldText;
        private IRootCauseDb _Db;

        public ChangeNodeTextCommand(IRootCauseDb db, Node node, string newText) : this(db,node, newText, false) { }

        public ChangeNodeTextCommand(IRootCauseDb db, Node node,string newText,bool executeImmediately)
        {
            _Node = node;
            _NewText = newText;
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if(_Db.ChangeNodeText(_Node, _NewText))
            {
                _OldText = _Node.Text;
                _Node.SetText(_NewText);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if(_Db.ChangeNodeText(_Node, _OldText))
            {
                _Node.SetText(_OldText);
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }
    }
}
