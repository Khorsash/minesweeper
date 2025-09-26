using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleMenu;
using LanguageUtils;

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
        static void SetBackground(ref Dictionary<string, SettingOption> settings)
        {
            Console.BackgroundColor = ((ColorOption)settings["backgroundcolor"]).GetColor();
        }
        static void GameParameterSetup(ref Dictionary<string, SettingOption> gameMode,
                                        ref Dictionary<string, (int, int)> sizes,
                                        ref Dictionary<string, int> bombCount)
        {
            sizes["9x9"] = (9, 9);
            sizes["9x16"] = (16, 9);
            sizes["16x16"] = (16, 16);
            sizes["16x30"] = (30, 16);
            sizes["30x16"] = (16, 30);
            sizes["30x30"] = (30, 30);
            bombCount["9x9"] = 9 * 9;
            bombCount["9x16"] = 9 * 16;
            bombCount["16x16"] = 16 * 16;
            bombCount["16x30"] = 16 * 30;
            bombCount["30x16"] = 16 * 30;
            bombCount["30x30"] = 30 * 30;

            // podesavanja tezine igrice
            gameMode["size"] = new StringOption("", sizes.Keys.ToArray(), 1);
            gameMode["difficulty"] = new IntRangeOption("", 1, 3, 1);
        }
        static string[] GetMenuOptions(ref Dictionary<string, Dictionary<string, string>> lngs,
                                                                                    string clng)
        {
            return new string[4] {Languages.GetTranslate(ref lngs, clng, "newgame"),
                                  Languages.GetTranslate(ref lngs, clng, "changedifficulty"),
                                  Languages.GetTranslate(ref lngs, clng, "settings"),
                                  Languages.GetTranslate(ref lngs, clng, "exit")};
        }
        static void GetSettingsTranslate(ref Dictionary<string, SettingOption> settings,
                                          ref Dictionary<string, Dictionary<string, string>> lngs,
                                                                                        string clng)
        {
            foreach (string key in settings.Keys.ToArray())
            {
                settings[key].Name = Languages.GetTranslate(ref lngs, clng, key);
            }
        }
        static Dictionary<string, SettingOption> LoadSettings(string settingsFilePath)
        {
            int[] allColors = new int[16];
            for (int i = 0; i < 16; i++) allColors[i] = i;
            Dictionary<string, SettingOption> settings = new Dictionary<string, SettingOption>();
            string txt = File.ReadAllText(settingsFilePath, System.Text.Encoding.UTF8);
            string[] txtl = txt.Split(";");
            for (int i = 0; i < txtl.Length; i++) SettingOption.ParseAndAddSetting(ref settings, txtl[i]);
            return settings;
        }
        static void SaveSettings(string settingsFilePath, Dictionary<string, SettingOption> settings)
        {
            string sttngsTxt = "";
            foreach (string k in settings.Keys.ToArray())
            {
                sttngsTxt += settings[k].Format(k) + ";";
            }
            File.WriteAllText(settingsFilePath, sttngsTxt.Substring(0, sttngsTxt.Length-1));
        }
        static Dictionary<string, SettingOption> DefaultSettings(ref Dictionary<string, Dictionary<string, string>> lngs,
                                                                                    string clng)
        {
            int[] allColors = ColorOption.AllColors();
            Dictionary<string, SettingOption> settings = new Dictionary<string, SettingOption>();
            settings["gameselectcolor"] = new ColorOption("", allColors, 10); // po default-u zeleni
            settings["gameerrorcolor"] = new ColorOption("", allColors, 12); // po default-u crveni
            settings["baseforeground"] = new ColorOption("", allColors, 7); // po default-u sivi
            settings["menuselectcolor"] = new ColorOption("", allColors, 10); // po default-u zeleni
            settings["backgroundcolor"] = new ColorOption("", allColors, 0); // po default-u crni
            settings["language"] = new StringOption("", lngs.Keys.ToArray(), Array.IndexOf(lngs.Keys.ToArray(), clng));
            return settings;
        }
        // koristim ANSI kod da bih izbrisao kes konsoli, 
        // jer Console.Clear(); izbrisa samo vidljiv kes 
        // jer ako ostaje nevidljivi kes, na scroll mozemo 
        // da vidimo ostatak kesa(koji je bio nevidljiv)
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        static void ExitMsg(ref Dictionary<string, Dictionary<string, string>> lngs,
                                                                                    string clng)
        {
            Console.WriteLine(Languages.GetTranslate(ref lngs, clng, "exitmessage"));
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
        static void SuccessfullyFoundBombCount(ref Cell[,] board, out int sfbc, out int nab)
        {
            int counter = 0;
            int counter1 = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (!board[i, j].isFlagged()) continue;
                    if (board[i, j].isBomb()) counter++;
                    else counter1++;
                }
            }
            sfbc = counter;
            nab = counter1;
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
                                (ConsoleColor, ConsoleColor) colors = default,
                                string cellisopenedmsg = "")
        {
            ConsoleColor selColor = colors.Item1;
            ConsoleColor errColor = colors.Item2;

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

            if (chosenCell.isOpened() && !isGameOver) Console.WriteLine(cellisopenedmsg);
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
                if (!cell.isOpened() && !cell.isBomb())
                {
                    board[coordsToCheck[i].Item1, coordsToCheck[i].Item2].Open();
                    if (cell.value == 0) FloodFill(ref board, coordsToCheck[i]);
                }
            }
        }
        public static void GenerateBoard(ref Cell[,] board, (int, int) coords0, int bc = 14, bool addStartZone = false)
        {
            // List<(int, int)> adjToStartCells = NearCoordsRectangle(ref board, coords0, 1);
            List<(int, int)> bombsCreated = new List<(int, int)>();
            Random r = new Random();
            while (bombsCreated.Count < bc)
            {
                (int, int) coords1 = RandomCoords(ref board, ref r);
                if (coords1 != coords0 && !bombsCreated.Contains(coords1))
                {
                    // && (addStartZone ? adjToStartCells.Contains(coords1) : true)
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
        static void MineSweeperGame(ref Dictionary<string, Dictionary<string, string>> lngs, string clng,
                                     int r = 16, int c = 9, int bc = 14, (ConsoleColor, ConsoleColor) colors = default)
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
                DrawBoard(ref board, x, y, false, Console.ForegroundColor, colors,
                            Languages.GetTranslate(ref lngs, clng, "cellalreadyopened"));
                if (IsWin(ref board, bc))
                {
                    Console.WriteLine(Languages.GetTranslate(ref lngs, clng, "win"));
                    ExitMsg(ref lngs, clng);
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
                        if (!board[y, x].isOpened())
                        {
                            board[y, x].ChangeFlagState();
                        }
                        break;
                    // potvrdjenje izbora
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
                                int sfbc;
                                int nab;
                                SuccessfullyFoundBombCount(ref board, out sfbc, out nab);
                                gameRunning = false;
                                ClearConsole();
                                OpenBoard(ref board);
                                DrawBoard(ref board, x, y, true, Console.ForegroundColor, colors,
                                            Languages.GetTranslate(ref lngs, clng, "cellalreadyopened"));
                                Console.WriteLine("Game Over");
                                Console.WriteLine(Languages.GetTranslate(ref lngs, clng, "youfound") + sfbc.ToString() +
                                                                            Languages.GetTranslate(ref lngs, clng, "bombs"));
                                Console.WriteLine(Languages.GetTranslate(ref lngs, clng, "youmissed") + nab.ToString() +
                                                                            Languages.GetTranslate(ref lngs, clng, "bombs"));
                                ExitMsg(ref lngs, clng);
                            }
                            else
                            {
                                board[y, x].Open();
                                if (board[y, x].value == 0) FloodFill(ref board, (y, x));
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

            const string VERSION = "0.1.1";

            string settingsPath = "settings.txt";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string languagePackPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "translates.json");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            string currlng = Languages.defaultLanguage;

            (ConsoleColor, ConsoleColor) gameColors;
            (ConsoleColor, ConsoleColor) menuColors;
            ConsoleColor defaultBackgrounColor = Console.BackgroundColor;
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;

            // opcije meni

            // podesavanja tezine igrice
            Dictionary<string, SettingOption> gameMode = new Dictionary<string, SettingOption>();
            // podesavanja igrice
            Dictionary<string, SettingOption> settings;
            Dictionary<string, Dictionary<string, string>> dict = Languages.Load(languagePackPath);

            string[] menuOptions = new string[4];

            // dimenzije polja
            Dictionary<string, (int, int)> sizes = new Dictionary<string, (int, int)>();
            // kolicina bomba na razliciti dimenzije polja
            Dictionary<string, int> bombCount = new Dictionary<string, int>();

            try
            {
                settings = LoadSettings(settingsPath);
                currlng = settings["language"].ToString() ?? currlng;
            }
            catch
            {
                settings = DefaultSettings(ref dict, currlng);
                SaveSettings(settingsPath, settings);
            }

            // kontrolira balans
            // ako vece, onda manje bomba
            // ako manje, onda vise bomba
            int k = 9;

            GameParameterSetup(ref gameMode, ref sizes, ref bombCount);

            menuOptions = GetMenuOptions(ref dict, currlng);
            GetSettingsTranslate(ref settings, ref dict, currlng);
            GetSettingsTranslate(ref gameMode, ref dict, currlng);


            bool running = true;
            while (running)
            {
                menuColors = (((ColorOption)settings["baseforeground"]).GetColor(),
                                ((ColorOption)settings["menuselectcolor"]).GetColor());
                gameColors = (((ColorOption)settings["gameselectcolor"]).GetColor(),
                                ((ColorOption)settings["gameerrorcolor"]).GetColor());
                string ch = Menu.ShowMenu(Menu.Paginate(menuOptions, menuOptions.Length), 0, "Minesweeper " + VERSION, menuColors);
                switch (Array.IndexOf(menuOptions, ch))
                {
                    // game
                    case 0:
                        string dmns = gameMode["size"].ToString() ?? "9x16";
                        (int, int) size = sizes[dmns];
                        int bc = bombCount[dmns] / (k - ((IntRangeOption)gameMode["difficulty"]).Value);
                        MineSweeperGame(ref dict, currlng,
                                            size.Item1, size.Item2, bc, gameColors);
                        ClearConsole();
                        break;
                    // gameMode settings
                    case 1:
                        Settings.ChangeSettings(gameMode, menuColors);
                        ClearConsole();
                        break;
                    // settings
                    case 2:
                        Settings.ChangeSettings(settings, menuColors);
                        if (settings["language"].ToString() != currlng)
                        {
                            currlng = settings["language"].ToString() ?? currlng;
                            menuOptions = GetMenuOptions(ref dict, currlng);
                            GetSettingsTranslate(ref settings, ref dict, currlng);
                            GetSettingsTranslate(ref gameMode, ref dict, currlng);
                        }
                        SetBackground(ref settings);
                        ClearConsole();
                        break;
                    // exit
                    case 3:
                        running = false;
                        ClearConsole();
                        break;
                }
            }
            SaveSettings(settingsPath, settings);
            Console.BackgroundColor = defaultBackgrounColor;
            Console.ForegroundColor = defaultForegroundColor;
        }
    }
}