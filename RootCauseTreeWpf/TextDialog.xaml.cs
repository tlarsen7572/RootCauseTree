using System.Windows;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : Window
    {
        public string Text { get; private set; }
        public TextDialog(Window owner, string title,string description1,string description2 = "")
        {
            InitializeComponent();
            Owner = owner;
            Title = title;
            Description1.Text = description1;
            Description2.Text = description2;
            if (description2.Equals(string.Empty)) Description2.Visibility = Visibility.Collapsed;
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
