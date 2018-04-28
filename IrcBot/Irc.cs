using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace IrcBot
{
    class Irc
    {
        public string Hostname { get; }
        public string ServerAdress { get; set; }
        public int Port { get; }
        public string Nickname { get; set; }
        public string Realname { get; }
        public bool Connected { get; }

        TcpClient tcpClient;
        StreamReader inputStream;
        StreamWriter outputStream;

        Queue<string> MessageQueue = new Queue<string>();

        bool QueueIsRunning = false;
        int QueueTimer = 800;

        public Irc(string hostname, int port, string nickname, string realname)
        {
            Hostname = hostname;
            Port = port;
            Nickname = nickname;
            Realname = realname;

            try
            {
                tcpClient = new TcpClient(Hostname, Port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());
                outputStream.Flush();

                outputStream.WriteLine("NICK " + Nickname);
                outputStream.WriteLine("USER " + Realname + " 8 * :" + Realname);// The USER message is used at the beginning of connection 
                                                                                 //to specifythe username, hostname, servername and realname of s new user.
                outputStream.Flush();

                Connected = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Connected = false;
            }

        }

        public string readData()
        {
            try
            {
                return inputStream.ReadLine();

            }
            catch (Exception ex)
            {
                return "Could not read data: " + ex.Message;
            }
        }
        public void sendData(string Data)
        {
            try
            {
                outputStream.WriteLine(Data);
                outputStream.Flush();
                Console.WriteLine("<" + Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not send data" + ex.Message);
            }

        }

        public void joinRoom(string message)
        {
            if (Regex.IsMatch(message, "#([a-zA-Z0-9]+)"))
            {
                sendData("JOIN " + message);
            }
            else Console.WriteLine("No such channel");

        }
        public void ChatMessage(string message, string i)
        {
            //PRIVMSG (privatemessage)
            string Message = "PRIVMSG " + i + " :" + message;
            MessageQueue.Enqueue(Message);

            if (!QueueIsRunning)
            {
                sendChatMessage();
            }

        }
        private void sendChatMessage()
        {
            foreach (string i in MessageQueue)
            {
                QueueIsRunning = true;
                outputStream.WriteLine(i);
                outputStream.Flush();
                System.Threading.Thread.Sleep(QueueTimer);

            }
            MessageQueue.Clear();
            QueueIsRunning = false;
        }

        public void listNames(string channel)
        {

            if (Regex.IsMatch(channel, "#([a-zA-Z0-9]+)"))
            {
                sendData("NAMES " + channel);
            }
            else
            {
                sendData("NAMES ");
            }

        }

        public void listChannels(string channel)
        {

            if (Regex.IsMatch(channel, "#([a-zA-Z0-9]+)"))
            {
                sendData("LIST " + channel);
            }
            else
            {
                sendData("LIST ");
            }

        }

        public void quitSession()
        {
            sendData("QUIT " + ":Bye!");
        }
        public void serverStats(string queries)
        {
            if (Regex.IsMatch(queries, @"[chiklmoyu]"))
            {
                sendData("STATS " + queries);
            }
            else Console.WriteLine("Need to put stats query (c, h, i, k, l, m, o, y, u)");

        }
        public void commandWho(string mess)
        {
            sendData("WHO " + mess);
        }
        public void commandWhois(string mess)
        {
            sendData("WHOIS " + mess);
        }
        public void partChannel(string mess)
        {
            if (Regex.IsMatch(mess, "#([a-zA-Z0-9]+)"))
            {
                sendData("PART " + mess);
            }
            else Console.WriteLine("Channel not specified");
        }
        public void newNick(string mess)
        {
            sendData("NICK " + mess);
        }
        public void serverInfo()
        {
            sendData("INFO " + ServerAdress);
        }
    }
}