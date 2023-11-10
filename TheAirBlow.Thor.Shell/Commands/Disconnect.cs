using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands; 

public class Disconnect : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count > 0)
            return new FailInfo("Too many arguments!", 1);
        if (!state.Handler.IsConnected())
            return new FailInfo("Not connected to a device!", 0);
        try {
            state.Handler.Disconnect();
            state.Protocol = Protocol.None;
            AnsiConsole.MarkupLine("[green]Successfully disconnected the device![/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "disconnect - Closes the current connection";
}