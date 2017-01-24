using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddCauseCommand : IRootCauseCommand
    {
        private Node _StartNode;
        private Node _NewNode;

        public Node NewNode { get { return _NewNode; } }

        public AddCauseCommand(Node startNode, string text) : this(startNode, text, false) { }

        public AddCauseCommand(Node startNode,string text,bool executeImmediately)
        {
            _StartNode = startNode;
            _NewNode = new Cause(text);
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            _StartNode.AddNode(_NewNode);
        }

        public void Undo()
        {
            _StartNode.RemoveNode(_NewNode);
        }
    }
}
