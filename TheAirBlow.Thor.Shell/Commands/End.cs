using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands; 

public class End : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count > 0)
            return new FailInfo("Too many arguments!", 1);
        if (!state.Handler.IsConnected())
            return new FailInfo("Not connected to a device!", 0);
        if (state.ProtocolType == Protocol.None)
            return new FailInfo("No protocol session is active!", 0);
        try {
            switch (state.ProtocolType) {
                case Protocol.Odin:
                    state.ProtocolType = Protocol.Odin;
                    var odin = (Odin)state.Protocol;
                    try {
                        odin.Shutdown();
                    } catch {
                        odin.EndSession();
                    }
                    state.ProtocolType = Protocol.None;
                    state.Protocol = null!;
                    AnsiConsole.MarkupLine("[green]Successfully ended an Odin session![/]");
                    break;
            }
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "end - Ends the current protocol session and shuts the device down if possible";
}