//using CodeGenerator.MacroProcesses.AESetups;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
using CodeGenerator.ProblemHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CgenMin.MacroProcesses
{
    public abstract class QRProject
    {
        //the project that is currently being worked on
        private static QRProject _CurrentWorkingProject;
        public static QRProject CurrentWorkingProject
        {
            get { return _CurrentWorkingProject; }
            set
            {
                _CurrentWorkingProject = value;

                _CurrentWorkingProject.InitCPPLibrary();
            }
        }

        public string Name { get { return this.GetType().Name; } }

        public static List<QRProject> AllProjects = new List<QRProject>();

        public QRTarget_cpLib Target_CPPLib { get; set; }
        public QRTarget_RosLib Target_ROSLib { get; set; }
        public QRTarget_IF Target_IF { get; set; }



        public static string BaseAEDir = CodeGenerator.Program.QRBaseDir;//"C:/AERTOSProjects";

        public static readonly List<string> ListOfBoardTargets = new List<string>() {
        "mingw",
        "STM32F411RE"
        };
         

        public string DirectoryOfProject
        {
            get
            {
                string _DirectoryOfLibrary = _GetDirectoryOfLibrary();
                //_DirectoryOfLibrary = QRInitializing.GetRunningDirectoryFromProjectName(Name);

                _DirectoryOfLibrary =
                    Path.IsPathRooted(_DirectoryOfLibrary) == false ? _DirectoryOfLibrary = Path.Combine(BaseAEDir, _DirectoryOfLibrary)
                    : _DirectoryOfLibrary;

                _DirectoryOfLibrary = _DirectoryOfLibrary.Replace("\\", @"/");

                return _DirectoryOfLibrary;
            }
        }



        public static void Reset()
        {
            AllProjects.Clear();
            _CurrentWorkingProject = null; 
            AllProjects.Clear(); 
        }

        public QRProject()
        {
            AllProjects.Add(this);

            //_LibrariesIDependOn_ForCPPLib = new List<QRTarget_Lib>();
            Target_CPPLib = InitCPPLibrary();
            Target_ROSLib = InitROSLibrary();
            Target_IF = InitIFLibrary();

            Target_CPPLib.ProjIBelongTo = this;
            Target_ROSLib.ProjIBelongTo = this;
            Target_IF.ProjIBelongTo = this;

            Target_CPPLib.Init();
            Target_ROSLib.Init();
            Target_IF.Init();


        }

        public void Init()
        {
            //initialize all exe targets
            var blaToInit = ListOfTargets_cpEXE;
            foreach (var item in ListOfTargets_cpEXE)
            {
                item.ProjIBelongTo = this;
                item.Init();
            }   
            var bla2ToInit = ListOfTargets_rosEXE;
            foreach (var item in ListOfTargets_rosEXE)
            {
                item.ProjIBelongTo = this;
                item.Init();
            }


            //_LibrariesIDependOn_ForCPPLib = new List<QRTarget_Lib>();
            try
            {
               // _LibrariesIDependOn_ForCPPLib = _GetLibrariesIDependOn_ForCPPLib();

                //foreach (var item in this.GetAllLibrariesIDependOnFlattened)
                //{
                //    item.Init();
                //}
            }
            catch (System.StackOverflowException e)
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem("there must have been a circular dependency on your libraries");
                throw;
            }



        }
        protected abstract QRTarget_cpLib InitCPPLibrary();
        protected abstract QRTarget_RosLib InitROSLibrary();
        protected abstract QRTarget_IF InitIFLibrary();
        public abstract List<TargetSettingsFile> GetTargetSetting();


        //public List<string> ListOfTests { get { return _GetListOfTests(); } }
        //private List<string> _ListOfTests = null;
        //protected List<string> _GetListOfTests()
        //{
        //    if (_ListOfTests == null)
        //    {
        //        _ListOfTests = new List<string>();

        //        var type = typeof(QRProject);
        //        var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
        //      .SelectMany(s => s.GetTypes())
        //      .Where(p => type.IsAssignableFrom(p))
        //      .Where(p => p.Name == Name)
        //      .FirstOrDefault();

        //        var tt = typeProcessToRun.GetMethods();

        //        var methodsOfAEEXETest = tt
        //      .Where(m => m.GetCustomAttributes(typeof(QRTarget_cpEXE), false).Length > 0)
        //      .ToArray();

        //        foreach (var item in methodsOfAEEXETest)
        //        {
        //            _ListOfTests.Add(item.Name);
        //        }

        //    }
        //    return _ListOfTests;
        //}


        bool _IsCPPListInitialized = false;
        bool _IsROSListInitialized = false;

        private List<QRTarget_cpEXE> qRTarget_CpEXEs = new List<QRTarget_cpEXE>();
        private List<QRTarget_RosEXE> qRTarget_RosEXEs = new List<QRTarget_RosEXE>();

        public List<QRTarget_cpEXE> ListOfTargets_cpEXE { get 
            {
                if (!_IsCPPListInitialized)
                {
                    qRTarget_CpEXEs = _GetListCPEXE<QRTarget_cpEXE>();

                    _IsCPPListInitialized = true;
                }    
                return qRTarget_CpEXEs;
            
            } 
        }

        public List<QRTarget_RosEXE> ListOfTargets_rosEXE { get
            {
                if (!_IsROSListInitialized)
                {
                    qRTarget_RosEXEs = _GetListCPEXE<QRTarget_RosEXE>();

                    _IsROSListInitialized = true;
                }
                return qRTarget_RosEXEs;

            }
        }

        public List<QRTarget_EXE> ListOfTargets_AllEXE
        { get { 
                     //get _GetListCPEXE<QRTarget_cpEXE>() and _GetListCPEXE<QRTarget_RosEXE>() and combine them as a type of QRTarget_EXE
                     var cpEXETargets = ListOfTargets_cpEXE.Cast<QRTarget_EXE>();
                var rosEXETargets = ListOfTargets_rosEXE.Cast<QRTarget_EXE>();
                return cpEXETargets.Concat(rosEXETargets).ToList();

            }
        }

     
        protected List<T> _GetListCPEXE<T>() where T : QRTarget_EXE
        {
            return __GetListOfCPEXE<T>();
        }

        public List<T> __GetListOfCPEXE<T>() where T : QRTarget_EXE
        {
            List<T> __ListOfTestsEXE = new List<T>();

            var type = typeof(QRProject);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.Name == Name)
                .FirstOrDefault();

            var tt = typeProcessToRun.GetMethods();

            var methodsOfAEEXETest = tt
                .Where(m => m.GetCustomAttributes(typeof(T), false).Length > 0)
                .ToArray();

            foreach (var item in methodsOfAEEXETest)
            {
                var attr = (T)item.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                attr.MethodName = item.Name;
                __ListOfTestsEXE.Add(attr);
            }

            return __ListOfTestsEXE;
        }

         


        protected abstract List<string> _GetAnyAdditionalIncludeDirs();
        private List<string> _AnyAdditionalIncludeDirs = null;

        protected abstract List<string> _GetAnyAdditionalSRCDirs();
        private List<string> _AnyAdditionalSrcDirs = null;


        private List<string> GetAnyAdditionalIncludeOrSrcDirs(bool ForInc)
        {
            List<string> listToReturn = new List<string>();
            bool isnullList = true;
            if (ForInc)
            {
                isnullList = _AnyAdditionalIncludeDirs == null;
            }
            else
            {
                isnullList = _AnyAdditionalSrcDirs == null;
            }

            if (isnullList)
            {

                if (ForInc)
                {
                    _AnyAdditionalIncludeDirs = new List<string>();

                    _AnyAdditionalIncludeDirs = _GetAnyAdditionalIncludeDirs();
                    listToReturn = _AnyAdditionalIncludeDirs;
                    isnullList = _AnyAdditionalIncludeDirs == null;
                }
                else
                {
                    _AnyAdditionalSrcDirs = new List<string>();

                    _AnyAdditionalSrcDirs = _GetAnyAdditionalSRCDirs();
                    listToReturn = _AnyAdditionalSrcDirs;
                    isnullList = _AnyAdditionalSrcDirs == null;
                }

                if (isnullList == false)
                {
                    List<int> relativeDirs = new List<int>();
                    for (int i = 0; i < listToReturn.Count; i++)
                    {
                        //check if the directory is absolute or relative
                        if (Path.IsPathRooted(listToReturn[i]) == false)
                        {
                            relativeDirs.Add(i);
                        }
                    }

                    foreach (var item in relativeDirs)
                    {
                        listToReturn[item] = Path.Combine(this.DirectoryOfProject, listToReturn[item]);
                    }
                }

            }

            for (int i = 0; i < listToReturn.Count; i++)
            {
                listToReturn[i] = listToReturn[i].Replace("\\", "/");
            }

            return listToReturn;
        }


        public List<string> GetAnyAdditionalSRCDirs()
        {
            return GetAnyAdditionalIncludeOrSrcDirs(false);
        }
        public List<string> GetAnyAdditionalIncludeDirs()
        {
            //if (_AnyAdditionalIncludeDirs == null)
            //{
            //    _AnyAdditionalIncludeDirs = new List<string>();

            //    _AnyAdditionalIncludeDirs = _GetAnyAdditionalIncludeDirs();
            //    if (_AnyAdditionalIncludeDirs != null)
            //    {
            //        List<int> relativeDirs = new List<int>();
            //        for (int i = 0; i < _AnyAdditionalIncludeDirs.Count; i++)
            //        {
            //            //check if the directory is absolute or relative
            //            if (Path.IsPathRooted(_AnyAdditionalIncludeDirs[i]) == false)
            //            {
            //                relativeDirs.Add(i);
            //            }
            //        }

            //        foreach (var item in relativeDirs)
            //        {
            //            _AnyAdditionalIncludeDirs[item] = Path.Combine(this.DirectoryOfLibrary, _AnyAdditionalIncludeDirs[item]);
            //        }
            //    }

            //}

            //for (int i = 0; i < _AnyAdditionalIncludeDirs.Count; i++)
            //{
            //    _AnyAdditionalIncludeDirs[i] = _AnyAdditionalIncludeDirs[i].Replace("\\", "/");
            //}

            //return _AnyAdditionalIncludeDirs;
            return GetAnyAdditionalIncludeOrSrcDirs(true);
        }




        public void GenerateAllTestForModule( )
        {
            var type = typeof(QRProject);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
          .Where(p => type.IsAssignableFrom(p))
          .Where(p => p.Name == Name)
          .FirstOrDefault();

            List<MethodInfo> methodsToRuncpp = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
           .Where(p => p.IsAssignableFrom(typeProcessToRun))
          .SelectMany(t => t.GetMethods())
          .Where(m => m.GetCustomAttributes(typeof(QRTarget_cpEXE), false).Length > 0)
          .ToList();

            var methodsToRun = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(s => s.GetTypes())
               .Where(p => p.IsAssignableFrom(typeProcessToRun))
              .SelectMany(t => t.GetMethods())
              .Where(m => m.GetCustomAttributes(typeof(QRTarget_RosEXE), false).Length > 0)
              .ToList();

            methodsToRun.AddRange(methodsToRuncpp);

            foreach (var m in methodsToRun)
            {
                m.Invoke(this, null);
            }
             
        }

        public QRConfig GenerateTestOfName(string testName)
        {
            var type = typeof(QRProject);
            var typeProcessToRun = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
          .Where(p => type.IsAssignableFrom(p))
          .Where(p => p.Name == Name)
          .FirstOrDefault();

            var methodsToRun = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
           .Where(p => p.IsAssignableFrom(typeProcessToRun))
          .SelectMany(t => t.GetMethods())
          .Where(m => m.GetCustomAttributes(typeof(QRTarget_cpEXE), false).Length > 0)
          .Where(m => m.Name == testName)
          .FirstOrDefault();

            //if (methodsToRun == null) try again but with QRTarget_RosEXE
            if (methodsToRun == null)
            {
                methodsToRun = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(s => s.GetTypes())
               .Where(p => p.IsAssignableFrom(typeProcessToRun))
              .SelectMany(t => t.GetMethods())
              .Where(m => m.GetCustomAttributes(typeof(QRTarget_RosEXE), false).Length > 0)
              .Where(m => m.Name == testName)
              .FirstOrDefault();
            }

            var proj = this.ListOfTargets_AllEXE.Where(a => a.MethodName == testName).FirstOrDefault();

            //var attr = (QRTarget_cpEXE)methodsToRun.GetCustomAttributes(typeof(QRTarget_cpEXE), false).FirstOrDefault();


            methodsToRun.Invoke(this, null);
            return proj.AEconfigToUse;
        }

        protected abstract string _GetDirectoryOfLibrary(); 
        protected List<QREvent> GetEventsInLibrary { get; set; }
        //protected List<QREvent> GetEventsInLibrary(); 
        //protected abstract List<AEHal> _GetPeripheralsInLibrary();

        //protected abstract List<QRTarget_Lib> _GetLibrariesIDependOn_ForCPPLib();// where T : AELibrary;


    }
}
