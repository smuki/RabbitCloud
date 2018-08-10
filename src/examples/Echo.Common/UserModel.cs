using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
namespace Jacob.Common
{
    [ProtoContract]
    public class UserModel
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int Age { get; set; }
    }

}
