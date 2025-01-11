//using CodeGenerator.MacroProcesses.AESetups;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
using CodeGenerator.ProblemHandler;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses
{



    public enum QRTargetProjectType
    {
        cp,
        rqt,
        IF
    }

    public enum QRTargetType
    {
        cpp_library,
        cpp_exe,
        cpp_unittest,
        rosqt_library,
        rosqt_exe,
        IF,
        NonQR
    }


    public class LibraryDependence
    {
        public QRModule ProjectIDependOn { get; set; }
        public QRTargetType TargetType { get; set; }

        public override string ToString()
        {
            return $"{ProjectIDependOn.Name}:{TargetType}";
        }

        public static LibraryDependence FromString(string str, object obj)
        {

            var parts = str.Split(':');
            var projectName = parts[0];
            var targetType = Enum.Parse<QRTargetType>(parts[1]);

            // Use reflection to find a derived class of AEProject with the specified name
            var projectType = obj.GetType().Assembly
                                      .GetTypes()
                                      .FirstOrDefault(t => t.IsSubclassOf(typeof(QRModule)) && t.Name == projectName);

            if (projectType == null)
            {
                throw new InvalidOperationException($"No derived class of AEProject found with the name {projectName}");
            }

            // Only create the instance of the project if it is not already created
            var projectInstance = QRModule.AllProjects
                                           .FirstOrDefault(a => a.GetType() == projectType)
                                           ?? (QRModule)Activator.CreateInstance(projectType);

            //var projectInstance = (QRProject)Activator.CreateInstance(projectType);

            return new LibraryDependence
            {
                ProjectIDependOn = projectInstance,
                TargetType = targetType
            };
        }
    }




    public class NonQRAONode : AONode<NonQRAONode>
    {
        public string CName;

        public List<ROSPublisher> RosPubs = new List<ROSPublisher>();
        public List<ROSSubscriber> RosSubs = new List<ROSSubscriber>();
        public NonQRAONode(string className, string aoName, string instanceName, List<ROSPublisher> rosPubs, List<ROSSubscriber> rosSubs) : base(className, instanceName)
        {
            IsNonQR = true;
            CName = className;
            RosPubs.AddRange(rosPubs);
            RosSubs.AddRange(rosSubs);
            _Init();
            AONAME = aoName;
        }


        public void SetServiceFunction(ServiceFunction serviceFunction)
        {
            ServiceFunctions.Add(serviceFunction);
        }

        public override List<ROSPublisher> SetAllPublishers()
        {
            return RosPubs;
        }
        public override List<ROSSubscriber> SetAllSubscribers()
        {
            return RosSubs;
        }

        public override List<ROSTimer> SetAllTimers()
        {
            return new List<ROSTimer>()
            {
            };
        }
    }



    public class NonQrServiceFunction
    {
        public string FileNameOfService;
        public string NameOfTopic;

        public NonQrServiceFunction(string fileNameOfService, string nameOfTopic)
        {
            FileNameOfService = fileNameOfService;
            NameOfTopic = nameOfTopic;
        }
    }
    public class NonQrMessage
    {
        public string FileNameOfService;
        public string FullTopicName;
        public string FullHeaderName;
        public string FullMsgClassName;

        public NonQrMessage(string fileNameOfService, string fullTopicName, string fullHeaderName, string fullMsgClassName)
        {
            FileNameOfService = fileNameOfService;
            FullTopicName = fullTopicName;
            FullHeaderName = fullHeaderName;
            FullMsgClassName = fullMsgClassName;
        }
    }



    //[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_NonQR : QRTarget
    {
        //public QRConfig AEconfigToUse { get; protected set; }




        public List<ServiceFunction> AllServiceFuncs = new List<ServiceFunction>();
        public List<ROSPublisher> AllPubs = new List<ROSPublisher>();
        public List<ROSSubscriber> AllSubs = new List<ROSSubscriber>();

        public string NameOfNonQRTarget { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromModuleName">name of module this belongs to</param>
        /// <param name="targetName">name of the outside ros node you want to turn into a target</param>
        /// <param name="pathRelativeToQR_Sync">path the the base of the outside project</param>
        /// <param name="relativePathToSRV">path to th the srv folder relative to the QR_Sync folder</param>
        /// <param name="relativePathToMSG">path to th the msg folder relative to the QR_Sync folder</param>
        /// <param name="nameServiceFileToTurnToServiceFunction">service files names you want to turn into service functions inside a simple AO</param>
        public QRTarget_NonQR(string fromModuleName, string targetName, string pathRelativeToQR_Sync, string relativePathToSRV = "", string relativePathToMSG = "",
            List<NonQrServiceFunction> nameServiceFileToTurnToServiceFunction = null,
            List<NonQrMessage> nameMessageFileToTurnToQrEvent = null)
        //, List<string> nameMessageFileToTurnPublisher = null, List<string> nameMessageFileToTurnSubscriber = null) : base("")
        {

            TargetName = fromModuleName;
            NameOfNonQRTarget = targetName;
            this.qRTargetType = QRTargetType.NonQR;
            BaseProjectPath = Path.Combine(CodeGenerator.Program.QRBaseDir, pathRelativeToQR_Sync);
            PathToSRV = Path.Combine(CodeGenerator.Program.QRBaseDir, relativePathToSRV);
            PathToMSG = Path.Combine(CodeGenerator.Program.QRBaseDir, relativePathToMSG);


            nonQRAONode = new NonQRAONode(TargetName, this.NameOfNonQRTarget, "instanceName", new List<ROSPublisher>(), new List<ROSSubscriber>());

            //=======================================================================================================
            //read the Message file and convert it to a QRevent that you can publish or subscribe to frmo other AOnodes
            //======================================================================================================= 
            if (nameMessageFileToTurnToQrEvent != null)
            {
                var files = Directory.GetFiles(PathToMSG, "*.msg");
                //filter files such that only the files that are in nameOfInterfacesToTurnIntoAOStuff are used
                //var filteredFiles = files.Where(f => nameServiceFileToTurnToServiceFunction.Contains(Path.GetFileNameWithoutExtension(f))).ToList();
                foreach (var filen in nameMessageFileToTurnToQrEvent)
                {
                    //if that file is not in the list of files, then give a problem
                    if (!files.Any(f => Path.GetFileNameWithoutExtension(f) == filen.FileNameOfService))
                    {
                        ProblemHandle problemHandled = new ProblemHandle();
                        problemHandled.ThereisAProblem($"The file {filen.FileNameOfService} does not exist in the path {PathToSRV}");
                    }

                    string f = Path.Combine(PathToMSG, Path.GetFileNameWithoutExtension(filen.FileNameOfService) + ".msg");

                    var fileContents = File.ReadAllLines(f);
                    List<FunctionArgsBase> requestArgs = new List<FunctionArgsBase>();

                    foreach (var line in fileContents)
                    {

                        var parts = line.Trim().Split(' ');
                        if (parts.Length == 2)
                        {
                            //convert string to Type

                            Type type = FunctionArgsBase.ServiceTypeToCsharpType(parts[0]);
                            var namearg = parts[1];
                            var arg = new FunctionArgsBase(type, namearg);
                            requestArgs.Add(arg);
                        } 

                    }
                    QREventMSGNonQR qREventMSG = new QREventMSGNonQR(targetName, filen.FullTopicName, filen.FullHeaderName, filen.FullMsgClassName, requestArgs);
                    var pub = ROSPublisher.CreatePublisher(filen.FileNameOfService, qREventMSG, false);

                    pub.SetAOIBelongTo(nonQRAONode);

                }
            }

            //=======================================================================================================
            //read the service file and convert the interface into a service function
            //======================================================================================================= 
            if (nameServiceFileToTurnToServiceFunction != null)
            {
                //var path = Path.Combine(pathRelativeToQR_Sync, relativePathToSRV);
                var files = Directory.GetFiles(PathToSRV, "*.srv");
                //filter files such that only the files that are in nameOfInterfacesToTurnIntoAOStuff are used
                //var filteredFiles = files.Where(f => nameServiceFileToTurnToServiceFunction.Contains(Path.GetFileNameWithoutExtension(f))).ToList();
                foreach (var filen in nameServiceFileToTurnToServiceFunction)
                {
                    //if that file is not in the list of files, then give a problem
                    if (!files.Any(f => Path.GetFileNameWithoutExtension(f) == filen.FileNameOfService))
                    {
                        ProblemHandle problemHandled = new ProblemHandle();
                        problemHandled.ThereisAProblem($"The file {filen.FileNameOfService} does not exist in the path {PathToSRV}");
                    }

                    string f = Path.Combine(PathToSRV, Path.GetFileNameWithoutExtension(filen.FileNameOfService) + ".srv");

                    var fileContents = File.ReadAllLines(f);
                    List<FunctionArgsBase> requestArgs = new List<FunctionArgsBase>();
                    List<FunctionArgsBase> responseArgs = new List<FunctionArgsBase>();
                    //List<FunctionArgsBase> AllArgs = new List<FunctionArgsBase>();
                    bool isResponsePart = false;

                    bool gettingResponseArgs = false;
                    int count = 0;

                    foreach (var line in fileContents)
                    {
                        if (line.Trim() == "---")
                        {
                            //responseAtBeginning = requestArgs.Count > 1 ? false : true;  
                            gettingResponseArgs = true;
                        }

                        var parts = line.Trim().Split(' ');
                        if (parts.Length == 2)
                        {
                            //convert string to Type

                            Type type = FunctionArgsBase.ServiceTypeToCsharpType(parts[0]);
                            var namearg = parts[1];
                            var arg = new FunctionArgsBase(type, namearg);

                            if (gettingResponseArgs)
                            {
                                responseArgs.Add(arg);
                            }
                            else
                            {
                                requestArgs.Add(arg);
                            }

                        }
                    }

                    //if (responseAtBeginning == true)
                    //{
                    //    responseArgs = requestArgs[0];
                    //    requestArgs.RemoveAt(0);
                    //}
                    //else
                    //{
                    //    responseArgs = requestArgs[requestArgs.Count - 1];
                    //    requestArgs.RemoveAt(requestArgs.Count - 1);
                    //}


                    string name = Path.GetFileNameWithoutExtension(f);
                    var serviceFunction = ServiceFunction.CreateServiceFunction(name, responseArgs, requestArgs, filen.NameOfTopic);
                    AllServiceFuncs.Add(serviceFunction);

                }

            }


            //go through the nameOfInterfacesToTurnIntoAOStuff, and read the file 


        }


        public QREventMSG GetQREventMSGFromMSGFile(string nameOfMSGFile)
        {
            QREventMSG ret = null;
            //=======================================================================================================
            //read the Message file and convert the interface into a Publisher
            //=======================================================================================================  

            //var path = Path.Combine(BaseProjectPath, PathToSRV);
            var files = Directory.GetFiles(PathToMSG, "*.msg");
            //filter files such that only the files that are in nameOfInterfacesToTurnIntoAOStuff are used
            var filteredFiles = files.Where(f => nameOfMSGFile.Contains(Path.GetFileNameWithoutExtension(f))).ToList();
            foreach (var f in filteredFiles)
            {
                var fileContents = File.ReadAllLines(f);
                List<FunctionArgsBase> requestArgs = new List<FunctionArgsBase>();

                foreach (var line in fileContents)
                {
                    var parts = line.Trim().Split(' ');
                    if (parts.Length == 2)
                    {
                        //convert string to Type


                        Type type = FunctionArgsBase.ServiceTypeToCsharpType(parts[0]);
                        var namearg = parts[1];
                        var arg = new FunctionArgsBase(type, namearg);

                        requestArgs.Add(arg);
                    }
                }

                string name = Path.GetFileNameWithoutExtension(f);

                ret = new QREventMSG(TargetName, name, requestArgs);

                //var pub = ROSPublisher.CreatePublisher(name, qREventMSG, false, 10);

                //AllPubs.Add(pub);

            }

            return ret;

        }

        NonQRAONode nonQRAONode; 
        public NonQRAONode Get_NonQRAONode(string instanceName)
        {
            nonQRAONode.InstanceName = instanceName;
            //nonQRAONode = new NonQRAONode(TargetName, this.NameOfNonQRTarget, instanceName, new List<ROSPublisher>(), new List<ROSSubscriber>());
            foreach (var item in AllServiceFuncs)
            {
                nonQRAONode.SetServiceFunction(item);
            }
            return nonQRAONode;
        }


        public string BaseProjectPath { get; }
        public string PathToSRV { get; }
        public string PathToMSG { get; }

        public override void Init()
        {
            AEconfigToUse = new QRConfig();
            //TargetName = $"{ProjIBelongTo.Name}";
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "include", $"{ProjIBelongTo.Name}_cp");
        }
    }



    public abstract class QRTarget_Lib : QRTarget
    {
        public QRTarget_Lib(params string[] libraryDependencies) : base(libraryDependencies)
        {

        }
    }



    //[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_IF : QRTarget
    {
        //public QRConfig AEconfigToUse { get; protected set; }

        //make variable params for the library dependence
        public QRTarget_IF(params string[] libraryDependencies) : base(libraryDependencies)
        {

        }
        public override void Init()
        {
            AEconfigToUse = new QRConfig();
            this.qRTargetType = QRTargetType.IF;

            //TargetName = $"{ProjIBelongTo.Name}_IFlib";
            TargetName = $"";
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "rosqt", "IF", "include", $"{ProjIBelongTo.Name}_i");

        }
    }

    //[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_RosLib : QRTarget_Lib
    {
        //public QRConfig AEconfigToUse { get; protected set; }

        //make variable params for the library dependence
        public QRTarget_RosLib(params string[] libraryDependencies) : base(libraryDependencies)
        {

        }
        public override void Init()
        {
            AEconfigToUse = new QRConfig();
            this.qRTargetType = QRTargetType.rosqt_library;

            TargetName = $"{ProjIBelongTo.Name}_ROSlib";
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "rosqt", "include", $"{ProjIBelongTo.Name}_rqt");

        }
    }




    public abstract class QRTarget_EXE : QRTarget
    {
        //public QRConfig AEconfigToUse { get; protected set; }
        public string MethodName { get; set; }

        //make variable params for the library dependence
        public QRTarget_EXE(params string[] libraryDependencies) : base(libraryDependencies)
        {
        }

    }





    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_RosEXE : QRTarget_EXE
    {
        //public QRConfig AEconfigToUse { get; protected set; }


        //make variable params for the library dependence
        public QRTarget_RosEXE(params string[] libraryDependencies) : base(libraryDependencies)
        {

            this.qRTargetType = QRTargetType.rosqt_exe;

        }

        public override void Init()
        {
            AEconfigToUse = new QRConfig();

            TargetName = MethodName;
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "rosqt", "include", $"{ProjIBelongTo.Name}_cp");

        }
    }



    //[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_cpLib : QRTarget_Lib
    {
        //public QRConfig AEconfigToUse { get; protected set; }



        //make variable params for the library dependence
        public QRTarget_cpLib(params string[] libraryDependencies) : base(libraryDependencies)
        {

            this.qRTargetType = QRTargetType.cpp_library;

        }

        public override void Init()
        {
            AEconfigToUse = new QRConfig();
            TargetName = $"{ProjIBelongTo.Name}_CPPlib";
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "include", $"{ProjIBelongTo.Name}_cp");
        }
    }

    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QRTarget_cpEXE : QRTarget_EXE
    {
        //public QRConfig AEconfigToUse { get; protected set; }

        //make variable params for the library dependence
        public QRTarget_cpEXE(params string[] libraryDependencies) : base(libraryDependencies)
        {
            this.qRTargetType = QRTargetType.cpp_exe;

        }

        public override void Init()
        {
            AEconfigToUse = new QRConfig();
            TargetName = MethodName;
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfProject, "include", $"{ProjIBelongTo.Name}_cp");
        }
    }








    //[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public abstract class QRTarget : System.Attribute
    {
        public QRModule ProjIBelongTo { get; set; }
        public QRTargetType qRTargetType;
        public QRTargetProjectType QRTargetProjType
        {
            get
            {
                QRTargetProjectType ret = qRTargetType == QRTargetType.rosqt_library ?
                    QRTargetProjectType.rqt :
                    qRTargetType == QRTargetType.rosqt_exe ?
                    QRTargetProjectType.rqt :
                    qRTargetType == QRTargetType.cpp_library ?
                    QRTargetProjectType.cp :
                    qRTargetType == QRTargetType.cpp_unittest ?
                    QRTargetProjectType.cp :
                    QRTargetProjectType.IF;
                return ret;

            }
        }

        public string TargetName { get; set; }
        public string TargetIncludePath { get; set; }
        public QRConfig AEconfigToUse { get; protected set; }

        public List<string> IF_Module_Dependencies { get; set; }
        public List<string> ROSQT_Module_Dependencies { get; set; }
        public List<string> CPP_Module_Dependencies { get; set; }
        public List<string> NonQR_Module_Dependencies { get; set; }

        public static List<QRTarget> AllTargets = new List<QRTarget>();


        public List<LibraryDependence> LibraryDependencies { get; set; }

        public static void Reset()
        {
            AllTargets.Clear();
        }

        public static string sscsc()
        {
            return $" ";
        }

        //get a list of all the library dependencies target names
        public List<string> LibraryDependenciesTargetNames
        {
            get
            {
                var ret = LibraryDependencies.Select(d =>
                {
                    if (d.TargetType == QRTargetType.cpp_library)
                    {
                        return d.ProjectIDependOn.Target_CPPLib.TargetName;
                    }
                    else if (d.TargetType == QRTargetType.rosqt_library)
                    {
                        return d.ProjectIDependOn.Target_ROSLib.TargetName;
                    }
                    else
                    {
                        return d.ProjectIDependOn.Target_IF.TargetName;
                    }
                    //remove the "" strings
                }).Where(d => d != "").ToList();

                return ret;
            }
        }

        public List<string> LibraryDependenciesTargetFULLNames
        {
            get
            {
                var ret = LibraryDependencies.Select(d =>
                {
                    if (d.TargetType == QRTargetType.cpp_library)
                    {
                        string targname = d.ProjectIDependOn.Target_CPPLib.TargetName;
                        string projname = d.ProjectIDependOn.Name;

                        //if the running target is the same as the this target project type and
                        //if this target's project it belongs to is the same as the running project,
                        //then dont add the namespace
                        string namespaceName =
                        (QRInitializing.RunningTarget.QRTargetProjType == QRTargetProjectType.cp &&
                        projname == QRInitializing.RunningProjectName) ?
                        "" : $"{projname}_cp::";

                        return $"{namespaceName}{targname}";
                    }
                    else if (d.TargetType == QRTargetType.rosqt_library)
                    {
                        string targname = d.ProjectIDependOn.Target_ROSLib.TargetName;
                        string projname = d.ProjectIDependOn.Name;

                        //if the running target is the same as the this target project type and
                        //if this target's project it belongs to is the same as the running project,
                        //then dont add the namespace
                        string namespaceName =
                        (QRInitializing.RunningTarget.QRTargetProjType == QRTargetProjectType.rqt &&
                        projname == QRInitializing.RunningProjectName) ?
                        "" : $"{projname}_rqt::";

                        return $"{namespaceName}{targname}";
                    }
                    else
                    {
                        return d.ProjectIDependOn.Target_IF.TargetName;
                    }
                    //remove the "" strings
                }).Where(d => d != "").ToList();

                return ret;
            }
        }


        public abstract void Init();

        public QRTarget(params string[] libraryDependencies)
        {
            IF_Module_Dependencies = new List<string>();
            ROSQT_Module_Dependencies = new List<string>();
            CPP_Module_Dependencies = new List<string>();
            NonQR_Module_Dependencies = new List<string>();
            //go through the libraryDependencies, parse out th string such that anycharacters followed by a colon is the module name
            //and any characters after the colon is the target name
            foreach (var item in libraryDependencies)
            {

                //if it doesnt contain a colon, then it is a Non-Qrcore module
                if (!item.Contains(":"))
                {
                    NonQR_Module_Dependencies.Add(item);
                    continue;
                }

                var parts = item.Split(':');
                if (parts[1] == "IF")
                {
                    IF_Module_Dependencies.Add(parts[0]);
                }
                else if (parts[1] == "rosqt_library")
                {
                    ROSQT_Module_Dependencies.Add(parts[0]);
                }
                else if (parts[1] == "cpp_library")
                {
                    CPP_Module_Dependencies.Add(parts[0]);
                }


            }

            //any modules in CPP_Module_Dependencies or ROSQT_Module_Dependencies  need to also have dependency in the interface project to that module
            List<string> CPP_Module_Dependencies_Toadd = new List<string>();
            List<string> ROSQT_Module_Dependencies_Toadd = new List<string>();
            foreach (var item in CPP_Module_Dependencies)
            {
                var parts = item.Split(':');
                CPP_Module_Dependencies_Toadd.Add(parts[0]);
            }
            foreach (var item in ROSQT_Module_Dependencies)
            {
                var parts = item.Split(':');
                ROSQT_Module_Dependencies_Toadd.Add(parts[0]);
            }
            IF_Module_Dependencies.AddRange(CPP_Module_Dependencies_Toadd);
            IF_Module_Dependencies.AddRange(ROSQT_Module_Dependencies_Toadd);



            //remove duplicates  
            CPP_Module_Dependencies = CPP_Module_Dependencies.Distinct().ToList();
            ROSQT_Module_Dependencies = ROSQT_Module_Dependencies.Distinct().ToList();
            IF_Module_Dependencies = IF_Module_Dependencies.Distinct().ToList();
            NonQR_Module_Dependencies = NonQR_Module_Dependencies.Distinct().ToList();



            AllTargets.Add(this);

            LibraryDependencies = new List<LibraryDependence>();

            foreach (var item in libraryDependencies)
            {
                if (!item.Contains(":"))
                {
                    continue;
                }

                LibraryDependencies.Add(LibraryDependence.FromString(item, QRModule.AllProjects[0]));
            }
        }
    }

}
