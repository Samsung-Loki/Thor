using TheAirBlow.Thor.Library.Communication;
using TheAirBlow.Thor.Library.Protocols;

namespace ThorRewrite.Shell; 

public class State {
    public IHandler Handler;
    public Protocol ProtocolType;
    public object Protocol;

    public State(IHandler handler)
        => Handler = handler;
}