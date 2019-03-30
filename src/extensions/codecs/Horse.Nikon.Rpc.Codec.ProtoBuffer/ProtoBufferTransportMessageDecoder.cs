using Horse.Nikon.Rpc.Codec.ProtoBuffer.Messages;
using Horse.Nikon.Rpc.Codec.ProtoBuffer.Utilities;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Transport.Codec;

namespace Horse.Nikon.Rpc.Codec.ProtoBuffer
{
    public sealed class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufferTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}