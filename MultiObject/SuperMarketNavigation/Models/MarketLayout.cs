using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SuperMarketNavigation.Models
{
    public class MarketLayout
    {
        public char[,] IsleMatrix;
        private int rows;
        private int cols;

        public MarketLayout(int rows, int cols, double ratio)
        {
            this.rows = rows;
            this.cols = cols;
            IsleMatrix = new char[rows, cols];
            Random rand = new Random();
            List<char> characters = new List<char>();

            // Add characters from 'A' to 'Z'
            for (char c = 'A'; c <= 'Z'; c++)
            {
                characters.Add(c);
            }

            // Shuffle the characters list
            characters = characters.OrderBy(_ => rand.Next()).ToList();

            // Track used positions to prevent overwriting
            HashSet<(int, int)> usedPositions = new HashSet<(int, int)>();

            // Randomly place letters in the grid
            foreach (char c in characters)
            {
                int x, y;
                do
                {
                    x = rand.Next(rows);
                    y = rand.Next(cols);
                } while (usedPositions.Contains((x, y)));

                IsleMatrix[x, y] = c;
                usedPositions.Add((x, y));
            }

            // Determine the remaining empty cells
            List<(int, int)> emptyCells = new List<(int, int)>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (IsleMatrix[i, j] == '\0')
                    {
                        emptyCells.Add((i, j));
                    }
                }
            }

            // Shuffle empty cells
            emptyCells = emptyCells.OrderBy(_ => rand.Next()).ToList();

            // Determine the number of 'x' and '0' based on ratio
            int xCount = (int)(emptyCells.Count * ratio);
            int zeroCount = emptyCells.Count - xCount;

            // Assign 'x' and '0' in the remaining spaces
            for (int i = 0; i < emptyCells.Count; i++)
            {
                (int x, int y) = emptyCells[i];
                IsleMatrix[x, y] = i < xCount ? 'x' : '0';
            }

            // Randomly place start '<' and exit '>' in existing '0' locations
            List<(int, int)> zeroCells = emptyCells.Where(p => IsleMatrix[p.Item1, p.Item2] == '0').ToList();
            if (zeroCells.Count >= 2)
            {
                IsleMatrix[zeroCells[0].Item1, zeroCells[0].Item2] = '<';
                IsleMatrix[zeroCells[1].Item1, zeroCells[1].Item2] = '>';
            }
        }

        public Dictionary<char, int> heatSensitivity = new Dictionary<char, int>()
        {
            {'A', -1}, {'B', -1}, {'C', -1}, {'D', -1}, {'E', -1},
            {'F', -1}, {'G', -1}, {'H', -1}, {'I', -1}, {'J', -1},
            {'K', -1}, {'L',  0}, {'M',  0}, {'N', -1}, {'O', -1},
            {'P', -1}, {'Q',  0}, {'R', -1}, {'S', -1}, {'T',  0},
            {'U',  0}, {'V', -1}, {'W',  0}, {'X',  0}, {'Y',  0},
            {'Z', -1}, {'<',  -1}, {'>', -1}
        };

        public void VisualizeMarket(string runFolderPath)
        {
            ScottPlot.Plot myPlot = new();

            int rows = IsleMatrix.GetLength(0);
            int cols = IsleMatrix.GetLength(1);

            // Define colors for different aisle types
            Color emptyColor = Colors.LightGray;
            Color hotColor = Colors.Red.WithAlpha(0.5);
            Color itemGreenColor = Colors.Green.WithAlpha(0.8);
            Color itemBlueColor = Colors.Blue.WithAlpha(0.8);
            Color startEndColor = Colors.Purple;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    char cell = IsleMatrix[i, j];
                    Coordinates location = new(j, -i);  // Flip Y-axis for correct visualization
                    CoordinateSize size = new(1, 1);
                    CoordinateRect rect = new(location, size);

                    var rectangle = myPlot.Add.Rectangle(rect);

                    switch (cell)
                    {
                        case '0': // Empty aisle
                            rectangle.FillStyle.Color = emptyColor;
                            rectangle.LineStyle.Color = Colors.Black;
                            break;
                        case 'x': // Hot aisle (🔥 should be RED)
                            rectangle.FillStyle.Color = hotColor;
                            rectangle.LineStyle.Color = Colors.DarkRed;
                            break;
                        case '<': // Start
                        case '>': // End
                            rectangle.FillStyle.Color = startEndColor;
                            rectangle.LineStyle.Color = Colors.Navy;
                            rectangle.LineStyle.Width = 2;
                            break;
                        default: // Regular aisle with items
                            if (heatSensitivity.ContainsKey(cell))
                            {
                                rectangle.FillStyle.Color = heatSensitivity[cell] == -1 ? itemGreenColor : itemBlueColor;
                            }
                            else
                            {
                                rectangle.FillStyle.Color = itemGreenColor;
                            }
                            rectangle.LineStyle.Color = Colors.DarkGreen;
                            break;
                    }

                    // Add text inside the rectangle
                    var text = myPlot.Add.Text(cell.ToString(), j + 0.5, -i + 0.5);
                    text.Alignment = Alignment.MiddleCenter;
                    text.LabelFontSize = 10;
                    text.LabelFontColor = Colors.Black;
                }
            }

            // Customize plot appearance
            myPlot.Title("Supermarket Layout Visualization");
            myPlot.XLabel("Aisles (X-axis)");
            myPlot.YLabel("Aisles (Y-axis)");
            myPlot.Axes.AutoScale();

            // Save the plot as an image
            string filePath = System.IO.Path.Combine(runFolderPath, "market_layout.png");
            myPlot.SavePng(filePath, 800, 600);

            Console.WriteLine($"Market layout saved as '{filePath}'.");
        }

        public void SaveLayout(string filePath)
        {
            var data = new
            {
                Rows = rows,
                Cols = cols,
                IsleMatrix = IsleMatrix.Cast<char>().ToArray() // Flatten for serialization
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Market layout saved to {filePath}");
        }

        public static MarketLayout LoadLayout(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: Layout file not found!");
                return null;
            }

            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<MarketLayoutData>(json);

            MarketLayout market = new MarketLayout(data.Rows, data.Cols, 0); // Use existing dimensions
            market.IsleMatrix = new char[data.Rows, data.Cols];

            // Restore the IsleMatrix from the flattened array
            for (int i = 0; i < data.Rows; i++)
            {
                for (int j = 0; j < data.Cols; j++)
                {
                    market.IsleMatrix[i, j] = data.IsleMatrix[i * data.Cols + j];
                }
            }

            Console.WriteLine($"Market layout loaded from {filePath}");
            return market;
        }

        // Helper class for serialization
        private class MarketLayoutData
        {
            public int Rows { get; set; }
            public int Cols { get; set; }
            public char[] IsleMatrix { get; set; }
        }

        public void PrintMarket()
        {
            for (int i = 0; i < IsleMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < IsleMatrix.GetLength(1); j++)
                {
                    Console.Write(IsleMatrix[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}