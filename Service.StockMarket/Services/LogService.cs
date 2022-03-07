using System;
using System.Configuration;

namespace Service.StockMarket.Services
{
    public class LogService
    {
        private readonly FileDirectoryService _fileDirectoryService;

        public LogService()
        {
            _fileDirectoryService = new FileDirectoryService();
        }

        public void Log(string message)
        {
            try
            {
                var logDirectory = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["logDirectory"];
                var logFile = logDirectory + ConfigurationManager.AppSettings["logFile"];

                _fileDirectoryService.IfDoesNotExistsCreateDirectory(logDirectory);
                _fileDirectoryService.IfDoesNotExistsCreateFile(logFile);
                _fileDirectoryService.WriteOnFile(message, logFile);
            }
            catch (Exception) { throw; }
        }
    }
}
