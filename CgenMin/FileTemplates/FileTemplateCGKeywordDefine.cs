using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.cgenXMLSaves.SaveFiles;

namespace CodeGenerator.FileTemplates
{
    public class FileTemplateCGKeywordDefine : FileTemplateBase
    {

        public FileTemplateCGKeywordDefine(string templateOutputDestination,
            SaveFilecgenProjectGlobal saveFilecgenProjectGlobal) : base(templateOutputDestination,
            "CGKeywordDefine.txt",
             "CGKeywordDefine.h")
        {
            var macroLoopGroup = new MacroGroupLoop("USE", "CGENPROJ_NAME");


            foreach (var globalProj in saveFilecgenProjectGlobal.CgenProjects.Projects)
            {
                macroLoopGroup.AddNewGroup(globalProj.NameOfProject, globalProj.PathOfProject); 
            }

            MacroLoopGroups.Add(macroLoopGroup);

        }
    }
}
