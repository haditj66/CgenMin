using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class CMDHandler
    {

        public string Output;
        public string Error;

        protected ProcessStartInfo processInfo { get; set; }
        protected Process process { get; set; }

        protected string Base_Directory_Of_BATCHFILES;
        protected string SourceBatchFile;  
        protected string BatchFileExt;
        protected string FilenameOfTerminal;
        protected string ArgumentPreChar;
        bool InSeperateTerminal;
        bool LinuxNewTerminalWorkaround;

        protected List<string> MultipleCommands;
        
        public string GetSourceBatchFile(){return SourceBatchFile;}

        public CMDHandler(string StartingWorkingDirtory, string base_Directory_Of_BATCHFILES, bool inSeperateTerminal)
        {
            Base_Directory_Of_BATCHFILES = base_Directory_Of_BATCHFILES;
            InSeperateTerminal = inSeperateTerminal;

            LinuxNewTerminalWorkaround = inSeperateTerminal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SourceBatchFile = "call";
                BatchFileExt = ".bat";
                FilenameOfTerminal = "cmd.exe";
                ArgumentPreChar = @"/c ";
            }else
            {
                InSeperateTerminal = false;//linux cant do that but there is a workaround that will happen
                SourceBatchFile = "source";
                BatchFileExt = ".bash";
                FilenameOfTerminal = "/bin/bash";
                ArgumentPreChar = @"-c ";
            }

            processInfo = new ProcessStartInfo() {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = FilenameOfTerminal,//"/home/user/Documents/blabla.bash",//"/home/user/QR_Sync/QR_Core/oursource.bash",//"/home/user/QR_Sync/QR_Core/oursource.bash",//$"/bin/bash",
                    //FileName = "/home/user/QR_Sync/QR_Core/oursource.bash",
                    //FileName = "/home/user/Documents/LiftPermissions.bash",
                    //WorkingDirectory =  "/mnt",
                    Arguments = ArgumentPreChar,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = InSeperateTerminal,
                    CreateNoWindow = true,
                    };
 
                    // var rrr = Process.Start(processInfo);
                    // Output = rrr.StandardOutput.ReadToEnd();
                    // Error = rrr.StandardError.ReadToEnd();

                //      using var process = new Process
                //         {
                //             StartInfo = new ProcessStartInfo
                //             {
                //                 WorkingDirectory = @"/home/user/Documents",
                //                 RedirectStandardOutput = true,
                //                 UseShellExecute = false,
                //                 CreateNoWindow = true,
                //                 WindowStyle = ProcessWindowStyle.Hidden,
                //                 FileName = "/bin/bash",
                //                 Arguments = $"-c \"source /home/user/Documents/blabla.bash\"",
                //                 //Arguments = $"-c {escapedArgs}",
                //                 //Arguments = $"-c \"{escapedArgs}\""
                //             }
                //         };
                //     process.Start();
                //     process.WaitForExit();

                // Output = process.StandardOutput.ReadToEnd();
                // Error = process.StandardError.ReadToEnd();

            SetWorkingDirectory(StartingWorkingDirtory);

            MultipleCommands = new List<string>(); 
        }


        public void SetMultipleCommands(string command)
        { 
            MultipleCommands.Add(command);
        }



        public void ExecuteMultipleCommands_InSeperateProcess()
        {
            Process p =  ExecuteMultipleCommands(true, false,"","",false);
            p.WaitForExit();
        }

        public void ExecuteMultipleCommands(bool SupressErrorMsg = false)
        {
            ExecuteMultipleCommands(false, false, "","", SupressErrorMsg);
        }


        public void ExecuteMultipleCommands_InItsOwnBatch(string toBatchFileDir, string nameOfBatch)
        {
            ExecuteMultipleCommands(false, true, toBatchFileDir, nameOfBatch);
        }


        private Process ExecuteMultipleCommands(bool RunSeperateProcess = false, bool justCreateBatchFile = false,  string toBatchFileDir ="", string nameOfBatch = "", bool SupressErrorMsg = false)
        {
            string NameOfBatch;
            string pathToBatch;
            if (justCreateBatchFile)
            {
                NameOfBatch = nameOfBatch + BatchFileExt;
                pathToBatch = Path.Combine(toBatchFileDir, NameOfBatch); 

                if(Directory.Exists(toBatchFileDir) == false)
                {
                    //Debug.Assert(false,$"the path to batch file dows not exist. {pathToBatch}");
                    Directory.CreateDirectory(toBatchFileDir);
                }

            }
            else
            {
                NameOfBatch = "batch_" + this.GetHashCode() + BatchFileExt;
 
                pathToBatch = Path.Combine(Base_Directory_Of_BATCHFILES, NameOfBatch);   
            }

            

            
            //File.Create(pathToBatch);

   
            bool keeptrying = true;
            while (keeptrying)
            { 
                try
                { 

                    //if this is in linux and for a new terminal, write a command that will run all commands in another batch 
                    if (LinuxNewTerminalWorkaround)
                    {
                        string commandsFlat ="";
                        MultipleCommands.ForEach((s)=>commandsFlat = commandsFlat + s + " ;");
                        commandsFlat = commandsFlat.Remove(commandsFlat.Count()-1,1);
                        commandsFlat =$"gnome-terminal -- sh -c \"bash -c \\\"{commandsFlat}; exec bash\\\"\"";

                        File.WriteAllText(pathToBatch, "");
                        File.WriteAllText(pathToBatch, commandsFlat);
                        keeptrying = false;
                        

                    }else
                    {   
                        File.WriteAllText(pathToBatch, "");
                        //send commands to a batch file that will run
                        File.WriteAllLines(pathToBatch, MultipleCommands);
                        keeptrying = false;
                    }
 
 
                }
                catch(Exception e)
                {

                }
            }


            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                    //append a @"#!/bin/bash" to the beggining of the file 
                    string rrff = File.ReadAllText(pathToBatch);
                    rrff = @"#!/bin/bash"+"\n"+ rrff;
                    File.WriteAllText(pathToBatch, rrff);


                             //if in linux, I need to change the permissions of this file.
                        //var escapedArgs = "chmod 777 /home/user/Documents/blabla.bash $echo asdf".Replace("\"", "\\\"");
                        var escapedArgs = $"chmod 777 {pathToBatch}".Replace("\"", "\\\"");
        
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                WorkingDirectory = Path.GetDirectoryName(pathToBatch),//@"/home/user/Documents",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = "/bin/bash",
                                //Arguments = "/c " + escapedArgs,
                                //Arguments = $"-c {escapedArgs}",
                                Arguments = $"-c \"{escapedArgs}\""
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                     //string Output2 = process.StandardOutput.ReadToEnd();
                    //  string Error2 = process.StandardError.ReadToEnd();

            }

            

 
                    


            //get the old workingdirectory
            string oldwd = processInfo.WorkingDirectory;

            //change to the directory where the batch file is in
            //SetWorkingDirectory(".");

            //execute the bash file
            Process p = null;
            if (RunSeperateProcess == true)
            {
                string ppp =  Path.Combine(Base_Directory_Of_BATCHFILES, "batch_" + this.GetHashCode() + BatchFileExt);
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo.FileName = ppp;
                    ExecuteCommand("");
                    processInfo.FileName = FilenameOfTerminal;
                }
                else{
                    p = System.Diagnostics.Process.Start(ppp);

                } 
                
            }
            else if (justCreateBatchFile == true)
            {  
                //processInfo.WorkingDirectory = Path.GetDirectoryName(pathToBatch); 
                //ExecuteCommand(@"chmod +x /home/user/Documents/blabla.bash " + @"$source /home/user/Documents/blabla.bash");
                //ExecuteCommand(pathToBatch);
                //ExecuteCommand(SourceBatchFile + " " + pathToBatch); 
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                { 
                    
                    processInfo.FileName = pathToBatch;



                        ExecuteCommand("");
                        processInfo.FileName = FilenameOfTerminal;
                    // //if this is supposed to be for a different window, then I need to create a workaround for that
                    // if (LinuxNewTerminalWorkaround)
                    // { 
 
                    //     //the command will be to run a seperate window while running the batch file within that window as source
                    //     ExecuteCommand("");//{SourceBatchFile} {pathToBatch}
                    // }
                    // else
                    // {
                    //     processInfo.FileName = pathToBatch;
                    //     ExecuteCommand("");
                    //     processInfo.FileName = FilenameOfTerminal;
                    // } 
                }
                else{
                    ExecuteCommand($"{SourceBatchFile} {pathToBatch}");
                }

            } 
            else if (justCreateBatchFile == false)
            {

                string ppp =  Path.Combine(Base_Directory_Of_BATCHFILES, "batch_" + this.GetHashCode());
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo.FileName = ppp;
                    ExecuteCommand("");
                    processInfo.FileName = FilenameOfTerminal;
                }
                else{
                    ExecuteCommand(SourceBatchFile + " " + ppp);
                } 

            } 
            
            MultipleCommands.Clear();
            //change back to the previous working directory
            SetWorkingDirectory(oldwd);
            return p; 
        }

        public void ExecuteCommand(string command, bool SupressErrorMsg = false)
        {
            //if this is a cd command, handle it differently
            if (Regex.IsMatch(command, @"^\s*cd\s*"))
            {
                //take out the cd part
                Regex.Replace(command, @"^\s*cd\s*", "");
                SetWorkingDirectory(command);
                return;
            }

            int exitCode;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                 processInfo.Arguments = ArgumentPreChar + command;//"/c " + command; 
            }
            else
            {
                
 
                var escapedArgs = command.Replace("\"", "\\\""); 
                processInfo.Arguments = $"-c \"{command}\"";
            }    
            //processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
 

            //processInfo.Arguments = "/c " + "oursource" + " /c " + "cd ../testmod" + " /c " + "ourcolcon";

            //GetEnvironmentVariables(processInfo);
            //processInfo = new ProcessStartInfo(@"""C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"" x86", "/c " + "cl");
            //processInfo.WorkingDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC";
            //processInfo.WorkingDirectory = @"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\Module1A";
            //processInfo.WorkingDirectory =@"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\CodeGenerator\bin\Debug";
            
            // processInfo.CreateNoWindow = true;
            // processInfo.UseShellExecute = false;
            // // *** Redirect the output ***
            // processInfo.RedirectStandardError = true;
            // processInfo.RedirectStandardOutput = true;

            //string error = "";
            //string output = "";
            
                    
            process = Process.Start(processInfo);
            /*
            process.OutputDataReceived += (sender, args) =>
            {
                string output = args.Data;
                // ...
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                string error = args.Data;
                // ...
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            */
            if (!SupressErrorMsg)
            {
                Output = process.StandardOutput.ReadToEnd();
                Error = process.StandardError.ReadToEnd();
            }
            else
            {
                Output = "";
                Error = ""; 
            }
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            //Output = process.StandardOutput.ReadToEnd();

             


            while (Output == null)
            {

            }

            exitCode = process.ExitCode;
