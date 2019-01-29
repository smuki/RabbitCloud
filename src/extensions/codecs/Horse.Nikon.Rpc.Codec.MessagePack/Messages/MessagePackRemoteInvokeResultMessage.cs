using MessagePack;
using Horse.Nikon.Rpc.Messages;

namespace Horse.Nikon.Rpc.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class MessagePackRemoteInvokeResultMessage
    {
        #region Constructor

        public MessagePackRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Content == null ? null : new DynamicItem(message.Content);
        }

        public MessagePackRemoteInvokeResultMessage()
        {
        }

        #endregion Constructor

        [Key(0)]
        public string ExceptionMessage { get; set; }

        [Key(1)]
        public DynamicItem Result { get; set; }

        public RemoteInvokeResultMessage GetRemoteInvokeResultMessage()
        {
            return new RemoteInvokeResultMessage
            {
                ExceptionMessage = ExceptionMessage,
                Content = Result?.Get()
            };
        }
    }
}

