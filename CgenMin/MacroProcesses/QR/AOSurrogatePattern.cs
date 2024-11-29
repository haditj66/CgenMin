using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses.QR
{



   public interface ISurrogateAO 
    {
        public string GenerateAO_SURROGATE_INIT_RQT(); 
    }


    public abstract class AOSurrogateNode<TDerivedType> : AOSurrogatePattern<TDerivedType>
    {
        public AOSurrogateNode(string instanceName,  bool isSurrogate) : base(instanceName, isSurrogate)
        { 
        }
    }


    public abstract class AOSurrogatePattern<TDerivedType> : AONode<TDerivedType>, ISurrogateAO
    {

        public bool IsSurrogate { get; }

        
        public static string DerivedClassName { get; set; }
        private static bool isInited = false;

        public static QREventMSG DataEvt { get; set; }

        protected override AOTypeEnum GetAOType()
        {
            return AOTypeEnum.AOSurrogatePattern;
        }


        //for every surrogate AO, there is a data eventMSG, and a eventSRV for every surrogate function args and response.


        public AOSurrogatePattern(string instanceName, bool isSurrogate) : base(instanceName)
        {
            IsSurrogate = isSurrogate;
             

            if (!isInited)
            {

                //create a ROSTimer that will be used to send the event that signifies the creation of the real AO
                ROSTimer rOSTimer = new ROSTimer("AOInitForSurrogatesTimer", 2000);
                this.AddROSTimer(rOSTimer);

                //========================================================================================


                //get the derived class name
                string derivedClassName = derivedType.Name;



                //go through all the properties of the derived class that have attributes of SurrogateData
                var properties = this.GetType().GetProperties();
                foreach (var property in properties)
                {
                    //go through each property and get the attributes of SurrogateData
                    if (property.GetCustomAttributes(typeof(SurrogateData), false).Length > 0)
                    {
                        //this is a surrogate data. create an instance of it
                        var surrogateDataInstance = (SurrogateData)property.GetCustomAttributes(typeof(SurrogateData), false)[0];
                        //var surrogateDataInstance = (SurrogateData)Activator.CreateInstance(typeof(SurrogateData));

                        //fill in all properties of the surrogate data
                        surrogateDataInstance.NameOfData = property.Name;
                        surrogateDataInstance._Type = property.PropertyType;
                        surrogateDataInstance.DefaultValue = "";

                        //add this to the list of surrogate datas
                        SurrogateDatas.Add(surrogateDataInstance);
                    }
                }

                //create the Data that every surrogate has
                List<FunctionArgsBase> DataEvtFuncArgs = new List<FunctionArgsBase>();
                DataEvtFuncArgs.Add(new FunctionArgsBase(typeof(string), "id"));
                foreach (var item in SurrogateDatas)
                {
                    DataEvtFuncArgs.Add(new FunctionArgsBase(item._Type, item.NameOfData));
                }

                DataEvt = new QREventMSG(this.FromModuleName, ClassName + "Data", DataEvtFuncArgs);

                isInited = true;

            }

        }







        public override string GetFullTemplateType()
        {
            return "";
        }

        public override string GetFullTemplateArgsValues()
        {
            return "";
        }

        public override string GetFullTemplateArgs()
        {
            return "";
        } 
        protected override string _GenerateAEConfigSection(int numOfAOOfThisSameTypeGeneratesAlready)
        {
            return "";
        }



        //=======================================================================================================
        #region cgenMM Generation variables for main.cpp
        //=======================================================================================================
         
        static bool GenerateAO_SURROGATE_INIT_RQT_Called_once = false;
        public string GenerateAO_SURROGATE_INIT_RQT()
        {
            //GameobjectInit* ginit = new GameobjectInit(); ginit->InitializeSurrogates();
            //GameobjectSurrogate* gObjsurr = new GameobjectSurrogate("Rock1");
            string ret = "";

            //do the first line only once per class of this surrogate
            if (GenerateAO_SURROGATE_INIT_RQT_Called_once == false)
            {
                ret += $"{AONAME}Init* {AONAME}init = new {AONAME}Init(); {AONAME}init->InitializeSurrogates();\n";
                 
                GenerateAO_SURROGATE_INIT_RQT_Called_once = true;
            }
            //only do this part if this is a surrogate 
            if (this.IsSurrogate)
            {
                ret += $"{AONAME}Surrogate* {InstanceName}surr = new {AONAME}Surrogate(\"{InstanceName}\");\n";
            }
            
            return ret;
        }





        public override string AO_DESCRIPTIONS_CP()
        {
            //AO: world 
            //instances: world 
            //type: surrogate pattern 
            //isSurrogate: false
            string strIsSurrogate = IsSurrogate ? "true\n" : "false\n";
            string ret = $"//InstanceName: {InstanceName} \n";
            ret += $"//     AO: {AONAME}\n";
            ret += $"//     type: {AOType.ToString()}\n";
            ret += $"//     isSurrogate: {strIsSurrogate}"; 
            return ret;
        }
        public override string AO_DESCRIPTIONS_RQT()
        {
            return AO_DESCRIPTIONS_CP();
        }


        public override string AO_MAINHEADER_CP()
        {
            //#include "world2_cp/World.h"
            string ret = $"#include \"{MODULENAME}_cp/{AONAME}.h\"";
            return ret;
        }

        static bool AO_MAINHEADER_RQT_Called_once = false;
        public override string AO_MAINHEADER_RQT()
        {
            string ret = "";
            if (AO_MAINHEADER_RQT_Called_once == false)
            {

                //#include "world2_rqt/WorldNode.hpp"
                ret = $"#include \"{MODULENAME}_rqt/{AONAME}Node.hpp\"\n";
                ret += $"#include \"{MODULENAME}_rqt/{AONAME}Surrogate.h\"";
                AO_MAINHEADER_RQT_Called_once = true;
            }
            return ret;
        }

        public override string AO_DECLARES_CP()
        {
            //WorldBase* World_cppobj;
            string ret = $"{AONAME}* {InstanceName}_cppobj;";
            return ret;
        }

        public override string AO_DECLARES_RQT()
        {
            //only do this part if this is not a surrogate
            string ret = "";
            if (this.IsSurrogate == false)
            {
                ret = AO_DECLARES_CP();
            }
            else
            {
                //WorldSurrogate* World1surr;
                ret = $"{AONAME}Surrogate* {InstanceName}surr;";
            }
            return ret;
        }


        public override string AO_DEFINE_COMMENTS_CP()
        { 

            //only do this part if this is not a surrogate
            string ret = "";
            if (this.IsSurrogate == false)
            {
                //example for @AONAME@: @AONAME@_cppobj = new @AONAME@(...);  
                ret = $"//example for {AONAME} of {InstanceName}: {InstanceName}_cppobj = new {AONAME}(...);";
            }
            return ret;
        } 
        public override string AO_DEFINE_COMMENTS_RQT()
        {
            return AO_DEFINE_COMMENTS_CP();
        }

        public override string AO_MAININIT_RQT()
        {
            //only do this part if this is not a surrogate
            string ret = "";
            if (this.IsSurrogate == false)
            {
                //auto wAO = QR_Core::CreateNode<world2_rqt::WorldNode>(&exec, "WorldNode");
                //wAO->Init(&w); 
                ret += $"auto {InstanceName}_nodeobj = QR_Core::CreateNode <{MODULENAME}_rqt::{AONAME}Node> (&exec, \"{InstanceName}\");\n";
                ret += $"{InstanceName}_nodeobj->Init({InstanceName}_cppobj);\n";
            } 
            return ret;
        }
        public override string AO_MAININIT_CP()
        {
            return "";
        }

        #endregion












       

        protected string PROPERTYGETANDSET(SurrogateData surrogateData)
        {
            //if this is a public set, then add the set
            string ret = $"virtual {surrogateData.TypeOfData} Get{surrogateData.NameOfData}() const = 0;"; 
            ret += "\n";
            if (surrogateData.IsPublicSet == true)
            {
                ret += $"public: virtual void Set{surrogateData.NameOfData}({surrogateData.TypeOfData} value) = 0;";
            }
            else
            {
                ret += $"protected: void Set{surrogateData.NameOfData}({surrogateData.TypeOfData} value) ";
                ret += "{";
                ret += $" data.{surrogateData.NameOfData} = value;";
                ret += "}"; 
            }
            ret += "\npublic:";
            return ret;
        }


        protected string PROPERTYEVT_CHANGED_DECLARE(SurrogateData surrogateData)
        {
            //dont do this if surrogateData is public set
            if (!surrogateData.IsPublicSet)
            {
                return "";
            }

            //if this is a public set, then add the set
            //I need to get the return type of the surrogateData and the argument type of the surrogateData and capitalize the first letter
            string argCap = char.ToUpper(surrogateData.TYPEASINSERVICE(false)[0]) + surrogateData.TYPEASINSERVICE(false).Substring(1);
            string ret = $"rclcpp::Publisher<qr_core::msg::Void{argCap}Changed>::SharedPtr publisher{surrogateData.NameOfData}Changed;";

            return ret;
        }


        //All qr SurrogateData events that are declared
        //give it a non duplicate data structure
        HashSet<string> PROPERTYEVTS_HEADERS = new HashSet<string>();


        protected string PROPERTYEVTS_DECLARED(SurrogateData surrogateData)
        {


            //dont do this if surrogateData is public set
            if (!surrogateData.IsPublicSet)
            {
                return "";
            }


            //if this is a public set, then add the set
            //I need to get the return type of the surrogateData and the argument type of the surrogateData and capitalize the first letter
            // rclcpp::Service<qr_core::srv::SyncInt64>::SharedPtr servicesetposx;
            string argCap = char.ToUpper(surrogateData.TYPEASINSERVICE(false)[0]) + surrogateData.TYPEASINSERVICE(false).Substring(1);
            string ret = $"rclcpp::Service<qr_core::srv::Sync{argCap}>::SharedPtr serviceset{surrogateData.NameOfData};";

            //add the event to the SurrogateDataEventsDeclared_HEADERS so to be able to include it in the headers
            PROPERTYEVTS_HEADERS.Add($"#include \"qr_core/srv/sync_{argCap.ToLower()}.hpp\"");
            PROPERTYEVTS_HEADERS.Add($"#include \"qr_core/msg/void_{argCap.ToLower()}_changed.hpp\"");

            return ret;
        }

        protected string PROPERTYEVTS_DEFINED(SurrogateData surrogateData)
        {
            //dont do this if surrogateData is public set
            if (!surrogateData.IsPublicSet)
            {
                return "";
            }

            //if this is a public set, then add the set
            //I need to get the return type of the surrogateData and the argument type of the surrogateData and capitalize the first letter
            string argCap = char.ToUpper(surrogateData.TYPEASINSERVICE(false)[0]) + surrogateData.TYPEASINSERVICE(false).Substring(1);
            string ret = $"serviceset{surrogateData.NameOfData} =  TheDataAccessManagerNode->create_service<qr_core::srv::Sync{argCap}>(";
            ret += $"cppobj->Getid() + \"/set{surrogateData.NameOfData}\", std::bind(&{this.AONAME}NodeAO::Set{surrogateData.NameOfData}Callback, this, _1, _2));";

            return ret;
        }
        protected string PROPERTYEVT_CHANGED_DEFINE(SurrogateData surrogateData)
        {
            //dont do this if surrogateData is public set
            if (!surrogateData.IsPublicSet)
            {
                return "";
            }

            //if this is a public set, then add the set
            //I need to get the return type of the surrogateData and the argument type of the surrogateData and capitalize the first letter
            string argCap = char.ToUpper(surrogateData.TYPEASINSERVICE(false)[0]) + surrogateData.TYPEASINSERVICE(false).Substring(1);
            string ret = $"publisher{surrogateData.NameOfData}Changed = TheDataAccessManagerNode->create_publisher<qr_core::msg::Void{argCap}Changed>(obj->Getid() + \"/\"+ \"{surrogateData.NameOfData}\" + \"DataChanged\",100);";

            return ret;
        }
        protected string PROPERTYEVTS_SET_CALLBACK(SurrogateData surrogateData)
        {
            //dont do this if surrogateData is public set
            if (!surrogateData.IsPublicSet)
            {
                return "";
            }
            string argCap = char.ToUpper(surrogateData.TYPEASINSERVICE(false)[0]) + surrogateData.TYPEASINSERVICE(false).Substring(1);

            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\WNPropertyEvt_Set_Callback",
              new MacroVar() { MacroName = "ARGCAP", VariableValue = argCap },
              new MacroVar() { MacroName = "NAMEOFDATA", VariableValue = surrogateData.NameOfData }
              );
            return ret;
        }


        protected string AOFUNCTION_CPPOBJ(ServiceFunction surrogateFunction)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunction_cppobj",
                    new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                    new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "ARGRETURN", VariableValue = surrogateFunction.ARGRETURN },
                    new MacroVar() { MacroName = "ARGS", VariableValue = surrogateFunction.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG, ",") },
                    new MacroVar() { MacroName = "ARGSNAME", VariableValue = surrogateFunction.ARGSNAME() },
                    new MacroVar() { MacroName = "COMMA_IF_ARGSNAME", VariableValue = surrogateFunction.ARGSNAME() == "" ? "" : "," }
                    );
            return ret;
        }

        protected string AOFUNCTION_REAL_IMPS(ServiceFunction surrogateFunction)
        {
            string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\AOFunction_Real_Imps",
                    new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION },
                    new MacroVar() { MacroName = "ARGRETURN", VariableValue = surrogateFunction.ARGRETURN },
                    new MacroVar() { MacroName = "ARGS", VariableValue = surrogateFunction.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG, ",") }
                    );
            return ret;
        }

        //protected string WNFUNCTION_SERVICE(ServiceFunction surrogateFunction)
        //{
        //    string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\WNFunction_Service",
        //             new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION } 
        //            );
        //    return ret;
        //}

        //protected string WNFUNCTION_SERVICES_DEFINE(ServiceFunction surrogateFunction)
        //{
        //    string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\WNFunction_Service_Define", 
        //            new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },
        //            new MacroVar() { MacroName = "NAMEOFFUNCTION", VariableValue = surrogateFunction.NAMEOFFUNCTION },
        //            new MacroVar() { MacroName = "NUMOFARGS", VariableValue = surrogateFunction.NUMOFARGS() }, 
        //            new MacroVar() { MacroName = "COMMA_IF_ARGSNAME", VariableValue = surrogateFunction.NUMOFARGS() == "" ? "" : "," }
        //            );
        //    return ret;
        //}


        protected string PROPERTYIMPL_CPPOBJ(SurrogateData surrogateData)
        {
            //if this is a public set, then add the set
            string ret = $"{surrogateData.TypeOfData} Get{surrogateData.NameOfData}() const" + "{{" + $"return this->data.{surrogateData.NameOfData};" + "}}";
            string access = surrogateData.IsPublicSet ? "public:" : "protected:";
            ret += "\n";

            if (surrogateData.IsPublicSet == true)
            {
                 ret += $"{access} void Set{surrogateData.NameOfData}({surrogateData.TypeOfData} value)" + "{" + $"this->data.{surrogateData.NameOfData} = value;" + "}";
            }

            ret += "\npublic:";
            return ret;
        }
        public string WNPLACEHOLDERARGS()
        {
            //go through every function and get the arguments count and get the largest one
            int max = 0;
            foreach (var item in SurrogateServiceFunctions)
            {
                if (item.Args.Count > max)
                {
                    max = item.Args.Count;
                }
            }

            //based on the number max, create a string that will be _1, _2, _3 up to the number of max
            string ret = "";
            for (int i = 1; i <= max; i++)
            {
                ret += $"using std::placeholders::_{i};\n";
            }
            return ret;

        }


        protected string AONAMEData;

        protected override List<RelativeDirPathWrite> _WriteTheContentedToFiles()
        {
            var ret = new List<RelativeDirPathWrite>();

            //dont do any of the bottom if the AO is not from the running project
            if (this.MODULENAME == QRInitializing.RunningProjectName)
            {

                //there are many files associated with this AO. These are all the files

                //Here is the hierarchy of the files
                //                          WorldBase
                //                          /       \
                //                         /         \      
                //             WorldBase_cppobj       \
                //                       /            \
                //                      /              \
                //                  World         WorldSurrogate  

                //                        WorldInit

                //                        WorldNodeAO --> World 
                //                             |
                //                             |
                //                        InstanceNode

                //here are .h files. 
                //WorldBase.h:  WorldBase - WorldBase_cppobj
                //      includes=> All interfaces
                //            
                //World.h:  World
                //      includes=> WorldBase.h
                //
                //WorldSurrogate.h: WorldSurrogate - WorldInit
                //      includes=> World.h
                //
                //WorldNodeAO.h: WorldNodeAO  
                //      includes=> World.h
                //
                //InstanceNode.hpp: InstanceNode
                //      includes=> WorldNodeAO.h

                //---------------------------------------------------------------------------------------------------
                //World: this is the class that is just the simple cpp object. This is the one the user fills logic with.

                //---------------------------------------------------------------------------------------------------
                //WorldSurrogate: this is the class ROS will use to take place of the world object. It is a surrogate that
                //will be used to forward messages to whereever the real WorldAO is

                //---------------------------------------------------------------------------------------------------
                //WorldInit: this is a node that is responsible for creating the surrogates AO when the real one is created.
                //The real AO sends an event when it is created that WorldInit will use.

                //---------------------------------------------------------------------------------------------------
                //WorldNodeAO: This is the real AO object for ROS. this class will be the one to handle any ROS calls to it
                //where it will simply forward those calls to the cpp object it has.

                //---------------------------------------------------------------------------------------------------
                //InstanceNode: This is the instance of the WorldNodeAO. You can have multiple instances of nodes of type WorldnodeAO .







            //****************************************************************************************************
            //worldBase.h
             

            string WorldBase = QRInitializing.TheMacro2Session.GenerateFileOut(
                $"QR\\SurrogatePattern\\WorldBase",
          new MacroVar() { MacroName = "INTERFACE_HEADERS", VariableValue = QREvent.INTERFACE_HEADERS() },
          new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
          new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },
          new MacroVar() { MacroName = "PROPERTYGETANDSETS", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYGETANDSET) },

          new MacroVar() { MacroName = "AOFUNCTIONS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION) },
          new MacroVar() { MacroName = "AOFUNCTION_CPPOBJS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_CPPOBJ) },

           new MacroVar() { MacroName = "PROPERTYIMPLS", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYIMPL_CPPOBJ) }

);

            //worldBase.cpp
            string WorldBasecpp = QRInitializing.TheMacro2Session.GenerateFileOut(
$"QR\\SurrogatePattern\\WorldBasecpp",
      new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
      new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }

);


            ret.Add(new RelativeDirPathWrite($"{this.ClassName}Base", "h",
Path.Combine("include", $"{QRInitializing.RunningProjectName}_cp"), WorldBase,
  true, true));

            ret.Add(new RelativeDirPathWrite($"{this.ClassName}Base", "cpp",
            "src", WorldBasecpp,
            true, true));


            //****************************************************************************************************
            //world.h

            string World = QRInitializing.TheMacro2Session.GenerateFileOut(
     $"QR\\SurrogatePattern\\World",
                  new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                  new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },
                      new MacroVar() { MacroName = "AOFUNCTION_REAL_IMPS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_REAL_IMPS) }

      );

            //world.cpp
            string Worldcpp = QRInitializing.TheMacro2Session.GenerateFileOut(
$"QR\\SurrogatePattern\\Worldcpp",
      new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
      new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }

);


            ret.Add(new RelativeDirPathWrite($"{this.ClassName}", "h",
            Path.Combine("include", $"{QRInitializing.RunningProjectName}_cp"), World,
              true, true));

            ret.Add(new RelativeDirPathWrite($"{this.ClassName}", "cpp",
            "src", Worldcpp,
              true, true));



   



            //****************************************************************************************************
            //WorldNodeAO.h

            string WorldNodeAO = QRInitializing.TheMacro2Session.GenerateFileOut(
     $"QR\\SurrogatePattern\\WorldNodeAO",
                  new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                  new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME },

                  new MacroVar() { MacroName = "PROPERTYEVTS_CHANGED_DECLARE", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYEVT_CHANGED_DECLARE) },
                  new MacroVar() { MacroName = "PROPERTYEVTS_DEFINED", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYEVTS_DEFINED) },
                  new MacroVar() { MacroName = "PROPERTYEVTS_DECLARED", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYEVTS_DECLARED) },
                  new MacroVar() { MacroName = "PROPERTYEVTS_CHANGED_DEFINE", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYEVT_CHANGED_DEFINE) },
                  new MacroVar() { MacroName = "PROPERTYEVTS_SET_CALLBACKS", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYEVTS_SET_CALLBACK) },
                  new MacroVar() { MacroName = "PROPERTYEVTS_HEADERS", VariableValue = string.Join("\n", PROPERTYEVTS_HEADERS) },

                  new MacroVar() { MacroName = "WNPLACEHOLDERARGS", VariableValue = WNPLACEHOLDERARGS() },
                  new MacroVar() { MacroName = "WNFUNCTION_SERVICES", VariableValue = ServiceFunction.WNFUNCTION_SERVICES(SurrogateServiceFunctions) },// GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICE) },
                  new MacroVar() { MacroName = "WNFUNCTION_SERVICES_DEFINES", VariableValue = ServiceFunction.WNFUNCTION_SERVICES_DEFINES(SurrogateServiceFunctions, this.AONAME, true) },//  GenerateAllForEvery_SurrogateFunction(WNFUNCTION_SERVICES_DEFINE) },
                  new MacroVar() { MacroName = "WNFUNCTIONS_IMPL", VariableValue = ServiceFunction.WNFUNCTIONS_IMPLS(SurrogateServiceFunctions, this.MODULENAME) }// GenerateAllForEvery_SurrogateFunction(WNFUNCTIONS_IMPL) }


      );


            ret.Add(new RelativeDirPathWrite($"{this.ClassName}NodeAO", "h",
            Path.Combine("rosqt", "include", $"{QRInitializing.RunningProjectName}_rqt"), WorldNodeAO,
              true, true));



            //****************************************************************************************************
            //InstanceNode.hpp  called from node base
            ret.AddRange(_WriteTheContentedToFiles_NodeBASE());



                //       string ALL = QRInitializing.TheMacro2Session.GenerateFileOut(
                //$"QR\\SurrogatePattern\\WorldTest",
                // new MacroVar() { MacroName = "INTERFACE_HEADERS", VariableValue = QREvent.INTERFACE_HEADERS() },
                //             new MacroVar() { MacroName = "MODULENAME", VariableValue = this.MODULENAME },
                //             new MacroVar() { MacroName = "AONAME", VariableValue = this.AONAME }, 
                //             new MacroVar() { MacroName = "PROPERTYGETANDSETS", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYGETANDSET) },

                //             new MacroVar() { MacroName = "AOFUNCTIONS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION) },
                //             new MacroVar() { MacroName = "AOFUNCTION_TICKETS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_TICKET) },
                //             new MacroVar() { MacroName = "AOFUNCTION_CLIENTS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_CLIENT) },
                //           new MacroVar() { MacroName = "AOFUNCTION_CLIENT_DECLARES", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_CLIENT_DECLARE) },
                //           new MacroVar() { MacroName = "AOFUNCTION_IMPS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_IMP) },
                //           new MacroVar() { MacroName = "AOFUNCTION_CPPOBJS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_CPPOBJ) },
                //           new MacroVar() { MacroName = "AOFUNCTION_REAL_IMPS", VariableValue = GenerateAllForEvery_SurrogateFunction(AOFUNCTION_REAL_IMPS) },

                //           new MacroVar() { MacroName = "PROPERTYIMPLS", VariableValue = GenerateAllForEvery_SurrogateData(PROPERTYIMPL) }

                // );


                //       ret.Add(new RelativeDirPathWrite($"{this.ClassName}Test", "h",
                //       Path.Combine("include", $"{QRInitializing.RunningProjectName}_cp"), ALL,
                //         true, false));



            }

            return ret;
        }

    }
}
