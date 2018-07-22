using MessagePack;
using Rabbit.Rpc.Codec.Codec.MessagePack.Messages;
using Rabbit.Rpc.Codec.MessagePack.Utilitys;
using Rabbit.Rpc.Messages;
using System;

namespace Rabbit.Rpc.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class MessagePackTransportMessage
    {
        public MessagePackTransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            ContentType = transportMessage.ContentType;

            object contentObject;
            if (transportMessage.IsInvokeMessage())
            {
                contentObject = new MessagePackRemoteInvokeMessage(transportMessage.GetContent<RemoteInvokeMessage>());
            }
            else if (transportMessage.IsInvokeResultMessage())
            {
                contentObject = new MessagePackRemoteInvokeResultMessage(transportMessage.GetContent<RemoteInvokeResultMessage>());
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }

            Content = SerializerUtilitys.Serialize(contentObject);
        }

        public MessagePackTransportMessage()
        {
        }

        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public byte[] Content { get; set; }

        [Key(2)]
        public string ContentType { get; set; }

        public bool IsInvokeMessage()
        {
            return ContentType == typeof(RemoteInvokeMessage).FullName;
        }

        public bool IsInvokeResultMessage()
        {
            return ContentType == typeof(RemoteInvokeResultMessage).FullName;
        }

        public TransportMessage GetTransportMessage()
        {
            var message = new TransportMessage
            {
                ContentType = ContentType,
                Id = Id,
                Content = null
            };

            object contentObject;
            if (IsInvokeMessage())
            {
                contentObject =
                    SerializerUtilitys.Deserialize<MessagePackRemoteInvokeMessage>(Content).GetRemoteInvokeMessage();
            }
            else if (IsInvokeResultMessage())
            {
                contentObject =
                    SerializerUtilitys.Deserialize<MessagePackRemoteInvokeResultMessage>(Content)
                        .GetRemoteInvokeResultMessage();
            }
            else
            {
                throw new NotSupportedException($"无法支持的消息类型：{ContentType}！");
            }
            message.Content = contentObject;
            return message;
        }
    }
}