using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.PorcupineSupernova.RootCauseTreeWpf;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm;
        private Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
        private Microsoft.Win32.SaveFileDialog newFileDlg = new Microsoft.Win32.SaveFileDialog();
        private string fileFilter = "Root Cause Files|*.rootcause";
        private string defaultExt = ".rootcause";

        public MainWindow()
        {
            InitializeComponent();
            vm = new MainWindowViewModel();
            DataContext = vm;
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            bool? didOpen = openFileDlg.ShowDialog();
            if (didOpen.HasValue && didOpen.Value == true)
            {
                vm.OpenFile(openFileDlg.FileName);
                Title = $"Arborist: {openFileDlg.SafeFileName}";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            openFileDlg.Multiselect = false;
            openFileDlg.DefaultExt = defaultExt;
            openFileDlg.Filter = fileFilter;
            newFileDlg.CheckPathExists = true;
            newFileDlg.DefaultExt = defaultExt;
            newFileDlg.Filter = fileFilter;
            newFileDlg.AddExtension = true;

            Microsoft.Msagl.WpfGraphControl.GraphViewer viewer = new Microsoft.Msagl.WpfGraphControl.GraphViewer();
            viewer.BindToPanel(Workspace);
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            bool? didSelectFile = newFileDlg.ShowDialog();
            if (didSelectFile.HasValue && didSelectFile.Value == true)
            {
                vm.NewFile(newFileDlg.FileName);
                Title = $"Arborist: {newFileDlg.SafeFileName}";
            }
        }

        private void AddProblem_Click(object sender, RoutedEventArgs e)
        {
            var textDlg = new TextDialog("Add Problem", "Enter a new problem statement.");
            if (textDlg.ShowDialog().Value)
            {
                vm.CreateProblem(textDlg.Text);
            }
        }
    }
}
