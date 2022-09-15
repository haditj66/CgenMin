using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgenMin.MacroProcesses
{
    class MyMacroProcessDer : MacroProcess
    {
        public override void RunProcess()
        {
            string param1 = GrabFileContents("inputs/SomeContent.txt");
            string param2 = "hello";
            string param3 = "blabla";
 
            string gout = GenerateFileOut("inputs/inputtest1", 
            new MacroVar{MacroName = "ClassName", VariableValue = param1.Trim()},
            new MacroVar{MacroName = "VarName1", VariableValue = param2},
            new MacroVar{MacroName = "VarName2", VariableValue = param3},
            new MacroVar{MacroName = "VarName3", VariableValue = "fdgr"});

            WriteFileContents(gout,"outputs/blabla",".cpp");
            
            return;
        }


        Dictionary<string,string> varList = new Dictionary<string, string>(){
            {"posx","int"},
            {"posy","float"}};

        public string Section2()
        {  

            StringBuilder sb = new StringBuilder();
            foreach (var item in varList)
            {
                string gout = GenerateFileOut("inputs/SetPosxInput", 
                    new MacroVar{MacroName = "VarName", VariableValue = item.Key},
                    new MacroVar{MacroName = "VarType", VariableValue = item.Value});

                    sb.Append(gout+ "\n"); 

            }
             
            return sb.ToString();
        }

        public string SomeDynamicStuff()
        {
            return "int somestuff = 4;";
        }

    }
}
