#if !UNITY_EDITOR
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;

namespace Alkawa.Core
{
    public class CrashLogger : SingletonMonoBehavior<CrashLogger>
    {
        private static readonly HttpClient Client = new();
        private const string Host = "https://nam270620.pythonanywhere.com/";
        private const string CreateSessionAPI = "api/create_session";
        private const string SendLogAPI = "api/log";

        private SessionResponse _sessionResponse;
        
        private readonly Regex _callstackRegex = new Regex(
            @"^(?>\s*at\s)?\s*(?<method>[\w\.<>\+]+)\s?\((?<params>.*)\)\s?(?>in\s(?<filename>.*):line\s(?<line>\d+))?",
            RegexOptions.Compiled);
        


        protected override void Awake()
        {
            base.Awake();
            Application.logMessageReceivedThreaded += HandleLog;
        }

        private void Start()
        {
            StartSession();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.LogError("hello");
            }
#endif
        }

        public void StartSession()
        {
            CreateSession();
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
            base.OnDestroy();
        }

        private async void CreateSession()
        {
            try
            {
                var deviceName = SystemInfo.deviceModel;
                var utcTime = DateTime.UtcNow;
                var tz7Time = utcTime.AddHours(7);
                var sessionTime = tz7Time.ToString("MM_dd_HH_mm");
                var sessionId = sessionTime + "_" + deviceName;

                var url = Host + CreateSessionAPI;
                var payload = new { session = sessionId };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await Client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                _sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(responseJson);
                Debug.Log("Session created: " + _sessionResponse.Session);

                CreateLogFileIfNotExist();
            }
            catch (Exception ex)
            {
                Debug.LogError("CreateSession error: " + ex.Message);
            }
        }

        private void CreateLogFileIfNotExist()
        {
            try
            {
                var fileName = $"{_sessionResponse?.Session ?? "default_session"}.txt";
                var filePath = Path.Combine(Application.persistentDataPath, fileName);
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath))
                    {
                    }
                    Debug.Log("Log file created at: " + filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("CreateLogFileIfNotExist error: " + ex.Message);
            }
        }

        public void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error)
                return;
            if (string.IsNullOrEmpty(stackTrace))
            {
                stackTrace = System.Environment.StackTrace;
            }
            
            var parsedCallstack = ParseCallstack(stackTrace);

            var logData = new
            {
                logType = type.ToString(),
                message = logString,
                stackTrace = stackTrace,
                callstackEntries = parsedCallstack
            };
            _ = SendLogToServer(logData);
            _ = SaveLogToLocal(logData);
        }
        
        private object[] ParseCallstack(string stackTrace)
        {
            var lines = stackTrace.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var entries = new System.Collections.Generic.List<object>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var match = _callstackRegex.Match(trimmedLine);
                if (match.Success)
                {
                    var method = match.Groups["method"].Value;
                    var parameters = match.Groups["params"].Value;
                    var filename = match.Groups["filename"].Success ? match.Groups["filename"].Value : "";
                    var lineNumber = match.Groups["line"].Success ? match.Groups["line"].Value : "";
                    entries.Add(new { method, parameters, filename, lineNumber });
                }
                else
                {
                    entries.Add(new { line = trimmedLine });
                }
            }
            return entries.ToArray();
        }

        private async Task SendLogToServer(object logData)
        {
            try
            {
                var payload = new
                {
                    log = logData,
                    session = _sessionResponse?.Session
                };
                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = Host + SendLogAPI;
                var response = await Client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.LogError("SendLogToServer error: " + ex.Message);
            }
        }

        private async Task SaveLogToLocal(object logData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(logData);
                var fileName = $"{_sessionResponse?.Session ?? "default_session"}.txt";
                var filePath = Path.Combine(Application.persistentDataPath, fileName);
                await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError("SaveLogToLocal error: " + ex.Message);
            }
        }
        
        private class SessionResponse
        {
            public string Message;
            public string Session;
        }
    }
}
#endif