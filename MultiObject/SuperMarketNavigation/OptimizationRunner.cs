using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SuperMarketNavigation.Models;
using SuperMarketNavigation.Algorithms;
using SuperMarketNavigation.Sorting;

namespace SuperMarketNavigation
{
    public class OptimizationRunner
    {
        public static void RunAlgorithm<T>(string runFolderPath, MarketLayout market, int populationSize, double mutationRate, int generations) where T : GeneticAlgorithm
        {
            string algorithmName = typeof(T).Name;
            string algorithmFolderPath = Path.Combine(runFolderPath, algorithmName);
            Directory.CreateDirectory(algorithmFolderPath);
            string rawDataFilePath = Path.Combine(algorithmFolderPath, $"{algorithmName}_raw_data.csv");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            GeneticAlgorithm algorithm = (T)Activator.CreateInstance(typeof(T), market, populationSize, mutationRate, algorithmFolderPath);
            Console.WriteLine($"Running {algorithmName} optimization...");

            // Create an instance of GenerationPerformance for tracking
            GenerationPerformance generationPerformance = new GenerationPerformance();
            using (StreamWriter writer = new StreamWriter(rawDataFilePath))
            {
                writer.WriteLine("Generation,WalkingTime,ExposureTime,IsleOrder,WalkingPattern");
                for (int gen = 0; gen < generations; gen++)
                {
                    Console.WriteLine($"{algorithmName} Generation {gen + 1}");
                    algorithm.Run(1); // Run one generation at a time
                    SaveRawData(writer, gen, algorithm.population); // Save raw data
                    generationPerformance.TrackPerformance(algorithm.population); // Track performance
                    //VisualizeFullPopulation(algorithm.population, gen, algorithmFolderPath);
                }
            }
            stopWatch.Stop();
            long duration = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"{algorithmName} Elapsed time = {duration} ms");
            generationPerformance.PlotPerformanceOverGenerations(algorithmFolderPath + $"/{algorithmName}");
            // Generate final population visualization
            VisualizeFullPopulation(algorithm.population, generations, algorithmFolderPath);

            Console.WriteLine($"\n{algorithmName} Optimization complete! Results saved.");
        }
        private static void SaveRawData(StreamWriter writer, int generation, Population population)
        {
            foreach (var ind in population.Individuals)
            {
                string itemSequence = string.Join("->", ind.items.Select(item => item.isle));
                writer.WriteLine($"{generation},{ind.Objectives[0]},{ind.Objectives[1]},{itemSequence},{ind.wp}");
            }
        }
        private static void VisualizeFullPopulation(Population population, int generation, string runPath)
        {

            var paretoFronts = NonDominatedSorting.PerformSorting(population.Individuals);
            var firstFront = paretoFronts[0];

            var nonDominated = firstFront.Select(i => (i.Objectives[0], i.Objectives[1])).ToList();
            var dominated = population.Individuals.Except(firstFront)
                                                  .Select(i => (i.Objectives[0], i.Objectives[1]))
                                                  .ToList();

            var plt = new ScottPlot.Plot();

            // Plot dominated solutions (gray)
            plt.Add.ScatterPoints(
                dominated.Select(i => i.Item1).ToArray(),
                dominated.Select(i => i.Item2).ToArray(),
                color: ScottPlot.Colors.Gray
            );

            // Plot non-dominated solutions (highlighted in red)
            plt.Add.ScatterPoints(
                nonDominated.Select(i => i.Item1).ToArray(),
                nonDominated.Select(i => i.Item2).ToArray(),
                color: ScottPlot.Colors.Red
            );

            //plt.Axes.SetLimitsX(50, 150);
            //plt.Axes.SetLimitsY(500, 3000);
            plt.Title($"Population Spread - Generation {generation}");
            plt.XLabel("Walking Time");
            plt.YLabel("Exposure Time");
            plt.SavePng(Path.Combine(runPath, $"population_spread_gen_{generation}.png"), 800, 600);

            Console.WriteLine($"Pareto Front Visualization Saved.");
        }

    }
}
