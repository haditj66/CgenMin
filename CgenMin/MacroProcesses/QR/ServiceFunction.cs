namespace CgenMin.MacroProcesses.QR
{


   


    public enum ServiceTypeEnum
    {
        Normal,
        Surrogate
    }


    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class SurrogateServiceFunction : ServiceFunction
    {
        public SurrogateServiceFunction() : base()
        {
            ServiceType = ServiceTypeEnum.Surrogate;
            PartOfCallBackGroup_Named = "client_cb_group_";

        }

    }









    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ServiceFunction : System.Attribute
    {
        public bool IsSurrogateFunction { get{ return ServiceType == ServiceTypeEnum.Surrogate; }   }

        public ServiceTypeEnum ServiceType;

        public string PartOfCallBackGroup_Named {   get; protected set; }

        public ServiceFunction(string partOfCallBackGroup_Named = "")
        {
            
            ServiceType = ServiceTypeEnum.Normal;
            PartOfCallBackGroup_Named = partOfCallBackGroup_Named;
        }

        public string GenerateAllForEvery_Arguments(Func<FunctionArgsBase, string> functionDelegate, string delimiter)
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
         

        public string NUMOFARGS_UNDERSCORE()
        {
            return Args.NUMOFARGS_UNDERSCORE();
      
        }
          

        //just the arguments names separated by comma without return type
        public string ARGSNAME()
        {
            return Args.ARGSNAME(); 
        }

        public List<FunctionArgsBase> ResponseArgs = new List<FunctionArgsBase>();
        public List<FunctionArgsBase> Args { get; set; }

        public QREventSRV FunctionServiceEvent { get; set; }
        public string Name { get; internal set; }

        public string NonQRTopicName { get; set; } = "";

        //create factory method

        public static ServiceFunction CreateServiceFunction(string name, List<FunctionArgsBase> typeOfResponse, List<FunctionArgsBase>  args, string topicName = "")
        {
            ServiceFunction serviceFunction = new ServiceFunction();
            serviceFunction.Name = name;
            serviceFunction.ResponseArgs = typeOfResponse;
            serviceFunction.Args = args;
            serviceFunction.NonQRTopicName = topicName;

            //DONT CREATE A SERVICE EVENT AS THIS WILL CREATE AN UNWANTED SERVICE FILE
            //FunctionServiceEvent = new QREventSRV(this.FromModuleName, name, new FunctionArgsBase(method.ReturnType, ""), tt);

            return serviceFunction;
        }


        public string NAMEOFFUNCTION { get { return Name; } }
        public string ARGRETURN { get { return ResponseArgs[0].TypeName; } } 
        public string TICKET_RETURN_TYPE1(string MODULENAME) {  return ARGRETURN == "void" ? "void" : $"{MODULENAME}_i::srv::{NAMEOFFUNCTION}_Response::_result_type"; } //@MODULENAME@_i::srv::@NAMEOFFUNCTION@_Response::_result_type
        public string TICKET_RETURN_TYPE2(string MODULENAME) { return ARGRETURN == "void" ? "void" : $"{MODULENAME}_i::srv::{NAMEOFFUNCTION}_Response::_result_type"; } //@MODULENAME@_i::srv::@NAMEOFFUNCTION@::Response::_result_type
        public string TICKET_RETURN_TYPE_multi(string MODULENAME) { 
            return ARGRETURN == "void" ? "void" : $"std::shared_ptr<{MODULENAME}_i::srv::{NAMEOFFUNCTION}::Response>"; } //@MODULENAME@_i::srv::@NAMEOFFUNCTION@_Response::_result_type
         
        public string ARGS
        {
            get
            {
               return Args.ARGS(); 
            }
        }

         
        //protected static string RunForAllServiceFunctions(List<ServiceFunction> serviceFunctions, string methodToExecutestr)
        //{
        //    MethodInfo methodToExecute = typeof(ServiceFunction).GetMethod(methodToExecutestr);

        //    if (serviceFunctions == null || methodToExecute == null)
        //    {
        //        throw new ArgumentNullException("serviceFunctions or methodToExecute cannot be null");
        //    }

        //    string ret = "";
        //    foreach (var servFun in serviceFunctions)
        //    {
        //        ret += methodToExecute.Invoke(servFun, null) + "\n";
        //    }
        //    return ret;
        //}
        public static string WNFUNCTION_SERVICES<TServiceFunctionType>(List<TServiceFunctionType> serviceFunctions, string modulename) where TServiceFunctionType : ServiceFunction
        {
            string ret = "";
            foreach (var servFun in serviceFunctions)
            {
                ret += servFun.WNFUNCTION_SERVICE(modulename) + "\n";
            }
            return ret;
            //return RunForAllServiceFunctions(  serviceFunctions, "WNFUNCTION_SERVICE"); 
        }        
       
        //this is a service declaration.     rclcpp::Service<world2_i::srv::MoveObject>::SharedPtr serviceMoveObject;
        protected string WNFUNCTION_SERVICE(string modulename)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\WNFunction_Service",
                     new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = this.NAMEOFFUNCTION },
                     new MacroVar() { MacroName = "MODULE_NAME", VariableValue = modulename }
                    );
            return ret;
        }


        public static string WNFUNCTION_SERVICES_DEFINES<TServiceFunctionType>(List<TServiceFunctionType> serviceFunctions, string AONAME, bool isForSurrogateAO, string MODULENAME) where TServiceFunctionType : ServiceFunction
        {
            string ret = "";
            foreach (var servFun in serviceFunctions)
            { 
                ret += servFun.WNFUNCTION_SERVICES_DEFINE(AONAME, isForSurrogateAO, MODULENAME) + "\n";
            }
            return ret;
            //return RunForAllServiceFunctions(  serviceFunctions, "WNFUNCTION_SERVICES_DEFINE"); 
        }

       //this defines the service in the constructor.
        //serviceMoveObject =
        //ForNode->create_service<world2_i::srv::MoveObject>(
        //cppobj->Getid() + "/MoveObject",
        //std::bind(&WorldNodeAO::MoveObject, this, _1,_2), //
        //rmw_qos_profile_services_default_MINE, client_cb_group_);
        protected string WNFUNCTION_SERVICES_DEFINE(string AONAME, bool isForSurrogateAO, string MODULENAME)
        {
            string fileGen = isForSurrogateAO ? "WNFunction_Service_Define" : "WNFunction_Service_Define_NodeAO";
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut($"QR\\SurrogatePattern\\{fileGen}",
                    new MacroVar() { MacroName = "AONAME", VariableValue = AONAME },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = this.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = MODULENAME },
                    new MacroVar() { MacroName = "NUMOFARGS", VariableValue = this.NUMOFARGS_UNDERSCORE() },
                    new MacroVar() { MacroName = "COMMA_IF_ARGSNAME", VariableValue = this.NUMOFARGS_UNDERSCORE() == "" ? "" : "," },
                    new MacroVar() { MacroName = "IF_PART_OF_CALLBACK_GROUP", VariableValue = this.PartOfCallBackGroup_Named == "" ? "" : ",rmw_qos_profile_services_default_MINE," },//
                    new MacroVar() { MacroName = "CALLBACK_GROUP", VariableValue = this.PartOfCallBackGroup_Named }
                    );
            return ret;
        }

 


        public static string WNFUNCTIONS_IMPLS<TServiceFunctionType>(List<TServiceFunctionType> serviceFunctions, string MODULENAME) where TServiceFunctionType : ServiceFunction
        {
            string ret = "";
            foreach (var servFun in serviceFunctions)
            {
                ret += servFun.WNFUNCTIONS_IMPL(MODULENAME) + "\n";
            }
            return ret;
            //return RunForAllServiceFunctions(  serviceFunctions, "WNFUNCTION_SERVICES_DEFINE"); 
        }



        protected string WNFUNCTIONS_IMPL(string MODULENAME)
        {
            //go through every serviceFunction's arguments and create a string that will be the arguments
            string ARGS_SURROGATE_GET = "";
            string ARGSFILL = "";
            int count = 0;
            foreach (var surArg in this.Args)
            {
                if (surArg.FunctionArgType == FunctionArgsType.SurrogateAO)
                {
                    ARGS_SURROGATE_GET += $"{surArg.TypeName.Replace("*", "")}* g{count.ToString()} = {surArg.TypeName.Replace("*", "")}::GetObjectFromPool(request->id);\n";
                    ARGSFILL += $"g{count.ToString()}";
                }
                else
                {
                    ARGSFILL += $"request->{surArg.Name}";
                }

                count++;
                if (count < this.Args.Count)
                {
                    ARGSFILL += ",";
                }
            }

            string COMMENTDOIFVOID = (this.ARGRETURN == "void" || this.IsSurrogateFunction == false) ? "//" : "";
            string COMMENTDOIFVOID2 = (this.IsSurrogateFunction == false ) ? "//" : "";

            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\WNFunction_impl",
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = MODULENAME },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = this.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "ARGS_SURROGATE_GET", VariableValue = ARGS_SURROGATE_GET },
                    new MacroVar() { MacroName = "COMMENTDOIFVOID", VariableValue = COMMENTDOIFVOID },
                    new MacroVar() { MacroName = "COMMENTDOIFVOID2", VariableValue = COMMENTDOIFVOID2 },
                    new MacroVar() { MacroName = "ARGSFILL", VariableValue = ARGSFILL }
                    );
            return ret;
        }

    }






    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class SurrogateData : System.Attribute
    {
        //FunctionArgs functionArg;

        public SurrogateData(bool isPublicSet, string defaultValue = "")
        {
            IsPublicSet = isPublicSet;
            DefaultValue = defaultValue;

        }
        public Type _Type;
        public string TypeOfData
        {
            get
            {
                string ret = FunctionArgsBase.CsharpTypeToCppType(_Type.Name);
                return ret;
            }
        }

        public string NAMEASINSERVICE(bool isSurrogate)
        {
            return FunctionArgsBase.STR_to_NAMEASINSERVICE(isSurrogate, NameOfData);

        }
        public string TYPEASINSERVICE(bool isSurrogate)
        {
            return FunctionArgsBase.STR_to_NAMEASINSERVICE(isSurrogate, this._Type.Name);
        }

        public string NameOfData { get; set; }
        public string DefaultValue { get; set; }
        public bool IsPublicSet { get; }
    }


    public abstract class AOSurrogatePatternBase : AOWritableConstructible
    {

        public AOSurrogatePatternBase(string AOName, bool isSurrogate) : base(QRInitializing.RunningProjectName, AOName )
        {
        }


    }



}
