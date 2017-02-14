using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    public class RootCauseVertex
    {
        private string text;

        public long Id { get; private set; }
        public string Text { get { return text; } private set { text = value; } }
        public Node Source { get; private set; }

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
