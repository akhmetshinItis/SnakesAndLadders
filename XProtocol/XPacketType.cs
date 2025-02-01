namespace XProtocol
{
    public enum XPacketType
    {
        Unknown,
        Handshake,
        NewPlayer,
        PlayersInfo,
        RequestPlayerInfo,
        DisconnectPlayer,
        PlayerInfoNotAvailable,
    }
}
