using Serilog;
using TheAirBlow.Thor.Library.PIT;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell.Commands.ProtoOdin; 

public class FlashPIT : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!Directory.Exists(Path.GetDirectoryName(args[0])))
            return new FailInfo("Directory in the path does not exist!", 1);
        if (Path.GetFileName(args[0]) == string.Empty)
            return new FailInfo("Path must contain a filename!", 1);
        var buf = File.ReadAllBytes(args[0]);
        try {
            _ = new PitData(buf);
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo("This is not a valid PIT file!", 0);
        }
        
        try {
            var odin = (Odin)state.Protocol;
            odin.FlashPIT(buf);
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "printPit <filename> - Prints PIT contents in human readable form";
}