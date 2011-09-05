//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Companywebcast">
//     Created by Marcel Brouns.
//     Licensed under Creative Commons (http://creativecommons.org/)
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Collections.Generic;
using CwcDownloader.com.companywebcast.services;

namespace CwcDownloader
{
    class Program
    {
        static MetaService ServiceClient = new MetaService();
        static DownloadConfig Config;

        private static WebcastSummary[] WebcastSearch(int Page)
        {
            try
            {
                int WebcastSearchResult;
                bool WebcastSearchResultSpecified;
                WebcastSummary[] WebcastSummaries;

                bool _fromSpecified = false;
                bool _toSpecified = false;
                DateTime? _from = Config.PeriodFrom;
                DateTime? _to = Config.PeriodTo;

                if (Config.DaysAgoFromTodayPeriodFrom != 0 && Config.DaysAgoFromTodayPeriodFrom > 0)
                {
                    _from = DateTime.Now.Subtract(TimeSpan.FromDays(Config.DaysAgoFromTodayPeriodFrom.Value));
                }
                if (Config.DaysAgoFromTodayPeriodTo != 0 && Config.DaysAgoFromTodayPeriodTo > 0)
                {
                    _to = DateTime.Now.Subtract(TimeSpan.FromDays(Config.DaysAgoFromTodayPeriodTo.Value));
                }
                _fromSpecified = _from == null ? false : true;
                _toSpecified = _to == null ? false : true;

                ServiceClient.WebcastSearch(
                    Config.Username,
                    Config.Password,
                    Config.CustomerName == "" ? null : Config.CustomerName,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    _from,
                    _fromSpecified,
                    _to,
                    _toSpecified,
                    WebcastStatus.Indexed,
                    true,
                    Page,
                    true,
                    100,
                    true,
                    out WebcastSearchResult,
                    out WebcastSearchResultSpecified,
                    out WebcastSummaries);

                if (WebcastSearchResult == 0 && WebcastSummaries.Length > 0)
                {
                    Console.WriteLine("Found " + WebcastSummaries.Length + " Webcasts on page " + Page.ToString());
                    return WebcastSummaries;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                Console.WriteLine("WebcastSearch Failed.");
                return null;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting.");
            string ConfigFile = "Default.xml";
            if (args.Length > 0)
            {
                ConfigFile = args[0];
            }

            try
            {
                 XmlSerializer xml = new XmlSerializer(typeof(DownloadConfig));
                TextReader XMLInput = new StreamReader(ConfigFile);
                Config = (DownloadConfig)xml.Deserialize(XMLInput);
                XMLInput.Close();
                XMLInput.Dispose();
            }
            catch
            {
                Console.WriteLine("Invalid Config file. Ending.");
                return;
            }

            int Page = 0;
            WebcastSummary[] result;
            List<WebcastSummary> WebcastSummaries = new List<WebcastSummary>();

            do
            {
                result = WebcastSearch(Page++);
                if (result != null) WebcastSummaries.AddRange(result);
            } while (result != null && result.Length == 100);

            if (WebcastSummaries.Count == 0)
            {
                Console.WriteLine("Ending.");
                return;
            }

            foreach (WebcastSummary _summary in WebcastSummaries)
            {
                string aPath = Config.UseProxyMode == true ? Config.FilePath : Config.FilePath + @"/" + _summary.Code.Replace("/", "-");
                string aCode = _summary.Code.Split('/')[1];

                if (!Config.UseProxyMode && Directory.Exists(aPath) && Config.ReplaceIfExists)
                {
                    Directory.Delete(aPath, true);
                }
                if (!Directory.Exists(aPath) || Config.UseProxyMode)
                {
                    try
                    {
                        if (!Directory.Exists(aPath)) Directory.CreateDirectory(aPath);

                        int WebcastGetResult;
                        bool WebcastGetResultSpecified;
                        Webcast Webcast;

                        ServiceClient.WebcastGet(
                            Config.Username,
                            Config.Password,
                            _summary.Code,
                            _summary.Languages[0],
                            out WebcastGetResult,
                            out WebcastGetResultSpecified,
                            out Webcast);

                        Console.WriteLine("Retrieved " + Webcast.Code + ".");

                        CookieAwareWebClient WebClient = new CookieAwareWebClient();
                        WebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                        try
                        {
                            WebClient.UploadString(Webcast.RegisterUrl, "POST", "Username=" + Config.Username + "&Password=" + Config.Password);
                            if (Config.RetrieveWebcastStreams || Config.RetrieveWebcastAttachments || Config.UseProxyMode)
                            {
                                bool bbExists = false;
                                foreach (Attachment _attachment in Webcast.Attachments)
                                {
                                    if (_attachment.Location.IndexOf("/bb/") > 0)
                                        bbExists = true;
                                }
                                foreach (Attachment _attachment in Webcast.Attachments)
                                {
                                    if ((_attachment.Location.EndsWith(".wmv") && (Config.RetrieveWebcastStreams || Config.UseProxyMode) && 
                                            (_attachment.Location.IndexOf("/bb/") > 0
                                            || _attachment.Location.IndexOf("/au/") > 0
                                            || (!bbExists && _attachment.Location.IndexOf("/nb/") > 0) )) 
                                        || (!_attachment.Location.EndsWith(".wmv") && Config.RetrieveWebcastAttachments && !Config.UseProxyMode))
                                    {
                                        try
                                        {
                                            string fileName = Config.UseProxyMode ? aCode + ".wmv" : _attachment.Location.Substring(_attachment.Location.LastIndexOf("/") + 1);
                                            Console.WriteLine("Now downloading " + fileName + ".");
                                            WebClient.DownloadFile(_attachment.Location, aPath + @"/" + fileName);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Failed.");
                                        }
                                    }
                                }
                            }
                            if (!Config.UseProxyMode && Config.RetrieveTopicAttachments)
                            {
                                foreach (Topic _topic in Webcast.Topics)
                                {
                                    foreach (Attachment _attachment in _topic.Attachments)
                                    {
                                        try
                                        {
                                            Console.WriteLine("Now downloading " + _attachment.Location.Substring(_attachment.Location.LastIndexOf("/") + 1) + ".");
                                            WebClient.DownloadFile(_attachment.Location, aPath + @"/" + _attachment.Location.Substring(_attachment.Location.LastIndexOf("/") + 1));
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Failed.");
                                        }
                                    }
                                }
                            }
                            if (!Config.UseProxyMode && Config.RetrieveSlideAttachments)
                            {
                                foreach (Slide _slide in Webcast.Slides)
                                {
                                    foreach (Attachment _attachment in _slide.Attachments)
                                    {
                                        try
                                        {
                                            string fileName = _attachment.Location.Substring(_attachment.Location.LastIndexOf("/", _attachment.Location.LastIndexOf("/") - 1) + 1).Replace("/","-");
                                            Console.WriteLine("Now downloading " + fileName + ".");
                                            WebClient.DownloadFile(_attachment.Location, aPath + @"/" + fileName);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Failed.");
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Authorization failed, skipping this Webcast.");
                        }

                        if (!Config.UseProxyMode && Config.ExportWebcastXML)
                        {
                            try
                            {
                                Console.WriteLine("Writing XML.");
                                XmlSerializer x = new XmlSerializer(Webcast.GetType());
                                TextWriter XMLDestination = new StreamWriter(aPath + @"/webcast.xml");
                                x.Serialize(XMLDestination, Webcast);
                                XMLDestination.Close();
                                XMLDestination.Dispose();
                            }
                            catch
                            {
                                Console.WriteLine("Failed.");
                            }
                        }

                        WebClient.Dispose();
                    }
                    catch
                    {
                        Console.WriteLine("Unable to retrieve " + _summary.Code + ".");
                    }
                }
            }
        }
    }
}
