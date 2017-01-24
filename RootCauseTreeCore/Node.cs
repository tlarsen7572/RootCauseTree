using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    abstract class Node
    {
        private Guid _NodeId;
        private string _Text;
        private Dictionary<string,Node> _Nodes;

        public Guid NodeId { get { return _NodeId; } }
        public string Text { get { return _Text; } }
        public IEnumerable<Node> Nodes { get { return _Nodes.Values; } }

        private Node() { }
        internal Node(string text)
        {
            _NodeId = SequentialGuid.NewGuid();
            _Text = text;
            _Nodes = new Dictionary<string,Node>();
        }

        public int CountNodes()
        {
            return _Nodes.Count;
        }

        internal protected void AddNode(Node node)
        {
            _Nodes.Add(node.Text,node);
        }

        internal protected void RemoveNode(Node node)
        {
            _Nodes.Remove(node.Text);
        }

        internal protected void SetText(string newText)
        {
            _Text = newText;
        }
    }
}