#if DEBUG
            //Console.WriteLine("output>>" + (String.IsNullOrEmpty(Output) ? "(none)" : Output));
            //Console.WriteLine("error>>" + (String.IsNullOrEmpty(Error) ? "(none)" : Error));
            //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
#endif
            process.Close();
        }

        public void SetWorkingDirectory(string workingDirectory)
        {
            //check if it contains ":" as this would mean to replace a global directory
            if (workingDirectory.Contains(":"))
            {
                processInfo.WorkingDirectory = workingDirectory;
            }
            else
            {
                //it must be a relative path, add to current path
                if (workingDirectory == "cd ..")
                {
                    processInfo.WorkingDirectory = Directory.GetParent(workingDirectory).FullName;
                }
                else
                {
                    processInfo.WorkingDirectory = Path.Combine(processInfo.WorkingDirectory, workingDirectory);
                }
            }

        }
    } 



// public class CMDHandler
// {

//     public string Output;
//     public string Error;

//     protected ProcessStartInfo processInfo { get; set; }
//     protected Process process { get; set; }

//     public CMDHandler(string StartingWorkingDirtory)
//     {
 

// processInfo = new ProcessStartInfo() {
//                     WindowStyle = ProcessWindowStyle.Hidden,
//                     FileName = "/home/user/QR_Sync/QR_Core/oursource.bash",//$"/bin/bash",
//                     //WorkingDirectory =  "/mnt",
//                     Arguments = $"-c",
//                     RedirectStandardOutput = true,
//                     RedirectStandardError = true,
//                     UseShellExecute = false
// };
        


