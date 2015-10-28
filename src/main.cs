using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            
            string postData = JsonConvert.SerializeObject(new UserInput(uname, pword));

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
            Console.WriteLine("User Data:");

            string sessionid = "";
            string[] parts = response.Headers["Set-Cookie"].Split(";"[0]);
            for (int i = 0; i < parts.Length; i++)
            {
                string cookie = parts[i].TrimStart(" "[0]);
                string[] cookieParts = cookie.Split("="[0]);
                if (cookieParts[0] == "scratchsessionsid")
                {
                    sessionid = cookieParts[1];
                }
            }

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            JArray bodyData = (JArray)JsonConvert.DeserializeObject(reader.ReadToEnd());

            User user = new User(bodyData[0]["username"].ToString(),(int) bodyData[0]["id"], sessionid);

            Console.Write("Username: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(user.Username);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("ID: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(user.ID);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Session ID: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(user.SessionID);

            reader.Close();
            responseStream.Close();
            response.Close();
            Console.Read();
        }
    }
}

struct UserInput
{
    public string username;
    public string password;

    public UserInput(string uname, string pword)
    {
        username = uname;
        password = pword;
    }
}

struct User
{
    public string Username;
    public int ID;
    public string SessionID;

    public User(string username, int id, string sessionid)
    {
        Username = username;
        ID = id;
        SessionID = sessionid;
    }
}
