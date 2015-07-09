using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awagame
{
    [Serializable]
    internal class Entry
    {
        public string GameName { get; set; }
        public string RomFileName { get; set; }
        public string RomSize { get; set; }
        public string HashCRC32 { get; set; }
        public string HashMD5 { get; set; }
        public string HashSHA1 { get; set; }
        public string OpenVGDB_RomID { get; set; }

    }
}
