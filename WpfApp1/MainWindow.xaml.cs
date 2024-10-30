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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancelTokenSource;
        Genom genom_rez;
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource = new CancellationTokenSource();
            int count_population, count_city, lr, count_threads = 0;
            count_population = Convert.ToInt32(TextBox1.Text);
            count_city = Convert.ToInt32(TextBox2.Text);
            lr = Convert.ToInt32(TextBox3.Text);
            count_threads = Convert.ToInt32(TextBox4.Text);

            int[][] city_map_gen = GenMatrixCity(count_city);

            genom_rez = new Genom(count_city);
            PlotMC plot_cl = new PlotMC(count_city, genom_rez.cityNumberConections);
            plot.Model = plot_cl.plot;

            string string_value_data = "-----";
            PlotModel plot_v = new PlotModel { Title = "Graph" };
            plot_values.Model = plot_v;
            InfoTextBlock.Text = string_value_data;
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

            genom_rez.CalculateGenomWayLenght(WayLengMap);
            List<Population> list_population = new List<Population>();
            Action<Genom> callback = (genom_inp) =>
            {
                if (genom_inp.GenomScore < genom_rez.GenomScore)
                {
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


            Parallel.For(0, count_thread, (i, state) =>
            {
                Debug.WriteLine("START PODBOR");
                Population tm = new Population(count_population, WayLengMap, lr, i);
                list_population.Add(tm);
                tm.StartPopulationEvolution(callback, cancelTokenSource.Token);
            });
            Console.WriteLine("FIN");
            Console.WriteLine(genom_rez.GenomScore);
            return genom_rez;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
            MessageBox.Show($"Fin rezult: {genom_rez.GenomScore}\n" +
                $"Genome way:\n" +
                $"{genom_rez.ToString()}");
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

    }

}