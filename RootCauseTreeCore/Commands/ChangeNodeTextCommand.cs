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

        public bool Executed { get; private set; }

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
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if(!_Db.ChangeNodeText(_Node, _NewText)) { throw new CommandFailedDbWriteException(); }
            _OldText = _Node.Text;
            _Node.SetText(_NewText);
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.ChangeNodeText(_Node, _OldText)) { throw new CommandFailedDbWriteException(); }
            _Node.SetText(_OldText);
            Executed = false;
        }
    }
}
