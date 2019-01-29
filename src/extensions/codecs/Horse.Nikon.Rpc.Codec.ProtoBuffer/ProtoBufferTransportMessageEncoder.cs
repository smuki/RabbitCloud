using Horse.Nikon.Rpc.Codec.ProtoBuffer.Messages;
using Horse.Nikon.Rpc.Codec.ProtoBuffer.Utilities;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Transport.Codec;

namespace Horse.Nikon.Rpc.Codec.ProtoBuffer
{
    public sealed class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufferTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}