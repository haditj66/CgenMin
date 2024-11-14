//#define TESTING
// See https://aka.ms/new-console-template for more information






using CgenMin.MacroProcesses;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.cgenXMLSaves.SaveFiles;
using CodeGenerator.FileTemplates;
using CodeGenerator.FileTemplates.GeneralMacoTemplate;
using CodeGenerator.FileTemplatesMacros;
using CodeGenerator.ProblemHandler;
using CommandLine;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static CodeGenerator.Program;





//Console.ReadLine();

//string DIRECTORYOFTHISCG = @"/home/user/QR_Sync/CgenMin/MacroTests/bin/Debug/net6.0";// Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//string pathtoTemplateFileAndOutputFiles = DIRECTORYOFTHISCG;// + "\\bin\\debug";

//string nameOfcGenMacroFile = "testForUserCodes.cgenM";

//GeneralMacro generalMacro = new GeneralMacro(pathtoTemplateFileAndOutputFiles, nameOfcGenMacroFile);
//generalMacro.CreateTemplate();



CodeGenerator.Program.Main(args);


namespace CodeGenerator
{




    public class Program
    {




        [Verb("macro", HelpText = "create a macro file in this directory out of all .cgenM files")]
        public class MacroOptions
        {

        }

        [Verb("macro2", HelpText = "create a macro using the new version of macro .cgenMM files")]
        public class Macro2Options
        {
            [Value(0, HelpText = "This is the macro process you want to use to generate all the code. Macro Processes")]
            public string macroProcess { get; set; }
        }


        [Verb("QRinit", HelpText = "create a new QR project")]
        public class QRInitOptions
        {
            [Value(0, HelpText = "name of the module you copy your's from")]
            public string fromModuleName { get; set; }
            [Value(1, HelpText = "your new module's name")]
            public string newModuleName { get; set; }

        }

        [Verb("QR_launch", HelpText = "create a new QR project")]
        public class QR_launchOptions
        {
            [Value(0, HelpText = "name of the launch script you want to run")]
            public string name { get; set; }

        }

        [Verb("QR_run", HelpText = "run a qr project passing in the moduleName exeName settingsName")]
        public class QR_runOptions
        {
            [Value(0, HelpText = "moduleName of the exe you want to run")]
            public string moduleName { get; set; }

            [Value(1, HelpText = "exeName of the exe you want to run")]
            public string exeName { get; set; }

            [Value(2, HelpText = "settingsName of the settings file you want")]
            public string settingsName { get; set; }

        }



        [Verb("QR_select", HelpText = "QR utility to selecting integration tests, and code generating AOs")]
        public class qrselectOptions
        {
            [Value(0, HelpText = "name of the QR project to select")]
            public string projectNameSelection { get; set; }
            [Value(1, HelpText = "name of the QR exe target to select")]
            public string projectEXETestSelection { get; set; }
            [Value(2, HelpText = "type of project you are selecting for. Can be cpp or rqt")]
            public string typeOfTheProject { get; set; }
        }


        //make another verb that will be called QR_generate
        [Verb("QR_generate", HelpText = "generate a qr project in your current environment directory")]
        public class QR_generateOptions
        {

        }





        public static string DIRECTORYOFTHISCG = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string CGSAVEFILESBASEDIRECTORY = "CGensaveFiles";
        public static string CGCONFCOMPILATOINSBASEDIRECTORY = "ConfigCompilations";
        public static string PATHTOCONFIGTEST = DIRECTORYOFTHISCG + "..\\..\\..\\..\\ConfigTest"; //@"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\ConfigTest";
        public static string PATHTOCMAKEGUI = DIRECTORYOFTHISCG + "..\\..\\..\\..\\CgenCmakeGui";

 
        public static string CGenMinLocation
        {
            get
            {
                //CGenMinLocation combine variable would be _QRBaseDir with /CgenMin  
                return IfUsingWindows_AllForwardSlashesToBackward(Path.Combine(QRBaseDir, "CgenMin"));
            }
        }

        public static bool IsInitQRBaseDir = false;
        public static string _QRBaseDir;//
        public static string QRBaseDir
        {
            get
            {
                if (!IsInitQRBaseDir)
                {


                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _QRBaseDir = Path.Combine(SyncThingBaseDir, @"QR_Sync");

                    }
                    else
                    {
                        //get the user name that cgen is currently in and use that as home QR base directory
                        _QRBaseDir = @"";
                        var c = new char[] { '/', '\\' };
                        string[] items = DIRECTORYOFTHISCG.Split(c, StringSplitOptions.RemoveEmptyEntries);
                        LinuxHomeUserDir = Path.Combine(@"/" + $"{items[0]}", $"{items[1]}");
                        _QRBaseDir = Path.Combine(@"/" + $"{items[0]}", $"{items[1]}", $"QR_Sync");

                    }

                    IsInitQRBaseDir = true;
                }

                return _QRBaseDir;
            } 
        }

        public static string QRCoreBaseDir;//
        public static string LinuxHomeUserDir;
        public static string SyncThingBaseDir = @"C:\Users\SyncthingServiceAcct";



