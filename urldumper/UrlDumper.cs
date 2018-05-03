using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace urldumper
{
    class UrlDumper
    {
        public void Dump(string input, string output)
        {
            string tempfile = System.IO.Path.GetTempPath() + "recovery.json";
            try
            {
                DecompressJson(input, tempfile);
                DumpJson(tempfile, output);
            }
            finally
            {
                if(File.Exists(tempfile))
                {
                    File.Delete(tempfile);
                }
            }
        }

        static void DecompressJson(string input, string output)
        {
            var decompressor = System.IO.Path.GetTempPath() + "decompressor.exe";

            try
            {
                File.WriteAllBytes(decompressor, Properties.Resources.dejsonlz4);
                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = decompressor,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"{input} {output}"
                };
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
            }
            catch (Exception e)
            {
                // Log error.
                Console.WriteLine(e);
            }
            finally
            {
                if(File.Exists(decompressor))
                {
                    File.Delete(decompressor);
                }
            }
        }

        static void DumpJson(string input, string output)
        {
            try
            {   // Open the text file using a stream reader.
                using (var fileStream = new FileStream(input, FileMode.Open))
                using (var reader = new StreamReader(fileStream))
                using (var file = new StreamWriter(output))
                {
                    string line = reader.ReadToEnd();
                    var json = JObject.Parse(line);
                    foreach(var w in json.SelectToken("windows").Children())
                    {
                        foreach(var t in w.SelectToken("tabs").Children())
                        { 
                            try
                            {
                                JArray arr = (JArray)t.First.First;
                                foreach (var jtoken in arr)
                                {
                                    var link = jtoken.SelectToken("url").ToString();
                                    var title = jtoken.SelectToken("title").ToString();
                                    file.WriteLine($"<a href=\"{link}\">{title}</a><br />");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
