using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

namespace ExtractPageLink
{
    class Program
    {
        private static string[] all = { };
        private static string fileName = "";
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("try 'guo --help' for more information ");
            }
            else
            {
                string comand = args[0];

                if (comand == "-s")
                {
                    fileName = GetFileFullPath(Path.GetDirectoryName(args[1]));

                    if (!File.Exists(fileName))
                    {
                        FileStream fs = File.Create(fileName);
                        fs.Close();
                    }

                    Task.Factory.StartNew(() =>
                    {
                        all = File.ReadAllLines(args[1]);
                    }).ContinueWith((t) =>
                    {
                        Run();
                    });


                } else if (comand == "-h" || comand == "-help")
                {
                    Console.WriteLine("帮助说明：命令行下执行");
                    Console.WriteLine("1");
                    Console.WriteLine("1");
                }

            }
            //Console.ReadLine();
        }


        public static string GetFileFullPath(string path)
        {
            return path + @"\" + GetFileName();
        }

        public static string GetFileName()
        {
            string name = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" +DateTime.Now.Minute.ToString() + "_" +DateTime.Now.Second.ToString() + ".txt";
            return name;
        }

        public static void Run(string a)
        {
            try
            {
                foreach (var item in all)
                {
                    Console.WriteLine(item);
                    HtmlWeb hw = new HtmlWeb();
                    HtmlDocument doc = hw.Load(item);
                }
            }
            catch (Exception)
            {

                
            }
                
        }

        public static async void Run()
        {
            int i = 1;
            int total = all.Length;
            foreach (var item in all)
            {
                Console.WriteLine(item + " [" + i.ToString() + "/"+ total.ToString() + "]");

                try
                {
                    HtmlWeb hw = new HtmlWeb();

                    HtmlDocument doc = await hw.LoadFromWebAsync(item);

                    List<string> links = ExtractPageLink(doc);

                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        foreach (var line in links)
                        {
                            if (IsFullUrl(line).Trim() != "")
                            {
                                sw.WriteLine(line);
                            }

                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine(item + "请求出错");
                }
                catch(Exception e)
                {

                }

                i++;

            }
            Console.WriteLine("finished");
            System.Environment.Exit(0);
        }

        public static string IsFullUrl(string url)
        {
            try
            {
                Uri u = new Uri(url);
                if (u.Scheme=="http" || u.Scheme=="https")
                {
                    return u.ToString();
                }
                else
                {
                    return "";
                }
                
            }
            catch (Exception)
            {

                return "";
            }
        }
 

        public static List<string> ExtractPageLink(HtmlDocument htmlSnippet)
        {
            List<string> hrefTags = new List<string>();
            HtmlNodeCollection hc = htmlSnippet.DocumentNode.SelectNodes("//a[@href]");
            if (hc != null)
            {
                foreach (HtmlNode link in hc)
                {
                    HtmlAttribute att = link.Attributes["href"];
                    hrefTags.Add(att.Value);
                }
            }
            return hrefTags;
        }

        private static void DisplayResults(string url,string state)
        {
            Console.WriteLine(url + state);
        }

        public static async Task<string> DownloadPageAsync(HttpClient client,string url)
        {
            try
            {
                string body = await client.GetStringAsync(url);
                DisplayResults(url, "succeed");
                return body;
            }
            catch (Exception)
            {
                DisplayResults(url, "error");
                return "";
            }
           
        }
    }
}
