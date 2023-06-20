using Serilog;
using Spectre.Console;
using TheAirBlow.Thor.Library;
using TheAirBlow.Thor.Library.Communication;
using TheAirBlow.Thor.Library.Protocols;
using ThorRewrite.Shell;
using ThorRewrite.Shell.Commands;
using ThorRewrite.Shell.Commands.ProtoOdin;
using PrintPIT = ThorRewrite.Shell.Commands.PrintPIT;
using RawWrite = ThorRewrite.Shell.Commands.RawWrite;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();
AnsiConsole.MarkupLine("[green]Welcome to Thor Shell v1.0.2![/]");
if (!USB.TryGetHandler(out var handler)) {
    AnsiConsole.MarkupLine("[red]A USB handler wasn't written for your platform![/]");
    AnsiConsole.MarkupLine($"[red]Currently supported platforms: {USB.GetSupported()}.[/]");
    return;
}

await AnsiConsole.Status().StartAsync(
    "[darkorange]Please be patient, loading device name database...[/]", 
    async _ => {
        switch (await Lookup.Initialize()) {
            case Lookup.InitState.Downloaded:
                AnsiConsole.MarkupLine("[green]Successfully downloaded \"[lime]usb.ids[/]\" and cached it locally.[/]");
                break;
            case Lookup.InitState.Failed:
                AnsiConsole.MarkupLine("[red]Failed to download \"[lime]usb.ids[/]\", no device names will be shown![/]");
                break;
            case Lookup.InitState.Cache:
                AnsiConsole.MarkupLine("[green]Successfully loaded \"[lime]usb.ids[/]\" from cache.[/]");
                break;
        }
    });

AnsiConsole.MarkupLine("[green]Type \"[lime]help[/]\" for list of commands.[/]");
AnsiConsole.MarkupLine("[green]To start off, type \"[lime]connect[/]\" to initiate a connection.[/]");
AnsiConsole.MarkupLine("[yellow]~~~~~~~~ Platform specific notes ~~~~~~~~[/]");
AnsiConsole.MarkupLine($"[yellow]{handler.GetNotes()}[/]");

// Load in all commands
Dictionary<string, ICommand> commands = new() {
    { "debug", new EnableDebug() },
    { "connect", new Connect() },
    { "printPit", new PrintPIT() },
    { "disconnect", new Disconnect() },
    { "write", new RawWrite() },
    { "read", new RawRead() },
    { "begin", new Begin() },
    { "end", new End() }
};

Dictionary<Protocol, Dictionary<string, ICommand>> protoCommands = new() {
    { Protocol.None, new Dictionary<string, ICommand>() },
    { Protocol.Odin, new Dictionary<string, ICommand> {
        { "printPit", new ThorRewrite.Shell.Commands.ProtoOdin.PrintPIT() },
        { "write", new ThorRewrite.Shell.Commands.ProtoOdin.RawWrite() },
        { "erasePartition", new ErasePartition() },
        { "factoryReset", new FactoryReset() },
        { "setRegion", new SetRegion() },
        { "flashFile", new FlashFile() },
        { "flashTar", new FlashTar() },
        { "flashPit", new FlashPIT() },
        { "options", new Options() },
        { "dumpPit", new DumpPIT() },
        { "reboot", new Reboot() }
    } },
};

// Begin loop
var state = new State(handler);
while (true) {
    Console.Write("shell> ");
    var input = Console.ReadLine();
    if (input == null) continue;
    var split = input.Split(" ").ToList();
    if (split.Count == 0) continue;
    var name = split[0];
    if (name is "quit" or "exit") {
        AnsiConsole.MarkupLine("[lime]Goodbye![/]");
        break;
    }

    var proto = protoCommands[state.ProtocolType];
    if (name == "help") {
        if (state.ProtocolType == Protocol.None) {
            AnsiConsole.MarkupLine("[green][bold][italic]Note: beginning a protocol session unlocks new commands for you to use[/][/][/]");
            AnsiConsole.MarkupLine("[green][bold][italic]Note: they can also override the default commands for extension purposes[/][/][/]");
        }
        
        AnsiConsole.MarkupLine("[yellow][[required]] {optional} - option list[/]");
        AnsiConsole.MarkupLine("[yellow]<required> (optional) - usual argument[/]");
        AnsiConsole.MarkupLine($"[green]Total commands: {commands.Count + 2}[/]");
        AnsiConsole.MarkupLine("[cyan]exit - Closes the shell, quit also works[/]");
        foreach (var (str, i) in commands) {
            if (state.ProtocolType != Protocol.None
                && proto.ContainsKey(str)) continue;
            AnsiConsole.MarkupLine($"[cyan]{i.GetDescription()}[/]");
        }
        if (state.ProtocolType != Protocol.None)
            AnsiConsole.MarkupLine($"[green]Total protocol commands: {proto.Count}[/]");
        foreach (var i in proto.Values)
            AnsiConsole.MarkupLine($"[cyan]{i.GetDescription()}[/]");
        continue;
    }

    if (!proto.TryGetValue(name, out var command)) {
        if (!commands.TryGetValue(name, out command)) {
            new FailInfo("This command does not exist", 0).Print(input);
            continue;
        }
    }

    split.RemoveAt(0);
    var info = command.RunCommand(state, split);
    if (info.Failed) info.Print(input);
}