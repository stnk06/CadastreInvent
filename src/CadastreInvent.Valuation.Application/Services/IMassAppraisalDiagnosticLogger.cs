using System;
using System.Collections.Generic;

namespace CadastreInvent.Valuation.Application.Services
{
    public class DiagnosticLogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ExceptionDetails { get; set; } = string.Empty;
    }

    public interface IMassAppraisalDiagnosticLogger
    {
        void LogError(string source, string message, Exception ex);
        void LogWarning(string source, string message);
        void LogInfo(string source, string message);
        IEnumerable<DiagnosticLogMessage> GetRecentLogs(int count = 100);
        void ClearLogs();
    }
}