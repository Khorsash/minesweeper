using System;
using System.Collections.Generic;


namespace MineSweeper
{
    struct Cell
    {
        // ako value == -1 -> bomba
        // inace kolicina bomba oko celiji
        public int value;
        public bool isOpen;
        public bool isFlagged;
        public Cell(int vl, bool status)
        {
            value = vl;
            isOpen = status;
            isFlagged = false;
        }
        public override string ToString()
        {
            // za zatvorenu celiju vracamo "▓▓"
            // a za zatvorenu sa flagom "|>"
            // zbog toga sto u blizu celiji ne moze biti vise 8 bomba
            // uvek dodamo "0" ka broju bomba oko celiji
            // (inace "o^" za bombu)
            return isOpen ? (value >= 0 ? (value > 0 ? " " + value.ToString() : "░░") : "o^") : (isFlagged ? "|>" : "▓▓");
        }
    }
    class Program
    {
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        public static void DrawBoard(ref Cell[,] board, int x1, int y1,
                                bool gameOver,
                                ConsoleColor defColor,
                                ConsoleColor selColor = ConsoleColor.Green,
                                ConsoleColor errColor = ConsoleColor.Red)
        {
            bool err = board[y1, x1].isOpen;
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
                    Console.Write("|");
                    if (y1 == i && x1 == j)
                    {
                        Console.ForegroundColor = err || board[y1, x1].isFlagged ? errColor : selColor;
                        Console.Write(board[i, j].ToString());
                        Console.ForegroundColor = defColor;
                        continue;
                    }
                    if (gameOver && board[i, j].value == -1)
                    {
                        Console.ForegroundColor = errColor;
                        Console.Write(board[i, j].ToString());
                        Console.ForegroundColor = defColor;
                        continue;
                    }
                    Console.Write(board[i, j].ToString());
                }
                Console.WriteLine("|");
            }

            // donji deo
            Console.Write("└");
            for (int i = 0; i < board.GetLength(1) * 3 - 1; i++) Console.Write("─");
            Console.WriteLine("┘");
            if (err && !gameOver) Console.WriteLine("Celija je vec otvorena");
        }

        static bool IsInside(int x1, int y1, int maxX = 8, int maxY = 15)
        {
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
        public static List<(int, int)> NearCoordsRectangle(int x0, int y0, int r, int maxX = 8, int maxY = 15)
        {
            List<(int, int)> nearcoords = new List<(int, int)>();
            for (int i = y0 - r; i < y0 + r + 1; i++)
            {
                for (int j = x0 - r; j < x0 + r + 1; j++)
                {
                    if (!IsInside(j, i, maxX, maxY) || (i == y0 && j == x0)) continue;
                    // kordinati su u redu y,x u tuple-u
                    nearcoords.Add((i, j));
                }
            }
            return nearcoords;
        }
        public static void FloodFill(ref Cell[,] board, int x0, int y0)
        {
            int maxX = board.GetLength(1) - 1;
            int maxY = board.GetLength(0) - 1;
            if (!IsInside(x0, y0, maxX, maxY)) return;
            (int, int)[] coordsToCheck = new (int, int)[4]
                                            { (y0 - 1, x0), (y0, x0 - 1), (y0 + 1, x0), (y0, x0 + 1) };
            for (int i = 0; i < 4; i++)
            {
                if (IsInside(coordsToCheck[i].Item2, coordsToCheck[i].Item1, maxX, maxY)
                    && !board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].isOpen)
                {
                    board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].isOpen = true;
                    if (board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].value == 0)
                    {
                        FloodFill(ref board, coordsToCheck[i].Item2, coordsToCheck[i].Item1);
                    }
                }
            }
        }
        public static void GenerateBoard(ref Cell[,] board, int x0, int y0, int bc = 14)
        {
            List<(int, int)> startzone = NearCoordsRectangle(x0, y0, 2, board.GetLength(1) - 1, board.GetLength(0) - 1);
            startzone.Add((y0, x0));
            Random r = new Random();
            // kordinati celija oko bombe
            List<(int, int)> cnb;
            int bombsNeeded = bc;
            List<(int, int)> bombsCreated = new List<(int, int)>();
            while (bombsCreated.Count < bombsNeeded)
            {
                // Item1 -> y
                // Item2 -> x
                (int, int) coords = (r.Next(board.GetLength(0)), r.Next(board.GetLength(1)));
                if (!startzone.Contains(coords) && !bombsCreated.Contains(coords))
                {
                    bombsCreated.Add(coords);
                    board[coords.Item1, coords.Item2].value = -1;
                    cnb = NearCoordsRectangle(coords.Item2, coords.Item1, 1, board.GetLength(1) - 1, board.GetLength(0) - 1);
                    // startzone.AddRange(cnb);
                    for (int i = 0; i < cnb.Count; i++)
                    {
                        // povecamo broj bomba u susednim celijama
                        // (ako nije bomba)
                        if (board[cnb[i].Item1, cnb[i].Item2].value != -1) board[cnb[i].Item1, cnb[i].Item2].value++;
                    }
                }
            }
            FloodFill(ref board, x0, y0);
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
                    board[i, j] = new Cell(0, false);
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
                        cntr += board[i, j].isOpen ? 1 : 0;
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
                        if (!board[y, x].isOpen)
                        {
                            board[y, x].isFlagged = !board[y, x].isFlagged;
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (!board[y, x].isOpen && !boardIsGenerated)
                        {
                            GenerateBoard(ref board, x, y, bc);
                            board[y, x].isOpen = true;
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

                        if (!board[y, x].isOpen
                                && !board[y, x].isFlagged
                                && boardIsGenerated)
                        {

                            if (board[y, x].value != -1)
                            {
                                board[y, x].isOpen = true;
                            }
                            else
                            {
                                // otvaramo svi celije
                                for (int i = 0; i < r; i++)
                                {
                                    for (int j = 0; j < c; j++)
                                    {
                                        board[i, j].isOpen = true;
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