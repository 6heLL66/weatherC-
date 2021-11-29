using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FontAwesome.WPF;
using SharpVectors.Converters;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers;

namespace WpfApp2_test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        private string apiKey = "33178d46dea4c98a92d98aa6ea4ebc24";
        private string country = "";
        DateTime date;
        private List<string> cities = new List<string>();
        private ImageAwesome Loader = new ImageAwesome();
        TextBlock errorBlock = new TextBlock();

        public MainWindow()
        {   
            SetCities();
            InitializeComponent();
            Result.Children.Add(errorBlock);
        }

        private void StartLoading()
        {
            SearchButton.IsEnabled = false;
            Loader.Icon =  FontAwesomeIcon.Spinner;
            Loader.Spin = true;
            Loader.Height = 40;
            Loader.Width = 40;
            Result.Children.Add(Loader);
        }

        private void SetCities()
        {
            string text = System.IO.File.ReadAllText("./city.list.json");
            var jsonResponse = JsonDocument.Parse(text);
            var enumerator = jsonResponse.RootElement.EnumerateArray();
            foreach (var val in enumerator)
            {
                string name = val.GetProperty("name").ToString();
                cities.Add(name);
            }
        }
        
        private void EndLoading()
        {
            SearchButton.IsEnabled = true;
            Result.Children.Remove(Loader);
        }

        private async void Search_Button_Click(object sender, RoutedEventArgs e)
        {   
            StartLoading();
            SelectedCity.Text = SearchInput.Text;
            HintsList.RowDefinitions.Clear();
            HintsList.Children.Clear();
            HintsContainer.Visibility = Visibility.Hidden;
            errorBlock.Visibility = Visibility.Hidden;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(
                $"http://api.openweathermap.org/data/2.5/weather?q={SelectedCity.Text}&units=metric&appid={apiKey}"
            );
            var responseString = await response.Content.ReadAsStringAsync();
            var resultJson = JsonDocument.Parse(responseString);
            if (resultJson.RootElement.GetProperty("cod").ToString() == "200")
            {   
                EndLoading();
                Debug.WriteLine("\n \n \n \n");
                Debug.WriteLine(resultJson.RootElement.ToString());
                date = new DateTime(1970, 1, 1, 0, 0, 0);
                date = date.AddSeconds(resultJson.RootElement.GetProperty("dt").GetInt32());
                country = resultJson.RootElement.GetProperty("sys").GetProperty("country").ToString();
                DrawWeather(resultJson.RootElement.GetProperty("weather"), resultJson.RootElement.GetProperty("main"), resultJson.RootElement.GetProperty("wind"));
                return;
            }
            errorBlock.Visibility = Visibility.Visible;
            errorBlock.Text = "Error: " + resultJson.RootElement.GetProperty("message");
            errorBlock.FontSize = 36;
            errorBlock.Foreground = Brushes.Red;
            EndLoading();
            Weather.Children.Clear();
            WeatherInfo.Children.Clear();
        }

        private void DrawWeather(JsonElement weather, JsonElement main, JsonElement wind)
        {   
            WeatherInfo.Children.Clear();
            TextBlock cityName = new TextBlock();
            TextBlock dateBlock = new TextBlock();
            dateBlock.Text = date.ToString();
            dateBlock.SetValue(Grid.RowProperty, 1);
            cityName.SetValue(Grid.RowProperty, 0);
            cityName.Text = $"{SelectedCity.Text}, {country}";
            cityName.FontSize = 28;
            dateBlock.FontSize = 18;
            dateBlock.Margin = new Thickness(20, 0, 0, 15);
            cityName.Margin = new Thickness(20, 20, 20, 5);
            cityName.FontWeight = FontWeights.Bold;
            WeatherInfo.Children.Add(cityName);
            WeatherInfo.Children.Add(dateBlock);
            Weather.Children.Clear();
            WeatherParams.Children.Clear();
            
            TextBlock description = new TextBlock();
            description.Text = weather[0].GetProperty("description").ToString();
            description.FontSize = 26;
            description.FontWeight = FontWeights.Bold;
            description.Margin = new Thickness(220, 0, 0, 0);
            description.SetValue(Grid.ColumnProperty, 1);
            Weather.Children.Add(description);
            
            TextBlock pressure = new TextBlock();
            Grid pressureGrid = new Grid();
            SvgViewbox pressureIcon = new SvgViewbox();
            pressureGrid.ColumnDefinitions.Add(new ColumnDefinition());
            pressureGrid.ColumnDefinitions.Add(new ColumnDefinition());
            pressureIcon.Source = new Uri("/images/Pressure.svg", UriKind.Relative);
            pressureIcon.Width = 25;
            pressureIcon.Height = 25;
            pressureIcon.Margin = new Thickness(0, 0, 0, 5);
            pressureIcon.SetValue(Grid.ColumnProperty, 0);
            pressure.Text = main.GetProperty("pressure") + " hPa";
            pressure.SetValue(Grid.RowProperty, 0);
            pressure.FontSize = 19;
            pressure.SetValue(Grid.ColumnProperty, 1);
            pressureGrid.Children.Add(pressureIcon);
            pressureGrid.Children.Add(pressure);
            WeatherParams.Children.Add(pressureGrid);
            
            TextBlock humidity = new TextBlock();
            Grid humidityGrid = new Grid();
            SvgViewbox humidityIcon = new SvgViewbox();
            humidityGrid.ColumnDefinitions.Add(new ColumnDefinition());
            humidityGrid.ColumnDefinitions.Add(new ColumnDefinition());
            humidityIcon.Source = new Uri("/images/Humidity.svg", UriKind.Relative);
            humidityIcon.Width = 25;
            humidityIcon.Height = 25;
            humidityIcon.Margin = new Thickness(0, 0, 0, 5);
            humidityIcon.SetValue(Grid.ColumnProperty, 0);
            humidity.Text = main.GetProperty("humidity") + " %";
            humidity.FontSize = 19;
            humidityGrid.SetValue(Grid.RowProperty, 1);
            humidity.SetValue(Grid.ColumnProperty, 1);
            humidityGrid.Children.Add(humidityIcon);
            humidityGrid.Children.Add(humidity);
            WeatherParams.Children.Add(humidityGrid);
            
            TextBlock speed = new TextBlock();
            Grid speedGrid = new Grid();
            SvgViewbox speedIcon = new SvgViewbox();
            speedGrid.ColumnDefinitions.Add(new ColumnDefinition());
            speedGrid.ColumnDefinitions.Add(new ColumnDefinition());
            speedIcon.Source = new Uri("/images/Wind.svg", UriKind.Relative);
            speedIcon.Width = 25;
            speedIcon.Height = 25;
            speedIcon.Margin = new Thickness(0, 0, 0, 5);
            speedIcon.SetValue(Grid.ColumnProperty, 0);
            speed.Text = wind.GetProperty("speed") + " mps";
            speed.FontSize = 19;
            speedGrid.SetValue(Grid.RowProperty, 2);
            speed.SetValue(Grid.ColumnProperty, 1);
            speedGrid.Children.Add(speedIcon);
            speedGrid.Children.Add(speed);
            WeatherParams.Children.Add(speedGrid);
            
            SvgViewbox weatherIcon = new SvgViewbox();
            TextBlock temp = new TextBlock();
            temp.Text = main.GetProperty("temp").ToString() + '°';
            temp.FontSize = 65;
            temp.FontWeight = FontWeights.Medium;
            weatherIcon.Source = new Uri($"/images/{weather[0].GetProperty("main").ToString()}.svg", UriKind.Relative);
            weatherIcon.Width = 100;
            weatherIcon.Height = 100;
            weatherIcon.SetValue(Grid.ColumnProperty, 0);
            temp.SetValue(Grid.ColumnProperty, 1);
            Weather.Children.Add(WeatherParams);
            Weather.Children.Add(weatherIcon);
            Weather.Children.Add(temp);
        }

        private List<string> GetHints(string text)
        {
            List<string> names = new List<string>();

            for (var i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                if (text.Length <= city.Length && text.ToLower() == city.Substring(0, text.Length).ToLower())
                {
                    names.Add(city);
                }

                if (names.Count >= 5) return names;
            }
            
            return names;
        }

        private void Input_Text_Change(object sender, RoutedEventArgs e)
        {
            if (SearchInput.Text.Length > 2)
            {   
                HintsList.Children.Clear();
                HintsList.RowDefinitions.Clear();
                List<string> hints = GetHints(SearchInput.Text);
                
                if (hints.Count > 0)
                {
                    int number = 0;
                    HintsContainer.Visibility = Visibility.Visible;
                    foreach (var hint in hints)
                    {
                        Button hintBlock = new Button();
                        RowDefinition rowDef = new RowDefinition();
                        hintBlock.Content = hint;
                        hintBlock.Height = 44;
                        hintBlock.Click += (object s, RoutedEventArgs el) =>
                        {
                            SearchInput.Text = hint;
                            HintsList.RowDefinitions.Clear();
                            HintsList.Children.Clear();
                            HintsContainer.Visibility = Visibility.Hidden;
                            SearchButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        };
                        hintBlock.Foreground = Brushes.Black;
                        hintBlock.Background = Brushes.White;
                        hintBlock.Margin = new Thickness(0, 10, 0, 0);
                        hintBlock.BorderThickness = new Thickness(0, 0, 0, 2);
                        hintBlock.FontSize = 18;
                        hintBlock.SetValue(Grid.RowProperty, number);
                        HintsList.RowDefinitions.Add(rowDef);
                        HintsList.Children.Add(hintBlock);
                        number++;
                    }
                }
                else
                {
                    HintsContainer.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                HintsContainer.Visibility = Visibility.Hidden;
            }
        }
    }
}