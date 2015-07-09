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
using Newtonsoft.Json;
namespace awagame
{
    public class awaseru
    {

        public static void BuildCurrentDirectorySqlite(string fileName)
        {
            string directory = Environment.CurrentDirectory;
            var combine = new awaseru(directory);
            awaseru.BuildDatabase(fileName, combine.GameEntries);
        }
        public static void BuildCurrentDirectoryJson(string fileName)
        {
            string directory = Environment.CurrentDirectory;
            var combine = new awaseru(directory);
            awaseru.BuildJson(fileName, combine.GameEntries);
        }

        string CurrentDirectory;
        IList<string> DatFilesPath;
        IList<XDocument> DatFiles;
        HashSet<Entry> GameEntries;
        private awaseru(string directory)
        {
            this.CurrentDirectory = directory;
            this.DatFilesPath = Directory.EnumerateFiles(this.CurrentDirectory).Where(filename => Path.GetExtension(filename) == ".dat").ToList();
            this.DatFiles = new List<XDocument>();
            foreach (string datFile in this.DatFilesPath)
            {
                try
                {
                    this.DatFiles.Add(XDocument.Load(File.OpenRead(datFile)));
                    Trace.WriteLine("[INFO] Loaded DAT file " + Path.GetFileName(datFile));
                }
                catch (XmlException)
                {
                    Trace.WriteLine("[WARN] Could not load " + Path.GetFileName(datFile));
                }
            }
            Trace.WriteLineIf(Program.OpenVGDB, "[INFO] Checking all records against OpenVGDB, this will take a while.");
            this.GameEntries = new HashSet<Entry>(this.DatFiles.SelectMany(datFile => awaseru.GetEntries(datFile)).OrderBy(entry => entry.GameName));
        }

        static IEnumerable<Entry> GetEntries(XDocument xmlDat)
        {
            var _entries = xmlDat.Root.Elements("game");
            if (!Program.OpenVGDB)
            {
                return _entries.SelectMany(game => game.Elements("rom").Select(rom => new Entry()
                    {
                        GameName = (string)game.Attribute("name"),
                        RomFileName = (string)rom.Attribute("name"),
                        RomSize = (string)rom.Attribute("size"),
                        HashCRC32 = ((string)rom.Attribute("crc")).ToUpperInvariant(),
                        HashMD5 = ((string)rom.Attribute("md5")).ToUpperInvariant(),
                        HashSHA1 = ((string)rom.Attribute("sha1")).ToUpperInvariant()
                    }));
            }
            else
            {
                SQLiteConnection openvgdb = new SQLiteConnection("Data Source=openvgdb.sqlite;Version=3;"); //Load in openvgdb
                openvgdb.Open();
                IDictionary<string, string> romids = new Dictionary<string, string>(); //load sha1s from openvgdb
                using (SQLiteCommand cmd = new SQLiteCommand(@"select `romHashSHA1`, `romID` from `ROMs`", openvgdb))
                {
                    SQLiteDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        romids[Convert.ToString(r["romHashSHA1"])] = Convert.ToString(r["romID"]);
                    }
                }
                openvgdb.Close();
                var entries = _entries.SelectMany(game => game.Elements("rom").Select(rom => new Entry()
                {
                    GameName = (string)game.Attribute("name"),
                    RomFileName = (string)rom.Attribute("name"),
                    RomSize = (string)rom.Attribute("size"),
                    HashCRC32 = ((string)rom.Attribute("crc")).ToUpperInvariant(),
                    HashMD5 = ((string)rom.Attribute("md5")).ToUpperInvariant(),
                    HashSHA1 = ((string)rom.Attribute("sha1")).ToUpperInvariant()
                })).GroupBy(entry => entry.HashSHA1).Select(x => x.First()).ToArray(); //dedupe to save on time
  
                foreach (Entry gameEntry in entries)
                {
                    if (romids.ContainsKey(gameEntry.HashSHA1))
                    {
                        gameEntry.OpenVGDB_RomID = romids[gameEntry.HashSHA1];
                        if (Program.Verbose)
                        {
                            Console.WriteLine("[INFO] OpenVGDB: Matched ROM ID " + gameEntry.OpenVGDB_RomID);
                        }

                    }
                    else
                    {
                        if (Program.Verbose)
                        {
                            Console.WriteLine("[INFO] OpenVGDB: Unable to match " + gameEntry.HashSHA1);
                        }
                    }

                }
                return entries;
            }
        }

        async static void BuildDatabase(string fileName, IEnumerable<Entry> entries)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (IOException)
            {

            }
            finally
            {
                SQLiteConnection.CreateFile(fileName);
            }
            SQLiteConnection database = new SQLiteConnection("Data Source=:memory:;Version=3;"); //use a memory database for speed purposes
            SQLiteConnection disk = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");

            database.Open();
            using (var sqlCommand = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS roms(
                                                                gamename TEXT,
                                                                romname TEXT,
                                                                size TEXT,
                                                                crc TEXT,
                                                                md5 TEXT,
                                                                sha1 TEXT PRIMARY KEY,
                                                                romID TEXT
                                                                )", database))
            {
                sqlCommand.ExecuteNonQuery();
            }
            foreach (Entry gameEntry in entries)
            {
                using (var sqlCommand = new SQLiteCommand(@"INSERT OR IGNORE INTO roms VALUES(
                                              @gamename,
                                              @romname,
                                              @size,
                                              @crc,
                                              @md5,
                                              @sha1,
                                              @romID)", database))
                {
                    sqlCommand.Parameters.AddWithValue("@gamename", gameEntry.GameName);
                    sqlCommand.Parameters.AddWithValue("@romname", gameEntry.RomFileName);
                    sqlCommand.Parameters.AddWithValue("@size", gameEntry.RomSize);
                    sqlCommand.Parameters.AddWithValue("@crc", gameEntry.HashCRC32);
                    sqlCommand.Parameters.AddWithValue("@md5", gameEntry.HashMD5);
                    sqlCommand.Parameters.AddWithValue("@sha1", gameEntry.HashSHA1);
                    sqlCommand.Parameters.AddWithValue("@romID", gameEntry.OpenVGDB_RomID);

                    await sqlCommand.ExecuteNonQueryAsync();
                    if (Program.Verbose)
                    {
                        await Console.Out.WriteLineAsync(String.Format("[INFO] Added ROM record {0} with game record {1} (SHA {2})",
                        gameEntry.RomFileName, gameEntry.GameName, gameEntry.HashSHA1));
                    }
                }
            }
            disk.Open();
            Console.WriteLine("Saving Database.");
            database.BackupDatabase(disk, "main", "main", -1, null, 0);
            Console.WriteLine("Save complete.");
            disk.Close();
            database.Close();

        }

        static void BuildJson(string fileName, IEnumerable<Entry> entries)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (IOException)
            {

            }
            Console.WriteLine("Saving JSON.");
            File.WriteAllText(fileName, JsonConvert.SerializeObject(entries.ToDictionary(entry => entry.HashSHA1)));
            Console.WriteLine("Save complete.");

        }
    }
}
