using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Data.SQLite;
using System.Net;
using System.Diagnostics;
namespace awagame
{
    public class awaseru
    {
        
        public static void BuildCurrentDirectory()
        {
            string directory = Environment.CurrentDirectory;
            var combine = new awaseru(directory);
           
        }
        
        string CurrentDirectory;
        IList<string> DatFilesPath;
        IList<XDocument> DatFiles;
        IList<Entry> GameEntries;
        SQLiteConnection Database;

        private awaseru(string directory)
        {
            this.CurrentDirectory = directory;
            this.DatFilesPath = Directory.EnumerateFiles(this.CurrentDirectory).Where(filename => Path.GetExtension(filename) == ".dat").ToList();
            this.DatFiles = new List<XDocument>();
            foreach (string datFile in this.DatFilesPath)
            {
                this.DatFiles.Add(XDocument.Load(File.OpenRead(datFile)));
                Trace.WriteLine("[INFO] Loaded DAT file " + Path.GetFileName(datFile));
            
            }
            this.GameEntries = this.DatFiles.SelectMany(datFile => GetEntries(datFile)).ToList();
            Console.WriteLine();
        }

        IList<Entry> GetEntries(XDocument xmlDat)
        {
            IList<Entry> gameEntries = new List<Entry>();
            var _entries = xmlDat.Root.Elements("game");
            gameEntries = _entries.SelectMany(game => game.Elements("rom").Select(rom => new Entry()
                {
                    GameName = (string)game.Attribute("name"),
                    RomFileName = (string)rom.Attribute("name"),
                    RomSize = (string)rom.Attribute("size"),
                    HashCRC32 = (string)rom.Attribute("crc"),
                    HashMD5 = (string)rom.Attribute("md5"),
                    HashSHA1 = (string)rom.Attribute("sha1")
                })).ToList();
            return gameEntries;
        }


    }
}
