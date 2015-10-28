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
            //Get user input

            Heading("Please enter your username:", true);
            string uname = Console.ReadLine();

            Heading("Please enter your password:", true);
            string pword = Console.ReadLine();

            Console.WriteLine("");
            Heading("Attempting Logon...", true);
            
            Scratch.Logon(uname, pword);

            Console.WriteLine("");
            Heading("Login Successful", true);

            //Wait for enter key press
            Console.Read();
        }
        
        /// <summary>
        /// Outputs a heading to the console.
        /// </summary>
        /// <param name="message">The message of the heading.</param>
        /// <param name="newLine">Indicates whether to append a new line to the end of the message.</param>
        private static void Heading(string message, bool newLine)
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

    /// <summary>
    /// Provides methods for interacting with Scratch.
    /// </summary>
    static class Scratch
    {
        /// <summary>
        /// Represents the current user.
        /// </summary>
        public static UserInfo User;

        /// <summary>
        /// Logs in to Scratch with the specified username and password combination.
        /// </summary>
        /// <param name="uname">The username to use.</param>
        /// <param name="pword">The password to use.</param>
        public static void Logon(string uname, string pword)
        {
            //Prepare login request
            HttpWebRequest login = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/login/");
            login.Referer = "https://scratch.mit.edu"; // Required by Scratch servers
            login.Headers.Add("Cookie", "scratchcsrftoken=a; scratchlanguage=en;");
            login.Headers.Add("X-CSRFToken", "a");
            login.Headers.Add("X-Requested-With", "XMLHttpRequest");
            login.Host = "scratch.mit.edu";
            login.Method = "POST";

            //Generate post data (JSON object with properties username and password)
            string postData = JsonConvert.SerializeObject(new UserInput(uname, pword));
            
            //Encode post data as byte array
            byte[] byteData = Encoding.UTF8.GetBytes(postData);

            login.ContentType = "application/x-www-form-urlencoded";
            login.ContentLength = byteData.Length;

            //Write post data to request data stream
            Stream dataStream = login.GetRequestStream();
            dataStream.Write(byteData, 0, byteData.Length);
            dataStream.Close();

            //Get request response
            HttpWebResponse response = (HttpWebResponse)login.GetResponse();
            
            //Get session id
            string sessionid = "";
            string[] parts = response.Headers["Set-Cookie"].Split(";"[0]);
            for (int i = 0; i < parts.Length; i++) //For each cookie
            {
                string cookie = parts[i].TrimStart(" "[0]); //Remove leading spaces
                string[] cookieParts = cookie.Split("="[0]); //Split by equals sine
                if (cookieParts[0] == "scratchsessionsid") //If cookie contains the session id
                {
                    sessionid = cookieParts[1]; //Set sessionid to the session id
                }
            }

            //Read response data
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            //Deserialize JSON object
            JArray bodyData = (JArray)JsonConvert.DeserializeObject(reader.ReadToEnd());

            //Create user
            User = new UserInfo(bodyData[0]["username"].ToString(), (int)bodyData[0]["id"], sessionid);
            
            //Clean up
            reader.Close();
            responseStream.Close();
            response.Close();
        }

        /// <summary>
        /// Used for encoding login request data
        /// </summary>
        private struct UserInput
        {
            public string username;
            public string password;

            public UserInput(string uname, string pword)
            {
                username = uname;
                password = pword;
            }
        }
    }
    

    /// <summary>
    /// Contains user info
    /// </summary>
    struct UserInfo
    {
        public string Username;
        public int ID;
        public string SessionID;

        public UserInfo(string username, int id, string sessionid)
        {
            Username = username;
            ID = id;
            SessionID = sessionid;
        }
    }
}
