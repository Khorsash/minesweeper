using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using ConsoleMenu;

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
        // koristim ANSI kod da bih izbrisao kes konsoli, 
        // jer Console.Clear(); izbrisa samo vidljiv kes 
        // jer ako ostaje nevidljivi kes, na scroll mozemo 
        // da vidimo ostatak kesa(koji je bio nevidljiv)
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        // Funkcija koja crta pole igrice
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
                    // ako izabrana celija
                    if (y1 == i && x1 == j)
                    {
                        // crtamo nju obojenu
                        Console.ForegroundColor = err || board[y1, x1].isFlagged ? errColor : selColor;
                        Console.Write(board[i, j].ToString());
                        Console.ForegroundColor = defColor;
                        continue;
                    }
                    // ako su bombe otvorene onda su nacrtani bojom "errColor"
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
        // Funkcija koja vraca true ili false u zavisnosti da li je celija unutra pravougaonika
        // s levim gornjim uglom 0,0 i s desnim donjim maxX, maxY
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
           zagradi su celije kordinati kojih se vracaju ako pozovemo NearCoordsRectangle(x, y, r:2),
           a "ci" - celija na kordinatama x, y
           pri uslovu da cilj-celija je udalena od svih granica bar na r=2 celije
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
        // Funkcija koja otvara svi prazni celije pocinjaci od prve izabrane celije
        // (tj. do cifara)
        public static void FloodFill(ref Cell[,] board, int x0, int y0)
        {
            int maxX = board.GetLength(1) - 1;
            int maxY = board.GetLength(0) - 1;
            // ako nije unutra polja izadjemo iz funkciji
            if (!IsInside(x0, y0, maxX, maxY)) return;
            // kordinati gornje, donje, desne i leve celija od celije x0,y0
            // (kordinati su u obrnutom redu, odnosno y,x)
            (int, int)[] coordsToCheck = new (int, int)[4]
                                            { (y0 - 1, x0), (y0, x0 - 1), (y0 + 1, x0), (y0, x0 + 1) };
            for (int i = 0; i < 4; i++)
            {
                // ako je unutra polja i nije otvorena
                if (IsInside(coordsToCheck[i].Item2, coordsToCheck[i].Item1, maxX, maxY)
                    && !board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].isOpen)
                {
                    // otvaramo nju
                    board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].isOpen = true;
                    // ako nije cifra onda idemo dalje
                    // pa pozovemo FloodFill opet
                    // da bi ona novi celije otvarala
                    if (board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].value == 0)
                    {
                        FloodFill(ref board, coordsToCheck[i].Item2, coordsToCheck[i].Item1);
                    }
                }
            }
        }
        // Funkcija koja kreira polje 
        public static void GenerateBoard(ref Cell[,] board, int x0, int y0, int bc = 14)
        {
            // Pocetna zona oko prve izabrane celije
            // u kojoj se ne kreiraju bombe
            List<(int, int)> startzone = NearCoordsRectangle(x0, y0, 2, board.GetLength(1) - 1, board.GetLength(0) - 1);
            startzone.Add((y0, x0));
            Random r = new Random();
            // kordinati celija oko bombe
            List<(int, int)> cnb;
            // kolicina bomba
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
        static void MineSweeperGame(int r = 16, int c = 9, int bc = 14,
                                        ConsoleColor selColor = ConsoleColor.Green,
                                        ConsoleColor errColor = ConsoleColor.Red)
        { 
            bool gameRunning = true;
            bool boardIsGenerated = false;

            int x = c / 2;
            int y = r / 2 - 1;

            // brojac otvorenih celija koje nisu bombe
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
                DrawBoard(ref board, x, y, false, Console.ForegroundColor, selColor, errColor);
                cntr = 0;
                // brojanje otvorenih celija koje nisu bombe
                for (int i = 0; i < r; i++)
                {
                    for (int j = 0; j < c; j++)
                    {
                        cntr += board[i, j].isOpen ? 1 : 0;
                    }
                }
                // ako kol. svih celija - kol. bomba = kol. otvorenih celija koje nisu bombe
                // onda smo pobedili
                if (cntr == r * c - bc)
                {
                    Console.WriteLine("Pobeda!");
                    Console.WriteLine("(Pritisnite koje bilo dugme da biste izasli)");
                    Console.ReadKey();
                    break;
                }
                // provera korisnickog unosa
                switch (Console.ReadKey().Key)
                {
                    // provere za biranje celije
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
                    // potvrdjenje izbora
                    case ConsoleKey.Enter:
                        // ako jos nije polje generisano
                        // (ali izabrana prva celija)
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
                            // gameRunning = false;
                            // Console.WriteLine("Game Over");
                            // Console.WriteLine("(Pritisnite koje bilo dugme da biste izasli)");
                            // Console.ReadKey();
                        }
                        // ako nije izabrana celija otvorena
                        // i nije s flagom
                        if (!board[y, x].isOpen
                                && !board[y, x].isFlagged
                                && boardIsGenerated)
                        {
                            // ako nije bomba jednostavno otvaramo
                            if (board[y, x].value != -1)
                            {
                                board[y, x].isOpen = true;
                            }
                            // inace gubimo
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
                                DrawBoard(ref board, x, y, true, Console.ForegroundColor, selColor, errColor);
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
            // da bismo mogli izvesti "▓▓"
            // moramo da izvodimo u UTF8 formatu
            // nego u ASCII
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // sve boje
            int[] allColors = new int[16];
            for (int i = 0; i < 16; i++) allColors[i] = i;

            // opcije meni
            string[] menuOptions = new string[4] {"Nova igra", "Promeniti tezinu", "Podesavnja", "Izlaz"};

            // podesavanja tezine igrice
            Dictionary<string, SettingOption> gameMode = new Dictionary<string, SettingOption>();
            // podesavanja igrice
            Dictionary<string, SettingOption> settings = new Dictionary<string, SettingOption>();

            // dimenzije polja
            Dictionary<string, (int, int)> sizes = new Dictionary<string, (int, int)>();
            // kolicina bomba na razliciti dimenzije polja
            Dictionary<string, int> bombCount = new Dictionary<string, int>();

            // kontrolira balans
            // ako vece, onda manje bomba
            // ako manje, onda vise bomba
            int k = 7; 

            sizes["9x9"] = (9, 9);
            sizes["9x16"] = (16, 9);
            sizes["16x16"] = (16, 16);
            sizes["16x30"] = (30, 16);
            sizes["30x16"] = (16, 30);
            sizes["30x30"] = (30, 30);
            bombCount["9x9"] = 9*9/k;
            bombCount["9x16"] = 9*16/k;
            bombCount["16x16"] = 16*16/k;
            bombCount["16x30"] = 16*30/k;
            bombCount["30x16"] = 16*30/k;
            bombCount["30x30"] = 30*30/k;
            
            // podesavanja tezine igrice
            gameMode["Dimenzije"] = new StringOption(sizes.Keys.ToArray(), 1);
            gameMode["Tezina"] = new IntRangeOption(1, 3, 1);

            // podesavanja igrice
            settings["Boja izabrane celije"] = new ColorOption(allColors, 10); // po default-u zeleni
            settings["Boja pogresne celije"] = new ColorOption(allColors, 12); // po default-u crveni

            bool running = true;
            while (running)
            {
                string ch = Menu.MenuShow(Menu.Paginate(menuOptions, 4), 0, "Minesweeper 0.1");
                switch (ch)
                {
                    case "Nova igra":
                        string dmns = gameMode["Dimenzije"].ToString() ?? "9x16";
                        (int, int) size = sizes[dmns];
                        int bc = ((IntRangeOption)gameMode["Tezina"]).Value * bombCount[dmns];
                        MineSweeperGame(size.Item1, size.Item2, bc,
                                            ((ColorOption)settings["Boja izabrane celije"]).GetColor(),
                                            ((ColorOption)settings["Boja pogresne celije"]).GetColor());
                        ClearConsole();
                        break;
                    case "Promeniti tezinu":
                        Menu.ChangeSettings(gameMode);
                        ClearConsole();
                        break;
                    case "Podesavnja":
                        Menu.ChangeSettings(settings);
                        ClearConsole();
                        break;
                    case "Izlaz":
                        running = false;
                        ClearConsole();
                        break;
                }
            }
        }
    }
}