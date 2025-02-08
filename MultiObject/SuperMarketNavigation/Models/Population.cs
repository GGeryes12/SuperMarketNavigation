using System;
using System.Collections.Generic;

namespace SuperMarketNavigation.Models
{
    public class Population
    {
        public List<Individual> Individuals { get; set; }
        public MarketLayout market { get; set; }
        public Population()
        {
            Individuals = new List<Individual>();
        }

        public void Initialize(int popSize, MarketLayout market)
        {
            this.market = market;
            //for (int i = 0; i < popSize; i++)
            //{
            //    Individuals.Add(new Individual(market));
            //}
            while (Individuals.Count < popSize)
            {
                Individual tempInd = new Individual(market);
                var evaluation = ObjectiveFunction.EvaluateIndiv(tempInd, market);
                // Ensure no individuals with zero objectives are added
                if (evaluation[0] > 0 || evaluation[1] > 0)
                {
                    Individuals.Add(tempInd);
                }
            }
        }
        /*public (double[], double[]) EvaluatePopulation()
        {
            double[] walkDist = new double[Individuals.Count];
            double[] ExposFoul = new double[Individuals.Count];
                for (int i = 0; i < Individuals.Count; i ++)
                {
                    var res = ObjectiveFunction.EvaluateIndiv(Individuals[i], market);
                    walkDist[i] = res.Item1;
                    ExposFoul[i] = res.Item2;
                }
                return(walkDist, ExposFoul);
        }
        */
        public void Combine(Population other)
        {
            Individuals.AddRange(other.Individuals);
        }

        public void SelectTopIndividuals(int count)
        {
            Individuals = Individuals.GetRange(0, Math.Min(count, Individuals.Count));
        }
        // Tournament Selection
        public Individual TournamentSelection()
        {
            Random random = new Random();
            Individual ind1 = Individuals[random.Next(Individuals.Count)];
            Individual ind2 = Individuals[random.Next(Individuals.Count)];

            return (ind1.Rank < ind2.Rank ||
                    (ind1.Rank == ind2.Rank && ind1.CrowdingDistance > ind2.CrowdingDistance)) ? ind1 : ind2;
        }
    }
}