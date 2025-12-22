using KlarfReviewTool.Model;
using KlarfReviewTool.VIewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace KlarfReviewTool
{
    public partial class MainWindow : Window
    {

        private Point _startPoint;
        private Point _endPoint;
        private bool _isDragging;

        private const double DPI = 72;
        private const double MicrometersPerInch = 25.40;

        private string _sizeInfo;
        public string SizeInfo
        {
            get => _sizeInfo;
            set
            {
                _sizeInfo = value;
                OnPropertyChanged(nameof(SizeInfo));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WaferMap_Mousedown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(waferMap);

            int intXIndex = (int)(position.X / 22);
            int intYIndex = (int)(position.Y / 7);

            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.SelectDefect(intXIndex, intYIndex);
            }
        }
        public void ImageBox2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = e.GetPosition(MainCanvas);
                _isDragging = true;
                Debug.WriteLine("마우스1");
            }
            Debug.WriteLine("ImageBox2_MouseDown " + e.LeftButton);
        }

        public void ImageBox2_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                _endPoint = e.GetPosition(MainCanvas);
                UpdateSelectionRectangle();
            }
            // Debug.WriteLine(e.GetPosition(MainCanvas));
        }

        public void ImageBox2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("마우스");
            _isDragging = false;
            UpdateSelectedAreaInfo();
            SelectionRectangle.Visibility = Visibility.Collapsed;

            Debug.WriteLine("ImageBox2_MouseUp " + e.LeftButton);
        }

        private void UpdateSelectedAreaInfo()
        {
            double width = Math.Abs(_endPoint.X - _startPoint.X);
            double height = Math.Abs(_endPoint.Y - _startPoint.Y);

            double scaleX = imageScaleTransform.ScaleX;
            double scaleY = imageScaleTransform.ScaleY;

            double widthInMicrometers = (width / scaleX) * (MicrometersPerInch/DPI);
            double heightInMicrometers = (height / scaleY) * (MicrometersPerInch/DPI);

            SizeInfo = $"Size: {widthInMicrometers:F2} µm X {heightInMicrometers:F2} µm";

            MessageBox.Show(SizeInfo);
        }

        private void UpdateSelectionRectangle()
        {
            double x = Math.Min(_startPoint.X, _endPoint.X);
            double y = Math.Min(_startPoint.Y, _endPoint.Y);
            double width = Math.Abs(_endPoint.X - _startPoint.X);
            double height = Math.Abs(_endPoint.Y - _startPoint.Y);

            SelectionRectangle.SetValue(Canvas.LeftProperty, x);
            SelectionRectangle.SetValue(Canvas.TopProperty, y);
            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FolderItem selectedFolder)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectedFolder = selectedFolder;
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
