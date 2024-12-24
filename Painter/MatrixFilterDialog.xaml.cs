using System.ComponentModel;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Painter
{
    /// <summary>
    /// Interaction logic for PickColorDialog.xaml
    /// </summary>
    public partial class MatrixFilterDialog : Window
    {
        private float[,] _kernel;
        public float[,] Kernel
        {
            get
            {
                return _kernel;
            }
            set
            {
                _kernel = value;
            }
        }
        Regex inputRegex = new Regex("^-?\\d*\\.?\\d*$");
        
        public bool resultOK = false;
        public bool normalize = false;
        public bool grayscale = false;

        public MatrixFilterDialog(float[,] kernel, bool normalizeChecked=true, bool grayscaleChecked=false)
        {
            InitializeComponent();
            _kernel = kernel;
            matrix00.Text = _kernel[0, 0].ToString();
            matrix01.Text = _kernel[0, 1].ToString();
            matrix02.Text = _kernel[0, 2].ToString();
            matrix10.Text = _kernel[1, 0].ToString();
            matrix11.Text = _kernel[1, 1].ToString();
            matrix12.Text = _kernel[1, 2].ToString();
            matrix20.Text = _kernel[2, 0].ToString();
            matrix21.Text = _kernel[2, 1].ToString();
            matrix22.Text = _kernel[2, 2].ToString();

            matrix00.Select(matrix00.Text.Length, 0);
            matrix01.Select(matrix01.Text.Length, 0);
            matrix02.Select(matrix02.Text.Length, 0);
            matrix10.Select(matrix10.Text.Length, 0);
            matrix11.Select(matrix11.Text.Length, 0);
            matrix12.Select(matrix12.Text.Length, 0);
            matrix20.Select(matrix20.Text.Length, 0);
            matrix21.Select(matrix21.Text.Length, 0);
            matrix22.Select(matrix22.Text.Length, 0);

            normalizationCheckBox.IsChecked = normalizeChecked;
            grayScaleCheeckBox.IsChecked = grayscaleChecked;
        }
        private void InputValidation(object sender, TextCompositionEventArgs e)
        {
            if (!inputRegex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _kernel[0, 0] = float.Parse(matrix00.Text);
                _kernel[0, 1] = float.Parse(matrix01.Text);
                _kernel[0, 2] = float.Parse(matrix02.Text);
                _kernel[1, 0] = float.Parse(matrix10.Text);
                _kernel[1, 1] = float.Parse(matrix11.Text);
                _kernel[1, 2] = float.Parse(matrix12.Text);
                _kernel[2, 0] = float.Parse(matrix20.Text);
                _kernel[2, 1] = float.Parse(matrix21.Text);
                _kernel[2, 2] = float.Parse(matrix22.Text);

                resultOK = true;
            }
            catch
            {
                MessageBox.Show("W macierzy wprowadzono nieprawidłowe wartości!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Close();
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (normalizationCheckBox.IsChecked == true)
            {
                normalize = true;
            }
            if (grayScaleCheeckBox.IsChecked == true)
            {
                grayscale = true;
            }
        }
    }
}
