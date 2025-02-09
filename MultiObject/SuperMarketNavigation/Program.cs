using System;
using SuperMarketNavigation.Models;
using SuperMarketNavigation.Algorithms;
using ScottPlot;
using SuperMarketNavigation.Operators;
using SuperMarketNavigation.Sorting;
using SuperMarketNavigation.Utils;
using System.Diagnostics;

namespace SuperMarketNavigation
{
    class Program
    {

        static void Main(string[] args)
        {
            // Paths
            string ffmpegPath = @"C:\Program Files\ffmpeg-2025-01-27-git-959b799c8d-essentials_build\bin\ffmpeg.exe";
            string debugFolder = AppDomain.CurrentDomain.BaseDirectory;

            // Generate a unique folder name for this run
            string runFolderName = "Run_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string runFolderPath = Path.Combine(debugFolder, runFolderName);

            // Create the subfolder
            Directory.CreateDirectory(runFolderPath);

            Console.WriteLine($"Run folder created: {runFolderPath}");

            // Set the images folder and output video path relative to the debug folder
            string imagesFolder = runFolderPath;
            string outputVideo = Path.Combine(runFolderPath, "output.mp4");
            string rawDataFilePath = Path.Combine(runFolderPath, "raw_data.csv");

            // Step 1: Initialize market layout
            //MarketLayout market = new MarketLayout(12,12,0.3);
            MarketLayout market = MarketLayout.LoadLayout(@"C:\Users\Jeryes\github\SuperMarketNavigation\MultiObject\SuperMarketNavigation\bin\Debug\net9.0\Run_20250208_175147\market_layout.json");
            market.SaveLayout(Path.Combine(runFolderPath, "market_layout.json"));
            market.VisualizeMarket(runFolderPath);
            int populationSize = 500;
            int generations = 80;
            int itirations = 10;
            for (int i = 0; i < itirations; i++)
            {
                OptimizationRunner.RunAlgorithm<NSGA2Algorithm>(Path.Combine(runFolderPath,$"Run{i+1}"), market, populationSize, 0.3, generations);
                OptimizationRunner.RunAlgorithm<NSGA3Algorithm>(Path.Combine(runFolderPath,$"Run{i+1}"), market, populationSize, 0.3, generations);
                OptimizationRunner.RunAlgorithm<SPEA2Algorithm>(Path.Combine(runFolderPath,$"Run{i+1}"), market, populationSize, 0.5, generations);
            }

            /*try
            {
                // Create the video for NSGA-III
                VideoCreator.CreateVideoFromImages(ffmpegPath, nsga3FolderPath, nsga3OutputVideo, 10);
                Console.WriteLine("Video generation complete!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }*/
        }


        // Visualize Pareto Front
        private static void VisualizeResults(Models.Population population)
        {
            double[] walkingTimes = population.Individuals.Select(i => i.Objectives[0]).ToArray();
            double[] exposureTimes = population.Individuals.Select(i => i.Objectives[1]).ToArray();

            var plt = new ScottPlot.Plot();
            plt.Add.ScatterPoints(walkingTimes, exposureTimes);
            plt.Title("Pareto Front");
            plt.XLabel("Walking Time");
            plt.YLabel("Exposure Time");
            plt.SaveBmp("pareto_front.png", 800, 600);
            Console.WriteLine("Pareto Front saved as 'pareto_front.png'.");
        }
        static void TestBruteForce()
        {
            // Create a new plot
            var plt = new Plot();

            MarketLayout market = new MarketLayout(8, 8, 0.5);
            Models.Population population = new Models.Population();
            population.Initialize(5000, market);

            //var results = population.EvaluatePopulation();
            double[] xs = new double[population.Individuals.Count];
            double[] ys = new double[population.Individuals.Count];
            for (int i = 0; i < population.Individuals.Count; i++)
            {
                double[] res = ObjectiveFunction.EvaluateIndiv(population.Individuals[i], market);
                //plt.Add.ScatterPoints(new double[] { res[0] }, new double[] { res[1] });
                xs[i] = res[0];
                ys[i] = res[1];
            }
            plt.Add.ScatterPoints(xs, ys);
            plt.Title("Population Evaluation");
            plt.XLabel("Walking Time");
            plt.YLabel("Heat-Sensitive Exposure Time");

            // Save the plot to a file
            plt.SaveBmp("PopulationEvaluation.png", 800, 600);
            Console.WriteLine("Graph saved as 'PopulationEvaluation.png'");
        }
        static void TestCrossover(MarketLayout market)
        {
            Individual ind1 = new Individual(market);
            Individual ind2 = new Individual(market);

            Individual child = Operators.GeneticOperators.Order1Crossover(ind1, ind2, market);

            Console.WriteLine("Testing Crossover:");
            Console.WriteLine("Parent 1:");
            Console.WriteLine(ind1.ToString());
            Console.WriteLine("Parent 2:");
            Console.WriteLine(ind2.ToString());

            Console.WriteLine("Child:");
            Console.WriteLine(child.ToString());
        }
        static void TestMutation(MarketLayout market)
        {
            Individual ind1 = new Individual(market);
            Console.WriteLine("Testing Mutation:");
            Console.WriteLine("Before Mutation:");
            Console.WriteLine(ind1.ToString());

            GeneticOperators.Mutate(ind1, 1);


            Console.WriteLine("After Mutation:");
            Console.WriteLine(ind1.ToString());
        }
        static void GeneratePopulation(MarketLayout market, int popSize)
        {
            Models.Population population = new Models.Population();
            population.Initialize(popSize, market);
            foreach (Individual ind in population.Individuals)
            {
                Console.WriteLine(ind.ToString());
            }
        }
    }
}
