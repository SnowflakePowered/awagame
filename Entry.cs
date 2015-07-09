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
        public string RomFileName { get; set; }
        public string RomLanguage { get; set; }
        public string OpenVGDB_RomID { get; set; }
        public string RomSize { get; set; }
        public string HashCRC32 { get; set; }
        public string HashMD5 { get; set; }
        public string HashSHA1 { get; set; }

    }
}
