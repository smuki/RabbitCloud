using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Rpc.Codec.MessagePack
{
    public class MessagePackTransportMessageCodecFactory : ITransportMessageCodecFactory
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder = new MessagePackTransportMessageEncoder();
        private readonly ITransportMessageDecoder _transportMessageDecoder = new MessagePackTransportMessageDecoder();

        #endregion Field

        #region Implementation of ITransportMessageCodecFactory

        /// <summary>
        /// 获取编码器。
        /// </summary>
        /// <returns>编码器实例。</returns>
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        /// <summary>
        /// 获取解码器。
        /// </summary>
        /// <returns>解码器实例。</returns>
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        #endregion Implementation of ITransportMessageCodecFactory
    }
}