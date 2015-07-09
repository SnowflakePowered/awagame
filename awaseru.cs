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
        List<string> DatFilesPath;
        List<XDocument> DatFiles;
        SQLiteConnection Database;

        private awaseru(string directory)
        {
            this.CurrentDirectory = directory;
            this.DatFilesPath = Directory.EnumerateFiles(this.CurrentDirectory).Where(filename => Path.GetExtension(filename) == ".dat").ToList();
            this.DatFiles = this.DatFilesPath.Select(filePath => XDocument.Load(File.OpenRead(filePath))).ToList();
        }
    }
}
