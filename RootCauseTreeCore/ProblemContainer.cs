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
                var command = _UndoActions.Pop();
                command.Undo();
                _RedoActions.Push(command);
            }
        }

        public void Redo()
        {
            if (_RedoActions.Count > 0)
            {
                var command = _RedoActions.Pop();
                command.Execute();
                _UndoActions.Push(command);
            }
        }

        private void _InitialProblem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged($"InitialProblem.{e.PropertyName}");
        }
    }
}
