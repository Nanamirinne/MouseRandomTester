# MouseRandomTester

A small Windows desktop utility for automation testing. It randomly moves the mouse within an estimated active browser page area and scrolls up or down at random intervals.

## Behavior

- Start and stop from a simple Windows Forms UI.
- Runs every 20-40 seconds.
- Moves the mouse 10-25 pixels per action.
- Scrolls 3-7 wheel clicks per action.
- Estimates the active browser page area and keeps movement inside that area.
- Tracks an estimated scroll position so scrolling tends to stay near the middle of a page.

## Build

This project targets .NET Framework 4.8 and Windows Forms.

You can compile it with the Windows .NET Framework C# compiler:

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /platform:x64 /optimize+ /out:MouseRandomTester.exe /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll Program.cs
```

Or open `MouseRandomTester.csproj` in Visual Studio and build the Release x64 configuration.

## Use

Run the generated `MouseRandomTester.exe`, click `Start`, then bring the browser page you want to test to the foreground. Click `Stop` or close the window to exit.
