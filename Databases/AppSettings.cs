
using System;

namespace Databases
{
  public static class AppSettings
  {
    public static string PathStorage = "C:\\Users\\" + Environment.UserName + "\\Documents\\ElectronicBookStorage";

    public static string StorageDBPath() => AppSettings.StoragePath() + "\\storage.db";

    public static string StoragePath()
    {
      return AnyConverter.Get_Directory_Name_Extension_FromFilePath(Environment.GetCommandLineArgs()[0])[0];
    }
  }
}
