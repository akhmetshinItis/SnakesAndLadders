using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using XOREncryptor;

namespace XProtocol
{
    public class XPacket
    {
        public byte PacketType { get; private set; }
        public byte PacketSubtype { get; private set; }
        public List<XPacketField> Fields { get; set; } = new List<XPacketField>();
        public bool Protected { get; set; }
        private bool ChangeHeaders { get; set; }

        private XPacket() {}

        public XPacketField GetField(byte id)
        {
            foreach (var field in Fields)
            {
                if (field.FieldID == id)
                {
                    return field;
                }
            }

            return null;
        }

        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }

        private T ByteArrayToFixedObject<T>(byte[] bytes) where T: struct 
        {
            T structure;
            
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            
            try
            {
                structure = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return structure;
        }

        public byte[] FixedObjectToByteArray(object value)
        {
            var rawsize = Marshal.SizeOf(value);
            var rawdata = new byte[rawsize];

            var handle =
                GCHandle.Alloc(rawdata,
                    GCHandleType.Pinned);

            Marshal.StructureToPtr(value,
                handle.AddrOfPinnedObject(),
                false);

            handle.Free();
            return rawdata;
        }

        public byte[] StringToByteArray(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public T GetValue<T>(byte id) where T : struct
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }

            var neededSize = Marshal.SizeOf(typeof(T));

            if (field.FieldSize != neededSize)
            {
                throw new Exception($"Can't convert field to type {typeof(T).FullName}.\n" +
                                    $"We have {field.FieldSize} bytes but we need exactly {neededSize}.");
            }

            return ByteArrayToFixedObject<T>(field.Contents);
        }

        public string GetStringValue(byte id)
        {
            var field = GetField(id);
            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }

