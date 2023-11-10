using System.Text;
using Serilog;
using Spectre.Console;

namespace TheAirBlow.Thor.Shell.Commands; 

public class RawRead : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!state.Handler.IsConnected())
            return new FailInfo("Not connected to a device!", 0);
        try {
            var amount = int.Parse(args[0]);
            var buf = state.Handler.BulkRead(amount, out var read);
            AnsiConsole.MarkupLine($"[cyan]Read {read} bytes in total[/]");
            var str = "";
            for (var i = 0; i < buf.Length; i++) {
                str += Convert.ToHexString(new[] { buf[i] });
                if (i + 1 != buf.Length) str += " ";
            }
            AnsiConsole.MarkupLine($"[cyan]Hexadecimal: {str}[/]");
            str = Encoding.ASCII.GetString(buf);
            AnsiConsole.MarkupLine($"[cyan]ASCII: {str}[/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "read <amount> - Spits out raw bytes from the device";
}