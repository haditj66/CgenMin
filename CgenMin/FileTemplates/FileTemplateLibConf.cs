using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.FileTemplates
{
    public class FileTemplateLibConf : FileTemplateBase
    {
        public FileTemplateLibConf(string templateOutputDestination, string CGENProjectName) : base(templateOutputDestination, "libconf.txt", CGENProjectName + "conf.h")
        {
            Macros = new List<Macro>();
            Macros.Add(new Macro("CGENCONF_NAME", CGENProjectName));
        }
    }
}
