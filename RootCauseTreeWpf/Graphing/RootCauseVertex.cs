using com.PorcupineSupernova.RootCauseTreeCore;
using System.Windows.Media;

namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    public enum RootCauseVertexType { RootNode=0,ChildNode=1,FinalChildNode=2}
    public class RootCauseVertex
    {
        private string text;

        public long Id { get; private set; }
        public string Text { get { return text; } private set { text = value; } }
        public Node Source { get; private set; }
        public RootCauseVertexType VertexType
        {
            get
            {
                if (Source.CountParentNodes() == 0) return RootCauseVertexType.RootNode;
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

        public override string ToString()
        {
            return $"{Id.ToString()}: {Text}";
        }
    }
}
