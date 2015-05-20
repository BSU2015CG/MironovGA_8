using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AForge.Imaging;
using Point = System.Windows.Point;

namespace TrompeLeCode.HistogramSample
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                ImageURL =
                    new Uri(Path.Combine(Environment.CurrentDirectory, "Sample.jpg"), UriKind.Absolute).AbsoluteUri;
            }
            catch
            {
                // do nothing, user must enter a URL manualy
            }
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Methods

        private PointCollection ConvertToPointCollection(int[] values)
        {
            var max = values.Max();


            var points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (var i = 0; i < values.Length; i++)
            {
                points.Add(new Point(i, max - values[i]));
            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));

            return points;
        }

        #endregion

        #region Private variables

        private string localImagePath;
        private PointCollection redColorHistogramPoints;
        private PointCollection greenColorHistogramPoints;
        private PointCollection blueColorHistogramPoints;

        #endregion

        #region Public Properties

        public string ImageURL { get; set; }

        public string LocalImagePath
        {
            get { return localImagePath; }
            set
            {
                if (localImagePath != value)
                {
                    localImagePath = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LocalImagePath"));
                    }
                }
            }
        }

        public bool PerformHistogramSmoothing { get; set; }

        public PointCollection RedColorHistogramPoints
        {
            get { return redColorHistogramPoints; }
            set
            {
                if (redColorHistogramPoints != value)
                {
                    redColorHistogramPoints = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("RedColorHistogramPoints"));
                    }
                }
            }
        }

        public PointCollection GreenColorHistogramPoints
        {
            get { return greenColorHistogramPoints; }
            set
            {
                if (greenColorHistogramPoints != value)
                {
                    greenColorHistogramPoints = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("GreenColorHistogramPoints"));
                    }
                }
            }
        }

        public PointCollection BlueColorHistogramPoints
        {
            get { return blueColorHistogramPoints; }
            set
            {
                if (blueColorHistogramPoints != value)
                {
                    blueColorHistogramPoints = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BlueColorHistogramPoints"));
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ImageURL))
            {
                OnButtonClick(null, null);
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            try
            {
                if (string.IsNullOrWhiteSpace(ImageURL))
                {
                    MessageBox.Show("Image URL is mandatory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string localFilePath = null;
                try
                {
                    localFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(ImageURL, localFilePath);
                    }
                    LocalImagePath = localFilePath;
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid image URL. Image could not be retrieved", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                using (var bmp = new Bitmap(localFilePath))
                {
                    // RGB
                    var rgbStatistics = new ImageStatistics(bmp);

                    RedColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Red.Values);
                    GreenColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Green.Values);
                    BlueColorHistogramPoints = ConvertToPointCollection(rgbStatistics.BlueWithoutBlack.Values);

                    double redAverage = 0.0 , greenAverage = 0.0, blueAverage = 0.0;
                    for (int i = 0; i < rgbStatistics.Red.Values.Length; ++i)
                    {
                        redAverage += rgbStatistics.Red.Values[i]*i;
                    }
                    for (int i = 0; i < rgbStatistics.Green.Values.Length; ++i)
                    {
                        greenAverage += rgbStatistics.Green.Values[i] * i;
                    }
                    for (int i = 0; i < rgbStatistics.Blue.Values.Length; ++i)
                    {
                        blueAverage += rgbStatistics.BlueWithoutBlack.Values[i] * i;
                    }
                    RedLabel.Content = redAverage / rgbStatistics.Red.Values.Sum();
                    GreenLabel.Content = greenAverage / rgbStatistics.Green.Values.Sum();
                    BlueLabel.Content = blueAverage / rgbStatistics.Blue.Values.Sum();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating histogram: " + ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var source = sender as Hyperlink;
            if (source != null)
            {
                Process.Start(source.NavigateUri.ToString());
            }
        }

        #endregion
    }
}