//using System.Collections.Generic;
//using System.Linq;
//using CodeGenerator.CMD_Handler;

//namespace CodeGenerator.ProblemHandler
//{
//    public class  LibrariesForCheckedOutGit : ICleanUp
//    {
//        protected CMDHandler Cmd { get; set; }

//        protected List<Library> LibrariesCheckedOutSoFar { get; set; } 

//        public LibrariesForCheckedOutGit(CMDHandler cmd)
//        {
//            LibrariesCheckedOutSoFar = new List<Library>();
//               Cmd = cmd;
//        }

//        public void AddLibraryCheckedOutSoFar(Library libraryCheckedout)
//        {
//            //dont add libraries that already have the same path of another library already added             
//            if (!LibrariesCheckedOutSoFar.Any(l => l.settings.PATHOfProject == libraryCheckedout.settings.PATHOfProject))
//            {
//                LibrariesCheckedOutSoFar.Add(libraryCheckedout);
//            } 
//        }


//        public void UncheckoutLibraryCheckedOutSoFar(Library libraryCheckedout)
//        {
//            if (LibrariesCheckedOutSoFar.FirstOrDefault(l=> 
//                    l.IsTopLevel == libraryCheckedout.IsTopLevel &&
//                    l.settings.PATHOfProject == libraryCheckedout.settings.PATHOfProject &&
//                    l.config.ClassName == libraryCheckedout.config.ClassName &&
//                    l.config.ConfTypePrefix == libraryCheckedout.config.ConfTypePrefix &&
//                    l.config.Major == libraryCheckedout.config.Major 
//                    ) != null)
//            {
//                UncheckoutLibrary(libraryCheckedout);
//                LibrariesCheckedOutSoFar.Remove(libraryCheckedout);
//            } 
            
//        }

//        private void UncheckoutLibrary(Library libraryToUncheckout)
//        {
//            Cmd.SetWorkingDirectory(libraryToUncheckout.settings.PATHOfProject);
//            Cmd.ExecuteCommand("git checkout -"); //go back to the commit I was on earlier
//            Cmd.ExecuteCommand("git stash pop"); //pop the stash back in
//        }

//        public void CleanUp()
//        { 
//            foreach (var library in LibrariesCheckedOutSoFar)
//            {
//                UncheckoutLibrary(library); 
//            }
//        }
//    }
//}