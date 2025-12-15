using CgenMin.MacroProcesses.QR;
using CodeGenerator.ProblemHandler;
using System.Text.RegularExpressions;

namespace CgenMin.MacroProcesses
{



    public enum FunctionArgsType
    {
        PrimitiveType,
        SurrogateAO,
        EnumType,
        UNKOWNTYPE,
        AnotherMSG //this is if the argument is of a type that is of another MSG
    }


    public interface IFunctionArgsValue
    {
        string ValueAsStr { get; }
        string TypeName { get; }
        string ARGNAME { get; }


    }

    public class FunctionArgsValue<T> : FunctionArgs<T>, IFunctionArgsValue
    {
        public FunctionArgsValue(string name, T value) : base(name, 0, false)
        {
            Value = value;
        }

        public T Value { get; }
        public string ValueAsStr => Value.ToString();
        public string TypeName => this.TypeName;
        public string ARGNAME => this.ARGNAME;
    }


    public class FunctionArgs<T> : FunctionArgsBase
    {

        public FunctionArgs(string name, int argnum = 0, bool isFromEnumArg = false) : base(typeof(T), name, argnum, isFromEnumArg)
        {
            _Type = typeof(T);
            //if the type is QREventMSG or NonQrMessage, then a problem saying that the wrong constructor is used
            if (typeof(T) == typeof(QREventMSG) || typeof(T) == typeof(QREventMSGNonQR))
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem($"FunctionArgs<{typeof(T).Name}> constructor cannot be used for QREventMSG or NonQrMessage types. Please use the appropriate constructor.");
            }
        }