//         // processInfo = new ProcessStartInfo("cmd.exe");
//         // processInfo = new ProcessStartInfo() {
//         //         FileName = "/bin/bash",
//         //         Arguments = "/dev/init.d/mnw stop",
//         //         //Arguments = argsPrepend + q,
//         //         RedirectStandardOutput = true,
//         //         RedirectStandardError = true,
//         //         UseShellExecute = false

//         // };//{ FileName = "/bin/bash"};//, Arguments = "/dev/init.d/mnw stop", };
        
//         //         var argsPrepend = "";
//         // string[] args; processInfo = 
//         //     args.Skip(1)
//         //     .Select(q => new ProcessStartInfo()
//         //     {
//         //         FileName = "/bin/bash",
//         //         Arguments = argsPrepend + q,
//         //         RedirectStandardOutput = true,
//         //         RedirectStandardError = true,
//         //         UseShellExecute = false,
//         //     }).ToList();
        
//         SetWorkingDirectory(StartingWorkingDirtory);


//     }


//     public void ExecuteCommand(string command, bool SupressErrorMsg = false)
//     {
//         //if this is a cd command, handle it differently
//         if (Regex.IsMatch(command, @"^\s*cd\s*"))
//         {
//             //take out the cd part
//             Regex.Replace(command, @"^\s*cd\s*", "");
//             SetWorkingDirectory(command);
//             return;
//         }

