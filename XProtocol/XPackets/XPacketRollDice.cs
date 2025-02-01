using XProtocol.Serializator;

namespace XProtocol ;

    public class XPacketRollDice
    {
        [XField(1)] public string Name;
        [XField(2)] public int Score;
    }