using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using CadastreInvent.Api.Hubs;
using CadastreInvent.Valuation.Application.Services;

namespace CadastreInvent.Api.Services
{
    public class MassAppraisalDiagnosticLogger : IMassAppraisalDiagnosticLogger
    {
        private readonly ConcurrentQueue<DiagnosticLogMessage> _logs = new();
        private const int MaxLogs = 200;
        private readonly IHubContext<MassAppraisalHub> _hubContext;

        public MassAppraisalDiagnosticLogger(IHubContext<MassAppraisalHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void LogError(string source, string message, Exception ex) => AddLog("ERROR", source, message, ex?.ToString());
        public void LogWarning(string source, string message) => AddLog("WARNING", source, message);
        public void LogInfo(string source, string message) => AddLog("INFO", source, message);

        private void AddLog(string level, string source, string message, string exception = "")
        {
            var logEntry = new DiagnosticLogMessage
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Source = source,
                Message = message,
                ExceptionDetails = exception ?? string.Empty
            };

            _logs.Enqueue(logEntry);

            while (_logs.Count > MaxLogs)
            {
                _logs.TryDequeue(out _);
            }

            _ = _hubContext.Clients.All.SendAsync("ReceiveDiagnosticLog", logEntry);
        }

        public IEnumerable<DiagnosticLogMessage> GetRecentLogs(int count = 100)
        {
            return _logs.ToArray().Reverse().Take(count).ToList();
        }

        public void ClearLogs()
        {
            _logs.Clear();
        }
    }
}