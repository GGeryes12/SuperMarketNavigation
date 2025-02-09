using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketNavigation.Models;
using SuperMarketNavigation.Sorting;

namespace SuperMarketNavigation.Algorithms
{
    public class SPEA2Algorithm : GeneticAlgorithm
    {
        private int archiveSize;

        public SPEA2Algorithm(MarketLayout market, int popSize, double mutationRate, string runPath)
            : base(market, popSize, mutationRate, runPath)
        {
            archive = new List<Individual>(); // Initialize archive for elite solutions
            archiveSize = popSize / 2; // Archive typically half the population size
        }

        public override void Run(int generations)
        {
            for (int gen = 0; gen < generations; gen++)
            {
                EvaluatePopulation();
                UpdateArchive(); // Maintain elite solutions
                AssignFitness();
                List<Individual> offspring = PerformCrossoverMutation();
                List<Individual> combinedPool = offspring.Concat(archive).ToList();
                population = SelectNextGeneration(combinedPool);
            }
        }

        private void UpdateArchive()
        {
            List<Individual> combined = population.Individuals.Concat(archive).ToList();
            var paretoFronts = NonDominatedSorting.PerformSorting(combined);
            archive.Clear();
            HashSet<string> uniqueSolutions = new HashSet<string>();

            foreach (var front in paretoFronts)
            {
                foreach (var ind in front)
                {
                    if (archive.Count >= archiveSize) break;

                    string key = ind.GetSolutionKey();
                    if (!uniqueSolutions.Contains(key))
                    {
                        uniqueSolutions.Add(key);
                        archive.Add(ind);
                    }
                }
            }

        }

        private void AssignFitness()
        {
            foreach (var ind in population.Individuals)
            {
                ind.Rank = 0;
                ind.CrowdingDistance = 0;
                ind.DominationCount = 0;
                ind.DominatedSet = new List<Individual>();

                foreach (var other in population.Individuals)
                {
                    if (NonDominatedSorting.Dominates(ind, other))
                        ind.DominatedSet.Add(other);
                    else if (NonDominatedSorting.Dominates(other, ind))
                        ind.DominationCount++;
                }
            }
        }

        protected override Population SelectNextGeneration(List<Individual> offspring)
        {
            Population nextGen = new Population();
            List<Individual> combined = population.Individuals.Concat(offspring).Concat(archive).ToList();
            var paretoFronts = NonDominatedSorting.PerformSorting(combined);

            foreach (var front in paretoFronts)
            {
                if (nextGen.Individuals.Count + front.Count <= population.Individuals.Count / 2)
                {
                    nextGen.Individuals.AddRange(front); // Keep some elite individuals
                }
                else
                {
                    NonDominatedSorting.CalculateCrowdingDistance(front);
                    var sortedFront = front.OrderByDescending(ind => ind.CrowdingDistance).ToList();
                    nextGen.Individuals.AddRange(sortedFront.Take(population.Individuals.Count / 2)); // Fill remaining spots
                    break;
                }
            }

            // Ensure diversity by adding some new offspring
            while (nextGen.Individuals.Count < population.Individuals.Count)
            {
                nextGen.Individuals.Add(offspring[random.Next(offspring.Count)]);
            }

            return nextGen;

        }

        protected override void SortPopulation()
        {
            var fronts = NonDominatedSorting.PerformSorting(population.Individuals);
            foreach (var (front, rank) in fronts.Select((value, i) => (value, i + 1)))
            {
                foreach (var ind in front)
                {
                    ind.Rank = rank;
                }
            }
        }
    }
}
