using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CodeGenerator.cgenXMLSaves
{
    public abstract class SaveFileBase
    {

        public string BaseDirectoryForProjectSaveFiles;// = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +"\\"+  @"CGensaveFiles\";
        private bool serializerCreated;
        private XmlSerializer _serializer;
        public XmlSerializer Serializer
        {
            get
            {
                if (!serializerCreated)
                {
                    _serializer =  CreateSerializer();
                    return _serializer;
                }
                else
                {
                    return _serializer;
                }
            }
            protected set { _serializer = value; } }

        protected abstract string FileNameDefault { get; }
        protected string _FileLocation;
        //protected string FileLocation { get { return _FileLocation != null ? _FileLocation : FileNameDefault; } }
        protected string FileLocation { get { return _FileLocation ; } }
        private dynamic _XMLClassToSave;
        public dynamic XMLClassToSave { get { return _XMLClassToSave; } protected set { _XMLClassToSave = value; } }
                     

        public SaveFileBase(string fileLocation) 
        {
            _FileLocation = Path.Combine(fileLocation, "cgenProjs.cgx");
            //_FileLocation = fileLocation +"\\"+ "cgenProjs.cgx";
            Init();
            Load();
        }

        public SaveFileBase()
        { 
            BaseDirectoryForProjectSaveFiles = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "CGensaveFiles");
            _FileLocation = FileNameDefault;
            Init();
            Load();
        }

        private void Init()
        {
            serializerCreated = false;

            //make sure directory exists
            if (!Directory.Exists(Path.GetDirectoryName(FileLocation)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FileLocation));
            }
        }

        public void Load()
        {
             
            //first make sure that the file exists.  .
            if (File.Exists(FileLocation))
            { 

                try
                {
                    using (StreamReader sr = new StreamReader(FileLocation))
                    {
                        XMLClassToSave = Serializer.Deserialize(sr);
                    }
                }
                catch (Exception)
                {
                    //if there was a problem than get default instead
                    Console.WriteLine("there was a problem loading file "+ _FileLocation);
                    XMLClassToSave = GetDefaultXMLClass();
                    //Save();
                }
            }
            else
            { 
                XMLClassToSave = GetDefaultXMLClass();
                //Save();
            }
        }



        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(FileLocation))
            {
                Serializer.Serialize(sw, XMLClassToSave);
            }
        }

        public abstract dynamic GetDefaultXMLClass();
        protected abstract XmlSerializer CreateSerializer();

    }
}
