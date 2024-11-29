using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CgenMin.MacroProcesses.QR
{

    public class ROSTimer
    {
        public string NameOfTimer { get; }
        public int PeriodInMillisec { get; }
        public bool IsForSurrogate { get; set; }
        public AONodeBase AOIBelongTo { get; set; }

        public ROSTimer(string nameOfTimer, int periodInMillisec )
        {
            NameOfTimer = nameOfTimer;
            PeriodInMillisec = periodInMillisec; 
        }




        public static string TIMER_DECLARES(List<ROSTimer> rOSTimers)  
        {
            string ret = "";
            foreach (var timer in rOSTimers)
            {
                ret += timer.TIMER_DECLARE + "\n";
            }
            return ret; 
        }
        public string TIMER_DECLARE { get 
            {
                return $"rclcpp::TimerBase::SharedPtr {NameOfTimer}; ";
            }
        }

        public static string TIMER_DEFINES(List<ROSTimer> rOSTimers)
        {
            string ret = "";
            foreach (var timer in rOSTimers)
            {
                ret += timer.TIMER_DEFINE + "\n";
            }
            return ret;
        }

        public string TIMER_DEFINE
        {
            get
            { 
                //timer_ = this->create_wall_timer(  3s, std::bind(&GenericGobjAO::timer_callback, this));
                return $"{NameOfTimer} = this->create_wall_timer(  {PeriodInMillisec.ToString()}ms, std::bind(&{AOIBelongTo.ClassName}Node::{NameOfTimer}_callback, this));";
            }
        }


        public static string TIMER_FUNC_IMPS(List<ROSTimer> rOSTimers)
        {
            string ret = "";
            foreach (var timer in rOSTimers)
            {
                ret += timer.TIMER_FUNC_IMP + "\n";
            }
            return ret;
        }
        public string TIMER_FUNC_IMP
        {
            get
            {
                string surrogateTimerRet = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\SurrogatePattern\\Timer_Func_Imp_Surrogate",
                     new MacroVar() { MacroName = "NameOfTimer", VariableValue = NameOfTimer }  
                    );

                string ret = QRInitializing.TheMacro2Session.GenerateFileOut("QR\\Timer_Func_Imp",
                    new MacroVar() { MacroName = "NameOfTimer", VariableValue = NameOfTimer } 
                    );
                return IsForSurrogate ? surrogateTimerRet  : ret;
            }
        }


    }
}
