using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace MyWeather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string apiKey = "67ba5ab753b84f85b07141755230405"; // Replace this with your Weather API key
        private string cityName;

        private DateTime _now;
        public DateTime Now
        {
            get { return _now; }
            set
            {
                _now = value;
                OnPropertyChanged(nameof(Now));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            lblDigitalClock.Visibility = Visibility.Hidden;

            // memperbarui waktu setiap detik
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (sender, args) =>
            {
                // memperbarui properti Now
                Now = DateTime.Now;
            };
            timer.Start();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void btnGetWeather_Click(object sender, RoutedEventArgs e)
        {
            cityName = txtCityName.Text.Trim();
            if (string.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("Please enter a city name!");
                return;
            }

            string apiUrl = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityName}";

            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(apiUrl);
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponse);
                            DisplayWeatherData(weatherData);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("An error occurred while fetching weather data: " + ex.Message);
            }

            lblDigitalClock.Visibility = Visibility.Visible;
            txtCityName.Text = "";
        }

        private void DisplayWeatherData(WeatherData weatherData)
        {
            lblCityName.Content = weatherData.Location.Name;
            lblTemperature.Content = weatherData.Current.TempC + "°C";
            lblCondition.Content = weatherData.Current.Condition.Text;
            lblHumidity.Content = weatherData.Current.Humidity + "%";

            BitmapImage weatherIcon = new BitmapImage();
            weatherIcon.BeginInit();
            weatherIcon.UriSource = new Uri("http:" + weatherData.Current.Condition.Icon);
            weatherIcon.EndInit();
            imgWeatherIcon.Source = weatherIcon;

            lblWindSpeed.Content = weatherData.Current.WindKph + " km/h";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
