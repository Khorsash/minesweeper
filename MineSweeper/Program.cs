using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;

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
            // za zatvorenu celiju vracamo "▓▓"(2 razmazka)
            // zbog toga sto u blizu celiji ne moze biti vise 8 bomba
            // uvek dodamo " "(razmazak) ka broju bomba oko celiji
            // (inace "o^" za bombu)
            return isOpen ? (value >= 0 ? (value > 0 ? "0" + value.ToString() : "░░") : "o^") : (isFlagged ? "I*" : "▓▓");
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
                                ConsoleColor defColor = (ConsoleColor)(-1),
                                ConsoleColor selColor = ConsoleColor.Green)
        {
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
                        Console.ForegroundColor = selColor;
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
            if (board[y1, x1].isOpen) Console.WriteLine("Celija je vec otvorena");
        }

        // Kordinati se vracaju u HashSet
        // kordinati su u redu y,x u tuple-u
        // primer:
        // [][][][][]
        // [][][][][]
        // [][]ci[][]
        // [][][][][]
        // [][][][][]
        // zagradi su celije kordinati kojih se vracaju ako pozovemo NearCoordsRectangle(x, y, 2),
        // a "ci" - celija na kordinatama x, y
        // pri uslovu da cilj-celija je udalena od svih granica bar na 2 celije
        // mozemo da definisemo maxX i maxY za donji granice
        public static List<(int, int)> NearCoordsRectangle(int x0, int y0, int r, int maxX = 8, int maxY = 15)
        {
            List<(int, int)> nearcoords = new List<(int, int)>();
            for (int i = y0 - r; i < y0 + r + 1; i++)
            {
                for (int j = x0 - r; j < x0 + r + 1; j++)
                {
                    if (i < 0 || j < 0 || i > maxY || j > maxX || (i == y0 && j == x0)) continue;
                    // kordinati su u redu y,x u tuple-u
                    nearcoords.Add((i, j));
                }
            }
            return nearcoords;
        }
        static List<(int, int)> GenerateBoard(ref Cell[,] board, int x0, int y0, int bc = 14)
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
                    startzone.AddRange(cnb);
                    for (int i = 0; i < cnb.Count; i++)
                    {
                        // povecamo broj bomba u susednim celijama
                        // (ako nije bomba)
                        if (board[cnb[i].Item1, cnb[i].Item2].value != -1) board[cnb[i].Item1, cnb[i].Item2].value++;
                    }
                }
            }
            return bombsCreated;
            
            
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool gameRunning = true;
            bool boardIsGenerated = false;

            // za sad 9x16, ali kasnije to moze da se promeni
            // da se pita u korisnika
            // ili bude samo po default-u drugacije
            int r = 16; // redova
            int c = 16;  // kolona
            int x = c / 2;
            int y = r / 2 - 1;

            // kolicina bomba
            int bc = 14;

            Cell[,] board = new Cell[r, c];
            List<(int, int)> bombsCreated;
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
                DrawBoard(ref board, x, y);
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.W: case ConsoleKey.UpArrow:
                        y = y == 0 ? r - 1 : y - 1;
                        break;
                    case ConsoleKey.A: case ConsoleKey.LeftArrow:
                        x = x == 0 ? c - 1 : x - 1;
                        break;
                    case ConsoleKey.S: case ConsoleKey.DownArrow:
                        y = (y + 1) % r;
                        break;
                    case ConsoleKey.D: case ConsoleKey.RightArrow:
                        x = (x + 1) % c;
                        break;
                    case ConsoleKey.F:
                        if (!board[y, x].isOpen)
                        {
                            board[y, x].isFlagged = !board[y, x].isFlagged;
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (!board[y, x].isOpen && !boardIsGenerated)
                        {
                            bombsCreated = GenerateBoard(ref board, x, y, bc);
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

                        if (!board[y, x].isOpen && boardIsGenerated)
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
                                DrawBoard(ref board, x, y);
                                Console.WriteLine("Game Over");
                            }

                        }
                        break;
                }
            }


        }
    }
}