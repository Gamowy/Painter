using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Painter
{
    /// <summary>
    /// Interaction logic for ResizeCanvasDialog.xaml
    /// </summary>
    public partial class ResizeCanvasDialog : Window
    {
        public ResizeCanvasDialog(double canvasWidth, double canvasHeight)
        {
            InitializeComponent();
            canvasWidthTextBox.Text = canvasWidth.ToString();
            canvasHeightTextBox.Text = canvasHeight.ToString();
            canvasWidthTextBox.Select(canvasWidthTextBox.Text.Length, 0);
            canvasHeightTextBox.Select(canvasHeightTextBox.Text.Length, 0);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(canvasWidthTextBox.Text) || string.IsNullOrEmpty(canvasHeightTextBox.Text) ||
                canvasWidthTextBox.Text.Equals("0") || canvasHeightTextBox.Text.Equals("0"))
            {
                MessageBox.Show("Wprowadź poprawną szerokość i wysokość płótna.", "Niepoprawne wartości!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                DialogResult = true;
            }
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public int CanvasWidth
        {
            get { return int.Parse(canvasWidthTextBox.Text); }
        }

        public int CanvasHeight
        {
            get { return int.Parse(canvasHeightTextBox.Text); }
        }
    }
}
