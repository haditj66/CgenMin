#pragma once

#include "moda1conf.h"
#include "GlobalBuildConfig.h"   

int mainCG()
{  

	GlobalBuildConfig glob;
	glob.Init();
	glob.BUILD->SetValue(BuildEnum::DEBUG);
	glob.PLATFORM->SetValue(PlatformEnum::VS);


	moda1Conf0  moda1;
	moda1.Init(); //dont forget to init() every library


	Config::PrintDefines(moda1);
	 return 1;
}