using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rabbit.Rpc.Codec.MessagePack.Utilitys;
using System;

namespace Rabbit.Rpc.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class DynamicItem
    {
        #region Constructor

        public DynamicItem()
        { }

        public DynamicItem(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var valueType = value.GetType();
            var code = Type.GetTypeCode(valueType);

            //����Ǽ�������ȡ�����ƣ�����ȡ�����ơ�
            if (code != TypeCode.Object)
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;

            Content = SerializerUtilitys.Serialize(value);
        }

        #endregion Constructor

        #region Property

        [Key(0)]
        public string TypeName { get; set; }

        [Key(1)]
        public byte[] Content { get; set; }
        #endregion Property

        #region Public Method
        public object Get()
        {
            if (Content == null || TypeName == null)
                return null;

            return SerializerUtilitys.Deserialize(Content, Type.GetType(TypeName));
        }

        #endregion Public Method
    }
}
