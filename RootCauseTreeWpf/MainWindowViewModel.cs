using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    class MainWindowViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private List<ProblemContainer> problems = new List<ProblemContainer>();
        private string Path;
        private bool isFileOpen;
        private ProblemContainer currentProblem;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public List<ProblemContainer> Problems { get { return problems; } private set { } }
        public ProblemContainer CurrentProblem { get { return currentProblem; } set
            {
                if (object.ReferenceEquals(value, currentProblem)) return;
                currentProblem = value;
                NotifyPropertyChanged("CurrentProblem");
            }
        }
        public bool IsFileOpen { get { return isFileOpen; } private set
            {
                if (value == isFileOpen) { return; }
                isFileOpen = value;
                NotifyPropertyChanged("IsFileOpen");
            }
        }

        public bool OpenFile(string path)
        {
            Path = path;
            problems.Clear();
            problems.AddRange(SqliteDb.GetInstance().LoadFile(path));
            IsFileOpen = true;
            NotifyPropertyChanged("Problems");
            return true;
        }

        public bool NewFile(string path)
        {
            Path = path;
            problems.Clear();
            bool result = SqliteDb.GetInstance().CreateNewFile(path);
            IsFileOpen = result;
            return result;
        }

        public void CreateProblem(string text)
        {
            var newProblem = new CreateProblemContainer(SqliteDb.GetInstance(), text, true).Container;
            problems.Add(newProblem);
            NotifyPropertyChanged("Problems");
        }
    }
}
