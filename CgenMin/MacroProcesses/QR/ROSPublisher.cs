using CodeGenerator.ProblemHandler;
using System.Diagnostics;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses.QR
{

    




    public  class ROSPublisher: ROSSubPub
    {

        public bool isdummy { get; protected set; } = false;

        public void SetAOIBelongTo(AONodeBase aoIBelongTo)
        {
            // Check if a publisher with the same IdName already exists
            string idName = GetIdName(this.Name, aoIBelongTo.ClassName, this.IsSubscribedToADifferentModule, DifferentModuleAOClassName);
            //go through the AllROSSubPub and report if there is a duplicate compared by IdName
            var duplicates = AllROSSubPub.OfType<ROSPublisher>().Where(s => (s.isdummy == false && s.AOIBelongTo != null))
         .GroupBy(item => item.IdName) // Group items by IdName
         .Where(group => group.Count() > 1) // Select groups with more than one item
         .Select(group => new { IdName = group.Key, Items = group.ToList() }) // Get duplicates
         .ToList();
             
            if (duplicates.Count > 0 )
            {
                return;
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem($"Publisher with the name {duplicates[0].IdName} under this class  already exists but you created two of them");
            }

            AOIBelongTo = aoIBelongTo;
        }

        public static void ResolveAnyDummyPublishers(ref List<ROSPublisher> listOfPublishers)
        { 

            //go through the list and find the dummy publisher that has the same idName as another publisher that is not a dummy
            foreach (var pub in listOfPublishers.Where(a=>a.isdummy))
            { 
                    string idName = GetIdName(pub.Name, pub.AOIBelongTo.ClassName, pub.IsSubscribedToADifferentModule, pub.DifferentModuleAOClassName);
                    var existingPublisher = listOfPublishers.OfType<ROSPublisher>().FirstOrDefault(p => p.IdName == idName && !p.isdummy);
                    if (existingPublisher != null)
                    {
                        //remove the dummy from the list
                        listOfPublishers.Remove(pub); 
                    } 
            }  
        }




        /// <param name="name">name if the pyublisher</param>
        /// <param name="fromClassOfName">name of the AO class that the publisher belongs to</param>
        /// <returns></returns>
        public static ROSPublisher CreatePublisherManualTopic(string name, QREventMSG msg,string manualTopicInput, int queueSize = 10)
        {
            Type aoType = AOReflectionHelper.GetCallingAONodeType();  
 

            //if this is a ros target msg, make sure the topic name follows QR conventions.
            if (msg.isNonQR)
            { 
                QREventMSGNonQR nonQRMsg = (QREventMSGNonQR)msg;
                if (nonQRMsg.IsRosMessage)
                {
                    //use reflection to get the class name of the instance this has been class from 
                    
                    nonQRMsg.FullTopicName = $"/{aoType.Name}/{name}" ;
                    
                }
            } 

            var pub = new ROSPublisher(name, msg, manualTopicInput, queueSize);
            pub._CallingAoNodeType = aoType;
            // pub.FULLTOPICNAME
            return pub;
        }     


        /// <param name="name">name if the pyublisher</param>
        /// <param name="fromClassOfName">name of the AO class that the publisher belongs to</param>
        /// <returns></returns>
        public static ROSPublisher CreatePublisher(string name, QREventMSG msg, bool hasInstanceNameInNameSpace, int queueSize = 10)
        {
            Type aoType = AOReflectionHelper.GetCallingAONodeType(); 

            //if hasInstanceNameInNameSpace is true but the msg does not have a AOIBelongTo, then make it set then give problem saying ot use the other overloaded create publisher function 
            if (hasInstanceNameInNameSpace && msg.InstanceName == null)
            {
                ProblemHandle problemHandle = new ProblemHandle();
                problemHandle.ThereisAProblem($"Cannot create publisher {name} with instance namespace because the message is null. Please use the other overloaded CreatePublisher function.");
            }

            //if this is a ros target msg, make sure the topic name follows QR conventions.
            if (msg.isNonQR)
            { 
                QREventMSGNonQR nonQRMsg = (QREventMSGNonQR)msg;
                if (nonQRMsg.IsRosMessage)
                {
                    //use reflection to get the class name of the instance this has been class from 
                    
                    nonQRMsg.FullTopicName =$"/{aoType.Name}/{name}" ;
                    
                }
            }


            var pub = new ROSPublisher(name, msg, hasInstanceNameInNameSpace, queueSize);
            pub._CallingAoNodeType = aoType;
            // pub.FULLTOPICNAME
            return pub;
        }     
        // public static ROSPublisher CreatePublisher(string name, QREventMSG msg, , int queueSize = 10)
        // {


        //     var pub = new ROSPublisher(name, msg, true, queueSize);
        //     //if the AO with the instance name given is not found, give a problem
        //     if (!AO.AllInstancesOfAO.Any(a => a.InstanceName == instanceNameOfAOPublisher))
        //     {
        //         ProblemHandle problemHandle = new ProblemHandle();
        //         problemHandle.ThereisAProblem($"Cannot create publisher {name} for AO instance {instanceNameOfAOPublisher} because that AO instance was not found.");
        //     } 
        //     pub.AOIBelongTo = (AONodeBase)AO.AllInstancesOfAO.Where(a => a.InstanceName == instanceNameOfAOPublisher).FirstOrDefault();
        //     return pub;
        // }
        
 

        /// <param name="name">name if the pyublisher</param>
        /// <param name="fromClassOfName">name of the AO class that the publisher belongs to</param>
        /// <returns></returns>
        public static ROSPublisher GetPublisher(string name, string fromClassOfName)
        {
            string idName = GetIdName(name, fromClassOfName, false, "");

            // Check if a publisher with the same IdName already exists
            var existingPublisher = _AllROSPub.OfType<ROSPublisher>().FirstOrDefault(pub => pub.IdName == idName);
            if (existingPublisher != null)
            {
                //if so, return that publisher
                return existingPublisher;
            }
            else
            {
                //otherwise create that publisher as a dummy for now until the real one is created and parameters filled in. 
                // if none are created but the time it is time to generate code, then there is a problem as the real publisher 
                //that this one subscribes to, never got created.
                return CreateDummyPublisher(name, fromClassOfName);

            }
             
        }
        

        public static ROSPublisher CreateDummyPublisher(string name, string fromClassOfName)
        {
            return new ROSPublisher(name, fromClassOfName);
        }

        protected ROSPublisher(string name, QREventMSG msg, bool giveNamespace, int queueSize = 10)
            : base(name, msg, giveNamespace, queueSize)
        {
            //check to see if the interface that this publisher uses is from a different module than the current running module
            if (IsSubscribedToADifferentModule)
            { 
                DifferentModuleAOClassName = msg.FromModuleName;
            }

            _AllROSPub.Add(this);
        }


                protected ROSPublisher(string name, QREventMSG msg, string manualTopicInput, int queueSize = 10)
            : base(name, msg, manualTopicInput, queueSize)
        {
            //check to see if the interface that this publisher uses is from a different module than the current running module
            if (IsSubscribedToADifferentModule)
            { 
                DifferentModuleAOClassName = msg.FromModuleName;
            }

            _AllROSPub.Add(this);
        }

 
        //constructor for dummy publisher
        protected ROSPublisher(string name, string fromClassOfName)
            : base(name,null, false, 10 )
        {
             isdummy = true;
        }

        public static ROSPublisher GetPublisherOfName(string name)
        {
            foreach (var pub in AllROSSubPub)
            {
                if (pub.Name == name)
                {
                    return (ROSPublisher)pub;
                }
            }
            //if none were found, give a problem
            ProblemHandle problemHandle = new ProblemHandle();
            problemHandle.ThereisAProblem($"Publisher with the name {name} was not found.");
            return null;
        }

        //create a private type that give the Type of Type callingAoNodeType = ROSPublisher.AOReflectionHelper.GetCallingAONodeType();
        private  Type _CallingAoNodeType;

        public string FULLTOPICNAME
        {
            get
            {

                if (this.ManualTopicInput != "")
                {
                    return this.ManualTopicInput;
                }

                if (this.MyQREventMSG.isNonQR)
                {

                    return  $"/{_CallingAoNodeType.Name}/{this.Name}" ; //((QREventMSGNonQR)this.MyQREventMSG).FullTopicName;
                }

                string ret = "";
                if (GiveInstanceNamespace)
                {
                    ret = $"{AOIBelongTo.ClassName}/{AOIBelongTo.InstanceName}/{TopicName()}";
                }
                else
                {
                    ret = $"{AOIBelongTo.ClassName}/{TopicName()}";
                }
                return ret;
            }
        }

        public string FULLTOPICNAME_IN_CGENMM
        {
            get
            {
                string topicName = "";

                if (this.MyQREventMSG.isNonQR)
                {
                    topicName =  $"/{_CallingAoNodeType.Name}/{this.Name}" ; //((QREventMSGNonQR)this.MyQREventMSG).FullTopicName;
                }
                else
                {
                    topicName = $"/{AOIBelongTo.ClassName}/{TopicName()}";
                }

                string ret = "";
                if (GiveInstanceNamespace)
                {
                    ret = $"this->id + \"{topicName}\"";
                }
                else
                {
                    ret = $"\"{topicName}\"";
                }
                return ret;
            }
        }




        public string PUBLISHER_DECLARE
        {
            get
            {
                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : QRInitializing.RunningProjectName;

                if (this.MyQREventMSG.isNonQR)
                {
                    return $"rclcpp::Publisher<{((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName}>::SharedPtr {Name};";
                }

                //rclcpp::Publisher<std_msgs::msg::String>::SharedPtr publisher_;
                return $"rclcpp::Publisher<{pubAoClassName}_i::msg::{MyQREventMSG.InstanceName}>::SharedPtr {Name};";
            }
        }
        public string PUBLISHER_DEFINE
        {
            get
            {
                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : QRInitializing.RunningProjectName;

                string fullheader = "";
                if (this.MyQREventMSG.isNonQR)
                {
                    fullheader = ((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName; 
                }
                else
                {
                    fullheader = $"{pubAoClassName}_i::msg::{MyQREventMSG.InstanceName}";

                }

                //publisher_ = this->create_publisher<std_msgs::msg::String>("output_topic", 10);
                return $"{Name} = this->create_publisher<{fullheader}>({FULLTOPICNAME_IN_CGENMM}, {QueueSize});";
            }
        }

        public string PUBLISHER_FUNCTION
        {
            get
            {
                //if this is an UNKOWNTYPE type, then we cannot generate the publish function
                if (this.MyQREventMSG.EventPropertiesList.Any(arg => arg.FunctionArgType == FunctionArgsType.UNKOWNTYPE))
                {
                    return $"// Publisher {Name} has unknown message type. Cannot generate publish function.";
                }


                // string FULLCLASSNAME = this.MyQREventMSG.isNonQR ?
                //     (((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName) :
                //     $"{QRInitializing.RunningProjectName}_i::msg::{MyQREventMSG.InstanceName}";

                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\PubsSubs\\PublisherFunction",
                   new MacroVar() { MacroName = "FULLCLASSNAME", VariableValue = MyQREventMSG.FULLCLASSNAME },
                   new MacroVar() { MacroName = "MODULENAME", VariableValue = QRInitializing.RunningProjectName },
                   new MacroVar() { MacroName = "NAME", VariableValue = this.Name },
                   new MacroVar() { MacroName = "MSGNAME", VariableValue = MyQREventMSG.InstanceName },


                   new MacroVar() { MacroName = "ARGS", VariableValue = MyQREventMSG.EventPropertiesList.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG, ",") },
                   new MacroVar() { MacroName = "ARG_FILL_REQUEST_DATAS", VariableValue = MyQREventMSG.EventPropertiesList.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG_FILL_REQUEST_DATA, "\n") }
                   );
                return ret;
            }
        }        
        public string PUBLISHER_FUNCTION2
        {
            get
            {  
                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : QRInitializing.RunningProjectName;

                string FULLCLASSNAME = this.MyQREventMSG.isNonQR ?
                    (((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName) : 
                    $"{pubAoClassName}_i::msg::{MyQREventMSG.InstanceName}";

                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\PubsSubs\\PublisherFunction2",
                   new MacroVar() { MacroName = "FULLCLASSNAME", VariableValue = FULLCLASSNAME }, 
                   new MacroVar() { MacroName = "NAME", VariableValue = this.Name }
                   );
                return ret;
            }
        }


        public static class AOReflectionHelper
{
    public static Type GetCallingAONodeType()
    {
        var stack = new StackTrace();

        foreach (var frame in stack.GetFrames())
        {
            var method = frame.GetMethod();
            if (method == null)
                continue;

            var declaringType = method.DeclaringType;
            if (declaringType == null)
                continue;

            // Walk inheritance chain
            var type = declaringType;
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition().Name.StartsWith("AONode"))
                {
                    return declaringType; // THIS is the AO class
                }

                type = type.BaseType;
            }
        }

        return null;
    }
}


    }
}