        public static bool IsInitenvIronDirectory = false;
        public static string envIronDirectory
        {
            get
            {

                if (!IsInitenvIronDirectory)
                {
                    _envIronDirectory = IfUsingWindows_AllForwardSlashesToBackward(_envIronDirectory);

                    //check if on windows
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {


                        //if _envIronDirectory starts with Backward slash, then remove it
                        while (_envIronDirectory.StartsWith("\\"))
                        {
                            _envIronDirectory = _envIronDirectory.Remove(0, 1);
                        }

                        //if _envIronDirectory starts with home, then remove it
                        if (_envIronDirectory.StartsWith("home"))
                        {
                            _envIronDirectory = _envIronDirectory.Remove(0, 4);
                        }

                        while (_envIronDirectory.StartsWith("\\"))
                        {
                            _envIronDirectory = _envIronDirectory.Remove(0, 1);
                        }


                        //change the environment directory to the directory of where syncthing is.
                        _envIronDirectory = Path.Combine(SyncThingBaseDir, _envIronDirectory);// $"{QRBaseDir}\\{envIronDirectory}"; 
                    }

                    IsInitenvIronDirectory = true;
                }
                return _envIronDirectory;

            }
        }


#if TESTING
        //        public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin";//@"C:\QR_sync";
        //public static string _envIronDirectory = @"/home/user/QR_Sync";//@"C:\QR_sync";
        //public static string _envIronDirectory = @"/home/luci/QR_Sync/World/rosqt/Launches";
        public static string _envIronDirectory = @"/home/QR_Sync/world2";

        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/CgenMin/cmakeTest";
        // public static string _envIronDirectory = @"/home/user/QR_Sync/World/rosqt/Launches";
        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/MacroTests";
        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/CgenMin/macro2Test";
        //public static string _envIronDirectory = @"/home/user/HelloWorld"; fb






        //static string[] command = "QRinit tutthree".Split(' ');
        // static string[] command = "macro".Split(' '); 
        //static string[] command = "QR_launch TestLaunchFile".Split(' '); 

        //static string[] command = "QRinit World QTUI".Split(' '); 

        //static string[] command = "QR_launch TestLaunchSur".Split(' '); 
        static string[] command = "QR_generate".Split(' ');
        //static string[] command = "macro2 MyMacroProcessDer".Split(' '); 
        //static string[] command = "QR_run world my_exe_for_my_node WorldNode".Split(' '); 


        // static string[] command = "QRinit tutthree".Split(' ');
        //static string[] command = "QRinit askjdbkjfb".Split(' ');

#else
        static string[] command;
        public static string _envIronDirectory = Environment.CurrentDirectory;
#endif



        public static SaveFilecgenProjectGlobal savefileProjGlobal = new SaveFilecgenProjectGlobal();
        public static SaveFilecgenConfig saveFilecgenConfigGlobal = new SaveFilecgenConfig();
        public static SaveFilecgenProjectLocal SaveFilecgenProjectLocal = new SaveFilecgenProjectLocal();


        public static void Main(string[] args)
        {

            // string blabla = Environment.UserName;

            //I need to change the cgenCmakeConfigFunctions.cmake file located at
            // /home/Environment.UserName/QR_Sync/CgenMin/CgenMin/CgenCmakeConfigFunctions.cmake
            // there is a line of code there that hardcodes the path to this executable directory. I need to change that. 
            // string fileContents = File.ReadAllText(@"/home/"+ Environment.UserName + @"/QR_Sync/CgenMin/CgenMin/CgenCmakeConfigFunctions.cmake");

            // Match m = Regex.Match(fileContents, @"execute_process\(COMMAND  dotnet \/home\/(.*)\/QR_Sync\/CgenMin\/CgenMin\/bin\/Debug\/net6.0\/CgenMin.dll macro" );
            // string modFileContents = fileContents.Remove(m.Groups[1].Index,m.Groups[1].Length);
            // modFileContents = modFileContents.Insert(m.Groups[1].Index, "luci");//Environment.UserName) ;


            // System.Console.WriteLine(   modFileContents);

            //if using windows, change the environment directory to the directory of where syncthing is.




            QRCoreBaseDir = Path.Combine(QRBaseDir, @"QR_Core");


            System.Console.WriteLine("environmentDir: " + envIronDirectory);



#if !TESTING
            command = new string[args.Length];
            if (args.Count() != 0)
            {Array.Copy(args,command,args.Length);}
            else
            { command = "--help".Split(' ');}
 
            
#endif

            System.Console.WriteLine("command: " + command[0]);


            Action RunParser = () =>
            {
                Parser.Default.ParseArguments<MacroOptions, Macro2Options, QRInitOptions, QR_launchOptions, qrselectOptions,  QR_generateOptions, QR_runOptions>(command)
.WithParsed<QRInitOptions>(opts => QRInit(opts))
.WithParsed<MacroOptions>(opts => Macro(opts))
.WithParsed<Macro2Options>(opts => Macro2(opts))
.WithParsed<QR_launchOptions>(opts => QR_launch(opts)) 
.WithParsed<qrselectOptions>(opts => QR_select(opts))
.WithParsed<QR_generateOptions>(opts => QR_generate(opts))
.WithParsed<QR_runOptions>(opts => QR_run(opts));

            };


#if !TESTING
            if (command.Count() != 0)
#else


            if (command != null && command.Count() > 0)
#endif 
            {
                RunParser();
            }
            else
            {
                RunParser();
            }


        }




        #region QR_run command ***************************************************************************
        static ParserResult<object> QR_run(QR_runOptions opts)
        {
            string nullProblem = opts.moduleName == null ? "moduleName" :
            opts.exeName == null ? "exeName" :
            opts.settingsName == null ? "settingsName" : "";
            if (nullProblem != "")
            {
                System.Console.WriteLine($"You didnt provide name {nullProblem}");
                return null;
            }

            QR_Run(opts.moduleName, opts.exeName, opts.settingsName);


            return null;

        }
        #endregion



