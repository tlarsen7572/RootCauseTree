using com.PorcupineSupernova.RootCauseTreeCore;
using System.Windows.Media;
using System.ComponentModel;

namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    public enum RootCauseVertexType { RootNode=0,ChildNode=1,FinalChildNode=2,SelectedNode=3}
    public class RootCauseVertex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string text;
        private bool selected = false;

        public long Id { get; private set; }
        public string Text { get { return text; } private set { text = value; } }
        public Node Source { get; private set; }
        public RootCauseVertexType VertexType
        {
            get
            {
                if (selected) return RootCauseVertexType.SelectedNode;
                else if (Source.CountParentNodes() == 0) return RootCauseVertexType.RootNode;
                else if (Source.CountChildNodes() == 0) return RootCauseVertexType.FinalChildNode;
                else return RootCauseVertexType.ChildNode;
            }
        }

        public RootCauseVertex(Node source)
        {
            Id = source.NodeId;
            Text = source.Text;
            Source = source;
        }

        public void Select()
        {
            selected = true;
            NotifyPropertyChanged("VertexType");
        }

        public void UnSelect()
        {
            selected = false;
            NotifyPropertyChanged("VertexType");
        }

        public override string ToString()
        {
            return $"{Id.ToString()}: {Text}";
        }
    }
}
