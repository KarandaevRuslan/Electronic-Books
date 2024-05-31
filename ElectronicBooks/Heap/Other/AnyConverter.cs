using ElectronicBooks.Views.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Media;

namespace ElectronicBooks.Heap.Other
{
    internal static class AnyConverter
    {
        public static string EncodeReservedUrlSymbols(string url)
        {
            List<string> stringList = new List<string>();
            foreach (char ch in url)
                stringList.Add(ch.ToString());
            for (int index = 0; index < stringList.Count; ++index)
            {
                switch (stringList[index])
                {
                    case "$":
                        stringList[index] = "%24";
                        break;
                    case "&":
                        stringList[index] = "%26";
                        break;
                    case "+":
                        stringList[index] = "%2b";
                        break;
                    case ",":
                        stringList[index] = "%2c";
                        break;
                    case "/":
                        stringList[index] = "%2f";
                        break;
                    case ":":
                        stringList[index] = "%3a";
                        break;
                    case ";":
                        stringList[index] = "%3b";
                        break;
                    case "=":
                        stringList[index] = "%3d";
                        break;
                    case "?":
                        stringList[index] = "%3f";
                        break;
                    case "@":
                        stringList[index] = "%40";
                        break;
                }
            }
            string str1 = "";
            foreach (string str2 in stringList)
                str1 += str2;
            return str1;
        }

        public static string EncodeAndDecodeUrl(string url)
        {
            return HttpUtility.UrlDecode(AnyConverter.EncodeReservedUrlSymbols(url));
        }

        public static string GetShortDirectoryPath(string dirP)
        {
            dirP = dirP.Replace('/', '\\');
            if (dirP.Last<char>() == '\\')
                dirP = dirP.Substring(0, dirP.Count<char>() - 1);
            return dirP;
        }

        public static string GetDirectoryName(string dirP)
        {
            dirP = AnyConverter.GetShortDirectoryPath(dirP);
            return ((IEnumerable<string>)dirP.Split('\\')).Last<string>();
        }

        public static bool IsUrlFileOnDisk(string url) => url.IndexOf("file:///") == 0;

        public static bool CanConvertFileToPDF(string e)
        {
            return e.Equals("doc") || e.Equals("docx") || e.Equals("ppt") || e.Equals("pptx") || e.Equals("xls") || e.Equals("xlsx");
        }

        public static bool CanConvertFileToWebM(string e)
        {
            return e.Equals("mp4") || e.Equals("mov") || e.Equals("avi") || e.Equals("flv") || e.Equals("mkv") || e.Equals("wmv");
        }

        public static string Get_NameAndExtension_FromFilePath(string filePath)
        {
            List<string> extensionFromFilePath = AnyConverter.Get_Directory_Name_Extension_FromFilePath(filePath);
            return extensionFromFilePath[1] + "." + extensionFromFilePath[2];
        }

        public static List<string> Get_Directory_Name_Extension_FromFilePath(string filePath)
        {
            filePath = filePath.Replace('/', '\\');
            List<string> list1 = ((IEnumerable<string>)filePath.Split('\\')).ToList<string>();
            List<string> list2 = ((IEnumerable<string>)list1.Last<string>().Split('.')).ToList<string>();
            string source = "";
            for (int index = 0; index < list1.Count - 1; ++index)
                source = source + list1[index] + "\\";
            return new List<string>()
      {
        source.Substring(0, source.Count<char>() - 1),
        list2[0],
        list2[list2.Count - 1]
      };
        }

        public static SolidColorBrush GetBrush(string htmlColor)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(htmlColor));
            }
            catch (Exception ex)
            {
                return Brushes.Black;
            }
        }

        public static bool IntToBool(int val) => val != 0;

        public static int BoolToInt(bool val) => !val ? 0 : 1;

        public static bool IsFile(string path)
        {
            return AnyConverter.GetDirectoryName(path).LastIndexOf(".") != -1;
        }

        public static void ConvertionVideoToWebm(string inputPath)
        {
            try
            {
                if (!AnyConverter.IsFile(inputPath))
                    return;
                List<string> extensionFromFilePath = AnyConverter.Get_Directory_Name_Extension_FromFilePath(inputPath);
                if (!AnyConverter.CanConvertFileToWebM(extensionFromFilePath[2]) || !AnyConverter.ShowMessageBox(extensionFromFilePath[2], "webm"))
                    return;
                string str = extensionFromFilePath[0] + "\\" + extensionFromFilePath[1] + ".webm";
                if (File.Exists(str))
                {
                    int num = (int)MessageBox.Show("Уже существует файл \"" + str + "\" с таким же названием и с расширением \".webm\". Удалите этот файл, чтобы конвертировать выбранный вами файл.", "Ошибка конвертации", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                else
                    new ConversionProgress(new List<string>()
          {
            extensionFromFilePath[1] + "." + extensionFromFilePath[2],
            extensionFromFilePath[1] + ".webm"
          }, inputPath, str).ShowDialog();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
        }

        public static bool ShowMessageBox(string extension, string necessaryExtension)
        {
            switch (MessageBox.Show("Расширение файла \"." + extension + "\" не поддерживается, однако вы можете сохранить этот файл как файл с расширением \"." + necessaryExtension + "\", и после открыть его. Желаете сохранить сохранить файл в формате \"." + necessaryExtension + "\"?", "Расширение не поддерживается", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.None:
                    return false;
                case MessageBoxResult.Yes:
                    return true;
                default:
                    goto case MessageBoxResult.None;
            }
        }
    }
}
