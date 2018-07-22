using Rabbit.Rpc.Codec.MessagePack.Messages;
using Rabbit.Rpc.Codec.MessagePack.Utilitys;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Rpc.Codec.MessagePack
{
    public class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);

            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}