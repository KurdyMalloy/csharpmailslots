using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cMailSlot;
using System.IO;
using System.Threading;
using System.Net;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Environment.MachineName --> {0}", Environment.MachineName);
            Console.WriteLine(@"Dns.Hostname --> {0}", Dns.GetHostName());
            Console.WriteLine(@"IPAdress --> {0}", Network.FindIPAddress(true).ToString());
            Console.WriteLine();

            Console.WriteLine(@"Enter Scope of search (domain name, computer name or '*' for primary domain : ");
            string scope = Console.ReadLine();
            Console.WriteLine(@"Enter Mailslot name to search (production, dev, test etc) : ");
            string mailslotname = Console.ReadLine();
            
            MailSlot listening_ms;
            try
            {
                // Create local Listening mailslot to get answer from server
                listening_ms = new MailSlot(@"listener");
                Console.WriteLine(@"Creating listeningMailSlot: {0}", listening_ms.Filename);
                //Creating Remote mailslot  
                MailSlot ms = new MailSlot(mailslotname, scope);
                Console.WriteLine(@"Creating Remote MailSlot {0}", ms.Filename);
                Console.WriteLine(@"Creating Stream");
                StreamWriter sw = new StreamWriter(ms.FStream);
                String message = String.Format(@"{0}:{1}", Environment.MachineName, @"listener");
                sw.WriteLine(message);
                sw.Flush();
                Console.WriteLine(@"Wrote to stream: {0}", message);
                sw.Dispose();
                ms.Dispose();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                return;
            }

            StreamReader sr = new StreamReader(listening_ms.FStream);
            while (true)
            {
                try
                {

                    while (listening_ms.IsMessageInSlot > 0)
                        Console.WriteLine(@"Messages {0}, {1}", listening_ms.IsMessageInSlot, sr.ReadLine());

                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }
                Thread.Sleep(100);
            }
            
            //Did not implement a way of exiting while loop so the following is unreachable.
            //sr.Dispose();
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
