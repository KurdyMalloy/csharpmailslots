using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cMailSlot;
using System.IO;
using System.Threading;
using System.Net;

namespace TestServer
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Environment.MachineName --> {0}", Environment.MachineName);
            Console.WriteLine(@"Dns.Hostname --> {0}", Dns.GetHostName());
            Console.WriteLine(@"IPAdress --> {0}", Network.FindIPAddress(true).ToString());

            Console.WriteLine(@"Enter Mailslot name to Host (production, dev, test etc) : ");
            string mailslotname = Console.ReadLine();

            MailSlot ms, remote_ms;
            StreamReader sr;
            try
            {
                Console.WriteLine(@"Creating Listening MailSlot");
                ms = new MailSlot(mailslotname);
                Console.WriteLine(@"Creating Stream");
                sr = new StreamReader(ms.FStream);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                return;
            }

            while (true)
            {
                try
                {

                    if (ms.IsMessageInSlot > 0)
                    {
                        Console.WriteLine(@"Messages waiting --> {0}", ms.IsMessageInSlot);
                        string message = sr.ReadLine();
                        Console.WriteLine();
                        Console.WriteLine(@"Message --> {0}", (message != null)?message:@"NULL");

                        string remote_env = message.Substring(message.IndexOf(':')+1);
                        Console.WriteLine(@"Remote Mailslot Name --> {0}", remote_env);
                        string scope = message.Substring(0, message.IndexOf(':'));
                        Console.WriteLine(@"Remote Adresse --> {0}", scope);
                        //Create remote mailslot to answer
                        remote_ms = new MailSlot(remote_env, scope);
                        StreamWriter sw = new StreamWriter(remote_ms.FStream);

                        sw.WriteLine(@"Server Name --> {0}", Environment.MachineName);
                        sw.Flush();
                        sw.WriteLine(@"Server Name (DNS) --> {0}", Dns.GetHostName());
                        sw.Flush();
                        sw.WriteLine(@"Server IP Adress --> {0}", Network.FindIPAddress(true).ToString());
                        sw.Flush();
                        sw.WriteLine(@"This message was sent using windows Mailslots...");
                        sw.Flush();
                        sw.Dispose();
                        remote_ms.Close();
                        Console.WriteLine(@"Message sent...");
                        Console.WriteLine(@"********************************************************************");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());

                }
                Thread.Sleep(100);
            }

            //Did not implement a way of exiting while loop so the following is unreachable.
            //sr.Dispose();
            //ms.Dispose();

        }
    }

    public static class Network
    {
        #region DNS
        public static IPAddress FindIPAddress(bool localPreference)
        {
            return FindIPAddress(Dns.GetHostEntry(Dns.GetHostName()),
            localPreference);
        }

        public static IPAddress FindIPAddress(IPHostEntry host, bool
        localPreference)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            if (host.AddressList.Length == 1)
                return host.AddressList[0];
            else
            {
                foreach (System.Net.IPAddress address in host.AddressList)
                {
                    bool local = IsLocal(address);

                    if (local && localPreference)
                        return address;
                    else if (!local && !localPreference)
                        return address;
                }

                return host.AddressList[0];
            }
        }

        public static bool IsLocal(IPAddress address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            byte[] addr = address.GetAddressBytes();

            return addr[0] == 10
            || (addr[0] == 192 && addr[1] == 168)
            || (addr[0] == 172 && addr[1] >= 16 && addr[1] <= 31);
        }
        #endregion
    }
}
