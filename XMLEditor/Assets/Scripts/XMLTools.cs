using System.Linq;

namespace Assets.Scripts
{
    public class XMLTools
    {
        // loads the xml file as a string[] and finds finds the line with </surfaces>
        // then adds </definition> after it
        public static string FixBadFormatting(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            int index = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("</surfaces>") || lines[i].Contains("<surfaces/>"))
                {
                    index = i;
                    break;
                }
            }
            lines[index] = lines[index] + "</definition>";
            // remove the remaining lines
            lines = lines.Take(index + 1).ToArray();
            return string.Join("\n", lines);
        }
    }
}