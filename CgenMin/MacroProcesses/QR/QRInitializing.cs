using CodeGenerator.MacroProcesses.AESetups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CgenMin.MacroProcesses.QR
{
    public class QRInitializing : MacroProcess
    {

        public static QRInitializing TheMacro2Session;
        public static string RunningProjectName;
        public static QRTarget RunningTarget;
        public static string RunningProjectDir;

        public static bool DependingLib = false;


        public QRInitializing(string projectName, QRTarget projectTargetGenerating)
        {
            RunningTarget = projectTargetGenerating;
            RunningProjectName = projectName;
        }


        static string GetBasePathOfQRModule(string moduleName)
        {
            string basePathOfModule = "";
            DirectoryInfo d = new DirectoryInfo(CodeGenerator.Program.QRBaseDir);
            foreach (var dir in d.GetDirectories())// Directory.GetDirectories(QRBaseDir))
            {
                var modPath = Path.Combine(dir.FullName, "config", "module_name.cmake");
                if (File.Exists(modPath))
                {
                    string mdcont = File.ReadAllText(modPath);
                    //get module name using regex
                    Regex modNameRegex = new Regex(@"\s*set\(\s*MODULE_NAME\s*(?<module>\w+)\s*\)");
                    Match mc = modNameRegex.Match(mdcont);
                    if (mc.Success && moduleName == mc.Groups["module"].Value)
                    {
                        basePathOfModule = dir.FullName;
                        break;
                    }

                }
            }
            return basePathOfModule;

        }
        public static string GetRunningDirectoryFromProjectName(string projectName)
        {

            // from this library name, I need to get the directory that it belongs to.
            //first grab all the contents of the cmake file in C:/AERTOS/AERTOS/CMakeLists.txt .

            return GetBasePathOfQRModule(projectName);
        }

        public static string GetRunningProjectNameFromDirectory(string dirOfProject)
        {

            // from this library name, I need to get the directory that it belongs to.
            //first grab all the contents of the cmake file in C:/AERTOS/AERTOS/CMakeLists.txt .
            string cmakeCont = File.ReadAllText(@"C:/AERTOS/AERTOS/CMakeLists.txt").Replace("/", "\\");
            dirOfProject = dirOfProject.Replace("/", "\\").Replace("\\", @"\\");

            //    STREQUAL "exeHalTest")
            //set(INTEGRATION_TARGET_DIRECTORY "C:/AERTOS/AERTOS/src/AE/hal/exeHalTest")
            string pat = @"STREQUAL\s*""(?<ArgProjName>.*)""\s*\)\s*set\s*\(\s*INTEGRATION_TARGET_DIRECTORY\s*""" + dirOfProject + @"""";
            Regex re = new Regex(pat);
            var mat = re.Match(cmakeCont);//.Groups["ArgProjName"].Value;
            if (mat.Success)
            {
                return mat.Groups["ArgProjName"].Value;
            }
            else
            {
                return "CGENTest";
            }

        }

        public static List<QRProject> GetAllCurrentAEProjects()
        {
            var type = typeof(QRProject);
            List<Type> allAEProjectsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && type != p).ToList();

            var allAEProjects = allAEProjectsTypes.Select(s => Activator.CreateInstance(s)).Cast<QRProject>().ToList();



            return allAEProjects;

            //if (typeProcessToRun == null)
            //{
            //    this.probhandler.ThereisAProblem($"the AEInit class of projectName {RunningProjectName}  does exist");
            //}

            //AEProject aeProject = (AEProject)Activator.CreateInstance(typeProcessToRun);

        }

        private static bool SameDirectory(string path1, string path2)
        {
            var path11 = Path.GetFullPath(path1);
            var path22 = Path.GetFullPath(path2);
            //Console.WriteLine("{0} == {1} ? {2}", path1, path2, string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase));
            return string.Equals(path11, path22, StringComparison.OrdinalIgnoreCase);

        }

        public static QRProject GetProjectIfDirExists(string ofDir)
        {
            var pr = GetAllCurrentAEProjects();

            return pr.Where(p => SameDirectory(ofDir, p.DirectoryOfLibrary)).FirstOrDefault();

        }

        public static QRProject GetProjectIfNameExists(string nameOfProj)
        {
            var pr = GetAllCurrentAEProjects();

            return pr.Where(p => p.Name == nameOfProj).FirstOrDefault();

        }


        public static string ProjectTestName { get; set; }


        public override void RunProcess()
        {

        }

        public void GenerateProject()
        {
            string projectTargetName = RunningTarget.TargetName;

            //AEUtilityService uts = new AEUtilityService("uartDriver", "UUartDriver",Path.Combine(this.EvironmentDirectory, "include", "UUARTDriver.h"));
            //AEUtilityService utstdu = new AEUtilityService("uartDriver", "UUartDriverTDU",Path.Combine(this.EvironmentDirectory, "include", "UUARTDriver.h"));

            const string on = "On";
            const string off = "Off";
            const char space = ' ';

            ProjectTestName = projectTargetName;

            

            TheMacro2Session = this;

            //string sd = GetRunningProjectNameFromDirectory("C:/AERTOS/AERTOS/src/AE/hal/exeHalTest");
            // GetRunningProjectNameFromDirectory(this.EvironmentDirectory);
            var projToGenerate = GetProjectIfNameExists(RunningProjectName);
            if (projToGenerate == null)
            {
                probhandler.ThereisAProblem($"project of name {RunningProjectName} did not exist.");
            }

            RunningProjectDir = projToGenerate.DirectoryOfLibrary;



            //use reflection to get the class with the same RunningProjectName
            var type = typeof(QRProject);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.Name == RunningProjectName)
                .FirstOrDefault();

            if (typeProcessToRun == null)
            {
                this.probhandler.ThereisAProblem($"the AEInit class of projectName {RunningProjectName}  does exist");
            }

            QRProject aeProject = (QRProject)Activator.CreateInstance(typeProcessToRun);
            aeProject.Init();
            //aeProject.InitAE();

            //get the project test lists
            //var methodsOfAEEXETest = AppDomain.CurrentDomain.GetAssemblies()
            //          .SelectMany(s => s.GetTypes())
            //           .Where(p => type.IsAssignableFrom(typeProcessToRun))
            //           .Where(p => typeProcessToRun.Name == p.Name)
            //          .SelectMany(t => t.GetMethods())
            //          .Where(m => m.GetCustomAttributes(typeof(AEEXETest), false).Length > 0)
            //          .ToArray(); 







            //var tt = aeProject.ListOfTests;

            List<QREvent> allevts = new List<QREvent>();
            var eventsinlin = QREvent.AllSelectedTargetEvents;
            allevts.AddRange(eventsinlin);
            //foreach (var projdep in aeProject.GetAllLibrariesIDependOnFlattened())
            //{
            //    DependingLib = true;
            //    var Dependingevts = QREvent.AllSelectedTargetEvents;// projdep.EventsInLibrary;//THIS CHANGED
            //    allevts.AddRange(Dependingevts);
            //    DependingLib = false;
            //}
            //there needs to be at least one event. if there isnt, init a dummyevent. 
            //if (allevts.Count == 0)
            //{
            //    DummyEVT.Init(1);
            //}

            //var peripheralslib = aeProject.PeripheralsInLibrary;

            //this will run the selected target for the project
            QREvent.TargetStartRun();
            QRConfig aEConfig = aeProject.GenerateTestOfName(projectTargetName);
            QREvent.TargetEndRun();



            //generate the cmake command for all the events ===================================================
            string allEVTcmake = "";
            foreach (var evtsTarg in QREvent.AllSelectedTargetEvents)
            {
                allEVTcmake += "\n\n"+evtsTarg.GenerateCmakeCommand();
            }
            this.WriteFileContents_FromCGENMMFile_ToFullPath(
                "QR\\InterfaceTargets",
                Path.Combine(QRInitializing.RunningProjectDir, "rosqt","IF", "InterfaceTargets.cmake"),
                true, false,
                 new MacroVar() { MacroName = "EMPTY", VariableValue = allEVTcmake } 
                 );
            //=================================================================================================



            //grab all FSMs and create their DOT files.
            foreach (var fsm in AO.AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.SimpleFSM).Cast<AEFSM>())
            {
                fsm.GenerateDOTDiagramFromUML();
            }
            
            


            string aeconfig = AO.All_GenerateAEConfigSection();

            //string linksInit = AO.All_GenerateMainLinkSetupsSection();
            //string funcInit = AO.All_GenerateFunctionDefinesSection();



            //AEConfig aEConfig = new AEConfig();
            aEConfig.GenerateFile(RunningProjectDir, this, aeconfig, "");


            //AOWritableToAOClassContents.WriteAllFileContents();

            //string aeConfigOUT = this.GenerateFileOut("AERTOS\\AEConfig",
            //    new MacroVar() { MacroName = "AODefines", VariableValue = aeconfig }
            //    );

            //Console.WriteLine($"generating AEConfig.h ");
            //WriteFileContentsToFullPath(aeConfigOUT, Path.Combine(RunningProjectDir, "conf", "AEConfig.h"), "h", true);

 


            //            rt(CGEN_PROJECT_DIRECTORY "@DependDir@")

            //CREATE_TARGET_INTEGRATIONEXE(NAME_OF_TARGET @DependName@
            //LOCATION_OF_TARGET "@DependDir@"
            //LibrariesToLinkTo AECoreLib @DependDepends@


            //Console.WriteLine($"generating TargetCreation.cmake ");
            //WriteFileContents_FromCGENMMFile_ToFullPath(
            //    "AERTOS\\TargetCreation",
            //    Path.Combine(RunningProjectDir, "cmake", "TargetCreation.cmake"),
            //    true, false,
            //     new MacroVar() { MacroName = "TestNamesList", VariableValue = string.Join(" ", aeProject.ListOfTests) },
            //     new MacroVar() { MacroName = "LibrariesIDependOn", VariableValue = string.Join(" ", aeProject.LibrariesIDependOnStr_LIB) },
            //     new MacroVar() { MacroName = "DependsInit", VariableValue = string.Join(" ", AllDepends) }
            //    );


            List<string> additionalwithParenthesis = new List<string>();
            foreach (var addinc in aeProject.GetAnyAdditionalIncludeDirs())
            {
                additionalwithParenthesis.Add($"\"{addinc}\"");
            }

            List<string> additionalsrcwithParenthesis = new List<string>();
            foreach (var adds in aeProject.GetAnyAdditionalSRCDirs())
            {
                additionalsrcwithParenthesis.Add($"\"{adds}\"");
            }


            //Console.WriteLine($"generating AEConfigProject.cmake ");
            //WriteFileContents_FromCGENMMFile_ToFullPath(
            //"AERTOS\\AEConfigProject",
            //Path.Combine(RunningProjectDir, "AEConfigProject.cmake"),
            //true, false,
            // new MacroVar() { MacroName = "TestNamesList", VariableValue = string.Join(" ", aeProject.ListOfTests) },
            // new MacroVar() { MacroName = "LibrariesIDependOn", VariableValue = string.Join(" ", aeProject.LibrariesIDependOnStr_LIB) },
            // new MacroVar() { MacroName = "ProjectName", VariableValue = QRInitializing.RunningProjectName },
            // new MacroVar() { MacroName = "ProjectDir", VariableValue = QRInitializing.RunningProjectDir },
            // new MacroVar() { MacroName = "AnyAdditionalIncludeDirs", VariableValue = string.Join(" ", additionalwithParenthesis) },
            // new MacroVar() { MacroName = "AnyAdditionalSRCDirs", VariableValue = string.Join(" ", additionalsrcwithParenthesis) },
            // new MacroVar() { MacroName = "DependsInit", VariableValue = string.Join(" ", AllDepends) }
            //);



            ////create testname.cpp file
            //Console.WriteLine($"generating {projectTest}.cpp ");

            
           
            if (RunningTarget.qRTargetType == QRTargetType.cpp_exe)
            {
                string MAINHEADER_CP = AO.All_GenerateMainHeaderSection_CP();
                string MAININIT_CP = AO.All_GenerateMainInitializeSection_CP();

                WriteFileContents_FromCGENMMFile_ToFullPath(
                   "QR\\QRMain_cp",
                   Path.Combine(RunningProjectDir,"src", $"{projectTargetName}_QRmain.cpp"),
                   true, true,
                    new MacroVar() { MacroName = "ProjectName", VariableValue = RunningProjectName },
                    new MacroVar() { MacroName = "ProjectTest", VariableValue = projectTargetName },
                    new MacroVar() { MacroName = "MAINHEADER_CP", VariableValue = MAINHEADER_CP }, 
                    new MacroVar() { MacroName = "MAININIT_CP", VariableValue = MAININIT_CP } 
                   );

            }
            else if (RunningTarget.qRTargetType == QRTargetType.rosqt_exe)
            {

                string MAINHEADER_RQT = AO.All_GenerateMainHeaderSection_RQT();
                string MAININIT_RQT = AO.All_GenerateMainInitializeSection_RQT();



                WriteFileContents_FromCGENMMFile_ToFullPath(
                "QR\\QRMain_rqt",
                Path.Combine(RunningProjectDir, "rosqt", "src", $"{projectTargetName}_QRmain.cpp"),
                true, true,
                 new MacroVar() { MacroName = "ProjectName", VariableValue = RunningProjectName },
                 new MacroVar() { MacroName = "ProjectTest", VariableValue = projectTargetName },
                 new MacroVar() { MacroName = "MAINHEADER_RQT", VariableValue = MAINHEADER_RQT }, 
                 new MacroVar() { MacroName = "MAININIT_RQT", VariableValue = MAININIT_RQT }  
                );
            }

       

            AOWritableToAOClassContents.WriteAllFileContents();
            //WriteFileContentsToFullPath(this.GenerateFileOut("AERTOS\\TargetCreation",
            //    new MacroVar() { MacroName = "AODefines", VariableValue = aeconfig }
            //    ) 
            //    , Path.Combine(RunningProjectDir, "cmake", "TargetCreation.cmake"), "cmake", false);


            return;
        }




        public string UtilityNameCTORSection()
        {

            return "";
        }


    }
}
