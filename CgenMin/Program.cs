//  #define TESTING
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
using System.Xml.Linq;
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
            //[Value(0, HelpText = "name of the module you copy your's from")]
            //public string fromModuleName { get; set; }
            [Value(0, HelpText = "your new module's name")]
            public string newModuleName { get; set; }
            //,ake a optional parameter to set the directory of you ROS sourcing
            [Option('s', "source", Required = false, HelpText = "set the directory of your ROS sourcing. example: -s opt/ros/jazzy")] 
            public string RosSourceDir { get; set; }

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
            [Value(3, HelpText = "optional settings file selection. the settings files are located in \\config\\AllAOSettings of your project")]
            public string SettingFileName { get; set; }
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
                #if WINDOWS
                if (!IsInitQRBaseDir)
                {

                    
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _QRBaseDir = Path.Combine(SyncThingBaseDir, @"QR_Sync");

                    }
                    else
                    {
                    #endif
                        //get the user name that cgen is currently in and use that as home QR base directory
                        _QRBaseDir = @"";
                        var c = new char[] { '/', '\\' };
                        string[] items = DIRECTORYOFTHISCG.Split(c, StringSplitOptions.RemoveEmptyEntries);
                        LinuxHomeUserDir = Path.Combine(@"/" + $"{items[0]}", $"{items[1]}");
                        _QRBaseDir = Path.Combine(@"/" + $"{items[0]}", $"{items[1]}", $"QR_Sync");
#if WINDOWS
                    }

                    IsInitQRBaseDir = true;
                }
