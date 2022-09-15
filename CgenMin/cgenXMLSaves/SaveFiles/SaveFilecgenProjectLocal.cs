using System.Collections.Generic;
using System.Xml.Serialization;

namespace CodeGenerator.cgenXMLSaves.SaveFiles
{
    public class SaveFilecgenProjectLocal : SaveFileBase
    {

        public SaveFilecgenProjectLocal() : base()
        {

        }

        public SaveFilecgenProjectLocal(string fileLocation) : base(fileLocation)
        {
        }

        public cgenProjectsLocal CgenProjects { get { return (cgenXMLSaves.cgenProjectsLocal)XMLClassToSave; } }

        protected override string FileNameDefault => Path.Combine(Program.envIronDirectory , Program.CGSAVEFILESBASEDIRECTORY, "cgenProjs.cgx");//this.BaseDirectoryForProjectSaveFiles + "cgenProjs.cgx";

        public override dynamic GetDefaultXMLClass()
        {
            cgenProjectsLocal returncgen = new cgenProjectsLocal();
            //returncgen.Projects = new List<cgenProjectLocal>();
            return returncgen;
        }

        protected override XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(cgenXMLSaves.cgenProjectsLocal));
        }


    }
}