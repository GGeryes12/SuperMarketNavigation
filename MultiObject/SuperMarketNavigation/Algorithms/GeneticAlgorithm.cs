using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SuperMarketNavigation.Models;
using SuperMarketNavigation.Sorting;
using SuperMarketNavigation.Algorithms;
using SuperMarketNavigation.Operators;

namespace SuperMarketNavigation.Algorithms
{
    public abstract class GeneticAlgorithm
    {
        public Population population;
        protected MarketLayout market;
        protected Random random = new Random();
        protected double mutationRate;
        protected string runPath;
        protected List<Individual> archive; // Stores elite solutions
        protected Models.GenerationPerformance generationPerformance = new GenerationPerformance();
        protected List<double[]> weightVectors;
        protected double[] idealPoint;

        public GeneticAlgorithm(MarketLayout market, int popSize, double mutationRate, string runPath)
        {
            this.market = market;
            this.mutationRate = mutationRate;
            this.runPath = runPath;
            population = new Population();
            population.Initialize(popSize, market);
            weightVectors = GenerateWeightVectors(popSize);
            idealPoint = new double[2];
            if (population.Individuals.Count == 0)
            {
                throw new Exception("Population was not initialized correctly!");
            }
        }

        private List<double[]> GenerateWeightVectors(int numVectors)
        {
            List<double[]> vectors = new List<double[]>();
            for (int i = 0; i <= numVectors; i++)
            {
                double w1 = (double)i / numVectors;
                double w2 = 1 - w1;
                vectors.Add(new double[] { w1, w2 });
            }
            return vectors;
        }

        public abstract void Run(int generations);

        private void ComputeIdealPoint()
        {
            idealPoint[0] = population.Individuals.Min(ind => ind.Objectives[0]);
            idealPoint[1] = population.Individuals.Min(ind => ind.Objectives[1]);
        }

        private void SaveRawData(StreamWriter writer, int generation)
        {
            foreach (var ind in population.Individuals)
            {
                double tchebycheffScore = weightVectors.Average(lambda =>
                    Math.Max(lambda[0] * Math.Abs(ind.Objectives[0] - idealPoint[0]),
                             lambda[1] * Math.Abs(ind.Objectives[1] - idealPoint[1])));
                string itemSequence = string.Join("->", ind.items.Select(item => item.isle));
                writer.WriteLine($"{generation},{ind.Objectives[0]},{ind.Objectives[1]},{tchebycheffScore},{itemSequence},{ind.wp}");
            }
        }

        protected void EvaluatePopulation()
        {
            foreach (var ind in population.Individuals)
                ObjectiveFunction.EvaluateIndiv(ind, market);
        }

        protected List<Individual> PerformCrossoverMutation()
        {
            List<Individual> offspring = new List<Individual>();
            HashSet<string> seenSolutions = new HashSet<string>(population.Individuals.Select(ind => ind.GetSolutionKey()));

            int maxOffspring = population.Individuals.Count;
            int attempts = 0;
            int maxAttempts = maxOffspring * 2; // Prevent infinite loops

            while (offspring.Count < maxOffspring && attempts < maxAttempts)
            {
                attempts++;

                Individual parent1 = TournamentSelection();
                Individual parent2 = TournamentSelection();

                Individual child = GeneticOperators.Order1Crossover(parent1, parent2, market);
                GeneticOperators.Mutate(child, mutationRate);
                ObjectiveFunction.EvaluateIndiv(child, market);

                string childKey = child.GetSolutionKey();

                // Ensure uniqueness
                if (!seenSolutions.Contains(childKey))
                {
                    seenSolutions.Add(childKey);
                    offspring.Add(child);
                }
            }

            Console.WriteLine($"Generated {offspring.Count} offspring for {maxOffspring} population slots.");
            return offspring;
        }
        protected Individual TournamentSelection()
        {
            int tournamentSize = 3;
            List<Individual> tournament = new List<Individual>();
            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = random.Next(population.Individuals.Count);
                tournament.Add(population.Individuals[randomIndex]);
            }
            return tournament.OrderBy(ind => ind.Objectives.Sum() + random.NextDouble() * 0.1).First();
        }


        protected abstract void SortPopulation();
        protected abstract Population SelectNextGeneration(List<Individual> offspring);
    }
}
