using SuperMarketNavigation.Models;
using SuperMarketNavigation.Sorting;

namespace SuperMarketNavigation.Algorithms
{
    public class NSGA3Algorithm : GeneticAlgorithm
    {
        private List<double[]> referencePoints;
        private string rawDataFilePath;

        public NSGA3Algorithm(MarketLayout market, int popSize, double mutationRate, string runPath, string rawDataFilePath) 
            : base(market, popSize, mutationRate, runPath)
        {
            referencePoints = GenerateReferencePoints(2, 12); // Example: 2 objectives, 12 divisions
            this.rawDataFilePath = rawDataFilePath;
        }

        protected override void SortPopulation()
        {
            // NSGA-III sorting implementation here
            var fronts = NonDominatedSorting.PerformSorting(population.Individuals);
            foreach (var front in fronts)
            {
                NonDominatedSorting.CalculateCrowdingDistance(front);
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
                    // Step 4: Reference point-based selection
                    AssignIndividualsToReferencePoints(front);
                    var selectedIndividuals = SelectIndividualsFromReferencePoints(front, population.Individuals.Count - nextGeneration.Individuals.Count);
                    nextGeneration.Individuals.AddRange(selectedIndividuals);
                    break;
                }
            }

            return nextGeneration;
        }

        private List<double[]> GenerateReferencePoints(int numObjectives, int divisions)
        {
            // Generate reference points for NSGA-III
            List<double[]> referencePoints = new List<double[]>();
            GenerateRecursive(referencePoints, new double[numObjectives], divisions, divisions, 0);
            return referencePoints;
        }

        private void GenerateRecursive(List<double[]> referencePoints, double[] point, int remaining, int total, int depth)
        {
            if (depth == point.Length - 1)
            {
                point[depth] = (double)remaining / total;
                referencePoints.Add((double[])point.Clone());
            }
            else
            {
                for (int i = 0; i <= remaining; i++)
                {
                    point[depth] = (double)i / total;
                    GenerateRecursive(referencePoints, point, remaining - i, total, depth + 1);
                }
            }
        }

        private void AssignIndividualsToReferencePoints(List<Individual> front)
        {
            foreach (var individual in front)
            {
                double minDistance = double.MaxValue;
                double[] closestReferencePoint = null;

                foreach (var referencePoint in referencePoints)
                {
                    double distance = CalculatePerpendicularDistance(individual.Objectives, referencePoint);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestReferencePoint = referencePoint;
                    }
                }

                individual.ReferencePoint = closestReferencePoint;
            }
        }

        private double CalculatePerpendicularDistance(double[] objectives, double[] referencePoint)
        {
            double dotProduct = 0;
            double referencePointMagnitude = 0;

            for (int i = 0; i < objectives.Length; i++)
            {
                dotProduct += objectives[i] * referencePoint[i];
                referencePointMagnitude += referencePoint[i] * referencePoint[i];
            }

            double projection = dotProduct / referencePointMagnitude;
            double distance = 0;

            for (int i = 0; i < objectives.Length; i++)
            {
                distance += Math.Pow(objectives[i] - projection * referencePoint[i], 2);
            }

            return Math.Sqrt(distance);
        }

        private List<Individual> SelectIndividualsFromReferencePoints(List<Individual> front, int remainingSlots)
        {
            List<Individual> selectedIndividuals = new List<Individual>();
            var referencePointGroups = front.GroupBy(ind => ind.ReferencePoint);

            foreach (var group in referencePointGroups)
            {
                var sortedGroup = group.OrderBy(ind => ind.CrowdingDistance).ToList();
                selectedIndividuals.AddRange(sortedGroup.Take(remainingSlots));
                remainingSlots -= sortedGroup.Count;

                if (remainingSlots <= 0)
                    break;
            }

            return selectedIndividuals;
        }
    }
}