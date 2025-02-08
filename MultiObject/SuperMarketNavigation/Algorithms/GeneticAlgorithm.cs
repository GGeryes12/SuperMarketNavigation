using SuperMarketNavigation.Models;
using SuperMarketNavigation.Sorting;

namespace SuperMarketNavigation.Algorithms
{
    public abstract class GeneticAlgorithm
    {
        public Population population;
        protected MarketLayout market;
        protected Random random = new Random();
        protected double mutationRate;
        protected string runPath;
        Models.GenerationPerformance generationPerformance = new GenerationPerformance();
        public GeneticAlgorithm(MarketLayout market, int popSize, double mutationRate, string runPath)
        {
            this.market = market;
            this.mutationRate = mutationRate;
            this.runPath = runPath;
            population = new Population();
            population.Initialize(popSize, market);
            if (population.Individuals.Count == 0)
            {
                throw new Exception("Population was not initialized correctly!");
            }
        }

        public void Run(int generations)
        {
            string rawDataFilePath = Path.Combine(runPath, "raw_data.csv");
            using (StreamWriter writer = new StreamWriter(rawDataFilePath))
            {
                writer.WriteLine("Generation,Individual,WalkingTime,ExposureTime");
                for (int i = 0; i < generations; i++)
                {
                    Console.WriteLine($"Generation {i + 1}");
                    EvaluatePopulation();
                    SortPopulation();
                    List<Individual> offspring = PerformCrossoverMutation();
                    population = SelectNextGeneration(offspring);
                    Console.WriteLine($"Generation {i + 1} completed, Population Size: {population.Individuals.Count}");
                    //VisualizeFullPopulation(population, i, runPath);
                    SaveRawData(writer, i);
                    generationPerformance.TrackPerformance(population);
                }
                VisualizeFullPopulation(population, generations, runPath);
            }
            generationPerformance.PlotPerformanceOverGenerations(Path.Combine(runPath, "POG_"));
        }

        private void SaveRawData(StreamWriter writer, int generation)
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
        private static void VisualizeResults(Models.Population population, int generation)
        {
            double[] walkingTimes = population.Individuals
                .Where(i => i.Objectives[0] > 0 && i.Objectives[1] > 0) // Exclude zero position point
                .Select(i => i.Objectives[0])
                .ToArray();

            double[] exposureTimes = population.Individuals
                .Where(i => i.Objectives[0] > 0 && i.Objectives[1] > 0) // Exclude zero position point
                .Select(i => i.Objectives[1])
                .ToArray();

            var plt = new ScottPlot.Plot();

            // Add scatter plot without lines
            plt.Add.ScatterPoints(walkingTimes, exposureTimes);
            plt.Axes.SetLimitsX(0, 300);
            plt.Axes.SetLimitsY(0, 1500);
            plt.Title($"Pareto Front - Generation {generation}");
            plt.XLabel("Walking Time");
            plt.YLabel("Exposure Time");
            // Save with generation number
            string filename = $"pareto_front_gen_{generation}.png";
            plt.SaveBmp(filename, 800, 600);

            Console.WriteLine($"Pareto Front for Generation {generation} saved as '{filename}'.");
        }
        protected void EvaluatePopulation()
        {
            foreach (var ind in population.Individuals)
                ObjectiveFunction.EvaluateIndiv(ind, market);
        }
        protected List<Individual> PerformCrossoverMutation()
        {
            List<Individual> offspring = new List<Individual>();
            HashSet<string> seenSolutions = new HashSet<string>(
                population.Individuals.Select(ind => ind.GetSolutionKey())); // Include existing population keys

            int attempts = 0;
            int maxAttempts = population.Individuals.Count * 2; // Prevent infinite loops

            while (offspring.Count < population.Individuals.Count / 2 && attempts < maxAttempts)
            {
                attempts++;

                Individual parent1 = TournamentSelection();
                Individual parent2 = TournamentSelection();

                Individual child = Operators.GeneticOperators.Order1Crossover(parent1, parent2, market);
                Operators.GeneticOperators.Mutate(child, mutationRate);

                // Evaluate the child
                var evaluation = ObjectiveFunction.EvaluateIndiv(child, market);
                child.Objectives[0] = evaluation[0];
                child.Objectives[1] = evaluation[1];

                string childKey = child.GetSolutionKey();

                // Ensure it's unique against BOTH current offspring AND existing population
                if (!seenSolutions.Contains(childKey))
                {
                    seenSolutions.Add(childKey);
                    offspring.Add(child);
                }
            }

            return offspring;
        }


        protected Individual TournamentSelection()
        {
            Individual ind1 = population.Individuals[random.Next(population.Individuals.Count)];
            Individual ind2 = population.Individuals[random.Next(population.Individuals.Count)];

            // % chance to select the better individual, 20% chance to select the worse one
            if (random.NextDouble() < 1)
            {
                return (ind1.Rank < ind2.Rank ||
                       (ind1.Rank == ind2.Rank && ind1.CrowdingDistance > ind2.CrowdingDistance)) ? ind1 : ind2;
            }
            else
            {
                return (ind1.Rank < ind2.Rank ||
                       (ind1.Rank == ind2.Rank && ind1.CrowdingDistance > ind2.CrowdingDistance)) ? ind2 : ind1;
            }
        }

        protected abstract void SortPopulation();
        protected abstract Population SelectNextGeneration(List<Individual> offspring);
    }
}
