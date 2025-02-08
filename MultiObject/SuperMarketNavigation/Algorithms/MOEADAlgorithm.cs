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
            for (int i = 0; i <= numVectors; i++)
            {
                double w1 = (double)i / numVectors;
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
            double[] lambda = weightVectors[subproblemIdx];
            double[] z_star = ComputeIdealPoint();
            double childFitness = TchebycheffFunction(child, lambda, z_star);
            List<int> neighbors = FindNeighbors(subproblemIdx);

            foreach (int neighborIdx in neighbors)
            {
                Individual neighbor = population.Individuals[neighborIdx];
                double neighborFitness = TchebycheffFunction(neighbor, lambda, z_star);
                if (childFitness < neighborFitness)
                {
                    population.Individuals[neighborIdx] = child;
                }
            }
        }

        /*public override void Run(int generations)
        {
            for (int gen = 0; gen < generations; gen++)
            {
                Console.WriteLine($"Generation {gen + 1}");
                
                double[] z_star = ComputeIdealPoint();
                List<Individual> offspring = new List<Individual>();

                for (int i = 0; i < population.Individuals.Count; i++)
                {
                    Individual parent1 = SelectParentFromNeighborhood(i);
                    Individual parent2 = SelectParentFromNeighborhood(i);
                    Individual child = GeneticOperators.Order1Crossover(parent1, parent2, market);
                    GeneticOperators.Mutate(child, mutationRate);
                    ObjectiveFunction.EvaluateIndiv(child, market);
                    offspring.Add(child);
                }
                
                for (int i = 0; i < population.Individuals.Count; i++)
                {
                    UpdatePopulation(offspring[i], i);
                }
                
                Console.WriteLine($"Generation {gen + 1} completed");
            }
        }*/

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
