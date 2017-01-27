using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    abstract class Node
    {
        private long _NodeId;
        private string _Text;
        private SortedDictionary<long,Node> _Nodes;
        private SortedDictionary<long, Node> _ParentNodes;

        public long NodeId { get { return _NodeId; } }
        public string Text { get { return _Text; } }
        public IEnumerable<Node> Nodes { get { return _Nodes.Values; } }
        public IEnumerable<Node> ParentNodes { get { return _ParentNodes.Values; } }

        private Node() { }
        internal Node(string text) : this(text, SequentialId.NewId()) { }
        internal Node(string text,long nodeId)
        {
            _Text = text;
            _NodeId = nodeId;
            _Nodes = new SortedDictionary<long, Node>();
            _ParentNodes = new SortedDictionary<long, Node>();
        }

        public int CountNodes()
        {
            return _Nodes.Count;
        }

        public int CountParentNodes()
        {
            return _ParentNodes.Count;
        }

        internal protected void AddNode(Node node)
        {
            if (!_Nodes.ContainsKey(node.NodeId))
            {
                _Nodes.Add(node.NodeId, node);
            }
        }

        internal protected void AddParent(Node node)
        {
            if (!_ParentNodes.ContainsKey(node.NodeId))
            {
                _ParentNodes.Add(node.NodeId, node);
            }
        }

        internal protected void RemoveNode(Node node)
        {
            _Nodes.Remove(node.NodeId);
        }

        internal protected void RemoveParent(Node node)
        {
            _ParentNodes.Remove(node.NodeId);
        }

        internal protected void SetText(string newText)
        {
            _Text = newText;
        }
    }
}
