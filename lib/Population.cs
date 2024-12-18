﻿using System.Diagnostics;
using System.Windows;
namespace lib;
using Newtonsoft.Json;

public class Population
{
    private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
    private static object lockObject = new object();
    public int numEpoch { get; set; }
    public Genom[] genArray { get; set; }
    public Genom bestGen { get; set; }
    public List<int> genomsresult { get; set; }
    public int[][] WayLengMap { get; set; }
    public int resultsolution = -1;
    public int learningRate = 0;
    public int numPopulation;
<<<<<<< HEAD
=======
    public bool loop = false;
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f

    /// <summary>
    /// Эта функция создает популяию
    /// </summary>
    /// <param name="count_population">Количество генов в популяции</param>
    /// <param name="cityMap">Матрица расстояний между городами</param>
    /// <param name="lr">максимальный кол-во мутаций</param>
    public Population(int count_population, int[][] cityMap, int lr, int numPopulation = 1)
    {
        learningRate = lr;
        genomsresult = new List<int>();
        numEpoch = 1;
        genArray = new Genom[count_population];
        for (int i = 0; i < count_population; i++)
        {
            genArray[i] = new Genom(cityMap.Length);
        }


        WayLengMap = new int[cityMap.Length][];
        for (int i = 0; i != cityMap.Length; i++)
        {
            WayLengMap[i] = new int[cityMap.Length];
            WayLengMap[i] = (int[])cityMap[i].Clone();
        }
        bestGen = genArray[this.CalculateSolutionsLenght()];
        this.numPopulation = numPopulation;
<<<<<<< HEAD
=======
    }

    [JsonConstructor]
    public Population(int numEpoch, Genom[] genArray, Genom bestGen, List<int> genomsresult, int[][] WayLengMap, int resultsolution, int learningRate, int numPopulation, bool loop)
    {
        this.numEpoch = numEpoch;
        this.genArray = genArray;
        this.bestGen = bestGen;
        this.genomsresult = genomsresult;
        this.WayLengMap = WayLengMap;
        this.resultsolution = resultsolution;
        this.learningRate = learningRate;
        this.numPopulation = numPopulation;
        this.loop = loop;
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f
    }

    public int CalculateSolutionsLenght()
    {
        genomsresult.Clear();
        foreach (Genom gen in genArray)
        {
            genomsresult.Add(gen.CalculateGenomWayLenght(WayLengMap));
        }

        return genomsresult.IndexOf(genomsresult.Min());
    }


    public int GenNewEpoch()
    {
        Random random = new Random();
        int mutatitionLr = random.Next(0, learningRate);
        int deadGenomLr = random.Next(0, genArray.Length - mutatitionLr);

        for (int i = 0; i < deadGenomLr; i++)
        {
            this.CreateNewGenom();
        }
        for (int i = 0; i < mutatitionLr; i++)
        {
            this.MutateRandomGen();
        }
<<<<<<< HEAD
        this.CalculateSolutionsLenght();
=======
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f
        numEpoch += 1;
        return numEpoch;

    }

    public string GetEpochResult()
    {
        return $"population number {numPopulation}: Epoch: {numEpoch}, best result is: {this.resultsolution} at genom\n {this.bestGen}\n";
    }

<<<<<<< HEAD
    public void StartPopulationEvolution(ref Genom outputGenom)
    {
        this.CalculateSolutionsLenght();
        resultsolution = this.genomsresult.Min();
        bool loop = false;
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            loop = true;
        };
=======
    public void StartPopulationEvolution(Action<Genom> callback, CancellationToken token)
    {

        Debug.WriteLine($"start {numPopulation}");
        int ind_best_gen = this.CalculateSolutionsLenght();
        resultsolution = this.genomsresult.Min();
        bestGen = genArray[ind_best_gen].ClonePopulation();
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f
        Console.WriteLine(this.GetEpochResult());
        Debug.WriteLine($"work full start {numPopulation}");
        while (true)
        {
            pauseEvent.Wait();
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("DEAD");
                break;
            }

            this.GenNewEpoch();
<<<<<<< HEAD
            if(resultsolution > this.genomsresult.Min())
=======
            ind_best_gen = this.CalculateSolutionsLenght();
            if (resultsolution > this.genomsresult.Min())
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f
            {
                lock (lockObject)
                {
                    Debug.WriteLine("LOCK");
                    callback(genArray[ind_best_gen]);
                }
                resultsolution = this.genomsresult.Min();
<<<<<<< HEAD
            }
            if (outputGenom.CalculateGenomWayLenght(WayLengMap) > this.genomsresult.Min())
            {
                resultsolution = this.genomsresult.Min();
                outputGenom = this.genArray[genomsresult.IndexOf(resultsolution)].ClonePopulation();
                Console.WriteLine(this.GetEpochResult());
=======
                bestGen = genArray[ind_best_gen].ClonePopulation();
                Debug.WriteLine(this.GetEpochResult());
>>>>>>> 82231f1d06c3d43fdbda24e649b87ced2096f27f
            }
        }
    }
    public override string ToString()
    {
        string outs = "";
        foreach (Genom gen in genArray)
        {
            outs += $"{gen.ToString()} \n\n";
        }
        return outs;
    }

    private void CreateNewGenom()
    {
        int i_genom_new = genomsresult.IndexOf(genomsresult.Max());
        genArray[i_genom_new] = new Genom(WayLengMap.Length);
    }

    private void MutateRandomGen()
    {
        genArray[new Random().Next(0, genArray.Length)].GenomMutation();
    }
    public void Pause()
    {
        if(pauseEvent.IsSet)
        {
            pauseEvent.Reset();
        }    
    }

    public void Resume()
    {
        pauseEvent.Set();
    }
}
