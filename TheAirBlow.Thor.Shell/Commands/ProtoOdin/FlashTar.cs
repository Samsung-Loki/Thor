using System.Formats.Tar;
using K4os.Compression.LZ4.Streams;
using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.PIT;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell.Commands.ProtoOdin; 

public class FlashTar : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (!Directory.Exists(Path.GetDirectoryName(args[0])))
            return new FailInfo("Directory in the path does not exist!", 1);
        try {
            var totalBytes = 0L;
            var totalPartitions = 0;
            var odin = (Odin)state.Protocol;
            var buf = odin.DumpPIT();
            var data = new PitData(buf);
            var toFlash = new Dictionary<string, Dictionary<string, PitEntry>>();
            foreach (var i in Directory.EnumerateFiles(args[0])) {
                if (!i.EndsWith(".tar") && !i.EndsWith(".tar.md5")) continue;
                using var tar = new FileStream(i, FileMode.Open, FileAccess.Read);
                using var reader = new TarReader(tar);
                var entries = new List<TarEntry>();
                while (reader.GetNextEntry() is { } entry)
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(entry.Name)))
                        entries.Add(entry); // Do not fetch files in dirs
                var dict = new Dictionary<PitEntry, TarEntry>();
                foreach (var entry in entries) {
                    var pitEntry = data.Entries.FirstOrDefault(
                        x => x.FileName == entry.Name);
                    if (pitEntry != null)
                        dict.Add(pitEntry, entry);
                }
                
                var list = dict.Select(x => $"[lime]{x.Value.Name} ({x.Key.Partition})[/]").ToList();
                var choices = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title($"[green]Choose what partitions to flash from {Path.GetFileName(i)}:[/]")
                        .NotRequired().AddChoices(list));
                
                var chosen = new Dictionary<string, PitEntry>();
                foreach (var choice in choices) {
                    var entry = dict.First(
                        x => $"[lime]{x.Value.Name} ({x.Key.Partition})[/]" == choice);
                    chosen.Add(entry.Value.Name, entry.Key);
                    totalBytes += entry.Value.Length;
                    totalPartitions++;
                }
                
                if (chosen.Count != 0)
                    toFlash.Add(i, chosen);
            }
            
            AnsiConsole.MarkupLine($"[green]You chose to flash [bold]{totalPartitions} partitions[/] in total:[/]");
            foreach (var i in toFlash)
            foreach (var j in i.Value)
                AnsiConsole.MarkupLine($"[lime]{j.Key} from {Path.GetFileName(i.Key)} on partition {j.Value.Partition}[/]");
            if (!AnsiConsole.Confirm($"[yellow]Are you [bold]absolutely[/] sure you want to flash those?[/]", false)) 
                return new FailInfo("Cancelled by user", 0);
            AnsiConsole.Progress().AutoRefresh(true).Columns(
                    new TaskDescriptionColumn(), new ProgressBarColumn(),
                    new PercentageColumn(), new TransferSpeedColumn(), 
                    new DownloadedColumn(), new RemainingTimeColumn())
                .Start(ctx => {
                    odin.SetTotalBytes(totalBytes);
                    foreach (var i in toFlash) {
                        using var tar = new FileStream(i.Key, FileMode.Open, FileAccess.Read);
                        using var reader = new TarReader(tar);
                        while (reader.GetNextEntry() is { } entry) {
                            if (entry.DataStream == null) continue;
                            if (i.Value.ContainsKey(entry.Name)) {
                                var stream = entry.DataStream;
                                if (stream == null) continue;
                                if (entry.Name.EndsWith(".lz4"))
                                    stream = LZ4Stream.Decode(stream);
                                var pitEntry = i.Value[entry.Name];
                                var task = ctx.AddTask("[green]Initializing flash operation[/]");
                                odin.FlashPartition(stream, pitEntry, 
                                    info => {
                                        task.MaxValue(info.TotalBytes);
                                        task.Value(info.SentBytes);
                                        switch (info.State) {
                                            case Odin.FlashProgressInfo.StateEnum.Flashing:
                                                task.Description($"[green]Flashing sequence {info.SequenceIndex + 1} / {info.TotalSequences} onto {pitEntry.Partition}[/]");
                                                break;
                                            case Odin.FlashProgressInfo.StateEnum.Sending:
                                                task.Description($"[green]Sending flash sequence {info.SequenceIndex + 1} / {info.TotalSequences}[/]");
                                                break;
                                        }
                                    });
                                stream.Dispose();
                                task.StopTask();
                            }
                        }
                    }
                });
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "flashTar <directory> - Flashes odin .tar.md5 or .tar files in a directory (AP, BL, CP, CSC)";
}