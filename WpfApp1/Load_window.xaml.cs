using lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static WpfApp1.Save_window;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Load_window.xaml
    /// </summary>
    public partial class Load_window : Window
    {
        private List<ExperimentData> experiments;
        public Load_window()
        {
            InitializeComponent();
            LoadExperiments();
        }

        private void LoadExperiments()
        {
            // Получаем путь к исполняемому файлу
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            // Поднимаемся на несколько уровней вверх, чтобы получить путь к корневой папке проекта
            string projectRootPath = Directory.GetParent(executablePath).Parent.Parent.Parent.FullName;

            // Путь к папке data в корне проекта
            string dataFolderPath = System.IO.Path.Combine(projectRootPath, "data");

            // Путь к JSON файлу для сохранения имен экспериментов
            string experimentsFilePath = System.IO.Path.Combine(dataFolderPath, "experiments.json");

            // Чтение существующих данных из файла
            string jsonData = File.Exists(experimentsFilePath) ? File.ReadAllText(experimentsFilePath) : "[]";
            experiments = JsonConvert.DeserializeObject<List<ExperimentData>>(jsonData) ?? new List<ExperimentData>();

            // Заполняем ListBox экспериментами
            ExperimentListBox.ItemsSource = experiments;
            ExperimentListBox.DisplayMemberPath = "Name";
        }

        private void ExperimentListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ExperimentListBox.SelectedItem is ExperimentData selectedExperiment)
            {
                CountPopulationTextBox.Text = selectedExperiment.CountPopulation.ToString();
                CountCityTextBox.Text = selectedExperiment.CountCity.ToString();
                LearningRateTextBox.Text = selectedExperiment.LearningRate.ToString();
                CountThreadsTextBox.Text = selectedExperiment.CountThreads.ToString();
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExperimentListBox.SelectedItem is ExperimentData selectedExperiment)
            {
                // Получаем путь к исполняемому файлу
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;

                // Поднимаемся на несколько уровней вверх, чтобы получить путь к корневой папке проекта
                string projectRootPath = Directory.GetParent(executablePath).Parent.Parent.Parent.FullName;

                // Путь к папке data в корне проекта
                string dataFolderPath = System.IO.Path.Combine(projectRootPath, "data");

                // Загружаем параметры эксперимента
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.CountPopulation = selectedExperiment.CountPopulation;
                mainWindow.CountCity = selectedExperiment.CountCity;
                mainWindow.Lr = selectedExperiment.LearningRate;
                mainWindow.CountThreads = selectedExperiment.CountThreads;
                mainWindow.city_map_gen = selectedExperiment.CityMapGen;

                // Загружаем популяции
                mainWindow.list_population = new List<Population>();
                foreach (var populationFile in selectedExperiment.PopulationFiles)
                {
                    string populationFilePath = System.IO.Path.Combine(dataFolderPath, populationFile);
                    if (File.Exists(populationFilePath))
                    {
                        string populationJsonData = File.ReadAllText(populationFilePath);
                        var population = JsonConvert.DeserializeObject<Population>(populationJsonData);
                        mainWindow.list_population.Add(population);
                    }
                }

                MessageBox.Show("Experiment loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
    public class ExperimentData
    {
        public string Name { get; set; }
        public int CountPopulation { get; set; }
        public int CountCity { get; set; }
        public int LearningRate { get; set; }
        public int CountThreads { get; set; }
        public int[][] CityMapGen { get; set; }
        public List<string> PopulationFiles { get; set; }
    }
}
