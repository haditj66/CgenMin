using CgenMin.MacroProcesses;
using CgenMin.MacroProcesses.QR;
using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace CodeGenerator.MacroProcesses.AESetups
{

    public class TargetSettingsFile
    {
        public TargetSettingsFile(string nameOFSettings, TargetSettings theSetting)
        {
            NameOFSettings = nameOFSettings;
            TheSetting = theSetting;
        }

        public string NameOFSettings { get; }
        public TargetSettings TheSetting { get; }


        public string ARGS_CEREAL_FILE()
        {
            string ret = "";
            // Go through all the FunctionArgs and get the ARGNAME and ARGTYPE and separate them by a comma and space except the last one
            for (int i = 0; i < TheSetting.TheFunctionArgs.Count; i++)
            {
                var funcArg = TheSetting.TheFunctionArgs[i];
                if (i == 0)
                {
                    ret += $"<{funcArg.ARGNAME()}>{TheSetting.ArgValueStr(funcArg)}</{funcArg.ARGNAME()}>";
                }
                else
                {
                    ret += $"\n\t\t<{funcArg.ARGNAME()}>{TheSetting.ArgValueStr(funcArg)}</{funcArg.ARGNAME()}>";
                }
            }
            return ret;
        }


        public void GenerateFile()
        {
            string CerealFileContents = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\QRSettingsFile",
        new MacroVar() { MacroName = "ARGS_CEREAL_FILE", VariableValue = ARGS_CEREAL_FILE() }
        );

            //write ret to file at Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "config", "AllAOSettings", $"{NameOFSettings}.xml")
            File.WriteAllText(Path.Combine(QRInitializing.RunningProjectDir, "rosqt", "config", "AllAOSettings", $"{NameOFSettings}.cereal"), CerealFileContents);
              

        }
    }



    public abstract class TargetSettings
    {
        public TargetSettings( )  
        {
            TheFunctionArgs = new List<FunctionArgsBase>();

            var derivedType = this.GetType();

            // Get all properties of the derived class
            var allProps = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            var allFields = derivedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            //allFields.AddRange(derivedType.decl);
            foreach (var prop in allProps)
            {
                TheFunctionArgs.Add(new FunctionArgsBase(prop.PropertyType, prop.Name));
            }
            foreach (var prop in allFields)
            {
                TheFunctionArgs.Add(new FunctionArgsBase(prop.FieldType, prop.Name));
            }

        }


        public string ArgValueStr(FunctionArgsBase forArg)
        {
            var derivedType = this.GetType();

            // Get all properties of the derived class
            var allProps = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            var allFields = derivedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

            foreach (var prop in allProps)
            {
                if (prop.Name == forArg.ARGNAME())
                {
                    return prop.GetValue(this).ToString();
                }
            }
            foreach (var prop in allFields)
            {
                if (prop.Name == forArg.ARGNAME())
                {
                    return prop.GetValue(this).ToString();
                }
            }

            return "";
        }

        public List<FunctionArgsBase> TheFunctionArgs { get; }

        public string Args()
        {
            string ret = "";
            //go through all the FunctionArgs and get the ARGNAME and ARGTYPE and seperate them by a comma and space except last one
            for (int i = 0; i < TheFunctionArgs.Count; i++)
            {
                ret += TheFunctionArgs[i].TypeName + " " + TheFunctionArgs[i].ARGNAME() + ";";
            }
            return ret;
        }
        public string ARGS_CEREAL()
        {
            string ret = "";
            //go through all the FunctionArgs and get the ARGNAME and ARGTYPE and seperate them by a comma and space except last one
            for (int i = 0; i < TheFunctionArgs.Count; i++)
            {
                ret += $"CEREAL_NVP({TheFunctionArgs[i].ARGNAME()})";
                if (i != TheFunctionArgs.Count - 1)
                {
                    ret += ", ";
                }
            }
            return ret;
        }

    }




    public class QRSetting
    {
        //constructor takes any number of FunctionArgs
        public QRSetting( )
        { 
            //use reflection to get all classes with the attribute QRSettingsAttr. but only get classes that are nested in a class named world2.then create an instance of that class with the atribute
            var world2Type = QRInitializing.RunningTarget.ProjIBelongTo.GetType();// Assembly.GetExecutingAssembly().GetTypes()
                

            if (world2Type != null)
            {
                var nestedTypes = world2Type.GetNestedTypes();
                

                foreach (var nestedType in nestedTypes)
                {
                    //get all nested classes derived from TargetSettings 
                    if (nestedType.IsSubclassOf(typeof(TargetSettings)))
                    {
                        // Create an instance of the class with the attribute
                        targetSettings = Activator.CreateInstance(nestedType) as TargetSettings;
                        //instance.fefef();
                    } 
                }
            }

        }

        TargetSettings targetSettings;

         
        public string GenerateFile()
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\QRSettings",
        new MacroVar() { MacroName = "ARGS", VariableValue = targetSettings.Args() },
        new MacroVar() { MacroName = "ARGS_CEREAL", VariableValue = targetSettings.ARGS_CEREAL() } 
        );

            return ret;

        }
    }
}


