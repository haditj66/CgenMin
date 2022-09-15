using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CodeGenerator.cgenXMLSaves.SaveFiles
{
    public class SaveFilecgenConfig : SaveFileBase
    {
        protected override string FileNameDefault => Path.Combine(this.BaseDirectoryForProjectSaveFiles,  "cgenConfig.cgx");
        public cgenConfig CgenConfig { get { return (cgenXMLSaves.cgenConfig)XMLClassToSave; } }

        public override dynamic GetDefaultXMLClass()
        {
            return new cgenConfig();
        }

        protected override XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(cgenConfig));
        }
    }
}
