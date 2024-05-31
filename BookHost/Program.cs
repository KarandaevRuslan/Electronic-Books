using Databases;
using Databases.SQLiteClasses;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using WcfServiceBook;

namespace BookHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Program.CreateStorage();
            StorageDBcontext storageDbcontext = new StorageDBcontext();
            IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            string str = "";
            foreach (IPAddress ipAddress in hostAddresses)
            {
                if (ipAddress.ToString().Count<char>() <= 15)
                    str = ipAddress.ToString();
            }
            Console.WriteLine("IP: " + str);
            using (ServiceHost serviceHost = new ServiceHost(typeof(ServiceBook), new Uri[1]{new Uri("http://" + str + ":8080/BookHostService")}))
            {
                serviceHost.Open();
                Console.WriteLine("Host started.");
                Console.ReadLine();
                serviceHost.Close();
            }
        }

        private static void CreateStorage()
        {
            string pathStorage = AppSettings.PathStorage;
            if (Directory.Exists(pathStorage))
                return;
            Directory.CreateDirectory(pathStorage);
        }
    }
}
