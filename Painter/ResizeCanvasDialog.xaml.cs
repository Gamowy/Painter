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
        Regex inputRegex = new Regex("[^0-9]+");

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
            e.Handled = inputRegex.IsMatch(e.Text);
        }

        private void NumberValidation(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (inputRegex.IsMatch(canvasWidthTextBox.Text))
                {
                    canvasWidthTextBox.Text = "";
                    throw new ArgumentException();
                }
                if (inputRegex.IsMatch(canvasHeightTextBox.Text))
                {
                    canvasHeightTextBox.Text = "";
                    throw new ArgumentException();
                }
            }
            catch
            {
                MessageBox.Show("Wprowadzono nieprawidłową wartość dla rozmiaru płótna!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
