﻿using Horse.Nikon.Rpc.Messages;

namespace Horse.Nikon.Rpc.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}