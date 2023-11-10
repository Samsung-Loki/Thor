using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library;

namespace TheAirBlow.Thor.Shell.Commands; 

public class Connect : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count > 0)
            return new FailInfo("Too many arguments!", 1);
        if (state.Handler.IsConnected())
            return new FailInfo("Already connected to a device!", 0);
        try {
            var devices = state.Handler.GetDevices().GetAwaiter().GetResult();
            if (devices.Count == 0)
                return new FailInfo("No Samsung devices were found!", 0);
            var list = devices.Select(x => $"{x.DisplayName.Ansify()} (ID {x.Identifier})").ToList();
            list.Add("[yellow]Cancel operation[/]");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Choose a device to connect to:[/]")
                    .AddChoices(list));
            if (choice == "[yellow]Cancel operation[/]")
                return new FailInfo("Cancelled by user", 0);
            var device = devices.First(x => $"{x.DisplayName.Ansify()} (ID {x.Identifier})" == choice);
            state.Handler.Initialize(device.Identifier);
            AnsiConsole.MarkupLine("[green]Successfully connected to the device![/]");
            AnsiConsole.MarkupLine("[green]Now run \"[lime]begin[/]\" with the protocol you need.[/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "connect - Initializes a connection";
}