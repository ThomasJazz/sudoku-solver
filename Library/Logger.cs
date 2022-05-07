using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using Newtonsoft.Json;
using Pastel;

namespace sudoku.solver
{
    public class Logger
    {
        public string? FunctionName { get; set; }
        private string InfoLogFilePath { get; set; }
        private string WarningLogFilePath { get; set; }
        private string ErrorLogFilePath { get; set; }
        private bool WriteJsonLogs = false;

        /// <summary>
        /// When Logger is initialized from PlaySudoku.cs...
        /// Environment.StackTrace.Split("\r\n")[2].Split(":line")[0].Split("\\")[^1];
        /// will yield "PlaySudoku.cs"
        /// </summary>
        public Logger(bool WriteJsonLogs=false)
        {
            this.WriteJsonLogs = WriteJsonLogs;

            // Should yield the calling classes function name
            this.FunctionName = new StackTrace()!
                .GetFrame(1)!
                .GetMethod()!
                .DeclaringType!
                .FullName!
                .Split(".")![^1];

            string logFileTime = Helper.GetCurrentDateTime();
            this.InfoLogFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"Dump/info_{FunctionName.ToLower()}_{logFileTime}.json");
            this.WarningLogFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"Dump/warning_{FunctionName.ToLower()}_{logFileTime}.json");
            this.ErrorLogFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"Dump/error_{FunctionName.ToLower()}_{logFileTime}.json");

            this.LogInfo($"Logging initialized. Writing logs to: {this.InfoLogFilePath}");
        }

        /************ CONSOLE LOGGING ************/
        public void LogInfo(string message)
        {
            Log msg = new Log(message, this.FunctionName!);
            Console.WriteLine(msg.ToString());

            if (this.WriteJsonLogs)
                AppendLog(msg, this.InfoLogFilePath);
        }

        public void LogWarning(string message, Exception e=null!)
        {
            WarningLog msg = new WarningLog(message, this.FunctionName!);
            Console.WriteLine(msg.ToString().Pastel(Color.Yellow));

            if (this.WriteJsonLogs)
                AppendLog(msg, this.WarningLogFilePath);
        }
        
        public void LogError(string message, Exception e=null!)
        {
            ErrorLog msg = new ErrorLog(message, this.FunctionName!);
            Console.WriteLine(msg.ToString().Pastel(Color.Red));

            if (this.WriteJsonLogs)
                AppendLog(msg, this.ErrorLogFilePath);
        }

        public void LogJson(object o, bool indent=true)
        {
            if (indent)
                Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented).Pastel(Color.White));
            else
                Console.WriteLine(JsonConvert.SerializeObject(o).Pastel(Color.White));
        }

        /************ I/O FUNCTIONS ************/
        public void AppendLog(Log log, string filePath)
        {
            string json = JsonConvert.SerializeObject(log);
            json = json.ReplaceLineEndings();

            using (StreamWriter file = File.AppendText(filePath))
            {
                file.WriteLine(json);
            }
        }

        /************ PRIVATE FUNCTIONS ************/
        
    }

    public class Log
    {
        public string Timestamp { get; set; }
        public string Body { get; set; }
        public string? FunctionName { get; set; }
        
        public Log(string Body, string FunctionName)
        {
            this.Body = Body;
            this.FunctionName = FunctionName;
            this.Timestamp = Helper.GetCurrentDateTime("yyyy-MM-dd HH:MM:ss.ffffff");
        }

        public override string ToString()
        {
            return $"[{this.Timestamp}] {this.Body}";
        }
    }

    public class WarningLog : Log
    {
        public Exception Exception = new Exception();
        public string StackTrace { get; set; }

        public WarningLog(string Body, string FunctionName, Exception Exception=null!) : base(Body, FunctionName)
        {
            this.Exception = Exception ?? new Exception();
            this.StackTrace = Environment.StackTrace;
        }
    }

    public class ErrorLog : WarningLog
    {
        public ErrorLog(string Body, string FunctionName, Exception Exception=null!) 
            : base(Body, FunctionName, Exception)
        {}
    }
}