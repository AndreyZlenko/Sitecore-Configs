using System;
using System.IO;
using System.Threading;

namespace BuildConfigTransformation.Services
{
    public static class FileService
    {
        public static string CreateTempFile(string stringToWrite = "", string fileExtension = "vstmp")
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.{1}", Guid.NewGuid(), fileExtension));

            using (StreamWriter streamWriter = new StreamWriter(tempFilePath, false, System.Text.Encoding.UTF8))
            {
                streamWriter.Write(stringToWrite);
            }

            return tempFilePath;
        }

        public static void RemoveTempFile(string tempFilePath)
        {
            for(int i = 0; i < 5; i++)
            {
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }

                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                    continue;
                }
                return;
            }
        }
    }
}
