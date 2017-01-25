using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class AddNodeCommand : IRootCauseCommand
    {
        private Node _StartNode;
        private Node _NewNode;

        public Node NewNode { get { return _NewNode; } }

        public AddNodeCommand(Node startNode, string text) : this(startNode, text, false) { }

        public AddNodeCommand(Node startNode,string text,bool executeImmediately)
        {
            _StartNode = startNode;
            _NewNode = new Cause(text);
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            _StartNode.AddNode(_NewNode);
            _NewNode.AddParent(_StartNode);
        }

        public void Undo()
        {
            _StartNode.RemoveNode(_NewNode);
            _NewNode.RemoveParent(_StartNode);
        }
    }
}
