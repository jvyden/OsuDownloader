using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OsuDownloader {
    public class Program {
        public static void Main() {
            string[] streams = {"Stable40", "Beta40", "cuttingedge", "lazer", "stable"};
//            string[] streams = {"beta"};

            foreach(string stream in streams) {
                Console.WriteLine("Downloading stream " + stream);
                DownloadStream(stream);
            }
        }
        
        public static void DownloadStream(string updateStream = "Stable40") {
            UpdateFile[] updateFiles = GetUpdateFiles(updateStream);

            UpdateFile exeFile = null;
            
            foreach(UpdateFile updateFile in updateFiles) {
                if(updateFile.Filename == "osu!.exe") exeFile = updateFile;
            }

            exeFile ??= updateFiles[0];

            string versionFolder = "dl/" + exeFile.ToBuild(updateStream) + "/";
            if(!Directory.Exists(versionFolder)) Directory.CreateDirectory(versionFolder);
            if(!Directory.Exists("dl")) Directory.CreateDirectory("dl");

            List<Task> downloads = updateFiles.Select(updateFile => DownloadFile(updateFile, versionFolder)).ToList();

            foreach(Task task in downloads) task.Start();
            Task.WaitAll(downloads.ToArray());

            Console.WriteLine("Done.");
        }

        public static UpdateFile[] GetUpdateFiles(string updateStream = "Stable40", string updateUrl = "https://osu.ppy.sh/web/check-updates.php") {
            string url = string.Format($"{updateUrl}?action=check&stream={updateStream}&time={DateTime.Now.Ticks}");

            Console.WriteLine("Getting update information for " + updateStream);

            HttpWebRequest updateRequest = WebRequest.Create(url) as HttpWebRequest;
            updateRequest!.Method = "GET";
            updateRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            string updateResponse = new StreamReader(updateRequest.GetResponse().GetResponseStream()!).ReadToEnd();

            UpdateFile[] updateFiles;

            try {
                updateFiles = JsonConvert.DeserializeObject<UpdateFile[]>(updateResponse);
            }
            catch {
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(updateResponse);
                if(errorResponse != null) throw new ArgumentException(errorResponse.Response);
                return null;
            }

            return updateFiles;
        }

        public static Task DownloadFile(UpdateFile updateFile, string versionFolder) {
            return new(delegate {
                Console.WriteLine($"Downloading {updateFile.Filename} ({updateFile.Filesize * 0.000001} MB)");

                WebRequest downloadRequest = WebRequest.Create(updateFile.UrlFull);
                downloadRequest.Method = "GET";

                WebResponse response = downloadRequest.GetResponse();
                Stream stream = response.GetResponseStream()!;
                MemoryStream ms = new();

                byte[] buffer = new byte[32768];

                while(true) {
                    int read = stream.Read(buffer, 0, buffer.Length);

                    if(read > 0) ms.Write(buffer, 0, read);
                    else {
                        ms.Seek(0, SeekOrigin.Begin);
                        break;
                    }
                }
                new FileInfo(versionFolder + updateFile.Filename).Directory!.Create();
                File.WriteAllBytes(versionFolder + updateFile.Filename, ms.ToArray());
                Console.WriteLine("Finished downloading " + updateFile.Filename);
            });
        }
    }
}