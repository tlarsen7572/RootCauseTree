namespace com.PorcupineSupernova.RootCauseTreeCore
{
    interface IRootCauseCommand
    {
        void Execute();
        void Undo();
        bool Executed { get; }
    }
}
