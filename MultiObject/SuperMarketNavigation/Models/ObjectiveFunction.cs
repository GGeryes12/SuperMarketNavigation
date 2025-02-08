using System;
using System.Collections.Generic;
using System.Drawing;

namespace SuperMarketNavigation.Models
{
    public class ObjectiveFunction
    {
        public static double foul = 10; //Time foul for each "cleaning isle" walking
        public static (double, double) walkIndiv(Individual indv, MarketLayout market, char orig, char targ)
        {
            double time = 0; // Keeps track of walking time
            double foul_time = 0; // Tracks "fouls" if touching undesired aisles

            // Find starting and target positions in the grid
            Point p1 = FindInstance(market.IsleMatrix, orig);
            Point p2 = FindInstance(market.IsleMatrix, targ);

            int curr_row = p1.X, curr_column = p1.Y;
            bool reached = false;

            while (!reached)
            {
                int moveH = 0, moveV = 0;

                // Determine movement based on the walking pattern
                switch (indv.wp)
                {
                    case Individual.WalkingPatter.V2H:
                        if (curr_row != p2.X)
                            moveV = curr_row < p2.X ? 1 : -1; // Move vertically
                        else
                            moveH = curr_column < p2.Y ? 1 : -1; // Move horizontally
                        break;

                    case Individual.WalkingPatter.H2V:
                        if (curr_column != p2.Y)
                            moveH = curr_column < p2.Y ? 1 : -1; // Move horizontally
                        else
                            moveV = curr_row < p2.X ? 1 : -1; // Move vertically
                        break;

                    case Individual.WalkingPatter.ZgZg:
                        int distRow = Math.Abs(p2.X - curr_row);
                        int distCol = Math.Abs(p2.Y - curr_column);

                        if (distRow > distCol)
                            moveV = curr_row < p2.X ? 1 : -1; // Prioritize vertical movement
                        else if (distCol > distRow)
                            moveH = curr_column < p2.Y ? 1 : -1; // Prioritize horizontal movement
                        else
                            moveH = curr_column < p2.Y ? 1 : -1; // If equal, move horizontally
                        break;
                }

                // Update current position
                curr_row += moveV;
                curr_column += moveH;

                // Check if the target is reached
                if (curr_row == p2.X && curr_column == p2.Y)
                {
                    reached = true;
                }

                // Increment time for each move
                time += 1;

                // Check if the individual "touches" undesired aisles
                char currentIsle = market.IsleMatrix[curr_row, curr_column];
                if (currentIsle == 'x')
                {
                    foul_time += foul; // Increment foul time for touching cleaning aisles
                }
            }

            // You can return just the walking time, or include foul_time as a penalty
            return (time, foul_time); // Adjust as needed
        }

