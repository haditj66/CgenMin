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

        public ROSSubPub(string name, QREventMSG msg, bool giveInstanceNamespace, int queueSize)
        {
            Name = name;// msg.InstanceName;
            GiveInstanceNamespace = giveInstanceNamespace;
            MyQREventMSG = msg;
            QueueSize = queueSize;
            _AllROSSubPub = new List<ROSSubPub>(); 

        }


        public string IdName { get
            {
                return GetIdName(Name, AOIBelongTo.ClassName);

            }
        }

        //classname is from the class that this publisher belongs to
        public static string GetIdName(string name, string className)
        {
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

         
        /// <param name="name"></param>
        /// <param name="pubToSubTo"></param>
        /// <param name="namespace_AOInstanceName">this is the name of the instance that you will subscribe to if the publisher you are subscribing to is using a namespace</param>
        /// <param name="queueSize"></param>
        public static ROSSubscriber CreateSubscriber(string name, ROSPublisher pubToSubTo, string namespace_AOInstanceName = "", int queueSize = 10)
        {
            var sub = new ROSSubscriber(  name,   pubToSubTo,   namespace_AOInstanceName ,   queueSize  );
            return sub;
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
                if (this.MyQREventMSG.isNonQR)
                {

                    return  ((QREventMSGNonQR)this.MyQREventMSG).FullTopicName ;
                }

                string ret = "";
                if (GiveInstanceNamespace)
                {
                    ret = $"{Namespace_AOInstanceName}/{PubToSubTo.AOIBelongTo.ClassName}/{TopicName()}";
                }
                else
                {
                    ret = $"{PubToSubTo.AOIBelongTo.ClassName}/{TopicName()}";
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
                if (this.MyQREventMSG.isNonQR)
                {
                    return $"rclcpp::Subscription<{((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName}>::SharedPtr {Name};";
                }

                // rclcpp::Subscription<std_msgs::msg::String>::SharedPtr subscription_;
                return $"rclcpp::Subscription<{QRInitializing.RunningProjectName}_i::msg::{MyQREventMSG.InstanceName}>::SharedPtr {Name};";
            }
        }
        public string SUBSCRIBER_DEFINE
        {
            get
            {
                string fullheader = "";
                if (this.MyQREventMSG.isNonQR)
                {
                    fullheader = ((QREventMSGNonQR)this.MyQREventMSG).FullMsgClassName;
                }
                else
                {
                    fullheader = $"{QRInitializing.RunningProjectName}_i::msg::{MyQREventMSG.InstanceName}";
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
                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\PubsSubs\\SubscriberFunctionCallback",
           new MacroVar() { MacroName = "MODULENAME", VariableValue = QRInitializing.RunningProjectName },
           new MacroVar() { MacroName = "NAME", VariableValue = this.Name },
           new MacroVar() { MacroName = "MSGNAME", VariableValue = MyQREventMSG.InstanceName }
           );
                return ret;
            }
        }

    }
}

