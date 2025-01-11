using CgenMin.MacroProcesses;
using CgenMin.MacroProcesses.QR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CgenMin.MacroProcesses.QR
{
    public class QRTargetCmake
    {
        public string PathToTargetFile_Targets { get;  }
        public string PathToTargetFile_DepNames { get;   }

        public QRTarget QRTarget_lib { get; protected set; }

        public QRTargetCmake(string projectBaseDir, bool isForCPP)
        {
            IsForCPP = isForCPP;

            PathToTargetFile_Targets = IsForCPP ?
                Path.Combine(projectBaseDir, "config", $"Targets.cmake") :
                Path.Combine(projectBaseDir, "rosqt", "config", $"Targets.cmake");
            PathToTargetFile_DepNames = IsForCPP ?
                Path.Combine(projectBaseDir, "config", $"DepNames.cmake") :
                Path.Combine(projectBaseDir, "rosqt", "config", $"DepNames.cmake");

        }

        public string GetSelectedProjName()
        {
            //get the contents of file at  Path.Combine(projectBaseDir, "config", $"Targets.cmake")
            string allcont = File.ReadAllText(PathToTargetFile_Targets);
            //if allcont contains a line that has "set(EXE_TARGET_SELECTED @EXE_TARGET_SELECTED@)" , get the value in the @EXE_TARGET_SELECTED@ . use regex
            string exeTargetSelected = "";


            //string exeOutputSelected =
            return Regex.Match(allcont, @"set\(EXE_TARGET_SELECTED\s*\s*(?<ArgReqContents>.*)\s*\s*\)").Groups["ArgReqContents"].Value;

        }

        public bool IsForCPP { get; }

        public void GenerateFile(QRInitializing aein, QRModule project, string EXE_TARGET_SELECTED = "null")
        {
            string exeOutputSelected = GetSelectedProjName();
            exeOutputSelected = exeOutputSelected == "" ? "null" : exeOutputSelected;

            EXE_TARGET_SELECTED = EXE_TARGET_SELECTED == "null" ? exeOutputSelected : EXE_TARGET_SELECTED;



            QRTarget_lib = IsForCPP ? project.Target_CPPLib : project.Target_ROSLib;
            string whichProjType = IsForCPP ? "cp" : "rqt";
            //if (IsForCPP)
            //{



            //=======================================================================================================
            //project LIB library target links
            //=======================================================================================================

            //get all target links that the library for this project depends on 
            var dependsStr = QRTarget_lib.LibraryDependenciesTargetFULLNames;
                //remove the target name that are ""
                dependsStr = dependsStr.Where(d => d != "").ToList();

                string LIBRARY_LINKS = "";


                foreach (var dep in dependsStr)
                {
                    LIBRARY_LINKS += aein.GenerateFileOut($"QR\\Targets_{whichProjType}1_targetlink",
                     new MacroVar() { MacroName = "PROJECTNAME", VariableValue = project.Name },
                     new MacroVar() { MacroName = "NAMEOFTARGET_TOLINKTO", VariableValue = dep },
                     new MacroVar() { MacroName = "NAMEOFTARGET", VariableValue = QRTarget_lib.TargetName }
                     );

                    LIBRARY_LINKS += "\n";
                }




            //=======================================================================================================
            //DepNames.cmake file
            //=======================================================================================================


            //write the contents to the file at DepNames.cmake. This file will be called to write modules to the QR_Find_List_Of_Ros_Packages
            string modules_depends_cp = "set(MODULE_DEPENDS_CP " + string.Join(";", QRTarget_lib.CPP_Module_Dependencies) + ")";
                string modules_depends_rqt = "set(MODULE_DEPENDS_RQT " + string.Join(";", QRTarget_lib.ROSQT_Module_Dependencies) + ")";
                List<string> allDependsIfStr = new List<string>(); 
            allDependsIfStr.AddRange(QRTarget_lib.IF_Module_Dependencies );
            //allDependsIfStr.AddRange(QRTarget_lib.NonQR_Module_Dependencies);
            string modules_depends_if = "set(MODULE_DEPENDS_IF " + string.Join(";", allDependsIfStr) + ")";
            string modules_depends_Nonqr = "set(MODULE_DEPENDS_NONQR " + string.Join(";", QRTarget_lib.NonQR_Module_Dependencies) + ")";
            //write out to file at PathToTargetFile_DepNames
            string modules_depends_str = modules_depends_cp + "\n" + modules_depends_rqt + "\n" + modules_depends_if + "\n" + modules_depends_Nonqr;
                File.WriteAllText(PathToTargetFile_DepNames, modules_depends_str);











            //=======================================================================================================
            //project exe targets 
            //=======================================================================================================


            //get all exe targets QREXETest that are currently in projAlreadyExists 
            IEnumerable<QRTarget_EXE> targetsExe = IsForCPP ?
            project.ListOfTargets_cpEXE
                .Where(d => d.qRTargetType == QRTargetType.cpp_exe)  :
             project.ListOfTargets_rosEXE
                .Where(d => d.qRTargetType == QRTargetType.rosqt_exe);
                string TargetsEXE = "";
                foreach (var item in targetsExe)
                {
                var rtrt = QRTarget.sscsc();
                    //get the targets that it wants to link to
                    dependsStr = item.LibraryDependenciesTargetFULLNames;
                    string TargetLinks = "";
                    foreach (var dep in dependsStr)
                    {
                        TargetLinks += aein.GenerateFileOut($"QR\\Targets_{whichProjType}1_targetlink",
                         new MacroVar() { MacroName = "PROJECTNAME", VariableValue = project.Name },
                         new MacroVar() { MacroName = "NAMEOFTARGET_TOLINKTO", VariableValue = dep },
                         new MacroVar() { MacroName = "NAMEOFTARGET", VariableValue = item.TargetName }
                         );

                        TargetLinks += "\n";
                    }

                    TargetsEXE += aein.GenerateFileOut($"QR\\Targets_{whichProjType}1",
                new MacroVar() { MacroName = "NAMEOFTARGET", VariableValue = item.MethodName },
                new MacroVar() { MacroName = "TARGETSLINK", VariableValue = TargetLinks }
                );

                    TargetsEXE += "\n";
                }



                //go through that list and create a string that 
                aein.WriteFileContents_FromCGENMMFile_ToFullPath(
                    $"QR\\Targets_{whichProjType}",
                    Path.Combine(PathToTargetFile_Targets),
                    true, false,
                     new MacroVar() { MacroName = "EXE_TARGET_SELECTED", VariableValue = EXE_TARGET_SELECTED },
                     new MacroVar() { MacroName = "TargetsEXE", VariableValue = TargetsEXE },
                     new MacroVar() { MacroName = "LIBRARY_LINKS", VariableValue = LIBRARY_LINKS },
                     new MacroVar() { MacroName = "TargetsUnitTest", VariableValue = "" }
                     );


                return;

            //}
            //else
            //{

            //}

        }




    }
}


