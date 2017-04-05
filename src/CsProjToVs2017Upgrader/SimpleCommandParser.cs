using System.Linq;

namespace CsProjToVs2017Upgrader
{
    public static class SimpleCommandParser
    {
        public static string ParseOption(string shortCut, string longSwitch, ref string[] args)
        {
            string result = string.Empty;
            var shortKey = $"-{shortCut}";
            var longKey = $"--{longSwitch}";

            if (args.Any(n => n.StartsWith(shortKey)))
            {
                result = args.FirstOrDefault(n => n.StartsWith(shortKey));
                args = args.Where(n => !n.StartsWith(shortKey)).ToArray();
            }
            else if (args.Any(n => n.StartsWith(longKey)))
            {
                result = args.FirstOrDefault(n => n.StartsWith(longKey));
                args = args.Where(n => !n.StartsWith(longKey)).ToArray();
            }
            return result;
        }
    }
}