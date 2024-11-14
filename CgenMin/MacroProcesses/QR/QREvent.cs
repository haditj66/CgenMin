using CgenMin.MacroProcesses.QR;
using CodeGenerator.ProblemHandler;
using System.Collections.Generic;
using System.Linq;
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



    public enum FunctionArgsType
    {
        PrimitiveType,
        SurrogateAO,
        EnumType,
        AnotherMSG //this is if the argument is of a type that is of another MSG
    }

  

    public class FunctionArgs
    {




        public string Name { get; set; }
        public int Argnum { get; }
        public bool IsFromEnumArg { get; }
        protected Type Type;
        public string TypeName { get
            {
                string ret = CsharpTypeToCppType(Type.Name);
                //if this is from a surrogate AO, then Base appended to the type name
                if (this.FunctionArgType == FunctionArgsType.SurrogateAO)
                {
                    ret += "Base*";
                }
                return ret;
            }
        }
        public FunctionArgsType _FunctionArgType;
        public FunctionArgsType FunctionArgType
        {
            get
            {
                //figure out wether the type is either a primitive type, a surrogate AO or an enum type. a surrogate AO is a type that is derived from AOSurrogatePattern 
                if (this.Type.IsPrimitive)
                {
                    this._FunctionArgType = FunctionArgsType.PrimitiveType;
                }
                else if (Type.IsEnum)
                {
                    this._FunctionArgType = FunctionArgsType.EnumType;

                    //if this is an enum type, create a QREventMSG for it. but first check if it exists first
                    QREventMSG.EnumFactory(this.Type);

                }
                //else if (this.Type.IsSubclassOf(typeof(AOSurrogatePatternBase)))
                else if (this.Type.BaseType.Name.Contains("AOSurrogatePattern"))
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
            ret = ret == "Single" ? "float" : ret;
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

        public string NAMEASINSERVICE()
        {
            return STR_to_NAMEASINSERVICE(this.FunctionArgType == FunctionArgsType.SurrogateAO, Name); 

        }
        public string TYPEASINSERVICE()
        {
            return STR_to_TYPEASINSERVICE(this.FunctionArgType == FunctionArgsType.SurrogateAO, this.Type.Name); 
        }

        public static string STR_to_NAMEASINSERVICE(bool isSurrogate, string toConvert)
        {
            string ret = isSurrogate ? "id" : toConvert;
            return ret;
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

        public FunctionArgs(Type type, string name, int argnum = 0, bool isFromEnumArg = false)
        {
            Type = type;
            Name = name;
            Argnum = argnum;
            IsFromEnumArg = isFromEnumArg;
            var s = FunctionArgType;
        }
    }


    public enum QREventType
    {
        MSG,
        SRV,
        ACTION
    }

    public abstract class QREvent : AOWritableToAOClassContents
    {
        public QREventType QREVTType { get; private set; }
        public FunctionArgs[] EventProperties { get; protected set; }
        public FunctionArgs ReturnType { get; protected set; }
        //public string ReturnName { get; protected set; }
        public bool IsFromEnum { get; protected set; }
        public int EventPoolSize { get; protected set; }
        public QREnum QREnumAttr { get; set; }

        public static int NumOfSRVCreatedSoFar { get { return numOfSRVCreatedSoFar; } }
        static int numOfSRVCreatedSoFar = 0;
        static int numOfMSGsCreatedSoFar = 0;
        int eventID = 0;
        int sigID = 0;

        public static List<QREvent> AllAEEvents = new List<QREvent>();
        public static List<QREvent> AllSelectedTargetEvents = new List<QREvent>();
        protected static bool isTargetStarted = false;
        public QREvent(string ClassName, QREventType qRType, params FunctionArgs[] eventDefinition)
            : base(QRInitializing.RunningProjectName, ClassName.First().ToString().ToUpper() + ClassName.Substring(1), AOTypeEnum.Event)
        {
            //uppercase the first letter of the class name 

            EventProperties = eventDefinition;
            Constr(qRType);
        }

        public QREvent(string ClassName, QREventType qRType, List<FunctionArgs> eventDefinition)
            : base(QRInitializing.RunningProjectName, ClassName.First().ToString().ToUpper() + ClassName.Substring(1), AOTypeEnum.Event)
        {
            //this(ClassName,   returnType,   returnName,  eventProperties.ToArray());
            EventProperties = eventDefinition.ToArray();
            Constr(qRType);
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

            //dont add the updateEvt
            if (this.ClassName != "UpdateEVT")
            {
                if (QREventType.MSG == QREVTType)
                {
                    numOfMSGsCreatedSoFar++;
                    sigID = numOfMSGsCreatedSoFar;
                    AllAEEvents.Add(this);
                }
                else
                {
                    numOfSRVCreatedSoFar++;
                    eventID = numOfSRVCreatedSoFar;
                    AllAEEvents.Add(this);
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

        public override string GenerateMainHeaderSection_CP()
        {
            return "";
        }

        public override string GenerateMainInitializeSection_CP()
        {
            return "";
        }

        public override string GenerateMainHeaderSection_RQT()
        {
            return "";
        }

        public override string GenerateMainInitializeSection_RQT()
        {
            return "";
        }

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



        protected AEEventBase(string ClassName, QREventType qrType, params FunctionArgs[] eventProperties)
            : base(ClassName, qrType, eventProperties)
        {
        }

        protected AEEventBase(string ClassName, QREventType qrType, List<FunctionArgs> eventProperties)
            : base(ClassName, qrType, eventProperties)
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

        public QREventSRV(string ClassName, FunctionArgs returnType, params FunctionArgs[] eventProperties)
         : base(ClassName, QREventType.SRV, eventProperties)
        {
            AO.atLeastOneEvt = true;
            ReturnType = returnType;
        }

        public QREventSRV(string ClassName, FunctionArgs returnType, List<FunctionArgs> eventProperties)
    : base(ClassName, QREventType.SRV, eventProperties)
        {
            AO.atLeastOneEvt = true;
            ReturnType = returnType;
        }

    }

    //    public class QREventMSG<TDerived> : AEEventBase<TDerived>
    //where TDerived : AEEventBase<TDerived>, new()
    public class QREventMSG : AEEventBase
    {
        public QREventMSG(string ClassName, params FunctionArgs[] eventProperties)
            : base(ClassName, QREventType.MSG, eventProperties)
        {
        }

        public QREventMSG(string ClassName, List<FunctionArgs> eventProperties)
            : base(ClassName, QREventType.MSG, eventProperties)
        {
        }


        public static QREventMSG EnumFactory(Type enumtype)
        {


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
            List<FunctionArgs> enumNamesList = new List<FunctionArgs>();
            //in c# get a typeof uint8


            enumNamesList.Add(new FunctionArgs(typeof(byte), "result")); 
            int count = 1;
            foreach (var item in enumNames)
            {
                enumNamesList.Add(new FunctionArgs(typeof(byte), item, count, true));
                count++;
            }



            var t = new QREventMSG(enumtype.Name, enumNamesList);
            t.IsFromEnum = true;
            t.QREnumAttr = qrenum;
            return t;
        }

    }



}
