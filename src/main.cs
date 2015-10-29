using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

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

            //Log on

            Console.WriteLine("");
            Heading("Attempting Logon...", true);
            
            UserInfo user = Scratch.Logon(uname, pword);

            Console.WriteLine("");
            Heading("Login Successful", true);

            //Find cloud session

            Console.WriteLine("");
            Heading("Finding Cloud Session...", true);

            CloudSession cloud = CloudSession.Create(user, 85458898);

            Heading("Cloud Session Found", true);

            //Connect to cloud session

            Console.WriteLine("");
            Heading("Connecting to Cloud Session...", true);

            cloud.Connect();

            Heading("Connected...?", true);

            //Attempt set var

            cloud.Set("Test", 123);

            Heading("Done", true);

            //Wait for enter key press
            Console.Read();

            //Clean up
            cloud.Dispose();
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
    /// Provides methods for interacting with https://scratch.mit.edu/.
    /// </summary>
    static class Scratch
    {
        /// <summary>
        /// Logs in to Scratch with the specified username and password combination.
        /// </summary>
        /// <param name="uname">The username to use.</param>
        /// <param name="pword">The password to use.</param>
        public static UserInfo Logon(string uname, string pword)
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

            //Clean up
            reader.Close();
            responseStream.Close();
            response.Close();

            //Create user
            return new UserInfo(bodyData[0]["username"].ToString(), (int)bodyData[0]["id"], sessionid);
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
    /// Represents a user.
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

    class CloudSession : IDisposable
    {
        public UserInfo User;
        public int ProjectId;
        public string CloudId;
        public string Hash;

        UdpClient udpClient;

        /// <summary>
        /// Creates a new <see cref="CloudSession"/> object.
        /// </summary>
        /// <param name="user">The user to interact with the cloud data.</param>
        /// <param name="projectId">The project id to connect to.</param>
        public static CloudSession Create(UserInfo user, int projectId)
        {
            //Prepare login request
            HttpWebRequest login = (HttpWebRequest)WebRequest.Create("https://scratch.mit.edu/projects/" + projectId + "/cloud-data.js");
            login.Referer = "https://scratch.mit.edu"; // Required by Scratch servers
            login.Headers.Add("Cookie", "scratchcsrftoken=a; scratchlanguage=en; scratchsessionsid=" + user.SessionID + ";");
            login.Headers.Add("X-CSRFToken", "a");
            login.Headers.Add("X-Requested-With", "XMLHttpRequest");
            login.Host = "scratch.mit.edu";
            login.Method = "GET";
            
            //Get request response
            HttpWebResponse response = (HttpWebResponse)login.GetResponse();

            //Read response data
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            
            return new CloudSession(user, projectId, reader.ReadToEnd().Substring(1495, 36));
        }
        
        private CloudSession(UserInfo user, int projectId, string cloudId)
        {
            User = user;
            ProjectId = projectId;
            CloudId = cloudId;
            Hash = Md5(cloudId);
        }

        public void Connect()
        {
            udpClient = new UdpClient("cloud.scratch.mit.edu", 531);
            string body = JsonConvert.SerializeObject(new Packet(CloudId, Hash, User.Username, ProjectId, "handshake")) + "\n";
            byte[] sendBytes = Encoding.UTF8.GetBytes(body);
            udpClient.Send(sendBytes, sendBytes.Length);

            Hash = Md5(Hash);

            udpClient.BeginReceive(new AsyncCallback(RecieveCallback), null);
        }

        public void Set(string name, int value)
        {
            udpClient = new UdpClient("cloud.scratch.mit.edu", 531);
            string body = JsonConvert.SerializeObject(new SetPacket(CloudId, Hash, User.Username, ProjectId, "set", name, value)) + "\n";
            byte[] sendBytes = Encoding.UTF8.GetBytes(body);
            udpClient.Send(sendBytes, sendBytes.Length);

            Hash = Md5(Hash);
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 531);
            byte[] receiveBytes = udpClient.EndReceive(ar, ref endPoint);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            Console.WriteLine("Received: {0}", receiveString);

            udpClient.BeginReceive(new AsyncCallback(RecieveCallback), null);
        }

        /// <summary>
        /// Calculates the md5 hash of a given string value.
        /// </summary>
        /// <param name="value">The value to compute the md5 hash of.</param>
        private string Md5(string value)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(value);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            udpClient.Close();
        }

        private class Packet
        {
            public string token;
            public string token2;
            public string user;
            public int project_id;
            public string method;

            public Packet(string token, string token2, string user, int project_id, string method)
            {
                this.token = token;
                this.token2 = token2;
                this.user = user;
                this.project_id = project_id;
                this.method = method;   
            }
        }

        private class SetPacket : Packet
        {
            public string name;
            public int value;

            public SetPacket(string token, string token2, string user, int project_id, string method, string name, int value) : base(token, token2, user, project_id, method)
            {
                this.name = name;
                this.value = value;
            }
        }
    }
}
