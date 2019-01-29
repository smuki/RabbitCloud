using Horse.Nikon.Rpc.Messages;

namespace Horse.Nikon.Rpc.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}