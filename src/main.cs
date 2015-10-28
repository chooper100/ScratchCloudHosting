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
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Please enter your username:");
            Console.ForegroundColor = ConsoleColor.Gray;
            string uname = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Please enter your password:");
            Console.ForegroundColor = ConsoleColor.Gray;
            string pword = Console.ReadLine();

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
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

            Console.Write("Post data: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(postData);

            byte[] byteData = Encoding.UTF8.GetBytes(postData);

            login.ContentType = "application/x-www-form-urlencoded";
            login.ContentLength = byteData.Length;

            Stream dataStream = login.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)login.GetResponse();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Response: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(response.StatusDescription);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            Console.WriteLine("Useful Stuff:");
            string[] parts = response.Headers["Set-Cookie"].Split(";"[0]);

            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = 0; i < parts.Length; i++)
            {
                Console.WriteLine(parts[i].Trim(" "[0]));
            }

            response.Close();
            Console.Read();
        }
    }
}
