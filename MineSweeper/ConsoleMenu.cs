using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleMenu
{
    public abstract class SettingOption
    {
        public abstract void NextValue();
        public abstract void PreviousValue();
        public abstract void SetValue(object value);
    }

    public class StringOption: SettingOption
    {
        private string[] Options;
        private int ValueIndex;
        public StringOption(string[] options, int valueIndex)
        {
            Options = options;
            ValueIndex = valueIndex;
        }
        public override void NextValue()
        {
            ValueIndex = (ValueIndex+1) % Options.Length;
        }
        public override void PreviousValue()
        {
            ValueIndex = ValueIndex == 0 ? Options.Length-1 : ValueIndex-1;
        }
        public override string ToString()
        {
            return Options[ValueIndex];
        }

        public override void SetValue(object value){}
    }
    public class StringSetValue: SettingOption
    {
        private string Value;
        public StringSetValue(string value)
        {
            Value = value;
        }
        public override void NextValue()
        {

        }
        public override void PreviousValue()
        {

        }
        public override string ToString()
        {
            return Value;
        }
        public override void SetValue(object value)
        {
            Value = Convert.ToString(value) ?? "";
        }
    }
    public class BoolOption: SettingOption
    {
        private bool Value;
        public BoolOption(bool value)
        {
            Value = value;
        }
        public override void NextValue()
        {
            Value = !Value;
        }
        public override void PreviousValue()
        {
            Value = !Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public override void SetValue(object value){}
    }
    public class IntOption: SettingOption
    {
        public int Value;
        private int Step;

        public IntOption(int value, int step)
        {
            Value = value;
            Step = step;
        }
        public override void NextValue()
        {
            Value += Step;
        }
        public override void PreviousValue()
        {
            Value -= Step;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public override void SetValue(object value){}
    }
    public class IntRangeOption : SettingOption
    {
        public int Value;
        private int Step;
        private int Start;
        private int End;

        public IntRangeOption(int start, int end, int step=1)
        {
            Value = start;
            Step = step;
            Start = start;
            End = end;
        }
        public override void NextValue()
        {
            Value = Value == End ? Start : Value+Step;
        }
        public override void PreviousValue()
        {
            Value = Value == Start ? End : Value-Step;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public override void SetValue(object value){}
    }
    public class DoubleOption : SettingOption
    {
        private double Value;
        private double Step;

        public DoubleOption(double value, double step)
        {
            Value = value;
            Step = step;
        }
        public override void NextValue()
        {
            Value += Step;
        }
        public override void PreviousValue()
        {
            Value -= Step;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public override void SetValue(object value) { }
    }
    public class ColorOption: SettingOption
    {
        private int[] Colors;
        private int ColorIndex;
        public ColorOption(int[] colors, int valueIndex)
        {
            Colors = colors;
            ColorIndex = valueIndex;
        }
        public override void NextValue()
        {
            ColorIndex = (ColorIndex+1) % Colors.Length;
        }
        public override void PreviousValue()
        {
            ColorIndex = ColorIndex == 0 ? Colors.Length-1 : ColorIndex-1;
        }
        public override string ToString()
        {
            string[] allcolors = new string[16] {"black", "dark blue", "dark green",
                                                "dark cyan", "dark red", "dark magenta",
                                                "dark yellow", "gray", "dark gray",
                                                "blue", "green", "cyan", "red",
                                                "magenta", "yellow", "white"};
            return Colors[ColorIndex] > 0 && Colors[ColorIndex] < allcolors.Length
                                    ? allcolors[Colors[ColorIndex]] 
                                    : "no color";
        }
        public override void SetValue(object value){}
        public ConsoleColor GetColor()
        {
            return (ConsoleColor)Colors[ColorIndex];
        }
    }
    class Menu
    {
        public static T[] GetRow<T>(ref T[,] mtrx, int r)
        {
            T[] row = new T[mtrx.GetLength(1)];
            for (int c = 0; c < mtrx.GetLength(1); c++) row[c] = mtrx[r, c];
            return row;
        }
        public static T[,] Paginate<T>(IEnumerable<T> array, int pageSize)
        {
            if (pageSize <= 0)
            { throw new ArgumentException("Page size must be greater than 0"); }
            if (array == null)
            { throw new ArgumentNullException("Array is null"); }

            T[] list = array.ToArray();
            int lstLen = list.Length;

            if (lstLen == 0)
            { throw new ArgumentException("Array is empty"); }

            int pageCount = lstLen % pageSize != 0 ? ((lstLen - lstLen % pageSize) / pageSize) + 1 : lstLen / pageSize;
            T[,] pages = new T[pageCount, pageSize];
            for (int i = 0; i < pageCount; i++)
            {
                int start = i * pageSize;
                int end = start + pageSize < lstLen ? start + pageSize : lstLen;
                int j = 0;
                for (int k = start; k < end; k++)
                {
                    pages[i, j] = list[k];
                    j++;
                }
            }
            return pages;
        }
        private static void ShowPage<T>(T[] page, string title = "", int startIndex = 0,
                                         (ConsoleColor, ConsoleColor) colors = default)
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            ConsoleColor consoleColor = colors.Item1;
            ConsoleColor selectionColor = colors.Item2;
            int YOffset = title != "" ? title.Split("\n").Length : 0;
            if (title != "") Console.WriteLine(title);
            for (int i=0; i<page.Length; i++)
            {
                if (i == startIndex)
                {
                    Console.ForegroundColor = selectionColor;
                    Console.WriteLine("> " + Convert.ToString(page[i]));
                    Console.ForegroundColor = consoleColor;
                    continue;
                }
                Console.WriteLine("  " + Convert.ToString(page[i]));
            }
            Console.ForegroundColor = consoleColor;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Console.SetCursorPosition(Convert.ToString(page[startIndex]).Length + 2, startIndex+YOffset);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        /// <summary>
        ///     Shows menu in console with paginated options<br/><br/>
        ///     Controls are:<br/>
        ///         - Arrow Up to go up<br/>
        ///         - Arrow Down to go down<br/>
        ///         - Arrow Right to go to next page<br/>
        ///         - Arrow Left to go to previous page<br/>
        ///         - Escape to return default value<br/>
        ///         - Enter to confirm choice
        /// 
        /// </summary> 
        /// <returns>T of choice</returns>
        public static T MenuShow<T>(T[,] pages, int pageIndex = 0, string title = "", 
                                     (ConsoleColor, ConsoleColor) colors = default)
        {
            int pi = pageIndex;
            int ei = 0;
            ShowPage(GetRow(ref pages, pi), title, 0, colors);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (ei > 0)
                        {ei--;}
                        else
                        {ei = pages.GetLength(1) - 1;}
                        ShowPage(GetRow(ref pages, pi), title, ei, colors);
                        break;
                    case ConsoleKey.DownArrow:
                        if (ei < pages.GetLength(1) - 1)
                        {ei++;}
                        else
                        {ei = 0;}
                        ShowPage(GetRow(ref pages, pi), title, ei, colors);
                        break;
                    case ConsoleKey.LeftArrow:
                        if (pi > 0)
                        {pi--;}
                        else
                        {pi = pages.Length - 1;}
                        ShowPage(GetRow(ref pages, pi), title, 0, colors);
                        ei = 0;
                        break;
                    case ConsoleKey.RightArrow:
                        if (pi < pages.Length - 1)
                        {pi++;}
                        else
                        {pi = 0;}
                        ShowPage(GetRow(ref pages, pi), title, 0, colors);
                        ei = 0;
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        return pages[pi, ei];
                    case ConsoleKey.Escape:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        T? x = default;
                        if(typeof(T) == typeof(string)) return (T)(object)"";
                        return x == null ? pages[0, 0] : x;
                }
            }
        }
        public static void ShowSettings(Dictionary<string, SettingOption> settings, int selected, 
                                        (ConsoleColor, ConsoleColor) colors = default)
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            ConsoleColor consoleColor = colors.Item1;
            ConsoleColor selectionColor = colors.Item2;
            string[] settingNames = settings.Keys.ToArray();
            ConsoleColor valueSelectedColor = ConsoleColor.White;
            for(int i=0; i<settingNames.Length; i++)
            {
                if(i==selected) Console.ForegroundColor = selectionColor;
                else Console.ForegroundColor = consoleColor;
                Console.Write(settingNames[i]+": ");
                if(i==selected) Console.ForegroundColor = valueSelectedColor;
                Console.WriteLine("< "+settings[settingNames[i]].ToString()+" >");
            }

        }
        public static void ChangeSettings(Dictionary<string, SettingOption> settings, 
                                           (ConsoleColor, ConsoleColor) colors = default)
        {
            string[] settingNames = settings.Keys.ToArray();
            int currentSetting = 0;
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            ShowSettings(settings, currentSetting, colors);
            while(true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        currentSetting = (currentSetting+1) % settingNames.Length;
                        ShowSettings(settings, currentSetting, colors);
                        break;
                    case ConsoleKey.UpArrow:
                        currentSetting = currentSetting == 0 ? settingNames.Length-1 : currentSetting-1;
                        ShowSettings(settings, currentSetting, colors);
                        break;
                    case ConsoleKey.LeftArrow:
                        settings[settingNames[currentSetting]].PreviousValue();
                        ShowSettings(settings, currentSetting, colors);
                        break;
                    case ConsoleKey.RightArrow:
                        settings[settingNames[currentSetting]].NextValue();
                        ShowSettings(settings, currentSetting, colors);
                        break;
                    case ConsoleKey.Escape:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        return;
                }
            }
        }
    }
}