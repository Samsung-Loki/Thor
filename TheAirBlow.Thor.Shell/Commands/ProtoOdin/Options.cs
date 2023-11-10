using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin; 

public class Options : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 2)
            return new FailInfo("Too many arguments!", 1);
        try {
            var odin = (Odin)state.Protocol;
            bool? value = null;
            if (args.Count > 1)
                switch (args[1].ToLower()) {
                    case "true":
                        value = true;
                        break;
                    case "false":
                        value = false;
                        break;
                    default:
                        return new FailInfo("Invalid option choice, must be [[true/false]]", 2);
                }

            switch (args[0].ToLower()) {
                case "tflash":
                    if (!value.HasValue) {
                        AnsiConsole.MarkupLine($"[green]Option \"[lime]T-Flash[/]\" is set to \"[lime]{odin.TFlashEnabled}[/]\"[/]");
                        break;
                    }
                    
                    if (!value.Value) return new FailInfo(
                        "T-Flash mode cannot be disabled, for that you have to restart the phone!", 1);
                    odin.EnableTFlash();
                    AnsiConsole.MarkupLine($"[green]Successfully set \"[lime]T-Flash[/]\" to \"[lime]{args[1].ToLower()}[/]\"![/]");
                    break;
                case "efsclear":
                    if (!value.HasValue) {
                        AnsiConsole.MarkupLine($"[green]Option \"[lime]EFS Clear[/]\" is set to \"[lime]{odin.EfsClear}[/]\"[/]");
                        break;
                    }
                    
                    odin.EfsClear = value.Value;
                    AnsiConsole.MarkupLine($"[green]Successfully set \"[lime]EFS Clear[/]\" to \"[lime]{args[1].ToLower()}[/]\"![/]");
                    break;
                case "blupdate":
                    if (!value.HasValue) {
                        AnsiConsole.MarkupLine($"[green]Option \"[lime]Bootloader Update[/]\" is set to \"[lime]{odin.BootloaderUpdate}[/]\"[/]");
                        break;
                    }
                    
                    odin.BootloaderUpdate = value.Value;
                    AnsiConsole.MarkupLine($"[green]Successfully set \"[lime]Bootloader Update[/]\" to \"[lime]{args[1].ToLower()}[/]\"![/]");
                    break;
                case "resetfc":
                    if (!value.HasValue) {
                        AnsiConsole.MarkupLine($"[green]Option \"[lime]Reset Flash Count[/]\" is set to \"[lime]{odin.ResetFlashCount}[/]\"[/]");
                        break;
                    }
                    
                    odin.ResetFlashCount = value.Value;
                    AnsiConsole.MarkupLine($"[green]Successfully set \"[lime]Reset Flash Count[/]\" to \"[lime]{args[1].ToLower()}[/]\"![/]");
                    break;
                default:
                    return new FailInfo("Invalid option choice, must be [[tflash/efsclear/blupdate/resetfc]]", 1);
            }
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "options [[tflash/efsclear/blupdate/resetfc]] (true/false) - Sets an Odin flash option, can show you current value";
}