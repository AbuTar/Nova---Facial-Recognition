using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Tensorflow;
using NumSharp;
using Path = System.IO.Path;



namespace Nova
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture? _capture = null;
        private DispatcherTimer? _timer = null;
        private System.Drawing.Bitmap? _temporaryImageCapture = null;

        public MainWindow()
        {
            InitializeComponent();
            LoadCameraDevices();
            App.CurrentWindow = this;
        }

        private void LoadCameraDevices()
        {
            var cameraDevices = GetAvailableCameraIndexes();
            foreach (var cameraIndex in cameraDevices)
            {
                cameraComboBox.Items.Add(cameraIndex.ToString());
            }

            if (cameraDevices.Count > 0)
            {
                cameraComboBox.SelectedIndex = 0;
            }
        }

        private List<int> GetAvailableCameraIndexes()
        {
            var availableCameras = new List<int>();
            for (int index = 0; index < 10; index++)
            {
                try
                {
                    using (var capture = new VideoCapture(index))
                    {
                        if (capture.IsOpened)
                        {
                            availableCameras.Add(index);
                            capture.Stop();
                        }
                    }
                }
                catch
                {
                }
            }

            return availableCameras;
        }

        private void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraComboBox.SelectedItem == null)
                return;

            int selectedCameraIndex = int.Parse(cameraComboBox.SelectedItem.ToString());
            StartCamera(selectedCameraIndex);
        }

        private void StartCamera(int cameraIndex)
        {
            _capture?.Dispose();

            _capture = new VideoCapture(cameraIndex);
            if (!_capture.IsOpened)
            {
                MessageBox.Show("Camera not found.");
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16.6); // Do whatever you want to adjust framerate
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_capture == null)
                return;

            using (var frame = _capture.QueryFrame())
            {
                if (frame != null)
                {
                    var bitmap = frame.ToImage<Bgr, byte>().ToBitmap();
                    videoImage.Source = BitmapToImageSource(bitmap);
                }
            }
        }

        private BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmap.Width, bitmap.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24,
                null,
                bitmapData.Scan0,
                bitmapData.Stride * bitmap.Height,
                bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _capture?.Dispose();
            _timer?.Stop();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimise_Click(object sender, RoutedEventArgs e)
        {
            bool isMaximized = (this.WindowState == WindowState.Maximized);
            if (isMaximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MinimizeWindow(this);
            }

        }

        private void Maximise_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CaptureImage_Click(object sender, RoutedEventArgs e)
        {
            if (_capture == null)
            {
                MessageBox.Show("Camera is not running. Please select a camera first.");
                return;
            }

            using (var frame = _capture.QueryFrame())
            {
                if (frame != null)
                {
                    _temporaryImageCapture = frame.ToImage<Bgr, byte>().ToBitmap();
                    var bitmap = frame.ToImage<Bgr, byte>().ToBitmap();
                    capturedImage.Source = BitmapToImageSource(bitmap);

                }
                else
                {
                    MessageBox.Show("Unable to capture frame.");
                }
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_temporaryImageCapture == null)
            {
                MessageBox.Show("No image captured. Please capture an image first.");
                return;
            }

            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string folderPath = Path.Combine(projectDirectory, "Resources");
            string filePath = Path.Combine(folderPath, $"CapturedImage.png");
            _temporaryImageCapture.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            MessageBox.Show($"Image saved successfully at {filePath}");

            ImageToPython();




            
        }

        private static void ImageToPython()
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    string pythonPath = Path.Combine(projectDirectory, "Resources", "predict.py");
                    string ExePath = Path.Combine(projectDirectory, "Resources", "dist", "predict.exe");
                    myProcess.StartInfo.UseShellExecute = true;

                    //Filename should be .exe file:
                    myProcess.StartInfo.FileName = ExePath;
                    //Argument should be the path of the .py file :
                    myProcess.StartInfo.Arguments = pythonPath;
                    myProcess.StartInfo.CreateNoWindow = false;
                    myProcess.Start();
                    // This code assumes the process you are starting will terminate itself.
                    // Given that it is started without a window I cannot terminate it
                    // on the desktop, it must terminate itself or using kill method from within WPF
                    myProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
         
            LogInTacker();
        }

        private static void LogInTacker()
        {
            // Load the txt file 
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string predictionPath = Path.Combine(projectDirectory, "Resources", "prediction.txt");

            // Split the part of the string following the '_'
            string predicted_user = File.ReadLines(predictionPath).First();
            string[] parts = predicted_user.Split('_');
            string user = parts.Length > 1 ? parts[1] : "";

            // Load login_records txt file
            string recordPath = Path.Combine(projectDirectory, "Resources", "login_records.txt");

            // Determine Date and Time
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {user}";

            File.AppendAllText(recordPath, Environment.NewLine + logEntry + Environment.NewLine);

            MessageBox.Show($"{user} has been identified");

            SwapToLoggedInWindow();

        }
        
        private static void SwapToLoggedInWindow()
        {
            // Create a new instance of MainWindow
            LoggedInWindow loggedinwindow = new LoggedInWindow();

            // Show the new window
            loggedinwindow.Show();

            // Close the current StartupMenuView's parent window
            App.CurrentWindow?.Close();
        }
        



        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }

       


    }

}