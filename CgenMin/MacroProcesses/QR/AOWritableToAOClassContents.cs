using CgenMin.MacroProcesses.QR;
using CodeGenerator.ProblemHandler;
using System;

namespace CgenMin.MacroProcesses
{


    public enum CppTypeEnum
    {
        float_t,
        uint32_t,
        uint16_t,
        uint8_t,
        int32_t,
        int16_t,
        int8_t,
        bool_t
    }

    public class CppFunctionArg
    {
        public CppFunctionArg(CppTypeEnum type, string name, bool isPublicSet = true, string defaultvalue = "")
        {
            Type = type;
            Name = name;
            _TypeStr = null;
            IsPublicSet = isPublicSet;
            Defaultvalue = defaultvalue;
        }

        public CppFunctionArg(string type, string name, bool isPublicSet = true, string defaultvalue = "")
        {
            _TypeStr = type;
            Name = name;
            IsPublicSet = isPublicSet;
            Defaultvalue = defaultvalue;
        }

        public string TypeStr { get { return CppTypeEnumToStr(Type); } }
        private string _TypeStr;
        public CppTypeEnum Type { get; }
        public string Name { get; }
        public bool IsPublicSet { get; }
        public string Defaultvalue { get; }

        public string GetPropertyFunctions()
        {
            string ret = "";

            //backing variable
            ret += $"protected: {TypeStr} {Name}; ";
            ret += "\n";

            //the Get
            ret += $"public: {TypeStr}  Get{Name}()   const " + "{" + $"return {Name}; " + "}";
            ret += "\n";
            //if set is protected, return nothing
            string publicORprotected = IsPublicSet == true ? "public" : "protected";
            if (IsPublicSet == true)
            {
                ret += $"{publicORprotected}:  void Set{Name}({TypeStr}  _{Name})" + "{" + $"{Name} = _{Name}; " + "}";
                ret += "\n";
            }

            return ret;
        }


        public string GetPropertyAllocating()
        {
            string ret = "";

            ret += $"{Name} =  _{Name};";
            return ret;
        }

        private string CppTypeEnumToStr(CppTypeEnum cppTypeEnum)
        {
            if (_TypeStr != null)
            {
                return _TypeStr;
            }

            switch (cppTypeEnum)
            {
                case CppTypeEnum.float_t:
                    return "float";
                    break;
                case CppTypeEnum.uint32_t:
                    return "uint32_t";
                    break;
                case CppTypeEnum.uint16_t:
                    return "uint16_t";
                    break;
                case CppTypeEnum.uint8_t:
                    return "uint8_t";
                    break;
                case CppTypeEnum.int32_t:
                    return "int32_t";
                    break;
                case CppTypeEnum.int16_t:
                    return "int16_t";
                    break;
                case CppTypeEnum.int8_t:
                    return "int8_t";
                    break;
                case CppTypeEnum.bool_t:
                    return "bool";
                    break;
                default:
                    ProblemHandle problem = new ProblemHandle();
                    problem.ThereisAProblem("you selected an invalid cpp type");
                    return "";
                    break;
            }
        }


    }


    public class CppFunctionArgWithValue<TargTyp1> : CppFunctionArg
    {
        public TargTyp1 ValueOfArg1;

        public string GetValueOfArgStr()
        {
            if (getType<TargTyp1>() == CppTypeEnum.float_t)
            {
                //FloatInTemplateWorkAround<15161, 20> 
                int numOFDecimalsToTheLeft = 0;
                float asdfe = (float)(object)(TargTyp1)ValueOfArg1;
                while (asdfe % 1 != 0)
                {
                    numOFDecimalsToTheLeft++;
                    asdfe *= 10;
                }

                return $"FloatInTemplateWorkAround<{asdfe.ToString("0." + new string('#', 339))}, {numOFDecimalsToTheLeft}>";
            }
            else
            {
                return ValueOfArg1.ToString();
            }
        }

        public CppFunctionArgWithValue(string name, TargTyp1 value, bool isPublicSet = true)
            : base(getType<TargTyp1>(), name, isPublicSet, "")
        {
            ValueOfArg1 = value;
        }


        public static CppTypeEnum getType<Targ>()
        {
            CppTypeEnum ret =
                typeof(Targ) == typeof(float) ?
                CppTypeEnum.float_t :
                typeof(Targ) == typeof(bool) ?
                CppTypeEnum.bool_t :
                typeof(Targ) == typeof(byte) ?
                CppTypeEnum.int8_t :
                typeof(Targ) == typeof(Int16) ?
                CppTypeEnum.int16_t :
                typeof(Targ) == typeof(Int32) ?
                CppTypeEnum.int32_t :
                typeof(Targ) == typeof(byte) ?
                CppTypeEnum.uint8_t :
                typeof(Targ) == typeof(UInt16) ?
                CppTypeEnum.uint16_t :
                typeof(Targ) == typeof(UInt32) ?
                CppTypeEnum.uint32_t

                : CppTypeEnum.int32_t;

            return ret;

        }

    }



