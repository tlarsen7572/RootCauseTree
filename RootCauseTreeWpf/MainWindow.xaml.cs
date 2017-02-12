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
using Microsoft.Win32;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm;
        private OpenFileDialog openFileDlg = new OpenFileDialog();
        private SaveFileDialog newFileDlg = new SaveFileDialog();
        private string fileFilter = "Root Cause Files|*.rootcause";
        private string defaultExt = ".rootcause";
        private double scaleFactor = 1.2;
        private bool leftMouseDragged = false;
        private Point lastMousePos;
        private double MaxScale = 3;
        private double MinScale = 0.3;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            vm = DataContext as MainWindowViewModel;

            openFileDlg.Multiselect = false;
            openFileDlg.DefaultExt = defaultExt;
            openFileDlg.Filter = fileFilter;
            newFileDlg.CheckPathExists = true;
            newFileDlg.DefaultExt = defaultExt;
            newFileDlg.Filter = fileFilter;
            newFileDlg.AddExtension = true;
        }

        private void TreeContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var position = e.GetPosition(TreeCanvas);
            var transform = TreeCanvas.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;

            var scale = e.Delta >= 0 ? scaleFactor : (1.0 / scaleFactor);
            var newScaleFactor = matrix.M11 * scale;
            if (!(newScaleFactor < MaxScale && newScaleFactor > MinScale)) return;

            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            transform.Matrix = matrix;
            TreeCanvas.UpdateLayout();
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
            var textDlg = new TextDialog(this,"Add Problem", "Enter a new problem statement.");
            if (textDlg.ShowDialog().Value)
            {
                vm.CreateProblem(textDlg.Text);
            }
        }

        private void TreeContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            leftMouseDragged = true;
            lastMousePos = e.GetPosition(this);
        }

        private void TreeContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            leftMouseDragged = false;
        }

        private void TreeContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!leftMouseDragged) return;

            var position = e.GetPosition(this);
            var transform = TreeCanvas.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            matrix.Translate(position.X - lastMousePos.X, position.Y - lastMousePos.Y);
            transform.Matrix = matrix;
            lastMousePos = position;
            TreeCanvas.UpdateLayout();
        }

        private void ProblemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TreeCanvas.Margin = new Thickness(ProblemList.ActualWidth + 50, 50, 0, 0);
            TreeCanvas.RenderTransform = new MatrixTransform();
        }

        private void Node_Click(object sender,RoutedEventArgs e)
        {
            var dataItem = ((FrameworkElement)sender).DataContext as Graphing.RootCauseVertex;
            var textDlg = new TextDialog(this, "Add Cause", "Enter a new causal statement.");
            if (textDlg.ShowDialog().Value)
            {
                vm.CreateChildNode(textDlg.Text,dataItem.Source);
            }
        }
    }
}
