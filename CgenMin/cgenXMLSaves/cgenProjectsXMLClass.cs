using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CodeGenerator.cgenXMLSaves
{


    [XmlRoot(ElementName = "AdditionalIncludes")]
    public class AdditionalIncludes
    {
        [XmlElement(ElementName = "AdditionalInclude")]
        public List<string> AdditionalInclude { get; set; }
    }

    [XmlRoot(ElementName = "AdditionalLibraries")]
    public class AdditionalLibraries
    {
        [XmlElement(ElementName = "AdditionalLibrary")]
        public List<string> AdditionalLibrary { get; set; }
    }

    [XmlRoot(ElementName = "PlatForm")]
    public class PlatForm
    {
        [XmlElement(ElementName = "AdditionalIncludes")]
        public AdditionalIncludes AdditionalIncludes { get; set; }
        [XmlElement(ElementName = "AdditionalLibraries")]
        public AdditionalLibraries AdditionalLibraries { get; set; }
        [XmlAttribute(AttributeName = "PlatFormName")]
        public string PlatFormName { get; set; }
    }

    [XmlRoot(ElementName = "PlatFormsInScope")]
    public class PlatFormsInScope
    {
        [XmlElement(ElementName = "PlatForm")]
        public List<PlatForm> PlatForms { get; set; }

        public List<string> GetIncludesFromPlatForm( string platformName)
        {
            var platforms = PlatForms.FirstOrDefault(p => p.PlatFormName == platformName);
            if (platforms == null)
            {
                return null;
            }
            else
            {
                return platforms.AdditionalIncludes.AdditionalInclude;
            }
        }

        public List<string> GetAdditionalLibrariesFromPlatForm(string platformName)
        {
            var platforms = PlatForms.FirstOrDefault(p => p.PlatFormName == platformName);
            if (platforms == null)
            {
                return null;
            }
            else
            {
                return platforms.AdditionalLibraries.AdditionalLibrary;
            }
        }
    }

    [XmlRoot(ElementName = "Project")]
    public class cgenProjectGlobal
    {
        [XmlElement(ElementName = "PlatFormsInScope")]
        public PlatFormsInScope PlatFormsInScope { get; set; }
        [XmlAttribute(AttributeName = "NameOfProject")]
        public string NameOfProject { get; set; }
        [XmlAttribute(AttributeName = "PathOfProject")]
        public string PathOfProject { get; set; }
        [XmlAttribute(AttributeName = "UniqueIdentifier")]
        public string UniqueIdentifier { get; set; }

        ///extension for conversion from cgenProjectLocal to cgenProjectGlobal
        public static implicit operator cgenProjectGlobal(cgenProjectLocal projlocal)
        {
            return new cgenProjectGlobal(){ PlatFormsInScope = new PlatFormsInScope(),
                NameOfProject = projlocal.NameOfProject,
                PathOfProject = projlocal.PathOfProject,
                UniqueIdentifier = projlocal.UniqueIdentifier
            };
        }
    }

    [XmlRoot(ElementName = "Projects")]
    public class cgenProjectsGlobal
    {
        [XmlElement(ElementName = "Projects")]
        public List<cgenProjectGlobal> Projects { get; set; }
         

    }



     
    [XmlRoot(ElementName = "Project")]
    public class cgenProjectsLocal
    {
        [XmlElement(ElementName = "Project")]
        public cgenProjectLocal Project { get; set; }

    }

    [XmlRoot(ElementName = "ProjectLocal")]
    public class cgenProjectLocal
    {
        [XmlAttribute(AttributeName = "NameOfProject")]
        public string NameOfProject { get; set; }

        [XmlAttribute(AttributeName = "PathOfProject")]
        public string PathOfProject { get; set; }

        [XmlAttribute(AttributeName = "UniqueIdentifier")]
        public string UniqueIdentifier { get; set; } 

    }
     
    

    public static class cgenXMLMemeberCreationHelper
    {
        static Random rng = new Random();
        public static string UniqueIdentifierCreator()
        {
            //9999999-9999-9999999
            int first = rng.Next(1000000, 9999999);
            int second = rng.Next(1000, 9999);
            int third = rng.Next(1000000, 9999999);
            return first.ToString() + second.ToString() + third.ToString();
        }



    }




    #region Config

    [XmlRoot(ElementName = "Config")]
    public class cgenConfig
    {
        [XmlAttribute(AttributeName = "DirectoryOfConfig")]
        public string DirectoryOfConfig { get; set; }

        [XmlElement(ElementName = "PlatForms")]
        public configPlatForms PlatForms { get; set; }
    }


    [XmlRoot(ElementName = "PlatForm")]
    public class configPlatForms
    {
        [XmlElement(ElementName = "PlatForm")]
        public List<string> PlatForm { get; set; }

    }

    #endregion








}
