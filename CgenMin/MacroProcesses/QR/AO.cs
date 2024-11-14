//using CodeGenerator.MacroProcesses.AESetups;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
using CodeGenerator.ProblemHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CgenMin.MacroProcesses
{


    public enum StyleOfSPB
    {
        // look at file AESPBObservor.cpp at funciton _RefreshCheckStyle() for where this implementation makes a difference. 
        EachSPBTask,  //each spb has its own task that it will use to execute its refresh
                      //TODO:ChainOfSPBsTask, // there is one "chain task" that will run all refreshs
                      //there are no tasks involved and so everything is run within the interrupt(although doesnt need to be an interrupt, can also just be from a normal tick() of a clock).
                      //Currently this doesnt look to be any different than the 
                      //ChainOfSPBsTask so maybe use this for now intead of that one. Remeber that if you do infact use this in an interrupt, it should be a VERY quick spb
        ChainOfSPBsFromInterrupt

    }
;

    public enum AOTypeEnum
    {
        AOSimple,
        AOSurrogatePattern,
        AONode,
        Event,
        LoopObject,
        SimpleFSM
    }

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
                        this.ProjIBelongTo.Name == QRInitializing.RunningProjectName) ?
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
                        this.ProjIBelongTo.Name == QRInitializing.RunningProjectName) ?
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

            //remove duplicates for CPP_Module_Dependencies
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
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfLibrary, "rosqt", "IF", "include", $"{ProjIBelongTo.Name}_i");

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
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfLibrary, "rosqt", "include", $"{ProjIBelongTo.Name}_rqt");

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
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfLibrary, "rosqt", "include", $"{ProjIBelongTo.Name}_cp");

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
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfLibrary, "include", $"{ProjIBelongTo.Name}_cp");
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
            TargetIncludePath = Path.Combine(ProjIBelongTo.DirectoryOfLibrary, "include", $"{ProjIBelongTo.Name}_cp");
        }
    }






    public abstract class AO
    {

        private static List<string> listOfAdditionalIncludes = new List<string>();
        protected string GetAdditionalIncludeInAEConfig(string fileNameWithoutTheExt)
        {
            //return only if this file has not been included yet
            var alreadyIncluded = listOfAdditionalIncludes.Where(a => a == fileNameWithoutTheExt).FirstOrDefault();

            if (alreadyIncluded == null)
            {
                listOfAdditionalIncludes.Add(fileNameWithoutTheExt);

                return $"#define AnyOtherNeededIncludes{listOfAdditionalIncludes.Count.ToString()} {fileNameWithoutTheExt} \n";
            }
            else
            {
                return "";
            }

        }

        public static bool atLeastOneEvt = false;

        public static List<AO> AllInstancesOfAO = new List<AO>();
        public AOTypeEnum AOType { get; protected set; }
        public string ClassName;
        public string InstanceName;

        public AO(string instanceName, AOTypeEnum aOType)
        {
            ClassName = GetType().Name;

            InstanceName = instanceName.Trim();
            AOType = aOType;

            if (this.ClassName != "UpdateEVT")
            {
                AllInstancesOfAO.Add(this);
            }
        }


        //public static List<T> GetAllAOOfType<T>() where T : AO
        //{
        //    if (typeof(T) == typeof(AESensor))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.Sensor).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AEFilter))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.Filter).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AESPBBase))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.SPB).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AEUtilityService))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.UtilityService).Cast<T>().ToList();
        //        return tt;
        //    }

        //    return null;
        //}


        //the part that goes into the header of the main.cpp file. stuff like callback function declarations static void clockTimer1(TimerHandle_t xTimerHandle);
        public abstract string GenerateMainHeaderSection_CP();
        public abstract string GenerateMainInitializeSection_CP();
        public abstract string GenerateMainHeaderSection_RQT();
        public abstract string GenerateMainInitializeSection_RQT();
        //set AO to clocks section
        //public abstract string GenerateMainClockSetupsSection();
        //link AOs together section
        //public abstract string GenerateMainLinkSetupsSection();

        //bottom area of main file for defining callback functions.
        //public abstract string GenerateFunctionDefinesSection();


        public static string All_GenerateMainHeaderSection_CP()
        {
            string ret = "";
            foreach (var ao in AllInstancesOfAO)
            {
                ret += ao.GenerateMainHeaderSection_CP();
            }

            return ret;
        }
        public static string All_GenerateMainInitializeSection_CP()
        {
            string ret = "";

            foreach (var ao in AllInstancesOfAO)
            {
                string rett = ao.GenerateMainInitializeSection_CP() + "\n";
                string retttrim = rett.Trim();
                ret = retttrim == "" ? ret : rett + "\n";
            }
            return ret;
        }


        public static string All_GenerateMainHeaderSection_RQT()
        {
            string ret = "";
            foreach (var ao in AllInstancesOfAO)
            {
                ret += ao.GenerateMainHeaderSection_RQT();
            }

            return ret;
        }
        public static string All_GenerateMainInitializeSection_RQT()
        {
            string ret = "";

            foreach (var ao in AllInstancesOfAO)
            {
                string rett = ao.GenerateMainInitializeSection_RQT() + "\n";
                string retttrim = rett.Trim();
                ret = retttrim == "" ? ret : ret + rett + "\n";
            }
            return ret;
        }


        //public static string All_GenerateMainClockSetupsSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateMainClockSetupsSection();
        //    }
        //    return ret;
        //}
        //public static string All_GenerateMainLinkSetupsSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateMainLinkSetupsSection();
        //    }
        //    ret += $"\n";

        //    //put any subscriptioins to any spbs here.
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        foreach (var spbSub in ao.AllSPBsISubTo)
        //        {
        //            //AE_SubscribeToSPB(encoderPosition_SPB, motorDriverSpeedControllerTDU, &motorDriverSpeedControllerTDU->CurrentPosition, 0);
        //            ret += $"AE_SubscribeToSPB({spbSub.spbToSubTo.InstanceName}, {spbSub.NameOfSubbingInstanceName}, &{spbSub.NameOfSubbingInstanceName}->{spbSub.MemberFloatNameTo}, {spbSub.filterNumToSubToFromSPB});"; ret += $"\n";
        //        }
        //    }


        //    return ret;
        //}

        //public static string All_GenerateFunctionDefinesSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateFunctionDefinesSection();
        //    }
        //    return ret;
        //}

        public static string All_GenerateAEConfigSection()
        {
            string ret = "";
            foreach (var ao in AllInstancesOfAO)
            {
                ret += ao.GenerateAEConfigSection();
            }
            return ret;
        }



        public static int numOfAOSoFarAEConfigGenerated { get { return _numOfAOSoFarAEConfigGenerated; } protected set { _numOfAOSoFarAEConfigGenerated = value; } }
        protected static int _numOfAOSoFarAEConfigGenerated = 0;

        public bool IsGeneratedConfg { get { return isGeneratedConfg; } protected set { isGeneratedConfg = value; } }
        public bool isGeneratedConfg = false;

        //the part that goes in the AEConfig. the defines and such
        protected abstract string _GenerateAEConfigSection(int numOfAOOfThisSameTypeGeneratesAlready);

        public string GenerateAEConfigSection()
        {
            //if (this.AOType == AOType.SPB)
            //{
            //    var t = this.GetAllAOOfType<AESPBBase>();

            //var tt = (IPartOfAEDefines)this;
            var allSameType = AllInstancesOfAO.Where(d => d.ClassName == this.ClassName);// && tt.GetFullTemplateArgs() == ((IPartOfAEDefines)d).GetFullTemplateArgs());
            int numAlreadyCreated = 0;

            foreach (var same in allSameType)
            {
                if (same.IsGeneratedConfg == true)
                {
                    numAlreadyCreated++;
                }
            }

            //}


            if (numAlreadyCreated == 0)
            {
                //increment numOfAOSoFarAEConfigGenerated as this is the start of a new AOcponfig generated
                //only do this for spbs, utilityServices, or AOSM
                //if (AOType == AOTypeEnum.SPB || AOType == AOTypeEnum.UtilityService)
                //{
                //    numOfAOSoFarAEConfigGenerated++;
                //}


            }



            isGeneratedConfg = true;
            return _GenerateAEConfigSection(numAlreadyCreated) + "\n";


        }
    }



    public interface IPartOfAEDefines
    {
        string GetFullTemplateType();
        string GetFullTemplateArgsValues();
    }

}
