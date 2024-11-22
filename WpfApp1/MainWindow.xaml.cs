using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
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
using lib;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
namespace WpfApp1
{
        public partial class MainWindow : Window, INotifyPropertyChanged
    {
        CancellationTokenSource cancelTokenSource;
        Genom genom_rez;
        public List<Population> list_population = new List<Population>();
        private PlotMC plot_cl = null;
        private bool runing = false;

        private int count_population;
        private int count_city;
        private int lr;
        private int count_threads;
        private bool dataChanged;

        public int CountPopulation
        {
            get { return count_population; }
            set { count_population = value; OnPropertyChanged(nameof(CountPopulation)); dataChanged = true; }
        }

        public int CountCity
        {
            get { return count_city; }
            set { count_city = value; OnPropertyChanged(nameof(CountCity)); dataChanged = true; }
        }

        public int Lr
        {
            get { return lr; }
            set { lr = value; OnPropertyChanged(nameof(Lr)); dataChanged = true; }
        }

        public int CountThreads
        {
            get { return count_threads; }
            set { count_threads = value; OnPropertyChanged(nameof(CountThreads)); dataChanged = true; }
        }
        public int[][] city_map_gen = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Save.IsEnabled = false;
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
            cancelTokenSource = new CancellationTokenSource();
            string string_value_data = "-----";
            city_map_gen = GenMatrixCity(count_city);
            if(dataChanged)
            {
                genom_rez = new Genom(count_city);
                plot_cl = new PlotMC(count_city, genom_rez.cityNumberConections);
                plot.Model = plot_cl.plot;
                InfoTextBlock.Text = string_value_data;
            }

            PlotModel plot_v = new PlotModel { Title = "Graph" };
            plot_values.Model = plot_v;
            Save.IsEnabled = true;
            Task.Run(async () =>
            {
                RunMulti(count_threads, count_population, city_map_gen, lr, plot_cl, plot_v);
            }, cancelTokenSource.Token);

        }
        public Genom RunMulti(int count_thread, int count_population, int[][] WayLengMap, int lr, PlotMC plot, PlotModel plot_v)
        {
            plot_v.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Time" });
            plot_v.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Value" });

            List<DataPoint> dataPoints = new List<DataPoint>();
            dataPoints.Add(new DataPoint(0, 0));
            LineSeries lineSeries = new LineSeries();

            plot_v.Series.Add(lineSeries);
            int i = 0;
            runing = true;
            Action<Genom> callback = (genom_inp) =>
            {
                if (genom_inp.GenomScore < genom_rez.GenomScore)
                {
                    Debug.WriteLine($"NEW GENOM {genom_inp.GenomScore}, WAS _ {genom_rez.GenomScore}");
                    genom_rez = genom_inp.ClonePopulation();
                    plot.PlotNewDrow(genom_inp.cityNumberConections);
                    dataPoints.Add(new DataPoint(i, genom_rez.GenomScore));
                    lineSeries.Points.Clear();
                    lineSeries.Points.AddRange(dataPoints);
                    plot_v.InvalidatePlot(true);
                    Dispatcher.Invoke(() =>
                    {
                        InfoTextBlock.Text = $"{genom_rez.GenomScore}";
                    });
                    i++;
                }
            };

            if (dataChanged)
            {
                genom_rez.CalculateGenomWayLenght(WayLengMap);
                list_population.Clear();
                Parallel.For(0, count_thread, (i, state) =>
                {
                    Debug.WriteLine("START PODBOR");
                    Population tm = new Population(count_population, WayLengMap, lr, i);
                    list_population.Add(tm);
                    tm.StartPopulationEvolution(callback, cancelTokenSource.Token);
                });
            }
            else
            {

                Debug.WriteLine($"Chage data = {dataChanged}");
                Parallel.For(0, count_thread, (i, state) =>
                {
                    list_population[i].StartPopulationEvolution(callback, cancelTokenSource.Token);
                });
            }
            Console.WriteLine("FIN");
            Console.WriteLine(genom_rez.GenomScore);
            return genom_rez;
        }

        private void Resume_all(object sender, RoutedEventArgs e)
        {
            foreach (var population in list_population)
            {
                population.Resume();
            }
        }
        private void Pause_all(object sender, RoutedEventArgs e)
        {
            foreach (var population in list_population)
            {
                population.Pause();
            }
        }
        private void Load_json(object sender, RoutedEventArgs e)
        {
            StopButton_Click(sender, e);
            Load_window loadWindow = new Load_window();
            loadWindow.ShowDialog();
            dataChanged = false;
            
            InfoTextBlock.Text = list_population.Min(p => p.resultsolution).ToString();
            genom_rez = list_population.OrderBy(p => p.resultsolution).FirstOrDefault()?.bestGen.ClonePopulation();
            plot_cl = new PlotMC(count_city, genom_rez.cityNumberConections);
            plot.Model = plot_cl.plot;
        }
        private void Save_json(object sender, RoutedEventArgs e)
        {
            Pause_all(sender, e);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            Save_window saveWindow = new Save_window(list_population, count_population, count_city, lr, count_threads, city_map_gen);
            bool? result = saveWindow.ShowDialog();
            if (result == true)
            {
                MessageBox.Show("Saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Resume_all(sender, e);
            }
            else
            {
                MessageBox.Show("Saving was canceled. Program stoped, ypu can resume it.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (runing)
            {
                SubmitButton.IsEnabled = true;
                cancelTokenSource.Cancel();
                MessageBox.Show($"Fin rezult: {genom_rez.GenomScore}\n" +
                    $"Genome way:\n" +
                    $"{genom_rez.ToString()}");
            }
        }

        public static int[][] GenMatrixCity(int size)
        {
            Random random = new Random();
            int[][] matrix = new int[size][];
            for (int i = 0; i < size; i++)
            {
                matrix[i] = new int[size];
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    int distance = random.Next(1, 100);
                    matrix[i][j] = distance;
                    matrix[j][i] = distance;
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    for (int k = j + 1; k < size; k++)
                    {
                        if (matrix[i][j] + matrix[j][k] < matrix[i][k])
                        {
                            matrix[i][k] = matrix[i][j] + matrix[j][k] + 1;
                            matrix[k][i] = matrix[i][k];
                        }
                    }
                }
            }

            return matrix;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}