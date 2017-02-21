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
        public delegate void FileChangedEventHandler(string fileName);
        public event FileChangedEventHandler FileChanged;
        private void NotifyFileChanged()
        {
            string fileName;
            if (Path.Equals(string.Empty)) fileName = string.Empty;
            else
            {
                fileName = new System.IO.FileInfo(Path).Name;
            }
            FileChanged?.Invoke(fileName);
        }

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
            algs = app.algs;
        }

        private App app;
        private string Path;
        private bool isFileOpen;
        private ProblemContainer currentProblem;
        private Graphing.RootCauseGraph graph;
        private bool IsAddingLink = false;
        public GraphSharp.Algorithms.Layout.ILayoutParameters algs;

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

        public bool CanInteractWithMenuArea { get { return !IsAddingLink; } set
            {
                IsAddingLink = !value;
                NotifyIsAddingLink();
            }
        }

        public Visibility ShowCancelAddLink
        {
            get
            {
                return IsAddingLink ? Visibility.Visible : Visibility.Hidden;
            }
            set
            {
                IsAddingLink = value == Visibility.Visible ? true : false;
                NotifyIsAddingLink();
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
            CloseFile();
            CheckForNetworkPath(path);
            SqliteDb.GetInstance().LoadFile(path).ToList().ForEach(Problems.Add);
            Path = path;
            IsFileOpen = true;
            NotifyFileChanged();
            return true;
        }

        public bool NewFile(string path)
        {
            CloseFile();
            CheckForNetworkPath(path);
            var conn = SqliteDb.GetInstance().CreateNewFile(path);
            Path = path;
            IsFileOpen = (conn != null);
            NotifyFileChanged();
            return IsFileOpen;
        }

        public void CloseFile()
        {
            SqliteDb.GetInstance().CloseConnection();
            Path = string.Empty;
            IsFileOpen = false;
            Problems.Clear();
            CurrentProblem = null;
            Graph = new Graphing.RootCauseGraph();
            NotifyFileChanged();
        }

        private void CheckForNetworkPath(string path)
        {
            bool isNetwork = false;
            var info = new System.IO.FileInfo(path);
            var root = info.Directory.Root;
            if (root.FullName.Substring(0, 2).Equals(@"\\")) isNetwork = true;
            else
            {
                var drive = new System.IO.DriveInfo(root.FullName.Substring(0,1));
                if (drive.DriveType == System.IO.DriveType.Network)
                    isNetwork = true;
            }
            if (isNetwork) MessageBox.Show("The selected file is on a network location.  It is recommended you work on your local machine and copy the file to the network after you are finished.","Network File Warning");
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
            try
            {
                new RemoveNodeChainCommand(SqliteDb.GetInstance(), currentProblem.InitialProblem, true);
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                ProcessConnectionLost();
            }
            catch (Exception)
            {
                throw;
            }
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

        public void AddLink(Node parent,Node child)
        {
            ExecuteCommand(new AddLinkCommand(SqliteDb.GetInstance(), parent, child));
        }

        public void RemoveLink(Node parent, Node child)
        {
            ExecuteCommand(new RemoveLinkCommand(SqliteDb.GetInstance(), parent, child));
        }

        private void ExecuteCommand(IRootCauseCommand command)
        {
            try
            {
                CurrentProblem.AddAction(command);
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                ProcessConnectionLost();
            }
            catch (Exception)
            {
                throw;
            }
            GenerateGraph();
            NotifyUndoRedo();
        }

        private void NotifyUndoRedo()
        {
            NotifyPropertyChanged("CanUndo");
            NotifyPropertyChanged("CanRedo");
        }

        private void ProcessConnectionLost()
        {
            MessageBox.Show("The connection to the file has been lost and will be closed.  Please re-open the file to continue working.", "File Connection Lost");

            CloseFile();
        }

        private void NotifyCurrentProblemChanged()
        {
            NotifyPropertyChanged("CurrentProblem");
            NotifyPropertyChanged("CanSaveImage");
            NotifyUndoRedo();
        }

        private void NotifyIsAddingLink()
        {
            NotifyPropertyChanged("CanInteractWithMenuArea");
            NotifyPropertyChanged("ShowCancelAddLink");
        }
    }
}
