using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin; 

public class DumpPIT : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!Directory.Exists(Path.GetDirectoryName(args[0])))
            return new FailInfo("Directory in the path does not exist!", 1);
        if (Path.GetFileName(args[0]) == string.Empty)
            return new FailInfo("Path must contain a filename!", 1);
        try {
            var odin = (Odin)state.Protocol;
            var buf = odin.DumpPIT();
            File.WriteAllBytes(args[0], buf);
            AnsiConsole.MarkupLine($"[green]Successfully dumped PIT to {args[0]}![/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "dumpPit <filename> - Dumps device's PIT to a file";
}