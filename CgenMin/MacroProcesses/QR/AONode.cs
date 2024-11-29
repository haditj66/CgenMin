using CodeGenerator.ProblemHandler;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace CgenMin.MacroProcesses.QR
{

    public abstract class AONodeBase : AOWritableConstructible
    {
        protected string MODULENAME;
        protected string AONAME;



        protected AONodeBase(string instanceNameOfTDU  ) : base("", instanceNameOfTDU )
        {
            //get the namespace where this comes from
            var t = this.GetType();
            var ns = t.Namespace; 
            

            //get all types of QRProject that are in the assembly of this namespace 
            var types = t.Assembly.GetTypes().Where(t => (t.Namespace == ns && t.IsSubclassOf(typeof(QRProject))) ).ToList();

            //if there are more than 2 types of QRproject, then give a problem as there should only be one in one namespace
            if (types.Count > 1)
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem($"There are more than one type of QRProject in the namespace {ns}. There should only be one type of QRProject in one namespace");
            }
            this.FromModuleName = types[0].Name;
            this.MODULENAME = types[0].Name;

        }

    }

    public abstract class AONode<TDerivedType> : AONodeBase, IGeneratedInMain
    {

        private static bool isInited = false;
        protected Type derivedType = typeof(TDerivedType);

        public static List<SurrogateData> SurrogateDatas { get; protected set; }

        protected virtual AOTypeEnum GetAOType()
        {
            return AOTypeEnum.AOSimple;
        }


        public static List<ServiceFunction> ServiceFunctions { get; private set; }
        public static List<SurrogateServiceFunction> SurrogateServiceFunctions { get; private set; }

        protected AONode(string instanceName) : base(instanceName)// AOTypeEnum.AOSurrogatePattern)
        {
            AOType = GetAOType();
            _Init();
        } 


        public abstract List<ROSTimer> SetAllTimers();
        public abstract List<ROSPublisher> SetAllPublishers();
        public abstract List<ROSSubscriber> SetAllSubscribers();


        public static List<ROSTimer> _ROSTimers = new List<ROSTimer>();
        public static List<ROSTimer> ROSTimers { get { return _ROSTimers; } }
        public void AddROSTimer(ROSTimer rOSTimer)
        {
            rOSTimer.AOIBelongTo = this;
            rOSTimer.IsForSurrogate = rOSTimer.NameOfTimer == "AOInitForSurrogatesTimer";
            _ROSTimers.Add(rOSTimer);
        }
        public static List<ROSPublisher> _ROSPublishers = new List<ROSPublisher>();
        public static List<ROSPublisher> ROSPublishers { get { return _ROSPublishers; } }
        public void AddROSPublisher(ROSPublisher rOSpub)
        {
            rOSpub.SetAOIBelongTo(this) ;
            _ROSPublishers.Add(rOSpub);

            //resolve all publishers. this is done to replace any dummy publishers needed to have been created from subscribners referencing publishers not created yet.
            ROSPublisher.ResolveAnyDummyPublishers(ref _ROSPublishers);
        }
        public static List<ROSSubscriber> _ROSSubscribers = new List<ROSSubscriber>();
        public static List<ROSSubscriber> ROSSubscribers { get { return _ROSSubscribers; } }
        public void AddROSSubscriber(ROSSubscriber rOSsub)
        {
            rOSsub.SetAOIBelongTo(this);
            _ROSSubscribers.Add(rOSsub);
        }


        protected List<TServiceFunctionType> GetAllServiceFunctions<TServiceFunctionType>() where TServiceFunctionType : ServiceFunction
        {
            List<TServiceFunctionType> ret = new List<TServiceFunctionType>();

            //===============
            //getting all the functions marked as ServiceFunction
            //=============== 
            //get all the methods in the derived class
            var methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute.GetType() == typeof(TServiceFunctionType))
                    {

                        //grab the surrogate function attribute instance that is on that functino 
                        var surrogateFunctionInstance = method.GetCustomAttributes(typeof(TServiceFunctionType), false)[0] as TServiceFunctionType;


                        //if this is a surrogate function, but the Node is not a surrogate node, then give a problem
                        if (((surrogateFunctionInstance.IsSurrogateFunction) && (this.AOType != AOTypeEnum.AOSurrogatePattern)))
                        {
                            ProblemHandle problemHandle = new ProblemHandle();
                            problemHandle.ThereisAProblem($"The function {method.Name} is marked as a surrogate function, but the AO {this.AONAME} you put this on is not a SurrogatePattern AO");
                        }

                        //fill in all properties of the surrogate function
                        surrogateFunctionInstance.Args = new List<FunctionArgsBase>();
                        surrogateFunctionInstance.Name = method.Name;


                        //fill in all properties of the surrogate function
                        surrogateFunctionInstance.TypeOFResponse = new FunctionArgsBase(method.ReturnType, method.ReturnType.Name);


                        surrogateFunctionInstance.Args = method.GetParameters().Select(p =>
                        {
                            //if parameter is of type AOSurrogatePattern
                            return new FunctionArgsBase(p.ParameterType, p.Name, p.Position);


                        }
                        ).ToList();




                        //create a service event for the function
                        List<FunctionArgsBase> tt = method.GetParameters().Select(p => new FunctionArgsBase(p.ParameterType, p.Name)).ToList();
                        //List<string> ttt = method.GetParameters().Select(p => p.ParameterType.Name).ToList();
                        ////alternate tt and ttt into a single list
                        //tt = ttt.SelectMany((x, i) => new List<string> { x, tt[i] }).ToList();
                        surrogateFunctionInstance.FunctionServiceEvent = new QREventSRV(this.FromModuleName, method.Name, new FunctionArgsBase(method.ReturnType, ""), tt);

                        //add this to the list of surrogate functions
                        ret.Add(surrogateFunctionInstance);
                    } 

                }
            }

            return ret;
        } 

        private void _Init()
        { 
                this.AONAME = this.ClassName; 
            


                //if the instance name is the same as the class name, then give a problem
                if (this.InstanceName == this.ClassName )
                {
                    ProblemHandle problemHandle = new ProblemHandle();
                    problemHandle.ThereisAProblem($" The instance name {this.InstanceName} is the same as the class name {this.ClassName}. This is not allowed");
                }
                if (this.InstanceName == this.MODULENAME)
                {
                    ProblemHandle problemHandle = new ProblemHandle();
                    problemHandle.ThereisAProblem($" The instance name {this.InstanceName} is the same as the MODULENAME name {this.MODULENAME}. This is not allowed");
                }
            if (!isInited)
            {
                 
                foreach (var timer in SetAllTimers())
                {
                    this.AddROSTimer(timer);
                    timer.AOIBelongTo = this;
                }

                foreach (var pub in SetAllPublishers())
                {
                    this.AddROSPublisher(pub);
                    pub.SetAOIBelongTo(this);
                }

                foreach (var sub in SetAllSubscribers())
                {
                    this.AddROSSubscriber(sub);
                    sub.SetAOIBelongTo(this);
                }


                //make sure there are no more dummies at all as publishers. dummies indicated publishers that are trying to be subscribed to but are never
                //created. in any project.
                if (_ROSPublishers.Where(d=> d.isdummy).ToList().Count > 0)
                {
                    var ttt = _ROSPublishers.Where(d => d.isdummy).FirstOrDefault();
                    ProblemHandle problemHandle = new ProblemHandle();
                    problemHandle.ThereisAProblem($"The publisher {ttt.IdName} was never created but there is a subscriber that is trying to subscribe tho that  topic. ");
                     
                }
                


                //Get all the SurrogateDatas. These are the properties that are marked with SurrogateData
                SurrogateDatas = new List<SurrogateData>();


                ServiceFunctions = new List<ServiceFunction>();
                SurrogateServiceFunctions = new List<SurrogateServiceFunction>();

                ServiceFunctions = GetAllServiceFunctions<ServiceFunction>();
                SurrogateServiceFunctions = GetAllServiceFunctions<SurrogateServiceFunction>();


                isInited = true;
            }
        }
         
         

        public override string GetFullTemplateArgs()
        {
            return "";
        }

        public override string GetFullTemplateArgsValues()
        {
            return "";
        }

        public override string GetFullTemplateType()
        {
            return "";
        }

        protected override string _GenerateAEConfigSection(int numOfAOOfThisSameTypeGeneratesAlready)
        {
            return "";
        }

        protected override List<RelativeDirPathWrite> _WriteTheContentedToFiles()
        {  
            return _WriteTheContentedToFiles_NodeBASE();
        }




        protected string GenerateAllForEvery_SurrogateFunction(Func<ServiceFunction, string> functionDelegate)
        {
            string ret = "";
            foreach (var item in SurrogateServiceFunctions)
            {
                ret += functionDelegate(item) + "\n";
            }
            return ret;
        }
        protected string GenerateAllForEvery_ServiceFunction(Func<ServiceFunction, string> functionDelegate)
        {
            string ret = "";
            foreach (var item in ServiceFunctions)
            {
                ret += functionDelegate(item) + "\n";
            }
            return ret;
        }

        protected string AOFUNCTION(ServiceFunction surrogateFunction)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunction",
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "ARGS", VariableValue = surrogateFunction.ARGS },
                    new MacroVar() { MacroName = "ARGRETURN", VariableValue = surrogateFunction.ARGRETURN });
            return ret;
        }
        protected string AOFUNCTION_TICKET(ServiceFunction surrogateFunction)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunctionTicket",
                    new MacroVar() { MacroName = "TICKET_RETURN_TYPE1", VariableValue = surrogateFunction.TICKET_RETURN_TYPE1(this.MODULENAME) },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION }
                    );
            return ret;
        }
        protected string AOFUNCTION_CLIENT(ServiceFunction surrogateFunction)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunctionClient",
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION });
            return ret;
        }
        protected string AOFUNCTION_CLIENT_DECLARE(ServiceFunction surrogateFunction)
        {
            return $"rclcpp::Client<{this.MODULENAME}_i::srv::{surrogateFunction.NAMEOFFUNCTION}>::SharedPtr client{surrogateFunction.NAMEOFFUNCTION};";
        }

        protected string AOFUNCTION_IMP(ServiceFunction surrogateFunction)
        {
            string isOverride = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? "override" : "";

            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunction_Imp",
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "ARGRETURN", VariableValue = surrogateFunction.ARGRETURN },
                    new MacroVar() { MacroName = "TICKET_RETURN_TYPE1", VariableValue = surrogateFunction.TICKET_RETURN_TYPE1(this.MODULENAME) },
                    new MacroVar() { MacroName = "TICKET_RETURN_TYPE2", VariableValue = surrogateFunction.TICKET_RETURN_TYPE2(this.MODULENAME) },
                    new MacroVar() { MacroName = "ARGS", VariableValue = surrogateFunction.Args.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG, ",") },//  .GenerateAllForEvery_Arguments(surrogateFunction.ARG, ",") },
                    new MacroVar() { MacroName = "ARG_FILL_REQUEST_DATAS", VariableValue = surrogateFunction.Args.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG_FILL_REQUEST_DATA, "\n") },// surrogateFunction.GenerateAllForEvery_Arguments(surrogateFunction.ARG_FILL_REQUEST_DATA, "\n") },
                    new MacroVar() { MacroName = "ARGSNAME", VariableValue = surrogateFunction.ARGSNAME() },
                    new MacroVar() { MacroName = "ISOVERRIDE", VariableValue = isOverride }
                    );
            return ret;
        }



        protected string GenerateAllForEvery_SurrogateData(Func<SurrogateData, string> functionDelegate)
        {
            string ret = "";
            foreach (var item in SurrogateDatas)
            {
                ret += functionDelegate(item) + "\n";
            }
            return ret;
        }


        protected string PROPERTYIMPL(SurrogateData surrogateData)
        {
            //if this is a public set, then add the set
            string ret = $"{surrogateData.TypeOfData} Get{surrogateData.NameOfData}() const" + "{{" + $"return this->data.{surrogateData.NameOfData};" + "}}";
            string access = surrogateData.IsPublicSet ? "public:" : "protected:";
            ret += "\n";

            if (surrogateData.IsPublicSet == true)
            {
               // ret += $"{access} void Set{surrogateData.NameOfData}({surrogateData.TypeOfData} value)" + "{" + $"this->data.{surrogateData.NameOfData} = value;" + "}";
            }

            ret += "\npublic:";
            return ret;
        }

        bool isDATASYNC_HEADERCalledOnce = false;
        protected string DATASYNC_HEADER(SurrogateData surrogateData)
        {
            if (isDATASYNC_HEADERCalledOnce == false)
            {
                //#include "qr_core/msg/void_int32_changed.hpp"
                //#include "qr_core/srv/sync_int32.hpp"
                string typedata = surrogateData.TYPEASINSERVICE(false);
                string ret =
                    $"#include \"qr_core/msg/void_{typedata}_changed.hpp\"\n" +
                    $"#include \"qr_core/srv/sync_{typedata}.hpp\"";
                ret = ret.ToLower();
                
                isDATASYNC_HEADERCalledOnce = true;
                return ret;
            }
            
            return "";
        }        
        protected string DATASYNC_DECL(SurrogateData surrogateData)
        {
            if (surrogateData.IsPublicSet == true)
            { 
            string typedata = surrogateData.TYPEASINSERVICE(false);

            //rclcpp::Client<qr_core::srv::SyncInt64>::SharedPtr clientsetposx;
            //    DataSyncing<int64_t,
            //   qr_core::srv::SyncInt64,
            //   qr_core::msg::VoidInt64Changed>* posx_datasync;
            //    TicketFuture<std::shared_future<rclcpp::Client<qr_core::srv::SyncInt64>::SharedResponse>, void>* TicketFor_Setposx;
            string ret = $"DataSyncing<{surrogateData.TypeOfData},\n" +
                $"qr_core::srv::Sync{typedata},\n" +
                $"qr_core::msg::Void{typedata}Changed>* {surrogateData.NameOfData}_datasync;\n" +
                $"TicketFuture<std::shared_future<rclcpp::Client<qr_core::srv::Sync{typedata}>::SharedResponse>, void>* TicketFor_Set{surrogateData.NameOfData};";

            ret += $"rclcpp::Client<qr_core::srv::Sync{typedata}>::SharedPtr clientset{surrogateData.NameOfData};";
            return ret;

            }
            return "";
        }

        protected string DATASYNC_DEFINE(SurrogateData surrogateData)
        {
            if (surrogateData.IsPublicSet == true)
            {
                string typedata = surrogateData.TYPEASINSERVICE(false);

                //clientsetposx = TheDataAccessManagerNode->create_client<qr_core::srv::SyncInt64>(id + "/setposx");
                //TicketFor_Setposx = nullptr;
                //posx_datasync = new DataSyncing<int64_t,
                //                               qr_core::srv::SyncInt64,
                //                               qr_core::msg::VoidInt64Changed>(id, "posx", &(this->data.posx), SurrogateDatasyncCallbackGroup);
                string ret = $"clientset{surrogateData.NameOfData} = TheDataAccessManagerNode->create_client<qr_core::srv::Sync{typedata}>(id + \"/set{surrogateData.NameOfData}\");\n";
                ret += $"TicketFor_Set{surrogateData.NameOfData} = nullptr;\n";
                ret += $"{surrogateData.NameOfData}_datasync = new DataSyncing<{surrogateData.TypeOfData},\n" +
                    $"qr_core::srv::Sync{typedata},\n" +
                    $"qr_core::msg::Void{typedata}Changed>(id, \"{surrogateData.NameOfData}\", &(this->data.{surrogateData.NameOfData}), SurrogateDatasyncCallbackGroup);";
                return ret;
            }
            return "";

        }

        protected string DATASYNC_IMP(SurrogateData surrogateData)
        {
            string Set = "";
            if (surrogateData.IsPublicSet)
            {
                 Set = QRInitializing.TheMacro2Session.GenerateFileOut(
    $"QR\\SurrogatePattern\\WorldSurrogate_DataSyncImpSet",
     new MacroVar() { MacroName = "NAME", VariableValue = surrogateData.NameOfData },
                 new MacroVar() { MacroName = "TYPEOFDATA", VariableValue = surrogateData.TypeOfData },
                 new MacroVar() { MacroName = "TYPEOFDATASERV", VariableValue = surrogateData.TYPEASINSERVICE(false) }
     );
            } 
           
            string Get = QRInitializing.TheMacro2Session.GenerateFileOut(
$"QR\\SurrogatePattern\\WorldSurrogate_DataSyncImpGet",
new MacroVar() { MacroName = "NAME", VariableValue = surrogateData.NameOfData } 
);
            string ret = Set + "\n" + Get;

            return ret;
        }
          
         


        protected List<RelativeDirPathWrite> _WriteTheContentedToFiles_NodeBASE()
        {
            var ret = new List<RelativeDirPathWrite>();
            //dont do any of the bottom if the AO is not from the running project
            if (this.MODULENAME == QRInitializing.RunningProjectName)
            {



                //****************************************************************************************************
                //WorldSurrogate.h

                Func<Func<ServiceFunction, string>, string> GenFuncToUse = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                GenerateAllForEvery_SurrogateFunction :
                GenerateAllForEvery_ServiceFunction;

                string WorldSurrogate_Init = QRInitializing.TheMacro2Session.GenerateFileOut(
    $"QR\\SurrogatePattern\\WorldSurrogate_Init",
                new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }

    );

                string WorldSurrogate_UpdateCallback = QRInitializing.TheMacro2Session.GenerateFileOut(
    $"QR\\SurrogatePattern\\WorldSurrogate_UpdateCallback",
    new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
    new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }
    );

                WorldSurrogate_Init = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                    WorldSurrogate_Init :
                    "";
                WorldSurrogate_UpdateCallback = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                    WorldSurrogate_UpdateCallback :
                    "";

                string PROPERTYIMPLS = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                     GenerateAllForEvery_SurrogateData(PROPERTYIMPL) :
                    "";

                string DATASYNC_HEADERS = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                   GenerateAllForEvery_SurrogateData(DATASYNC_HEADER) :
                  "";                
                
                string DATASYNC_DECLS = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                   GenerateAllForEvery_SurrogateData(DATASYNC_DECL) :
                  "";

                string DATASYNC_DEFINES = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                   GenerateAllForEvery_SurrogateData(DATASYNC_DEFINE) :
                  "";

                string DATASYNC_IMPS = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ?
                   GenerateAllForEvery_SurrogateData(DATASYNC_IMP) :
                  "";

                string IS_INHERIT1 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $"#include \"{MODULENAME}_cp/{AONAME}Base.h\" " : "";
                string IS_INHERIT2 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $": public  {AONAME}Base " : "";
                string IS_INHERIT3 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $"friend void {AONAME}Initialized_Callback(const {MODULENAME}_i::msg::{AONAME}Data msg);" : "";
                string IS_INHERIT4 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $": {AONAME}Base(id)" : "";
                string IS_INHERIT5 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $"_isReal = false;" : "";
                string IS_INHERIT6 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $"" : "//";
                string IS_INHERIT7 = this.GetAOType() == AOTypeEnum.AOSurrogatePattern ? $"" : "#include \"QR_Core.h\"";


                string WorldSurrogate = QRInitializing.TheMacro2Session.GenerateFileOut(
         $"QR\\SurrogatePattern\\WorldSurrogate",
          new MacroVar() { MacroName = "INTERFACE_HEADERS", VariableValue = QREvent.INTERFACE_HEADERS() },
                      new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                      new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },

                      new MacroVar() { MacroName = "IS_INHERIT1", VariableValue = IS_INHERIT1 },
                      new MacroVar() { MacroName = "IS_INHERIT2", VariableValue = IS_INHERIT2 },
                      new MacroVar() { MacroName = "IS_INHERIT3", VariableValue = IS_INHERIT3 },
                      new MacroVar() { MacroName = "IS_INHERIT4", VariableValue = IS_INHERIT4 },
                      new MacroVar() { MacroName = "IS_INHERIT5", VariableValue = IS_INHERIT5 },
                      new MacroVar() { MacroName = "IS_INHERIT6", VariableValue = IS_INHERIT6 },
                      new MacroVar() { MacroName = "IS_INHERIT7", VariableValue = IS_INHERIT7 },

                      new MacroVar() { MacroName = "AOFUNCTIONS", VariableValue = GenFuncToUse(AOFUNCTION) },
                      new MacroVar() { MacroName = "AOFUNCTION_TICKETS", VariableValue = GenFuncToUse(AOFUNCTION_TICKET) },
                      new MacroVar() { MacroName = "AOFUNCTION_CLIENTS", VariableValue = GenFuncToUse(AOFUNCTION_CLIENT) },
                    new MacroVar() { MacroName = "AOFUNCTION_CLIENT_DECLARES", VariableValue = GenFuncToUse(AOFUNCTION_CLIENT_DECLARE) },
                    new MacroVar() { MacroName = "AOFUNCTION_IMPS", VariableValue = GenFuncToUse(AOFUNCTION_IMP) },

                    new MacroVar() { MacroName = "SURROGATE_INIT", VariableValue = WorldSurrogate_Init },
                    new MacroVar() { MacroName = "UPDATE_CALLBACK", VariableValue = WorldSurrogate_UpdateCallback },

                    new MacroVar() { MacroName = "DATASYNC_HEADER", VariableValue = DATASYNC_HEADERS },
                    new MacroVar() { MacroName = "DATASYNC_DECL", VariableValue = DATASYNC_DECLS },
                    new MacroVar() { MacroName = "DATASYNC_DEFINE", VariableValue = DATASYNC_DEFINES },
                    new MacroVar() { MacroName = "DATASYNC_IMPS", VariableValue = DATASYNC_IMPS }

          );

                //if this is not a surrogate and does not have any service functions, then dont write this file
                if (this.GetAOType() == AOTypeEnum.AOSurrogatePattern || ServiceFunctions.Count > 0)
                {
                    ret.Add(new RelativeDirPathWrite($"{this.ClassName}Surrogate", "h",
                        Path.Combine("rosqt", "include", $"{QRInitializing.RunningProjectName}_rqt"), WorldSurrogate,
                          true, true));
                }




                //****************************************************************************************************
                //InstanceNode.h

                //#include "world2_rqt/WorldNodeAO.h"
                string ISSURROGATE_HEADER = this.AOType == AOTypeEnum.AOSurrogatePattern ? $"#include \"{MODULENAME}_rqt/{AONAME}NodeAO.h\"" : "";

                string InstanceNode = QRInitializing.TheMacro2Session.GenerateFileOut(
         $"QR\\InstanceNode",
                      new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                      new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },

                      new MacroVar() { MacroName = "SERVICE_INTERFACE_HEADERS", VariableValue = QREvent.INTERFACE_HEADERS() },

                      new MacroVar() { MacroName = "ISSURROGATE_HEADER", VariableValue = ISSURROGATE_HEADER },
                      new MacroVar() { MacroName = "ISSURROGATE_INHERIT1", VariableValue = this.AOType == AOTypeEnum.AOSurrogatePattern ? $", public {this.AONAME}NodeAO" : "" },
                      new MacroVar() { MacroName = "ISSURROGATE_INHERIT2", VariableValue = this.AOType == AOTypeEnum.AOSurrogatePattern ? $"" : "std::string id;" },
                      new MacroVar() { MacroName = "ISSURROGATE_INHERIT3", VariableValue = this.AOType == AOTypeEnum.AOSurrogatePattern ? $"" : "id = this->get_name();" },
                      new MacroVar() { MacroName = "ISSURROGATE_FORNODE_DEC", VariableValue = this.AOType == AOTypeEnum.AOSurrogatePattern ? $"" : "rclcpp::Node* ForNode;" },

                      new MacroVar() { MacroName = "INSTANCENAME", VariableValue = this.InstanceName },


                      // dont do this for surrogate types as they already have this done in the NodeAO.h file.
                      new MacroVar() { MacroName = "WNFUNCTION_SERVICES", VariableValue = ServiceFunction.WNFUNCTION_SERVICES(ServiceFunctions) },// GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICE) },
                      new MacroVar() { MacroName = "WNFUNCTION_SERVICES_DEFINES", VariableValue = ServiceFunction.WNFUNCTION_SERVICES_DEFINES(ServiceFunctions, this.AONAME, this.AOType == AOTypeEnum.AOSurrogatePattern) },//  GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICES_DEFINE) },
                      new MacroVar() { MacroName = "WNFUNCTIONS_IMPL", VariableValue = ServiceFunction.WNFUNCTIONS_IMPLS(ServiceFunctions, this.MODULENAME) },

                      //timer stuff
                      new MacroVar() { MacroName = "TIMER_DECLARES", VariableValue = ROSTimer.TIMER_DECLARES(ROSTimers) },
                      new MacroVar() { MacroName = "TIMER_DEFINES", VariableValue = ROSTimer.TIMER_DEFINES(ROSTimers) },
                      new MacroVar() { MacroName = "TIMER_FUNC_IMPS", VariableValue = ROSTimer.TIMER_FUNC_IMPS(ROSTimers) },

                      //subpub stuff
                      new MacroVar() { MacroName = "PUB_HEADER", VariableValue = ROSPublishers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.SUBPUB_HEADER, "\n") },
                      new MacroVar() { MacroName = "SUB_HEADER", VariableValue = ROSSubscribers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.SUBPUB_HEADER, "\n") },
                      new MacroVar() { MacroName = "PUBLISHER_DECLARE", VariableValue = ROSPublishers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.PUBLISHER_DECLARE, "\n") },
                      new MacroVar() { MacroName = "SUBSCRIBER_DECLARE", VariableValue = ROSSubscribers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.SUBSCRIBER_DECLARE, "\n") },
                      new MacroVar() { MacroName = "PUBLISHER_DEFINE", VariableValue = ROSPublishers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.PUBLISHER_DEFINE, "\n") },
                      new MacroVar() { MacroName = "SUBSCRIBER_DEFINE", VariableValue = ROSSubscribers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.SUBSCRIBER_DEFINE, "\n") },
                      new MacroVar() { MacroName = "PUBLISHER_FUNCTION", VariableValue = ROSPublishers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.PUBLISHER_FUNCTION, "\n") },
                      new MacroVar() { MacroName = "SUBSCRIBER_FUNCTION_CALLBACK", VariableValue = ROSSubscribers.GenerateAllForEvery_Arguments(ROSPublisherExtensions.SUBSCRIBER_FUNCTION_CALLBACK, "\n") }

                );

                string InstanceNodecpp = QRInitializing.TheMacro2Session.GenerateFileOut(
       $"QR\\InstanceNode_cpp",
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                    new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }
              );


                ret.Add(new RelativeDirPathWrite($"{this.ClassName}Node", "hpp",
                Path.Combine("rosqt", "include", $"{QRInitializing.RunningProjectName}_rqt"), InstanceNode,
                  true, true));

                ret.Add(new RelativeDirPathWrite($"{this.ClassName}Node", "cpp",
          Path.Combine("rosqt", "src"), InstanceNodecpp,
            true, true));


            }

            return ret;
        }


        public virtual string AO_DESCRIPTIONS_CP()
        {
            //AO: world 
            //instances: world 
            //type: simpleao
            string ret = $"//InstanceName: {InstanceName} \n";
            ret += $"//     AO: {AONAME}\n";
            ret += $"//     type: {AOType.ToString()}\n"; 
            return ret;
        }
        public virtual string AO_DESCRIPTIONS_RQT()
        {
            return AO_DESCRIPTIONS_CP();
        }
        public virtual string AO_MAINHEADER_CP()
        {
            return "";
        }
        public virtual string AO_MAINHEADER_RQT()
        {
            //#include "world2_rqt/TestSimpleNode.h"
            string ret = $"#include \"{MODULENAME}_rqt/{AONAME}Node.hpp\"";
            return ret;
        }
        public virtual string AO_DECLARES_CP()
        {
            return "";
        }
        public virtual string AO_DECLARES_RQT()
        {
            return "";
        }
        public virtual string AO_DEFINE_COMMENTS_CP()
        {
            return "";
        }
        public virtual string AO_DEFINE_COMMENTS_RQT()
        {
            return "";
        }
        public virtual string AO_MAININIT_CP()
        {
            return "";
        }
        public virtual string AO_MAININIT_RQT()
        {
            //auto World1_nodeobj = QR_Core::CreateNode<world2_rqt::WorldNode>(&exec, "World1");
            //World1_nodeobj->Init(World1_cppobj);
            string ret = "";
            ret += $"auto {InstanceName}_nodeobj = QR_Core::CreateNode <{MODULENAME}_rqt::{AONAME}Node> (&exec, \"{InstanceName}\");\n";
            return ret;
        }

        
    }
}
