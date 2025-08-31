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
            switch(Colors[ColorIndex])
            {
                case 0:
                    return "black";
                case 1:
                    return "dark blue";
                case 2:
                    return "dark green";
                case 3:
                    return "dark cyan";
                case 4:
                    return "dark red";
                case 5:
                    return "dark magenta";
                case 6:
                    return "dark yellow";
                case 7:
                    return "gray";
                case 8:
                    return "dark gray";
                case 9:
                    return "blue";
                case 10:
                    return "green";
                case 11:
                    return "cyan";
                case 12:
                    return "red";
                case 13:
                    return "magenta";
                case 14:
                    return "yellow";
                case 15:
                    return "white";
                default:
                    return "";   
            }
        }
        public override void SetValue(object value){}
        public ConsoleColor GetColor()
        {
            return (ConsoleColor)Colors[ColorIndex];
        }
    }
    class Menu
    {
        public static T[][] Paginate<T>(IEnumerable<T> array, int pageSize)
        {
            if (pageSize <= 0)
            {throw new ArgumentException("Page size must be greater than 0");}
            if (array == null)
            {throw new ArgumentNullException("Array is null");}

            T[] list = array.ToArray();
            int count = list.Length;

            if (count == 0)
            {throw new ArgumentException("Array is empty");}

            int pageCount = count % pageSize != 0 ?((count - count % pageSize) / pageSize) + 1 : count / pageSize;
            T[][] pages = new T[pageCount][];
            for (int i=0; i<pageCount; i++)
            {
                pages[i] = new T[pageSize];
                int start = i * pageSize;
                int end = start + pageSize < count ? start + pageSize : count;
                int j = 0;
                for (int k=start; k<end; k++)
                {
                    pages[i][j] = list[k];
                    j++;
                }
            }
            return pages;
        }
        private static void ShowPage<T>(T[] page, string title = "", 
                                         ConsoleColor selectionColor = ConsoleColor.Green,
                                          ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            bool cond = false;
            int YOffset = title != "" ? title.Split("\n").Length : 0;
            if (title != "") Console.WriteLine(title);
            for (int i=0; i<page.Length; i++)
            {
                if (cond)
                {
                    Console.ForegroundColor = consoleColor;
                    cond = false;
                }
                if (i == 0)
                {
                    Console.ForegroundColor = selectionColor;
                    cond = true;
                    Console.WriteLine("> " + Convert.ToString(page[i]));
                    continue;
                }
                Console.WriteLine("  " + Convert.ToString(page[i]));
            }
            Console.ForegroundColor = consoleColor;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Console.SetCursorPosition(Convert.ToString(page[0]).Length + 2, YOffset);
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
        public static T MenuShow<T>(T[][] pages, int pageIndex = 0, string title = "", 
                                     ConsoleColor selectionColor = ConsoleColor.Green, 
                                      ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            // BlockConsole();
            int choice = 0;
            
            int YOffset = title != "" ? title.Split("\n").Length : 0;

            int pi = pageIndex;
            ShowPage(pages[pi], title, selectionColor, consoleColor);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = consoleColor;
                        Console.Write("  " + pages[pi][choice]);
                        if (choice > 0)
                        {choice--;}
                        else
                        {choice = pages[pi].Length - 1;}
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = selectionColor;
                        Console.Write("> " + pages[pi][choice]);
                        Console.ForegroundColor = consoleColor;
                        break;
                    case ConsoleKey.DownArrow:
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = consoleColor;
                        Console.Write("  " + pages[pi][choice]);
                        if (choice < pages[pi].Length - 1)
                        {choice++;}
                        else
                        {choice = 0;}
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = selectionColor;
                        Console.Write("> " + pages[pi][choice]);
                        Console.ForegroundColor = consoleColor;
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        return pages[pi][choice];
                    case ConsoleKey.LeftArrow:
                        if (pi > 0)
                        {pi--;}
                        else
                        {pi = pages.Length - 1;}
                        ShowPage(pages[pi], title, selectionColor, consoleColor);
                        choice = 0;
                        break;
                    case ConsoleKey.RightArrow:
                        if (pi < pages.Length - 1)
                        {pi++;}
                        else
                        {pi = 0;}
                        ShowPage(pages[pi], title, selectionColor, consoleColor);
                        choice = 0;
                        break;
                    case ConsoleKey.Escape:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        T? x = default;
                        if(typeof(T) == typeof(string)) return (T)(object)"";
                        return x == null ? pages[0][0] : x;
                }
            }
        }
        public static void ShowSettings(Dictionary<string, SettingOption> settings, int selected, 
                                        ConsoleColor selectionColor=ConsoleColor.Green,
                                        ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            string[] settingNames = settings.Keys.ToArray();
            ConsoleColor valueSelectedColor = ConsoleColor.White;
            for(int i=0; i<settingNames.Length; i++)
            {
                if(i==selected) Console.ForegroundColor = selectionColor;
                else Console.ForegroundColor = consoleColor;
                Console.Write(settingNames[i]+": ");
                if(i==selected) Console.ForegroundColor = valueSelectedColor;
                Console.Write("< ");
                Console.Write(settings[settingNames[i]]);
                Console.WriteLine(" >");
            }

        }
        public static void ChangeSettings(Dictionary<string, SettingOption> settings, 
                                           ConsoleColor selectionColor = ConsoleColor.Green,
                                            ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            string[] settingNames = settings.Keys.ToArray();
            int currentSetting = 0;
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            ShowSettings(settings, currentSetting, selectionColor, consoleColor);
            bool notConfirmed = true;
            while(notConfirmed)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        currentSetting = (currentSetting+1) % settingNames.Length;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor, consoleColor);
                        break;
                    case ConsoleKey.UpArrow:
                        currentSetting = currentSetting == 0 ? settingNames.Length-1 : currentSetting-1;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor, consoleColor);
                        break;
                    case ConsoleKey.LeftArrow:
                        settings[settingNames[currentSetting]].PreviousValue();
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor, consoleColor);
                        break;
                    case ConsoleKey.RightArrow:
                        settings[settingNames[currentSetting]].NextValue();
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor, consoleColor);
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