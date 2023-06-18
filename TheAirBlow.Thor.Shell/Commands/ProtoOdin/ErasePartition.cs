using K4os.Compression.LZ4.Streams;
using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library.PIT;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell.Commands.ProtoOdin; 

public class ErasePartition : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count > 1)
            return new FailInfo("Too many arguments!", 1);
        if (args.Count < 1)
            return new FailInfo("Not enough arguments!", 0);
        try {
            var odin = (Odin)state.Protocol;
            var buf = odin.DumpPIT();
            var data = new PitData(buf);
            var length = int.Parse(args[0]);
            var list = data.Entries.Select(x => $"[lime]{x.Partition} ({x.FileName})[/]").ToList();
            list.Insert(0, "[yellow]Cancel operation[/]");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Choose a partition to permanently erase:[/]")
                    .AddChoices(list));
            if (choice == "[yellow]Cancel operation[/]")
                return new FailInfo("Cancelled by user", 0);
            var entry = data.Entries.First(x => $"[lime]{x.Partition} ({x.FileName})[/]" == choice);
            if (!AnsiConsole.Confirm($"[yellow]Are you [bold]absolutely[/] sure you want to [bold]permanently erase[/] \"[lime]{entry.Partition}[/]\"?[/]", false)) 
                return new FailInfo("Cancelled by user", 0);
            AnsiConsole.Progress().Columns(
                    new TaskDescriptionColumn(), new ProgressBarColumn(),
                    new PercentageColumn(), new TransferSpeedColumn(), 
                    new DownloadedColumn(), new RemainingTimeColumn())
                .Start(ctx => {
                    var task = ctx.AddTask("[green]Initializing flash operation[/]");
                    odin.SetTotalBytes(length);
                    odin.FlashPartition(null, entry, 
                        info => {
                            task.MaxValue(info.TotalBytes);
                            task.Value(info.SentBytes);
                            switch (info.State) {
                                case Odin.FlashProgressInfo.StateEnum.Flashing:
                                    task.Description($"[green]Flashing sequence {info.SequenceIndex} / {info.TotalSequences} onto {entry.Partition}[/]");
                                    break;
                                case Odin.FlashProgressInfo.StateEnum.Sending:
                                    task.Description($"[green]Sending flash sequence {info.SequenceIndex} / {info.TotalSequences}[/]");
                                    break;
                            }
                        }, length);
                    task.StopTask();
                });
            AnsiConsole.MarkupLine($"[green]Successfully erased {entry.Partition}![/]");
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "flashFile <size> - Permanently erases a partition, you have to supply it's size in bytes";
}