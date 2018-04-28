using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace IrcBot
{

    class IrcBot
    {
        Irc IrcConnection;
        string channel = null;
        bool input = true;
        string nickName;

        static void Main(string[] args)
        {
            IrcBot Program = new IrcBot();
            Program.GetData();
        }
        private void TimerCallback(Object o)
        {
            string Data = IrcConnection.readData();
            if (Data != null)
            {
                Console.WriteLine(Data);
            }

        }
        private void GetData()
        {

            IrcConnection = new Irc("shell.riftus.lt", 6667, "prataVetra_123", "ElenaReivytyte");
            if (!IrcConnection.Connected)
            {
                Console.WriteLine("Cannot connect");
                Console.ReadKey();
                return;
            }
            while (true)
            {
                string Data = IrcConnection.readData();

                if (Data != null)
                {
                    Console.WriteLine(Data);

                    if (Regex.IsMatch(Data, @":(.+) ([0-9]+) " + IrcConnection.Nickname + " :End of /MOTD command."))
                    {
                        string[] tRegEx = Regex.Split(Data, @":(.+) ([0-9]+) " + IrcConnection.Nickname + " :End of /MOTD command.");
                        IrcConnection.ServerAdress = tRegEx[1];
                        Messages();

                    }

                }

            }

        }
        private void ChannelData()
        {
            while (true)
            {
                string Data = IrcConnection.readData();
                if (Data != null)
                {
                    Console.WriteLine(Data);

                    if (Regex.IsMatch(Data, @":End of /NAMES list."))
                    {
                        Messages();

                        break;
                    }
                    else
                    if (Regex.IsMatch(Data, @":End of /LIST"))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @":End of /STATS report"))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @":End of /WHO list."))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @":End of /WHOIS list."))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @"PART " + channel))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @"NICK :" + nickName))
                    {
                        Messages();
                        break;
                    }
                    else if (Regex.IsMatch(Data, @":End of /INFO list."))
                    {
                        Messages();
                        break;
                    }

                }
            }

        }
        private void Messages()
        {
            string command;
            string mess;
            bool a = true;
            while (a)
            {
                command = Console.ReadLine();
                if (Regex.IsMatch(command, @"/*"))
                {
                    switch (command)
                    {
                        case "/NAMES":    //user can list all nicknames that are visible to them on any channel that they can see
                            Console.WriteLine("Enter channel or press enter");
                            mess = Console.ReadLine();
                            IrcConnection.listNames(mess);
                            ChannelData();
                            break;
                        case "/LIST": //The list message is used to list channels and their topics.
                            Console.WriteLine("Enter channel or press enter");
                            mess = Console.ReadLine();
                            IrcConnection.listChannels(mess);

                            ChannelData();
                            break;
                        case "/QUIT"://A client session is ended with a quit message.                          
                            input = false;
                            a = false;
                            IrcConnection.quitSession();
                            break;
                        case "/PRIVMSG"://PRIVMSG is used to send private messages between users.
                            if (channel != null)
                            {
                                Chat();
                            }
                            else Console.WriteLine("Connect to channel");
                            break;

                        case "/STATS": //The stats message is used to query statistics of certain server.
                            Console.WriteLine("Enter stats query (c, h, i, k, l, m, o, y, u)");
                            mess = Console.ReadLine();
                            IrcConnection.serverStats(mess);
                            ChannelData();
                            break;
                        case "/JOIN":// The JOIN command is used by client to start listening a specificchannel.
                            Console.WriteLine("Enter Channel");
                            mess = Console.ReadLine();
                            IrcConnection.joinRoom(mess);
                            channel = mess;
                            ChannelData();
                            break;
                        case "/WHO"://returns a list of information which 'matches' the<name> parameter given by the client.
                            Console.WriteLine("Enter name");
                            mess = Console.ReadLine();
                            IrcConnection.commandWho(mess);
                            ChannelData();
                            break;
                        case "/WHOIS"://This message is used to query information about particular user.
                            Console.WriteLine("Enter name");
                            mess = Console.ReadLine();
                            IrcConnection.commandWhois(mess);
                            ChannelData();
                            break;
                        case "/PART": //removed from the list of active users for all given channels listed in theparameter string.
                            Console.WriteLine("Enter Channel");
                            mess = Console.ReadLine();
                            IrcConnection.partChannel(mess);
                            ChannelData();
                            break;
                        case "/NICK"://NICK message is used to give user a nickname or change the previous one.
                            Console.WriteLine("//Enter new nickname");
                            mess = Console.ReadLine();
                            IrcConnection.newNick(mess);
                            nickName = mess;
                            ChannelData();
                            break;
                        case "/INFO":
                            IrcConnection.serverInfo();
                            ChannelData();
                            break;

                    }

                }

            }
        }

        private void Chat()
        {
            Timer t = new Timer(TimerCallback, null, 0, 2000);
            Console.WriteLine("Enter your message");
            string message = Console.ReadLine();
            IrcConnection.ChatMessage(message, channel);
            t.Change(Timeout.Infinite, Timeout.Infinite);

        }



    }
}
