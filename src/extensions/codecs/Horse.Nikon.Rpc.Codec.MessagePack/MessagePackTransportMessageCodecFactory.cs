using Horse.Nikon.Rpc.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Horse.Nikon.Rpc.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageCodecFactory : ITransportMessageCodecFactory
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]		
        public ITransportMessageEncoder GetEncoder()
        {
            return _transportMessageEncoder;
        }

        /// <summary>
        /// 获取解码器。
        /// </summary>
        /// <returns>解码器实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]		
        public ITransportMessageDecoder GetDecoder()
        {
            return _transportMessageDecoder;
        }

        #endregion Implementation of ITransportMessageCodecFactory
    }
}