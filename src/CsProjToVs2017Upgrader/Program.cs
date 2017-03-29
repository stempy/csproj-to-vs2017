using System;
using System.Xml.Linq;

namespace CsProjToVs2017Upgrader
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectReader = new ProjectFileReader();
            var projInfo = projectReader.LoadProjectFile(args[0]);

            //var json = 
        }
    }
}