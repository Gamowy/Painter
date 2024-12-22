using System.Windows;
using System.Windows.Media;

namespace Painter
{
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

        public PickColorDialog(Color toolColor)
        {
            InitializeComponent();
            ColorViewerColor = toolColor;     
            colorViewer.Fill = new SolidColorBrush(ColorViewerColor);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
