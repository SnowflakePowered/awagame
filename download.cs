using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using RestSharp;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.IO.Compression;

namespace awagame
{
    class download
    {
        public async static Task DownloadNoIntro()
        {

            Console.WriteLine("[WARN] No-Intro download will take a long time and may not work due to throttling");
            Console.WriteLine("[WARN] It is probably better to manually download from datomatic.no-intro.org");

            var nointroList = download.GetNoIntroSystems();
            foreach(string system in nointroList)
            {
                await download.DownloadNoIntroDat(system);
            }
            
        }
        public async static Task DownloadRedump()
        {

            var nointroList = download.GetRedumpSystems();
            foreach (string system in nointroList)
            {
                await download.DownloadRedumpDat(system);
            }

        }

        private static IEnumerable<string> GetNoIntroSystems()
        {
            var client = new RestClient("http://datomatic.no-intro.org/?page=download&fun=xml");
            string website = client.Get(new RestRequest()).Content;
            var html = new HtmlDocument();
            html.LoadHtml(website);
            IEnumerable<string> systems = html.DocumentNode.QuerySelectorAll("select[name=sel_s] > option")
                .Select(node => node.Attributes["value"].Value)
                .Select(system => system.Replace(' ', '+')); //Get available select options in sel_s and replace spaces with +
            return systems;
        }
        private static IEnumerable<string> GetRedumpSystems()
        {
         
            var client = new RestClient("http://redump.org/");
            string website = client.Get(new RestRequest()).Content;
            var html = new HtmlDocument();
            html.LoadHtml(website);
            IEnumerable<string> systems = html.DocumentNode.QuerySelectorAll("#submenu3 > a")
                .Select(node => node.Attributes["href"].Value)
                .Where(system => system.StartsWith("/datfile/"))
                .Where(system => !system.EndsWith("-bios/"));
            return systems;
        }
        private async static Task DownloadNoIntroDat(string sel_s, int delay=0)
        {
            if (delay > 0)
            {
                Console.WriteLine("[WARN] Must wait " + (delay / 1000 / 60).ToString() + " minutes due to throttling.");
                await Task.Delay(delay);
            }
            Console.WriteLine("[INFO] Downloading " + sel_s.Replace('+', ' '));
            var cookies = new CookieContainer();
            var client = new RestClient("http://datomatic.no-intro.org/?page=download&fun=xml"); //Download the P/Clone
            client.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.132 Safari/537.36";
            client.CookieContainer = cookies; //DAT-o-MATIC relies on cookies
            Console.WriteLine("[INFO] Waiting 1 minute to avoid throttling");
            await Task.Delay(60000);
            await client.ExecuteGetTaskAsync(new RestRequest()); //Save the session cookie
            var setSystemRequest = new RestRequest(Method.POST);
            setSystemRequest.AddParameter("application/x-www-form-urlencoded", "sel_s="+sel_s, ParameterType.RequestBody);
            setSystemRequest.AddHeader("DNT", "1");
            setSystemRequest.AddHeader("Referer", "http://datomatic.no-intro.org/index.php?page=download");
            setSystemRequest.AddHeader("Host", "datomatic.no-intro.org");
            setSystemRequest.AddHeader("Cache-Control", "max-age=0");
            Console.WriteLine("[INFO] Waiting 1 minute to avoid throttling");
            await Task.Delay(60000);
            await client.ExecutePostTaskAsync(setSystemRequest); //POST the system.
           
            var downloadRequest = new RestRequest(Method.POST);
            downloadRequest.AddParameter("application/x-www-form-urlencoded", "Download=Download", ParameterType.RequestBody);
            downloadRequest.AddHeader("DNT", "1");
            downloadRequest.AddHeader("Referer", "http://datomatic.no-intro.org/index.php?page=download");
            downloadRequest.AddHeader("Host", "datomatic.no-intro.org");
            downloadRequest.AddHeader("Cache-Control", "max-age=0");
            Console.WriteLine("[INFO] Waiting 1 minute to avoid throttling");

            await Task.Delay(60000);
            var response = await client.ExecuteTaskAsync(downloadRequest); //POST the download request
            if (response.ResponseUri.OriginalString.StartsWith("http://datomatic.no-intro.org/index.php?page=message"))
            {
                Console.WriteLine("[WARN] Throttle detected, must wait 5 minutes before retrying");
                await download.DownloadNoIntroDat(sel_s, delay + 600000);
            }
            else
            {
                File.WriteAllBytes(sel_s.Replace('+', ' ') + ".zip", response.RawBytes);
                using (var package = new ZipArchive(new MemoryStream(response.RawBytes)))
                {
                    foreach (var entry in package.Entries)
                    {
                        if (Path.GetExtension(entry.FullName) == ".dat")
                        {
                            entry.ExtractToFile("nointro_" + entry.Name, true);
                        }
                    }
                }
                Console.WriteLine("[INFO] Download Complete");
            }
            
        }
        private async static Task DownloadRedumpDat(string href)
        {
            var client = new RestClient("http://redump.org"+href); //Download the P/Clone
            Console.WriteLine("[INFO] Downloading redump.org" + href);
            var response = await client.ExecuteGetTaskAsync(new RestRequest());
            try
            {
                using (var package = new ZipArchive(new MemoryStream(response.RawBytes)))
                {
                    foreach (var entry in package.Entries)
                    {
                        if (Path.GetExtension(entry.FullName) == ".dat")
                        {
                            entry.ExtractToFile("redump_" + entry.Name, true);
                        }
                    }
                }
                Console.WriteLine("[INFO] Download Complete");
            }
            catch(IOException)
            {
                Console.WriteLine("[WARN] Could not extract redump.org (file already exists?) " + href);
            }
        }

