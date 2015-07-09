using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace awagame
{
    [Serializable]
    [ProtoContract]
    internal class Entry
    {
        [ProtoMember(1)]
        public string GameName { get; set; }
        [ProtoMember(2)]
        public string RomFileName { get; set; }
        [ProtoMember(3)]
        public string RomSize { get; set; }
        [ProtoMember(4)]
        public string HashCRC32 { get; set; }
        [ProtoMember(5)]
        public string HashMD5 { get; set; }
        [ProtoMember(6)]
        public string HashSHA1 { get; set; }
        [ProtoMember(7)]
        public string OpenVGDB_RomID { get; set; }

    }
}
