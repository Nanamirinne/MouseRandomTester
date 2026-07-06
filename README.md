# MouseRandomTester

A small Windows desktop utility for automation testing. It randomly moves the mouse within an estimated active browser page area and scrolls up or down at random intervals.

## Behavior

- Start and stop from a simple Windows Forms UI.
- Toggle start and stop from the background with a configurable global hotkey, defaulting to `Ctrl+Alt+M`.
- Runs at a configurable random interval, defaulting to 10-20 seconds.
- Can automatically stop after a configurable number of minutes, or run until stopped.
- Plays a repeated alarm and shows a topmost alert when the configured run duration ends.
- Includes a Test alarm button for checking whether the alarm is audible without blocking the UI.
- Moves the mouse 10-25 pixels per action.
- Scrolls 3-7 wheel clicks per action.
- Optionally types 3-10 random letters, numbers, or spaces per action with human-like pauses between characters.
- Estimates the active browser page area and keeps movement inside that area.
- Tracks an estimated scroll position so scrolling tends to stay near the middle of a page.

## Build

This project targets .NET Framework 4.8 and Windows Forms.

You can compile it with the Windows .NET Framework C# compiler:

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /platform:x64 /optimize+ /out:"Nethard Music.exe" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll Program.cs
```

Or open `NethardMusic.csproj` in Visual Studio and build the Release x64 configuration.

## Use

Run the generated `Nethard Music.exe`, choose the minimum and maximum interval seconds, optionally set run minutes (`0` means until stopped), choose a hotkey if the default is unavailable, click `Start` or press the configured hotkey, then bring the browser page you want to test to the foreground. Uncheck keyboard actions if you only want mouse movement and wheel scrolling without random typing. Click `Stop`, press the configured hotkey again, or close the window to exit.