    public class CppFunctionArgsWithValue<TargTyp1, TargTyp2, TargTyp3> : CppFunctionArgs
    {
        public CppFunctionArgsWithValue(
            string name1, TargTyp1 value1,
            string name2, TargTyp2 value2,
            string name3, TargTyp3 value3,
            bool isPublicSet1 = true, bool isPublicSet2 = true, bool isPublicSet3 = true)
            : base(new CppFunctionArgWithValue<TargTyp1>(name1, value1, isPublicSet1),
                   new CppFunctionArgWithValue<TargTyp2>(name2, value2, isPublicSet2),
                   new CppFunctionArgWithValue<TargTyp3>(name3, value3, isPublicSet3))
        {
        }

    }
    public class CppFunctionArgsWithValue<TargTyp1, TargTyp2> : CppFunctionArgs
    {
        public CppFunctionArgsWithValue(
            string name1, TargTyp1 value1,
            string name2, TargTyp2 value2,
            bool isPublicSet1 = true, bool isPublicSet2 = true)
            : base(new CppFunctionArgWithValue<TargTyp1>(name1, value1, isPublicSet1),
                   new CppFunctionArgWithValue<TargTyp2>(name2, value2, isPublicSet2))
        {
        }

    }

    public class CppFunctionArgsWithValue<TargTyp1> : CppFunctionArgs
    {
        public CppFunctionArgsWithValue(string name, TargTyp1 value, bool isPublicSet = true)
            : base(new CppFunctionArgWithValue<TargTyp1>(name, value, isPublicSet))
        {
        }

    }


    public class CppFunctionArgs
    {

        public CppFunctionArg[] TheCppFunctionArgs { get; }
        public CppFunctionArgs(params CppFunctionArg[] cppFunctionArgs)
        {
            TheCppFunctionArgs = cppFunctionArgs;
        }


        private string _GetContructorArgs(bool withTypes)
        {
            string ret = "";
            if (TheCppFunctionArgs.Length > 0)
            {
                string typeName = withTypes ? $"{TheCppFunctionArgs[0].TypeStr}" : "";
                string defualtStr = TheCppFunctionArgs[0].Defaultvalue == "" ? "" : $" = {TheCppFunctionArgs[0].Defaultvalue}";
                ret += $"{typeName} _{TheCppFunctionArgs[0].Name} {defualtStr}";
            }
            for (int i = 1; i < TheCppFunctionArgs.Length; i++)
            {
                string typeName = withTypes ? $"{TheCppFunctionArgs[i].TypeStr}" : "";
                string defualtStr = TheCppFunctionArgs[i].Defaultvalue == "" ? "" : $" = {TheCppFunctionArgs[i].Defaultvalue}";
                ret += $", {typeName} _{TheCppFunctionArgs[i].Name} {defualtStr}";
            }

            return ret;
        }


        public string GetContructorArgs()
        {

            return _GetContructorArgs(true);
        }


        public string GetContructorArgsWithoutTypes()
        {
            return _GetContructorArgs(false);
        }


        public string GetAllPropertyAllocating()
        {
            string ret = "";
            foreach (var item in TheCppFunctionArgs)
            {
                ret += item.GetPropertyAllocating();
                ret += "\n";
            }

            return ret;
        }
        public string GetAllPropertyFunctions()
        {
            string ret = "";
            foreach (var item in TheCppFunctionArgs)
            {
                ret += item.GetPropertyFunctions();
                ret += "\n";
            }

            return ret;
        }
    }





    public abstract class AOWritableConstructible : AOWritableToAOClassContents
    {
        public CppFunctionArgs TheCppFunctionArgs { get; protected set; }


        public AOWritableConstructible(string fromLibrary, string instanceNameOfTDU,  CppFunctionArgs cppFunctionArgs = null)
            : base(fromLibrary, instanceNameOfTDU )
        {
            TheCppFunctionArgs = cppFunctionArgs;
        }



        public string GetInitializationFunction()
        {

            string ret = TheCppFunctionArgs == null ? "" :
                QRInitializing.TheMacro2Session.GenerateFileOut("AERTOS\\UserInitialize",
                new MacroVar() { MacroName = "ClassName", VariableValue = ClassName },
                new MacroVar() { MacroName = "CTORArgs", VariableValue = TheCppFunctionArgs.GetContructorArgs() },
                new MacroVar() { MacroName = "AllProperties", VariableValue = TheCppFunctionArgs.GetAllPropertyFunctions() },
                new MacroVar() { MacroName = "CTORArgsDefining", VariableValue = TheCppFunctionArgs.GetAllPropertyAllocating() }
                );

            return ret;
        }

    }

}
