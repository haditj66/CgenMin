using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CodeGenerator.cgenXMLSaves.SaveFiles
{
    public class SaveFilecgenProjectGlobal : SaveFileBase
    {
        
        public SaveFilecgenProjectGlobal() : base()
        {
            
        }

        public SaveFilecgenProjectGlobal(string fileLocation) : base(fileLocation)
        { 
        }

        public cgenProjectsGlobal CgenProjects { get { return (cgenXMLSaves.cgenProjectsGlobal)XMLClassToSave; } }

        protected override string FileNameDefault => Path.Combine(this.BaseDirectoryForProjectSaveFiles, "cgenProjs.cgx");

        public override dynamic GetDefaultXMLClass()
        {
            cgenProjectsGlobal returncgen = new cgenProjectsGlobal();
            returncgen.Projects = new List<cgenProjectGlobal>();
            
            return returncgen;
        }

        protected override XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(cgenXMLSaves.cgenProjectsGlobal));
        }


        public void AddNewProject(cgenProjectGlobal projectToAdd)
        {
             CgenProjects.Projects.Add((cgenProjectGlobal)projectToAdd);
             
        }


        public void RemoveProject(cgenProjectGlobal projectToRemove)
        {
            var proj =  CgenProjects.Projects.First(p=> p.NameOfProject == projectToRemove.NameOfProject && p.UniqueIdentifier == projectToRemove.UniqueIdentifier);

            CgenProjects.Projects.Remove((cgenProjectGlobal)proj);

        }

        public void AddNewPlatformInScope(string toProjectName, PlatForm PlatformToAdd)
        {
            //check for null first
            if (CgenProjects.Projects.First(p =>
                    p.NameOfProject == toProjectName).
                PlatFormsInScope.PlatForms == null)
            {
                CgenProjects.Projects.First(p =>
                        p.NameOfProject == toProjectName).
                    PlatFormsInScope.PlatForms = new List<PlatForm>();
            }

            CgenProjects.Projects.First(p =>
                    p.NameOfProject == toProjectName).
                PlatFormsInScope.PlatForms.Add(PlatformToAdd);
             
            Save();
            Load(); 
        }

        public void AddNewInclude(string toprojectname, string forplatform, string theincludetoadd)
        {
            if (CgenProjects.Projects
                .First(p => p.NameOfProject == toprojectname)
                .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                .AdditionalIncludes.AdditionalInclude == null)
            {
                CgenProjects.Projects
                    .First(p => p.NameOfProject == toprojectname)
                    .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                    .AdditionalIncludes.AdditionalInclude = new List<string>();
            }
             
            CgenProjects.Projects
                .First(p => p.NameOfProject == toprojectname)
                .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                .AdditionalIncludes.AdditionalInclude.Add(theincludetoadd);

            Save();
            Load();
        }


        public void AddNewLibrary(string toprojectname, string forplatform, string theLibraryToAdd)
        {
            if (CgenProjects.Projects
                    .First(p => p.NameOfProject == toprojectname)
                    .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                    .AdditionalLibraries.AdditionalLibrary == null)
            {
                CgenProjects.Projects
                    .First(p => p.NameOfProject == toprojectname)
                    .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                    .AdditionalLibraries.AdditionalLibrary = new List<string>();
            }

            CgenProjects.Projects
                .First(p => p.NameOfProject == toprojectname)
                .PlatFormsInScope.PlatForms.First(pl => pl.PlatFormName == forplatform)
                .AdditionalLibraries.AdditionalLibrary.Add(theLibraryToAdd);

            Save();
            Load();
        }
    }
}
