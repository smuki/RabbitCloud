using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Horse.Nikon.Rpc.Codec.MessagePack.Utilities;
using Horse.Nikon.Rpc.Utilities;
using System;

namespace Horse.Nikon.Rpc.Codec.MessagePack.Messages
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

            //如果是简单类型则取短名称，否则取长名称。
            if (code != TypeCode.Object)
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;

            if (valueType == UtilityType.JObjectType || valueType == UtilityType.JArrayType)
                Content = SerializerUtilitys.Serialize(value.ToString());
            else
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

            var typeName = Type.GetType(TypeName);
            if (typeName == UtilityType.JObjectType || typeName == UtilityType.JArrayType)
            {
                var content = SerializerUtilitys.Deserialize<string>(Content);
                return JsonConvert.DeserializeObject(content, typeName);
            }
            else
            {
                return SerializerUtilitys.Deserialize(Content, typeName);
        }
        }
        #endregion Public Method
    }
}
