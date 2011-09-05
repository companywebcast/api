using System;
using System.Collections.Generic;
using System.Text;

namespace CwcDownloader
{
    public class DownloadConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RetrieveWebcastAttachments { get; set; }
        public bool RetrieveWebcastStreams { get; set; }
        public bool RetrieveTopicAttachments { get; set; }
        public bool RetrieveSlideAttachments { get; set; }
        public bool ExportWebcastXML { get; set; }
        public bool ReplaceIfExists { get; set; }
        public string FilePath { get; set; }
        public string CustomerName { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public int? DaysAgoFromTodayPeriodFrom { get; set; }
        public int? DaysAgoFromTodayPeriodTo { get; set; }
        public bool UseProxyMode { get; set; }
    }
}
