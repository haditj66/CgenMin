using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CodeGenerator.FileTemplates
{
    public class FileTemplateMainCG : FileTemplateBase
    {
        public FileTemplateMainCG(string templateOutputDestination, string CGENProjectName) : base( templateOutputDestination, "mainCG.txt", "mainCG.cpp")
        {
            Macros = new List<Macro>();
            Macros.Add(new Macro("CGENCONF_NAME", CGENProjectName));
        }
    }
}
