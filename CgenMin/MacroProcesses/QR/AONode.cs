using CodeGenerator.ProblemHandler;
using System.Collections.Generic;
using System.IO;

namespace CgenMin.MacroProcesses.QR
{

    public abstract class AONodeBase : AOWritableConstructible
    {
        protected string MODULENAME;
        protected string AONAME;
         
        protected AONodeBase(string fromLibrary, string instanceNameOfTDU, AOTypeEnum aOTypeEnum ) : base(fromLibrary, instanceNameOfTDU, aOTypeEnum )
        {

        }


    }

    public abstract class AONode<TDerivedType> : AONodeBase
    {

        private static bool isInited = false;
        protected Type derivedType = typeof(TDerivedType);


        public static List<ServiceFunction> ServiceFunctions { get; private set; }
        public static List<SurrogateServiceFunction> SurrogateServiceFunctions { get; private set; }

        protected AONode(string AOName) : base(QRInitializing.RunningProjectName, AOName, AOTypeEnum.AOSurrogatePattern)
        {
            _Init();
        }
        public AONode(string AOName, int s) : base(QRInitializing.RunningProjectName, AOName, AOTypeEnum.AONode)
        {
            _Init();
        }


        public abstract List<ROSTimer> SetAllTimers();


        public static List<ROSTimer> _ROSTimers = new List<ROSTimer>();
        public static List<ROSTimer> ROSTimers { get { return _ROSTimers; } }
        public void AddROSTimer(ROSTimer rOSTimer)
        {
            rOSTimer.AOIBelongTo = this;
            rOSTimer.IsForSurrogate = rOSTimer.NameOfTimer == "AOInitForSurrogatesTimer";
            _ROSTimers.Add(rOSTimer);
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
                        surrogateFunctionInstance.Args = new List<FunctionArgs>();
                        surrogateFunctionInstance.Name = method.Name;


                        //fill in all properties of the surrogate function
                        surrogateFunctionInstance.TypeOFResponse = new FunctionArgs(method.ReturnType, method.ReturnType.Name);


                        surrogateFunctionInstance.Args = method.GetParameters().Select(p =>
                        {
                            //if parameter is of type AOSurrogatePattern
                            return new FunctionArgs(p.ParameterType, p.Name, p.Position);


                        }
                        ).ToList();




                        //create a service event for the function
                        List<FunctionArgs> tt = method.GetParameters().Select(p => new FunctionArgs(p.ParameterType, p.Name)).ToList();
                        //List<string> ttt = method.GetParameters().Select(p => p.ParameterType.Name).ToList();
                        ////alternate tt and ttt into a single list
                        //tt = ttt.SelectMany((x, i) => new List<string> { x, tt[i] }).ToList();
                        surrogateFunctionInstance.FunctionServiceEvent = new QREventSRV(method.Name, new FunctionArgs(method.ReturnType, ""), tt);

                        //add this to the list of surrogate functions
                        ret.Add(surrogateFunctionInstance);
                    } 

                }
            }

            return ret;
        } 

        private void _Init()
        {

            if (!isInited)
            {
                 
                foreach (var timer in SetAllTimers())
                {
                    this.AddROSTimer(timer);
                    timer.AOIBelongTo = this;
                }

                this.MODULENAME = QRInitializing.RunningProjectName;
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

                ServiceFunctions = new List<ServiceFunction>();
                SurrogateServiceFunctions = new List<SurrogateServiceFunction>();

                ServiceFunctions = GetAllServiceFunctions<ServiceFunction>();
                SurrogateServiceFunctions = GetAllServiceFunctions<SurrogateServiceFunction>();


                isInited = true;
            }
        }

        public override string GenerateMainHeaderSection_CP()
        {
            return "";
        }

        public override string GenerateMainHeaderSection_RQT()
        {
            return "";
        }

        public override string GenerateMainInitializeSection_CP()
        {
            return "";
        }

        public override string GenerateMainInitializeSection_RQT()
        {
            return "";
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



        protected List<RelativeDirPathWrite> _WriteTheContentedToFiles_NodeBASE()
        {
            var ret = new List<RelativeDirPathWrite>();


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
                  new MacroVar() { MacroName = "ISSURROGATE_INHERIT", VariableValue = this.AOType == AOTypeEnum.AOSurrogatePattern ? $", public {this.AONAME}NodeAO" : "" },

                  new MacroVar() { MacroName = "INSTANCENAME", VariableValue = this.InstanceName },

                   
                  // dont do this for surrogate types as they already have this done in the NodeAO.h file.
                  new MacroVar() { MacroName = "WNFUNCTION_SERVICES", VariableValue =  ServiceFunction.WNFUNCTION_SERVICES(ServiceFunctions) },// GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICE) },
                  new MacroVar() { MacroName = "WNFUNCTION_SERVICES_DEFINES", VariableValue =  ServiceFunction.WNFUNCTION_SERVICES_DEFINES(ServiceFunctions, this.AONAME) },//  GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICES_DEFINE) },
                  new MacroVar() { MacroName = "WNFUNCTIONS_IMPL", VariableValue = ServiceFunction.WNFUNCTIONS_IMPLS(ServiceFunctions, this.MODULENAME) },

                  //timer stuff
                  new MacroVar() { MacroName = "TIMER_DECLARES", VariableValue = ROSTimer.TIMER_DECLARES(ROSTimers) }, 
                  new MacroVar() { MacroName = "TIMER_DEFINES", VariableValue = ROSTimer.TIMER_DEFINES(ROSTimers) }, 
                  new MacroVar() { MacroName = "TIMER_FUNC_IMPS", VariableValue = ROSTimer.TIMER_FUNC_IMPS(ROSTimers) }
            );

            ret.Add(new RelativeDirPathWrite($"{this.ClassName}Node", "hpp",
            Path.Combine("rosqt", "include", $"{QRInitializing.RunningProjectName}_rqt"), InstanceNode,
              true, true));



            return ret;
        }
    }
}
