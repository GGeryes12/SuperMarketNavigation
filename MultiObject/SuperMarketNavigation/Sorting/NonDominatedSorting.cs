using System.Collections.Generic;
using System.Linq;
using SuperMarketNavigation.Models;

namespace SuperMarketNavigation.Sorting
{
    public static class NonDominatedSorting
    {
        public static bool Dominates(Individual p, Individual q)
        {
            bool atLeastOneBetter = false;

            for (int i = 0; i < p.Objectives.Length; i++)
            {
                if (p.Objectives[i] > q.Objectives[i])  // Minimization problem
                    return false;  // p does not dominate q

                if (p.Objectives[i] < q.Objectives[i])
                    atLeastOneBetter = true; // p is better in at least one objective
            }

            return atLeastOneBetter;
        }


        public static void CalculateCrowdingDistance(List<Individual> front)
        {
            int numObjectives = front[0].Objectives.Length;

            foreach (var individual in front)
                individual.CrowdingDistance = 0;

            for (int m = 0; m < numObjectives; m++)
            {
                front = front.OrderBy(i => i.Objectives[m]).ToList();
                front[0].CrowdingDistance = double.PositiveInfinity;
                front[^1].CrowdingDistance = double.PositiveInfinity;

                for (int i = 1; i < front.Count - 1; i++)
                {
                    front[i].CrowdingDistance += (front[i + 1].Objectives[m] - front[i - 1].Objectives[m]) /
                                                 (front[^1].Objectives[m] - front[0].Objectives[m]);
                }
            }
        }

        public static List<List<Individual>> PerformSorting(List<Individual> population)
        {
            var fronts = new List<List<Individual>>();

            foreach (var p in population)
            {
                p.DominatedSet = new List<Individual>();
                p.DominationCount = 0;

                foreach (var q in population)
                {
                    if (Dominates(p, q))
                    {
                        p.DominatedSet.Add(q);
                    }
                    else if (Dominates(q, p))
                    {
                        p.DominationCount++;
                    }
                }

                if (p.DominationCount == 0)
                {
                    p.Rank = 1;
                    if (fronts.Count == 0) fronts.Add(new List<Individual>());
                    fronts[0].Add(p);
                }
            }

            int currentFront = 0;
            while (fronts.Count > currentFront && fronts[currentFront].Count > 0)
            {
                var nextFront = new List<Individual>();
                foreach (var p in fronts[currentFront])
                {
                    foreach (var q in p.DominatedSet)
                    {
                        q.DominationCount--;
                        if (q.DominationCount == 0)
                        {
                            q.Rank = currentFront + 2;
                            nextFront.Add(q);
                        }
                    }
                }
                if (nextFront.Count > 0) fronts.Add(nextFront);
                currentFront++;
            }

            return fronts;
        }

        public static List<double[]> GenerateReferencePoints(int numObjectives, int divisions)
        {
            // Generate reference points for NSGA-III
            List<double[]> referencePoints = new List<double[]>();
            // Implementation of reference point generation
            // ...
            return referencePoints;
        }

        public static void AssignIndividualsToReferencePoints(List<Individual> front, List<double[]> referencePoints)
        {
            // Assign individuals to reference points
            // Implementation of assignment
            // ...
        }

        public static List<Individual> SelectIndividualsFromReferencePoints(List<Individual> front, List<double[]> referencePoints, int remainingSlots)
        {
            // Select individuals from reference points
            List<Individual> selectedIndividuals = new List<Individual>();
            // Implementation of selection
            // ...
            return selectedIndividuals;
        }
    }
}
