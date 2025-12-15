using CgenMin.MacroProcesses;
using CgenMin.MacroProcesses.QR;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CgenMin.MacroProcesses
{
     

    public interface IWriteAOClassContents
    {
        string ProjectDirectory { get; set; }

        void WriteTheContentedToFiles();
    }

    public abstract class AOWritableToAOClassContents : AO, IWriteAOClassContents, IPartOfAEDefines
    {


        public static List<AOWritableToAOClassContents> AllAOWritableToAOClassContents = new List<AOWritableToAOClassContents>();
        protected class RelativeDirPathWrite
        {
            public string FileNameWithoutExt { get; protected set; }
            public string FileExtension { get; protected set; }
            public string RelativePath { get; protected set; }
            public string ConentsToWrite { get; protected set; }
            public bool UseMacro1 { get; }
            public bool IncludeHeader { get; }

            public RelativeDirPathWrite(string fileNameWithOutExt, string fileExtension, string relativePath, string conentsToWrite, bool useMacro1 = false, bool includeHeader = true)
            {
                FileNameWithoutExt = fileNameWithOutExt;
                FileExtension = fileExtension;
                RelativePath = relativePath;
                ConentsToWrite = conentsToWrite;
                UseMacro1 = useMacro1;
                IncludeHeader = includeHeader;
            }
        }


        public AOWritableToAOClassContents(string fromLibraryName, string instanceName ) :
            base(instanceName  )
        {
            FromModuleName = fromLibraryName;
            //// from this library name, I need to get the directory that it belongs to.
            ////first grab all the contents of the cmake file in C:/AERTOS/AERTOS/CMakeLists.txt .
            //string cmakeCont = File.ReadAllText(@"C:/AERTOS/AERTOS/CMakeLists.txt");

            ////    STREQUAL "exeHalTest")
            ////set(INTEGRATION_TARGET_DIRECTORY "C:/AERTOS/AERTOS/src/AE/hal/exeHalTest")
            //Regex re = new Regex(@"STREQUAL\s*\""" + FromLibraryName + @"\""\s*\)\s*set\s*\(\s*INTEGRATION_TARGET_DIRECTORY\s*\""(?<ArgReqContents>.*)\""");
            //_ProjectDirectory = re.Match(cmakeCont).Value;

            //_ProjectDirectory = FromLibraryName == "CGENTest" ? @"C:\CodeGenerator\CodeGenerator\macro2Test\CGENTest" : _ProjectDirectory; //for debugging


            _ProjectDirectory = QRInitializing.GetRunningDirectoryFromProjectName(fromLibraryName);

            AllAOWritableToAOClassContents.Add(this);
        }

        public string ProjectDirectory { get => _ProjectDirectory; set => _ProjectDirectory = value; }
        private string _FromModuleName;
        public string FromModuleName { get { return _FromModuleName; } protected set {
                _FromModuleName = value;
                _ProjectDirectory = QRInitializing.GetRunningDirectoryFromProjectName(value);
            }
        }

        private string _ProjectDirectory;

        public void WriteTheContentedToFiles()
        {
            //???if this AO is not apart of the current project, DONT write any files. This might mess with build 
            var tt = _WriteTheContentedToFiles();
            if (tt != null)
            {
                foreach (var contentesToW in tt)
                {

                    string FullPath = Path.Combine(_ProjectDirectory, contentesToW.RelativePath, contentesToW.FileNameWithoutExt);

                    QRInitializing.TheMacro2Session.WriteFileContents(contentesToW.ConentsToWrite, FullPath, contentesToW.FileExtension, contentesToW.UseMacro1, contentesToW.IncludeHeader);
                }
            }

        }

        public static void WriteAllFileContents()
        {
                var snapshot = AllAOWritableToAOClassContents.ToList();
            foreach (var item in snapshot)
            {
                item.WriteTheContentedToFiles();
            }
        }

        protected abstract List<RelativeDirPathWrite> _WriteTheContentedToFiles();

        public abstract string GetFullTemplateType();

        public abstract string GetFullTemplateArgsValues();
        public abstract string GetFullTemplateArgs();

        public static void Reset()
        {
            AllAOWritableToAOClassContents.Clear();
            listOfAdditionalIncludes.Clear();
            atLeastOneEvt = false;
            AllInstancesOfAO.Clear();
            _numOfAOSoFarAEConfigGenerated = 0;
        }
    }

}
