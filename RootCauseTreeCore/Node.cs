using System.Collections.Generic;
using System.ComponentModel;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    public abstract class Node : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private long _NodeId;
        private string _Text;
        private SortedDictionary<long,Node> _ChildNodes;
        private SortedDictionary<long, Node> _ParentNodes;

        public long NodeId { get { return _NodeId; } }
        public string Text { get { return _Text; } }
        public IEnumerable<Node> ChildNodes { get { return _ChildNodes.Values; } }
        public IEnumerable<Node> ParentNodes { get { return _ParentNodes.Values; } }

        private Node() { }
        internal Node(string text) : this(text, SequentialId.NewId()) { }
        internal Node(string text,long nodeId)
        {
            _Text = text;
            _NodeId = nodeId;
            _ChildNodes = new SortedDictionary<long, Node>();
            _ParentNodes = new SortedDictionary<long, Node>();
        }

        public int CountChildNodes()
        {
            return _ChildNodes.Count;
        }

        public int CountParentNodes()
        {
            return _ParentNodes.Count;
        }

        internal protected void AddChild(Node node)
        {
            if (!_ChildNodes.ContainsKey(node.NodeId))
            {
                _ChildNodes.Add(node.NodeId, node);
                NotifyPropertyChanged("ChildNodes");
            }
        }

        internal protected void AddParent(Node node)
        {
            if (!_ParentNodes.ContainsKey(node.NodeId))
            {
                _ParentNodes.Add(node.NodeId, node);
                NotifyPropertyChanged("ParentNodes");
            }
        }

        internal protected void RemoveChild(Node node)
        {
            _ChildNodes.Remove(node.NodeId);
            NotifyPropertyChanged("ChildNodes");
        }

        internal protected void RemoveParent(Node node)
        {
            _ParentNodes.Remove(node.NodeId);
            NotifyPropertyChanged("ParentNodes");
        }

        internal protected void SetText(string newText)
        {
            _Text = newText;
            NotifyPropertyChanged("Text");
        }
    }
}
