using System.Dynamic;

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
        public List<GenerationPerformance> performanceHistory = new List<GenerationPerformance>();

        public void TrackPerformance(Population population)
        {
            double avgWalking = population.Individuals.Average(i => i.Objectives[0]);
            double avgExposure = population.Individuals.Average(i => i.Objectives[1]);
            double minWalking = population.Individuals.Min(i => i.Objectives[0]);
            double minExposure = population.Individuals.Min(i => i.Objectives[1]);
            double maxWalking = population.Individuals.Max(i => i.Objectives[0]);
            double maxExposure = population.Individuals.Max(i => i.Objectives[1]);

            performanceHistory.Add(new GenerationPerformance
            {
                AverageWalkingTime = avgWalking,
                AverageExposureTime = avgExposure,
                MinWalkingTime = minWalking,
                MinExposureTime = minExposure,
                MaxWalkingTime = maxWalking,
                MaxExposureTime = maxExposure
            });
        }
        public void PlotPerformanceOverGenerations(string savePathBase)
        {
            // Data for generations
            double[] generations = Enumerable.Range(0, performanceHistory.Count).Select(i => (double)i).ToArray();

            // Walking Time Data
            double[] avgWalking = performanceHistory.Select(p => p.AverageWalkingTime).ToArray();
            double[] minWalking = performanceHistory.Select(p => p.MinWalkingTime).ToArray();
            double[] maxWalking = performanceHistory.Select(p => p.MaxWalkingTime).ToArray();

            // Exposure Time Data
            double[] avgExposure = performanceHistory.Select(p => p.AverageExposureTime).ToArray();
            double[] minExposure = performanceHistory.Select(p => p.MinExposureTime).ToArray();
            double[] maxExposure = performanceHistory.Select(p => p.MaxExposureTime).ToArray();

            // Plot for Walking Time
            var walkingPlot = new ScottPlot.Plot();//800, 600);
            walkingPlot.Add.Scatter(generations, avgWalking);//, label: "Avg Walking Time");//
            walkingPlot.Add.Scatter(generations, minWalking);//, label: "Min Walking Time");
            walkingPlot.Add.Scatter(generations, maxWalking);//, label: "Max Walking Time");
            walkingPlot.Title("Walking Time Over Generations");
            walkingPlot.XLabel("Generation");
            walkingPlot.YLabel("Walking Time");
            //walkingPlot.Legend();
            string walkingSavePath = $"{savePathBase}_walking_time.png";
            walkingPlot.SavePng(walkingSavePath,800,600);
            Console.WriteLine($"Walking time graph saved: {walkingSavePath}");

            // Plot for Exposure Time
            var exposurePlot = new ScottPlot.Plot();//800, 600);
            exposurePlot.Add.Scatter(generations, avgExposure);//, label: "Avg Exposure Time");
            exposurePlot.Add.Scatter(generations, minExposure);//, label: "Min Exposure Time");
            exposurePlot.Add.Scatter(generations, maxExposure);//, label: "Max Exposure Time");
            exposurePlot.Title("Exposure Time Over Generations");
            exposurePlot.XLabel("Generation");
            exposurePlot.YLabel("Exposure Time");
            //exposurePlot.Legend();
            string exposureSavePath = $"{savePathBase}_exposure_time.png";
            exposurePlot.SavePng(exposureSavePath,800,height: 600);
            Console.WriteLine($"Exposure time graph saved: {exposureSavePath}");
        }

    }

}