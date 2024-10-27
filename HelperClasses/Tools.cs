using Common.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Lib.HelperClasses
{
    public static class Tools
    {
        public static string[] DirSearch(string sDir)
        {
            List<string> result = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, "pattern"))
                    {
                        result.Add(f);
                    }
                    DirSearch(d);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return result.ToArray();
        }

        public static string CalculateMD5Hash(FileInfo filename)
        {
            MD5 md5 = MD5.Create();
            FileStream stream = File.OpenRead(filename.FullName);
            byte[] hash = md5.ComputeHash(stream);
            stream.Close();

            return ConvertByteArrayToHex(hash);
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input

            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            return ConvertByteArrayToHex(hash);
        }

        private static string ConvertByteArrayToHex(byte[] hash)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        // If you want to implement both "*" and "?"
        public static string WildCardToRegular(string value)
        {
            string regex = "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            return regex;
        }

        public static FileInfo SaveFileStream(Stream stream, string filePath, bool overwrite = false)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                if (overwrite)
                    fileInfo.Delete();
                else
                {
                    int c = 1;
                    do
                    {
                        string newFileName = $"{fileInfo.DirectoryName}\\{fileInfo.Name.Replace($".{fileInfo.Extension}", "")}({c}){fileInfo.Extension}";
                        if (!File.Exists(newFileName))
                        {
                            fileInfo = new FileInfo(newFileName);
                            break;
                        }
                        c++;
                    } while (true);
                }
            }

            using (var fileStream = File.Create(fileInfo.FullName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return new FileInfo(fileInfo.FullName);
        }

        public static async Task CopyFileWithProgressAsync(string sourceFileName, string destinationFileName, bool overwrite, IProgress<int> progress)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                try
                {
                    XCopy.Copy(sourceFileName, destinationFileName, overwrite, true, (o, pce) =>
                    {
                        worker.ReportProgress(pce.ProgressPercentage, sourceFileName);
                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };
            worker.ProgressChanged += (s, e) => progress.Report(e.ProgressPercentage);
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();

            await Task.Run(() => { while (worker.IsBusy) { } });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destFilePath"></param>
        /// <param name="patternToDelete">Supports wildcard expressions '*' and '?'</param>
        public static void DeleteLinesFromTextFile(string sourceFilePath, string destFilePath, string patternToDelete)
        {
            var pattern = WildCardToRegular(patternToDelete);
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                using (StreamWriter writer = new StreamWriter(destFilePath))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!Regex.IsMatch(line, pattern))
                            writer.WriteLine(line);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destFilePath"></param>
        /// <param name="patternToFind">Supports wildcard expressions '*' and '?'</param>
        /// <param name="replaceWith"></param>
        public static void ReplacePatternAllLinesFromTextFile(string sourceFilePath, string destFilePath, string patternToFind, string replaceWith)
        {
            var pattern = WildCardToRegular(patternToFind);
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                using (StreamWriter writer = new StreamWriter(destFilePath))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, pattern))
                        {
                            //var match = Regex.Match(line, pattern);
                            var text = Regex.Replace(line, pattern.Replace("^.*", ""), "");
                            writer.WriteLine(text + replaceWith);
                        }
                        else
                            writer.WriteLine(line);
                    }
                }
            }
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        public static string ExtractResourceToLocalFile(string resourceName, string assemblyName, string filePath = null)
        {
            if (filePath.IsNullOrEmpty())
                filePath = Path.GetTempFileName();

            var assembly = GetAssemblyByName(assemblyName);

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream reader = new MemoryStream())
            {
                stream.CopyTo(reader);
                File.WriteAllBytes(filePath, reader.ToArray());
            }

            return filePath;
        }

        public static string ExtractResourceFileContentToString(string resourceName, string assemblyName)
        {
            var assembly = GetAssemblyByName(assemblyName);

            return ExtractResourceFileContentToString(resourceName, assembly);
        }

        public static string ExtractResourceFileContentToString(string resourceName, Type typeDeclaredInTargetAssembly)
        {
            var assembly = Assembly.GetAssembly(typeDeclaredInTargetAssembly);

            return ExtractResourceFileContentToString(resourceName, assembly);
        }

        public static string ExtractResourceFileContentToString(string resourceName, Assembly assembly)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        public static string ConvertStringEncoding(string utf16String, Encoding inputEncoding, Encoding outputEncoding, Encoding outputStringEncoding)
        {
            if (inputEncoding == outputEncoding)
                return utf16String;

            // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
            byte[] inputBytes = inputEncoding.GetBytes(utf16String);
            byte[] outputBytes = Encoding.Convert(inputEncoding, outputEncoding, inputBytes);

            // Return UTF8 bytes as ANSI string
            return outputStringEncoding.GetString(outputBytes);
        }

        public static int GreatestCommonDivider(int num1, int num2)
        {
            if (num2 == 0)
                return num1;
            return GreatestCommonDivider(num2, num1 % num2);
        }

        public static int LeastCommonMultipleOfArray(IEnumerable<int> arr)
        {
            int lcm = arr.Max();
            for (int i = 0; i < arr.Count(); i++)
            {
                int num1 = lcm;
                int num2 = arr.ElementAt(i);
                int gcdVal = GreatestCommonDivider(num1, num2);
                lcm = num2 == 0 ? lcm : gcdVal;
            }
            return lcm;
        }
    }
}
