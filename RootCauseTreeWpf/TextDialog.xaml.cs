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

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : Window
    {
        public string Text { get; private set; }
        public TextDialog(Window owner, string title,string description)
        {
            InitializeComponent();
            Owner = owner;
            Title = title;
            Description.Text = description;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Hide();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NodeText.Text))
            {
                Error.Text = "The text cannot be blank.";
                return;
            }
            Text = NodeText.Text;
            DialogResult = true;
            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NodeText.Focus();
        }
    }
}
