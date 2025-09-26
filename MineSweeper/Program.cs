using System;
using System.Collections.Generic;


namespace MineSweeper
{
    struct Cell
    {
        // ako value == -1 -> bomba
        // inace kolicina bomba oko celiji
        public int value;
        public bool flagged;
        private bool opened;
        public Cell(int vl, bool sts)
        {
            value = vl;
            opened = sts;
            flagged = false;
        }
        public bool isOpened() { return opened; }
        public bool isBomb() { return value == -1; }
        public bool isFlagged() { return flagged; }
        public ConsoleColor getColor(bool isGameOver, bool isSelected,
                                        ConsoleColor selColor, ConsoleColor errColor, ConsoleColor defColor)
        {
            return isGameOver && isBomb() ? errColor : isSelected ? (opened || flagged ? errColor : selColor) : defColor;
        }
        public void Open() { opened = true; } 
        public void PlantBomb() { value = -1; }
        public void ChangeFlagState()
        {
            flagged = !flagged;
        }
        public override string ToString()
        {
            // za zatvorenu celiju vracamo "▓▓"
            // a za zatvorenu sa flagom "|>"
            // zbog toga sto u blizu celiji ne moze biti vise 8 bomba
            // uvek dodamo "0" ka broju bomba oko celiji
            // (inace "o^" za bombu)
            return opened ? (value >= 0 ? (value > 0 ? " " + value.ToString() : "░░") : "o^") : (flagged ? "|>" : "▓▓");
        }
    }
    class Program
    {
        static int[] ParseConsoleArgs(string[] args)
        {
            int[] values = new int[3] { 16, 9, 14 };
            int i = 0;
            int temp;
            if (args.Length > 0)
            {
                foreach (string el in args)
                {
                    if (int.TryParse(el, out temp))
                    {
                        values[i] = temp;
                        i++;
                    }
                }
            }
            return values;
        }
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        static void ExitMsg()
        {
            Console.WriteLine("(Pritisnite koje bilo dugme da biste izasli)");
            Console.ReadKey();
        }
        static Cell MtrxYXEl(ref Cell[,] mtrx, (int, int) coords)
        {
            return mtrx[coords.Item1, coords.Item2];
        }
        static void OpenBoard(ref Cell[,] board)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j].Open();
                }
            }
        }
        static bool IsWin(ref Cell[,] board, int bc)
        {
            int c = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    c += board[i, j].isOpened() ? 1 : 0;
                }
            }
            return c == board.GetLength(0) * board.GetLength(1) - bc;
        }
        static void DrawCell(Cell cell, ConsoleColor color, ConsoleColor defColor)
        {
            Console.Write("|");
            Console.ForegroundColor = color;
            Console.Write(cell.ToString());
            Console.ForegroundColor = defColor;
        }
        static void DrawOpeningClosing(int n, bool isOpening)
        {
            Console.Write(isOpening ? "┌" : "└");
            for (int i = 0; i < n * 3 - 1; i++) Console.Write("─");
            Console.WriteLine(isOpening ? "┐" : "┘");
        }

        static (int, int) RandomCoords(ref Cell[,] board, ref Random r)
        {
            return (r.Next(board.GetLength(0)), r.Next(board.GetLength(1)));
        }
        public static void DrawBoard(ref Cell[,] board, int x1, int y1,
                                bool isGameOver,
                                ConsoleColor defColor,
                                ConsoleColor selColor = ConsoleColor.Green,
                                ConsoleColor errColor = ConsoleColor.Red)
        {
            Cell chosenCell = board[y1, x1];
            ConsoleColor color;

            DrawOpeningClosing(board.GetLength(1), true);

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    color = board[i, j].getColor(isGameOver, i == y1 && j == x1,
                                                        selColor, errColor, defColor);
                    DrawCell(board[i, j], color, defColor);
                }
                Console.WriteLine("|");
            }

            DrawOpeningClosing(board.GetLength(1), false);

            if (chosenCell.isOpened() && !isGameOver) Console.WriteLine("Celija je vec otvorena");
        }

        static bool AreCoordsInside(ref Cell[,] board, (int, int) coords)
        {
            int y1 = coords.Item1;
            int x1 = coords.Item2;
            int maxY = board.GetLength(0);
            int maxX = board.GetLength(1);
            return !(x1 < 0 || y1 < 0 || x1 >= maxX || y1 >= maxY);
        }

        public static List<(int, int)> NearCoordsRectangle(ref Cell[,] board, (int, int) coords, int r)
        {
            int y0 = coords.Item1;
            int x0 = coords.Item2;
            List<(int, int)> nearcoords = new List<(int, int)>();
            for (int i = y0 - r; i < y0 + r + 1; i++)
            {
                for (int j = x0 - r; j < x0 + r + 1; j++)
                {
                    if (!AreCoordsInside(ref board, (i, j))) continue;
                    // kordinati su u redu y,x u tuple-u
                    nearcoords.Add((i, j));
                }
            }
            return nearcoords;
        }
        public static void FloodFill(ref Cell[,] board, (int, int) coords)
        {
            int y0 = coords.Item1;
            int x0 = coords.Item2;
            if (!AreCoordsInside(ref board, coords)) return;
            (int, int)[] coordsToCheck = new (int, int)[4] { (y0 - 1, x0), (y0, x0 - 1), (y0 + 1, x0), (y0, x0 + 1) };
            for (int i = 0; i < 4; i++)
            {
                if (!AreCoordsInside(ref board, coordsToCheck[i])) continue;
                Cell cell = MtrxYXEl(ref board, coordsToCheck[i]);
                if (!cell.isOpened())
                {
                    board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].Open();
                    if (cell.value == 0) FloodFill(ref board, coordsToCheck[i]);
                }
            }
        }
        public static void GenerateBoard(ref Cell[,] board, (int, int) coords0, int bc = 14)
        {
            List<(int, int)> adjToStartCells = NearCoordsRectangle(ref board, coords0, 1);
            List<(int, int)> bombsCreated = new List<(int, int)>();
            Random r = new Random();
            while (bombsCreated.Count < bc)
            {
                (int, int) coords1 = RandomCoords(ref board, ref r);
                if (!adjToStartCells.Contains(coords1) && !bombsCreated.Contains(coords1))
                {
                    bombsCreated.Add(coords1);
                    board[coords1.Item1, coords1.Item2].PlantBomb();
                    List<(int, int)> cnb = NearCoordsRectangle(ref board, coords1, 1);
                    for (int i = 0; i < cnb.Count; i++)
                    {
                        Cell cell = MtrxYXEl(ref board, cnb[i]);
                        if (!cell.isBomb()) board[cnb[i].Item1, cnb[i].Item2].value++;
                    }
                }
            }
            FloodFill(ref board, coords0);
        }
        static void MineSweeperGame(int r = 16, int c = 9, int bc = 14)
        { 
            bool gameRunning = true;
            bool boardIsGenerated = false;

            int x = c / 2;
            int y = r / 2 - 1;

            Cell[,] board = new Cell[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    board[i, j] = new Cell(0, false);
                }
            }
            while (gameRunning)
            {
                ClearConsole();
                DrawBoard(ref board, x, y, false, Console.ForegroundColor);
                if (IsWin(ref board, bc))
                {
                    Console.WriteLine("Pobeda!");
                    ExitMsg();
                    break;
                }
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        y = y == 0 ? r - 1 : y - 1;
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        x = x == 0 ? c - 1 : x - 1;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        y = (y + 1) % r;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        x = (x + 1) % c;
                        break;
                    // zbog preskakivanja s jedne na drugu tasteturu
                    // moze da ne radi "F"
                    // onda radi "7" ili PageDown
                    case ConsoleKey.F:
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                    case ConsoleKey.PageDown:
                        if (!board[y, x].isOpened())
                        {
                            board[y, x].ChangeFlagState();
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (board[y, x].isOpened()) break;
                        if (!boardIsGenerated)
                        {
                            GenerateBoard(ref board, (y, x), bc);
                            board[y, x].Open();
                            boardIsGenerated = true;

                            // za debug, otvara svi celije
                            // OpenBoard(ref board);

                            break;
                        }

                        if (!board[y, x].isOpened()
                                && !board[y, x].isFlagged())
                        {

                            if (board[y, x].isBomb())
                            {
                                // otvaramo svi celije
                                
                                gameRunning = false;
                                ClearConsole();
                                OpenBoard(ref board);
                                DrawBoard(ref board, x, y, true, Console.ForegroundColor);
                                Console.WriteLine("Game Over");
                                ExitMsg();
                            }
                            if (board[y, x].value != 0)
                            {
                                board[y, x].Open();
                            }
                            else
                            {
                                FloodFill(ref board, (y, x));
                            }
                            
                        }
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int[] values = ParseConsoleArgs(args);
            MineSweeperGame(values[0], values[1], values[2]);
        }
    }
}