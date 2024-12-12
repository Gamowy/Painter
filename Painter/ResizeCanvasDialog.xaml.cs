using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Painter
{
    /// <summary>
    /// Interaction logic for ResizeCanvasDialog.xaml
    /// </summary>
    public partial class ResizeCanvasDialog : Window
    {
        public ResizeCanvasDialog()
        {
            InitializeComponent();
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
