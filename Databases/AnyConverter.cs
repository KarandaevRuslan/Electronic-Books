
using System.Collections.Generic;
using System.Linq;

namespace Databases
{
  public class AnyConverter
  {
    public static List<string> Get_Directory_Name_Extension_FromFilePath(string filePath)
    {
      filePath = filePath.Replace('/', '\\');
      List<string> list1 = ((IEnumerable<string>) filePath.Split('\\')).ToList<string>();
      List<string> list2 = ((IEnumerable<string>) list1.Last<string>().Split('.')).ToList<string>();
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
  }
}
