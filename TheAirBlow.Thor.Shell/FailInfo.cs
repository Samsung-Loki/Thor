using Spectre.Console;

namespace ThorRewrite.Shell; 

public class FailInfo {
    public bool Failed { get; }
    private readonly string _description;
    private readonly int _argument;

    public FailInfo(string desc, int arg) {
        _description = desc; 
        _argument = arg; Failed = true;
    }

    public FailInfo() { }

    public void Print(string input) {
        if (!Failed) throw new InvalidOperationException(
            "This command didn't fail!");
        PrintCursor(input);
        AnsiConsole.MarkupLine($"[red]{_description}[/]");
    }

    private void PrintCursor(string input) {
        if (_argument == 0) {
            AnsiConsole.MarkupLine("[red]~~~~~~~^[/]");
            return;
        }

        var split = input.Split(" ");
        var cursorStr = "~~~~~~~";
        for (var i = 0; i < _argument; i++) {
            for (var j = 0; j < split[i].Length; j++)
                cursorStr += "~";
            if (i + 1 == _argument)
                cursorStr += "~^";
            else cursorStr += "~";
        }
        AnsiConsole.MarkupLine($"[red]{cursorStr}[/]");
    }
}