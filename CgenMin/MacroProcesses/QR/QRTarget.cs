//using CodeGenerator.MacroProcesses.AESetups;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
using System.Collections.Generic;

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
        IF
    }


    public class LibraryDependence
    {
        public QRProject ProjectIDependOn { get; set; }
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
                                      .FirstOrDefault(t => t.IsSubclassOf(typeof(QRProject)) && t.Name == projectName);

            if (projectType == null)
            {
                throw new InvalidOperationException($"No derived class of AEProject found with the name {projectName}");
            }

            // Only create the instance of the project if it is not already created
            var projectInstance = QRProject.AllProjects
                                           .FirstOrDefault(a => a.GetType() == projectType)
                                           ?? (QRProject)Activator.CreateInstance(projectType);

            //var projectInstance = (QRProject)Activator.CreateInstance(projectType);

            return new LibraryDependence
            {
                ProjectIDependOn = projectInstance,
                TargetType = targetType
            };
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
        public QRProject ProjIBelongTo { get; set; }
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
                    QRTargetProjectType.cp  :
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

        public static List<QRTarget> AllTargets = new List<QRTarget>();
     

        public List<LibraryDependence> LibraryDependencies { get; set; }

        public static void Reset()
        {
            AllTargets.Clear(); 
        }

        public static string sscsc()
        {
            return $"fuck off!!";
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
            //go through the libraryDependencies, parse out th string such that anycharacters followed by a colon is the module name
            //and any characters after the colon is the target name
            foreach (var item in libraryDependencies)
            {
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

            //any modules in CPP_Module_Dependencies or ROSQT_Module_Dependencies  need to also have dependency int the interface project to that module
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



            AllTargets.Add(this);

            LibraryDependencies = new List<LibraryDependence>();

            foreach (var item in libraryDependencies)
            {
                LibraryDependencies.Add(LibraryDependence.FromString(item, QRProject.AllProjects[0]));
            }
        }
    }

}
