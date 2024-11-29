using CgenMin.MacroProcesses;
using CgenMin.MacroProcesses.QR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.MacroProcesses.AESetups
{

    public class QRConfig
    {


        public QRConfig()
        {

        }




        public string GetFileDefineContents()
        {


            Type type = typeof(QRConfig);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            var allprops = type.GetProperties();
            var allfields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var allfieldsint = allfields.Where(a => a.FieldType == typeof(int));
            var allfieldsbool = allfields.Where(a => a.FieldType == typeof(bool));
            var allfieldsstr = allfields.Where(a => a.FieldType == typeof(string));


            string ret = "";

            foreach (FieldInfo f in allfieldsint)
            {
                ret += $"#define {f.Name} {f.GetValue(this).ToString()}"; ret += "\n";
            }
            foreach (FieldInfo f in allfieldsbool)
            {
                if ((bool)f.GetValue(this))
                {
                    ret += $"#define {f.Name}"; ret += "\n";
                }

            }
            foreach (FieldInfo f in allfieldsstr)
            {
                ret += $"#define {f.Name} {f.GetValue(this).ToString()}"; ret += "\n";
            }
            return ret;
        }

        public void GenerateFile(string RunningProjectDir, QRInitializing aein, string AODefines, string AEHalDefines, QRSetting qRSetting)
        {

            RUNNING_TARGET = QRInitializing.RunningTarget.TargetName;

            //these are objects that are considered AOs
            //--utilities (not the services! a utility is considered ONE AO despite how many services it has)
            //--spb
            //--aesensor
            //--AELoopObject
            //--simpleFSM 
            NUMOFACTIVEOBJECTS = 0;
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.AOSimple);
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.AOSurrogatePattern);
            //NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.LoopObject);
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.SimpleFSM);
            NUMOFACTIVEOBJECTS++;

            HIGHEST_NUM_OF_EVT_INSTANCES = QREvent.NumOfSRVCreatedSoFar + 1;



            AODefines = AODefines == "" ? "\n" : AODefines;
            AEHalDefines = AEHalDefines == "" ? "\n" : AEHalDefines;


            string pathfiletype = QRInitializing.RunningTarget.qRTargetType == QRTargetType.rosqt_exe ?
                "rqt" :
                "cp";

            string aeConfigOUT = aein.GenerateFileOut($"QR\\Config_{pathfiletype}",
                new MacroVar() { MacroName = "AODefines", VariableValue = AODefines },
                new MacroVar() { MacroName = "AESettingsDefines", VariableValue = GetFileDefineContents() },
                new MacroVar() { MacroName = "AEHalDefines", VariableValue = AEHalDefines },
                new MacroVar() { MacroName = "QR_SETTING", VariableValue = qRSetting.GenerateFile() },
                new MacroVar() { MacroName = "PROJECT_NAME", VariableValue = QRInitializing.RunningProjectName }
                );

            Console.WriteLine($"generating cp Config.h ");
            //check for whick project type this is for
            //QRInitializing.RunningTarget.qRTargetType == QRTargetType.rosqt_exe ? 
            string path = QRInitializing.RunningTarget.qRTargetType == QRTargetType.rosqt_exe ?
                Path.Combine(RunningProjectDir, "rosqt", "include", $"{QRInitializing.RunningProjectName}_rqt", "Config.hpp") :
                Path.Combine(RunningProjectDir, "include", $"{QRInitializing.RunningProjectName}_cp", "Config.hpp");


            aein.WriteFileContentsToFullPath(aeConfigOUT, path, "hpp", true);

        }





        //AE configs
        private int AOPRIORITYLOWEST = 5;
        private int AOPRIORITYMEDIUM = 10;
        private int AOPRIORITYHIGHEST = 29;
        private int NUMOFACTIVEOBJECTS;
        private int HIGHEST_NUM_OF_EVT_INSTANCES;

        private string RUNNING_TARGET;

    }
}


