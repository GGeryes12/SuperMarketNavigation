using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketNavigation.Models;
using ScottPlot;

namespace SuperMarketNavigation.Models
{
    public class GenerationPerformance
    {
        public double AverageWalkingTime { get; set; }
        public double AverageExposureTime { get; set; }
        public double MinWalkingTime { get; set; }
        public double MinExposureTime { get; set; }
        public double MaxWalkingTime { get; set; }
        public double MaxExposureTime { get; set; }
        public double AverageTchebycheff { get; set; }
        public double PopulationSize { get; set; }
        public double ArchiveSize { get; set; }
        public List<GenerationPerformance> performanceHistory = new List<GenerationPerformance>();
        public void TrackPerformance(Population population)
        {
            if (population.Individuals.Count == 0)
            {
                Console.WriteLine("Warning: Population is empty when tracking performance.");
                return;
            }

            double avgWalking = population.Individuals.Average(i => i.Objectives[0]);
            double avgExposure = population.Individuals.Average(i => i.Objectives[1]);
            double minWalking = population.Individuals.Min(i => i.Objectives[0]);
            double minExposure = population.Individuals.Min(i => i.Objectives[1]);
            double maxWalking = population.Individuals.Max(i => i.Objectives[0]);
            double maxExposure = population.Individuals.Max(i => i.Objectives[1]);

            // Ensure PopulationSize is assigned correctly
            this.PopulationSize = population.Individuals.Count;

            performanceHistory.Add(new GenerationPerformance
            {
                AverageWalkingTime = avgWalking,
                AverageExposureTime = avgExposure,
                MinWalkingTime = minWalking,
                MinExposureTime = minExposure,
                MaxWalkingTime = maxWalking,
                MaxExposureTime = maxExposure,
                PopulationSize = this.PopulationSize
            });
        }

        public void TrackPerformance(Population population, List<double[]> weightVectors, double[] idealPoint)
        {
            double avgWalking = population.Individuals.Average(i => i.Objectives[0]);
            double avgExposure = population.Individuals.Average(i => i.Objectives[1]);
            double minWalking = population.Individuals.Min(i => i.Objectives[0]);
            double minExposure = population.Individuals.Min(i => i.Objectives[1]);
            double maxWalking = population.Individuals.Max(i => i.Objectives[0]);
            double maxExposure = population.Individuals.Max(i => i.Objectives[1]);

            double avgTchebycheff = population.Individuals.Average(i =>
                weightVectors.Average(lambda =>
                    Math.Max(lambda[0] * Math.Abs(i.Objectives[0] - idealPoint[0]),
                             lambda[1] * Math.Abs(i.Objectives[1] - idealPoint[1]))
                )
            );

            performanceHistory.Add(new GenerationPerformance
            {
                AverageWalkingTime = avgWalking,
                AverageExposureTime = avgExposure,
                MinWalkingTime = minWalking,
                MinExposureTime = minExposure,
                MaxWalkingTime = maxWalking,
                MaxExposureTime = maxExposure,
                AverageTchebycheff = avgTchebycheff,

            });
        }

        public void PlotPerformanceOverGenerations(string savePathBase)
        {
            double[] generations = Enumerable.Range(0, performanceHistory.Count).Select(i => (double)i).ToArray();

            double[] avgWalking = performanceHistory.Select(p => p.AverageWalkingTime).ToArray();
            double[] minWalking = performanceHistory.Select(p => p.MinWalkingTime).ToArray();
            double[] maxWalking = performanceHistory.Select(p => p.MaxWalkingTime).ToArray();

            double[] avgExposure = performanceHistory.Select(p => p.AverageExposureTime).ToArray();
            double[] minExposure = performanceHistory.Select(p => p.MinExposureTime).ToArray();
            double[] maxExposure = performanceHistory.Select(p => p.MaxExposureTime).ToArray();

            double[] avgTchebycheff = performanceHistory.Select(p => p.AverageTchebycheff).ToArray();

            double[] popSize = performanceHistory.Select(p => p.PopulationSize).ToArray();

            var walkingPlot = new ScottPlot.Plot();
            walkingPlot.Add.Scatter(generations, avgWalking);
            walkingPlot.Add.Scatter(generations, minWalking);
            walkingPlot.Add.Scatter(generations, maxWalking);
            walkingPlot.Title("Walking Time Over Generations");
            walkingPlot.XLabel("Generation");
            walkingPlot.YLabel("Walking Time");
            walkingPlot.SavePng($"{savePathBase}_walking_time.png", 800, 600);

            var exposurePlot = new ScottPlot.Plot();
            exposurePlot.Add.Scatter(generations, avgExposure);
            exposurePlot.Add.Scatter(generations, minExposure);
            exposurePlot.Add.Scatter(generations, maxExposure);
            exposurePlot.Title("Exposure Time Over Generations");
            exposurePlot.XLabel("Generation");
            exposurePlot.YLabel("Exposure Time");
            exposurePlot.SavePng($"{savePathBase}_exposure_time.png", 800, 600);

            var popsizePlot = new ScottPlot.Plot();
            popsizePlot.Add.Scatter(generations, popSize);
            popsizePlot.Title("Population Size over Generations");
            popsizePlot.XLabel("Generation");
            popsizePlot.YLabel("Population Size");
            popsizePlot.SavePng($"{savePathBase}_population_size.png", 800, 600);
        }
    }
}
