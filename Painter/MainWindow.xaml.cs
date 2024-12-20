﻿using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Painter
{
    enum ToolType
    {
        Brush,
        Point,
        Line,
        EditLine,
        Polyline,
        Ellipse,
        Circle,
        Square,
        Rectangle,
        Hexagon
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point mouseDownPoint = new Point(); // Mouse position when left button is pressed
        
        Line? straightLine = null; // Straight line currently being created
        List<Line> canvasStraightLines = new List<Line>(); // List of straight lines on paintSurface
        Line? selectedLine = null; // Stores the selected line
        bool isEditingStartPoint = false; // Track which point is being edited

        Ellipse? newEllipse = null;
        Rectangle? newRect = null;
        Polyline? newPolyline = null;

        ToolType selectedTool; // Currently selected tool
        Color toolColor = Colors.Black; // Color for all tools
        int toolSize
        {
            get
            {
                return (int)sizeComboBox.SelectedValue;

            }
        }


        System.Windows.Media.Effects.Effect dropShadow;
        bool mouseUp = true;

        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth / 1.45;
            this.Height = SystemParameters.PrimaryScreenHeight / 1.45;
            paintSurface.Width = 1200;
            paintSurface.Height = 600;
            selectedTool = ToolType.Brush;
            brushButton.IsChecked = true;

            // Makes menu items align to right
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () =>
            {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
            };
            setAlignmentValue();
            SystemParameters.StaticPropertyChanged += (sender, e) => { setAlignmentValue(); };

            // Add values to tool size combobox
            for (int i = 4; i <= 64; i+=4)
            {
                sizeComboBox.Items.Add(i);
            }
            sizeComboBox.SelectedIndex = 0;

            // Save canvas dropshadow for time when we need to remove it during saving to png
            dropShadow = paintSurface.Effect;
        }

        private void resetTools()
        {
            switch (selectedTool)
            {
                case ToolType.Line:
                    straightLine = null;
                    paintSurface.Cursor = Cursors.Cross;
                    break;
                case ToolType.EditLine:
                    selectedLine = null;
                    isEditingStartPoint = false;
                    paintSurface.Cursor = Cursors.Arrow;
                    statusBar.Visibility = Visibility.Collapsed;
                    ResetLineHighlight();
                    break;
                case ToolType.Polyline:
                    newPolyline = null;
                    statusBar.Visibility = Visibility.Collapsed;
                    break;
                case ToolType.Ellipse:
                    paintSurface.Cursor = Cursors.Cross;
                    newEllipse = null;
                    break;
                case ToolType.Rectangle:
                case ToolType.Square:
                    paintSurface.Cursor = Cursors.Cross;
                    newRect = null;
                    break;
            }
        }

        private void attachCanvas(Canvas canvas)
        {
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;

            foreach (var line in canvas.Children)
            {
                var l = line as Line;
                if (l != null && l.Tag != null)
                {
                    canvasStraightLines.Add(l);
                }
            }
        }
        private void HighlightLines()
        {
            var shadowEffect = new DropShadowEffect
            {
                Color = Colors.Blue,
                BlurRadius = 15,
                ShadowDepth = 0
            };

            foreach (var line in canvasStraightLines)
            {
                line.Effect = shadowEffect;
            }
        }

        private void ResetLineHighlight()
        {
            foreach (var line in canvasStraightLines)
            {
                line.Effect = null;
            }
        }

        #region Canvas events

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                mouseDownPoint = e.GetPosition(paintSurface);
                mouseUp = false;
            }
            switch (selectedTool)
            {
                case ToolType.Point:
                    Ellipse ellipse = new Ellipse();
                    ellipse.Fill = new SolidColorBrush(toolColor);
                    ellipse.Width = toolSize;
                    ellipse.Height = toolSize;
                    ellipse.Margin = new Thickness(mouseDownPoint.X - ellipse.Width / 2, mouseDownPoint.Y - ellipse.Width / 2, 0, 0);
                    paintSurface.Children.Add(ellipse);
                    break;
                case ToolType.Line:
                    if (straightLine is null)
                    {
                        straightLine = new Line();
                        straightLine.Stroke = new SolidColorBrush(toolColor);
                        straightLine.Tag = "straightLine";
                        straightLine.StrokeThickness = toolSize;
                        straightLine.X1 = mouseDownPoint.X;
                        straightLine.Y1 = mouseDownPoint.Y;
                        paintSurface.Cursor = Cursors.Cross;
                    }
                    else
                    {
                        straightLine.X2 = mouseDownPoint.X;
                        straightLine.Y2 = mouseDownPoint.Y;
                        paintSurface.Children.Add(straightLine);
                        canvasStraightLines.Add(straightLine);
                        straightLine = null;
                        paintSurface.Cursor = Cursors.Arrow;
                    }
                    break;
                case ToolType.EditLine:
                    foreach (var line in canvasStraightLines)
                    {
                        if (IsMouseOverLinePoint(line.X1, line.Y1, mouseDownPoint))
                        {
                            selectedLine = line;
                            isEditingStartPoint = true;
                            break;
                        }
                        else if (IsMouseOverLinePoint(line.X2, line.Y2, mouseDownPoint))
                        {
                            selectedLine = line;
                            isEditingStartPoint = false;
                            break;
                        }
                    }
                    break;
                case ToolType.Polyline:
                    if (newPolyline is null)
                    {
                        newPolyline = new Polyline();
                        newPolyline.Stroke = new SolidColorBrush(toolColor);
                        newPolyline.StrokeThickness = toolSize;
                        newPolyline.Points.Add(mouseDownPoint);
                        paintSurface.Children.Add(newPolyline);
                        paintSurface.Cursor = Cursors.Cross;
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        newPolyline.Points.Add(mouseDownPoint);
                    }
                    else
                    {
                        if (newPolyline.Points.Count() < 2)
                            newPolyline.Points.Add(mouseDownPoint);
                        newPolyline = null;
                        paintSurface.Cursor = Cursors.Arrow;
                    }
                    break;
                case ToolType.Ellipse:
                case ToolType.Circle:
                    newEllipse = new Ellipse();
                    newEllipse.Stroke = new SolidColorBrush(toolColor);
                    newEllipse.StrokeThickness = toolSize;
                    newEllipse.Width = toolSize;
                    newEllipse.Height = toolSize;
                    newEllipse.Margin = new Thickness(mouseDownPoint.X - newEllipse.Width / 2, mouseDownPoint.Y - newEllipse.Height / 2, 0, 0);
                    paintSurface.Children.Add(newEllipse);
                    break;
                case ToolType.Rectangle:
                case ToolType.Square:
                    newRect = new Rectangle();
                    newRect.Stroke = new SolidColorBrush(toolColor);
                    newRect.StrokeThickness = toolSize;
                    newRect.Width = toolSize;
                    newRect.Height = toolSize;
                    newRect.Margin = new Thickness(mouseDownPoint.X - newRect.Width / 2 , mouseDownPoint.Y - newRect.Height / 2, 0, 0);
                    paintSurface.Children.Add(newRect);
                    break;
                case ToolType.Hexagon:
                    Polygon newHexagon = new Polygon();
                    newHexagon.Stroke = new SolidColorBrush(toolColor);
                    newHexagon.StrokeThickness = toolSize;
                    newHexagon.Points.Add(new Point(mouseDownPoint.X - 40, mouseDownPoint.Y - 80));
                    newHexagon.Points.Add(new Point(mouseDownPoint.X + 40, mouseDownPoint.Y - 80));
                    newHexagon.Points.Add(new Point(mouseDownPoint.X + 80, mouseDownPoint.Y));
                    newHexagon.Points.Add(new Point(mouseDownPoint.X + 40, mouseDownPoint.Y + 80));
                    newHexagon.Points.Add(new Point(mouseDownPoint.X - 40, mouseDownPoint.Y + 80));
                    newHexagon.Points.Add(new Point(mouseDownPoint.X - 80, mouseDownPoint.Y));
                    paintSurface.Children.Add(newHexagon);
                    break;
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(paintSurface);
            switch (selectedTool)
            {
                case ToolType.Brush:
                    if (!mouseUp && e.LeftButton == MouseButtonState.Pressed)
                    {
                        Line line = new Line();
                        line.Stroke = new SolidColorBrush(toolColor);
                        line.StrokeThickness = toolSize;
                        line.X1 = mouseDownPoint.X;
                        line.Y1 = mouseDownPoint.Y;
                        line.X2 = e.GetPosition(paintSurface).X;
                        line.Y2 = e.GetPosition(paintSurface).Y;
                        mouseDownPoint = e.GetPosition(paintSurface);
                        paintSurface.Children.Add(line);
                    }
                    break;
                case ToolType.EditLine:
                    paintSurface.Cursor = Cursors.Arrow;
                    // Change coursor if hoverd over line edit point
                    foreach (var line in canvasStraightLines)
                    {
                        if (IsMouseOverLinePoint(line.X1, line.Y1, currentMousePosition))
                        {
                            paintSurface.Cursor = Cursors.SizeAll;
                            break;
                        }
                        else if (IsMouseOverLinePoint(line.X2, line.Y2, currentMousePosition))
                        {
                            paintSurface.Cursor = Cursors.SizeAll;
                            break;
                        }
                    }

                    if (!mouseUp && selectedLine != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        
                        if (isEditingStartPoint)
                        {
                            selectedLine.X1 = currentMousePosition.X;
                            selectedLine.Y1 = currentMousePosition.Y;
                        }
                        else
                        {
                            selectedLine.X2 = currentMousePosition.X;
                            selectedLine.Y2 = currentMousePosition.Y;
                        }
                    }
                    break;
                case ToolType.Ellipse:
                case ToolType.Circle:
                    if (!mouseUp && newEllipse!= null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        paintSurface.Cursor = Cursors.SizeAll;
                        newEllipse.Width = Math.Abs(currentMousePosition.X - newEllipse.Margin.Left);
                        newEllipse.Height = (selectedTool == ToolType.Ellipse) ? Math.Abs(currentMousePosition.Y - newEllipse.Margin.Top) : newEllipse.Width;
                        newEllipse.Margin = new Thickness(mouseDownPoint.X - newEllipse.Width / 2, mouseDownPoint.Y - newEllipse.Height / 2, 0, 0);
                    }
                    break;
                case ToolType.Rectangle:
                case ToolType.Square:
                    if (!mouseUp && newRect != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        paintSurface.Cursor = Cursors.SizeAll;
                        newRect.Width = Math.Abs(currentMousePosition.X - newRect.Margin.Left);
                        newRect.Height = (selectedTool == ToolType.Rectangle) ? Math.Abs(currentMousePosition.Y - newRect.Margin.Top) : newRect.Width;
                        newRect.Margin = new Thickness(mouseDownPoint.X - newRect.Width / 2, mouseDownPoint.Y - newRect.Height / 2, 0, 0);
                    }
                    break;
            }
        }
        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            mouseUp = true;
            switch (selectedTool)
            {
                case ToolType.EditLine:
                    selectedLine = null;
                    break;
                case ToolType.Ellipse:
                case ToolType.Circle:
                    paintSurface.Cursor = Cursors.Cross;
                    newEllipse = null;
                    break;
                case ToolType.Rectangle:
                case ToolType.Square:
                    paintSurface.Cursor = Cursors.Cross;
                    newRect = null;
                    break;
            }
        }

        private bool IsMouseOverLinePoint(double x, double y, Point mousePosition)
        {
            const double tolerance = 10.0;
            return (Math.Abs(x - mousePosition.X) < tolerance) && (Math.Abs(y - mousePosition.Y) < tolerance);
        }

        #endregion

        #region Toolbar events

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                // uncheck all buttons in toolbar
                foreach (var item in toolbar.Items)
                {
                    if (item is ToggleButton)
                    {
                        var toggleButton = item as ToggleButton;
                        toggleButton!.IsChecked = false;
                    }
                }

                resetTools();
                switch (button.Name)
                {
                    case "brushButton":
                        selectedTool = ToolType.Brush;
                        paintSurface.Cursor = Cursors.Pen;
                        break;
                    case "pointButton":
                        selectedTool = ToolType.Point;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                    case "lineButton":
                        selectedTool = ToolType.Line;
                        paintSurface.Cursor = Cursors.Arrow;
                        break;
                    case "editLineButton":
                        selectedTool = ToolType.EditLine;
                        paintSurface.Cursor = Cursors.Arrow;
                        statusBarText.Text = "Możesz edytować linie podświetlone na niebiesko";
                        statusBar.Visibility = Visibility.Visible;
                        HighlightLines();
                        break;
                    case "polylineButton":
                        selectedTool = ToolType.Polyline;
                        paintSurface.Cursor = Cursors.Arrow;
                        statusBarText.Text = "Przytrzymaj CTRL aby dodać więcej punktów do lini łamanej";
                        statusBar.Visibility = Visibility.Visible;
                        break;
                    case "circleButton":
                        selectedTool = ToolType.Circle;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                    case "ellipseButton":
                        selectedTool = ToolType.Ellipse;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                    case "rectangleButton":
                        selectedTool = ToolType.Rectangle;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                    case "squareButton":
                        selectedTool = ToolType.Square;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                    case "hexagonButton":
                        selectedTool = ToolType.Hexagon;
                        paintSurface.Cursor = Cursors.Cross;
                        break;
                }
                button.IsChecked = true;
            }
        }

        #endregion

        #region Menu events
        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Plik painter (*pnt)|*.pnt";
                dialog.RestoreDirectory = true;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // Save file
                    using (var fileStream = File.Create(dialog.FileName))
                    {
                        XamlWriter.Save(paintSurface, fileStream);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Wystąpił błąd podczas zapisywania pliku", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadToCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "Plik painter (*pnt)|*.pnt";
                dialog.RestoreDirectory = true;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    Canvas savedCanvas;
                    using (var fileStream = File.OpenRead(dialog.FileName))
                    {
                        savedCanvas = (Canvas)XamlReader.Load(fileStream);
                    }
                    var parent = paintSurface.Parent as ScrollViewer;
                    if (parent != null)
                    {
                        parent.Content = savedCanvas;
                        paintSurface = savedCanvas;
                        canvasStraightLines.Clear();
                        attachCanvas(paintSurface);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Wystąpił błąd podczas odczytywania pliku", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCanvasToImg_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Plik PNG (*.png)|*.png|Plik JPG (*.jpg)|*.jpg|Plik GIF (*.gif)|*.gif|Plik BMP (*.bmp)|*.bmp|Plik TIFF (*.tiff)|*.tiff";
                dialog.RestoreDirectory = true;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // Prepare canvas for saving
                    paintSurface.Effect = null;
                    paintSurface.Measure(new Size((int)paintSurface.ActualWidth, (int)paintSurface.ActualHeight));
                    paintSurface.Arrange(new Rect(new Size((int)paintSurface.ActualWidth, (int)paintSurface.ActualHeight)));

                    // Create bitmap from canvas
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)paintSurface.ActualWidth, (int)paintSurface.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    VisualBrush sourceBrush = new VisualBrush(paintSurface);
                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    using (drawingContext)
                    {
                        drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(paintSurface.ActualWidth, paintSurface.ActualHeight)));
                    }
                    rtb.Render(drawingVisual);


                    // Encode to correct format
                    var extension = System.IO.Path.GetExtension(dialog.FileName);
                    BitmapEncoder encoder;
                    switch (extension.ToLower())
                    {
                        case ".png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case ".jpg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".gif":
                            encoder = new GifBitmapEncoder();
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        case ".tiff":
                            encoder = new TiffBitmapEncoder();
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    encoder.Frames.Add(BitmapFrame.Create(rtb));

                    // Save file
                    using (var fileStream = File.Create(dialog.FileName))
                    {
                        encoder.Save(fileStream);
                    }
                    paintSurface.Effect = dropShadow;
                }
            }
            catch
            {
                MessageBox.Show("Wystąpił błąd podczas zapisywania pliku", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImgToCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "Plik PNG (*.png)|*.png|Plik JPG (*.jpg)|*.jpg|Plik GIF (*.gif)|*.gif|Plik BMP (*.bmp)|*.bmp|Plik TIFF (*.tiff)|*.tiff";
                dialog.RestoreDirectory = true;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    BitmapImage image = new BitmapImage();
                    using (var fileStream = File.OpenRead(dialog.FileName))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = fileStream;

                        image.EndInit();
                    }
                    paintSurface.Width = image.Width;
                    paintSurface.Height = image.Height;
                    paintSurface.Children.Clear();
                    canvasStraightLines.Clear();
                    paintSurface.Background = new ImageBrush(image);
                }
            }
            catch
            {
                MessageBox.Show("Wystąpił błąd podczas odczytywania pliku", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            if (MessageBox.Show("Wyczyścić zawartość płótna?", "Wyczyść płótno", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                paintSurface.Children.Clear();
                paintSurface.Background = new SolidColorBrush(Colors.White);
            }
        }
        private void ResizeCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            var dialog = new ResizeCanvasDialog();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                if (MessageBox.Show("Zmiana rozmiaru płótna spowoduje wyczyszczenie go.\nCzy chcesz kontynuować?", "Zmiana rozmiaru płótna",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    // Clear and resize canvas
                    paintSurface.Children.Clear();
                    canvasStraightLines.Clear();
                    paintSurface.Background = new SolidColorBrush(Colors.White);
                    paintSurface.Width = dialog.CanvasWidth;
                    paintSurface.Height = dialog.CanvasHeight;
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            resetTools();
            MessageBox.Show("Painter - aplikacja do tworzenia grafiki\nProjekt zaliczeniowy z przedmiotu \"Grafika komputerowa i przetwarzanie obrazów\" 2024/2025\n" +
                "Autor: Patryk Gamrat", "O programie", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    #endregion
}