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
            Title("Please enter your username:", true);
            string uname = Console.ReadLine();

            Title("Please enter your password:", true);
            string pword = Console.ReadLine();

            Console.WriteLine("");
            Title("Attempting Logon...", true);

            HttpWebRequest login = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/login/");
            login.Referer = "https://scratch.mit.edu";
            login.Headers.Add("Cookie", "scratchcsrftoken=a; scratchlanguage=en;");
            login.Headers.Add("X-CSRFToken", "a");
            login.Headers.Add("X-Requested-With", "XMLHttpRequest");
            login.Host = "scratch.mit.edu";
            login.Method = "POST";
            
            string postData = JsonConvert.SerializeObject(new UserInput(uname, pword));

            Title("Post Data: ", false);
            Console.WriteLine(postData);

            byte[] byteData = Encoding.UTF8.GetBytes(postData);

            login.ContentType = "application/x-www-form-urlencoded";
            login.ContentLength = byteData.Length;

            Stream dataStream = login.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)login.GetResponse();

            Title("Response: ", false);
            Console.WriteLine(response.StatusDescription);

            Console.WriteLine("");
            Title("User Data: ", true);

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
            
            Title("Username: ", false);
            Console.WriteLine(user.Username);

            Title("ID: ", false);
            Console.WriteLine(user.ID);

            Title("Session ID: ", false);
            Console.WriteLine(user.SessionID);

            reader.Close();
            responseStream.Close();
            response.Close();
            Console.Read();
        }
        
        private static void Title(string message, bool newLine)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (newLine)
            {
                Console.WriteLine(message);
            } else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
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
