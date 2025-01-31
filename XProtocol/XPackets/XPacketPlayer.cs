using XProtocol.Serializator;

namespace XProtocol
{
    public class XPacketPlayer
    {
        [XField(1)] public string Name;
        [XField(2)] public int Color;
    }
}