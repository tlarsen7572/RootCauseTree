using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class ProblemContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private Node _InitialProblem;
        private Stack<IRootCauseCommand> _UndoActions = new Stack<IRootCauseCommand>();
        private Stack<IRootCauseCommand> _RedoActions = new Stack<IRootCauseCommand>();

        public Node InitialProblem { get { return _InitialProblem; } }

        public ProblemContainer(string initialProblemText)
        {
            _InitialProblem = NodeFactory.CreateProblem(initialProblemText, SequentialId.NewId());
            _InitialProblem.PropertyChanged += _InitialProblem_PropertyChanged;
        }

        public ProblemContainer(Node node)
        {
            _InitialProblem = node;
            _InitialProblem.PropertyChanged += _InitialProblem_PropertyChanged;
        }

        public int CountUndoActions() { return _UndoActions.Count; }
        public int CountRedoActions() { return _RedoActions.Count; }

        public void AddAction(IRootCauseCommand command)
        {
            if (!command.Executed) { command.Execute(); }
            _UndoActions.Push(command);
            _RedoActions.Clear();
        }

        public void Undo()
        {
            if (_UndoActions.Count > 0)
            {
                _UndoActions.Peek().Undo();
                _RedoActions.Push(_UndoActions.Pop());
            }
        }

        public void Redo()
        {
            if (_RedoActions.Count > 0)
            {
                _RedoActions.Peek().Execute();
                _UndoActions.Push(_RedoActions.Pop());
            }
        }

        private void _InitialProblem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged($"InitialProblem.{e.PropertyName}");
        }
    }
}