#endif

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

                if (IsInitenvIronDirectory == false)
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
        public static string _envIronDirectory = @"/home/hadi/QR_Sync/world2"; 

        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/CgenMin/cmakeTest";
        // public static string _envIronDirectory = @"/home/user/QR_Sync/World/rosqt/Launches";
        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/MacroTests";
        //public static string _envIronDirectory = @"/home/user/QR_Sync/CgenMin/CgenMin/macro2Test";
        //public static string _envIronDirectory = @"/home/user/HelloWorld"; fb






        //static string[] command = "QRinit tutthree".Split(' ');
        // static string[] command = "macro".Split(' '); 
        //static string[] command = "QR_launch TestLaunchFile".Split(' '); 

        //static string[] command = "QRinit World QTUI".Split(' '); 

        // static string[] command = "QR_launch ".Split(' '); 
        static string[] command = "QR_launch TestLaunchFile".Split(' '); 
        //static string[] command = "QR_generate".Split(' ');
        //static string[] command = "macro2 MyMacroProcessDer".Split(' '); 
        //static string[] command = "QR_run world my_exe_for_my_node WorldNode".Split(' '); 


        // static string[] command = "QRinit tutthree".Split(' ');
        //static string[] command = "QRinit sometest  -s opt/ros/jazzy".Split(' ');
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

            //get directory of the Launch file directory
            string moduleName = GetProjectNameFromDirectory(envIronDirectory);
            string projDir = Path.Combine(QRBaseDir, moduleName);
            Console.WriteLine(moduleName);
            if (moduleName == null)
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem("You are not in a QR project directory with base directory of QR_Sync");
            }

            string launchFile = opts.name + ".QRL";
            string launchFileFullpath = Path.Combine($"{projDir}", "rosqt", "Launches", launchFile);
            string launchFullpath = Path.Combine($"{projDir}", "rosqt", "Launches");
            //check if there are any launch files in the directory
            string[] launchFiles = Directory.GetFiles(launchFullpath, "*.QRL"); 
            if (launchFiles.Length == 0)
            {
                System.Console.WriteLine($"The directory you are in does not have a config directory with AllAOSettings directory.  You are not in a QR project directory with base directory of QR_Sync");
                return null;
            }
            //get all the files names in the directory with QRL extension
            string[] files = Directory.GetFiles(launchFullpath, "*.QRL");
            //split files into a string  
            string launchfilesString = files.Select(f => Path.GetFileName(f)).Aggregate((a, b) => a + "\n" + b);


            if (opts.name == null || opts.name == "")
            {
                System.Console.WriteLine($"You didnt provide a name for the launch file you want to run.  Launch files available are\n {launchfilesString}");
                return null;
            }
             

            
            //check if the file exists
            if (!File.Exists(launchFileFullpath))
            {
               
                System.Console.WriteLine($"The launch file you are trying to call does not exist at {launchFileFullpath}. Launch files available are\n {launchfilesString}");
                return null;
            }


            //the commands that can be run are 
            //QR_run <module-name> <setting-file-name>
            //WaitForMilliseconds <num-of-seconds>
            //WaitForUserApproval
            string[] fileContents = File.ReadAllLines(launchFileFullpath);
            string toBashContents = $"#!/bin/bash\n";
            foreach (var line in fileContents)
            {

                //ignore lines that start with //
                if (line.StartsWith("//"))
                {
                    continue;
                }

                //go through and look for the commands
                Regex QRRunRegex = new Regex(@"QR_run\s+(?<module>\w+)\s+(?<exeName>\w+)\s+(?<setting>\w+)");
                Match mc = QRRunRegex.Match(line);
                if (mc.Success)
                {
                    toBashContents += QR_Run(mc.Groups["module"].Value, mc.Groups["exeName"].Value, mc.Groups["setting"].Value);
                }

                Regex WaitForSecRegex = new Regex(@"\s*WaitForMilliseconds\s+(?<seconds>\d+)\s*");
                mc = WaitForSecRegex.Match(line);
                if (mc.Success)
                {   float timemilli = Int32.Parse(mc.Groups["seconds"].Value)/ 1000;
                    toBashContents += $"sleep {timemilli.ToString()}\n";
                    //Thread.Sleep(Int32.Parse(mc.Groups["seconds"].Value));
                }

                Regex WaitForUserRegex = new Regex(@"\s*WaitForUserApproval\s*");
                mc = WaitForUserRegex.Match(line);
                if (mc.Success)
                {
                    toBashContents += "read -p \"Press any key to continue\" ";
                


                    // string resp;
                    // do
                    // {
                    //     resp = "";
                    //     System.Console.WriteLine("Continue? [Y/N]");
                    //     resp = Console.ReadLine();
                    // } while (resp != "Y" && resp != "N" && resp != "y" && resp != "n");

                    // if (resp == "N" || resp == "n")
                    // {
                    //     System.Console.WriteLine("exiting program");
                    //     return null;
                    // }
                    // else
                    // {
                    //     System.Console.WriteLine("Continuing...");
                    // }
                }

            }
            
            //write the bash file
            string bashFile = Path.Combine(launchFullpath, "Bash", opts.name + ".bash");
            File.WriteAllText(bashFile, toBashContents);

            System.Console.WriteLine("Done, written to the bash file at " + opts.name);
            System.Console.WriteLine($"you can run the bash file by typing \n . {bashFile}");

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

            string oldName = "templateprojectwev";

            //getting the settings file for the RosSourceDir
            string RosSourceDir = "";
            string settingsFile = Path.Combine(CGenMinLocation, "CgenMin", "settings", "RosSourceDir.txt");
             

            //check if the settings file exists
            if (!File.Exists(settingsFile))
            {
                //create the file
                File.WriteAllText(settingsFile, "");
            } 
            //check if the optional directory of the ROS source is provided
            if (opts.RosSourceDir != null && opts.RosSourceDir != "")
            {
                RosSourceDir = opts.RosSourceDir;

                // first if it is using windows, dont check for directory validity
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //make sure the directory exists.
                    if (!Directory.Exists(RosSourceDir))
                    {
                        ProblemHandle p = new ProblemHandle();
                        p.ThereisAProblem($"The directory you provided for the ROS sourcing does not exist at {RosSourceDir}");
                        return null;
                    }

                    //else make sure the directory has a file names setup.bash
                    if (!File.Exists(Path.Combine(RosSourceDir, "setup.bash")))
                    {
                        ProblemHandle p = new ProblemHandle();
                        p.ThereisAProblem($"The directory you provided for the ROS sourcing does not have a setup.bash file at {RosSourceDir}");
                        return null;
                    }
                } 
             


                File.WriteAllText(settingsFile, RosSourceDir);

            }
            else
            {
                //check if there is something in that directory
               string RosSourceDirCurrent = File.ReadAllText(settingsFile);
                if (RosSourceDirCurrent == "")
                {
                    ProblemHandle p = new ProblemHandle();
                    p.ThereisAProblem("You did not provide the directory of your ROS sourcing. Please provide it with the -s option. example: -s opt/ros/jazzy");
                    return null;
                }
                else
                {
                    RosSourceDir = RosSourceDirCurrent;
                }
                 
            } 

            //ReplaceTextInFiles.DoIt();



            //if they didnt provide a name for the module
            //if (opts.fromModuleName == null)
            //{
            //    ProblemHandle p = new ProblemHandle();
            //    p.ThereisAProblem("you did not provide the first argument fromModuleName.  QRinit <fromModuleName> <newModuleName>");
            //    return null;
            //}
            if (opts.newModuleName == null)
            {
                ProblemHandle p = new ProblemHandle();
                p.ThereisAProblem("you did not provide the   argument newModuleName.  QRinit  <newModuleName>");
                return null;
            }
            //string pathToFromBaseMod = GetBasePathOfQRModule(opts.fromModuleName);

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

            //if (false)//(opts.isToRename == false)
            //{

            //    //----------------------------------------------------------------------------------------
            //    Console.WriteLine("---cloning base template from the QR_sync/cpp_template directory");
            //    CloneDirectory($"{pathToFromBaseMod}", $"{pathToBaseMod}", new List<string>() { @".git" });
            //    Console.WriteLine("---finished cloning");

            //    Console.WriteLine("---deleting the git repo.");
            //    DeleteDirectoryIfExists_RemoveReadonly(pathToBaseMod + @"/.git");

            //}

            string pathToTemplate = Path.Combine(QRBaseDir, "templateprojectwev");
            CloneDirectory($"{pathToTemplate}", $"{pathToBaseMod}", new List<string>() {
                @".git" });

            //----------------------------------------------------------------------------------------
            Console.WriteLine("---deleting build and install directories");
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"build"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"install_win"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"install_lin"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(pathToBaseMod, @"log"));



            //----------------------------------------------------------------------------------------
            //changing all instances of name templateprojectwev with the new module name
    
            ReplaceTextInFiles.ReplaceAllTextInAllFilesAndDirWithNewText(pathToBaseMod, oldName, opts.newModuleName);




            //----------------------------------------------------------------------------------------
            //Console.WriteLine("---changing the module name in CMakeLists.txt and in the config/module_name.cmake, while keeping the old name in memory");
            ////get old name
            //string contents = File.ReadAllText(pathToBaseMod + @"/CMakeLists.txt");
            //string contents2 = File.ReadAllText(pathToBaseMod + @"/config/module_name.cmake");
            //var maches = Regex.Matches(contents, @"QR_module\((.*)\)");
            //string oldName = opts.fromModuleName;//maches[0].Groups[1].Value;
            //string contentsReplace = Regex.Replace(contents, oldName, opts.newModuleName);
            //string contentsReplace2 = Regex.Replace(contents2, oldName, opts.newModuleName);
            //File.WriteAllText(pathToBaseMod + @"/CMakeLists.txt", contentsReplace);
            //File.WriteAllText(pathToBaseMod + @"/config/module_name.cmake", contentsReplace2);

            //Console.WriteLine("---going through all files package.xml files replacing all instances of the old module name that you find");
            //var fileIncpp = pathToBaseMod + @"/package.xml";
            //var fileInrqt = pathToBaseMod + @"/rosqt/package.xml";
            //var fileInIF = pathToBaseMod + @"/rosqt/IF/package.xml";
            //List<string> allFilesToChangexml = new List<string>();
            //allFilesToChangexml.Add(fileIncpp); allFilesToChangexml.Add(fileInrqt); allFilesToChangexml.Add(fileInIF);
            //foreach (var filetochange in allFilesToChangexml)
            //{
            //    Console.WriteLine($"    changing all occurences of {oldName} with {opts.newModuleName} in file {filetochange}");
            //    contents = File.ReadAllText(filetochange);
            //    string contentrp = Regex.Replace(contents, opts.fromModuleName + @"_i", opts.newModuleName + @"_i");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + @"_cp", opts.newModuleName + @"_cp");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + @"_rqt", opts.newModuleName + @"_rqt");
            //    File.WriteAllText(filetochange, contentrp);
            //}



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

            string pathToBatch = Path.Combine(rosqtPathOfModule, "Launches", "Bash");

            Console.WriteLine("build the interface project. this is the first one that is built because everthing in this module depends on this one");

            //only do this in ubuntu
            CMDHandler cmdvs = new CMDHandler(pathToBaseMod, pathToBaseMod, true);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            { 
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
            }

    

            //----------------------------------------------------------------------------------------
            //Console.WriteLine($"---going through all files in src and include/{opts.newModuleName}_cp and" +
            //    " replacing all instances of the old module name that you find");
            //var filesInSrc = Directory.GetFiles(Path.Combine(pathToBaseMod, "src")).ToList();
            //var filesInInc = Directory.GetFiles(Path.Combine(pathToBaseMod, "include", opts.newModuleName + "_cp")).ToList();
            //var filesInSrcUnit = Directory.GetFiles(Path.Combine(pathToBaseMod, "unit_tests", "src")).ToList();
            //var filesInIncUnit = Directory.GetFiles(Path.Combine(pathToBaseMod, "unit_tests", "include")).ToList();
            //List<string> allFilesToChange = new List<string>();
            //allFilesToChange.AddRange(filesInSrc); allFilesToChange.AddRange(filesInInc); allFilesToChange.AddRange(filesInSrcUnit); allFilesToChange.AddRange(filesInIncUnit);
            //foreach (var filetochange in allFilesToChange)
            //{
            //    Console.WriteLine($"    changing all occurences of {oldName}_cp with {opts.newModuleName}_cp in file {filetochange}");
            //    contents = File.ReadAllText(filetochange);
            //    string contentrp = Regex.Replace(contents, opts.fromModuleName + "_i", opts.newModuleName + "_i");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_cp", opts.newModuleName + "_cp");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_rqt", opts.newModuleName + "_rqt");
            //    File.WriteAllText(filetochange, contentrp);
            //}


            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- running ourcolcon for the cpp project portion of your module");

            //only do this in ubuntu
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            }




            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //creating batch file for opening up qt creator with cp sourced
                CreateQTCreatorOpenBatch(cmdvs, pathToBaseMod);
            cmdvs.ExecuteMultipleCommands_InItsOwnBatch(pathToBaseMod, "OpenQTCreatorHere");
            }

            Console.WriteLine("\n\n--- done running ourcolcon fpr cpp project");

            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- switching to the rosqt portion of your module");
            //pathToBaseMod = pathToBaseMod + "/rosqt"; 


            //----------------------------------------------------------------------------------------
            Console.WriteLine($"--- change the include directory name to ${opts.newModuleName}_rqt.");
            string pathToRosqtIncludeDirNEW = Path.Combine(rosqtPathOfModule, "include", opts.newModuleName + "_rqt");
            //string pathToRosqtIncludeDirOLD = Path.Combine(rosqtPathOfModule, "include", opts.fromModuleName + "_rqt");
            //if (!Directory.Exists(pathToRosqtIncludeDirOLD))
            //{
            //    Console.WriteLine("WARNING: could not find directory rosqt/include/" + oldName + "");
            //}
            //else
            //{
            //    Directory.Move(pathToRosqtIncludeDirOLD, pathToRosqtIncludeDirNEW);
            //}


            //----------------------------------------------------------------------------------------
            Console.WriteLine("---deleting build and install directories");
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"build"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"install_win"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"install_lin"));
            DeleteDirectoryIfExists_RemoveReadonly(Path.Combine(rosqtPathOfModule, @"log"));

            //----------------------------------------------------------------------------------------
            //Console.WriteLine("---changing the module name in CMakeLists.txt, ");
            //contents = File.ReadAllText(Path.Combine(rosqtPathOfModule, @"CMakeLists.txt"));
            //contentsReplace = Regex.Replace(contents, opts.fromModuleName, opts.newModuleName);
            //File.WriteAllText(Path.Combine(rosqtPathOfModule, @"CMakeLists.txt"), contentsReplace);



            //----------------------------------------------------------------------------------------
            //Console.WriteLine("---going through all files in src and include /${ MODULE_NAME}_rqt and" +
            //    " replacing all instances of the old modulename_rqt that you find");
            //filesInSrc = Directory.GetFiles(Path.Combine(rosqtPathOfModule, "src")).ToList();
            //filesInInc = Directory.GetFiles(pathToRosqtIncludeDirNEW).ToList();
            //allFilesToChange = new List<string>(); allFilesToChange.AddRange(filesInSrc); allFilesToChange.AddRange(filesInInc);
            //foreach (var filetochange in allFilesToChange)
            //{
            //    Console.WriteLine($"    changing all occurences of {oldName}_rqt with {opts.newModuleName}_rqt in file {filetochange}");
            //    contents = File.ReadAllText(filetochange);
            //    string contentrp = Regex.Replace(contents, opts.fromModuleName + "_i", opts.newModuleName + "_i");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_cp", opts.newModuleName + "_cp");
            //    contentrp = Regex.Replace(contentrp, opts.fromModuleName + "_rqt", opts.newModuleName + "_rqt");
            //    File.WriteAllText(filetochange, contentrp);
            //}





            //----------------------------------------------------------------------------------------
            Console.WriteLine("--- finally time to build and source everything. do this all in one bash file to keep environment");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            }
            


            CreateQTCreatorOpenBatch(cmdvs, rosqtPathOfModule);

            Console.WriteLine("\n\n--- done running ourcolcon for rosqt");


            //Console.WriteLine("\n\n--- finished initializing everything. wanna open qt creator for the cp project?");


            //----------------------------------------------------------------------------------------
            Console.WriteLine("changing config project name");
            string configProject = Path.Combine(pathToBaseMod,"config", $"{opts.newModuleName}.cs");
            //rename the project file name in the configProject.cs
            File.Move(Path.Combine(pathToBaseMod, "config", $"{oldName}.cs"), configProject);


            //----------------------------------------------------------------------------------------
            Console.WriteLine("adding config file to the project.");
            //get the path to the config project QR_Sync\AAAConfigProj\ConfigProjects\ConfigProjects
            string pathToConfigProject = Path.Combine(QRBaseDir, "AAAConfigProj", "ConfigProjects", "ConfigProjects", "ConfigProjects.csproj");
            CsProjModifier.AddCompileItem(pathToConfigProject, $"..\\..\\..\\{opts.newModuleName}\\config\\{opts.newModuleName}.cs", $"{opts.newModuleName}.cs");



            Console.WriteLine("Done===========================================");
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




        #region QRselect command ***************************************************************************


        static ParserResult<object> QR_select(qrselectOptions opts)
        {

            RunAEConfigProjectCommand($"QR_select {opts.projectNameSelection} {opts.projectEXETestSelection} {opts.typeOfTheProject}  {opts.SettingFileName}");


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



        //=================================================
        //get the project name from the directory you are in 
        //do this by getting the name of the folder that is one above the one called QR_Sync
        //for example if directory is  C:\Users\SyncthingServiceAcct\QR_Sync\world\rosqt\include\world_rqt
        //then answer would be world
        static string GetProjectNameFromDirectory(string directoryPath)
        {
            // Convert the directory path to a DirectoryInfo object
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            // Traverse up the directory structure to find QR_Sync
            int count = 0;
            while (dirInfo != null && dirInfo.Parent.Name != "QR_Sync")
            {
                dirInfo = dirInfo.Parent;
                count++;
                if (((count > 14 )|| (dirInfo == null)))
                  { return null; }
            }
            return dirInfo.Name;
            DirectoryInfo dirInfo2 = new DirectoryInfo(directoryPath);
            for (int i = 0; i < count-1; i++)
            {
                dirInfo2 = dirInfo2.Parent;
            }
            return dirInfo2.Name;

            // If QR_Sync or its parent was not found, return null or throw an exception
            return "";
        }

        public class CsProjModifier
        {
            public static void AddCompileItem(string csprojFilePath, string includePath, string linkPath)
            {
                // Load the .csproj file
                XDocument csproj = XDocument.Load(csprojFilePath);

                // Find the first <ItemGroup> containing <Compile> elements
                XElement? compileItemGroup = csproj
                    .Element("Project")?
                    .Elements("ItemGroup")
                    .FirstOrDefault(group => group.Elements("Compile").Any());

                if (compileItemGroup == null)
                {
                    Console.WriteLine("No <ItemGroup> with <Compile> elements found.");
                    return;
                }

                // Create the new <Compile> element
                XElement newCompileItem = new XElement("Compile",
                    new XAttribute("Include", includePath),
                    new XAttribute("Link", linkPath)
                );

                // Add the new <Compile> element to the <ItemGroup>
                compileItemGroup.Add(newCompileItem);

                // Save the updated .csproj file
                csproj.Save(csprojFilePath);
                Console.WriteLine($"Added new Compile item to {csprojFilePath}");
            }
        }


            public class ReplaceTextInFiles
        {
            public static void ReplaceAllTextInAllFilesAndDirWithNewText(string rootDirectory, string oldText, string newText)
            {
                // Define the root directory to start
                // string rootDirectory = @"C:\Users\SyncthingServiceAcct\QR_Sync\templateprojectwev";

                // Define the text to search for and the replacement text
                string searchText = oldText;// "world2";
                string replacementText = newText;// "templateprojectwev";

                try
                {
                    // Process all files in the directory and subdirectories
                    ProcessDirectory(rootDirectory, searchText, replacementText);
                    Console.WriteLine("Replacement completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            static void ProcessDirectory(string directoryPath, string searchText, string replacementText)
            {
                // Get all files in the current directory
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    ProcessFile(filePath, searchText, replacementText);
                }

                // Recursively process subdirectories
                foreach (string subdirectoryPath in Directory.GetDirectories(directoryPath))
                {
                    ProcessDirectory(subdirectoryPath, searchText, replacementText);
                }
            }

            static void ProcessFile(string filePath, string searchText, string replacementText)
            {
                // Read the file content
                string content = File.ReadAllText(filePath);
                string updatedContent = Regex.Replace(
                        content,
                        $@"{searchText}",
                        replacementText);
                // Replace occurrences of the search text with the replacement text (case-sensitive)
                //string updatedContent = Regex.Replace(content, $@"\b{searchText}\b", replacementText);

                // Write the updated content back to the file
                File.WriteAllText(filePath, updatedContent);

                Console.WriteLine($"Processed file: {filePath}");
            }
        }




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

        static string QR_Run(string moduleName, string exeName, string settings)
        {
            //I need to find the directory this module lives in.
            //go through all directories in the QR_Sync base and look for the  module_name.cmake file
            string basePathOfModule = GetBasePathOfQRModule(moduleName);
            if (basePathOfModule == "")
            {
                return "";
            }
            string rosqtPathOfModule = Path.Combine(basePathOfModule, "rosqt");


 
string pathToBatchDir = Path.Combine(rosqtPathOfModule, "Launches", "Bash");
string pathToBatch = Path.Combine(pathToBatchDir, $"{moduleName}_{exeName}_{settings}.bash");
 
 CMDHandler cmdh = new CMDHandler($"{envIronDirectory}", DIRECTORYOFTHISCG, false); 
 cmdh.SetWorkingDirectory(rosqtPathOfModule);
//  cmdh.ExecuteCommand($"source ~/.bashrc; source ./oursource.bash; ros2 run {moduleName}_rqt {exeName}  {settings};");
//  cmdh.ExecuteCommand($" gnome-terminal -- bash -c \". {moduleName}_{exeName}_{settings}.bash\"");
 //cmdh.ExecuteCommand($"source ./oursource.bash");
//  cmdh.ExecuteCommand($"gnome-terminal -- bash -c \"ros2 run {moduleName}_rqt {exeName}  {settings}; exec bash\"");



            string bashContents = "";//$"#!/bin/bash\n";
            bashContents += $"source ~/.bashrc\n";
            bashContents += $"cd {rosqtPathOfModule}\n";
            bashContents += $"oursource\n";
            bashContents += $"cd -\n";
            // bashContents += $"ros2 run {moduleName}_rqt {exeName}  {settings}"; 
            // bashContents += $"gnome-terminal -- bash -c\n"; 
            bashContents += $"gnome-terminal -- bash -c \"ros2 run {moduleName}_rqt {exeName}  {settings}; exec bash\"\n"; 
 return bashContents;
            cmdh.SetMultipleCommands($"source ~/.bashrc");
            cmdh.SetMultipleCommands($"cd {rosqtPathOfModule}");
            cmdh.SetMultipleCommands($"oursource");
            // cmdh.SetMultipleCommands($"cd -"); 
            cmdh.SetMultipleCommands($"gnome-terminal -- bash -c \"ros2 run {moduleName}_rqt {exeName}  {settings}; exec bash\""); 
             
bool keeptrying = true;
int count =0;
            while (keeptrying)
            { 
                try
                { 
 
                        File.WriteAllText(pathToBatch, "");
                        File.WriteAllText(pathToBatch, bashContents);
                        keeptrying = false; 
  
                }
                catch(Exception e)
                {
                    count++;
                    if (count > 10000)
                    {
                        ProblemHandle p = new ProblemHandle();  
                        p.ThereisAProblem("could not write to the batch file. " + e.Message);
                    }

                }
            }
            //cmdh.ExecuteMultipleCommands_InItsOwnBatch(pathToBatchDir, $"{moduleName}_{exeName}_{settings}");
            //cmdh.ExecuteCommandFromBatch($"{pathToBatch}",  true);
            cmdh.ExecuteCommandFromBatch($"{pathToBatch}",  false);

            
            // cmdh.SetMultipleCommands("chmod +x ~/.bashrc");
            // cmdh.SetMultipleCommands("source ~/.bashrc");
            // cmdh.SetMultipleCommands($"cd {rosqtPathOfModule}");
            // cmdh.SetMultipleCommands("oursource");
            // cmdh.SetMultipleCommands($"ros2 run {moduleName}_rqt {exeName}  {settings}"); 

            // //cmdh.SetMultipleCommands("read -p \"Press enter to continue\"");
            // string pathToBatch = Path.Combine(rosqtPathOfModule, "Launches", "Bash");
            // cmdh.ExecuteMultipleCommands_InItsOwnBatch($"{pathToBatch}", $"{moduleName}_{exeName}_{settings}");
            return "true";
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


