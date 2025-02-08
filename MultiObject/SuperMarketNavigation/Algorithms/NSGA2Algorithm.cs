using SuperMarketNavigation.Models;
using SuperMarketNavigation.Sorting;
namespace SuperMarketNavigation.Algorithms
{
    public class NSGA2Algorithm : GeneticAlgorithm
    {
        private string rawDataFilePath;
        public NSGA2Algorithm(MarketLayout market, int popSize, double mutationRate, string runPath, string rawDataFilePath) 
            : base(market, popSize, mutationRate, runPath) 
        {
            this.rawDataFilePath = rawDataFilePath;
        }

        protected override void SortPopulation()
        {
            var fronts = new List<List<Individual>>();
            foreach (var p in population.Individuals)
            {
                p.DominationCount = 0;
                p.DominatedSet = new List<Individual>();

                foreach (var q in population.Individuals)
                {
                    if (NonDominatedSorting.Dominates(p, q))
                        p.DominatedSet.Add(q);
                    else if (NonDominatedSorting.Dominates(q, p))
                        p.DominationCount++;
                }

                if (p.DominationCount == 0)
                {
                    p.Rank = 1;
                    if (fronts.Count == 0) fronts.Add(new List<Individual>());
                    fronts[0].Add(p);
                }
            }
        }

        protected override Population SelectNextGeneration(List<Individual> offspring)
        {
            // Step 1: Combine current population and offspring
            List<Individual> combined = population.Individuals.Concat(offspring).ToList();

            // Step 2: Perform non-dominated sorting
            var paretoFronts = NonDominatedSorting.PerformSorting(combined);

            Population nextGeneration = new Population();

            // Step 3: Fill the next generation with Pareto fronts
            foreach (var front in paretoFronts)
            {
                if (nextGeneration.Individuals.Count + front.Count <= population.Individuals.Count)
                {
                    nextGeneration.Individuals.AddRange(front); // Add entire front
                }
                else
                {
                    // Step 4: Crowding distance sorting to pick the best
                    NonDominatedSorting.CalculateCrowdingDistance(front);
                    var sortedFront = front.OrderByDescending(ind => ind.CrowdingDistance).ToList();

                    int remainingSlots = population.Individuals.Count - nextGeneration.Individuals.Count;
                    nextGeneration.Individuals.AddRange(sortedFront.Take(remainingSlots));
                    break;
                }
            }

            return nextGeneration;
        }


    }
}