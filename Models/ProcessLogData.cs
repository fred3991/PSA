using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessSurveyApp.Models
{
    public class ProcessLogData
    {
        public DateTime ProccesStartTime { get; set; }  // process.StartTime
        public DateTime ProccesStopTime { get; set; }  // process.ExitTime


        public string ProccesName { get; set; }
        public int ID { get; set; }
        public int PeakWorkingSet64_kB { get; set; }   // Process.PeakWorkingSet64;
        public int PeakVirtualMemorySize64_kB { get; set; }    // process.PeakVirtualMemorySize64;
        public int PeakPagedMemorySize64_kB { get; set; } 
        public int PagedSystemMemorySize64_kB { get; set; }
        
        public ProcessLogData(Process process)
        {
            ProccesName = process.ProcessName;
            ID = process.Id;
            ProccesStartTime = process.StartTime;



            PeakWorkingSet64_kB = (int)(process.PeakWorkingSet64/1024);
            PeakVirtualMemorySize64_kB = (int)(process.PeakVirtualMemorySize64 / 1024);
            PeakPagedMemorySize64_kB = (int)(process.PeakPagedMemorySize64 / 1024);
            PagedSystemMemorySize64_kB = (int)(process.PagedSystemMemorySize64 / 1024);

        }
    }
}
