//using CodeGenerator.MacroProcesses.AESetups;
using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
using CodeGenerator.ProblemHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CgenMin.MacroProcesses
{


    public enum StyleOfSPB
    {
        // look at file AESPBObservor.cpp at funciton _RefreshCheckStyle() for where this implementation makes a difference. 
        EachSPBTask,  //each spb has its own task that it will use to execute its refresh
                      //TODO:ChainOfSPBsTask, // there is one "chain task" that will run all refreshs
                      //there are no tasks involved and so everything is run within the interrupt(although doesnt need to be an interrupt, can also just be from a normal tick() of a clock).
                      //Currently this doesnt look to be any different than the 
                      //ChainOfSPBsTask so maybe use this for now intead of that one. Remeber that if you do infact use this in an interrupt, it should be a VERY quick spb
        ChainOfSPBsFromInterrupt

    }
;

    public enum AOTypeEnum
    {
        AOSimple,
        AOSurrogatePattern,  
        SimpleFSM,
        Event
    }



    public interface IGeneratedInMain
    {
        public string AO_DESCRIPTIONS_CP();
        public  string AO_MAINHEADER_CP();
        public  string AO_DECLARES_CP();
        public  string AO_DEFINE_COMMENTS_CP();
        public  string AO_MAININIT_CP();


        public string AO_DESCRIPTIONS_RQT();
        public  string AO_MAINHEADER_RQT();
        public  string AO_DECLARES_RQT();
        public  string AO_DEFINE_COMMENTS_RQT();
        public  string AO_MAININIT_RQT();
    }
    


    public abstract class AO
    {

        protected static List<string> listOfAdditionalIncludes = new List<string>();
        protected string GetAdditionalIncludeInAEConfig(string fileNameWithoutTheExt)
        {
            //return only if this file has not been included yet
            var alreadyIncluded = listOfAdditionalIncludes.Where(a => a == fileNameWithoutTheExt).FirstOrDefault();

            if (alreadyIncluded == null)
            {
                listOfAdditionalIncludes.Add(fileNameWithoutTheExt);

                return $"#define AnyOtherNeededIncludes{listOfAdditionalIncludes.Count.ToString()} {fileNameWithoutTheExt} \n";
            }
            else
            {
                return "";
            }

        }

        public static bool atLeastOneEvt = false;

        public static List<AO> AllInstancesOfAO = new List<AO>();
        public AOTypeEnum AOType { get; protected set; }
        public string ClassName;
        public string InstanceName;
        public static int numOfAOSoFarAEConfigGenerated { get { return _numOfAOSoFarAEConfigGenerated; } protected set { _numOfAOSoFarAEConfigGenerated = value; } }
        protected static int _numOfAOSoFarAEConfigGenerated = 0;

        public AO(string instanceName )
        {
            ClassName = GetType().Name;

            InstanceName = instanceName.Trim(); 

            if (this.ClassName != "UpdateEVT")
            {
                AllInstancesOfAO.Add(this);
            }
        }


        //public static List<T> GetAllAOOfType<T>() where T : AO
        //{
        //    if (typeof(T) == typeof(AESensor))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.Sensor).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AEFilter))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.Filter).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AESPBBase))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.SPB).Cast<T>().ToList();
        //        return tt;
        //    }
        //    if (typeof(T) == typeof(AEUtilityService))
        //    {
        //        List<T> tt = AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.UtilityService).Cast<T>().ToList();
        //        return tt;
        //    }

        //    return null;
        //}


        //the part that goes into the header of the main.cpp file. stuff like callback function declarations static void clockTimer1(TimerHandle_t xTimerHandle);

        


        protected static string _All_Generate(Func<IGeneratedInMain, string> generateSectionFunc)
        {
            string ret = "";
            foreach (var ao in AllInstancesOfAO)
            {
                if (ao is IGeneratedInMain aao)
                {
                    //IGeneratedInMain aao = (IGeneratedInMain)ao;
                    string rett = generateSectionFunc(aao) + "\n";
                    string retttrim = rett.Trim();
                    ret = retttrim == "" ? ret + "" : ret + rett;
                }
             
            }

            return ret;
        }

        public static string All_GenerateAO_DESCRIPTIONS_CP()
        {
            return _All_Generate(ao => ao.AO_DESCRIPTIONS_CP());
        }
        public static string All_GenerateAO_DESCRIPTIONS_RQT()
        {
            return _All_Generate(ao => ao.AO_DESCRIPTIONS_RQT());
        }

        public static string All_GenerateAO_MAINHEADER_CP()
        {
            return _All_Generate(ao => ao.AO_MAINHEADER_CP());
        }
        public static string All_GenerateAO_DECLARES_CP()
        {
            return _All_Generate(ao => ao.AO_DECLARES_CP());
        }

        public static string All_GenerateAO_DEFINE_COMMENTS_CP()
        {
            return _All_Generate(ao => ao.AO_DEFINE_COMMENTS_CP());
        }  
        
        public static string All_GenerateAO_MAININIT_CP()
        {
            return _All_Generate(ao => ao.AO_MAININIT_CP());
        }
         
        public static string All_GenerateAO_MAINHEADER_RQT()
        {
            return _All_Generate(ao => ao.AO_MAINHEADER_RQT());
        }
        public static string All_GenerateAO_DECLARES_RQT()
        {
            return _All_Generate(ao => ao.AO_DECLARES_RQT());
        }

        public static string All_GenerateAO_DEFINE_COMMENTS_RQT()
        {
            return _All_Generate(ao => ao.AO_DEFINE_COMMENTS_RQT());
        }

        public static string All_GenerateAO_MAININIT_RQT()
        {
            return _All_Generate(ao => ao.AO_MAININIT_RQT());
        }




        public static string All_GenerateAO_SURROGATE_INIT_RQT()
        {
            //go through all AllInstancesOfAO and get the ones of type surrogatepattern
            string ret = "";
            foreach (var item in AllInstancesOfAO)
            {
                if (item.AOType == AOTypeEnum.AOSurrogatePattern)
                {
                    string rett = ((ISurrogateAO)item).GenerateAO_SURROGATE_INIT_RQT() + "\n";
                    string retttrim = rett.Trim();
                    ret = retttrim == "" ? ret + "" : ret + rett;

                }
            }
            return ret;

        }




        //public static string All_GenerateMainClockSetupsSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateMainClockSetupsSection();
        //    }
        //    return ret;
        //}
        //public static string All_GenerateMainLinkSetupsSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateMainLinkSetupsSection();
        //    }
        //    ret += $"\n";

        //    //put any subscriptioins to any spbs here.
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        foreach (var spbSub in ao.AllSPBsISubTo)
        //        {
        //            //AE_SubscribeToSPB(encoderPosition_SPB, motorDriverSpeedControllerTDU, &motorDriverSpeedControllerTDU->CurrentPosition, 0);
        //            ret += $"AE_SubscribeToSPB({spbSub.spbToSubTo.InstanceName}, {spbSub.NameOfSubbingInstanceName}, &{spbSub.NameOfSubbingInstanceName}->{spbSub.MemberFloatNameTo}, {spbSub.filterNumToSubToFromSPB});"; ret += $"\n";
        //        }
        //    }


        //    return ret;
        //}

        //public static string All_GenerateFunctionDefinesSection()
        //{
        //    string ret = "";
        //    foreach (var ao in AllInstancesOfAO)
        //    {
        //        ret += ao.GenerateFunctionDefinesSection();
        //    }
        //    return ret;
        //}

        public static string All_GenerateAEConfigSection()
        {
            string ret = "";
            foreach (var ao in AllInstancesOfAO)
            {
                ret += ao.GenerateAEConfigSection();
            }
            return ret;
        }



 

        public bool IsGeneratedConfg { get { return isGeneratedConfg; } protected set { isGeneratedConfg = value; } }
        public bool isGeneratedConfg = false;

        //the part that goes in the AEConfig. the defines and such
        protected abstract string _GenerateAEConfigSection(int numOfAOOfThisSameTypeGeneratesAlready);

        public string GenerateAEConfigSection()
        {
            //if (this.AOType == AOType.SPB)
            //{
            //    var t = this.GetAllAOOfType<AESPBBase>();

            //var tt = (IPartOfAEDefines)this;
            var allSameType = AllInstancesOfAO.Where(d => d.ClassName == this.ClassName);// && tt.GetFullTemplateArgs() == ((IPartOfAEDefines)d).GetFullTemplateArgs());
            int numAlreadyCreated = 0;

            foreach (var same in allSameType)
            {
                if (same.IsGeneratedConfg == true)
                {
                    numAlreadyCreated++;
                }
            }

            //}


            if (numAlreadyCreated == 0)
            {
                //increment numOfAOSoFarAEConfigGenerated as this is the start of a new AOcponfig generated
                //only do this for spbs, utilityServices, or AOSM
                //if (AOType == AOTypeEnum.SPB || AOType == AOTypeEnum.UtilityService)
                //{
                //    numOfAOSoFarAEConfigGenerated++;
                //}


            }



            isGeneratedConfg = true;
            return _GenerateAEConfigSection(numAlreadyCreated) + "\n";


        }
    }



    public interface IPartOfAEDefines
    {
        string GetFullTemplateType();
        string GetFullTemplateArgsValues();
    }

}
