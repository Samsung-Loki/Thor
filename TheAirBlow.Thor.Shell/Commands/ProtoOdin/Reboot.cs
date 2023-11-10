using Serilog;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin; 

public class Reboot : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        try {
            var odin = (Odin)state.Protocol;
            switch (args[0].ToLower()) {
                case "normal":
                    odin.EndSession();
                    state.ProtocolType = Protocol.None;
                    state.Protocol = null!;
                    odin.Reboot();
                    break;
                case "odin":
                    odin.EndSession();
                    state.ProtocolType = Protocol.None;
                    state.Protocol = null!;
                    try {
                        odin.RebootToOdin();
                    } catch (Exception e) {
                        odin.Reboot(); state.Handler.Disconnect();
                        Log.Debug("Full exception: {0}", e);
                        return new FailInfo("Your device does not support rebooting into Odin mode, so we did a regular one", 1);
                    }
                    break;
                default:
                    return new FailInfo("Invalid choice, must be [[odin/normal]]", 1);
            }
            
            state.Handler.Disconnect();
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "reboot [[odin/normal]] - Reboots the device";
}