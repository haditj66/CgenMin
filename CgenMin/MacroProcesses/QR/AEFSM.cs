using CgenMin.MacroProcesses.QR;
using CodeGenerator.MacroProcesses.AESetups;
//using CodeGenerator.MacroProcesses.AESetups.SPBs;
using CodeGenerator.ProblemHandler;
using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgenMin.MacroProcesses
{

    
    //public class UpdateEVT : QREventMSG<UpdateEVT>
    //{
    //    public UpdateEVT() :
    //        base("UpdateEVT",
    //        ""
    //        )
    //    {

    //    }

    //}


    public class AEStateEvent
    {
        

        public QREvent TheEventThstStateSubscribesTo { get; }
        public bool TransitionsOutOfSubFSM { get; }
        public string TheStateThisEventTransitionsTo { get; }
        public AEStateEvent(QREvent theEventThstStateSubscribesTo, string theStateThisEventTransitionsTo = null)
        {
            TheEventThstStateSubscribesTo = theEventThstStateSubscribesTo;
            TheStateThisEventTransitionsTo = theStateThisEventTransitionsTo;
        }

        public AEStateEvent(QREvent theEventThstStateSubscribesTo, bool transitionsOutOfSubFSM)
            : this(theEventThstStateSubscribesTo, null)
        {
            TransitionsOutOfSubFSM = transitionsOutOfSubFSM;
        }

    }

    public class AEState
    {
        public List<AEStateEvent> allevents = new List<AEStateEvent>();
        public string NameOfState { get; }
        public int FrequencyOfUpdate { get; }
        public string ThisStateIsASubFSMNamed { get; }

        private static QREventMSG updEvent = new QREventMSG("updEvent", new FunctionArgs(typeof(bool),""));
        
        //private static UpdateEVT updEvent = (UpdateEVT)UpdateEVT.Init(10);
        //private static ExitFSM ExitFSMEvent = (ExitFSM)ExitFSM.QREventFactory(10);

        public AEState(string nameOfState, int frequencyOfUpdate = 0, string thisStateIsASubFSMNamed = null, params AEStateEvent[] eventsISubscribeTo)
        {
            
            NameOfState = nameOfState;
            FrequencyOfUpdate = frequencyOfUpdate;
            ThisStateIsASubFSMNamed = thisStateIsASubFSMNamed;
            allevents = eventsISubscribeTo.ToList();
        }

        public void ConfigureStateless(StateMachine<string, string>.StateConfiguration state, AEFSM fromFsm)
        {
            //if this state is updateable, 
            if (FrequencyOfUpdate > 0)
            {
                state.PermitReentry(updEvent.ClassName);
            }

            if (ThisStateIsASubFSMNamed != null)
            {
                //if an fsm of this name does not exist, give problem
                if (AEFSM.FSMOfNameExists(ThisStateIsASubFSMNamed) == false)
                {
                    ProblemHandle prob = new ProblemHandle();
                    prob.ThereisAProblem($"the FSM of name {ThisStateIsASubFSMNamed} did not exist when trying to make this a subFSM for state {state.State}");
                }
                state.SubstateOf(ThisStateIsASubFSMNamed);
            }

            foreach (var ev in allevents)
            {

                //if this event transitions the subFSM out to exit from the subFSM
                if (ev.TransitionsOutOfSubFSM == true)
                {
                    //make sure this fsm was a sub fsm
                    //if (fromFsm.IsSubMachine == false)
                    //{
                    //    ProblemHandle prob = new ProblemHandle();
                    //    prob.ThereisAProblem($"you tried to exit out of fsm via state { ev.TheStateThisEventTransitionsTo}. \n but the FSM named {fromFsm.ClassName} this state belongs to is not a sub FSM ");
                    //}
                    state.Permit(ev.TheEventThstStateSubscribesTo.ClassName, "EXIT_FSM");
                }
                //if this event transitions to  something, use Permit
                else if (ev.TheStateThisEventTransitionsTo != null)
                {
                    //check if the state it is transitioning to exists even
                    if (fromFsm.StateOfNameExists(ev.TheStateThisEventTransitionsTo) == false)
                    {
                        ProblemHandle prob = new ProblemHandle();
                        prob.ThereisAProblem($"event {ev.TheEventThstStateSubscribesTo} is transitioning to a state named {ev.TheStateThisEventTransitionsTo} that does not exist. add such a state yet to this FSM name {fromFsm.ClassName} ");
                    }

                    state.Permit(ev.TheEventThstStateSubscribesTo.ClassName, ev.TheStateThisEventTransitionsTo);



                }
                else//else use InternalTransition
                {
                    state.InternalTransition(ev.TheEventThstStateSubscribesTo.ClassName, () => { Console.WriteLine($"triggered event {ev.TheEventThstStateSubscribesTo.ClassName}"); });

                }

            }

            //entry and exits
            state.OnEntry((a) => { Console.WriteLine($"entered {a.Destination}"); });
            state.OnExit((a) => { Console.WriteLine($"Transitioning {a.Source} => {a.Destination}"); });
        }


    }


    public abstract class AEFSM : AOWritableConstructible
    {
        StateMachine<string, string> fsm;
        private static List<AEFSM> AllAEFSMs = new List<AEFSM>();

        HashSet<AEState> _AllStates = new HashSet<AEState>();

        public bool StateOfNameExists(string nameOfState)
        {
            return _AllStates.FirstOrDefault(s => s.NameOfState == nameOfState) != null;
        }

        public static bool FSMOfNameExists(string nameOfFSM)
        {
            return AllAEFSMs.FirstOrDefault(s => s.ClassName == nameOfFSM) != null;
        }

        public AEFSM(string fromLibrary, string instanceNameOfMachine, AEState initialState,  CppFunctionArgs constructorArgs = null)
            : base(fromLibrary, instanceNameOfMachine, AOTypeEnum.SimpleFSM, constructorArgs)
        {


            fsm = new StateMachine<string, string>(initialState.NameOfState);
            _AllStates.Add(initialState);

            AllAEFSMs.Add(this);
            //IsSubMachine = isSubMachine;
            //Priority = priority;
            InitialTimerPeriodIfAnyMilli = initialState.FrequencyOfUpdate;

            SubMachines = new List<string>();

            //SubMachine1 = subMachine1;
            //SubMachine2 = subMachine2;
            //SubMachine3 = subMachine3;

            //if (subMachine1 != null)
            //{
            //    SubMachine1.IAmASubMachineForThis = this;
            //}
            //if (subMachine2 != null)
            //{
            //    SubMachine2.IAmASubMachineForThis = this;
            //}
            //if (subMachine3 != null)
            //{
            //    SubMachine3.IAmASubMachineForThis = this;
            //}
        }

        //public bool IsSubMachine { get; }
        //public AEPriorities Priority { get; }
        public int InitialTimerPeriodIfAnyMilli { get; }
        public List<string> SubMachines { get; }

        protected AEFSM IAmASubMachineForThis = null;

//        public string PriorityStr
//        {
//            get
//            {
//                return
//Priority == AEPriorities.LowPriority ? "LowestPriority" :
//Priority == AEPriorities.MediumPriority ? "MediumPriority" :
//Priority == AEPriorities.HighPriority ? "HighestPriority" : "";
//            }
//        }

        public AEFSM GetSubmachine1()
        {
            return _GetSubmachine(0);
        }
        public AEFSM GetSubmachine2()
        {
            return _GetSubmachine(1);
        }
        public AEFSM GetSubmachine3()
        {
            return _GetSubmachine(3);
        }
        private AEFSM _GetSubmachine(int id)
        {

            AEFSM submachine1 = null;
            ProblemHandle problemHandle = new ProblemHandle();
            if (SubMachines.Count > id)
            {
                submachine1 = (AEFSM)AO.AllInstancesOfAO.Where(a => a.AOType == AOTypeEnum.SimpleFSM).FirstOrDefault(a => a.ClassName == SubMachines[id]);
                //if this submachine does not exist, throw a problem. 
                if (submachine1 == null)
                {
                    problemHandle.ThereisAProblem($"fsm {ClassName} is trying to use submachine {SubMachines[id]}. but it does not exist");
                }
            }
            return submachine1;
        }



        //state adding stuff--------------------------------------
        private bool AllStatesGotten = false;
        public List<AEState> AllStates
        {
            get
            {
                if (AllStatesGotten == false)
                {
                    var t_AllStates = _GetAllStates();
                    foreach (var s in t_AllStates)
                    {
                        _AllStates.Add(s);
                    }

                    //check all states and see if any are subFSMs
                    for (int i = 0; i < t_AllStates.Count; i++)
                    {
                        var state = t_AllStates[i];
                        if (state.ThisStateIsASubFSMNamed != null)
                        {
                            //add the submachine if it is not already there tho.
                            if (SubMachines.FirstOrDefault(a => a == state.ThisStateIsASubFSMNamed) == null)
                            {
                                SubMachines.Add(state.ThisStateIsASubFSMNamed);
                            }
                        }

                    }

                    AllStatesGotten = true;
                }

                return _AllStates.ToList();
            }
        }
        public abstract List<AEState> _GetAllStates();


        private bool StatelessConfigured = false;
        private void ConfigureStateless()
        {
            if (StatelessConfigured == false)
            {
                foreach (var state in AllStates)
                {
                    StateMachine<string, string>.StateConfiguration stConf = fsm.Configure(state.NameOfState);
                    state.ConfigureStateless(stConf, this);
                }
                StatelessConfigured = true;
            }

        }

        public void GenerateDOTDiagramFromUML()
        {
            ConfigureStateless();

            string asdDOT = UmlDotGraph.Format(fsm.GetInfo());

            asdDOT = asdDOT.Replace("Function [Function]", "");

            string pathToDOTFile = @"C:\CodeGenerator\CodeGenerator\MacroProcesses\AESetups\AEProjects" + $"\\{QRInitializing.RunningProjectName}\\";
            if (Directory.Exists(pathToDOTFile) == false)
            {
                Directory.CreateDirectory(pathToDOTFile);
            }
            File.WriteAllText(pathToDOTFile + $"{ClassName}.svg", asdDOT);
        }










        //public override string GenerateFunctionDefinesSection()
        //{
        //    string ret = "";
        //    //nothing
        //    return ret;
        //}

        //public override string GenerateMainClockSetupsSection()
        //{
        //    string ret = "";
        //    //nothing
        //    return ret;
        //}

        public override string GenerateMainHeaderSection_CP()
        {
            //static AELoopObject1Test * objectTest;
            //#include "LoopObjeect1Test.h"
            string ret = "";
            ret += $"#include \"{ClassName}.h\""; ret += "\n";
            ret += $"static {ClassName}* {InstanceName};"; ret += "\n";
            return ret;
        }




        public override string GenerateMainInitializeSection_CP()
        {

            //static BlindsUITopFSM BlindsUIfsmL;
            //BlindsUIfsm = &BlindsUIfsmL;
            //BlindsUIfsm->Init(false, 1, ConfigBlindsUIfsm, NormalUserOperationBlindsfsm);

            string issubMachine = AllAEFSMs[0].ClassName == this.ClassName ? "false" : "true";

            //AEFSM submachine1 = GetSubmachine1();
            //AEFSM submachine2 = GetSubmachine2();
            //AEFSM submachine3 = GetSubmachine3();
            //string submachine1NameStr = submachine1 == null ? "" : "," + submachine1.InstanceName;
            //string submachine2NameStr = submachine2 == null ? "" : "," + submachine2.InstanceName;
            //string submachine3NameStr = submachine3 == null ? "" : "," + submachine3.InstanceName;

            string freqstr = InitialTimerPeriodIfAnyMilli == 0 ? "1" : InitialTimerPeriodIfAnyMilli.ToString();

            string ret = "";
            ret += $"static {ClassName} {InstanceName}_l;"; ret += "\n";
            ret += $"{InstanceName} = &{InstanceName}_l;"; ret += "\n";
            ret += $"{InstanceName}->Init({issubMachine},  {freqstr});"; ret += "\n";
            return ret;
        }

        //public override string GenerateMainLinkSetupsSection()
        //{
        //    string ret = "";

        //    //blinduifsm->AddSubmachine1(configfsm);
        //    //blinduifsm->AddSubmachine2(normalfsm);

        //    //add submachines 
        //    AEFSM submachine1 = GetSubmachine1();
        //    AEFSM submachine2 = GetSubmachine2();
        //    AEFSM submachine3 = GetSubmachine3();
        //    string submachine1NameStr = submachine1 == null ? "" : $"{this.InstanceName}->AddSubmachine1({submachine1.InstanceName});\n";
        //    string submachine2NameStr = submachine2 == null ? "" : $"{this.InstanceName}->AddSubmachine2({submachine2.InstanceName});\n";
        //    string submachine3NameStr = submachine3 == null ? "" : $"{this.InstanceName}->AddSubmachine3({submachine3.InstanceName});\n";

        //    ret += submachine1NameStr + submachine2NameStr + submachine3NameStr;
        //    return ret;
        //}

        public override string GetFullTemplateArgs()
        {
            string ret = "";
            //nothing
            return ret;
        }

        public override string GetFullTemplateArgsValues()
        {
            string ret = "";
            //nothing
            return ret;
        }

        public override string GetFullTemplateType()
        {
            string ret = "";
            //nothing
            return ret;
        }

        protected override string _GenerateAEConfigSection(int numOfAOOfThisSameTypeGeneratesAlready)
        {
            //DONT DO THIS!!!!!
            //#define AnyOtherNeededIncludes2 LoopObjeect1Test 
            string ret = "";
            //nothing
            return ret;
        }

        protected override List<RelativeDirPathWrite> _WriteTheContentedToFiles()
        {
            List<RelativeDirPathWrite> relativeDirPathWrites = new List<RelativeDirPathWrite>();




            //first go through state function definitions and generate those 
            string fsmStateFunctionsContent = "";
            foreach (var state in this.AllStates)
            {
                fsmStateFunctionsContent += $"\n\n\n//state: {state.NameOfState} ======================================================\n";


                //enter subs exit subs -------------------------------
                //@EnterSubs@
                //thisFSM->Subscribe<Button1_Sig>();
                // thisFSM->Subscribe<Button2_Sig>();
                // thisFSM->Subscribe<Button3_Sig>();
                // thisFSM->StartUpdateTimer(100); 
                //thisFSM->ActivateSubmachine(thisFSM->SubMachine1);
                // -------------------------------
                //@ExitSubs@
                //thisFSM->UnSubscribe<Button1_Sig>();
                //thisFSM->UnSubscribe<Button2_Sig>();
                //thisFSM->UnSubscribe<Button3_Sig>();
                //thisFSM->StopUpdateTimer(); 
                //thisFSM->DeActivateSubmachine(thisFSM->SubMachine1);
                string EnterSubs = "";
                string ExitSubs = "";
                foreach (var ev in state.allevents)
                {
                    EnterSubs += $"         thisFSM->Subscribe<{ev.TheEventThstStateSubscribesTo.ClassName}>();"; EnterSubs += "\n";
                    ExitSubs += $"          thisFSM->UnSubscribe<{ev.TheEventThstStateSubscribesTo.ClassName}>();"; ExitSubs += "\n";

                }
                if (state.FrequencyOfUpdate > 0)
                {
                    EnterSubs += $"         thisFSM->StartUpdateTimer({state.FrequencyOfUpdate.ToString()});"; EnterSubs += "\n";
                    ExitSubs += $"          thisFSM->StopUpdateTimer();"; ExitSubs += "\n";
                }
                //check if it transitions to another fsm
                if (state.ThisStateIsASubFSMNamed != null)
                {
                    //get submachine id
                    int idSub = 0;
                    foreach (var subf in SubMachines)
                    {
                        idSub++;
                        if (subf == state.ThisStateIsASubFSMNamed)
                        {
                            break;
                        }
                    }
                    EnterSubs += $"         thisFSM->ActivateSubmachine(thisFSM->SubMachine{idSub.ToString()});"; EnterSubs += "\n";
                    ExitSubs += $"         thisFSM->DeActivateSubmachine(thisFSM->SubMachine{idSub.ToString()});"; ExitSubs += "\n";
                }

                //generate event callbacks -------------------------------
                //@EventCallbacks@
                //case @EvtName@:
                //{
                //    ##UserCode_@StateName@@EvtName@
                //    AEPrint("\nBlindsUITopFSM:  Idle:Button1\n");
                //    TRANSITION_TOSTATE(&Configuring)
                //    break;
                //} 
                string EventCallbacks = "";
                foreach (var ev in state.allevents)
                {
                    EventCallbacks += $"\n// {ev.TheEventThstStateSubscribesTo.ClassName} ------------------------------------------------------------\n";
                    string usercodeName = $"{state.NameOfState}{ev.TheEventThstStateSubscribesTo.ClassName.Replace("_Sig", "").Replace("_", "")}";

                    EventCallbacks += $"    case {ev.TheEventThstStateSubscribesTo.ClassName}:"; EventCallbacks += "\n";
                    EventCallbacks += "     {"; EventCallbacks += "\n";
                    EventCallbacks += $"    ##UserCode_{usercodeName}"; EventCallbacks += "\n";


                    //check if this transitions to another state 
                    if (ev.TheStateThisEventTransitionsTo != null)
                    {
                        EventCallbacks += $"        TRANSITION_TOSTATE(&{ev.TheStateThisEventTransitionsTo})"; EventCallbacks += "\n";
                    }
                    if (ev.TransitionsOutOfSubFSM)
                    {
                        EventCallbacks += $"        AE_EXIT()"; EventCallbacks += "\n";
                    }

                    EventCallbacks += $"        break;"; EventCallbacks += "\n";
                    EventCallbacks += "     }"; EventCallbacks += "\n\n";

                }

                //EventCallbacks += $"    default:"; EventCallbacks += "\n";
                //EventCallbacks += $"         break;"; EventCallbacks += "\n";
                //EventCallbacks += "     }"; EventCallbacks += "\n\n";


                fsmStateFunctionsContent += QRInitializing.TheMacro2Session.GenerateFileOut("AERTOS/FSMStateFunction",
                  new MacroVar() { MacroName = "ClassName", VariableValue = $"{ClassName}" },
                  new MacroVar() { MacroName = "StateName", VariableValue = $"{state.NameOfState}" },
                  new MacroVar() { MacroName = "EnterSubs", VariableValue = $"{EnterSubs}" },
                  new MacroVar() { MacroName = "ExitSubs", VariableValue = $"{ExitSubs}" },
                  new MacroVar() { MacroName = "EventCallbacks", VariableValue = $"{EventCallbacks}" }
                  );


            }


            //#include "ConfigBlindsUIFSM.h"
            //#include "NormalUserOperationBlindsFSM.h"
            string OtherSubMachineFSMHeader = "";
            foreach (var subf in SubMachines)
            {
                OtherSubMachineFSMHeader += $"#include \"{subf}.h\""; OtherSubMachineFSMHeader += "\n";
            }

            //BlindsUITopFSM, ConfigBlindsUIFSM, NormalUserOperationBlindsFSM
            string BaseTemplate = "";
            BaseTemplate += ClassName;
            foreach (var subf in SubMachines)
            {
                BaseTemplate += $", {subf}"; OtherSubMachineFSMHeader += "\n";
            }


            //static AETransitionType Idle(TypeOfThisFSM* const thisFSM, AEEventDiscriminator_t const * const evt);
            //static AETransitionType Configuring(TypeOfThisFSM* const thisFSM, AEEventDiscriminator_t const * const evt);
            //static AETransitionType NormalOperating(TypeOfThisFSM* const thisFSM, AEEventDiscriminator_t const * const evt);
            string StatesDeclared = "";
            foreach (var state in AllStates)
            {
                StatesDeclared += $"static AETransitionType {state.NameOfState}(TypeOfThisFSM* const thisFSM, AEEventDiscriminator_t const * const evt);"; StatesDeclared += "\n";
            }

            string InitialStateName = AllStates.Count > 0 ? AllStates[0].NameOfState : "";

            string fsmContent = QRInitializing.TheMacro2Session.GenerateFileOut("AERTOS/FSMClass",
               new MacroVar() { MacroName = "ClassName", VariableValue = ClassName },
               new MacroVar() { MacroName = "OtherSubMachineFSMHeader", VariableValue = OtherSubMachineFSMHeader },
               new MacroVar() { MacroName = "BaseTemplate", VariableValue = BaseTemplate },
               new MacroVar() { MacroName = "StatesDeclared", VariableValue = StatesDeclared },
               new MacroVar() { MacroName = "StatesDefined", VariableValue = fsmStateFunctionsContent },
               new MacroVar() { MacroName = "InitialStateName", VariableValue = InitialStateName }
               );


            relativeDirPathWrites.Add(new RelativeDirPathWrite($"{ClassName}", ".h", "include", fsmContent, true));
            return relativeDirPathWrites;
        }
    }




}
