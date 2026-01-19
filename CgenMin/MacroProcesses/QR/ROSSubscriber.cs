using CodeGenerator.ProblemHandler;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses.QR
{
    public static class ROSPublisherExtensions 
    {
        public static string GenerateAllForEvery_Arguments<T>(this List<T> Args, Func<T, string> functionDelegate, string delimiter)
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

        public static string FULLTOPICNAME(ROSSubscriber rosSubPub)
        {
            return rosSubPub.FULLTOPICNAME;
        }
        public static string FULLTOPICNAME(ROSPublisher rosSubPub)
        {
            return rosSubPub.FULLTOPICNAME;
        }

        public static string FULLTOPICNAME_IN_CGENMM(ROSSubscriber rosSubPub)
        {
            return rosSubPub.FULLTOPICNAME_IN_CGENMM;
        }       
        
        public static string FULLTOPICNAME_IN_CGENMM(ROSPublisher rosSubPub)
        {
            return rosSubPub.FULLTOPICNAME_IN_CGENMM;
        }

        public static string SUBPUB_HEADER(ROSSubPub rosSubPub)
        { 

            return rosSubPub.SUBPUB_HEADER;
        }
        

        public static string SUBSCRIBER_DECLARE(ROSSubscriber rosSubPub)
        {
            return rosSubPub.SUBSCRIBER_DECLARE;
        }

        public static string SUBSCRIBER_DEFINE(ROSSubscriber rosSubPub)
        {
            return rosSubPub.SUBSCRIBER_DEFINE;
        }        
        
        public static string SUBSCRIBER_FUNCTION_CALLBACK(ROSSubscriber rosSubPub)
        {
            return rosSubPub.SUBSCRIBER_FUNCTION_CALLBACK;
        }
         



        public static string PUBLISHER_DECLARE(ROSPublisher rosSubPub)
        {
            return rosSubPub.PUBLISHER_DECLARE;
        }        
        public static string PUBLISHER_DEFINE(ROSPublisher rosSubPub)
        {
            return rosSubPub.PUBLISHER_DEFINE;
        }
        public static string PUBLISHER_FUNCTION(ROSPublisher rosSubPub)
        {
            return rosSubPub.PUBLISHER_FUNCTION;
        }
        public static string PUBLISHER_FUNCTION2(ROSPublisher rosSubPub)
        {
            return rosSubPub.PUBLISHER_FUNCTION2;
        }

    }



    public abstract class ROSSubPub
    {
        protected bool isSub;
        //private string _name;
        public string Name { get; } 
                //    string sp = isSub ? "s" : "p";
                //return $"{_name}_{sp}ub"; } }
        
        public bool GiveInstanceNamespace { get; }
        public int QueueSize { get; }
        public AONodeBase AOIBelongTo { get; protected set; }
        public QREventMSG MyQREventMSG { get; set; }
                protected bool IsSubscribedToADifferentModule
        {
            get
            {
                return MyQREventMSG.FromModuleName != QRInitializing.RunningProjectName;
            }
        } //= false;
        protected string DifferentModuleAOClassName = "";

        protected static List<ROSSubscriber> _AllROSSub = new List<ROSSubscriber>();
        protected static List<ROSPublisher> _AllROSPub = new List<ROSPublisher>();
        protected static List<ROSSubPub> _AllROSSubPub = new List<ROSSubPub>();

        //create a list of all ROSSubPub that are created
        public static List<ROSSubPub> AllROSSubPub { get{
                _AllROSSubPub.Clear();
                _AllROSSubPub.AddRange(_AllROSSub.Cast<ROSSubPub>());
                _AllROSSubPub.AddRange(_AllROSPub.Cast<ROSSubPub>());
                return _AllROSSubPub;
            }
        }


        public static void Reset()
        {
            _AllROSSub.Clear();
            _AllROSPub.Clear(); 
        }

        public void SetAOIBelongTo(AONodeBase aoIBelongTo)
        { 
            AOIBelongTo = aoIBelongTo;
        }



        protected string ManualTopicInput = "";
        public ROSSubPub(string name, QREventMSG msg, string  manualTopicInput, int queueSize)
        {
            ManualTopicInput = manualTopicInput;
            Name = name;// msg.InstanceName;
            GiveInstanceNamespace = false;
            MyQREventMSG = msg;
            QueueSize = queueSize;
            _AllROSSubPub = new List<ROSSubPub>();  
        }

        public ROSSubPub(string name, QREventMSG msg, bool giveInstanceNamespace, int queueSize)
        {
            Name = name;// msg.InstanceName;
            GiveInstanceNamespace = giveInstanceNamespace;
            MyQREventMSG = msg;
            QueueSize = queueSize;
            _AllROSSubPub = new List<ROSSubPub>();  
        }
        public ROSSubPub(string name, string fromADifferentModuleAOClassName, QREventMSG msg, bool giveInstanceNamespace, int queueSize)
        : this(name, msg, giveInstanceNamespace, queueSize)
        { 
            DifferentModuleAOClassName = fromADifferentModuleAOClassName; 
        }

        public string IdName { get
            {
                return GetIdName(Name, AOIBelongTo.ClassName, this.IsSubscribedToADifferentModule,   DifferentModuleAOClassName);

            }
        }

        //classname is from the class that this publisher belongs to
        public static string GetIdName(string name, string className, bool isFromDifferentModule, string differentModuleAOClassName)
        {
            //if this is from a different module, then the className will be the name that the user passes to in. 
            if(isFromDifferentModule)
            {
                return $"{differentModuleAOClassName}/{name}";
            }

            return $"{className}/{name}"; 
        }

        public string SUBPUB_HEADER
        {
            get
            {
                return MyQREventMSG.INTERFACE_HEADER();
            }
        }
         
        public virtual string TopicName()
        {  
             return Name; 
        }

        public virtual string TopicNameSpace()
        {
            return AOIBelongTo.InstanceName;
        }
    }



    public class ROSSubscriber : ROSSubPub
    {


        public ROSPublisher PubToSubTo { get; }
        public string Namespace_AOInstanceName { get; }




        public static ROSSubscriber CreateSubscriberNonQR(string name, QREventMSGNonQR nonQRMsg, int queueSize = 10)
        {
            var sub = new ROSSubscriber(name, nonQRMsg, queueSize);
            return sub;
        }

        // Protected constructor for NonQR messages
  


        /// <param name="name"></param>
        /// <param name="pubToSubTo"></param>
        /// <param name="namespace_AOInstanceName">this is the name of the instance that you will subscribe to if the publisher you are subscribing to is using a namespace</param>
        /// <param name="queueSize"></param>
        public static ROSSubscriber CreateSubscriber(string name, ROSPublisher pubToSubTo, string namespace_AOInstanceName = "", int queueSize = 10)
        {
            //create the publisher  
            var sub = new ROSSubscriber(name, pubToSubTo, namespace_AOInstanceName, queueSize);
            return sub;
        }
        
        public static ROSSubscriber CreateSubscriber<AOType>(string subname, string pubname, QREventMSG msg,  string namespace_AOInstanceName = "", int queueSize = 10)
            where AOType : AONodeBase
        {
            //create an instance of the AO from the other node
            // FromAOT aoInst = Activator.CreateInstance<FromAOT>();
            // ROSPublisher.CreatePublisher("PointCloud2", livoxmock.pointcloud2msg, true),
            var tt = ROSPublisher.CreatePublisher(pubname, msg, true);
            //get the string of the AOType 
            string otherModuleAOClassName = typeof(AOType).Name;

 
 
            var sub = new ROSSubscriber(  subname, otherModuleAOClassName,  tt,   namespace_AOInstanceName ,   queueSize  );
            return sub;
        }


        public static ROSSubscriber CreateSubscriberManualTopicName (string subname,  QREventMSG msg,  string manualTopicInput, int queueSize = 10)
        {
            //create an instance of the AO from the other node
            // FromAOT aoInst = Activator.CreateInstance<FromAOT>();
            // ROSPublisher.CreatePublisher("PointCloud2", livoxmock.pointcloud2msg, true),
            //var tt = ROSPublisher.CreatePublisherManualTopic(pubname, msg, manualTopicInput, queueSize);
            //get the string of the AOType 
            // string otherModuleAOClassName = typeof(AOType).Name;

  
            var sub = new ROSSubscriber(  subname, msg, manualTopicInput ,   queueSize  );
            return sub; 
        }

         
      protected ROSSubscriber(string name, QREventMSGNonQR nonQRMsg, int queueSize = 10)
            : base(name, nonQRMsg, false, queueSize)
        {
            PubToSubTo = null; // no publisher linkage for non-QR
            Namespace_AOInstanceName = "";
            _AllROSSub.Add(this);
        }


        //manual topic name input
        protected ROSSubscriber(string name,    QREventMSG msg,  string manualTopicInput = "", int queueSize = 10)
            : base(name, msg, manualTopicInput  , queueSize)
        {
            PubToSubTo = null;
            Namespace_AOInstanceName = manualTopicInput;

            _AllROSSub.Add(this);
        }


 
        /// <param name="name"></param>
        /// <param name="pubToSubTo"></param>
        /// <param name="namespace_AOInstanceName">this is the name of the instance that you will subscribe to if the publisher you are subscribing to is using a namespace</param>
        /// <param name="queueSize"></param>
        protected ROSSubscriber(string name, ROSPublisher pubToSubTo, string namespace_AOInstanceName = "", int queueSize = 10)
            : base(name, pubToSubTo.MyQREventMSG, namespace_AOInstanceName != "", queueSize)
        {
            PubToSubTo = pubToSubTo;
            Namespace_AOInstanceName = namespace_AOInstanceName;

            _AllROSSub.Add(this);
        }
        protected ROSSubscriber(string name, string otherModuleAOClassName, ROSPublisher pubToSubTo, string namespace_AOInstanceName = "", int queueSize = 10)
            : base(name,otherModuleAOClassName, pubToSubTo.MyQREventMSG, namespace_AOInstanceName != "", queueSize)
        {
            PubToSubTo = pubToSubTo;
            Namespace_AOInstanceName = namespace_AOInstanceName;

            _AllROSSub.Add(this);
        }

        public override string TopicName()
        {
            //if (this.MyQREventMSG.isNonQR)
            //{ 
            //    return ((QREventMSGNonQR)this.MyQREventMSG).FullTopicName;
            //}
            return PubToSubTo.Name;
        } 

        public string FULLTOPICNAME
        {
            get
            {
                if (this.ManualTopicInput != "")
                {
                    return this.ManualTopicInput;
                }

                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.DifferentModuleAOClassName : PubToSubTo.AOIBelongTo.ClassName;

                if (this.MyQREventMSG.isNonQR)
                {

                    return  ((QREventMSGNonQR)this.MyQREventMSG).FullTopicName ;
                }

                string ret = "";
                if (GiveInstanceNamespace)
                {
                    ret = $"{Namespace_AOInstanceName}/{pubAoClassName}/{TopicName()}";
                }
                else    
                {
                    ret = $"{pubAoClassName}/{TopicName()}";
                }
                return ret;
            }
        }

        public string FULLTOPICNAME_IN_CGENMM
        {
            get
            { 
                return FULLTOPICNAME;
            }
        }



        public string SUBSCRIBER_DECLARE
        {
            get
            {
                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : PubToSubTo.AOIBelongTo.ClassName;

                if (this.MyQREventMSG.isNonQR)
                {
                    return $"rclcpp::Subscription<{((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName}>::SharedPtr {Name};";
                }

                // rclcpp::Subscription<std_msgs::msg::String>::SharedPtr subscription_;
                return $"rclcpp::Subscription<{pubAoClassName}_i::msg::{MyQREventMSG.InstanceName}>::SharedPtr {Name};";
            }
        }
        public string SUBSCRIBER_DEFINE
        {
            get
            {                
                string pubAoClassName = this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : PubToSubTo.AOIBelongTo.ClassName;

                string fullheader = "";
                if (this.MyQREventMSG.isNonQR)
                {
                    fullheader = ((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName;
                }
                else
                {
                    fullheader = $"{pubAoClassName}_i::msg::{MyQREventMSG.InstanceName}";
                }


                // subscription_ = this->create_subscription<std_msgs::msg::String>(
                // "topic",
                // 10, std::bind(&WorldNode::topic_callback, this, std::placeholders::_1));
                string ret = "";
                ret += $"{Name} = this->create_subscription<{fullheader}> ";
                ret += $"(\"{FULLTOPICNAME_IN_CGENMM}\",";
                ret += $"{QueueSize.ToString()}, std::bind(&{AOIBelongTo.ClassName}Node::{Name}_callback, this, std::placeholders::_1));";
                return ret;
            }
        }


        public string SUBSCRIBER_FUNCTION_CALLBACK
        {
            get
            {
                string fromModuleName = this.MyQREventMSG.FromModuleName+ "_i";//this.IsSubscribedToADifferentModule ? this.MyQREventMSG.FromModuleName : PubToSubTo.AOIBelongTo.ClassName;
                string msgName = MyQREventMSG.InstanceName;

                if (this.MyQREventMSG.isNonQR)
                {
                    // if (((QREventMSGNonQR)this.MyQREventMSG).IsRosMessage == false)
                    // {
                        //get the first part of FullClassName sensor_msgs::msg::PointCloud2. set that in fromModuleName. get last part and set that in msgname
                        fromModuleName = ((QREventMSGNonQR)this.MyQREventMSG).FULLCLASSNAME.Split("::")[0];
                        msgName = ((QREventMSGNonQR)this.MyQREventMSG).FULLCLASSNAME.Split("::")[2];
                    // }
                }

                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\PubsSubs\\SubscriberFunctionCallback",
           new MacroVar() { MacroName = "MODULENAME", VariableValue =fromModuleName},
           new MacroVar() { MacroName = "NAME", VariableValue = this.Name },
           new MacroVar() { MacroName = "MSGNAME", VariableValue = msgName }
           );
                return ret;
            }
        }

    }
}

