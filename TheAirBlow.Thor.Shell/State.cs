using TheAirBlow.Thor.Library.Communication;
using TheAirBlow.Thor.Library.Protocols;

namespace TheAirBlow.Thor.Shell; 

public class State(IHandler handler) {
    public IHandler Handler { get; } = handler;
    public Protocol ProtocolType { get; set; }
    public object Protocol { get; set; } = null!;
}