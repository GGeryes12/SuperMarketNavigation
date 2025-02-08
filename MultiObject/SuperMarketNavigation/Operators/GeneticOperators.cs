using System;
using SuperMarketNavigation.Models;

namespace SuperMarketNavigation.Operators
{
    public static class GeneticOperators
    {
        public static Individual Order1Crossover(Individual parent1, Individual parent2, MarketLayout market)
        {
            Random random = new Random();
            int length = parent1.items.Length;

            // Step 1: Select random crossover segment
            int start = random.Next(0, length);
            int end = random.Next(start, length);

            // Step 2: Create an empty child array
            Item[] childItems = new Item[length];

            // Step 3: Copy the segment from Parent 1 into the child
            for (int i = start; i <= end; i++)
            {
                childItems[i] = new Item { isle = parent1.items[i].isle };
            }

            // Step 4: Fill remaining positions with items from Parent 2
            int currentIndex = 0;
            for (int i = 0; i < length; i++)
            {
                char isleFromParent2 = parent2.items[i].isle;

                // Skip if the item is already in the child
                if (childItems.Any(item => item != null && item.isle == isleFromParent2))
                {
                    continue;
                }

                // Find the next empty position in the child
                while (childItems[currentIndex] != null)
                {
                    currentIndex++;
                }

                // Add the item from Parent 2
                childItems[currentIndex] = new Item { isle = isleFromParent2 };
            }

            // Step 5: Create the child individual
            Individual child = new Individual(market)
            {
                items = childItems
            };

            // Assign a walking pattern (equal chance to inherit from either parent)
            child.wp = random.NextDouble() < 0.5 ? parent1.wp : parent2.wp;

            return child;
        }


        public static void Mutate(Individual individual, double mutationRate)
        {
            // Create a random number generator
            Random random = new Random();

            // Apply mutation with a chance determined by mutationRate
            if (random.NextDouble() < mutationRate)
            {
                //50\50 chance eathier mutate walking pattern or mutate isles sequince
                if (random.NextDouble() < 0.5)
                {
                    // Mutate the walking pattern
                    Array patterns = Enum.GetValues(typeof(Individual.WalkingPatter));
                    Individual.WalkingPatter newPattern;

                    do
                    {
                        newPattern = (Individual.WalkingPatter)patterns.GetValue(random.Next(patterns.Length));
                    } while (newPattern == individual.wp); // Ensure it's a new pattern

                    individual.wp = newPattern; // Assign the new walking pattern
                }
                else
                {
                    // Get the number of items (aisles) in the individual
                    int numItems = individual.items.Length;

                    // Select two random indices
                    int index1 = random.Next(0, numItems);
                    int index2 = random.Next(0, numItems);

                    // Ensure the two indices are not the same
                    while (index1 == index2)
                    {
                        index2 = random.Next(0, numItems);
                    }

                    // Swap the aisles at the two indices
                    Item temp = individual.items[index1];
                    individual.items[index1] = individual.items[index2];
                    individual.items[index2] = temp;
                }
            }
        }
    }
}