        public async static Task DownloadOpenVGDB()
        {
            Console.WriteLine("[INFO] Please wait, downloading OpenVGDB 21");
            byte[] data = await new WebClient().DownloadDataTaskAsync("https://github.com/OpenVGDB/OpenVGDB/releases/download/v21.0/openvgdb.zip");
            Console.WriteLine("[INFO] Extracting OpenVGDB");
            using (var package = new ZipArchive(new MemoryStream(data)))
            {
                foreach (var entry in package.Entries)
                {
                    if (entry.Name == "openvgdb.sqlite")
                    {
                        entry.ExtractToFile(entry.Name, true);
                    }
                }
            }
            Console.WriteLine("[INFO] Download Complete");
        }
        public async static Task DownloadTOSEC()
        {
            var client = new RestClient("http://www.tosecdev.org/downloads/category/22-datfiles");
            string website = client.Get(new RestRequest()).Content;
            var html = new HtmlDocument();
            html.LoadHtml(website);
            string latest = "http://www.tosecdev.org" + html.DocumentNode.QuerySelectorAll(".pd-subcategory").First()
                .QuerySelectorAll("a").First().Attributes["href"].Value;
            string downloadwebsite = new WebClient().DownloadString(latest);
            html.LoadHtml(downloadwebsite);
            string download = "http://www.tosecdev.org" + html.DocumentNode.QuerySelectorAll(".pd-button-download > a").First().Attributes["href"].Value;
            Console.WriteLine("[INFO] Please wait, downloading latest TOSEC DATs package");
            Console.WriteLine(download);
            Console.WriteLine("[INFO] This will take a while. Get a coffee while you wait.");
            var response = await new RestClient(download).ExecuteTaskAsync(new RestRequest());
            try
            {
                using (var package = new ZipArchive(new MemoryStream(response.RawBytes)))
                {
                    foreach (var entry in package.Entries)
                    {
                        if (Path.GetExtension(entry.FullName) == ".dat")
                        {
                            entry.ExtractToFile("tosec_" + entry.Name, true);
                            Console.WriteLine("[INFO] Extracted TOSEC DAT " + "tosec_" + entry.Name);

                        }
                    }
                }
                Console.WriteLine("[INFO] Download Complete");
            }
            catch (IOException)
            {
                Console.WriteLine("[WARN] Could not extract tosecdev.org");
            }
        }
    }
}
