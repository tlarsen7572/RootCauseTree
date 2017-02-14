using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.PorcupineSupernova.RootCauseTreeCore;
using System.Windows;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public MainWindowViewModel()
        {
            Problems = new ObservableCollection<ProblemContainer>();
            Graph = new Graphing.RootCauseGraph();
            app = (App)Application.Current;
        }

        private App app;
        private string Path;
        private bool isFileOpen;
        private ProblemContainer currentProblem;
        private Graphing.RootCauseGraph graph;

        public Graphing.RootCauseGraph Graph { get { return graph; } private set
            {
                if (ReferenceEquals(value, graph)) return;
                graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        public string LayoutAlgorithmType { get; private set; }
        public ObservableCollection<ProblemContainer> Problems { get; private set; }
        public ProblemContainer CurrentProblem { get { return currentProblem; } set
            {
                currentProblem = value;
                if (value != null) GenerateGraph();
                NotifyCurrentProblemChanged();
            }
        }

        public bool IsFileOpen { get { return isFileOpen; } private set
            {
                isFileOpen = value;
                NotifyPropertyChanged("IsFileOpen");
                NotifyUndoRedo();
            }
        }
        public bool CanUndo { get { return currentProblem?.CountUndoActions() > 0; } }
        public bool CanRedo { get { return currentProblem?.CountRedoActions() > 0; } }
        public bool CanSaveImage { get { return currentProblem != null; } }

        public bool OpenFile(string path)
        {
            Path = path;
            Problems.Clear();
            SqliteDb.GetInstance().LoadFile(path).ToList().ForEach(Problems.Add);
            IsFileOpen = true;
            Graph = new Graphing.RootCauseGraph();
            return true;
        }

        public bool NewFile(string path)
        {
            Path = path;
            Problems.Clear();
            bool result = SqliteDb.GetInstance().CreateNewFile(path);
            IsFileOpen = result;
            return result;
        }

        public void CreateProblem(string text)
        {
            var newProblem = new CreateProblemContainer(SqliteDb.GetInstance(), text, true).Container;
            Problems.Add(newProblem);
            CurrentProblem = newProblem;
        }

        public void GenerateGraph()
        {
            Graph = app.GenerateGraph(CurrentProblem?.InitialProblem);
        }

        public void CreateChildNode(string text,Node parent)
        {
            ExecuteCommand(new AddNodeCommand(SqliteDb.GetInstance(), parent, text));
        }

        public void EditNodeText(string text,Node parent)
        {
            ExecuteCommand(new ChangeNodeTextCommand(SqliteDb.GetInstance(), parent, text));
        }

        public void Undo()
        {
            currentProblem.Undo();
            GenerateGraph();
            NotifyUndoRedo();
        }

        public void Redo()
        {
            currentProblem.Redo();
            GenerateGraph();
            NotifyUndoRedo();
        }

        public void DeleteProblem()
        {
            new RemoveNodeChainCommand(SqliteDb.GetInstance(), currentProblem.InitialProblem,true);
            Problems.Remove(currentProblem);
            currentProblem = null;
            GenerateGraph();
        }

        public void DeleteCause(Node node)
        {
            ExecuteCommand(new RemoveNodeCommand(SqliteDb.GetInstance(), node));
        }

        public void DeleteCauseChain(Node node)
        {
            ExecuteCommand(new RemoveNodeChainCommand(SqliteDb.GetInstance(), node));
        }

        private void ExecuteCommand(IRootCauseCommand command)
        {
            CurrentProblem.AddAction(command);
            GenerateGraph();
            NotifyUndoRedo();
        }

        private void NotifyUndoRedo()
        {
            NotifyPropertyChanged("CanUndo");
            NotifyPropertyChanged("CanRedo");
        }

        private void NotifyCurrentProblemChanged()
        {
            NotifyPropertyChanged("CurrentProblem");
            NotifyPropertyChanged("CanSaveImage");
            NotifyUndoRedo();
        }
    }
}
