using Serilog;
using Spectre.Console;

namespace ThorRewrite.Shell.Commands; 

public class EnableDebug : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 2);
        switch (args[0]) {
            case "on":
                AnsiConsole.MarkupLine("[green]Enabled debug logging[/]");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
                break;
            case "off":
                AnsiConsole.MarkupLine("[red]Disabled debug logging[/]");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateLogger();
                break;
            default:
                return new FailInfo("Invalid option choice, should be [[on/off]]", 1);
        }

        return new FailInfo();
    }

    public string GetDescription()
        => "debug [[on/off]] - enables or disables debug log level";
}