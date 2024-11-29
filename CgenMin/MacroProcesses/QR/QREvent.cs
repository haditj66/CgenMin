﻿using CgenMin.MacroProcesses.QR;
using CodeGenerator.ProblemHandler;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace CgenMin.MacroProcesses
{

    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class QREnum : System.Attribute
    {
        public QREnum(Type fromModule)
        {
            FromModule = fromModule;
        }

        public Type FromModule { get; }


    }



    //^(?!.*__)(?!.*_$)[a-z][a-z0-9_]*$
    public enum QREventType
    {
        MSG,
        SRV,
        ACTION
    }

    public abstract class QREvent : AOWritableToAOClassContents
    {
        public QREventType QREVTType { get; private set; }
        public FunctionArgsBase[] EventProperties { get; protected set; }
        public List<FunctionArgsBase > EventPropertiesList { get { return EventProperties.ToList(); } }
        public FunctionArgsBase ReturnType { get; protected set; }
        //public string ReturnName { get; protected set; }
        public bool IsFromEnum { get; protected set; }
        public int EventPoolSize { get; protected set; }
        public QREnum QREnumAttr { get; set; }

        public string GetUniqueName()
        { 
            return $"{this.FromModuleName}:{this.InstanceName}";
        }

        public static int NumOfSRVCreatedSoFar { get { return numOfSRVCreatedSoFar; }  }
        static int numOfSRVCreatedSoFar = 0;
        static int numOfMSGsCreatedSoFar = 0;
        int eventID = 0;
        int sigID = 0;

        public static List<QREvent> AllAEEvents = new List<QREvent>();
        public static List<QREvent> AllSelectedTargetEvents = new List<QREvent>();
        protected static bool isTargetStarted = false;
        public QREvent(string fromModule, string ClassName, QREventType qRType, params FunctionArgsBase[] eventDefinition)
            : base(fromModule, ClassName.First().ToString().ToUpper() + ClassName.Substring(1))
        {
            //uppercase the first letter of the class name 
            AOType = AOTypeEnum.Event;
            EventProperties = eventDefinition;
            Constr(qRType);
        }

        public QREvent(string fromModule, string ClassName, QREventType qRType, List<FunctionArgsBase> eventDefinition)
            : base(fromModule, ClassName.First().ToString().ToUpper() + ClassName.Substring(1))
        {
            //this(ClassName,   returnType,   returnName,  eventProperties.ToArray());
            AOType = AOTypeEnum.Event;
            EventProperties = eventDefinition.ToArray();
            Constr(qRType);
        }

        public static void Reset()
        {
            numOfSRVCreatedSoFar = 0;
            numOfAOSoFarAEConfigGenerated = 0;
            numOfMSGsCreatedSoFar = 0;
            AllAEEvents.Clear();
            AllSelectedTargetEvents.Clear();
            isTargetStarted = false;

        }

        public static string INTERFACE_HEADERS()
        {
            // for every event in AllAEEvents, get the INTERFACE_HEADER 
            string ret = "";
            foreach (var item in AllAEEvents)
            {
                ret += item.INTERFACE_HEADER();
                ret += "\n";
            }
            return ret; 
        }

        public string INTERFACE_HEADER()
        {
            string evtType =
               QREVTType == QREventType.MSG ? $"msg" :
               QREVTType == QREventType.SRV ? "srv" :
               "act";

            return $"#include \"{QRInitializing.RunningProjectName}_i/{evtType}/{INTERFACE_HEADER_NAME()}.hpp\" " ;
        }


        public string INTERFACE_HEADER_NAME()
        {
            // Initialize return string
            string ret = "";
            bool isFirstCharacter = true;
            bool lastWasUpper = false;

            foreach (var item in this.InstanceName)
            {
                if (isFirstCharacter)
                {
                    // Convert the first character to lowercase if it's uppercase, without adding an underscore
                    ret += char.ToLower(item);
                    isFirstCharacter = false;
                    lastWasUpper = char.IsUpper(item);
                }
                else
                {
                    if (char.IsUpper(item))
                    {
                        // If the last character was not uppercase, add an underscore
                        if (!lastWasUpper)
                        {
                            ret += "_";
                        }

                        // Add the lowercase version of the current character
                        ret += char.ToLower(item);
                        lastWasUpper = true;
                    }
                    else
                    {
                        // If the current character is lowercase, reset lastWasUpper and add it as-is
                        ret += item;
                        lastWasUpper = false;
                    }
                }
            }

            return ret;
        }



        private void Constr(QREventType qRType)
        {
            QREVTType = qRType;

            //check that all service names are compatible with the ROS naming convention
            var compatibleArgs = EventPropertiesList.CheckIfAllArgNamesAreCompatibleForRosService();
            if (compatibleArgs != null)
            {
                if (compatibleArgs.IsFromEnumArg == false)
                {
                    ProblemHandle problemHandle = new ProblemHandle();
                    problemHandle.ThereisAProblem($"Argument name '{compatibleArgs.Name}' in QREvent {this.InstanceName} is not compatible with the ROS service naming convention. No capital letters and No double underscores __");
                }
                else
                {
                    ProblemHandle problemHandle = new ProblemHandle();
                    problemHandle.ThereisAProblem($"Argument name '{compatibleArgs.Name}' in QREvent {this.InstanceName} is not compatible with the ROS service naming convention. Since it from an enum, it needs to be all capitalized letters.");

                }

            }
 
            //dont add the updateEvt
            if (this.ClassName != "UpdateEVT")
            {
                if (QREventType.MSG == QREVTType)
                {
                    numOfMSGsCreatedSoFar++;
                    sigID = numOfMSGsCreatedSoFar; 
                    var exists = AllAEEvents.Where(s => s.GetUniqueName() == this.GetUniqueName()).FirstOrDefault();
                    if (exists ==null)
                    {
                        AllAEEvents.Add(this);
                    }
                }
                else
                {
                    numOfSRVCreatedSoFar++;
                    eventID = numOfSRVCreatedSoFar;
                    var exists = AllAEEvents.Where(s => s.GetUniqueName() == this.GetUniqueName()).FirstOrDefault();
                    if (exists == null)
                    {
                        AllAEEvents.Add(this);
                    }
                }
            }

            if (isTargetStarted)
            {
                AllSelectedTargetEvents.Add(this);
            }
        }

        public static void TargetStartRun()
        {
            //reset the list of all events
            AllSelectedTargetEvents = new List<QREvent>();
            AllAEEvents = new List<QREvent>();

            //set bool that will be used to add an event to the list in the constructor
            isTargetStarted = true;
        }

        public static void TargetEndRun()
        {
            isTargetStarted = false;
        }



        //public override string GenerateFunctionDefinesSection()
        //{
        //    return "";
        //}

        //public override string GenerateMainClockSetupsSection()
        //{
        //    return "";
        //}

    

        //public override string GenerateMainLinkSetupsSection()
        //{
        //    return "";
        //}

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
            //#define Event1 EventTest1
            //#define Event1Size 10

            string ret = "";

            if (QREVTType == QREventType.MSG)
            {
                ret += $"#define Event{eventID.ToString()} {ClassName}"; ret += "\n";
                ret += $"#define Event{eventID.ToString()}Size {EventPoolSize.ToString()}"; ret += "\n";
            }
            else
            {
                //#define Signal1 DoneUploading_Sig
                //#define Signal2 SomeOther_Sig
                //#define Signal3 Button1_Sig //value for a null signal
                //#define Signal4 Button2_Sig //value for a null signal
                //#define Signal5 Button3_Sig //value for a null signal 
                ret += $"#define Signal{sigID.ToString()} {ClassName}"; ret += "\n";
            }

            return ret;
        }


        static bool onlyWriteFilesOnce = false;
        protected override List<RelativeDirPathWrite> _WriteTheContentedToFiles()
        {

            //dont do this if the event comes from a QREnum attribute type
            if (this.IsFromEnum)
            {
                //get the attribute of QREnum attatxched to this class 
                if (this.QREnumAttr.FromModule.Name != QRInitializing.RunningProjectName)
                {
                    return null;
                }
                
            }



            //get all eventproperties and put them in correct format 
            //go through EventProperties, grab every 2 elements, concatenate them and put them in a list
            var concatenatedProperties = new List<string>();

            for (int i = 0; i < EventProperties.Length; i += 1)
            {
                string toAdd = EventProperties[i].TYPEASINSERVICE() + " " + EventProperties[i].NAMEASINSERVICE();
                toAdd = EventProperties[i].IsFromEnumArg ? toAdd + "=" + EventProperties[i].Argnum.ToString() : toAdd;
                concatenatedProperties.Add(toAdd); 

            }

            string AllEventContents = string.Join("\n", concatenatedProperties);

            string path = Path.Combine("rosqt", "IF");
            string ext = "";
            if (this.QREVTType == QREventType.SRV)
            {
                //if the ReturnType is "" than give empty 
                //if this is a service, get the return type and return name
                AllEventContents = ReturnType.TYPEASINSERVICE() == "" ? AllEventContents + "\n---\n" :
                   AllEventContents + "\n---\n" + ReturnType.TYPEASINSERVICE() + " " + "result";
                path = Path.Combine(path, "srv");
                ext = "srv";
            }
            if (this.QREVTType == QREventType.MSG)
            {
                path = Path.Combine(path, "msg");
                ext = "msg";
            }

            List<RelativeDirPathWrite> ret = new List<RelativeDirPathWrite>();
            //for (int i = 0; i < AllAEEvents.Count; i++)
            //{
            ret.Add(new RelativeDirPathWrite(this.InstanceName, ext, path, AllEventContents, false, false));
            onlyWriteFilesOnce = true;
            return ret;

            ////first grab all the contents of the events
            //List<string> Allcontents = new List<string>();
            //foreach (var item in AllSelectedTargetEvents)
            //{
            //    //dont do this for signals
            //    if (item.QRType == QREventType.MSG)
            //    {
            //        string ArgSection = string.Join("\n", item.EventProperties);
            //        Allcontents.Add(QRInitializing.TheMacro2Session.GenerateFileOut("AERTOS/AEEvent",
            //            new MacroVar() { MacroName = "ClassName", VariableValue = $"{item.ClassName}" },
            //            new MacroVar() { MacroName = "ArgSection", VariableValue = $"{ArgSection}" }
            //            ));
            //    }
            //}

            //string AllEventContents = string.Join("\n", Allcontents);
            //onlyWriteFilesOnce = true;
            //return ret;



            //AllEventContents = QRInitializing.TheMacro2Session.GenerateFileOut("AERTOS/EventsForProject",
            //    new MacroVar() { MacroName = "AllEvents", VariableValue = $"{AllEventContents}" }
            //    );



            //List<RelativeDirPathWrite> ret = new List<RelativeDirPathWrite>();
            ////for (int i = 0; i < AllAEEvents.Count; i++)
            ////{
            //ret.Add(new RelativeDirPathWrite("EventsForProject.h", "h", "conf", AllEventContents, false));
            ////}


            //onlyWriteFilesOnce = true;
            //return ret; 
            return null;

        }

        public string GenerateCmakeCommand()
        {
             return QRInitializing.TheMacro2Session.GenerateFileOut("QR\\InterfaceCmakeCommand",
                new MacroVar() { MacroName = "NAME_OF_MESSAGE", VariableValue = this.InstanceName },
                new MacroVar() { MacroName = "EVT_TYPE", VariableValue = this.QREVTType == QREventType.MSG ? "message" : this.QREVTType == QREventType.SRV ? "service" : "action" },
                new MacroVar() { MacroName = "INTERFACE_PKG_DEPENDS", VariableValue = "builtin_interfaces std_msgs" }
                );
        }
    }



    //public abstract class AEEventBase<TDerived> : QREvent
    //where TDerived : AEEventBase<TDerived>, new()

    public class AEEventBase : QREvent
    {



        protected AEEventBase(string fromModule, string ClassName, QREventType qrType, params FunctionArgsBase[] eventProperties)
            : base(fromModule, ClassName, qrType, eventProperties)
        {
        }

        protected AEEventBase(string fromModule, string ClassName, QREventType qrType, List<FunctionArgsBase> eventProperties)
            : base(fromModule, ClassName, qrType, eventProperties)
        {

        }



        //protected static void _Init(int eventPoolSize)
        ////: base(fromLibrary, ClassName, eventPoolSize, eventDefinition)
        //{
        //    if (_instance == null)
        //    {
        //        _instance = this;
        //        _instance.EventPoolSize = eventPoolSize;
        //    }
        //}


        public static void Init()
        {
            //if this is a depending library, make this always be one.
            //if (QRInitializing.DependingLib == true)
            //{
            //    eventPoolSize = 1;
            //}


            //_Init(eventPoolSize);
            //_instance.EventPoolSize = eventPoolSize;
            //return (QREvent)_instance;

        }

 

        //public static TDerived Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            ProblemHandle prob = new ProblemHandle();
        //            prob.ThereisAProblem($"You never created an instance of  event {typeof(TDerived).ToString()} \n by calling the AEEventFactory() for it in the _GetEventsInLibrary() function for the AEProject");
        //        }
        //        return _instance;
        //    }
        //}

    }

    //    public class QREventSRV<TDerived> : AEEventBase<TDerived>
    //where TDerived : AEEventBase<TDerived>, new()
    public class QREventSRV : AEEventBase
    {

        public QREventSRV(string fromModule, string ClassName, FunctionArgsBase returnType, params FunctionArgsBase[] eventProperties)
         : base(fromModule, ClassName, QREventType.SRV, eventProperties)
        {
            AO.atLeastOneEvt = true;
            ReturnType = returnType;
        }

        public QREventSRV(string fromModule, string ClassName, FunctionArgsBase returnType, List<FunctionArgsBase> eventProperties)
    : base(fromModule, ClassName, QREventType.SRV, eventProperties)
        {
            AO.atLeastOneEvt = true;
            ReturnType = returnType;
        }

        public static QREventSRV GetEventOfName(string name)
        {
            return (QREventSRV)AllAEEvents.Where(d => d.InstanceName == name).FirstOrDefault();

        }

    }


    public class QREventMSGTemplate<TARG1Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1) : base(  fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1) 
            })
        { 
        }
    
    }
    public class QREventMSGTemplate<TARG1Type, TARG2Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1, string nameOfArg2) : base(fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1),
                new FunctionArgsBase(typeof(TARG2Type), nameOfArg2)
            })
        {
        } 
    }

    public class QREventMSGTemplate<TARG1Type, TARG2Type, TARG3Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1, string nameOfArg2, string nameOfArg3) : base(fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1),
                new FunctionArgsBase(typeof(TARG2Type), nameOfArg2),
                new FunctionArgsBase(typeof(TARG3Type), nameOfArg3)
            })
        {
        }
    }

    public class QREventMSGTemplate<TARG1Type, TARG2Type, TARG3Type, TARG4Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1, string nameOfArg2, string nameOfArg3, string nameOfArg4) : base(fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1),
                new FunctionArgsBase(typeof(TARG2Type), nameOfArg2),
                new FunctionArgsBase(typeof(TARG3Type), nameOfArg3),
                new FunctionArgsBase(typeof(TARG4Type), nameOfArg4)
            })
        {
        }
    }

    public class QREventMSGTemplate<TARG1Type, TARG2Type, TARG3Type, TARG4Type, TARG5Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1, string nameOfArg2, string nameOfArg3, string nameOfArg4, string nameOfArg5) : base(fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1),
                new FunctionArgsBase(typeof(TARG2Type), nameOfArg2),
                new FunctionArgsBase(typeof(TARG3Type), nameOfArg3),
                new FunctionArgsBase(typeof(TARG4Type), nameOfArg4),
                new FunctionArgsBase(typeof(TARG5Type), nameOfArg5)
            })
        {
        }
    }

    public class QREventMSGTemplate<TARG1Type, TARG2Type, TARG3Type, TARG4Type, TARG5Type, TARG6Type> : QREventMSG
    {
        public QREventMSGTemplate(string fromModule, string ClassName, string nameOfArg1, string nameOfArg2, string nameOfArg3, string nameOfArg4, string nameOfArg5, string nameOfArg6) : base(fromModule, ClassName,
            new List<FunctionArgsBase>() {
                new FunctionArgsBase(typeof(TARG1Type), nameOfArg1),
                new FunctionArgsBase(typeof(TARG2Type), nameOfArg2),
                new FunctionArgsBase(typeof(TARG3Type), nameOfArg3),
                new FunctionArgsBase(typeof(TARG4Type), nameOfArg4),
                new FunctionArgsBase(typeof(TARG5Type), nameOfArg5),
                new FunctionArgsBase(typeof(TARG6Type), nameOfArg6)
            })
        {
        }
    } 


    //    public class QREventMSG<TDerived> : AEEventBase<TDerived>
    //where TDerived : AEEventBase<TDerived>, new()
    public class QREventMSG : AEEventBase
    {
        public QREventMSG(string fromModule, string ClassName, params FunctionArgsBase[] eventProperties)
            : base(fromModule, ClassName, QREventType.MSG, eventProperties)
        {
        }

        public QREventMSG(string fromModule, string ClassName, List<FunctionArgsBase> eventProperties)
            : base(fromModule, ClassName, QREventType.MSG, eventProperties)
        {
        }

        public static QREventMSG GetEventOfName(string name)
        {
            return (QREventMSG)AllAEEvents.Where(d => d.InstanceName == name).FirstOrDefault();

        }


        public static QREventMSG EnumFactory( Type enumtype)
        {

            //first get the name of the module that this enum is from
            var fromModule = QRInitializing.GetModuleNameForType(enumtype);


            //get the QREnum attribute attached to this type
            var qrenum = (QREnum)enumtype.GetCustomAttributes(typeof(QREnum), false)[0];
            if (qrenum == null)
            {
                ProblemHandle prob = new ProblemHandle();
                prob.ThereisAProblem($"The enum type {enumtype.Name} does not have a QREnum attribute attached to it. This is required for this to be a ROS MSG");
                
            }

            //get the class name, if there was already a class with this name, then return that class 
            var evt = QREvent.AllAEEvents.Where(d => d.InstanceName == enumtype.Name).FirstOrDefault();
            if (evt != null)
            {
                return (QREventMSG)evt;
            }



            //grab all enum names for the enumtype
            var enumNames = enumtype.GetEnumNames();
            List<FunctionArgsBase> enumNamesList = new List<FunctionArgsBase>();
            //in c# get a typeof uint8


            enumNamesList.Add(new FunctionArgsBase(typeof(byte), "result")); 
            int count = 1;
            foreach (var item in enumNames)
            {
                enumNamesList.Add(new FunctionArgsBase(typeof(byte), item, count, true));
                count++;
            }



            var t = new QREventMSG(fromModule,enumtype.Name, enumNamesList);
            t.IsFromEnum = true;
            t.QREnumAttr = qrenum;
            return t;
        }

    }



}

