using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketNavigation.Models;
using SuperMarketNavigation.Operators;

namespace SuperMarketNavigation.Algorithms
{
    public class MOEADAlgorithm : GeneticAlgorithm
    {
        private List<double[]> weightVectors;
        private int T;

        public MOEADAlgorithm(MarketLayout market, int popSize, double mutationRate, string runPath)
            : base(market, popSize, mutationRate, runPath)
        {
            weightVectors = GenerateWeightVectors(popSize);
            T = Math.Max(2, (int)(0.1 * popSize)); // 10% neighborhood size
        }

        private List<double[]> GenerateWeightVectors(int numVectors)
        {
            List<double[]> vectors = new List<double[]>();

            for (int i = 0; i < numVectors; i++) // Ensure correct number of vectors
            {
                double w1 = (double)i / (numVectors - 1);
                double w2 = 1 - w1;
                vectors.Add(new double[] { w1, w2 });
            }

            return vectors;
        }

        private List<int> FindNeighbors(int index)
        {
            double[] target = weightVectors[index];
            return weightVectors.Select((w, i) => new { Distance = Math.Sqrt(Math.Pow(w[0] - target[0], 2) + Math.Pow(w[1] - target[1], 2)), Index = i })
                                .OrderBy(n => n.Distance)
                                .Where(n => n.Index < population.Individuals.Count) // Ensure valid index
                                .Take(T)
                                .Select(n => n.Index)
                                .ToList();
        }


        private double[] ComputeIdealPoint()
        {
            double minWalking = population.Individuals.Min(ind => ind.Objectives[0]);
            double minExposure = population.Individuals.Min(ind => ind.Objectives[1]);
            return new double[] { minWalking, minExposure };
        }

        private double TchebycheffFunction(Individual ind, double[] lambda, double[] z_star)
        {
            double maxTerm = double.MinValue;
            for (int i = 0; i < ind.Objectives.Length; i++)
            {
                double weightedDistance = lambda[i] * Math.Abs(ind.Objectives[i] - z_star[i]);
                maxTerm = Math.Max(maxTerm, weightedDistance);
            }
            return maxTerm;
        }

        private Individual SelectParentFromNeighborhood(int idx)
        {
            List<int> neighbors = FindNeighbors(idx);
            Random rand = new Random();
            return population.Individuals[neighbors[rand.Next(neighbors.Count)]];
        }

        private void UpdatePopulation(Individual child, int subproblemIdx)
        {
            if (subproblemIdx >= population.Individuals.Count)
            {
                Console.WriteLine($"Warning: Subproblem index {subproblemIdx} out of range. Skipping update.");
                return; // Avoid crashing due to an invalid index
            }

            double[] lambda = weightVectors[subproblemIdx];
            double[] z_star = ComputeIdealPoint();
            double childFitness = TchebycheffFunction(child, lambda, z_star);
            List<int> neighbors = FindNeighbors(subproblemIdx);

            foreach (int neighborIdx in neighbors)
            {
                if (neighborIdx >= population.Individuals.Count)
                {
                    Console.WriteLine($"Warning: Neighbor index {neighborIdx} out of range. Skipping update.");
                    continue; // Prevent out-of-range access
                }

                Individual neighbor = population.Individuals[neighborIdx];
                double neighborFitness = TchebycheffFunction(neighbor, lambda, z_star);

                if (childFitness < neighborFitness)
                {
                    population.Individuals[neighborIdx] = child;
                }
            }
        }

        public override void Run(int generations)
        {
            for (int gen = 0; gen < generations; gen++)
            {
                double[] z_star = ComputeIdealPoint();
                List<Individual> offspring = PerformCrossoverMutation();

                // Ensure uniqueness of offspring
                HashSet<string> seenSolutions = new HashSet<string>(population.Individuals.Select(ind => ind.GetSolutionKey()));
                List<Individual> uniqueOffspring = offspring.Where(child => seenSolutions.Add(child.GetSolutionKey())).ToList();

                foreach (var child in offspring)
                {
                    string childKey = child.GetSolutionKey();
                    if (!seenSolutions.Contains(childKey))
                    {
                        seenSolutions.Add(childKey);
                        uniqueOffspring.Add(child);
                    }
                }

                int minCount = Math.Min(population.Individuals.Count, uniqueOffspring.Count); // Ensure valid index range

                for (int i = 0; i < minCount; i++)
                {
                    UpdatePopulation(uniqueOffspring[i], i);
                }
            }
        }

        protected override void SortPopulation()
        {
            throw new NotImplementedException();
        }

        protected override Population SelectNextGeneration(List<Individual> offspring)
        {
            throw new NotImplementedException();
        }
    }
}
