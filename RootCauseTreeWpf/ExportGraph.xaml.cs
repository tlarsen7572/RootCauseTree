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
using System.Windows.Shapes;
using System.ComponentModel;
using com.PorcupineSupernova.RootCauseTreeCore;
using Microsoft.Win32;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for SaveGraph.xaml
    /// </summary>
    public partial class ExportGraph : Window, INotifyPropertyChanged
    {
        public ExportGraph(Node node)
        {
            InitializeComponent();
            app = (App)Application.Current;
            _node = node;
            saveImageDlg.DefaultExt = imageExt;
            saveImageDlg.Filter = imageFilter;
            saveImageDlg.CheckPathExists = true;
            saveImageDlg.AddExtension = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private App app;
        private Node _node;
        private Graphing.RootCauseGraph _graph;
        private SaveFileDialog saveImageDlg = new SaveFileDialog();
        private string imageFilter = "Portable Network Graphics (.png)|*.png";
        private string imageExt = ".png";

        public Graphing.RootCauseGraph Graph { get { return _graph; }private set
            {
                if (value == _graph) return;
                _graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        public string LayoutAlgorithmType { get; private set; }
        public string MinMaxState
        {
            get
            {
                return WindowState == WindowState.Maximized? "\uE1D8" : "\uE1D9";
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            Graph = app.GenerateGraph(_node);
        }

        private void SaveFile_Click(object sender,RoutedEventArgs e)
        {
            bool? didSelectFile = saveImageDlg.ShowDialog();
            if (didSelectFile.HasValue && didSelectFile.Value == true)
            {
                string path = saveImageDlg.FileName;

                //Draw the bitmap from the control
                var bounds = new Rect(TreeGrid.RenderSize);
                double dpi = 96d;
                var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Pbgra32);
                bitmap.Render(TreeGrid);

                //Encode the bitmap as png
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                //Save the png encoding as a file
                try
                {
                    using (var stream = System.IO.File.Create(path))
                    {
                        encoder.Save(stream);
                    }
                }
                catch (System.Security.SecurityException)
                {
                    MessageBox.Show("You do not have permission to save the image in this location.", "Save Image Failed");
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not save the image.  Please try again or select a different location.", "Save Image Failed");
                }
            }
            saveImageDlg.FileName = string.Empty;
        }

        private void MinMaxWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
            NotifyPropertyChanged("MinMaxState");
        }
    }
}
