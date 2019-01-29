using Newtonsoft.Json;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Transport.Codec;
using System.Text;

namespace Horse.Nikon.Rpc.Codec.Json
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