using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OsuDownloader {
    public class UpdateFile {
        [JsonProperty("file_version")]
        public int FileVersion { get; set; }

        [JsonProperty]
        public string Filename { get; set; }

        [JsonProperty("file_hash")]
        public string FileHash { get; set; }
        [JsonProperty]
        public long Filesize { get; set; }

        [JsonProperty]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty]
        public long? PatchId { get; set; }

        [JsonProperty("url_full")]
        public Uri UrlFull { get; set; }

        [JsonProperty("url_patch")]
        public Uri UrlPatch { get; set; }
        
        public override string ToString() {
            return $"UpdateFile {this.Filename} (id:{this.FileVersion} hash:{this.FileHash} b:{this.ToBuild()})";
        }

        public string ToBuild(string updateStream = "") {
            string versionSuffix = Regex.Replace(updateStream.ToLower(), @"[\d-]", string.Empty);
            return "b" + this.Timestamp.ToString("yyyyMMdd") + (versionSuffix == "stable" ? "" : versionSuffix);
        }
    }
}