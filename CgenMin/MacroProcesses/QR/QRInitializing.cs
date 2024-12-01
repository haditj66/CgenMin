using CodeGenerator.MacroProcesses.AESetups;
using CodeGenerator.ProblemHandler;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static string GetModuleNameForType(Type type)
        {
            var t = type;// typeof(T);// this.GetType();
            var ns = t.Namespace;


            //get all types of QRProject that are in the assembly of this namespace 
            var types = t.Assembly.GetTypes().Where(t => (t.Namespace == ns && t.IsSubclassOf(typeof(QRModule)))).ToList();

            //if there are more than 2 types of QRproject, then give a problem as there should only be one in one namespace
            if (types.Count > 1)
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem($"There are more than one type of QRProject in the namespace {ns}. There should only be one type of QRProject in one namespace");
            }
            else if (types.Count == 0)
            {
                return "";
            }
            return types[0].Name;
        }

        public static string GetModuleNameForType<T>()
        {
            return GetModuleNameForType(typeof(T));
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

        public static List<QRModule> GetAllCurrentAEProjects()
        {
            var type = typeof(QRModule);
            List<Type> allAEProjectsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && type != p).ToList();

            var allAEProjects = allAEProjectsTypes.Select(s => Activator.CreateInstance(s)).Cast<QRModule>().ToList();



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

        public static QRModule GetProjectIfDirExists(string ofDir)
        {
            var pr = GetAllCurrentAEProjects();

            return pr.Where(p => SameDirectory(ofDir, p.DirectoryOfProject)).FirstOrDefault();

        }

        public static QRModule GetProjectIfNameExists(string nameOfProj)
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

            RunningProjectDir = projToGenerate.DirectoryOfProject;



            //use reflection to get the class with the same RunningProjectName
            var type = typeof(QRModule);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.Name == RunningProjectName)
                .FirstOrDefault();

            if (typeProcessToRun == null)
            {
                this.probhandler.ThereisAProblem($"the AEInit class of projectName {RunningProjectName}  does exist");
            }


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

            //=====================================================
            //generate all events in module, saves events created. 
            //QRProject aeProjectToGetAllEvts = (QRProject)Activator.CreateInstance(typeProcessToRun);
            //aeProjectToGetAllEvts.Init();
            //aeProjectToGetAllEvts.GenerateAllTestForModule();
            //string allEVTcmake = "";
            //foreach (var evtsTarg in QREvent.AllAEEvents)
            //{
            //    //to make sure there are not duplicates, check if the name is already there 
            //    allEVTcmake += "\n\n" + evtsTarg.GenerateCmakeCommand();

            //}

            //QRTarget.Reset();
            //AOWritableToAOClassContents.Reset();
            //QRProject.Reset();
            //ROSSubPub.Reset();
            //QREvent.Reset();


            //=====================================================
            //run the selected target for the project
            QRModule aeProject = (QRModule)Activator.CreateInstance(typeProcessToRun);
            aeProject.Init();

            QREvent.TargetStartRun();
            QRConfig aEConfig = aeProject.GenerateTestOfName(projectTargetName);
            QREvent.TargetEndRun();





            //this will write to a file that will have all interfaces for this exe target. But first, we need to get all the interfaces from all files that have file name InterfaceTargets_filename the name will be under the line that says NAME_OF_MESSAGE.
            //get all files that have naming convention InterfaceTargets_filename.cmake
            //string pathToInterfaceTargets = Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "IF");
            //string[] files = Directory.GetFiles(pathToInterfaceTargets, "InterfaceTargets_*.cmake");
            //List<string> interfaceNamesAlreadyThere = new List<string>();
            //foreach (string file in files)
            //{
            //    if (File.Exists(file))
            //    {
            //        string[] lines = File.ReadAllLines(file);
            //        for (int i = 0; i < lines.Length; i++)
            //        //foreach (string line in lines)
            //        {
            //            if (lines[i].Contains("NAME_OF_MESSAGE\n"))
            //            {
            //                //the next line will have the name of the interface
            //                interfaceNamesAlreadyThere.Add(lines[i + 1]);
            //            }
            //        }
            //    } 
            //}

            //string pathToInterfaceTargets1 = Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "IF", "srv");
            //string pathToInterfaceTargets2 = Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "IF", "msg");
            //string[] files1 = Directory.GetFiles(pathToInterfaceTargets1, "*.srv");
            //string[] files2 = Directory.GetFiles(pathToInterfaceTargets2, "*.msg");
            ////append the two arrays
            //string[] files = new string[files1.Length + files2.Length];
            //List<string> interfaceNamesAlreadyThere = new List<string>();
            //foreach (string file in files)
            //{
            //    string[] lines = File.ReadAllLines(file);
            //    for (int i = 0; i < lines.Length; i++)
            //    //foreach (string line in lines)
            //    {
            //        if (lines[i].Contains("NAME_OF_MESSAGE\n"))
            //        {
            //            //the next line will have the name of the interface
            //            interfaceNamesAlreadyThere.Add(lines[i + 1]);
            //        }
            //    }

            //    if (File.Exists(file))
            //    {
            //        //get the file name
            //        string fileName = Path.GetFileName(file);
            //        interfaceNamesAlreadyThere.Add(fileName);
            //    }
            //}

            //=================================================================================================



            //grab all FSMs and create their DOT files.
            foreach (var fsm in AO.AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.SimpleFSM).Cast<AEFSM>())
            {
                fsm.GenerateDOTDiagramFromUML();
            }




            string aeconfig = AO.All_GenerateAEConfigSection();

            //string linksInit = AO.All_GenerateMainLinkSetupsSection();
            //string funcInit = AO.All_GenerateFunctionDefinesSection();

            QRSetting qrsetting = new QRSetting();// aeProject.GetQRSetting();

            //AEConfig aEConfig = new AEConfig();
            aEConfig.GenerateFile(RunningProjectDir, this, aeconfig, "", qrsetting);


            //generate all cereal settings files
            foreach (var setting in aeProject.GetTargetSetting())
            {
                setting.GenerateFile();
            }

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

            
            string VisualCodeIDE_rqt = "";
            foreach (var exe in aeProject.ListOfTargets_rosEXE)
            {
                VisualCodeIDE_rqt += QRInitializing.TheMacro2Session.GenerateFileOut(
             $"QR\\VisualCodeIDE_RQT",
             new MacroVar() { MacroName = "NAME", VariableValue = exe.TargetName}
             );

                VisualCodeIDE_rqt += "\n";
            }
            //write all text to the launch file for the rqt project 
            CodeGenerator.Program.ReplaceTextInFiles.ReplaceAllTextInAllFilesAndDirWithNewText(
                Path.Combine(RunningProjectDir, "rosqt", ".vscode"), "//AEROSTargetConfig3674563", VisualCodeIDE_rqt, true);

            //now for the cpp project
            string VisualCodeIDE_cp = "";
            foreach (var exe in aeProject.ListOfTargets_cpEXE)
            {
                VisualCodeIDE_cp += QRInitializing.TheMacro2Session.GenerateFileOut(
             $"QR\\VisualCodeIDE_CPP",
             new MacroVar() { MacroName = "NAME", VariableValue = exe.TargetName }
             );

                VisualCodeIDE_cp += "\n";
            }
            //write all text to the launch file for the rqt project
            CodeGenerator.Program.ReplaceTextInFiles.ReplaceAllTextInAllFilesAndDirWithNewText(
                Path.Combine(RunningProjectDir, ".vscode"), "//AEROSTargetConfig3674563", VisualCodeIDE_cp, true);


            if (RunningTarget.qRTargetType == QRTargetType.cpp_exe)
            {
                string AO_DESCRIPTIONS = AO.All_GenerateAO_DESCRIPTIONS_CP();
                string AO_MAINHEADER = AO.All_GenerateAO_MAINHEADER_CP();
                string AO_MAININIT = AO.All_GenerateAO_MAININIT_CP();
                string AO_DECLARES = AO.All_GenerateAO_DECLARES_CP();
                string AO_DEFINE_COMMENTS = AO.All_GenerateAO_DEFINE_COMMENTS_CP();

                WriteFileContents_FromCGENMMFile_ToFullPath(
                   "QR\\QRMain_cp",
                   Path.Combine(RunningProjectDir, "src", $"{projectTargetName}_QRmain.cpp"),
                   true, true,
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = RunningProjectName },
                    new MacroVar() { MacroName = "ProjectTest", VariableValue = projectTargetName },
                    new MacroVar() { MacroName = "AO_DESCRIPTIONS", VariableValue = AO_DESCRIPTIONS },
                    new MacroVar() { MacroName = "AO_MAINHEADER", VariableValue = AO_MAINHEADER },
                    new MacroVar() { MacroName = "AO_MAININIT", VariableValue = AO_MAININIT },
                    new MacroVar() { MacroName = "AO_DECLARES", VariableValue = AO_DECLARES },
                    new MacroVar() { MacroName = "AO_DEFINE_COMMENTS", VariableValue = AO_DEFINE_COMMENTS }
                   );

            }
            else if (RunningTarget.qRTargetType == QRTargetType.rosqt_exe)
            {
                string AO_DESCRIPTIONS = AO.All_GenerateAO_DESCRIPTIONS_RQT();
                string AO_MAINHEADER = AO.All_GenerateAO_MAINHEADER_RQT();
                string AO_MAININIT = AO.All_GenerateAO_MAININIT_RQT();
                string AO_DECLARES = AO.All_GenerateAO_DECLARES_RQT();
                string AO_DEFINE_COMMENTS = AO.All_GenerateAO_DEFINE_COMMENTS_RQT();
                string AO_SURROGATE_INIT = AO.All_GenerateAO_SURROGATE_INIT_RQT();


                WriteFileContents_FromCGENMMFile_ToFullPath(
                "QR\\QRMain_rqt",
                Path.Combine(RunningProjectDir, "rosqt", "src", $"{projectTargetName}_QRmain.cpp"),
                true, true,
                 new MacroVar() { MacroName = "MODULENAME", VariableValue = RunningProjectName },
                    new MacroVar() { MacroName = "AO_DESCRIPTIONS", VariableValue = AO_DESCRIPTIONS },
                    new MacroVar() { MacroName = "AO_MAINHEADER", VariableValue = AO_MAINHEADER },
                    new MacroVar() { MacroName = "AO_MAININIT", VariableValue = AO_MAININIT },
                    new MacroVar() { MacroName = "AO_DECLARES", VariableValue = AO_DECLARES },
                    new MacroVar() { MacroName = "AO_DEFINE_COMMENTS", VariableValue = AO_DEFINE_COMMENTS },
                    new MacroVar() { MacroName = "AO_SURROGATE_INIT", VariableValue = AO_SURROGATE_INIT }
                );
            }



            AOWritableToAOClassContents.WriteAllFileContents();



            //======================================================================================================
            //generate the cmake command for all the events ===================================================
            QRModule aeProjectToGetAllEvts = (QRModule)Activator.CreateInstance(typeProcessToRun);
            aeProjectToGetAllEvts.Init();
            aeProjectToGetAllEvts.GenerateAllTestForModule();

            string allEVTcmake = "";
            foreach (var evtsTarg in QREvent.AllAEEvents)
            {
                if (evtsTarg.FromModuleName == RunningProjectName)
                {
                    //to make sure there are not duplicates, check if the name is already there 
                    allEVTcmake += "\n\n" + evtsTarg.GenerateCmakeCommand();
                }

            } 


            this.WriteFileContents_FromCGENMMFile_ToFullPath(
                "QR\\InterfaceTargets",
                Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "IF", $"InterfaceTargets.cmake"),
                true, false,
                 new MacroVar() { MacroName = "EMPTY", VariableValue = allEVTcmake }
                 );

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
