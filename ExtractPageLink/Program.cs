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
        private static string fileName = "";
        static void Main(string[] args)
        {

            string[] footprint = { };
            string[] urlfilelst = { };
            string filepath = String.Empty;

            if (args.Length == 0)
            {
                Console.WriteLine("请输入命令");
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
                    //Console.WriteLine(args[1]);
                    Task.Factory.StartNew(() =>
                    {
                        urlfilelst = File.ReadAllLines(args[1]);
                    }).ContinueWith((t) =>
                    {
                        Run(urlfilelst);
                    });


                } else if (comand == "-v") {

                    fileName = GetFileFullPath(Path.GetDirectoryName(args[3]));

                    if (!File.Exists(fileName))
                    {
                        FileStream fs = File.Create(fileName);
                        fs.Close();
                    }

                    Task.Factory.StartNew(() =>
                    {
                        footprint = File.ReadAllLines(args[1]);
                        filepath = args[2];
                        urlfilelst = File.ReadAllLines(args[3]);
                    }).ContinueWith((t) =>
                    {
                        Run(footprint, filepath, urlfilelst);
                    });

                } else if (comand == "-t") {
                    Test();
                } else if (comand == "-h" || comand == "-help")
                {
                    Console.WriteLine("帮助说明：命令行下执行");
                    Console.WriteLine("-s 网址列表文件.txt [需要1个参数，提取页面网址]");
                    Console.WriteLine("-v 页面特征码.txt 检测的页面地址 网址列表文件.txt [需要3个参数，验证网址是否包含特征码]");
                }

            }
            Console.ReadLine();
        }

        public static void Test()
        {
            string[] a = new string[] { "test a", "test b", "xx", "33.com" };
            string s = "fsdfsdfdsfsdqrew sd fds";
            foreach (var el in a)
            {
                if (s.ToLower().Contains(el.ToLower()))
                {
                    Console.WriteLine("包含");
                    break; ;
                }
            }
          
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

        public static void Run2(string[] footprint, string[] urlfilelst)
        {
            int i = 1;
            int total = urlfilelst.Length;
            foreach (var item in urlfilelst)
            {
                Console.WriteLine(item + " [开始请求：" + i.ToString() + "/" + total.ToString() + "]");

                try
                {
                    HtmlWeb hw = new HtmlWeb();
  
                    HtmlDocument doc =  hw.Load(item);

                    string html = doc.DocumentNode.OuterHtml;

                    foreach (var el in footprint)
                    {
                        if (html.ToLower().Contains(el.ToLower()))
                        {
                            using (StreamWriter sw = File.AppendText(fileName))
                            {
                                sw.WriteLine(item);
                            }
                            break; ;
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine(item + " [请求出错：" + e.Message + "]");
                }
                catch (Exception e)
                {
                    Console.WriteLine(item + " [请求出错：" + e.Message + "]");
                }

                i++;

            }
            Console.WriteLine("finished");
            System.Environment.Exit(0);
        }

        public async static void Run(string[] footprint, string filepath , string[] urlfilelst)
        {
            int i = 1;
            int total = urlfilelst.Length;
            string url = String.Empty;
            foreach (var item in urlfilelst)
            {
                url = item + "/" + filepath;
                Console.WriteLine(url + " [开始请求：" + i.ToString() + "/" + total.ToString() + "]");

                try
                {
                    HtmlWeb hw = new HtmlWeb();

                    HtmlDocument doc = await hw.LoadFromWebAsync(url);

                    string html = doc.DocumentNode.OuterHtml;

                    foreach (var el in footprint)
                    {
                        if (html.ToLower().Contains(el.ToLower()))
                        {
                            using (StreamWriter sw = File.AppendText(fileName))
                            {
                                sw.WriteLine(url);
                            }
                            break; ;
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine(item + " [请求出错："+ e.Message + "]");
                }
                catch (Exception e)
                {
                    Console.WriteLine(item + " [请求出错：" + e.Message + "]");
                }

                i++;
                url = String.Empty;

            }
            Console.WriteLine("finished");
            System.Environment.Exit(0);
        }


        public static async void Run(string[] urlfilelst)
        {
            int i = 1;
            int total = urlfilelst.Length;
            foreach (var item in urlfilelst)
            {
                Console.WriteLine(item + " [开始请求：" + i.ToString() + "/"+ total.ToString() + "]");

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
                    Console.WriteLine(item + " [请求出错：" + e.Message + "]");
                }
                catch(Exception e)
                {
                    Console.WriteLine(item + " [请求出错：" + e.Message + "]");
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
