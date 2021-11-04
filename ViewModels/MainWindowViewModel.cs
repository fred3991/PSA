using ProcessSurveyApp.Commands;
using ProcessSurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessSurveyApp.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {


        // временная задержка для логов
        public static int LogTimeDelay = 1000;
        // директория проекта
        //public static string projectPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName).FullName;
        public static string projectPath = Path.Combine(Environment.CurrentDirectory);
        public static string logDir = projectPath + @"\LogDir\";

        // лист дочерних процессов
        public List<Process> childProcess {get; set;}
        // основной процесс приложения
        public Process mainProcess { get; set; }
        // поток для отслеживания всех процессов
        public Thread MainSurveyThread { get; set; }
        // Поток для построчной записи всех логов
        public Thread LogThread { get; set; }


        #region WPF поля
        private string _title = "Procces Survey Application";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        private int _processNum = 0;
        public int ProcessNum
        {
            get => _processNum;
            set => Set(ref _processNum, value);
        }
        private string _processData = "Process Data";
        public string ProcessData
        {
            get => _processData;
            set => Set(ref _processData, value);
        }
        #endregion
        #region Комманды для кнопок
        public ICommand StartProcessFFMPEG { get; }
        private void OnStartProcessFFMPEGCommandExecuted(object p)
        {
            Thread processThread = new Thread(new ThreadStart(StartThreadProccesFFMPEG));
            processThread.Start(); // запускаем поток

        }
        private bool CanStartProcessFFMPEGCommandExecute(object p) => true;

        public ICommand StartNotePad { get; }
        private void OnStartNotePadCommandExecuted(object p)
        {
            Thread processThread = new Thread(new ThreadStart(StartThreadProccesNotePad));
            processThread.Start(); // запускаем поток
        }
        private bool CanStartNotePadCommandExecute(object p) => true;

        public ICommand StartProcessCMD { get; }
        private void OnStartProcessCMDCommandExecuted(object p)
        {
            Thread processThread = new Thread(new ThreadStart(StartThreadProccesCMD));
            processThread.Start(); // запускаем поток
        }
        private bool CanStartProcessCMDCommandExecute(object p) => true;
        public ICommand DebugProcess { get; }
        private void OnDebugProcessCommandExecuted(object p )
        {

            //MainSurveyThread = new Thread(new ThreadStart(SurveyThreadStart));
            //MainSurveyThread.Start();

        }
        private bool CanDebugProcessCommandExecute(object p) => true;
        public ICommand FullLoging { get; }
        private void OnFullLogingCommandExecuted(object p)
        {

            LogThread = new Thread(new ThreadStart(LogginProcces));
            LogThread.IsBackground = true;
            LogThread.Start();

        }
        private bool CanFullLogingCommandExecute(object p) => true;
        public ICommand RequestProccesData { get; }
        private void OnRequestProccesDataCommandExecuted(object p)
        {

            //ChildSurveyThread = new Thread(new ThreadStart(GetProcessData));
            //ChildSurveyThread.IsBackground = true;
            //ChildSurveyThread.Start();
        }
        private bool CanRequestProccesDataCommandExecute(object p) => true;
        #endregion
        #region Методы для кнопок
        public static void StartThreadProccesFFMPEG()
        {

            string ffmpegPath = projectPath + @"\Libs\ffmpeg\ffmpeg.exe";
            var fps = 10;
            var currentRecordPath = projectPath + @"\Libs\ImageDB\";
            var ext = ".tif";
            var currentRecordCounter = 1;
            var inputPath = Path.Combine(currentRecordPath, "%d" + ext);
            var quality = 1;
            var outputPath = Path.Combine(currentRecordPath, "Temp Records Medium", $"{currentRecordCounter}.wmv");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var ffmpegProcess = new ProcessStartInfo(ffmpegPath);
            ffmpegProcess.WindowStyle = ProcessWindowStyle.Normal;
            ffmpegProcess.UseShellExecute = true;
            ffmpegProcess.Arguments = string.Format(CultureInfo.InvariantCulture, "-y -r {0:#.##} -start_number {3} -i \"{1}\" -vframes {4} -q:v {5} -s 5000x5000 -c:v msmpeg4 \"{2}\"", fps, inputPath, outputPath, 1, 1000, quality);
            Process.Start(ffmpegProcess);

        }
        public void StartThreadProccesCMD()
        {

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"cmd";
            psi.UseShellExecute = true;
            Process.Start(psi);

        }
        public void StartThreadProccesNotePad()
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = @"NotePad";
            procInfo.UseShellExecute = true;
            Process.Start(procInfo);
        }
        #endregion




        public MainWindowViewModel()
        {

            // Запускаем в фоне поток отслеживания всех процессов
            MainSurveyThread = new Thread(new ThreadStart(SurveyThreadStart));
            MainSurveyThread.IsBackground = true;
            MainSurveyThread.Start();




            //

            #region Лямбда комманды для кнопок WPF
            StartProcessFFMPEG = new LambdaCommand(OnStartProcessFFMPEGCommandExecuted, CanStartProcessFFMPEGCommandExecute);
            StartProcessCMD = new LambdaCommand(OnStartProcessCMDCommandExecuted, CanStartProcessCMDCommandExecute);
            StartNotePad = new LambdaCommand(OnStartNotePadCommandExecuted, CanStartNotePadCommandExecute);

            RequestProccesData = new LambdaCommand(OnRequestProccesDataCommandExecuted, CanRequestProccesDataCommandExecute);
            DebugProcess = new LambdaCommand(OnDebugProcessCommandExecuted, CanDebugProcessCommandExecute);
            FullLoging = new LambdaCommand(OnFullLogingCommandExecuted, CanFullLogingCommandExecute);
            #endregion


        }


        public void GetProcessData()
        {            
            if (!Directory.Exists(logDir)) // создаем директорию, если нет
            {
                Directory.CreateDirectory(logDir);
            }
            ProcessData = "Total processes " + childProcess.Count();

            try
            {
                ChildProccesLogging();
                MainProccesLogging();
            }
            catch
            {
                return;
            }
           
        }


        public void LogginProcces()
        {
            while (true)
            {

                GetProcessData();
                // Логирование по времени для всех процессов
                Thread.Sleep(LogTimeDelay);
            }
           
        }



        public void ChildProccesLogging()
        {
            if (childProcess.Count != 0) // проверка - есть ли дочерние процессы вообще - если есть, то 
            {
                foreach (var process in childProcess)
                {
                    string path = logDir + @"\" + process.StartTime.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + process.ProcessName + "_ID_" + process.Id + "_.log";

                    if (!process.HasExited)
                    {
                        process.Refresh(); // обновляем процесс для который логируем
                        // Заголовок для файла
                        if (!File.Exists(path))
                        {
                            // Create a file to write to.
                            string FileHeader = string.Format(CultureInfo.InvariantCulture, "!START LOG PROCCES: {0} START TIME: {1} PROCESS ID: {2}", process.ProcessName, process.StartTime.ToString("yyyy-MM-dd  HH:mm:ss:ffff"), process.Id) + Environment.NewLine;
                            File.WriteAllText(path, FileHeader);
                            string headeString = "CurrentDateTime" + "\t" + "PhysicalMemoryUsage" + "\t" + "UserProcessorTime" + "\t"
                                                + "PrivilegedProcessorTime" + "\t" + "TotalProcessorTime" + "\t" + "PagedSystemMemorySize" + "\t"
                                                + "PagedMemorySize" + "\t" + "PeakPagedMemory" + "\t" + "PeakVirtualMemory" + "\t"
                                                + "PeakWorkingSet" + Environment.NewLine + "\n";

                            File.AppendAllText(path, headeString);
                        }

                        // Здесь получаем данные для процесса строка - память и все остальное записываем все показатели                  
                        string appendText = string.Format(CultureInfo.InvariantCulture,
                                                  "{0}" + "\t" + "{1}" + "\t" + "{2}" + "\t"
                                                + "{3}" + "\t" + "{4}" + "\t" + "{5}" + "\t"
                                                + "{6}" + "\t" + "{7}" + "\t" + "{8}" + "\t"
                                                + "{9}" + "\t" + Environment.NewLine,

                                                  DateTime.Now.ToString("HH:mm:ss:ffff"),
                                                  (double)process.WorkingSet64 / (1024 * 1024),  // PhysicalMemoryUsage 
                                                  process.UserProcessorTime,  // UserProcessorTime

                                                  process.PrivilegedProcessorTime, // PrivilegedProcessorTime
                                                  process.TotalProcessorTime, //  TotalProcessorTime
                                                  (double)process.PagedSystemMemorySize64 / (1024 * 1024), // PagedSystemMemorySize

                                                  (double)process.PagedMemorySize64 / (1024 * 1024),  // PagedMemorySize
                                                  (double)process.PeakPagedMemorySize64 / (1024 * 1024), // PeakPagedMemory
                                                  (double)process.PeakVirtualMemorySize64 / (1024 * 1024), // PeakVirtualMemory

                                                  (double)process.PeakWorkingSet64 / (1024 * 1024) // PeakWorkingSet

                                                  );

                        File.AppendAllText(path, appendText);
                    }
                    else
                    {
                        ProcessData = "Procces exited";
                    }
                }

            }
            else // если дочерних процессов нет то
            {
                ProcessData = "No child procces";
            }

        }


        public void MainProccesLogging()
        {
            string path = logDir + @"\" + mainProcess.StartTime.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + mainProcess.ProcessName + "_ID_" + mainProcess.Id + "_.log";

            if (!mainProcess.HasExited)
            {
                mainProcess.Refresh(); // обновляем процесс для который логируем
                                   // Заголовок для файла
                if (!File.Exists(path))
                {
                    // Create a file to write to.
                    string FileHeader = string.Format(CultureInfo.InvariantCulture, "!START LOG MAIN PROCCES: {0} START TIME: {1} PROCESS ID: {2}", mainProcess.ProcessName, mainProcess.StartTime.ToString("yyyy-MM-dd  HH:mm:ss:ffff"), mainProcess.Id) + Environment.NewLine;
                    File.WriteAllText(path, FileHeader);
                    string headeString = "CurrentDateTime" + "\t" + "PhysicalMemoryUsage" + "\t" + "UserProcessorTime" + "\t"
                                        + "PrivilegedProcessorTime" + "\t" + "TotalProcessorTime" + "\t" + "PagedSystemMemorySize" + "\t"
                                        + "PagedMemorySize" + "\t" + "PeakPagedMemory" + "\t" + "PeakVirtualMemory" + "\t"
                                        + "PeakWorkingSet" + Environment.NewLine + "\n";

                    File.AppendAllText(path, headeString);
                }

                // Здесь получаем данные для процесса строка - память и все остальное записываем все показатели                  
                string appendText = string.Format(CultureInfo.InvariantCulture,
                                          "{0}" + "\t" + "{1}" + "\t" + "{2}" + "\t"
                                        + "{3}" + "\t" + "{4}" + "\t" + "{5}" + "\t"
                                        + "{6}" + "\t" + "{7}" + "\t" + "{8}" + "\t"
                                        + "{9}" + "\t" + Environment.NewLine,

                                          DateTime.Now.ToString("HH:mm:ss:ffff"),
                                          (double)mainProcess.WorkingSet64 / (1024 * 1024),  // PhysicalMemoryUsage 
                                          mainProcess.UserProcessorTime,  // UserProcessorTime

                                          mainProcess.PrivilegedProcessorTime, // PrivilegedProcessorTime
                                          mainProcess.TotalProcessorTime, //  TotalProcessorTime
                                          (double)mainProcess.PagedSystemMemorySize64 / (1024 * 1024), // PagedSystemMemorySize

                                          (double)mainProcess.PagedMemorySize64 / (1024 * 1024),  // PagedMemorySize
                                          (double)mainProcess.PeakPagedMemorySize64 / (1024 * 1024), // PeakPagedMemory
                                          (double)mainProcess.PeakVirtualMemorySize64 / (1024 * 1024), // PeakVirtualMemory

                                          (double)mainProcess.PeakWorkingSet64 / (1024 * 1024) // PeakWorkingSet
                                          );

                File.AppendAllText(path, appendText);
            }
            else
            {
                return;
            }
        }



        public void SurveyThreadStart()
        {
            // Бесконечный цикл опроса количества дочерних процессов и получения главного процесса
            while (true)
            {
                try
                {
                    string mainProcessName = Assembly.GetExecutingAssembly().GetName().Name;
                    mainProcess = Process.GetProcessesByName(mainProcessName)[0];
                    childProcess = ProcessExtensions.GetChildProcesses(mainProcess);
                    ProcessNum = childProcess.Count();
                    //Thread.Sleep(500);
                }
                catch
                {

                }
            }        
        }
    }
}
