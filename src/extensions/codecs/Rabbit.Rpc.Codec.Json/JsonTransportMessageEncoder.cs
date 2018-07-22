using Newtonsoft.Json;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport.Codec;
using System.Text;

namespace Rabbit.Rpc.Codec.Json
{
    public sealed class JsonTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var content = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(content);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}