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
            Console.WriteLine("Please enter your username:");
            string uname = Console.ReadLine();

            Console.WriteLine("Please enter your password:");
            string pword = Console.ReadLine();

            Console.WriteLine("");
            Console.WriteLine("Attempting Logon...");

            HttpWebRequest login = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/login/");
            login.Referer = "https://scratch.mit.edu";
            login.Headers.Add("Cookie", "scratchcsrftoken=a; scratchlanguage=en;");
            login.Headers.Add("X-CSRFToken", "a");
            login.Headers.Add("X-Requested-With", "XMLHttpRequest");
            login.Host = "scratch.mit.edu";
            login.Method = "POST";

            //Will improve this eventually to avoid SQL injection type inputs
            string postData = "{\"username\":\"" + uname + "\", \"password\":\"" + pword + "\"}";

            Console.WriteLine("Post data: " + postData);

            byte[] byteData = Encoding.UTF8.GetBytes(postData);

            login.ContentType = "application/x-www-form-urlencoded";
            login.ContentLength = byteData.Length;

            Stream dataStream = login.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)login.GetResponse();
            Console.WriteLine("Response: " + response.StatusDescription);
            
            Console.Read();
        }
    }
}
