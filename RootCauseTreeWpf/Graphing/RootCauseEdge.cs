using QuickGraph;

namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    class RootCauseEdge : Edge<RootCauseVertex>
    {
        public RootCauseEdge(RootCauseVertex source,RootCauseVertex target) : base(source, target) { }
    }
}
