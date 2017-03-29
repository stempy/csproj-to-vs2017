using System;
using System.Xml.Linq;

namespace CsProjToVs2017Upgrader
{
    class Program
    {
        static void Main(string[] args)
        {

            var projectReader = new ProjectFileReader();
            var projInfo = projectReader.LoadProjectFile(@"C:\_Codebucket\ia\ia_backend\ia.domain\src\Ia.Domain.Models\Ia.Domain.Models.csproj");
            
            


            Console.WriteLine("Hello World!");
        }
    }
}