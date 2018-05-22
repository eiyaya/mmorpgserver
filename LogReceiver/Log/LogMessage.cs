using System;
using System.Collections.Generic;


namespace LogReceiver
{
    public class LogMessage
    {
        /// <summary>
        /// The Line Number of the Log Message
        /// </summary>
        public ulong SequenceNr;

        /// <summary>
        /// Logger Name.
        /// </summary>
        public string LoggerName;

        /// <summary>
        /// Root Logger Name.
        /// </summary>
        public string RootLoggerName;

        /// <summary>
        /// Log Message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Thread Name.
        /// </summary>
        public string ThreadName;

        /// <summary>
        /// Time Stamp.
        /// </summary>
        public DateTime TimeStamp;

        /// <summary>
        /// Properties collection.
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        /// <summary>
        /// An exception message to associate to this message.
        /// </summary>
        public string ExceptionString;

        /// <summary>
        /// The CallSite Class
        /// </summary>
        public string CallSiteClass;


        /// <summary>
        /// The CallSite Method in which the Log is made
        /// </summary>
        public string CallSiteMethod;

        /// <summary>
        /// The Name of the Source File
        /// </summary>
        public string SourceFileName;

        /// <summary>
        /// The Line of the Source File
        /// </summary>
        public uint SourceFileLineNr;

        public void CheckNull()
        {
            if (string.IsNullOrEmpty(LoggerName))
                LoggerName = "Unknown";
            if (string.IsNullOrEmpty(RootLoggerName))
                RootLoggerName = "Unknown";
            if (string.IsNullOrEmpty(Message))
                Message = "Unknown";
            if (string.IsNullOrEmpty(ThreadName))
                ThreadName = string.Empty;
            if (string.IsNullOrEmpty(ExceptionString))
                ExceptionString = string.Empty;
            if (string.IsNullOrEmpty(ExceptionString))
                ExceptionString = string.Empty;
            if (string.IsNullOrEmpty(CallSiteClass))
                CallSiteClass = string.Empty;
            if (string.IsNullOrEmpty(CallSiteMethod))
                CallSiteMethod = string.Empty;
            if (string.IsNullOrEmpty(SourceFileName))
                SourceFileName = string.Empty;
        }

        private string GetInformation(FieldType fieldType)
        {
            string result = string.Empty;
            switch (fieldType.Field)
            {
                case LogMessageField.SequenceNr:
                    result = SequenceNr.ToString();
                    break;
                case LogMessageField.LoggerName:
                    result = LoggerName;
                    break;
                case LogMessageField.RootLoggerName:
                    result = RootLoggerName;
                    break;
                case LogMessageField.Message:
                    result = Message;
                    break;
                case LogMessageField.ThreadName:
                    result = ThreadName;
                    break;
                case LogMessageField.TimeStamp:
                    result = TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    break;
                case LogMessageField.Exception:
                    result = ExceptionString;
                    break;
                case LogMessageField.CallSiteClass:
                    result = CallSiteClass;
                    break;
                case LogMessageField.CallSiteMethod:
                    result = CallSiteMethod;
                    break;
                case LogMessageField.SourceFileName:
                    result = SourceFileName;
                    break;
                case LogMessageField.SourceFileLineNr:
                    result = SourceFileLineNr.ToString();
                    break;
                case LogMessageField.Properties:
                    result = Properties.ToString();
                    break;
            }
            return result;
        }
    }
}
