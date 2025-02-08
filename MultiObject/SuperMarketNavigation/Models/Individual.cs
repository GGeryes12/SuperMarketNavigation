using System.Dynamic;

namespace SuperMarketNavigation.Models
{
    public class Individual
    {
        public enum WalkingPatter
        {
            V2H,
            H2V,
            ZgZg
        }
        public Item[] items { get; set; } // Permutation of aisles/letters
        public WalkingPatter wp { get; set; }
        // Array to store multiple objectives (e.g., walking time, exposure damage)
        public double[] Objectives { get; set; }

        // For NSGA-II: Pareto rank (lower is better, 1 = best front)
        public int Rank { get; set; }

        // For NSGA-II: Crowding distance to measure solution diversity
        public double CrowdingDistance { get; set; }

        // For NSGA-II: Domination count (number of solutions dominating this one)
        public int DominationCount { get; set; }

        // For NSGA-II: List of individuals dominated by this one
        public List<Individual> DominatedSet { get; set; }

        // Reference point for each individual
        public double[] ReferencePoint { get; set; }

        /// <summary>
        /// Constructor to create a random individual
        /// </summary>
        public Individual(MarketLayout market)
        {
            // Step 1: Extract all valid aisles (excluding '<' and '>')
            List<char> validAisles = new List<char>();
            foreach (var key in market.heatSensitivity.Keys)
            {
                if (key != '<' && key != '>') // Exclude entrance and cashier
                {
                    validAisles.Add(key);
                }
            }

            // Step 2: Shuffle the aisles to create a random sequence
            Random random = new Random();
            validAisles = validAisles.OrderBy(x => random.Next()).ToList();

            // Convert the shuffled aisles to an array of items
            items = validAisles.Select(aisle => new Item { isle = aisle }).ToArray();

            // Step 3: Assign a random walking pattern
            wp = (WalkingPatter)random.Next(Enum.GetValues(typeof(WalkingPatter)).Length);
            Objectives = new double[2]; // Two objectives: walking time, exposure time
            Rank = 0;
            CrowdingDistance = 0.0;
            DominationCount = 0;
            DominatedSet = new List<Individual>();
            ReferencePoint = null;
        }
        public override string ToString()
        {
            string itemSequence = string.Join(" -> ", items.Select(item => item.isle));
            return $"Walking Pattern: {wp}\nItems: {itemSequence}\nObjectives: [{Objectives[0]}, {Objectives[1]}]\nRank: {Rank}, Crowding Distance: {CrowdingDistance}";
        }
        public string GetSolutionKey()
        {
            return string.Join(",", items.Select(i => i.isle)) + $"_{wp}";
        }
    }
    public class Item
    {
        public char isle { get; set; }
    }
}