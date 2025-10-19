<h1>Install guide</h1><hr/><br/>
<h2>Windows</h2>

- <h3>Just install:</h3>

Just download the latest release zip, extract and it will work.<br/>
Haven't made installer yet :((

- <h3>To build yourself:<h3>
1. Install <a href="https://dotnet.microsoft.com/en-us/download">.NET SDK 8</a><br/>
2. Install <a href="https://git-scm.com/downloads">Git</a> and copy repo:
```bash
git clone https://github.com/Khorsash/minesweeper.git 
```
- Or just download source code zip and extract it
3. Run in repo dir location 
```bash
dotnet publish -c Release -r win-x64
``` 
4. Builded executable is in ```...\MineSweeper\bin\Release\net8.0\win-x64\publish```

<br/>
<h2>Linux</h2>

- <h3>To build yourself:<h3>
1. Install <a href="https://dotnet.microsoft.com/en-us/download">.NET SDK 8</a><br/>
2. Install <a href="https://git-scm.com/downloads">Git</a> if you haven't and copy repo:
```bash
git clone https://github.com/Khorsash/minesweeper.git 
```
3. Run in repo dir location 
```bash
dotnet publish -c Release -r linux-x64
``` 
- If you have arm64 or arm32 system then change "-64x" with "-arm64" or "-arm32"
4. Builded executable is in ```.../MineSweeper/bin/Release/net8.0/win-x64/publish```

<br/>

[comment]: <> (Mac OS)
