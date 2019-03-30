using Horse.Nikon.Rpc.Codec.MessagePack.Messages;
using Horse.Nikon.Rpc.Codec.MessagePack.Utilities;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Horse.Nikon.Rpc.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}