//         int exitCode;
//         //processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

//         //processInfo.Arguments = "/c " + command;



//         //GetEnvironmentVariables(processInfo);
//         //processInfo = new ProcessStartInfo(@"""C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"" x86", "/c " + "cl");
//         //processInfo.WorkingDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC";
//         //processInfo.WorkingDirectory = @"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\Module1A";
//         //processInfo.WorkingDirectory =@"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\CodeGenerator\bin\Debug";
//         processInfo.CreateNoWindow = true;
//         processInfo.UseShellExecute = false;
//         // *** Redirect the output ***
//         processInfo.RedirectStandardError = true;
//         processInfo.RedirectStandardOutput = true;

//         //string error = "";
//         //string output = "";

//         process = Process.Start(processInfo);
//         /*
//         process.OutputDataReceived += (sender, args) =>
//         {
//             string output = args.Data;
//             // ...
//         };
//         process.ErrorDataReceived += (sender, args) =>
//         {
//             string error = args.Data;
//             // ...
//         };
//         process.BeginOutputReadLine();
//         process.BeginErrorReadLine();
//         */
//         if (!SupressErrorMsg)
//         {
//             Output = process.StandardOutput.ReadToEnd();
//             Error = process.StandardError.ReadToEnd();
//         }
//         else
//         {
//             Output = "";
//             Error = "";
//         }
//         process.WaitForExit();

//         // *** Read the streams ***
//         // Warning: This approach can lead to deadlocks, see Edit #2
//         //Output = process.StandardOutput.ReadToEnd();




//         while (Output == null)
//         {

//         }

//         exitCode = process.ExitCode;
// #if DEBUG
//         //Console.WriteLine("output>>" + (String.IsNullOrEmpty(Output) ? "(none)" : Output));
//         //Console.WriteLine("error>>" + (String.IsNullOrEmpty(Error) ? "(none)" : Error));
//         //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
// #endif
//         process.Close();
//     }

//     public void SetWorkingDirectory(string workingDirectory)
//     {
//         //check if it contains ":" as this would mean to replace a global directory
//         if (workingDirectory.Contains(":"))
//         {
//             processInfo.WorkingDirectory = workingDirectory;
//         }
//         else
//         {
//             //it must be a relative path, add to current path
//             if (workingDirectory == "cd ..")
//             {
//                 processInfo.WorkingDirectory = Directory.GetParent(workingDirectory).FullName;
//             }
//             else
//             {
//                 processInfo.WorkingDirectory = Path.Combine(processInfo.WorkingDirectory, workingDirectory);
//             }
//         }

//     }
// }
