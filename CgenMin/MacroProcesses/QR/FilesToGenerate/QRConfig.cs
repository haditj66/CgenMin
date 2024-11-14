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


        public QRConfig(    )
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
            return ret;
        }

        public void GenerateFile(string RunningProjectDir, QRInitializing aein, string AODefines, string AEHalDefines)
        {
             


            //these are objects that are considered AOs
            //--utilities (not the services! a utility is considered ONE AO despite how many services it has)
            //--spb
            //--aesensor
            //--AELoopObject
            //--simpleFSM 
            NUMOFACTIVEOBJECTS = 0;
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.AOSimple);
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.AOSurrogatePattern); 
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.LoopObject);
            NUMOFACTIVEOBJECTS += AO.AllInstancesOfAO.Count(a => a.AOType == AOTypeEnum.SimpleFSM);
            NUMOFACTIVEOBJECTS++;

            HIGHEST_NUM_OF_EVT_INSTANCES = QREvent.NumOfSRVCreatedSoFar + 1;



            AODefines = AODefines == "" ? "\n" : AODefines;
            AEHalDefines = AEHalDefines == "" ? "\n" : AEHalDefines; 

            string aeConfigOUT = aein.GenerateFileOut("QR\\Config_cp",
                new MacroVar() { MacroName = "AODefines", VariableValue = AODefines },
                new MacroVar() { MacroName = "AESettingsDefines", VariableValue = GetFileDefineContents() },
                new MacroVar() { MacroName = "AEHalDefines", VariableValue = AEHalDefines },
                new MacroVar() { MacroName = "PROJECT_NAME", VariableValue = QRInitializing.RunningProjectName }
                );

            Console.WriteLine($"generating cp Config.h ");
            aein.WriteFileContentsToFullPath(aeConfigOUT, Path.Combine(RunningProjectDir, "include",$"{QRInitializing.RunningProjectName}_cp", "Config.h"), "h", true);

        }


        
         

        //AE configs
        private int AOPRIORITYLOWEST = 5;
        private int AOPRIORITYMEDIUM = 10;
        private int AOPRIORITYHIGHEST = 29;
        private int NUMOFACTIVEOBJECTS;
        private int HIGHEST_NUM_OF_EVT_INSTANCES;



        private int MaxNumOfAELoops;
         

        private int configAE_USE_TDUs_AsService;
        private int configAE_USE_U_AsService;
        private int configAE_USE_DDSM_AsService;
    }
}


 