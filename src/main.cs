using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Scratch_Cloud
{
    class Program
    {

        const string SERVER = "scratch.mit.edu";
        const string PROJECTS_SERVER = "projects.scratch.mit.edu";
        const string CDN_SERVER = "cdn.scratch.mit.edu";
        const string CLOUD_SERVER = "cloud.scratch.mit.edu";
        const int CLOUD_SERVER_PORT = 531;

        static void Main(string[] args)
        {
            //Console.WriteLine("Requesting index page");
            //HttpWebRequest requestIndex = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/");
            //requestIndex.Accept = "text/html";
            //requestIndex.Referer = "https://scratch.mit.edu";

            //HttpWebResponse response1 = (HttpWebResponse)requestIndex.GetResponse();
            
            //Console.WriteLine(response1.StatusDescription);
            
            //Console.WriteLine(response1.Cookies.Count);

            //response1.Close();

            //Console.WriteLine("");

            //Console.WriteLine("Getting CRSF Token");
            
            //HttpWebRequest requestCRSF = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/csrf_token/");
            //requestCRSF.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
            //requestCRSF.Accept = "*/*";
            //requestCRSF.CookieContainer = new CookieContainer(1);
            //requestCRSF.CookieContainer.Add(new Cookie("scratchlanguage", "en", "/", ".scratch.mit.edu"));
            //requestCRSF.Referer = "https://scratch.mit.edu";
            //requestCRSF.Headers.Add("X-Requested-With", "XMLHttpRequest");

            //HttpWebResponse response2 = (HttpWebResponse)requestIndex.GetResponse();

            //Console.WriteLine(response2.StatusDescription);

            //for (int i = 0; i < response2.Headers.AllKeys.Length; i++)
            //{
            //    Console.WriteLine(response2.Headers.AllKeys[i]);
            //}
            
            //response2.Close();

            //Console.WriteLine("");

            HttpWebRequest login = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/login/");
            login.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
            login.Accept = "application / json, text / javascript, */*; q=0.01";
            login.ContentType = "application/json";
            login.Referer = "https://scratch.mit.edu";
            login.Headers.Add("Origin", "https://scratch.mit.edu");
            login.Headers.Add("X-Requested-With", "XMLHttpRequest");
            login.Headers.Add("X-CRSFToken", "a");


            Console.Read();
        }
    }
}
