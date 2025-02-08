using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using ScottPlot;

namespace SuperMarketNavigation.Models
{
    public class MarketLayout
    {
        public MarketLayout(int rows, int cols, double ratio)
        {
            IsleMatrix = new char[rows, cols];
            List<char> characters = new List<char>();

            // Add characters from 'A' to 'Z'
            for (char c = 'A'; c <= 'Z'; c++)
            {
                characters.Add(c);
            }

            // Shuffle the characters
            Random rand = new Random();
            for (int i = characters.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                char temp = characters[i];
                characters[i] = characters[j];
                characters[j] = temp;
            }

            // Place characters in the matrix
            int index = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (index < characters.Count)
                    {
                        IsleMatrix[i,j] = characters[index++];
                    }
                    else
                    {
                        IsleMatrix[i,j] = ' ';
                    }
                }
            }

            // Fill the remaining cells with 'X's and '0's according to the ratio
            int totalCells = rows * cols;
            int remainingCells = totalCells - characters.Count;
            int xCount = (int)(remainingCells * ratio);
            int zeroCount = remainingCells - xCount;

            List<char> fillers = new List<char>();
            for (int i = 0; i < xCount; i++) fillers.Add('X');
            for (int i = 0; i < zeroCount; i++) fillers.Add('0');

            // Shuffle the fillers
            for (int i = fillers.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                char temp = fillers[i];
                fillers[i] = fillers[j];
                fillers[j] = temp;
            }

            // Place fillers in the matrix
            index = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (IsleMatrix[i,j] == ' ')
                    {
                        IsleMatrix[i,j] = fillers[index++];
                    }
                }
            }

            // Insert '<' and '>' by swapping two '0's
            bool swapped = false;
            for (int i = 0; i < rows && !swapped; i++)
            {
                for (int j = 0; j < cols && !swapped; j++)
                {
                    if (IsleMatrix[i, j] == '0')
                    {
                        IsleMatrix[i, j] = '<';
                        swapped = true;
                    }
                }
            }

            swapped = false;
            for (int i = 0; i < rows && !swapped; i++)
            {
                for (int j = 0; j < cols && !swapped; j++)
                {
                    if (IsleMatrix[i, j] == '0')
                    {
                        IsleMatrix[i, j] = '>';
                        swapped = true;
                    }
                }
            }
        }
        /*
                public char[,] IsleMatrix = {
                {'<','0','G','0','0'},
                {'0','C','x','H','0'},
                {'F','0','B','0','E'},
                {'0','x','0','x','0'},
                {'A','I','D','0','>'}
            };
                public Dictionary<char, int> heatSensitivity = new Dictionary<char, int>(){
                        {'A',-1},
                        {'B',-1},
                        {'C',-1},
                        {'D',-1},
                        {'E',-1},
                        {'F',0},
                        {'G',0},
                        {'H',0},
                        {'I',0},
                        {'<',-1},
                        {'>',-1}
                    };
                    */
        /*  public char[,] IsleMatrix = {
      {'<','0','G','0','0','0','0','H','0','0'},
      {'0','C','x','H','0','E','0','x','I','0'},
      {'F','0','B','0','E','x','G','0','B','0'},
      {'0','x','0','x','0','I','0','x','0','0'},
      {'A','I','D','0','0','0','C','0','D','0'},
      {'0','0','0','x','0','0','x','0','E','0'},
      {'J','K','x','0','0','F','0','0','0','0'},
      {'0','x','0','x','0','L','x','H','0','M'},
      {'N','O','P','0','x','0','Q','0','0','0'},
      {'0','0','R','S','T','U','V','W','X','>'}
  };*/
        public char[,] IsleMatrix; /*= {
        {'<','0','G','0','0','0','0','H','x','x','0','F','0','0','0'},
        {'0','C','x','H','0','E','0','x','I','0','A','x','x','B','0'},
        {'F','0','M','0','x','x','G','0','P','0','0','D','0','0','I'},
        {'0','x','0','x','0','I','0','x','0','0','C','x','E','x','0'},
        {'0','I','D','0','0','0','C','0','D','0','x','x','x','x','x'},
        {'0','0','0','x','0','0','x','0','E','0','0','0','K','0','0'},
        {'J','K','x','0','0','F','0','0','0','0','0','0','0','G','x'},
        {'0','x','0','x','0','L','x','0','0','M','N','O','0','0','P'},
        {'N','O','P','0','x','0','Q','0','0','0','0','0','x','0','0'},
        {'0','0','0','0','0','0','0','0','0','0','x','x','x','x','x'},
        {'0','0','R','S','T','U','V','W','X','0','0','0','D','F','0'},
        {'0','x','0','x','x','x','0','0','0','0','0','G','0','C','0'},
        {'x','x','x','x','0','0','0','0','0','0','0','x','x','x','0'},
        {'0','B','0','0','x','0','J','0','K','0','L','M','x','O','0'},
        {'0','0','0','0','0','0','0','0','0','0','0','0','x','>','x'}
    };*/
        public Dictionary<char, int> heatSensitivity = new Dictionary<char, int>()
{
    {'A', -1}, {'B', -1}, {'C', -1}, {'D', -1}, {'E', -1},
    {'F',  0}, {'G',  0}, {'H',  0}, {'I',  0}, {'J',  0},
    {'K', -1}, {'L',  0}, {'M',  0}, {'N', -1}, {'O', -1},
    {'P', -1}, {'Q',  0}, {'R', -1}, {'S', -1}, {'T',  0},
    {'U',  0}, {'V', -1}, {'W',  0}, {'X',  0}, {'<', -1},
    {'>', -1}
};
        public void VisualizeMarket(string runFolderPath)
        {
            ScottPlot.Plot myPlot = new();

            int rows = IsleMatrix.GetLength(dimension: 0);
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
                        case 'x': // Hot aisle
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
                }
            }

            // Customize plot appearance
            myPlot.Title("Supermarket Layout Visualization");
            myPlot.XLabel("Aisles (X-axis)");
            myPlot.YLabel("Aisles (Y-axis)");
            myPlot.Axes.AutoScale();

            // Save the plot as an image
            myPlot.SavePng(runFolderPath+"/market_layout.png", 800, 600);

            Console.WriteLine("Market layout saved as 'market_layout.png'.");
        }
    }
}