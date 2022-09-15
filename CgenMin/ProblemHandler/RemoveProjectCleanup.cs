using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.cgenXMLSaves;
using CodeGenerator.cgenXMLSaves.SaveFiles;

namespace CodeGenerator.ProblemHandler
{
    public class RemoveProjectCleanup : ICleanUp
    {
        private string DirOfLocalCgsavefilesbasedirectory { get; }
        private SaveFilecgenProjectGlobal SavefileProjGlobal { get; }
        private cgenProjectGlobal ProjToRemove { get; }

        public RemoveProjectCleanup(string dirOfLocalCGSAVEFILESBASEDIRECTORY,SaveFilecgenProjectGlobal  savefileProjGlobal, cgenProjectGlobal projToRemove)
        {
            DirOfLocalCgsavefilesbasedirectory = dirOfLocalCGSAVEFILESBASEDIRECTORY;
            SavefileProjGlobal = savefileProjGlobal;
            ProjToRemove = projToRemove;
        }

        public void CleanUp()
        {
            SavefileProjGlobal.RemoveProject(ProjToRemove);
            SavefileProjGlobal.Save();
            if (Directory.Exists(DirOfLocalCgsavefilesbasedirectory))
            {
                Directory.Delete(DirOfLocalCgsavefilesbasedirectory, true);
            } 
            Program.UpdateCCGKeywordsIncludes();
        }
    }
}