        public static Point FindInstance(char[,] grid, char target)
        {
            // Loop through the rows and columns of the grid
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    // Check if the current cell matches the target letter
                    if (grid[row, col] == target)
                    {
                        return (new Point(row, col)); // Return the row and column of the first instance
                    }
                }
            }
            return new Point(-1, -1); // Return null if the letter is not found
        }
        public static double[] EvaluateIndiv(Individual individual, MarketLayout market)
        {
            List<double> itemsHS = new List<double>(); // Tracks heat sensitivity times for items
            double totalWalkingTime = 0;
            char prev = '<'; // Starting point (entrance)

            // Iterate over each item in the individual's path
            for (int i = 0; i <= individual.items.Length; i++)
            {
                double currWalkingTime = 0;
                double currFoul = 0;
                char currIsle;

                // Determine the current target aisle
                if (i < individual.items.Length)
                {
                    currIsle = individual.items[i].isle; // Move to next item aisle
                }
                else
                {
                    currIsle = '>'; // Move to cashier at the end
                }

                // Retrieve heat sensitivity for the current aisle
                if (!market.heatSensitivity.TryGetValue(currIsle, out int currHS))
                    throw new Exception($"Isle '{currIsle}' not found in Market layout!");

                // Calculate walking time and foul penalty from the previous aisle to the current one
                (currWalkingTime, currFoul) = walkIndiv(individual, market, prev, currIsle);

                // Update heat sensitivity exposure times for all previously picked items
                for (int j = 0; j < itemsHS.Count; j++)
                {
                    itemsHS[j] += currWalkingTime + currFoul;
                }

                // Add the heat sensitivity for the current aisle
                itemsHS.Add(currHS);

                // Update the total walking time
                totalWalkingTime += currWalkingTime;

                // Update the previous aisle to the current one
                prev = currIsle;
            }

            // Calculate total exposure damage (penalty for heat-sensitive items)
            double totalExpDamage = itemsHS.Where(exposureTime => exposureTime > 0).Sum();

            // Store the results in the individual for later use
            individual.Objectives = new double[] { totalWalkingTime, totalExpDamage };

            // Return the objectives as an array for easier processing
            return individual.Objectives;
        }
    }
        /*

       public static double EvaluateIndiv(Individual individual, MarketLayout market)
               {
                   List<double> itemsHS = new List<double>();
                   double total_walking_time = 0;
                   char prev = '<';// = FindInstance(market.IsleMatrix, '<');
                   for (int i = 0; i <= individual.items.Length; i++)
                   {
                       double curr_walking_time = 0;
                       char currIsle;
                       if (individual.items.Length < i)
                           currIsle = individual.items[i].isle; //Go to next isle
                       else
                           currIsle = '>'; //Go to cashier
                       if (!market.heatSensitivity.TryGetValue(currIsle, out int currHS))
                           throw new Exception("Isle not found in Market layout!");

                       curr_walking_time = walkIndiv(individual, market, prev, currIsle);

                       for (int j = 0; j < i; j++)
                       {
                           if (itemsHS[j] != -1)
                               itemsHS[j] += curr_walking_time;
                       }

                       itemsHS.Add(currHS);
                       total_walking_time += curr_walking_time;
                       prev = currIsle;
                   }

                       double totalFoul = 0;
                       foreach (double item in itemsHS)
                           if (item > 0)
                               totalFoul += item;
                       return total_walking_time + totalFoul;

               }



               // Delegate for the objective function
               public Func<double[], double[]> Evaluate { get; set; }

               // Constructor
               public ObjectiveFunction(Func<double[], double[]> evaluationFunc)
               {
                   Evaluate = evaluationFunc;
               }

               // Evaluate the objectives for a population
               public void EvaluatePopulation(Population population)
               {
                   foreach (var individual in population.Individuals)
                   {
                       //individual.Objectives = Evaluate(individual.Variables);
                   }
               }
               public double EvalIndiv(Individual individual))
               {
                   double score = 100;
                   List<
               }
               void walk(double distance)
               {
                   walkingDistance += distance;
                   for( int i = 0; i < items.Length; i++)
                   {
                       items[i].heatScore -= distance;
                   }
                   foreach(Item itm in items)
                   {
                       itm.heatScore-= distance;
                   }
               }
               public static double calcDistance(char point1, char point2)
               {
                   var position1 = FindInstance(IsleMatrix, point1);
                   var position2 = FindInstance(IsleMatrix, point2);
                   double[] hpat = new double[] { 0, 1 };
                   double euclideanDistance = CalculateEuclideanDistance(position1.Value, position2.Value);
                   return euclideanDistance;
               }
               public double foulTime((int row, int col) start, (int row, int col) end)
               {
                   double foul = 0;
                   List<char> intersects = GetIntersectedAisles(IsleMatrix, start, end);
                   foreach (char inters in intersects)
                   {
                       switch (inters)
                       {
                           case '0':
                               foul += 1;
                               break;
                           case 'x':
                               foul += 2;
                               break;
                           default:
                               foul += 1.5;
                               break;
                       }
                   }
                   return foul;
               }
               private static double CalculateEuclideanDistance((int row, int col) position1, (int row, int col) position2)
               {
                   return Math.Sqrt(Math.Pow(position2.row - position1.row, 2) + Math.Pow(position2.col - position1.col, 2));
               }

               public static List<char> GetIntersectedAisles(char[,] grid, (int row, int col) start, (int row, int col) end)
               {
                   var intersectedAisles = new List<char>();

                   int x0 = start.col, y0 = start.row;
                   int x1 = end.col, y1 = end.row;

                   int dx = Math.Abs(x1 - x0);
                   int dy = Math.Abs(y1 - y0);

                   int sx = x0 < x1 ? 1 : -1;
                   int sy = y0 < y1 ? 1 : -1;

                   int err = dx - dy;

                   while (true)
                   {
                       // Add the current cell to the intersected aisles list
                       if (grid[y0, x0] != '0') // Avoid empty spaces
                       {
                           if (!intersectedAisles.Contains(grid[y0, x0]))
                           {
                               intersectedAisles.Add(grid[y0, x0]);
                           }
                       }

                       // Check if we've reached the end point
                       if (x0 == x1 && y0 == y1) break;

                       // Update error and step
                       int e2 = 2 * err;
                       if (e2 > -dy)
                       {
                           err -= dy;
                           x0 += sx;
                       }
                       if (e2 < dx)
                       {
                           err += dx;
                           y0 += sy;
                       }
                   }

                   return intersectedAisles;
               }


               }

               */

    }
