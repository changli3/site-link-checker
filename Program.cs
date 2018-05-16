
using Abot.Crawler;
using Abot.Poco;
using System;
using System.IO;

namespace Abot.Demo
{
    class Program
    {
        static string rootPath = @"C:\temp\tgov";
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            //PrintDisclaimer();

            Uri uriToCrawl = new Uri("https://www.treasury.gov/about/Pages/Secretary.aspx"); // GetSiteToCrawl(args);

            IWebCrawler crawler;
            crawler = GetCustomBehaviorUsingLambdaWebCrawler();
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            CrawlResult result = crawler.Crawl(uriToCrawl);

        }

        private static IWebCrawler GetCustomBehaviorUsingLambdaWebCrawler()
        {
            IWebCrawler crawler = new PoliteWebCrawler();

            crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
            {

                return new CrawlDecision {

                    Allow = 
                    (pageToCrawl.Uri.AbsoluteUri.StartsWith("https://home.treasury.gov") || 
                    pageToCrawl.Uri.AbsoluteUri.StartsWith("https://www.treasury.gov"))
                    && !pageToCrawl.Uri.AbsoluteUri.EndsWith(".pdf")

                };
            });


            crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
            {
                return new CrawlDecision { Allow = true };
            });

            //Register a lambda expression that will tell Abot to not crawl links on any page that is not internal to the root uri.
            //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPageLinks method is run
            crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
            {
                return new CrawlDecision { Allow = true };
            });

            return crawler;
        }




        static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            Console.WriteLine(e.PageToCrawl.Uri);
        }

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            //Process data
            if (e.CrawledPage.Content.Text != "" )
            {
                var fpath = e.CrawledPage.Uri.ToString();
                if (fpath.IndexOf("?") > 0)
                {
                    fpath = fpath.Substring(0, fpath.IndexOf("?"));
                }
                fpath = getDirectoryFromUrl(fpath);
                fpath = rootPath + fpath;
                ensurePath(fpath);

                try
                {
                    if (!File.Exists(fpath))
                    {
                        var s = e.CrawledPage.Content.Text;
                        var find = s.IndexOf("/NEW HEADER");
                        if (find > -1) s = s.Substring(find);
                        find = s.IndexOf("<!-- Start Bottom Footer -->");
                        if (find > -1) s = s.Substring(0, find);
                        File.WriteAllText(fpath, s);
                    }
                }
                catch { }
            }
        }

        static string getDirectoryFromUrl(string url)
        {
            string fpath = url;
            fpath = fpath.Substring(7).Replace("/","\\");
            if (fpath.EndsWith("\\")) fpath += "default.aspx";
            return fpath + ".htm";
        }

        static void ensurePath(string file)
        {
            string[] folders = file.Split(new char[] { '\\' });
            var d = "C:\\";

            for (var i=1; i<folders.Length-1; i++)
            {
                d = Path.Combine(d, folders[i]);
                if ( !Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
            }
        }

        static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            //Process data
        }

        static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            //Process data
        }
    }
}
