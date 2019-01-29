using Horse.Nikon.Rpc.Codec.MessagePack.Messages;
using Horse.Nikon.Rpc.Codec.MessagePack.Utilities;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Transport.Codec;

namespace Horse.Nikon.Rpc.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new MessagePackTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}