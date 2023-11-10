using System.Text;
using Serilog;
using TheAirBlow.Thor.Library;

namespace TheAirBlow.Thor.Shell.Commands.ProtoOdin; 

public class RawWrite : ICommand {
    public FailInfo RunCommand(State state, List<string> args) {
        if (args.Count < 2)
            return new FailInfo("Not enough arguments!", args.Count - 1);
        if (!state.Handler.IsConnected())
            return new FailInfo("Not connected to a device!", 1);
        try {
            switch (args[0].ToLower()) {
                case "string": {
                    state.Handler.BulkWrite(Encoding.ASCII.GetBytes(args[1]).OdinAlign());
                    break;
                }
                case "int": {
                    var buf = new byte[(args.Count - 1) * 2];
                    for (var i = 1; i < args.Count; i++) {
                        var bytes = BitConverter.GetBytes(int.Parse(args[i]));
                        var pos = (i - 1) * 2;
                        buf[pos] = bytes[0];
                        buf[pos+1] = bytes[1];
                    }
                    
                    state.Handler.BulkWrite(buf.OdinAlign());
                    break;
                }
                case "bytes": {
                    var buf = new byte[args.Count - 1];
                    for (var i = 1; i < args.Count; i++) {
                        var b = Convert.FromHexString(args[i])[0];
                        buf[i - 1] = b;
                    }
                    
                    state.Handler.BulkWrite(buf.OdinAlign());
                    break;
                }
                default:
                    return new FailInfo("Invalid option choice, must be [[string/int/bytes]]", 1);
            }
        } catch (Exception e) {
            Log.Debug("Full exception: {0}", e);
            return new FailInfo(e.Message, 0);
        }
        
        return new FailInfo();
    }

    public string GetDescription()
        => "write [[string/int/bytes]] <content> - Send a packet of specified type, aligns to odin packet size";
}