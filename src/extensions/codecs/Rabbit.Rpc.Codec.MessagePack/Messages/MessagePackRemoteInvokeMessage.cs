using MessagePack;
using Rabbit.Rpc.Codec.MessagePack.Messages;
using Rabbit.Rpc.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Codec.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class ParameterItem
    {
        #region Constructor

        public ParameterItem(KeyValuePair<string, object> item)
        {
            Key = item.Key;
            Value = item.Value == null ? null : new DynamicItem(item.Value);
        }

        public ParameterItem()
        {
        }

        #endregion Constructor

        [Key(0)]
        public string Key { get; set; }

        [Key(1)]
        public DynamicItem Value { get; set; }
    }

    [MessagePackObject]
    public class MessagePackRemoteInvokeMessage
    {
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage message)
        {
            ServiceId = message.ServiceId;
            ServiceTag = message.ServiceTag;
            Parameters = message.Parameters?.Select(i => new ParameterItem(i)).ToArray();
        }

        public MessagePackRemoteInvokeMessage()
        {
        }

        /// <summary>
        /// 服务Id。
        /// </summary>
        [Key(0)]
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务Tag
        /// </summary>
        [Key(1)]

        public string ServiceTag { get; set; }

        [Key(2)]
        public ParameterItem[] Parameters { get; set; }

        public RemoteInvokeMessage GetRemoteInvokeMessage()
        {
            return new RemoteInvokeMessage
            {
                Parameters = Parameters?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                ServiceId = ServiceId,
                ServiceTag = ServiceTag
            };
        }
    }
}
