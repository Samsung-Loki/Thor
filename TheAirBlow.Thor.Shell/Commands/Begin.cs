using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands; 

public class Begin : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!state.Handler.IsConnected())
            return new FailInfo("Not connected to a device!", 0);
        try {
            switch (args[0].ToLower()) {
                case "odin":
                    state.ProtocolType = Protocol.Odin;
                    var odin = new Odin(state.Handler);
                    state.Protocol = odin;
                    odin.Handshake();
                    odin.BeginSession();
                    AnsiConsole.MarkupLine("[green]Successfully began an Odin session![/]");
                    break;
                default:
                    return new FailInfo("Invalid option choice, should be [[odin]]", 1);
            }
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "begin [[odin]] - Begins a session with chosen protocol";
}