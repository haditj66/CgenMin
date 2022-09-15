cmake_minimum_required(VERSION 3.16)

project(whatever)
message("-- USER environment variable is set to: " $ENV{USER})
message("-- HOSTNAME environment variable is set to: " $ENV{NAME})
set(USERNAME $ENV{USER})

set(CMAKE_BUILD_TYPE Debug)
if (WIN32)

    #set(CODEGENGUI_PATH "C:/QR_Sync/CgenMin/CgenMin")
    set(CgenConfigPath "C:/QR_Sync/CgenMin/CgenMin")
    FILE(READ "${CgenConfigPath}/CgenCmakeConfigFunctions.cmake" contentsOut)
    FILE(WRITE "${CMAKE_CURRENT_SOURCE_DIR}/cmake/CgenCmakeConfigFunctions.cmake" ${contentsOut})
else()

    set(CgenConfigPath "/home/${USERNAME}/QR_Sync/CgenMin/CgenMin")#C:/CodeGenerator/CgenCmakeGui")
    FILE(READ "${CgenConfigPath}/CgenCmakeConfigFunctions.cmake" contentsOut)
    FILE(WRITE "${CMAKE_CURRENT_SOURCE_DIR}/cmake/CgenCmakeConfigFunctions.cmake" ${contentsOut})

    
endif()
#endif()
include("${CMAKE_CURRENT_SOURCE_DIR}/cmake/CgenCmakeConfigFunctions.cmake")
Cgen_Start(
    CGEN_DIRECTORY_OF_CACHE "GenericConfig"
    )





#configure_file(${CMAKE_CURRENT_SOURCE_DIR}/testcgen.cgenM.in  ${CMAKE_CURRENT_SOURCE_DIR}/testFromCgen2.cgenM @ONLY)
            
Generate_File_Using_Cgen(
        INPUT_DIRECTORY  ${CMAKE_CURRENT_SOURCE_DIR}
        OUTPUT_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        INPUT_FILE_NAME testcgen
        OUTPUT_FILE_NAME testFromCgen2
        OUTPUT_FILE_EXTENSION .cpp
)
