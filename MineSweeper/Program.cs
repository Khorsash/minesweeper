using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;


namespace MineSweeper
{
    struct Cell
    {
        // ako value == -1 -> bomba
        // inace kolicina bomba oko celiji
        public int value;
        public bool isFlagged;
        private bool opened;
        private (int, int) coords;
        public Cell(int vl, bool sts, (int, int) cCoords)
        {
            value = vl;
            opened = sts;
            isFlagged = false;
            coords = cCoords;
        }

        public bool isOpened() { return opened; }
        public bool isBomb() { return value == -1; }
        public void Open() { opened = true; } 
        public void PlantBomb() { value = -1; }
        public bool IsSameWith(Cell c1)
        {
            return coords == c1.coords;
        }
        public override string ToString()
        {
            // za zatvorenu celiju vracamo "▓▓"
            // a za zatvorenu sa flagom "|>"
            // zbog toga sto u blizu celiji ne moze biti vise 8 bomba
            // uvek dodamo "0" ka broju bomba oko celiji
            // (inace "o^" za bombu)
            return opened ? (value >= 0 ? (value > 0 ? " " + value.ToString() : "░░") : "o^") : (isFlagged ? "|>" : "▓▓");
        }
    }
    class Program
    {
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        /// <summary>
        ///     Funkcija koja vraca celiju na kordinatama X, Y
        ///     X, Y dobijamo u vidu tuple(y,x)
        /// </summary>
        static Cell MtrxYXEl(ref Cell[,] mtrx, (int, int) coords)
        {
            return mtrx[coords.Item1, coords.Item2];
        }
        static void DrawCell(Cell cell, ConsoleColor color, ConsoleColor defColor)
        {
            Console.Write("|");
            Console.ForegroundColor = color;
            Console.Write(cell.ToString());
        }
        public static void DrawBoard(ref Cell[,] board, int x1, int y1,
                                bool gameOver,
                                ConsoleColor defColor,
                                ConsoleColor selColor = ConsoleColor.Green,
                                ConsoleColor errColor = ConsoleColor.Red)
        {
            Cell chosenCell = board[y1, x1];
            ConsoleColor color;
            // gornji deo
            // (u celiji je 2 karaktera)
            Console.Write("┌");
            for (int i = 0; i < board.GetLength(1) * 3 - 1; i++) Console.Write("─");
            Console.WriteLine("┐");

            // srednji deo
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    color = (gameOver && board[i, j].isBomb())
                                ? errColor
                                : (board[i, j].IsSameWith(chosenCell)
                                    ? (chosenCell.isOpened() || chosenCell.isFlagged ? errColor : selColor)
                                    : defColor);
                    DrawCell(board[i, j], color, defColor);
                }
                Console.WriteLine("|");
            }

