using System;
using System.IO;

namespace Service.StockMarket.Services
{
    public class FileDirectoryService
    {
        public void WriteOnFile(string message, string path)
        {
            try
            {
                var writer = new StreamWriter(path, true);
                writer.WriteLine($"{DateTime.Now} - {message}");
                writer.Close();
                writer.Dispose();
            }
            catch (Exception) { throw; }
        }

        public void IfDoesNotExistsCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception) { throw; }
        }

        public void IfDoesNotExistsCreateFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    var stream = File.Create(path);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception) { throw; }
        }
    }
}
