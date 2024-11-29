using CodeGenerator.ProblemHandler;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses.QR
{

    




    public  class ROSPublisher: ROSSubPub
    {

        public bool isdummy { get; protected set; } = false;

        public void SetAOIBelongTo(AONodeBase aoIBelongTo)
        {
            // Check if a publisher with the same IdName already exists
            string idName = GetIdName(this.Name, aoIBelongTo.ClassName);
            //go through the AllROSSubPub and report if there is a duplicate compared by IdName
            var duplicates = AllROSSubPub.OfType<ROSPublisher>().Where(s => (s.isdummy == false && s.AOIBelongTo != null))
         .GroupBy(item => item.IdName) // Group items by IdName
         .Where(group => group.Count() > 1) // Select groups with more than one item
         .Select(group => new { IdName = group.Key, Items = group.ToList() }) // Get duplicates
         .ToList();
             
            if (duplicates.Count > 0 )
            {
                 
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
                    string idName = GetIdName(pub.Name, pub.AOIBelongTo.ClassName);
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
        public static ROSPublisher CreatePublisher(string name, QREventMSG msg, bool hasInstanceNameInNameSpace, int queueSize = 10)
        {
            var pub = new ROSPublisher(name, msg, hasInstanceNameInNameSpace, queueSize); 
            return pub;
        }
         
        /// <param name="name">name if the pyublisher</param>
        /// <param name="fromClassOfName">name of the AO class that the publisher belongs to</param>
        /// <returns></returns>
        public static ROSPublisher GetPublisher(string name, string fromClassOfName)
        {
            string idName = GetIdName(name, fromClassOfName);

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


        protected static ROSPublisher CreateDummyPublisher(string name, string fromClassOfName)
        {
            return new ROSPublisher(name, fromClassOfName);
        }

        protected ROSPublisher(string name, QREventMSG msg, bool giveNamespace, int queueSize = 10)
            : base(name, msg, giveNamespace, queueSize)
        {
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
            return null;
        }

        public string FULLTOPICNAME
        {
            get
            {
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
                string ret = "";
                if (GiveInstanceNamespace)
                {
                    ret = $"this->id + \"/{AOIBelongTo.ClassName}/{TopicName()}\"";
                }
                else
                {
                    ret = $"\"{AOIBelongTo.ClassName}/{TopicName()}\"";
                }
                return ret;
            }
        }




        public string PUBLISHER_DECLARE
        {
            get
            {
                //rclcpp::Publisher<std_msgs::msg::String>::SharedPtr publisher_;
                return $"rclcpp::Publisher<{QRInitializing.RunningProjectName}_i::msg::{MyQREventMSG.InstanceName}>::SharedPtr {Name};";
            }
        }
        public string PUBLISHER_DEFINE
        {
            get
            { 
                //publisher_ = this->create_publisher<std_msgs::msg::String>("output_topic", 10);
                return $"{Name} = this->create_publisher<{QRInitializing.RunningProjectName}_i::msg::{MyQREventMSG.InstanceName}>({FULLTOPICNAME_IN_CGENMM}, {QueueSize});";
            }
        }

        public string PUBLISHER_FUNCTION
        {
            get
            {
                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\PubsSubs\\PublisherFunction",
                   new MacroVar() { MacroName = "MODULENAME", VariableValue = QRInitializing.RunningProjectName },
                   new MacroVar() { MacroName = "NAME", VariableValue = this.Name },
                   new MacroVar() { MacroName = "MSGNAME", VariableValue = MyQREventMSG.InstanceName },

                   
                   new MacroVar() { MacroName = "ARGS", VariableValue = MyQREventMSG.EventPropertiesList.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG, ",") },
                   new MacroVar() { MacroName = "ARG_FILL_REQUEST_DATAS", VariableValue = MyQREventMSG.EventPropertiesList.GenerateAllForEvery_Arguments(FunctionArgsBaseExtension.ARG_FILL_REQUEST_DATA, "\n") }
                   );
                return ret;
            }
        }
    }
}
