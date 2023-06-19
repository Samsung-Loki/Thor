using K4os.Compression.LZ4.Streams;
using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.PIT;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell.Commands.ProtoOdin; 

public class FlashFile : ICommand {
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
            var data = new PitData(buf);
            var filename = Path.GetFileName(args[0]);
            var entry = data.Entries.FirstOrDefault(x =>
                x.FileName == filename);
            if (entry != null && !AnsiConsole.Confirm(
                    $"[yellow]Is \"[lime]{entry.Partition}[/]\" the right partition?[/]", false))
                entry = null;
            if (entry == null) {
                AnsiConsole.MarkupLine("[yellow]Failed to auto-select partition by filename, proceeding to ask[/]");
                var list = data.Entries.Select(x => $"[lime]{x.Partition} ({x.FileName})[/]").ToList();
                list.Insert(0, "[yellow]Cancel operation[/]");
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Choose a partition to flash this file on:[/]")
                        .AddChoices(list));
                if (choice == "[yellow]Cancel operation[/]")
                    return new FailInfo("Cancelled by user", 0);
                entry = data.Entries.First(x => $"[lime]{x.Partition} ({x.FileName})[/]" == choice);
            }
            
            if (!AnsiConsole.Confirm($"[yellow]Are you [bold]absolutely[/] sure you want to flash \"[lime]{entry.Partition}[/]\"?[/]", false)) 
                return new FailInfo("Cancelled by user", 0);
            var file = new FileStream(args[0], FileMode.Open, FileAccess.Read);
            Stream stream = file;
            if (filename.EndsWith(".lz4"))
                stream = LZ4Stream.Decode(file);
            AnsiConsole.Progress().AutoRefresh(true).Columns(
                    new TaskDescriptionColumn(), new ProgressBarColumn(),
                    new PercentageColumn(), new TransferSpeedColumn(), 
                    new DownloadedColumn(), new RemainingTimeColumn())
                .Start(ctx => {
                    var task = ctx.AddTask("[green]Initializing flash operation[/]");
                    odin.SetTotalBytes(stream.Length);
                    odin.FlashPartition(stream, entry,
                        info => {
                            task.MaxValue(info.TotalBytes);
                            task.Value(info.SentBytes);
                            switch (info.State) {
                                case Odin.FlashProgressInfo.StateEnum.Flashing:
                                    task.Description($"[green]Flashing sequence {info.SequenceIndex + 1} / {info.TotalSequences} onto {entry.Partition}[/]");
                                    break;
                                case Odin.FlashProgressInfo.StateEnum.Sending:
                                    task.Description($"[green]Sending flash sequence {info.SequenceIndex + 1} / {info.TotalSequences}[/]");
                                    break;
                            }
                        });
                    task.StopTask();
                });
            stream.Dispose();
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "flashFile <filename> - Flash a file onto the device";
}