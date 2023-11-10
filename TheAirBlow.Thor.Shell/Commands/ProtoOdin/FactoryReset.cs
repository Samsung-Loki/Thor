using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin; 

public class FactoryReset : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count > 0)
            return new FailInfo("Too many arguments!", 1);
        try {
            var ans = AnsiConsole.Confirm("[yellow]Are you [bold]absolutely[/] sure you want to do this?[/]", false);
            if (!ans) return new FailInfo("Cancelled by user", 0);
            AnsiConsole.Status().Start(
                "[darkorange]Erasing userdata partition, please be patient...[/]", 
                _ => { 
                    var odin = (Odin)state.Protocol;
                    odin.EraseUserData();
                });
            AnsiConsole.MarkupLine("[green]Successfully erased userdata![/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "factoryReset - Erases the userdata partition, will take a while";
}