            return Encoding.UTF8.GetString(field.Contents);
        }
        

        public void SetValueType(byte id, object structure)
        {
            var test = structure.GetType();
            if (!structure.GetType().IsValueType)
            {
                throw new Exception("Only value types are available.");
            }

            var field = GetField(id);

            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            
            var bytes = FixedObjectToByteArray(structure);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte) bytes.Length;
            field.Contents = bytes;
        }

        public void SetStringValue(byte id, string value)
        {
            var field = GetField(id);
            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }
            
            var bytes = StringToByteArray(value);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }
            field.Contents = bytes;
            field.FieldSize = (byte) bytes.Length;
        }

        public byte[] GetValueRaw(byte id)
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }

            return field.Contents;
        }

        public void SetValueRaw(byte id, byte[] rawData)
        {
            var field = GetField(id);

            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            if (rawData.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte) rawData.Length;
            field.Contents = rawData;
        }

        public static XPacket Create(XPacketType type)
        {
            var t = XPacketTypeManager.GetType(type);
            return Create(t.Item1, t.Item2);
        }

        public static XPacket Create(byte type, byte subtype)
        {
            return new XPacket
            {
                PacketType = type,
                PacketSubtype = subtype
            };
        }

        public byte[] ToPacket()
        {
            var packet = new MemoryStream();

            packet.Write(
                ChangeHeaders
                    ? new byte[] {0x95, 0xAA, 0xFF, PacketType, PacketSubtype}
                    : new byte[] {0xAF, 0xAA, 0xAF, PacketType, PacketSubtype}, 0, 5);

            // Сортируем поля по ID
            var fields = Fields.OrderBy(field => field.FieldID);

            // Записываем поля
            foreach (var field in fields)
            {
                packet.Write(new[] {field.FieldID, field.FieldSize}, 0, 2);
                packet.Write(field.Contents, 0, field.Contents.Length);
            }

            // Записываем конец пакета
            packet.Write(new byte[] {0xFF, 0x00}, 0, 2);

            return packet.ToArray();
        }

        // public static XPacket Parse(byte[] packet, bool markAsEncrypted = false)
        // {
        //     /*
        //      * Минимальный размер пакета - 7 байт
        //      * HEADER (3) + TYPE (1) + SUBTYPE (1) + PACKET ENDING (2)
        //      */
        //     if (packet.Length < 7)
        //     {
        //         return null;
        //     }
        //
        //     var encrypted = false;
        //
        //     // Проверяем заголовок
        //     if (packet[0] != 0xAF ||
        //         packet[1] != 0xAA ||
        //         packet[2] != 0xAF)
        //     {
        //         var headers = XOREcnryptor.XORDecrypt([ packet[0], packet[1], packet[2] ]);
        //         if(headers[0] == 0xAF && headers[1] == 0xAA && headers[2] == 0xAF)
        //         {
        //             encrypted = true;
        //         }
        //         else
        //         {
        //             return null;
        //         }
        //     }
        //
        //     var mIndex = packet.Length - 1;
        //
        //     // Проверяем, что бы пакет заканчивался нужными байтами
        //     if (packet[mIndex - 1] != 0xFF ||
        //         packet[mIndex] != XOREcnryptor.XOREncrypt([0x00])[0])
        //     {
        //         return null;
        //     }
        //
        //     var type = packet[3];
        //     var subtype = packet[4];
        //
        //     var xpacket = new XPacket {PacketType = type, PacketSubtype = subtype, Protected = encrypted};
        //     
        //     var fields = packet.Skip(5).ToArray();
        //     
        //     while (true)
        //     {
        //         if (fields.Length == 2) // Остались последние два байта, завершающие пакет.
        //         {
        //             return encrypted ? Parse(XOREcnryptor.XORDecrypt(xpacket.ToPacket())) : xpacket;
        //         }
        //
        //         var id = fields[0];
        //         var size = fields[1];
        //
        //         var contents = size != 0 ?
        //             fields.Skip(2).Take(size).ToArray() : null;
        //
        //         xpacket.Fields.Add(new XPacketField
        //         {
        //             FieldID = id,
        //             FieldSize = size,
        //             Contents = contents
        //         });
        //
        //         fields = fields.Skip(2 + size).ToArray();
        //     }
        // }
        
    // public static XPacket Parse(byte[] packet, bool markAsEncrypted = false)
    //     {
    //         /*
    //          * Минимальный размер пакета - 7 байт
    //          * HEADER (3) + TYPE (1) + SUBTYPE (1) + PACKET ENDING (2)
    //          */
    //         if (packet.Length < 7)
    //         {
    //             return null;
    //         }
    //
    //         var encrypted = false;
    //
    //         // Проверяем заголовок
    //         if (packet[0] != 0xAF ||
    //             packet[1] != 0xAA ||
    //             packet[2] != 0xAF)
    //         {
    //             if (packet[0] == 0xC7 ||
    //                 packet[1] == 0xCB ||
    //                 packet[2] == 0xDD)
    //             {
    //                 encrypted = true;
    //             }
    //             else
    //             {
    //                 return null;
    //             }
    //         }
    //
    //         var mIndex = packet.Length - 1;
    //
    //         // Проверяем, что бы пакет заканчивался нужными байтами
    //         if (packet[mIndex - 1] != 0xFF ||
    //             packet[mIndex] != 0x00)
    //         {
    //             return null;
    //         }
    //     if (encrypted)
    //     {
    //         packet = XProtocolEncryptor.Decrypt(packet);
    //     }
    //         var type = packet[3];
    //         var subtype = packet[4];
    //
    //         var xpacket = new XPacket {PacketType = type, PacketSubtype = subtype, Protected = markAsEncrypted};
    //         
    //         var fields = packet.Skip(5).ToArray();
    //         
    //         while (true)
    //         {
    //             if (fields.Length == 2) // Остались последние два байта, завершающие пакет.
    //             {
    //                 return xpacket;
    //             }
    //
    //             var id = fields[0];
    //             var size = fields[1];
    //
    //             var contents = size != 0 ?
    //                 fields.Skip(2).Take(size).ToArray() : null;
    //
    //             xpacket.Fields.Add(new XPacketField
    //             {
    //                 FieldID = id,
    //                 FieldSize = size,
    //                 Contents = contents
    //             });
    //
    //             fields = fields.Skip(2 + size).ToArray();
    //         }
    //     }
    
    public static XPacket Parse(byte[] packet, bool markAsEncrypted = false)
{
    /*
     * Минимальный размер пакета - 7 байт
     * HEADER (3) + TYPE (1) + SUBTYPE (1) + PACKET ENDING (2)
     */
    if (packet.Length < 7)
    {
        return null;
    }

    var isEncrypted = false;

    // Проверяем заголовок
    if (packet[0] != 0xAF ||
        packet[1] != 0xAA ||
        packet[2] != 0xAF)
    {
        // Если заголовок зашифрован
        if (packet[0] == 0xC7 &&
            packet[1] == 0xCB &&
            packet[2] == 0xDD)
        {
            isEncrypted = true; // Устанавливаем флаг зашифрованного пакета
        }
        else
        {
            return null; // Неверный заголовок
        }
    }

    // Проверяем, что пакет заканчивается нужными байтами
    var lastIndex = packet.Length - 1;
    if (packet[lastIndex - 1] != 0xFF || packet[lastIndex] != 0x00 || packet[lastIndex] != 0x79 || packet[lastIndex-1] != 0x97)
    {
        return null; // Неверное завершение пакета
    }

    // Если пакет зашифрован, расшифровываем его
    if (isEncrypted)
    {
        packet = XProtocolEncryptor.Decrypt(packet);
    }

    // Извлекаем тип и подтип пакета
    var type = packet[3];
    var subtype = packet[4];

    // Создаем экземпляр XPacket
    var xpacket = new XPacket
    {
        PacketType = type,
        PacketSubtype = subtype,
    };

    // Начинаем обработку полей, начиная с 5-го байта
    var fields = packet.Skip(5).ToArray();

    while (true)
    {
        // Если остались только два байта завершения, возвращаем результат
        if (fields.Length == 2 && fields[0] == 0xFF && fields[1] == 0x00)
        {
            return xpacket;
        }

        if (fields.Length < 2)
        {
            return null; // Некорректный пакет, данных меньше минимально возможного
        }

        var id = fields[0];
        var size = fields[1];

        // Проверяем, достаточно ли данных для текущего поля
        if (fields.Length < 2 + size)
        {
            return null; // Некорректный пакет, данных не хватает
        }

        // Извлекаем содержимое поля
        var contents = size > 0 ? fields.Skip(2).Take(size).ToArray() : null;

        // Добавляем поле в список
        xpacket.Fields.Add(new XPacketField
        {
            FieldID = id,
            FieldSize = size,
            Contents = contents
        });

        // Пропускаем обработанное поле
        fields = fields.Skip(2 + size).ToArray();
    }
}


        public static XPacket EncryptPacket(XPacket packet)
        {
            if (packet == null)
            {
                return null; // Нам попросту нечего шифровать
            }

            var rawBytes = packet.ToPacket(); // получаем пакет в байтах
            var encrypted = XProtocolEncryptor.Encrypt(rawBytes); // шифруем его

            var p = Create(0, 0); // создаем пакет
            p.SetValueRaw(0, encrypted); // записываем данные
            p.ChangeHeaders = true; // помечаем, что нам нужен другой заголовок

            return p;
        }

        public XPacket Encrypt()
        {
            return EncryptPacket(this);
        }

        public XPacket Decrypt()
        {
            return DecryptPacket(this);
        }

        private static XPacket DecryptPacket(XPacket packet)
        {
            if (!packet.HasField(0))
            {
                return null; // Зашифрованные данные должны быть в 0 поле
            }

            var rawData = packet.GetValueRaw(0); // получаем зашифрованный пакет
            var decrypted = XProtocolEncryptor.Decrypt(rawData);

            return Parse(decrypted, true);
        }
    }
}
