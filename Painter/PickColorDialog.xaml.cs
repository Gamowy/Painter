using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Painter
{
    public struct Rgb
    {
        public Rgb(double red, double blue, double green)
        {
            this.red = red;
            this.blue = blue;
            this.green = green;
        }
        public double red;
        public double blue;
        public double green;
    }
    public struct Hsv
    {
        public Hsv(double hue, double saturation, double value)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }
        public double hue;
        public double saturation;
        public double value;
    }

    /// <summary>
    /// Interaction logic for PickColorDialog.xaml
    /// </summary>
    public partial class PickColorDialog : Window
    {
        private Color _colorViewerColor;
        public Color ColorViewerColor
        {
            get
            {
                return _colorViewerColor;
            }
            set
            {
                _colorViewerColor = value;
            }
        }

        Regex rgbValueRegex = new Regex("[0-9]{1,3}");
        Regex hsvValueRegex = new Regex("^\\d*\\.?\\d*$");
        public bool resultOK = false;
        bool valueUpdateFlag = false;

        public PickColorDialog(Color toolColor)
        {
            InitializeComponent();
            _colorViewerColor = toolColor;
            colorViewer.Fill = new SolidColorBrush(ColorViewerColor);
            setRgbTextBoxes();
            setHsvTextBoxes();

        }

        private Hsv rgbToHsvConverter(Rgb color)
        {
            double rPrim = color.red / 255.0;
            double gPrim = color.green / 255.0;
            double bPrim = color.blue / 255.0;

            double mMax = Math.Max(rPrim, Math.Max(gPrim, bPrim));
            double mMin = Math.Min(rPrim, Math.Min(gPrim, bPrim));
            double delta = mMax - mMin;

            double hue = 0;
            if (delta == 0)
            {
                hue = 0;
            }
            else if (mMax == rPrim)
            {
                hue = 60 * (((gPrim - bPrim) / delta) % 6);
            }
            else if (mMax == gPrim)
            {
                hue = 60 * (((bPrim - rPrim) / delta) + 2);
            }
            else if (mMax == bPrim)
            {
                hue = 60 * (((rPrim - gPrim) / delta) + 4);
            }

            double saturation = (mMax == 0) ? 0 : delta / mMax;

            double value = mMax;

            return new Hsv(Math.Abs(hue), saturation, value);
        }

        private Rgb hsvToRgbConverter(Hsv color)
        {
            double hue = color.hue;

            double c = color.value * color.saturation;
            double x = c * (1 - Math.Abs((color.hue / 60) % 2 - 1));
            double m = color.value - c;

            double rPrim = 0;
            double gPrim = 0;
            double bPrim = 0;
            if (0 <= hue && hue < 60)
            {
                rPrim = c;
                gPrim = x;
            }
            else if (60 <= hue && hue < 120)
            {
                rPrim = x;
                gPrim = c;
            }
            else if (120 <= hue && hue < 180)
            {
                gPrim = c;
                bPrim = x;
            }
            else if (180 <= hue && hue < 240)
            {
                gPrim = x;
                bPrim = c;
            }
            else if (240 <= hue && hue < 300)
            {
                rPrim = x;
                bPrim = c;
            }
            else if (300 <= hue && hue < 360)
            {
                rPrim = c;
                bPrim = x;
            }

            return new Rgb((rPrim + m) * 255, (gPrim + m) * 255, (bPrim + m) * 255);
        }

        private void RgbTextBoxValidation(object sender, TextCompositionEventArgs e)
        {
            if (!rgbValueRegex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private void HsvTextBoxValidation(object sender, TextCompositionEventArgs e)
        {
            if (!hsvValueRegex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private void setRgbTextBoxes()
        {
            valueUpdateFlag = true;
            redTextbox.Text = ColorViewerColor.R.ToString();
            greenTextbox.Text = ColorViewerColor.G.ToString();
            blueTextbox.Text = ColorViewerColor.B.ToString();
            redTextbox.Select(redTextbox.Text.Length, 0);
            blueTextbox.Select(blueTextbox.Text.Length, 0);
            greenTextbox.Select(greenTextbox.Text.Length, 0);
            valueUpdateFlag = false;
        }

        private void setHsvTextBoxes()
        {
            valueUpdateFlag = true;
            Hsv hsvToolColor = rgbToHsvConverter(new Rgb(ColorViewerColor.R, ColorViewerColor.G, ColorViewerColor.B));
            hueTextbox.Text = Math.Round(hsvToolColor.hue, 4).ToString();
            saturationTextbox.Text = Math.Round(hsvToolColor.saturation, 4).ToString();
            valueTextbox.Text = Math.Round(hsvToolColor.value, 4).ToString();
            hueTextbox.Select(hueTextbox.Text.Length, 0);
            saturationTextbox.Select(saturationTextbox.Text.Length, 0);
            valueTextbox.Select(valueTextbox.Text.Length, 0);
            valueUpdateFlag = false;
        }

        private void rgbTextboxChanged(object sender, TextChangedEventArgs e)
        {
            if (!valueUpdateFlag)
            {
                try
                {
                    // Validate input
                    if (!rgbValueRegex.IsMatch(redTextbox.Text))
                    {
                        redTextbox.Text = "";
                    }
                    if (!rgbValueRegex.IsMatch(greenTextbox.Text))
                    {
                        greenTextbox.Text = "";
                    }
                    if (!rgbValueRegex.IsMatch(blueTextbox.Text))
                    {
                        blueTextbox.Text = "";
                    }

                    int red, green, blue;
                    if (int.TryParse(redTextbox.Text, out red) && int.TryParse(greenTextbox.Text, out green) && int.TryParse(blueTextbox.Text, out blue))
                    {
                        if (red > 255)
                        {
                            red = 255;
                        }
                        if (green > 255)
                        {
                            green = 255;
                        }
                        if (blue > 255)
                        {
                            blue = 255;
                        }

                        ColorViewerColor = Color.FromRgb((byte)red, (byte)green, (byte)blue);
                        colorViewer.Fill = new SolidColorBrush(ColorViewerColor);
                        setHsvTextBoxes();
                    }
                }
                catch
                {
                    MessageBox.Show("Wprowadzono nieprawidłową wartość dla koloru!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void hsvTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (!valueUpdateFlag)
            {
                try
                {
                    // Validate input
                    if (!hsvValueRegex.IsMatch(hueTextbox.Text))
                    {
                        hueTextbox.Text = "";
                    }
                    if (!hsvValueRegex.IsMatch(saturationTextbox.Text))
                    {
                        saturationTextbox.Text = "";
                    }
                    if (!hsvValueRegex.IsMatch(valueTextbox.Text))
                    {
                        valueTextbox.Text = "";
                    }

                    double hue, saturation, value;
                    if (double.TryParse(hueTextbox.Text, out hue) && double.TryParse(saturationTextbox.Text, out saturation) && double.TryParse(valueTextbox.Text, out value))
                    {
                        if (hue > 360)
                        {
                            hue = 360;
                        }
                        if (saturation > 1)
                        {
                            saturation = 1;
                        }
                        if (value > 1)
                        {
                            value = 1;
                        }

                        Rgb rgbColor = hsvToRgbConverter(new Hsv(hue, saturation, value));
                        ColorViewerColor = Color.FromRgb((byte)rgbColor.red, (byte)rgbColor.green, (byte)rgbColor.blue);
                        colorViewer.Fill = new SolidColorBrush(ColorViewerColor);
                        setRgbTextBoxes();
                    }
                }
                catch
                {
                    MessageBox.Show("Wprowadzono nieprawidłową wartość dla koloru!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            resultOK = true;
            Close();
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
