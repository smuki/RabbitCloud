using ProtoBuf;
using Rabbit.Rpc.Messages;

namespace Rabbit.Rpc.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
    public class ProtoBufferRemoteInvokeResultMessage
    {
        #region Constructor

        public ProtoBufferRemoteInvokeResultMessage(RemoteInvokeResultMessage message)
        {
            ExceptionMessage = message.ExceptionMessage;
            Result = message.Content == null ? null : new DynamicItem(message.Content);
        }

        public ProtoBufferRemoteInvokeResultMessage()
        {
        }

        #endregion Constructor

        /// <summary>
        /// 异常消息。
        /// </summary>
        [ProtoMember(1)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 结果内容。
        /// </summary>
        [ProtoMember(2)]
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