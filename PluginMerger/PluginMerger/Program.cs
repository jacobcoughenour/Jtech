using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PluginMerger{

    internal class Program {

        private readonly static Regex isUsingReg;
        private readonly static Regex isNamespaceReg;
        private readonly static Regex isCommentReg;

        static Program(){

            Program.isUsingReg = new Regex("^using\\s+?[^\\(]+\\;$");
            Program.isNamespaceReg = new Regex("^using\\s+(.+?);$");
        }

        public Program() {}

        private static string GetNameSpace(string line) {
            return Program.isNamespaceReg.Match(line).Groups[1].Value;
        }
        
        private static bool IsUsingLine(string line) {
            return Program.isUsingReg.IsMatch(line);
        }

        private static int Main(string[] args)
        {
            Console.WriteLine(string.Concat("Args: ", string.Join("\n", args), "\n"));
            if ((int)args.Length != 2) {
                Console.WriteLine("Usage: PluginMerger.exe \"Source\" \"Target\".");
                return 1;
            }

            using (StreamWriter streamWriter = File.CreateText(args[1])) { // write file to target (args[1])

                List<string> usingStrings = new List<string>();
                // TODO: Dictonary of namespaces and the strings they contain

                StringBuilder stringBuilder = new StringBuilder();

                string[] files = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories);

                for (int i = 0; i < (int)files.Length; i++) { // for each file in directory

                    string curfile = files[i];
                    if (Path.GetFileName(curfile) != "AssemblyInfo.cs") { // ignore AssemblyInfo.cs
                        
                        List<string> strs1 = new List<string>(File.ReadAllLines(curfile));
                        List<string> list = strs1.ToList<string>(); // TODO: why is this here?

                        int lineoffset = 0; // count lines removed
                        for (int j = 0; j < list.Count; j++) { // for each line in curfile

                            string item = list[j];
                            if (Program.IsUsingLine(item)) {

                                Program.GetNameSpace(item); // TODO: why is this here?
                                if (!usingStrings.Contains(item)) 
                                    usingStrings.Add(item);

                                strs1.RemoveAt(j - lineoffset); // TODO: instead of removing, just add to new list
                                lineoffset++;
                            }
                        }
                        stringBuilder.AppendLine(string.Join("\n", strs1)); // append lines
                    }
                }
                stringBuilder.Insert(0, string.Concat(string.Join("\n", usingStrings), "\n")); // add usingStrings to the top
                streamWriter.Write(stringBuilder.ToString()); // write to file
            }
            return 0;
        }
    }
}