            // donji deo
            Console.Write("└");
            for (int i = 0; i < board.GetLength(1) * 3 - 1; i++) Console.Write("─");
            Console.WriteLine("┘");
            if (chosenCell.isOpened() && !gameOver) Console.WriteLine("Celija je vec otvorena");
        }

        static bool IsCellInside(ref Cell[,] board, (int, int) coords)
        {
            int y1 = coords.Item1;
            int x1 = coords.Item2;
            int maxY = board.GetLength(0);
            int maxX = board.GetLength(1);
            return !(x1 < 0 || y1 < 0 || x1 > maxX || y1 > maxY);
        }

        /* Kordinati se vracaju u HashSet
           kordinati su u redu y,x u tuple-u
           primer:
           [][][][][]
           [][][][][]
           [][]ci[][]
           [][][][][]
           [][][][][]
           zagradi su celije kordinati kojih se vracaju ako pozovemo NearCoordsRectangle(x, y, 2),
           a "ci" - celija na kordinatama x, y
           pri uslovu da cilj-celija je udalena od svih granica bar na 2 celije
           mozemo da definisemo maxX i maxY za donji granice */
        public static List<(int, int)> NearCoordsRectangle(ref Cell[,] board, (int, int) coords, int r)
        {
            int y0 = coords.Item1;
            int x0 = coords.Item2;
            List<(int, int)> nearcoords = new List<(int, int)>();
            for (int i = y0 - r; i < y0 + r + 1; i++)
            {
                for (int j = x0 - r; j < x0 + r + 1; j++)
                {
                    if (!IsCellInside(ref board, (i,j))) continue;
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
            if (!IsCellInside(ref board, (y0, x0))) return;
            (int, int)[] coordsToCheck = new (int, int)[4] { (y0 - 1, x0), (y0, x0 - 1), (y0 + 1, x0), (y0, x0 + 1) };
            for (int i = 0; i < 4; i++)
            {
                Cell c = MtrxYXEl(ref board, coordsToCheck[i]);
                if (IsCellInside(ref board, coordsToCheck[i]) && !c.isOpened())
                {
                    c.Open();
                    if (c.value == 0) FloodFill(ref board, coordsToCheck[i]);
                }
            }
        }
        public static void GenerateBoard(ref Cell[,] board, (int, int) coords0, int bc = 14)
        {
            List<(int, int)> startzone = NearCoordsRectangle(ref board, coords0, 2);
            List<(int, int)> bombsCreated = new List<(int, int)>();
            Random r = new Random();
            while (bombsCreated.Count < bc)
            {
                (int, int) coords1 = (r.Next(board.GetLength(0)), r.Next(board.GetLength(1)));
                if (!startzone.Contains(coords1) && !bombsCreated.Contains(coords1))
                {
                    bombsCreated.Add(coords1);
                    MtrxYXEl(ref board, coords1).PlantBomb();
                    List<(int, int)> cnb = NearCoordsRectangle(ref board, coords1, 1);
                    for (int i = 0; i < cnb.Count; i++)
                    {
                        // povecamo broj bomba u susednim celijama
                        // (ako nije bomba)
                        Cell c = MtrxYXEl(ref board, cnb[i]);
                        if (c.isBomb()) c.value++;
                    }
                }
            }
            FloodFill(ref board, coords0);
        }
        // za sad 9x16, ali kasnije to moze da se promeni
        // da se pita u korisnika
        // ili bude samo po default-u drugacije
        static void MineSweeperGame(int r = 16, int c = 9, int bc = 14)
        { 
            bool gameRunning = true;
            bool boardIsGenerated = false;

            int x = c / 2;
            int y = r / 2 - 1;
            int cntr;

            Cell[,] board = new Cell[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    board[i, j] = new Cell(0, false, (i, j));
                }
            }
            while (gameRunning)
            {
                ClearConsole();
                DrawBoard(ref board, x, y, false, Console.ForegroundColor);
                cntr = 0;
                for (int i = 0; i < r; i++)
                {
                    for (int j = 0; j < c; j++)
                    {
                        cntr += board[i, j].isOpened() ? 1 : 0;
                    }
                }
                if (cntr == r * c - bc)
                {
                    Console.WriteLine("Pobeda!");
                    Console.WriteLine("(Pritisnite koje bilo dugme da biste izasli)");
                    Console.ReadKey();
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
                            board[y, x].isFlagged = !board[y, x].isFlagged;
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (!board[y, x].isOpened() && !boardIsGenerated)
                        {
                            GenerateBoard(ref board, (y, x), bc);
                            board[y, x].Open();
                            boardIsGenerated = true;

                            // za debug, otvara svi celije

                            // for (int i = 0; i < r; i++)
                            // {
                            //     for (int j = 0; j < c; j++)
                            //     {
                            //         board[i, j].isOpen = true;
                            //     }
                            // }
                        }

                        if (!board[y, x].isOpened()
                                && !board[y, x].isFlagged
                                && boardIsGenerated)
                        {

                            if (board[y, x].isBomb())
                            {
                                board[y, x].Open();
                            }
                            else
                            {
                                // otvaramo svi celije
                                for (int i = 0; i < r; i++)
                                {
                                    for (int j = 0; j < c; j++)
                                    {
                                        board[i, j].Open();
                                    }
                                }
                                gameRunning = false;
                                ClearConsole();
                                DrawBoard(ref board, x, y, true, Console.ForegroundColor);
                                Console.WriteLine("Game Over");
                                Console.WriteLine("(Pritisnite koje bilo dugme da biste izasli)");
                                Console.ReadKey();
                            }
                        }
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            MineSweeperGame(16, 9, 24);
        }
    }
}