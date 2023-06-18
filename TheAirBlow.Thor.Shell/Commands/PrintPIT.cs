using Serilog;
using Spectre.Console;
using Spectre.Console.Rendering;
using TheAirBlow.Thor.Library.PIT;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell.Commands; 

public class PrintPIT : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 1);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!Directory.Exists(Path.GetDirectoryName(args[0])))
            return new FailInfo("Directory in the path does not exist!", 1);
        if (Path.GetFileName(args[0]) == string.Empty)
            return new FailInfo("Path must contain a filename!", 1);
        try {
            var data = new PitData(args[0]);
            var mapper = data.Mapper;
            var root = new Tree("[green]PIT File[/]");
            var header = root.AddNode("[yellow]Header[/]");
            header.AddNode($"[lime]Unknown string: {data.Unknown}[/]");
            header.AddNode($"[lime]Project name: {data.Project}[/]");
            var version = data.IsNewVersion ? "v2 (new)" : "v1 (old)";
            header.AddNode($"[lime]Version: {version}[/]");
            header.AddNode($"[lime]Reserved: {data.Reserved}[/]");
            for (var i = 0; i < data.Entries.Count; i++) {
                var node = root.AddNode($"[darkorange]Entry #{i}[/]");
                var entry = data.Entries[i];
                node.AddNode($"[lime]{mapper.UpdateAttributes[0]}: " +
                             $"{mapper.UpdateAttributes[entry.UpdateAttributes + 1]} ({entry.UpdateAttributes})[/]");
                node.AddNode($"[lime]{mapper.Attributes[0]}: " +
                             $"{mapper.Attributes[entry.Attributes + 1]} ({entry.Attributes})[/]");
                node.AddNode($"[lime]{mapper.BinaryType[0]}: " +
                             $"{mapper.BinaryType[entry.BinaryType + 1]} ({entry.BinaryType})[/]");
                node.AddNode($"[lime]{mapper.DeviceType[0]}: " +
                             $"{mapper.DeviceType[entry.DeviceType + 1]} ({entry.DeviceType})[/]");
                node.AddNode($"[lime]{mapper.BlockSize}: {entry.BlockSize}[/]");
                node.AddNode($"[lime]{mapper.BlockCount}: {entry.BlockCount}[/]");
                node.AddNode($"[lime]Partition Name: {entry.Partition}[/]");
                node.AddNode($"[lime]Partition ID: {entry.PartitionID}[/]");
                node.AddNode($"[lime]File Offset: {entry.FileOffset}[/]");
                node.AddNode($"[lime]File Size: {entry.FileSize}[/]");
                node.AddNode($"[lime]File Name: {entry.FileName}[/]");
                node.AddNode(!string.IsNullOrWhiteSpace(entry.DeltaName)
                    ? $"[lime]Delta Name: {entry.DeltaName}[/]"
                    : $"[lime]Empty Delta Name[/]");
            }

            AnsiConsole.Write(root);
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo("This is not a valid PIT file!", 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "printPit <filename> - Prints PIT contents in human readable form";
}