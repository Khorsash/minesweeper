using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LanguageUtils
{
    class Languages
    {
        public static string defaultLanguage = "english";
        public static Dictionary<string, Dictionary<string, string>> Load(string languagePackPath)
        {
            string txt = File.ReadAllText(languagePackPath, System.Text.Encoding.UTF8);
#pragma warning disable CS8603 // Possible null reference return.
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(txt);
#pragma warning restore CS8603 // Possible null reference return.
        }
        public static string GetTranslate(ref Dictionary<string, Dictionary<string, string>> lngs,
                                                                                        string currlng, string txt)
        {
            if (!lngs.ContainsKey(currlng) && !lngs[defaultLanguage].ContainsKey(txt)) return txt;
            if (!lngs.ContainsKey(currlng)) return lngs[defaultLanguage][txt];
            return lngs[currlng][txt];
        }
    }
}