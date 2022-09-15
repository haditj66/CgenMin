//generated file: testcgen2.cpp
//**********************************************************************
//this is an auto-generated file using the template file located in the directory of /home/user/QR_Sync/CgenMin/CgenMin/FileTemplates/Files
//ONLY WRITE CODE IN THE UserCode_Section BLOCKS
//If you write code anywhere else,  it will be overwritten. modify the actual template file if needing to modify code outside usersection blocks.


//includes
#include "AETimerSimple.h"

//UserCode_Sectionf
sdfbdfb sdv
//UserCode_Sectionf_end


AETimerSimple::AETimerSimple(uint32_t periodOfTimerTicksInMilli, bool autoReload,TimerSimpleCallback_t timerSimpleCallback)
{
TimerSimpleCallback = timerSimpleCallback
PeriodOfTimerTicksInMilli = periodOfTimerTicksInMilli
AutoReload = autoReload

//UserCode_Sectiona
//UserCode_Sectiona_end

    //always stop the timer
    StopTimer()

}



void AETimerSimple::StartTimer(){
    //UserCode_Sectionb
//UserCode_Sectionb_end
}


void AETimerSimple::StopTimer(){
    //UserCode_Sectionc
//UserCode_Sectionc_end
}

void AETimerSimple::PauseTimer(){
    //UserCode_Sectiond
//UserCode_Sectiond_end
}

bool AETimerSimple::IsTimerActive(){
    //UserCode_Sectione
//UserCode_Sectione_end
}


void AETimerSimple::ChangePeriod(uint32_t newPeriod){
    //UserCode_Sectiong
//UserCode_Sectiong_end
}

uint32_t AETimerSimple::GetPeriodOfTimer()  const {
    //UserCode_Sectionh
//UserCode_Sectionh_end
}


