namespace com.PorcupineSupernova.RootCauseTreeWpf.Graphing
{
    class RootCauseVertex
    {
        public long Id { get; private set; }
        public string Text { get; private set; }

        public RootCauseVertex(long id, string text)
        {
            Id = id;
            Text = text;
        }

        public override string ToString()
        {
            return $"{Id.ToString()}: {Text}";
        }
    }
}
