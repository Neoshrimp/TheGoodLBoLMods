using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace RngFix.Patches
{


    public sealed class Logger : IDisposable
    {
        private static readonly Logger instance = new Logger();
        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;
        private readonly object lockObj = new object();

        private Logger()
        {
            string filePath = $"{Application.persistentDataPath}/extraDeez.txt";
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream);
        }

        public static Logger i => instance;

        public void Log(string message)
        {
            //var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            var logEntry = message;

            lock (lockObj)
            {
                _streamWriter.WriteLine(logEntry);
                _streamWriter.Flush();
            }
        }

        public void Dispose()
        {
            lock (lockObj)
            {
                _streamWriter?.Dispose();
                _fileStream?.Dispose();
            }
        }
    }

}
