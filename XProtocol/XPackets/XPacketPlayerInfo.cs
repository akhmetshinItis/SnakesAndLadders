using XProtocol.Serializator;

namespace XProtocol ;

    /// <summary>
    /// Пакет для отправки Json с информацией об игроках
    /// </summary>
    public class XPacketPlayerInfo
    {
        /// <summary>
        /// Json c информацией об игроках
        /// </summary>
        [XField(1)] public string InformationJson;
    }