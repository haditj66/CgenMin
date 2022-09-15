using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.cgenXMLSaves.SaveFiles;
using CodeGenerator.FileTemplates;

namespace CodeGenerator.FileTemplatesMacros
{
    public class FileTemplateAllLibraryInlcudes : FileTemplateBase
    {
        public FileTemplateAllLibraryInlcudes(string templateOutputDestination, SaveFilecgenProjectGlobal saveFilecgenProjectGlobal) : base(templateOutputDestination, "AllLibraryIncludes.txt", "AllLibraryIncludes.h")
        {
            var macroLoopGroup = new MacroGroupLoop("USE", "CGENPROJ_NAME", "PATH_OF_PROJ");
            var macroLoopGroup2 = new MacroGroupLoop("INFO", "CGENPROJ_NAME");


            foreach (var globalProj in saveFilecgenProjectGlobal.CgenProjects.Projects)
            {

                macroLoopGroup.AddNewGroup(globalProj.NameOfProject, Path.Combine(globalProj.PathOfProject, "Config",globalProj.NameOfProject+"Conf.h"));
                macroLoopGroup2.AddNewGroup(globalProj.NameOfProject);
                /*
                macroLoopGroup1 = new List<Macro>();
                macroLoopGroup1.Add(new Macro("CGENPROJ_NAME", globalProj.NameOfProject));
                macroLoopGroup1.Add(new Macro("PATH_OF_PROJ", globalProj.PathOfProject));
                MacroLoopGroups.Add(macroLoopGroup1);
                */ 
            }
            MacroLoopGroups.Add(macroLoopGroup);
            MacroLoopGroups.Add(macroLoopGroup2); 

        }
    }
}