        public FunctionArgs(QREventMSG TheMSG, string name) : base(typeof(T),name, 0, false)
        {
            qREventMSG = TheMSG;
            _Type = typeof(QREventMSG);
        }        
        public FunctionArgs(QREventMSGNonQR TheMSG, string name) : base(typeof(T),name, 0, false)
        {
            nonQrMessage = TheMSG;
            _Type = typeof(QREventMSGNonQR);
        } 

    }



    //give an extension to List<FunctionArgsBase> to get the ARGSNAME
    public static class FunctionArgsBaseExtension
    {

        public static string GenerateAllForEvery_Arguments(this List<FunctionArgsBase> Args, Func<FunctionArgsBase, string> functionDelegate, string delimiter)
        {

            if (Args.Count == 0)
            {
                return "";
            }


            string ret = "";
            //for the last one, do not add the delimiter
            for (int i = 0; i < Args.Count - 1; i++)
            {
                ret += functionDelegate(Args[i]) + delimiter;
            }
            ret += functionDelegate(Args[Args.Count - 1]); //add the last one
            return ret;
        }


        public static string ARG_FILL_REQUEST_DATA(FunctionArgsBase functionArg)
        {
            return functionArg.ARG_FILL_REQUEST_DATA();
        }

        public static FunctionArgsBase CheckIfAllArgNamesAreCompatibleForRosService(this List<FunctionArgsBase> Args )
        { 

            //^(?!.*__)(?!.*_$)[a-z][a-z0-9_]*$
            string pattern = @"^(?!.*__)(?!.*_$)[a-z][a-z0-9_]*$";
            Regex regex = new Regex(pattern);

            //get Args that are not typeof(QREventMSGNonQR) or typeof(QREventMSG)  
            var argsToCheck = Args.Where(a => 
            !(a.GetCSType == typeof(QREventMSGNonQR))).ToList();
            argsToCheck = argsToCheck.Where(a => 
            !(a.GetCSType == typeof(QREventMSG))).ToList();
            
            foreach (var arg in argsToCheck)
            { 
                string nameAsInService = arg.NAMEASINSERVICE(); 
                //if it is from an enum, then the name needs to all be capital letters.  
                if (arg.IsFromEnumArg)
                {
                    // Check if the name is all capital letters or underscores
                    if (!nameAsInService.All(c => char.IsUpper(c) || c == '_'))
                    {
                        return arg;
                    }
                }
               else if (!regex.IsMatch(nameAsInService))
                { 
                    return arg;
                }
            }
            return null;
        }

        public static string ARG(FunctionArgsBase functionArg)
        {
            //the return type  and the name of the argument
            return $"{functionArg.TypeName} {functionArg.ARGNAME()}";
        }

        public static string ARGS(this List<FunctionArgsBase> Args)
        {
            if (Args.Count == 0)
            {
                return "";
            }

            string ret = "";
            //foreach Args, get the type and name  and separate by comma
            //for the last one, do not add the delimiter
            for (int i = 0; i < Args.Count - 1; i++)
            {
                ret += $"{Args[i].TypeName} {Args[i].ARGNAME()},";
            }
            ret += $"{Args[Args.Count - 1].TypeName} {Args[Args.Count - 1].ARGNAME()}"; //add the last one
            return ret;
        }

        public static string ARGSNAME(this List<FunctionArgsBase> args)
        {
            string ret = "";
            //go through all the FunctionArgs and get the ARGNAME   and seperate them by a comma and space except last one
            for (int i = 0; i < args.Count; i++)
            {
                ret +=  args[i].ARGNAME();// args[i].TypeName + " " + args[i].ARGNAME();
                if (i != args.Count - 1)
                {
                    ret += ", ";
                }
            }
            return ret;
        }

        //now for NUMOFARGS
        public static string NUMOFARGS_UNDERSCORE(this List<FunctionArgsBase> args)
        {
            //for every num of Args.Count, make a string that will be _1, _2, _3 up to the number of Args.Count
            string ret = "";
            for (int i = 1; i <= args.Count; i++)
            {
                //dont put a comma for the last one
                ret += i == args.Count ? $"_{i}" : $"_{i},";
            }
            return ret;
        }
    }





    public class FunctionArgsBase
    { 

        public string Name { get; set; }
        public int Argnum { get; }
        public bool IsFromEnumArg { get; }
        public string UNKOWNTYPE { get; }

        public int MyProperty { get; set; }

        protected Type _Type;
        public Type GetCSType
        {
            get
            {
                return _Type;
            }
        }

        public QREventMSG QREventMSG_  { 
            get
            {
                return qREventMSG;
            }
        }
        public QREventMSGNonQR QREventMSGNonQR_  { 
            get
            {
                return nonQrMessage;
            }
        }
        protected QREventMSG qREventMSG = null;
        protected QREventMSGNonQR nonQrMessage = null;




        public FunctionArgsBase(Type type, string name, int argnum = 0, bool isFromEnumArg = false )
        {

            _Type = type;
            Name = name;
            Argnum = argnum;
            IsFromEnumArg = isFromEnumArg; 
            // var s = FunctionArgType;  
        }
        public FunctionArgsBase( string UNKOWNTYPE )
        {

            _Type = null;
            Name = "pointertodata";
            Argnum = 0;
            IsFromEnumArg = false;
            this.UNKOWNTYPE = UNKOWNTYPE;
            // var s = FunctionArgType;
        }

        public string TypeName
        {
            get
            {
                if (_Type == null)
                {
                    return UNKOWNTYPE + "*";
                }
                if(this.qREventMSG != null)
                {
                    return this.qREventMSG.FULLCLASSNAME;
                }
                if(this.nonQrMessage != null)
                {
                    return this.nonQrMessage.FullMsgClassName;
                }
          
                string ret = CsharpTypeToCppType(_Type.Name);
                //if this is from a surrogate AO, then Base appended to the type name
                if (this.FunctionArgType == FunctionArgsType.SurrogateAO)
                {
                    ret += "Base*";
                }
                return ret;
            }
        }

        protected FunctionArgsType _FunctionArgType;
        public FunctionArgsType FunctionArgType
        {
            get
            {
                if (this._Type == null)
                {
                    this._FunctionArgType = FunctionArgsType.UNKOWNTYPE; 
                }
                //figure out wether the type is either a primitive type, a surrogate AO or an enum type. a surrogate AO is a type that is derived from AOSurrogatePattern 
                else if (this._Type.IsPrimitive)
                {
                    this._FunctionArgType = FunctionArgsType.PrimitiveType;
                }
                else if (_Type.IsEnum)
                {
                    this._FunctionArgType = FunctionArgsType.EnumType;

                    //if this is an enum type, create a QREventMSG for it. but first check if it exists first
                    QREventMSG.EnumFactory(this._Type);

                }
                //else if (this.Type.IsSubclassOf(typeof(AOSurrogatePatternBase)))
                else if (this._Type.BaseType.Name.Contains("AOSurrogateNode"))
                {
                    this._FunctionArgType = FunctionArgsType.SurrogateAO;
                }
                return this._FunctionArgType;
            }
        }

        public static string CsharpTypeToCppType(string typestr)
        {
            string ret = typestr;

            // Integer types
            ret = ret == "Byte" ? "uint8_t" : ret;
            ret = ret == "SByte" ? "int8_t" : ret;
            ret = ret == "Int16" ? "int16_t" : ret;
            ret = ret == "Int32" ? "int32_t" : ret;
            ret = ret == "Int64" ? "int64_t" : ret;
            ret = ret == "UInt16" ? "uint16_t" : ret;
            ret = ret == "UInt32" ? "uint32_t" : ret;
            ret = ret == "UInt64" ? "uint64_t" : ret;
            
            ret = ret == "Void" ? "void" : ret;


            // Floating-point types
            ret = ret == "float" ? "float" : ret;
            ret = ret == "Single" ? "float" : ret;
            ret = ret == "Double" ? "double" : ret;
            ret = ret == "Decimal" ? "double" : ret; // C++ does not have a direct decimal type; double is commonly used instead.

            // Boolean type
            ret = ret == "Boolean" ? "bool" : ret;

            // Character type
            ret = ret == "Char" ? "char" : ret;

            // String type (assuming std::string for C++)
            ret = ret == "String" ? "std::string" : ret;

            return ret;


        }

        public static string CsharpTypeToServiceType(string typestr)
        {
            string ret = typestr;

            // Integer types
            ret = ret == "Byte" ? "uint8" : ret;
            ret = ret == "SByte" ? "int8" : ret;
            ret = ret == "Int16" ? "int16" : ret;
            ret = ret == "Int32" ? "int32" : ret;
            ret = ret == "Int64" ? "int64" : ret;
            ret = ret == "UInt16" ? "uint16" : ret;
            ret = ret == "UInt32" ? "uint32" : ret;
            ret = ret == "UInt64" ? "uint64" : ret;


            ret = ret == "Void" ? "" : ret;

            // Floating-point types
            ret = ret == "Single" ? "float32" : ret;
            ret = ret == "float" ? "float32" : ret;
            ret = ret == "Double" ? "double" : ret;
            ret = ret == "Decimal" ? "double" : ret; // C++ does not have a direct decimal type; double is commonly used instead.

            // Boolean type
            ret = ret == "Boolean" ? "bool" : ret;

            // Character type
            ret = ret == "Char" ? "char" : ret;

            // String type (assuming std::string for C++)
            ret = ret == "String" ? "string" : ret;

            return ret;
             
        }

        public static Type ServiceTypeToCsharpType(string typestr)
        {
            string ret = typestr;

            // Integer types
            ret = ret == "uint8" ? "System.Byte" : ret;
            ret = ret == "int8" ? "System.SByte" : ret;
            ret = ret == "int16" ? "System.Int16" : ret;
            ret = ret == "int32" ? "System.Int32" : ret;
            ret = ret == "int64" ? "System.Int64" : ret;
            ret = ret == "uint16" ? "System.UInt16" : ret;
            ret = ret == "uint32" ? "System.UInt32" : ret;
            ret = ret == "uint64" ? "System.UInt64" : ret;

            // Floating-point types
            ret = ret == "float32" ? "System.Single" : ret;
            ret = ret == "double" ? "System.Double" : ret;

            // Boolean type
            ret = ret == "bool" ? "System.Boolean" : ret;

            // Character type
            ret = ret == "char" ? "System.Char" : ret;

            // String type
            ret = ret == "string" ? "System.String" : ret;

            //if none of the above, then return null
                
              return Type.GetType(ret) ?? null;
        }
         
         


        public static string STR_to_NAMEASINSERVICE(bool isSurrogate, string toConvert)
        {
            string ret = isSurrogate ? "id" : toConvert;
            return ret;
        }
        
      

        public string NAMEASINSERVICE()
        {
            return STR_to_NAMEASINSERVICE(this.FunctionArgType == FunctionArgsType.SurrogateAO, Name);

        }
        public string TYPEASINSERVICE()
        {
 
 
            string typename = "";
            if (this._Type == null)
            {
                typename = this.UNKOWNTYPE;
            }
            //if this is a QREventMSGNonQR type and it is a ROS message, then return the package name and then type
            else if (this._Type == typeof(QREventMSGNonQR))
            {
                var nonqrmsg = (QREventMSGNonQR)this.QREventMSGNonQR_;
                if (nonqrmsg.IsRosMessage)
                {
                    return $"{this.QREventMSGNonQR_.FromModuleName}/{this.QREventMSGNonQR_.FileNameOfService}";
                }
                else //if this is a just a QREventMSGNonQR type, then return the class name and that module name
                {
                   
                    return $"{this.QREventMSGNonQR_.FromModuleName}/{this.QREventMSGNonQR_.FileNameOfService}";
                }
            }
            //if this is a QREventMSG type, then return the class name 
            else if (this._Type == typeof(QREventMSG))
            {
                //but if this is from a different module, then return the module name and then the class name
                if (this.QREventMSG_.FromModuleName != QRInitializing.RunningProjectName)
                {
                    return $"{this.QREventMSG_.FromModuleName}_i/{this.QREventMSG_.InstanceName}";
                }

                return this.QREventMSG_.InstanceName;
            }
            else
            {
                typename = this._Type.Name;
            }
            return STR_to_TYPEASINSERVICE(this.FunctionArgType == FunctionArgsType.SurrogateAO, typename);
        }


        public static string STR_to_TYPEASINSERVICE(bool isSurrogate, string toConvert)
        {
            string ret = isSurrogate  ? "string" : toConvert;
            ret = CsharpTypeToServiceType(ret);
            return ret;
        }


        public string ARGNAME()
        {
            //if this is a surrogate AO, then return the AOObj->Getid()
            if (this.FunctionArgType == FunctionArgsType.SurrogateAO)
            {
                return $"AOObj";
            }
            else
            {
                return NAMEASINSERVICE();
            } 
        }
        public string ARGREQUESTFILL()
        {
            //if this is a surrogate AO, then return the AOObj->Getid()
            if (this.FunctionArgType == FunctionArgsType.SurrogateAO)
            {
                return $"AOObj->Getid()";
            }
            else
            {
                return ARGNAME();
            }
        }        
        
        public string ARG_FILL_REQUEST_DATA()
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOArgFillRequestData",
                    new MacroVar() { MacroName = "NAMEASINSERVICE", VariableValue = this.NAMEASINSERVICE() },
                    new MacroVar() { MacroName = "ARGREQUESTFILL", VariableValue = this.ARGREQUESTFILL() }
                    );
            return ret;
        } 
       
    }



}
