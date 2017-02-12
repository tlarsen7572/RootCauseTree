using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.PorcupineSupernova.RootCauseTreeCore;

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
            LayoutAlgorithmType = "Tree";
        }

        private string Path;
        private bool isFileOpen;
        private ProblemContainer currentProblem;
        private Graphing.RootCauseGraph graph;
        private bool treeIsVisible;

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
                NotifyPropertyChanged("CurrentProblem");
            }
        }
        public bool IsFileOpen { get { return isFileOpen; } private set
            {
                isFileOpen = value;
                NotifyPropertyChanged("IsFileOpen");
                NotifyPropertyChanged("CanUndo");
                NotifyPropertyChanged("CanRedo");
            }
        }
        public bool CanUndo { get { return currentProblem?.CountUndoActions() > 0; } }
        public bool CanRedo { get { return currentProblem?.CountRedoActions() > 0; } }
        public bool TreeIsVisible { get { return treeIsVisible; } set
            {
                treeIsVisible = value;
                NotifyPropertyChanged("TreeIsVisible");
            }
        }

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
            var newGraph = new Graphing.RootCauseGraph();
            var vertices = new Dictionary<long, Graphing.RootCauseVertex>();
            var problem = currentProblem.InitialProblem;
            bool vertexExists;

            vertices.Add(problem.NodeId, new Graphing.RootCauseVertex(problem));
            newGraph.AddVertex(vertices[problem.NodeId]);

            Func<Node, bool> recurseNodes = null;
            recurseNodes = (Node parent) =>
            {
                foreach (var child in parent.ChildNodes)
                {
                    vertexExists = vertices.ContainsKey(child.NodeId);

                    if (!vertexExists)
                    {
                        vertices.Add(child.NodeId, new Graphing.RootCauseVertex(child));
                        newGraph.AddVertex(vertices[child.NodeId]);
                    }

                    newGraph.AddEdge(new Graphing.RootCauseEdge(vertices[parent.NodeId], vertices[child.NodeId]));
                    if (!vertexExists) recurseNodes(child);
                }
                return true;
            };

            recurseNodes(problem);
            Graph = newGraph;
        }

        public void CreateChildNode(string text,Node parent)
        {
            currentProblem.AddAction(new AddNodeCommand(SqliteDb.GetInstance(), parent, text));
            GenerateGraph();
            NotifyPropertyChanged("CanUndo");
        }
    }
}
