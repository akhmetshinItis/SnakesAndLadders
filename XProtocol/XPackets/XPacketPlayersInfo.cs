using XProtocol.Serializator;

namespace XProtocol ;

    /// <summary>
    /// Пакет для отправки Json с информацией об игроках
    /// </summary>
    public class XPacketPlayersInfo
    {
        /// <summary>
        /// Json c информацией об игроках
        /// </summary>
        [XField(1)] public string InformationJson;
    }