using ElectronicBooks.BookReference;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Xabe.FFmpeg;

namespace ElectronicBooks.Heap.Other
{
    internal static class AppSettings
    {
        public static string pathBookCatalog = "";
        public static string defaultPathBookCatalog = "C:\\Users\\" + Environment.UserName + "\\Documents\\ElectronicBook";
        public static int UserId = 0;
        public static string hostIP = "";

        public static string AppDBPath()
        {
            string directoryWithFFmpegAndFFprobe = AppSettings.AppDirectoryPath();
            if (FFmpeg.ExecutablesPath == null)
                FFmpeg.SetExecutablesPath(directoryWithFFmpegAndFFprobe);
            return directoryWithFFmpegAndFFprobe + "\\App.db";
        }

        public static string AppDirectoryPath()
        {
            return AnyConverter.Get_Directory_Name_Extension_FromFilePath(Environment.GetCommandLineArgs()[0])[0];
        }

        public static ServiceBookClient GetClient()
        {
            return new ServiceBookClient("BasicHttpBinding_IServiceBook", new EndpointAddress(new Uri("http://" + AppSettings.hostIP + ":8080/BookHostService"), Array.Empty<AddressHeader>()));
        }
    }
}
