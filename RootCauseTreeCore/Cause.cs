namespace com.PorcupineSupernova.RootCauseTreeCore
{
    sealed class Cause : Node
    {
        public Cause(string text) : base(text) { }
        public Cause(string text,long nodeId) : base(text, nodeId) { }
    }
}
