using System;

namespace MineSweeper
{
    struct Cell
    {
        // ako value == -1 -> bomba
        // inace kolicina bomba oko celiji
        public int value;
        public bool isOpen;
        public Cell(int vl, bool status)
        {
            value = vl;
            isOpen = status;
            ToString();
        }
        public override string ToString()
        {
            // za zatvorenu celiju vracamo "▓▓"(2 razmazka)
            // zbog toga sto u blizu celiji ne moze biti vise 8 bomba
            // uvek dodamo " "(razmazak) ka broju bomba oko celiji
            // (inace "o^" za bombu)
            return isOpen ? (value >= 0 ? (value > 0 ? "0" + value.ToString() : "░░") : "o^") : "▓▓";
        }
    }
    class Program
    {
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
        }
        static void DrawBoard(Cell[,] board, int x1, int y1,
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
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // za sad 9x16, ali kasnije to moze da se promeni
            // da se pita u korisnika
            // ili bude samo po default-u drugacije
            int r = 16; // redova
            int c = 9;  // kolona
            int x = c / 2;
            int y = r / 2-1;

            Cell[,] board = new Cell[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    board[i, j] = new Cell(0, false);
                }
            }
            bool gameRunning = true;
            while (gameRunning)
            {
                ClearConsole();
                DrawBoard(board, x, y);
            }

            
        }
    }
}