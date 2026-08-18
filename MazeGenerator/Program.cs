using System;
using System.Collections.Specialized;
using System.IO;

namespace MazeGenerator
{
    internal class Program
    {
        //whole maze column count
        static int wmColumnCount;
        //whole maze row count
        static int wmRowCount;
        //large maze column count
        static int lmColumnCount;
        //large maze row count
        static int lmRowCount;
        //size of sub-grid
        static int gridSizeCount;

        //grids
        static int[,] wholeGrid = { };
        static int[,] largeGrid = { };
        static int[,] pathfinderGrid = { };

        //subgrids
        static int[,] pathGridUp = { };
        static int[,] pathGridRight = { };
        static int[,] pathGridDown = { };
        static int[,] pathGridLeft = { };

        //pathfinding variables
        static bool targetMet = false;
        static int counter = 0;
        static int counterTarget = 3;

        //display variables
        static bool reset = false;
        static bool pathGridShown = false;

        static void Main(string[] args)
        {
            //allows for looping
            while (true)
            {
                RandomMazeGenerator();
            }
        }

        /// <summary>
        /// Creates a randomised maze that is completable.
        /// </summary>
        static void RandomMazeGenerator()
        {
            //find maze variables
            SetLargeGridLength();

            //finds size of whole grid and initialises array
            wmColumnCount = lmColumnCount * gridSizeCount;
            wmRowCount = lmRowCount * gridSizeCount;
            wholeGrid = new int[wmRowCount, wmColumnCount];

            //clears entire console (not just screen console)
            Console.Clear();
            Console.WriteLine("\x1b[3J");

            int resetCounter = 0;
            //repeats until a path is found
            do
            {
                LargeGridPopulation();
                WholeGridPopulation();

                //rudimentary pathfinding technique
                PathfindingChecker();

                //prevents infinite pathfinding
                resetCounter++;
                if (resetCounter == 100)
                {
                    targetMet = true;
                    Console.WriteLine("No Path could be found through maze. Incomplete maze generated.\n");
                }
            } while (!targetMet);

            //reset for loop
            targetMet = false;

            DrawFinalGrid(wholeGrid);

            do
            {
                string? s = Console.ReadLine();
                AfterGridActions(s);
            } while (!reset);
            reset = false;
        }

        /// <summary>
        /// Checks for player input as for what to do next.
        /// </summary>
        /// <param name="s"></param>
        private static void AfterGridActions(string? s)
        {
            //clears entire console (not just screen console)
            Console.Clear();
            Console.WriteLine("\x1b[3J");

            try
            {
                if (s == "e")
                    //exits code without error
                    Environment.Exit(0);
                else if (s == "n")
                {
                    //displays pathfinding grid (if not shown)
                    if (pathGridShown)
                    {
                        DrawFinalGrid(wholeGrid);
                    }
                    else
                    {
                        DrawFinalGrid(pathfinderGrid);
                    }

                    pathGridShown = !pathGridShown;
                }
                else
                    //allows for loop
                    reset = true;
            }
            catch (Exception ex) { Environment.Exit(-1); /*exits with error code*/ }
        }

