using Serilog;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin;

public class SetRegion : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (args[0].Length != 3)
            return new FailInfo("A region code is 3 characters long", 1);
        try {
            var odin = (Odin)state.Protocol;
            odin.SetRegionCode(args[0].ToUpper());
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "setRegion <code> - Changes the region code of the device";
}