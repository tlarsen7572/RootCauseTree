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

        public ChangeNodeTextCommand(Node node, string newText) : this(node, newText, false) { }

        public ChangeNodeTextCommand(Node node,string newText,bool executeImmediately)
        {
            _Node = node;
            _NewText = newText;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            _OldText = _Node.Text;
            _Node.SetText(_NewText);
        }

        public void Undo()
        {
            _Node.SetText(_OldText);
        }
    }
}