        /// <summary>
        /// Sets length of the large wall/path grid
        /// </summary>
        static void SetLargeGridLength()
        {
            try
            {
                //set large row and column counts
                Console.Write("How many columns in the large grid: ");
                lmColumnCount = Convert.ToInt32(Console.ReadLine());
                Console.Write("How many rows in the large grid: ");
                lmRowCount = Convert.ToInt32(Console.ReadLine());

                //sets minimum amount to prevent errors
                if (lmRowCount < 3)
                {
                    lmRowCount = 3;
                    Console.WriteLine("Minimum column number is 3. Column number set to 3.");
                    Thread.Sleep(3000);
                }
                if (lmColumnCount < 3)
                {
                    lmColumnCount = 3;
                    Console.WriteLine("Minimum row number is 3. Row number set to 3.");
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured. \nDefault values set to 10.");
                Thread.Sleep(3000);

                lmColumnCount = 10; lmRowCount = 10;
            }

            Console.WriteLine();

            //initialises array and calls subgrid
            largeGrid = new int[lmRowCount, lmColumnCount];
            SetSubGridLength();
        }

        /// <summary>
        /// Finds pattern and size of subgrid from file
        /// </summary>
        static void SetSubGridLength()
        {
            try
            {
                //find file and number of lines
                string filePath = Path.GetFullPath("Pattern.txt");
                StreamReader sr = new StreamReader(filePath);
                int lineNos = File.ReadLines(filePath).Count();

                string[] subGrid = new string[lineNos];
                int lineNo = 0;
                //line is nullable
                string? line = sr.ReadLine();

                while (line != null)
                {
                    //adds lines to subgrid for splitting
                    subGrid[lineNo] = line;
                    lineNo++;
                    line = sr.ReadLine();
                }
                sr.Close();

                //splits subgrid into temporary array
                string[] temp = subGrid[0].Split(',');
                int subColumnCount = temp.Length;
                for (int i = 1; i < subGrid.Length; i++)
                {
                    temp = temp.Concat(subGrid[i].Split(',')).ToArray();
                }
                int subRowCount = temp.Length / subColumnCount;

                if (subColumnCount != subRowCount)
                {
                    //exits with error code
                    Console.WriteLine("Pattern not square. Request cannot be completed.");
                    Environment.Exit(-1);
                }
                else
                {
                    gridSizeCount = subColumnCount;
                }

                //pathgrid initialised and populated
                pathGridUp = new int[gridSizeCount, gridSizeCount];
                pathGridRight = new int[gridSizeCount, gridSizeCount];
                pathGridDown = new int[gridSizeCount, gridSizeCount];
                pathGridLeft = new int[gridSizeCount, gridSizeCount];

                int tempCount = 0;
                for (int c = 0; c < gridSizeCount; c++)
                {
                    for (int r = 0; r < gridSizeCount; r++)
                    {
                        pathGridUp[c, r] = Convert.ToInt32(temp[tempCount]);
                        tempCount++;
                    }
                }
            }
            catch 
            { 
                //exits with error code
                Console.WriteLine("Could not read Sub-File properly. Exiting...");
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Creates the large grid with the predetermined size
        /// </summary>
        static void LargeGridPopulation()
        {
            //generate grid
            for (int c = 0; c < lmRowCount; c++)
            {
                for (int r = 0; r < lmColumnCount; r++)
                {
                    Random rnd = new Random();
                    largeGrid[c, r] = rnd.Next(0, 2);
                }
            }

            //refine grid
            bool retry = true;
            while (retry)
            {
                retry = false;
                for (int c = 0; c < lmRowCount; c++)
                {
                    for (int r = 0; r < lmColumnCount; r++)
                    {
                        if (largeGrid[c, r] == 1)
                        {
                            CheckArea(c, r, retry);
                        }
                    }
                }
            }

            PlaceGoals();
        }

        /// <summary>
        /// Refines grid by adding paths or adding walls
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <param name="retry"></param>
        static void CheckArea(int c, int r, bool retry)
        {
            //checks for walls
            int wallCounter = 0;
            int wallTarget = 3;

            //checks for paths
            int pathCounter = 0;
            int pathTarget = 4;

            if (c != 0)
            {
                if (largeGrid[c - 1, r] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }
            else {
                if (largeGrid[lmRowCount - 1, r] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }

            if (c != lmRowCount - 1)
            {
                if (largeGrid[c + 1, r] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }
            else {
                if (largeGrid[0, r] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }

            if (r != 0)
            {
                if (largeGrid[c, r - 1] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }
            else {
                if (largeGrid[c, lmColumnCount - 1] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }

            if (r != lmColumnCount - 1)
            {
                if (largeGrid[c, r + 1] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }
            else {
                if (largeGrid[c, 0] == 0)
                {
                    wallCounter++;
                }
                else { pathCounter++; }
            }

            if (wallCounter >= wallTarget)
            {
                retry = true;

                Random rnd = new Random();
                int wallChanger = rnd.Next(0, 4);

                switch (wallChanger)
                {
                    case 0:
                        //up
                        if (c != 0)
                            largeGrid[c - 1, r] = 1;
                        else
                            largeGrid[lmRowCount - 1, r] = 1;
                        break;
                    case 1:
                        //right
                        if (r != lmColumnCount - 1)
                            largeGrid[c, r + 1] = 1;
                        else
                            largeGrid[c, 0] = 1;
                        break;
                    case 2:
                        //down
                        if (c != lmRowCount - 1)
                            largeGrid[c + 1, r] = 1;
                        else
                            largeGrid[0, r] = 1;
                        break;
                    case 3:
                        //left
                        if (r != 0)
                            largeGrid[c, r - 1] = 1;
                        else
                            largeGrid[c, lmColumnCount - 1] = 1;
                        break;
                }
            }

            if (pathCounter >= pathTarget)
            {
                retry = true;

                Random rnd = new Random();
                int pathChanger = rnd.Next(0, pathTarget);

                switch (pathChanger)
                {
                    case 0:
                        //up
                        if (c != 0)
                            largeGrid[c - 1, r] = 0;
                        else
                            largeGrid[lmRowCount - 1, r] = 0;
                        break;
                    case 1:
                        //right
                        if (r != lmColumnCount - 1)
                            largeGrid[c, r + 1] = 0;
                        else
                            largeGrid[c, 0] = 0;
                        break;
                    case 2:
                        //down
                        if (c != lmRowCount - 1)
                            largeGrid[c + 1, r] = 0;
                        else
                            largeGrid[0, r] = 0;
                        break;
                    case 3:
                        //left
                        if (r != 0)
                            largeGrid[c, r - 1] = 0;
                        else
                            largeGrid[c, lmColumnCount - 1] = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// Adds goals and a key to the map in a random location
        /// </summary>
        static void PlaceGoals()
        {
            Random rnd = new Random();
            bool retry = false;

            //adds a beginning and end (2)
            for (int i = 0; i < 2; i++)
            {
                do
                {
                    retry = false;
                    int x = rnd.Next(0, lmRowCount); int y = rnd.Next(0, lmColumnCount);
                    if (largeGrid[x, y] == 1)
                        largeGrid[x, y] = 2;
                    else
                        retry = true;
                } while (retry);
            }

            //adds a key (3)
            do
            {
                retry = false;
                int x = rnd.Next(0, lmRowCount); int y = rnd.Next(0, lmColumnCount);
                if (largeGrid[x, y] == 1)
                    largeGrid[x, y] = 3;
                else
                    retry = true;
            } while (retry);
        }

        /// <summary>
        /// Applies random subgrids to paths and magnifying key areas/walls
        /// </summary>
        static void WholeGridPopulation()
        {
            PopulateGridRotations();

            //draw wholeGrid
            for (int i = 0; i < wmRowCount; i += gridSizeCount)
            {
                for (int j = 0; j < wmColumnCount; j += gridSizeCount)
                {
                    //pick random wall design
                    int[,] choice = new int[gridSizeCount, gridSizeCount];

                    Random rnd = new Random();
                    switch (rnd.Next(0, 4))
                    {
                        case 0:
                            choice = pathGridUp;
                            break;
                        case 1:
                            choice = pathGridRight;
                            break;
                        case 2:
                            choice = pathGridDown;
                            break;
                        case 3:
                            choice = pathGridLeft;
                            break;
                    }

                    for (int c = 0; c < gridSizeCount; c++)
                    {
                        for (int r = 0; r < gridSizeCount; r++)
                        {
                            if (largeGrid[i / gridSizeCount, j / gridSizeCount] == 1)
                                wholeGrid[i + c, j + r] = choice[c, r];
                            else if (largeGrid[i / gridSizeCount, j / gridSizeCount] == 2)
                                wholeGrid[i + c, j + r] = 2;
                            else if (largeGrid[i / gridSizeCount, j / gridSizeCount] == 3)
                                wholeGrid[i + c, j + r] = 3;
                            else
                                wholeGrid[i + c, j + r] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies rotations to subgrid file for randomness
        /// </summary>
        static void PopulateGridRotations()
        {
            //PathGridUp already initialised

            //PathGridDown
            for (int c = 0; c < gridSizeCount; c++)
            {
                for (int r = 0; r < gridSizeCount; r++)
                {
                    pathGridDown[c, r] = pathGridUp[gridSizeCount - 1 - c, gridSizeCount - 1 - r];
                }
            }

            //PathGridLeft
            for (int r = 0; r < gridSizeCount; r++)
            {
                for (int c = 0; c < gridSizeCount; c++)
                {
                    pathGridLeft[c, gridSizeCount - 1 - r] = pathGridUp[r, c];
                }
            }

            //PathGridRight
            for (int c = 0; c < gridSizeCount; c++)
            {
                for (int r = 0; r < gridSizeCount; r++)
                {
                    pathGridRight[c, r] = pathGridLeft[gridSizeCount - 1 - c, gridSizeCount - 1 - r];
                }
            }
        }

        /// <summary>
        /// Draws grid with colours
        /// </summary>
        static void DrawFinalGrid(int[,] grid)
        {
            for (int c = 0; c < wmRowCount; c++)
            {
                for (int r = 0; r < wmColumnCount; r++)
                {
                    switch(grid[c, r])
                    {
                        case 0:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.BackgroundColor = ConsoleColor.Red;
                            break;
                        case 1:
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.White;
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.BackgroundColor = ConsoleColor.Green;
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            break;
                        case 9:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.BackgroundColor = ConsoleColor.Blue;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                    }
                    Console.Write(grid[c, r] + " ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Finds starting point and calls first instance of CoordinateTraker()
        /// </summary>
        static void PathfindingChecker()
        {
            //reset for when looping
            counter = 0;
            counterTarget = 3 * (int)MathF.Pow(gridSizeCount, 2);

            pathfinderGrid = new int[wmRowCount, wmColumnCount];

            //find beginning and start recursion
            for (int c = 0; c < wmRowCount; c++)
            {
                for (int r = 0; r < wmColumnCount; r++)
                {
                    if (wholeGrid[c, r] == 2)
                    {
                        CoordinateTracker(c, r, true);

                        //add pathfinding to original grid
                        for (int col = 0; col < wmRowCount; col++)
                        {
                            for (int row = 0; row < wmColumnCount; row++)
                            {
                                if (wholeGrid[col, row] == 1)
                                {
                                    if (pathfinderGrid[col, row] == 1)
                                        pathfinderGrid[col, row] = 9;
                                    else 
                                        pathfinderGrid[col, row] = 1;
                                }
                            }
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Recursively runs through grid to check for completable path
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="firstTime"></param>
        /// <returns></returns>
        static bool CoordinateTracker(int column, int row, bool firstTime = false)
        {
            //allows quick exits if target met
            if (targetMet)
                return true;

            //added to prevent errors when checking for goals
            if (firstTime)
            {
                pathfinderGrid[column, row] = 2;
                counter++;
            }
            else
            {
                pathfinderGrid[column, row] = 1;
            }

            //check for goals
            if (wholeGrid[column, row] > 1 && pathfinderGrid[column, row] == 1)
            {
                counter++;
                pathfinderGrid[column, row] = wholeGrid[column, row];
            }

            if (counter == counterTarget)
            {
                targetMet = true;
                return true;
            }

            //check around current coordinate
            if (column != 0)
            {
                if (wholeGrid[column - 1, row] != 0 && pathfinderGrid[column - 1, row] == 0)
                {
                    targetMet = CoordinateTracker(column - 1, row);
                }
            }
            else
            {
                if (wholeGrid[wmRowCount - 1, row] != 0 && pathfinderGrid[wmRowCount - 1, row] == 0)
                {
                    targetMet = CoordinateTracker(wmRowCount - 1, row);
                }
            }

            if (targetMet)
                return true;

            //check around current coordinate
            if (column != wmRowCount - 1)
            {
                if (wholeGrid[column + 1, row] != 0 && pathfinderGrid[column + 1, row] == 0)
                {
                    targetMet = CoordinateTracker(column + 1, row);
                }
            }
            else
            {
                if (wholeGrid[0, row] != 0 && pathfinderGrid[0, row] == 0)
                {
                    targetMet = CoordinateTracker(0, row);
                }
            }

            if (targetMet)
                return true;

            //check around current coordinate
            if (row != 0)
            {
                if (wholeGrid[column, row - 1] != 0 && pathfinderGrid[column, row - 1] == 0)
                {
                    targetMet = CoordinateTracker(column, row - 1);
                }
            }
            else
            {
                if (wholeGrid[column, wmColumnCount - 1] != 0 && pathfinderGrid[column, wmColumnCount - 1] == 0)
                {
                    targetMet = CoordinateTracker(column, wmColumnCount - 1);
                }
            }

            if (targetMet)
                return true;

            //check around current coordinate
            if (row != wmColumnCount - 1)
            {
                if (wholeGrid[column, row + 1] != 0 && pathfinderGrid[column, row + 1] == 0)
                {
                    targetMet = CoordinateTracker(column, row + 1);
                }
            }
            else
            {
                if (wholeGrid[column, 0] != 0 && pathfinderGrid[column, 0] == 0)
                {
                    targetMet = CoordinateTracker(column, 0);
                }
            }

            return targetMet;
        }
    }
}
