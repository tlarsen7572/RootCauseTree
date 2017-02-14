using GraphSharp.Controls;

namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    public class RootCauseGraphLayout : GraphLayout<RootCauseVertex, RootCauseEdge, RootCauseGraph>
    {
        public RootCauseGraphLayout Copy()
        {
            var layout= new RootCauseGraphLayout();
            layout.Graph = Graph;
            return layout;
        }
    }
}