        #region QR_launch command ***************************************************************************
        static ParserResult<object> QR_launch(QR_launchOptions opts)
        {
            if (opts.name == null || opts.name == "")
            {
                System.Console.WriteLine("You didnt provide a name for the launch file you want to run");
                return null;
            }

            string launchFile = opts.name + ".QRL";
            string launchFileFullpath = Path.Combine($"{envIronDirectory}", launchFile);
            //check if the file exists
            if (!File.Exists(launchFileFullpath))
            {
                System.Console.WriteLine($"The launch file you are trying to call does not exist at {launchFileFullpath}");
                return null;
            }


            //the commands that can be run are 
            //QR_run <module-name> <setting-file-name>
            //WaitForMilliseconds <num-of-seconds>
            //WaitForUserApproval
            string[] fileContents = File.ReadAllLines(launchFileFullpath);
            foreach (var line in fileContents)
            {
                //go through and look for the commands
                Regex QRRunRegex = new Regex(@"QR_run\s+(?<module>\w+)\s+(?<exeName>\w+)\s+(?<setting>\w+)");
                Match mc = QRRunRegex.Match(line);
                if (mc.Success)
                {
                    QR_Run(mc.Groups["module"].Value, mc.Groups["exeName"].Value, mc.Groups["setting"].Value);
                }

                Regex WaitForSecRegex = new Regex(@"\s*WaitForMilliseconds\s+(?<seconds>\d+)\s*");
                mc = WaitForSecRegex.Match(line);
                if (mc.Success)
                {
                    Thread.Sleep(Int32.Parse(mc.Groups["seconds"].Value));
                }

                Regex WaitForUserRegex = new Regex(@"\s*WaitForUserApproval\s*");
                mc = WaitForUserRegex.Match(line);
                if (mc.Success)
                {
                    string resp;
                    do
                    {
                        resp = "";
                        System.Console.WriteLine("Continue? [Y/N]");
                        resp = Console.ReadLine();
                    } while (resp != "Y" && resp != "N" && resp != "y" && resp != "n");

                    if (resp == "N" || resp == "n")
                    {
                        System.Console.WriteLine("exiting program");
                        return null;
                    }
                    else
                    {
                        System.Console.WriteLine("Continuing...");
                    }
                }

            }


            // var FF = Assembly.LoadFile(launchFileFullpath);

            // foreach (var item in FF.GetTypes())
            // {
            //     System.Console.WriteLine(FF.GetName());
            // }

            // var launchfileInst = Activator.CreateInstance(FF.GetType("LaunchFile"), new object[] {});
            // Type LaunchType = FF.GetType();

            // var lunchmethod = LaunchType.GetMethod("Launch");
            // lunchmethod.Invoke(launchfileInst, null);






            return null;
        }
        #endregion

        #region QRInit command ***************************************************************************


        // copy directory from cpp template to new directory
        // -delete the git directory, build, install directories
        //-change the module name in CMakeLists.txt, while keeping the old name in memory
        //- change the include directory name to NEW_MODULE_NAME_cp.
        //- go through all files in src and include/NEW_MODULE_NAME_cp
        //replace all instances of the old module name that you find
        //-ourcolcon
        //- make sure everything built



        //-cd rqt
        //- change the include directory name to ${MODULE_NAME}_rqt.
        //- go through all files in src and include/${MODULE_NAME}_rqt
        //    replace all instances of the old module name that you find
        //- delete the install_win and install_lin folders

        //-cd IF
        //- delete the install_win and install_lin folders


        //finally time to build and source everything. do this all in one bash file to keep environment
        //variables

        //- call c:\opt\ros\foxy\x64\setup.bat
        //- cd base directory of your module
        //- call ourcolcon
        //- call oursource

        //- cd rqt/IF
        //- call ourcolcon
        //- call oursource

        //- cd ..
        //- call ourcolcon
        //- call oursource


        private static void CloneDirectory(string root, string dest, List<string> neglectedDirs = null)
        {
            if (neglectedDirs == null)
            {
                neglectedDirs = new List<string>();
            }

            foreach (string directory in Directory.GetDirectories(root))
            {
                if (!neglectedDirs.Contains(new DirectoryInfo(directory).Name))
                {
                    string dirName = Path.GetFileName(directory);
                    if (!Directory.Exists(Path.Combine(dest, dirName)))
                    {
                        Directory.CreateDirectory(Path.Combine(dest, dirName));
                    }
                    CloneDirectory(directory, Path.Combine(dest, dirName));
                }
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
            }
        }

        private static void SetAttributesNormal(DirectoryInfo dir)
        {

            foreach (var subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }


        private static void DeleteDirectoryIfExists_RemoveReadonly(string dir)
        {

            if (Directory.Exists(dir))
            {
                //remove readonly crap
                SetAttributesNormal(new System.IO.DirectoryInfo(dir));

                //delete directory recursively
                Directory.Delete(dir, true);
            }

        }


        private static void CreateQTCreatorOpenBatch(CMDHandler cmdvs, string pathOpenQTFrom)
        {


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmdvs.SetMultipleCommands(@"cd " + pathOpenQTFrom);
                cmdvs.SetMultipleCommands("call oursource");
                cmdvs.SetMultipleCommands(@"C:\Qt\Tools\QtCreator\bin\qtcreator.exe");
            }
            else
            {
                cmdvs.SetMultipleCommands(@"cd " + pathOpenQTFrom);
                SetCommandsToSource(cmdvs);
                cmdvs.SetMultipleCommands("~/Qt/Tools/QtCreator/bin/qtcreator &");
                cmdvs.SetMultipleCommands("source ~/.bashrc");
            }

        }

