using System;
using System.Collections.Generic;
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
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using lib;
using OxyPlot;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Save_window.xaml
    /// </summary>
    public partial class Save_window : Window
    {
        private List<Population> list_population;
        private int countPopulation;
        private int countCity;
        private int learningRate;
        private int countThreads;
        private int[][] cityMapGen;

        public Save_window(List<Population> populationList, int countPopulation, int countCity, int learningRate, int countThreads, int[][] cityMapGen)
        {
            InitializeComponent();
            this.list_population = populationList;
            this.countPopulation = countPopulation;
            this.countCity = countCity;
            this.learningRate = learningRate;
            this.countThreads = countThreads;
            this.cityMapGen = cityMapGen;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string experimentName = ExperimentNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(experimentName))
            {
                MessageBox.Show("Please enter a valid experiment name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем путь к исполняемому файлу
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            // Поднимаемся на несколько уровней вверх, чтобы получить путь к корневой папке проекта
            string projectRootPath = Directory.GetParent(executablePath).Parent.Parent.Parent.FullName;

            // Путь к папке data в корне проекта
            string dataFolderPath = System.IO.Path.Combine(projectRootPath, "data");

            // Убедитесь, что папка существует
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }

            // Путь к JSON файлу для сохранения имен экспериментов
            string experimentsFilePath = System.IO.Path.Combine(dataFolderPath, "experiments.json");

            // Проверка на существование файла
            if (!File.Exists(experimentsFilePath))
            {
                // Создаем файл с пустым содержимым
                File.WriteAllText(experimentsFilePath, "[]");
            }

            // Чтение существующих данных из файла
            string jsonData = File.ReadAllText(experimentsFilePath);
            var experiments = JsonConvert.DeserializeObject<List<ExperimentData>>(jsonData) ?? new List<ExperimentData>();

            // Проверка на уникальность имени эксперимента
            if (experiments.Exists(exp => exp.Name == experimentName))
            {
                MessageBox.Show("An experiment with this name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаем копию основного файла перед сохранением
            string backupFilePath = System.IO.Path.Combine(dataFolderPath, "experiments_backup.json");
            File.Copy(experimentsFilePath, backupFilePath, true);

            // Добавление нового эксперимента
            var newExperiment = new ExperimentData
            {
                Name = experimentName,
                CountPopulation = countPopulation,
                CountCity = countCity,
                LearningRate = learningRate,
                CountThreads = countThreads,
                CityMapGen = cityMapGen,
                PopulationFiles = new List<string>()
            };

            // Сохраняем параметры популяций
            for (int i = 0; i < list_population.Count; i++)
            {
                string populationFilePath = System.IO.Path.Combine(dataFolderPath, $"{experimentName}_population_{i}.json");
                string populationJsonData = JsonConvert.SerializeObject(list_population[i], Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(populationFilePath, populationJsonData);
                newExperiment.PopulationFiles.Add(populationFilePath);
            }

            // Добавляем новый эксперимент в список
            experiments.Add(newExperiment);

            // Сохранение данных в файл
            File.WriteAllText(experimentsFilePath, JsonConvert.SerializeObject(experiments, Newtonsoft.Json.Formatting.Indented));

            // Удаляем копию основного файла после сохранения
            File.Delete(backupFilePath);

            MessageBox.Show("Experiment data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true; // Устанавливаем DialogResult в true, чтобы закрыть окно
            this.Close();
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
}
