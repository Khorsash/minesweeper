<h1> 0.1.1 </h1>

- added <code>Cell</code> struct, basic game operations(<code>GenerateBoard</code>, <code>FloodFill</code>), game main loop <code>MineSweeper</code>, defining size of the board and count of bombs with arguments

<h1> 0.1.2 </h1>

- added <code>ConsoleMenu</code> for game menu, settings for customization in <code>ConsoleMenu.cs</code>, added <code>LanguageUtils.cs</code> and translates.json for translations for other languages

<h1> 0.1.3 </h1>

- added <code>WriteTranslates</code>, <code>Translates.defaultLanguages</code> to write default translates, keeping possibility to change translates and add another language in <code>translates.json</code>, and from now <code>translates.json</code> and <code>settings.txt</code> are in <code>AppData/Roaming</code> for windows(i haven't actually testing for linux/mac os, so check out <code>TODO.md</code>)

- added color fill for bomb count