        private static void SetCommandsToSourceROS(CMDHandler cmdvs)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmdvs.SetMultipleCommands(@$"{cmdvs.GetSourceBatchFile()} c:\opt\ros\foxy\x64\setup.bat");
            }
            else
            {
                cmdvs.SetMultipleCommands("chmod +x ~/.bashrc");
                cmdvs.SetMultipleCommands("source ~/.bashrc");
            }
        }

        private static void SetCommandsToSourceQRCore(CMDHandler cmdvs)
        {
            cmdvs.SetMultipleCommands(@$"cd {QRCoreBaseDir}");
            SetCommandsToSource(cmdvs);
        }

        private static void SetCommandsToPressAnyKeyToContinue(CMDHandler cmdvs)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmdvs.SetMultipleCommands("echo Press any key to exit . . .");
                cmdvs.SetMultipleCommands(@"pause>nul");
            }
            else
            {
                cmdvs.SetMultipleCommands("read -p \"Press any key to exit\" ");
            }
        }

        private static void SetCommandsToBuildAndSourceProjAt(CMDHandler cmdvs, string atDir)
        {
            //go to the IF directory to build
            cmdvs.SetMultipleCommands(@"cd " + atDir);
            //build the interface project
            SetCommandsToColconBuild(cmdvs);
            //source the Interface project
            SetCommandsToSource(cmdvs);

        }

        private static void SetCommandsToColconBuild(CMDHandler cmdvs)
        {
            cmdvs.SetMultipleCommands($"ourcolcon");
        }
        private static void SetCommandsToSource(CMDHandler cmdvs)
        {
            cmdvs.SetMultipleCommands($"{cmdvs.GetSourceBatchFile()} oursource.bash");
        }








        static ParserResult<object> QRInit(QRInitOptions opts)
        {
            Console.WriteLine(envIronDirectory);



            //if they didnt provide a name for the module
            if (opts.fromModuleName == null)
            {
                ProblemHandle p = new ProblemHandle();
                p.ThereisAProblem("you did not provide the first argument fromModuleName.  QRinit <fromModuleName> <newModuleName>");
                return null;
            }
            if (opts.newModuleName == null)
            {
                ProblemHandle p = new ProblemHandle();
                p.ThereisAProblem("you did not provide the second argument newModuleName.  QRinit <fromModuleName> <newModuleName>");
                return null;
            }
            string pathToFromBaseMod = GetBasePathOfQRModule(opts.fromModuleName);

            string pathToBaseMod = Path.Combine(QRBaseDir, opts.newModuleName);//$"{envIronDirectory}\\{opts.name}";
            string rosqtPathOfModule = Path.Combine(pathToBaseMod, "rosqt");
            string IFPathOfModule = Path.Combine(rosqtPathOfModule, "IF");




            //if they tried to init the project in a directory that already exists
            string sss = GetBasePathOfQRModule(opts.newModuleName);
            if (sss != "")//(Directory.Exists($"{envIronDirectory}\\{opts.name}") && (opts.isToRename == false))
            {
                ProblemHandle p = new ProblemHandle();
                p.ThereisAProblem($"you tried to initialize a module named {opts.newModuleName} that already exists");
                return null;
            }

            if (false)//(opts.isToRename == false)
            {

                //----------------------------------------------------------------------------------------
                Console.WriteLine("---cloning base template from the QR_sync/cpp_template directory");
                CloneDirectory($"{pathToFromBaseMod}", $"{pathToBaseMod}", new List<string>() { @".git" });
                Console.WriteLine("---finished cloning");

                Console.WriteLine("---deleting the git repo.");
                DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/.git");

            }


            CloneDirectory($"{pathToFromBaseMod}", $"{pathToBaseMod}", new List<string>() { @".git" });

            //----------------------------------------------------------------------------------------
            Console.WriteLine("---deleting build and install directories");
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"build"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"install_win"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"install_lin"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"log"));


            //----------------------------------------------------------------------------------------
            Console.WriteLine("---changing the module name in CMakeLists.txt and in the config/module_name.cmake, while keeping the old name in memory");
            //get old name
            string contents = File.ReadAllText(pathToBaseMod + @"/CMakeLists.txt");
            string contents2 = File.ReadAllText(pathToBaseMod + @"/config/module_name.cmake");
            var maches = Regex.Matches(contents, @"QR_module\((.*)\)");
            string oldName = opts.fromModuleName;//maches[0].Groups[1].Value;
            string contentsReplace = Regex.Replace(contents, oldName, opts.newModuleName);
            string contentsReplace2 = Regex.Replace(contents2, oldName, opts.newModuleName);
            File.WriteAllText(pathToBaseMod + @"/CMakeLists.txt", contentsReplace);
            File.WriteAllText(pathToBaseMod + @"/config/module_name.cmake", contentsReplace2);

            Console.WriteLine("---going through all files package.xml files replacing all instances of the old module name that you find");
            var fileIncpp = pathToBaseMod + @"/package.xml";
            var fileInrqt = pathToBaseMod + @"/rosqt/package.xml";
            var fileInIF = pathToBaseMod + @"/rosqt/IF/package.xml";
            List<string> allFilesToChangexml = new List<string>();
            allFilesToChangexml.Add(fileIncpp); allFilesToChangexml.Add(fileInrqt); allFilesToChangexml.Add(fileInIF);
            foreach (var filetochange in allFilesToChangexml)
            {
                Console.WriteLine($"    changing all occurences of {oldName} with {opts.newModuleName} in file {filetochange}");
                contents = File.ReadAllText(filetochange);
                string contentrp = Regex.Replace(contents, opts.fromModuleName + @"_i", opts.newModuleName + @"_i");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + @"_cp", opts.newModuleName + @"_cp");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + @"_rqt", opts.newModuleName + @"_rqt");
                File.WriteAllText(filetochange, contentrp);
            }



            //----------------------------------------------------------------------------------------
            Console.WriteLine("---change the include directory name to ${MODULE_NAME}_cp.");
            if (!Directory.Exists(pathToBaseMod + @"/include/" + oldName + "_cp"))
            {
                Console.WriteLine("WARNING: could not find directory include/" + oldName + "");
            }
            else
            {
                Directory.Move(pathToBaseMod + @"/include/" + oldName + "_cp", pathToBaseMod + @"/include/" + opts.newModuleName + "_cp");
            }


            //----------------------------------------------------------------------------------------
            //-cd IF 
            Console.WriteLine("deleting the build, install_win, and install_lin folders in the IF folder");
            DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/rosqt/IF/build");
            DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/rosqt/IF/install_win");
            DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/rosqt/IF/install_lin");
            DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/rosqt/IF/log");

            Console.WriteLine("build the interface project. this is the first one that is built because everthing in this module depends on this one");
            string pathToBatch = Path.Combine(rosqtPathOfModule, "Launches", "Bash");
            CMDHandler cmdvs = new CMDHandler(pathToBaseMod, pathToBaseMod, true);


            SetCommandsToSourceROS(cmdvs);
            SetCommandsToSourceQRCore(cmdvs);
            SetCommandsToBuildAndSourceProjAt(cmdvs, Path.Combine(pathToBaseMod, "rosqt", "IF"));
            cmdvs.ExecuteMultipleCommands_InItsOwnBatch(pathToBaseMod, "initModule");


            PromptAQuestionToContinue("did it colcon build right?", () =>
            {
                //delete dir
                DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod);
                System.Environment.Exit(1);
            });

            //----------------------------------------------------------------------------------------
            Console.WriteLine($"---going through all files in src and include/{opts.newModuleName}_cp and" +
                " replacing all instances of the old module name that you find");
            var filesInSrc = Directory.GetFiles(Path.Combine(pathToBaseMod, "src")).ToList();
            var filesInInc = Directory.GetFiles(Path.Combine(pathToBaseMod, "include", opts.newModuleName + "_cp")).ToList();
            var filesInSrcUnit = Directory.GetFiles(Path.Combine(pathToBaseMod, "unit_tests", "src")).ToList();
            var filesInIncUnit = Directory.GetFiles(Path.Combine(pathToBaseMod, "unit_tests", "include")).ToList();
            List<string> allFilesToChange = new List<string>();
            allFilesToChange.AddRange(filesInSrc); allFilesToChange.AddRange(filesInInc); allFilesToChange.AddRange(filesInSrcUnit); allFilesToChange.AddRange(filesInIncUnit);
            foreach (var filetochange in allFilesToChange)
            {
                Console.WriteLine($"    changing all occurences of {oldName}_cp with {opts.newModuleName}_cp in file {filetochange}");
                contents = File.ReadAllText(filetochange);
                string contentrp = Regex.Replace(contents, opts.fromModuleName + "_i", opts.newModuleName + "_i");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_cp", opts.newModuleName + "_cp");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_rqt", opts.newModuleName + "_rqt");
                File.WriteAllText(filetochange, contentrp);
            }


            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- running ourcolcon for the cpp project portion of your module");



            //first source QR_core
            SetCommandsToSourceROS(cmdvs);
            SetCommandsToSourceQRCore(cmdvs);
            //source the Interface project
            cmdvs.SetMultipleCommands(@"cd " + Path.Combine(pathToBaseMod, "rosqt", "IF"));
            SetCommandsToSource(cmdvs);
            //colcon build the cpp project
            SetCommandsToBuildAndSourceProjAt(cmdvs, pathToBaseMod);

            cmdvs.ExecuteMultipleCommands_InItsOwnBatch(pathToBaseMod, "initModule");



            PromptAQuestionToContinue("did it colcon build right?", () =>
            {
                //delete dir
                DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod);
                System.Environment.Exit(1);
            });


            //creating batch file for opening up qt creator with cp sourced
            CreateQTCreatorOpenBatch(cmdvs, pathToBaseMod);
            cmdvs.ExecuteMultipleCommands_InItsOwnBatch(pathToBaseMod, "OpenQTCreatorHere");


            Console.WriteLine("\n\n--- done running ourcolcon fpr cpp project");

            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- switching to the rosqt portion of your module");
            //pathToBaseMod = pathToBaseMod + "/rosqt"; 


            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- change the include directory name to ${MODULE_NAME}_rqt.");
            string pathToRosqtIncludeDirNEW = Path.Combine(rosqtPathOfModule, "include", opts.newModuleName + "_rqt");
            string pathToRosqtIncludeDirOLD = Path.Combine(rosqtPathOfModule, "include", opts.fromModuleName + "_rqt");
            if (!Directory.Exists(pathToRosqtIncludeDirOLD))
            {
                Console.WriteLine("WARNING: could not find directory rosqt/include/" + oldName + "");
            }
            else
            {
                Directory.Move(pathToRosqtIncludeDirOLD, pathToRosqtIncludeDirNEW);
            }


            //----------------------------------------------------------------------------------------
            Console.WriteLine("---deleting build and install directories");
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"build"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"install_win"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"install_lin"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"log"));

            //----------------------------------------------------------------------------------------
            Console.WriteLine("---changing the module name in CMakeLists.txt, ");
            contents = File.ReadAllText(Path.Combine(rosqtPathOfModule, @"CMakeLists.txt"));
            contentsReplace = Regex.Replace(contents, opts.fromModuleName, opts.newModuleName);
            File.WriteAllText(Path.Combine(rosqtPathOfModule, @"CMakeLists.txt"), contentsReplace);



            //----------------------------------------------------------------------------------------
            Console.WriteLine("---going through all files in src and include /${ MODULE_NAME}_rqt and" +
                " replacing all instances of the old modulename_rqt that you find");
            filesInSrc = Directory.GetFiles(Path.Combine(rosqtPathOfModule, "src")).ToList();
            filesInInc = Directory.GetFiles(pathToRosqtIncludeDirNEW).ToList();
            allFilesToChange = new List<string>(); allFilesToChange.AddRange(filesInSrc); allFilesToChange.AddRange(filesInInc);
            foreach (var filetochange in allFilesToChange)
            {
                Console.WriteLine($"    changing all occurences of {oldName}_rqt with {opts.newModuleName}_rqt in file {filetochange}");
                contents = File.ReadAllText(filetochange);
                string contentrp = Regex.Replace(contents, opts.fromModuleName + "_i", opts.newModuleName + "_i");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_cp", opts.newModuleName + "_cp");
                contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_rqt", opts.newModuleName + "_rqt");
                File.WriteAllText(filetochange, contentrp);
            }





            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- finally time to build and source everything. do this all in one bash file to keep environment");

            SetCommandsToSourceROS(cmdvs);
            SetCommandsToSourceQRCore(cmdvs);
            //go to the IF directory to build
            cmdvs.SetMultipleCommands(@"cd " + IFPathOfModule);
            //source the Interface project 
            SetCommandsToSource(cmdvs);
            //go back to the cpp project
            cmdvs.SetMultipleCommands(@"cd ../..");
            //just source this one
            SetCommandsToSource(cmdvs);
            //go to the rosqt again
            cmdvs.SetMultipleCommands(@"cd rosqt");
            //build and source this one
            SetCommandsToColconBuild(cmdvs);
            SetCommandsToSource(cmdvs);
            //SetCommandsToPressAnyKeyToContinue(cmdvs);

            cmdvs.ExecuteMultipleCommands_InItsOwnBatch(pathToBaseMod, "initModule");

            PromptAQuestionToContinue("did it colcon build right?", () =>
            {
                //delete dir
                DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod);
                System.Environment.Exit(1);
            });


            CreateQTCreatorOpenBatch(cmdvs, rosqtPathOfModule);

            Console.WriteLine("\n\n--- done running ourcolcon for rosqt");


            Console.WriteLine("\n\n--- finished initializing everything. wanna open qt creator for the cp project?");
            return null;
        }

        #endregion



        #region Macro command ***************************************************************************


        static ParserResult<object> Macro(MacroOptions opts)
        {


            System.Console.WriteLine("creating macros from all .cgen files located at" + envIronDirectory);

            //get all macro files in the environment directory
            List<string> cgenMFiles = Directory.GetFiles(envIronDirectory).Where((file) =>
            {
                return Path.GetExtension(file).Equals(".cgenM");
            }).ToList();


            if (cgenMFiles.Count == 0)
            {
                Console.WriteLine("no files with extension .cgenM was found at this directory.");
            }
            else
            {

                //go through each .cgenM file and create it
                foreach (var cgenMFilePath in cgenMFiles)
                {
                    string s = Path.GetDirectoryName(cgenMFilePath);
                    GeneralMacro generalMacro = new GeneralMacro(s, Path.GetFileName(cgenMFilePath));
                    generalMacro.CreateTemplate();

                    Console.WriteLine(Path.GetFileName(cgenMFilePath) + " macro was created.");
                }

            }



            return null;
        }

        #endregion


         


        #region Macro2 command ***************************************************************************






        static ParserResult<object> Macro2(Macro2Options opts)
        {
            ProblemHandle prob = new ProblemHandle();

            if (opts.macroProcess == null || opts.macroProcess == "")
            {
                prob.ThereisAProblem("You didnt provide a macroProcess name. do that with \"macro <macroProcess>\"");
                return null;
            }

            //use reflection to get all classes that inherit from the abstract MacroProcess class
            var type = typeof(MacroProcess);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.Name == opts.macroProcess)
                .FirstOrDefault();

            if (typeProcessToRun == null)
            {
                prob.ThereisAProblem($"the process {opts.macroProcess} you gave does not have an implementation here. create a class that derives from MacroProcess and give it the name of the process you provided");
            }

            MacroProcess myProc = (MacroProcess)Activator.CreateInstance(typeProcessToRun);
            myProc.Init(envIronDirectory);
            myProc.RunProcess();






            return null;
        }

        #endregion




        #region aeselect command ***************************************************************************


        static ParserResult<object> QR_select(qrselectOptions opts)
        {

            RunAEConfigProjectCommand($"QR_select {opts.projectNameSelection} {opts.projectEXETestSelection} {opts.typeOfTheProject}");


            return null;
        }
        

        #endregion


        #region QR_generate command ***************************************************************************






        static ParserResult<object> QR_generate(QR_generateOptions opts)
        {
            RunAEConfigProjectCommand("QR_generate");

            //string ProjectName = GetQRProjectName();
            //string ProjectTest = "";// GetAEProjectTestName();

            ////generate the project.
            //QRInitializing aEInitializing = new QRInitializing();
            //var projectSelected = QRInitializing.GetProjectIfNameExists(ProjectName);
            ////QRInitOptions aeinitOptions = new QRInitOptions() { fromModuleName = ProjectName };
            ////_qrinitProjectFileStructure(aeinitOptions, aEInitializing, projectSelected, projectSelected.DirectoryOfLibrary);

            //aEInitializing.GenerateProject(ProjectName, ProjectTest);

            return null;
        }

        #endregion






        #region aeinit command ***************************************************************************

        //public static void _aeinitProjectFileStructure(string nameOfTheProject, QRInitializing aEInitializing, QRProject projAlreadyExists, string pathToProject)
        //{

        //    // write to all initialization files within the project. DONT OVERWRITE THESE FILES IF THEY ALREADY EXIST!

        //    string PathToThis = pathToProject;
        //    //root======================================================
        //    //AEConfigProject.cmake 
        //    //AEConfigProjectUser.cmake     (blank) 
        //    //IntegTestPipeline.h           (blank)
        //    //main.cpp
        //    string AEConfigProject = Path.Combine(PathToThis, "AEConfigProject.cmake");
        //    string AEConfigProjectUser = Path.Combine(PathToThis, "AEConfigProjectUser.cmake");
        //    string IntegTestPipeline = Path.Combine(PathToThis, "IntegTestPipeline.h");
        //    string main = Path.Combine(PathToThis, "main.cpp");

        //    aEInitializing.WriteFileContents_FromCGENMMFile_ToFullPath(
        //        "AERTOS\\AEConfigProject",
        //        AEConfigProject,
        //        false, false,
        //         new MacroVar() { MacroName = "TestNamesList", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "LibrariesIDependOn", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "ProjectName", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "ProjectDir", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "AnyAdditionalIncludeDirs", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "AnyAdditionalSRCDirs", VariableValue = string.Join(" ", "") },
        //         new MacroVar() { MacroName = "DependsInit", VariableValue = string.Join(" ", "") }
        //        );

        //    if (File.Exists(AEConfigProjectUser) == false)
        //    { File.Create(AEConfigProjectUser); }

        //    if (File.Exists(IntegTestPipeline) == false)
        //    { File.Create(IntegTestPipeline); }

        //    aEInitializing.WriteFileContents_FromCGENMMFile_ToFullPath(
        //        "AERTOS\\main",
        //        main,
        //        false, false
        //        );


        //    PathToThis = Path.Combine(pathToProject, "include");
        //    //root/include======================================================
        //    if (Directory.Exists(PathToThis) == false)
        //    { Directory.CreateDirectory(PathToThis); }

        //    PathToThis = Path.Combine(pathToProject, "src");
        //    //root/src======================================================
        //    if (Directory.Exists(PathToThis) == false)
        //    { Directory.CreateDirectory(PathToThis); }


        //    PathToThis = Path.Combine(pathToProject, "conf");
        //    //root/conf======================================================
        //    //AEConfig.h            (blank)
        //    //EventsForProject.h    (blank)
        //    //EventsForProject.cpp  (blank)
        //    //UserBSPConfig.cpp   
        //    string AEConfig = Path.Combine(PathToThis, "AEConfig.h");
        //    string EventsForProjecth = Path.Combine(PathToThis, "EventsForProject.h");
        //    string EventsForProjectcpp = Path.Combine(PathToThis, "EventsForProject.cpp");
        //    string UserBSPConfig = Path.Combine(PathToThis, "UserBSPConfig.cpp");

        //    if (Directory.Exists(PathToThis) == false)
        //    { Directory.CreateDirectory(PathToThis); }

        //    if (File.Exists(AEConfig) == false)
        //    { File.Create(AEConfig); }

        //    if (File.Exists(EventsForProjecth) == false)
        //    { File.Create(EventsForProjecth); }

        //    if (File.Exists(EventsForProjectcpp) == false)
        //    { File.Create(EventsForProjectcpp); }

        //    aEInitializing.WriteFileContents_FromCGENMMFile_ToFullPath(
        //        "AERTOS\\UserBSPConfig",
        //        UserBSPConfig,
        //        false, false
        //        );

        //    PathToThis = Path.Combine(pathToProject, "CGensaveFiles");
        //    //root/CGensaveFiles======================================================
        //    //SavedOptions.txt (blank)
        //    if (Directory.Exists(PathToThis) == false)
        //    { Directory.CreateDirectory(PathToThis); }
        //    string SavedOptions = Path.Combine(PathToThis, "SavedOptions.txt");

        //    if (File.Exists(SavedOptions) == false)
        //    { File.Create(SavedOptions); }


        //    PathToThis = Path.Combine(pathToProject, "CGensaveFiles", "cmakeGui");
        //    //root/CGensaveFiles/cmakeGui------------------------
        //    if (Directory.Exists(PathToThis) == false)
        //    { Directory.CreateDirectory(PathToThis); }


        //    //root/CGensaveFiles/cmakeGui/{ProjectName}_{boardTarget}/Debug======================================================
        //    //cgenCmakeCache.cmake      (blank)
        //    //IntegrationTestMacros.h   
        //    List<string> tdepends = new List<string>();
        //    if (projAlreadyExists != null)
        //    {
        //        tdepends = projAlreadyExists.GetAllLibrariesIDependOnFlattenedSTR();
        //    }
        //    //IntegrationMacroFileHandler integMacroFile = new IntegrationMacroFileHandler(nameOfTheProject, tdepends, pathToProject, aEInitializing);
        //    //integMacroFile.CreateAllIntegrationFilesTheFiles();

        //    foreach (var boardTarget in QRProject.ListOfBoardTargets)
        //    {
        //        PathToThis = Path.Combine(pathToProject, "CGensaveFiles", "cmakeGui", $"{nameOfTheProject}_{boardTarget}", "DEBUG");

        //        string cgenCmakeCache = Path.Combine(PathToThis, "cgenCmakeCache.cmake");
        //        string IntegrationTestMacros = Path.Combine(PathToThis, "IntegrationTestMacros.h");
        //        if (Directory.Exists(PathToThis) == false)
        //        { Directory.CreateDirectory(PathToThis); }

        //        if (File.Exists(cgenCmakeCache) == false)
        //        { File.Create(cgenCmakeCache); }

        //    }

        //    return;
        //}



 

        #endregion






        #region helper static functions  ***************************************************************************
        //***************************************************************************************************  



        private static void RunAEConfigProjectCommand(string commandToRun)
        {
            string pathToconfexe = Path.Combine(QRProject.BaseAEDir, @"AAAConfigProj\ConfigProjects\bin\Debug");
            CMDHandler cMDHandler = new CMDHandler(pathToconfexe, pathToconfexe, true);

            cMDHandler.ExecuteCommand($"ConfigProjects.exe {commandToRun}");

            Console.WriteLine(cMDHandler.Error);
            Console.WriteLine(cMDHandler.Output);
        }


        public static bool isSubDirOfPath(string ParentDir, string SubDir)
        {
            DirectoryInfo di1 = new DirectoryInfo(ParentDir);
            DirectoryInfo di2 = new DirectoryInfo(SubDir);
            bool isParent = false;
            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                {
                    isParent = true;
                    break;
                }
                else di2 = di2.Parent;
            }

            return isParent;
        }


        public static string IfUsingWindows_AllForwardSlashesToBackward(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return path.Replace("/", "\\");
            }
            return path;
        }

        public static string GetQRProjectName()
        {
            string pathTomodulenamecmake = $"{envIronDirectory}/config/module_name.cmake";
            //if using windows
            string AETargetContents = File.ReadAllText(IfUsingWindows_AllForwardSlashesToBackward(pathTomodulenamecmake));

            Regex re = new Regex(@"\s*set\(\s*MODULE_NAME\s+(?<ProjectName>.*)\s*\)\s*\n");
            string ProjectName = re.Match(AETargetContents).Groups["ProjectName"].Value;

            return ProjectName;
        }

        //enum QRProjectType
        //{
        //    cpp,
        //    rqt,
        //    if
        //}

        //public static QRProjectType GetQRProjectType()
        //{

        //    //if the envIronDirectory  variable ends in "rqt" then it is a rqt project, otherwise it is a cpp project
        //    string ProjectType = envIronDirectory.EndsWith("rosqt") ? QRProjectType.rqt : 
        //        envIronDirectory.EndsWith("IF") ? "if" :
        //        "cpp"; 
        //    return ProjectType;
        //}

        public static string GetAEProjectTestName()
        {
            string AETargetContents = File.ReadAllText(@"C:/AERTOS/AERTOS/AETarget.cmake");

            string ProjectName = GetQRProjectName();
            Regex re2 = new Regex(@"\s*set\(\s*INTEGRATION_TESTS_FOR_" + ProjectName + @"\s+(?<ProjectTest>.*)\s*\)\s*\n?");
            string ProjectTest = re2.Match(AETargetContents).Groups["ProjectTest"].Value;

            return ProjectTest;
        }



        private static void PromptAQuestionToContinue(string q, Action whatToDoIfNo = null)
        {
            do
            {
                Console.WriteLine(q + " \n  \'y\' or \'n\'");
                var mm = Console.ReadKey();
                if (mm.KeyChar == 'n' || mm.KeyChar == 'N')
                {
                    if (whatToDoIfNo != null)
                    {
                        whatToDoIfNo();
                    }
                    return;
                }
                else if (mm.KeyChar == 'y' || mm.KeyChar == 'Y')
                {
                    break;
                }
            } while (true);

        }


        static string GetBasePathOfQRModule(string moduleName)
        {
            string basePathOfModule = "";
            DirectoryInfo d = new DirectoryInfo(QRBaseDir);
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

        static bool QR_Run(string moduleName, string exeName, string settings)
        {
            //I need to find the directory this module lives in.
            //go through all directories in the QR_Sync base and look for the  module_name.cmake file
            string basePathOfModule = GetBasePathOfQRModule(moduleName);
            if (basePathOfModule == "")
            {
                return false;
            }
            string rosqtPathOfModule = Path.Combine(basePathOfModule, "rosqt");



            CMDHandler cmdh = new CMDHandler($"{envIronDirectory}", DIRECTORYOFTHISCG, true);
            //cmdh.SetMultipleCommands("gnome-terminal -- sh -c \"bash -c \\\"echo iutiu; exec bash\\\"\"");
            cmdh.SetMultipleCommands("chmod +x ~/.bashrc");
            cmdh.SetMultipleCommands("source ~/.bashrc");
            cmdh.SetMultipleCommands($"cd {rosqtPathOfModule}");
            cmdh.SetMultipleCommands("oursource");
            cmdh.SetMultipleCommands($"ros2 run {moduleName}_rqt {exeName}  {settings}");
            // cmdh.SetMultipleCommands("echo osiufnboi");
            // cmdh.SetMultipleCommands("source /home/user/ros2_foxy/ros2-linux/setup.bash");
            // cmdh.SetMultipleCommands("source /opt/ros/foxy/setup.bash");
            // cmdh.SetMultipleCommands("cd ~/QR_Sync/QR_Core"); 
            // cmdh.SetMultipleCommands("ourcolcon");

            //cmdh.SetMultipleCommands("read -p \"Press enter to continue\"");
            string pathToBatch = Path.Combine(rosqtPathOfModule, "Launches", "Bash");
            cmdh.ExecuteMultipleCommands_InItsOwnBatch($"{pathToBatch}", $"{moduleName}_{exeName}_{settings}");
            return true;
        }




        public static void UpdateCCGKeywordsIncludes()
        {
            //update CGKeywords.h and alllibraryincludes.h here
            FileTemplateAllLibraryInlcudes faAllLibraryInlcudes = new FileTemplateAllLibraryInlcudes(PATHTOCONFIGTEST, savefileProjGlobal);
            FileTemplateCGKeywordDefine fileTemplateCgKeyword = new FileTemplateCGKeywordDefine(PATHTOCONFIGTEST, savefileProjGlobal);
            faAllLibraryInlcudes.CreateTemplate();
            fileTemplateCgKeyword.CreateTemplate();
            Console.WriteLine("LibraryIncludes updated");
        }





        #endregion



    }

}


