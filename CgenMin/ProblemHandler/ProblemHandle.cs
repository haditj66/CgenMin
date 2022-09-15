using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator.ProblemHandler
{
    public class ProblemHandle
    {
        public List<ICleanUp> Cleanups { get; }

        public ProblemHandle(params ICleanUp[] cleanups)
        {
            Cleanups = new List<ICleanUp>();
            for (int i = 0; i < cleanups.Length; i++)
            {
                AddCleanUp(ref cleanups[0]);
            }  
        }

        public void AddCleanUp(ref ICleanUp cleanUp)
        {
            Cleanups.Add(cleanUp);
        }

        public void ThereisAProblem(string MsgOfProblem)
        {

            //now cleanup
            foreach (var cleanup in Cleanups)
            {
                cleanup.CleanUp();
            }

            //first display problem
            Console.WriteLine("-------------------------------------PROBLEM-----------------------------");
            Console.WriteLine(MsgOfProblem);
            Console.WriteLine("-------------------------------------------------------------------------");

            //exit program
            Environment.Exit(0);
        }

    }
}
