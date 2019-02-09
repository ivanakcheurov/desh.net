using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Desh.Execution;
using Desh.Execution.Logging;

namespace Desh.OutOfTheBox
{
    public class ExecutionLogger : IExecutionLogger
    {
        private readonly Log _log;
        private readonly Dictionary<int, Step> _logSteps = new Dictionary<int, Step>();
        private int _currentStepNumber = 1;

        public ExecutionLogger()
        {
            _log = new Log() { Steps = new Dictionary<int, Step>() };
        }

        /// <summary>
        /// Saves desh and the engine version
        /// </summary>
        /// <param name="desh">should be the desh snippet that is going to be passed to <param name="engine"> for execution</param></param>
        /// <param name="engine">should be the engine used for execution. only used to get version of assembly that contains engine's type</param>
        public void Initialize(string desh, Engine engine)
        {
            //string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //string fileVersion = FileVersionInfo.GetVersionInfo(engine.GetType().Assembly.Location).FileVersion;
            //string productVersion = FileVersionInfo.GetVersionInfo(engine.GetType().Assembly.Location).ProductVersion;
            string productVersion = ((AssemblyInformationalVersionAttribute)Assembly
                    .GetAssembly(engine.GetType())
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0])
                .InformationalVersion;
            _log.Desh = desh;
            _log.DeshMd5 = GetMd5(desh);
            _log.EngineVersion = productVersion;
        }

        public void AddStep(Step step)
        {
            _logSteps.Add(step.Number, step);
        }

        public Log GetLog()
        {
            _log.Steps = _logSteps;
            return _log;
        }

        public string GetMd5(string text)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    var hash = md5.ComputeHash(stream);
                    var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return hashString;
                }
            }
        }
    }
}
