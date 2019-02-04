﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AllInOne.Forms;

namespace AllInOne.Logic
{
    public static class Patcher
    {
        private static object locker = new object();
        public static MainForm mainf;
        public static int replacedCount = 0;
        public static string patchName = "";

        public static void loadAllInOne(string[] args)
        {
            FileInfo fileInfo;
            if (!Program.standalone)
            {
                Program.workdir = args[3];
                Program.ApkDir = args[1];
                Program.pathToBatchapktool = args[4].Replace("\"", "");
                if ("*".Equals(Program.ApkDir))
                {
                    if (".".Equals(Program.workdir))
                    {
                        Program.processApkPath = Program.pathToBatchapktool + "\\_INPUT_APK";
                    }
                    else
                    {
                        Program.processApkPath = Program.pathToBatchapktool + "\\" + Program.workdir + "\\_INPUT_APK";
                    }
                }
                else if (".".Equals(Program.workdir))
                {
                    Program.processApkPath = Program.pathToBatchapktool + "\\_INPUT_APK\\" + Program.ApkDir;
                }
                else
                {
                    Program.processApkPath = string.Concat(new string[]
                    {
                        Program.pathToBatchapktool,
                        "\\",
                        Program.workdir,
                        "\\_INPUT_APK\\",
                        Program.ApkDir
                    });
                }
                fileInfo = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                Program.pathToMyPluginDir = fileInfo.DirectoryName;
                Language.Load(args[2]);
                return;
            }
            fileInfo = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            Program.pathToMyPluginDir = fileInfo.DirectoryName;
            Program.pathToBatchapktool = fileInfo.DirectoryName.Substring(0, fileInfo.DirectoryName.IndexOf("\\bin\\"));
            Program.workdir = ".";
            Program.ApkDir = "NONE";
            Program.processApkPath = "";
            Language.Load(Settings.language);
        }

        public static void setMainForm(ref MainForm f)
        {
            Patcher.mainf = f;
        }

        public static void WriteLog(string str)
        {
            object obj = Patcher.locker;
            lock (obj)
            {
                File.AppendAllText(Program.pathToBatchapktool + "\\AllInOne_log.txt", DateTime.Now.ToString("HH:mm:ss") + " :\n" + str);
            }
        }

        public static string getJavaPath()
        {
            string result;
            try
            {
                result = Directory.GetFiles(Program.pathToBatchapktool + "\\bin", "java.exe", SearchOption.AllDirectories)[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("getJavaPath\n" + ex.Message + "\n" + ex.ToString());
                result = "java.exe";
            }
            return result;
        }

        public static ExcludeRes deSerializeExcludeResFromXml(string filePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExcludeRes));
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            ExcludeRes result = (ExcludeRes)xmlSerializer.Deserialize(fileStream);
            fileStream.Close();
            return result;
        }

        public static Sensors deSerializeSensorsFromXml(string filePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Sensors));
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Sensors result = (Sensors)xmlSerializer.Deserialize(fileStream);
            fileStream.Close();
            return result;
        }

        public static void WriteDebugLog(string str)
        {
            object obj = Patcher.locker;
            lock (obj)
            {
                File.AppendAllText(Program.pathToBatchapktool + "\\AllInOne_DEBUG.txt", str);
            }
        }

        public static void DelDebugLogFile()
        {
            object obj = Patcher.locker;
            lock (obj)
            {
                File.Delete(Program.pathToBatchapktool + "\\AllInOne_DEBUG.txt");
            }
        }

        public static string getSignatureData(string apkPath)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = Patcher.getJavaPath();
            process.StartInfo.Arguments = string.Concat(new string[]
            {
                "-jar \"",
                Program.pathToMyPluginDir,
                "\\nkstoolMod.jar\" \"",
                apkPath,
                "\""
            });
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        public static void filterExclude(ref List<string> list, ExcludeRes eRes)
        {
            if (list.Count == 0) { return; }

            for (int i = 0; i < list.Count; i++)
            {
                string dirname = Path.GetFileName(list[i]).Substring(0, Path.GetFileName(list[i]).IndexOf("-"));
                if (list[i].EndsWith("-" + eRes.basis))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.anim && "anim".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.animator && "animator".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.color && "color".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.drawable && "drawable".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.font && "font".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.layout && "layout".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.menu && "menu".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.mipmap && "mipmap".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.raw && "raw".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.values && "values".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (!eRes.xml && "xml".Equals(dirname))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                int excludeCount = 0;

                MatchCollection mCol = Regex.Matches(Path.GetFileName(list[i]), @"-([^-]+)");
                foreach (Match m in mCol)
                {
                    foreach (string lang in eRes.langList)
                    {
                        if (m.Groups[1].Value.Equals(lang))
                        {
                            excludeCount++;
                            break;
                        }
                    }

                    foreach (string qual in eRes.qualList)
                    {
                        if (m.Groups[1].Value.Equals(qual))
                        {
                            excludeCount++;
                            break;
                        }
                    }
                }
                if (excludeCount == mCol.Count)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void filterLibs(ref List<string> list, ExcludeRes eRes)
        {
            if (list.Count == 0) { return; }
            if (!eRes.lib)
            {
                list.Clear();
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                foreach (string lib in eRes.libList)
                {
                    if (Path.GetFileName(list[i]).Equals(lib))
                    {
                        list.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }

        public static void moveToClassesNorNot(string apkDir, string smaliPath)
        {
            if (!File.Exists(smaliPath)) { return; }

            string[] dirs = Directory.GetDirectories(apkDir, "smali*");

            if ("smali".Equals(Path.GetFileName(dirs[dirs.Length - 1])))
            {
                return;
            }
            else
            {
                string dest = apkDir + "\\" + Path.GetFileName(dirs[dirs.Length - 1]) + smaliPath.Remove(0, apkDir.Length + 6);
                if (!Directory.Exists(Path.GetDirectoryName(dest)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                }
                File.Move(smaliPath, dest);
            }
        }

        public static void copyFileOrNot(string from, string to, bool copy)
        {
            if (!copy)
            {
                return;
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(to)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(to));
                }
                File.Copy(from, to, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("Error in copyFileOrNot\nError message:" + ex.Message + "\n" + ex.ToString());
            }
        }

        public static byte[] stringToBytes(string str)
        {
            string[] array = str.Split(new char[]
            {
                ' '
            });
            byte[] array2 = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array2[i] = Convert.ToByte(array[i], 16);
            }
            return array2;
        }

        public static string stringToHex(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(str))
            {
                stringBuilder.Append(b.ToString("X2"));
                stringBuilder.Append(" ");
            }
            return stringBuilder.ToString().TrimEnd(new char[0]);
        }

        public static string convertUnicodeToText(string text)
        {
            string result = "";
            try
            {
                UTF8Encoding utf = new UTF8Encoding();
                UnicodeEncoding unicode = new UnicodeEncoding();
                foreach (Match m in Regex.Matches(text, @"\\u([0-9a-fA-F]+)"))
                {
                    text = text.Replace(m.Value, ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                }
                result = utf.GetString(Encoding.Convert(unicode, utf, unicode.GetBytes(text))).Replace("\r", "").Replace("\n", "");
            }
            catch (Exception e)
            {
                return "";
            }
            return result;
        }

        public static string convertBase64ToText(string text)
        {
            UTF8Encoding utf = new UTF8Encoding();
            UnicodeEncoding unicode = new UnicodeEncoding();
            string result = "";
            byte[] resultBytes;

            try
            {
                if (text.Length % 4 == 0 && text.Length >= 4)
                {
                    if (text.Length == 4 && text.IndexOf("=") == -1) { return ""; }

                    resultBytes = Convert.FromBase64String(text);

                    result = utf.GetString(resultBytes).Replace("\r", "").Replace("\n", "");
                    foreach (char c in result)
                    {
                        if (Char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter || Char.GetUnicodeCategory(c) == UnicodeCategory.OtherSymbol || Char.GetUnicodeCategory(c) == UnicodeCategory.Control)
                        {
                            result = "";
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return "";
            }
            return result;
        }

        public static string convertArrayDataToString(string text)
        {
            try
            {
                string result = "";
                List<string> replaced = new List<string>();
                string[] letters = new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", ".", ",", "\'", "\"", "%", "\\", "@", ":", ";", "(", ")", "*", "&", "[", "]", "/", "_", "-", "+", "=", "?", "{", "}", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", " " };
                string[] lettersHex = new string[] { "-0x30t-0x70t", "-0x30t-0x6ft", "-0x30t-0x6et", "-0x30t-0x6dt", "-0x30t-0x6ct", "-0x30t-0x6bt", "-0x30t-0x7ft", "-0x30t-0x6at", "-0x30t-0x69t", "-0x30t-0x68t", "-0x30t-0x67t", "-0x30t-0x66t", "-0x30t-0x65t", "-0x30t-0x64t", "-0x30t-0x63t", "-0x30t-0x62t", "-0x30t-0x61t", "-0x30t-0x60t", "-0x30t-0x5ft", "-0x30t-0x5et", "-0x30t-0x5dt", "-0x30t-0x5ct", "-0x30t-0x5bt", "-0x30t-0x5at", "-0x30t-0x59t", "-0x30t-0x58t", "-0x30t-0x57t", "-0x30t-0x56t", "-0x30t-0x55t", "-0x30t-0x54t", "-0x30t-0x53t", "-0x30t-0x52t", "-0x30t-0x51t", "-0x30t-0x50t", "-0x30t-0x4ft", "-0x30t-0x4et", "-0x30t-0x4dt", "-0x30t-0x4ct", "-0x30t-0x4bt", "-0x2ft-0x6ft", "-0x30t-0x4at", "-0x30t-0x49t", "-0x30t-0x48t", "-0x30t-0x47t", "-0x30t-0x46t", "-0x30t-0x45t", "-0x30t-0x44t", "-0x30t-0x43t", "-0x30t-0x42t", "-0x30t-0x41t", "-0x2ft-0x80t", "-0x2ft-0x7ft", "-0x2ft-0x7et", "-0x2ft-0x7dt", "-0x2ft-0x7ct", "-0x2ft-0x7bt", "-0x2ft-0x7at", "-0x2ft-0x79t", "-0x2ft-0x78t", "-0x2ft-0x77t", "-0x2ft-0x76t", "-0x2ft-0x75t", "-0x2ft-0x74t", "-0x2ft-0x73t", "-0x2ft-0x72t", "-0x2ft-0x71t", "0x41t", "0x42t", "0x43t", "0x44t", "0x45t", "0x46t", "0x47t", "0x48t", "0x49t", "0x4at", "0x4bt", "0x4ct", "0x4dt", "0x4et", "0x4ft", "0x50t", "0x51t", "0x52t", "0x53t", "0x54t", "0x55t", "0x56t", "0x57t", "0x58t", "0x59t", "0x5at", "0x61t", "0x62t", "0x63t", "0x64t", "0x65t", "0x66t", "0x67t", "0x68t", "0x69t", "0x6at", "0x6bt", "0x6ct", "0x6dt", "0x6et", "0x6ft", "0x70t", "0x71t", "0x72t", "0x73t", "0x74t", "0x75t", "0x76t", "0x77t", "0x78t", "0x79t", "0x7at", "0x2et", "0x2ct", "0x27t", "0x22t", "0x25t", "0x5ct", "0x40t", "0x3at", "0x3bt", "0x28t", "0x29t", "0x2at", "0x26t", "0x5bt", "0x5dt", "0x2ft", "0x5ft", "0x2dt", "0x2bt", "0x3dt", "0x3ft", "0x7bt", "0x7dt", "0x31t", "0x32t", "0x33t", "0x34t", "0x35t", "0x36t", "0x37t", "0x38t", "0x39t", "0x30t", "0x20t" };
                UTF8Encoding utf = new UTF8Encoding();
                ASCIIEncoding asci = new ASCIIEncoding();
                UnicodeEncoding unicode = new UnicodeEncoding();

                text = text.Replace("\r\n", "");
                text = text.Replace(" ", "");

                for (int i = 0; i < letters.Length; i++)
                {
                    if (replaced.Contains(letters[i])) { continue; }

                    text = text.Replace(lettersHex[i], letters[i]);
                    replaced.Add(letters[i]);
                }
                if (Regex.Matches(text, @"-?0x[a-fA-F0-9]+t").Count != 0) { return ""; }

                replaced.Clear();
                result = utf.GetString(Encoding.Convert(unicode, utf, unicode.GetBytes(text))).Replace("\r", "").Replace("\n", "");

                return result;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static string getCurrentTime()
        {
            return "0x" + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString("X") + "L";
        }

        public static string parseDateTime(string dateTime, string path)
        {
            string result;
            try
            {
                result = "0x" + ((long)(Convert.ToDateTime(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString("X") + "L";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);

                Patcher.WriteLog("Error in parseDateTime\nError message:" + ex.Message + "\n" + ex.ToString());
                result = "";
            }
            return result;
        }

        public static string ReadRSA(string path)
        {
            if (!File.Exists(path + "\\original\\META-INF\\CERT.RSA"))
            {
                MessageBox.Show(path + Language.errorMsgRsa, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return "";
            }
            FileStream fileStream = new FileStream(path + "\\original\\META-INF\\CERT.RSA", FileMode.Open);
            string text = "";
            int num = 0;
            int num2;
            while ((num2 = fileStream.ReadByte()) != -1)
            {
                text += string.Format("{0:X2}", num2);
                num++;
            }
            return text;
        }

        public static string TrimPathToInput(string str)
        {
            string result;
            try
            {
                result = str.Remove(0, str.IndexOf("_INPUT_APK") + "_INPUT_APK\\".Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("Error in TrimPathToInput\nError message: " + ex.Message + "\n" + ex.ToString());
                result = str;
            }
            return result;
        }

        public static string getPathToSmali(string path, string activityName)
        {
            string result = "";
            if (activityName.StartsWith("."))
            {
                activityName = Patcher.GetAppPackage(path) + activityName;
            }
            if (File.Exists(path + "\\smali\\" + activityName.Replace(".", "\\") + ".smali"))
            {
                result = path + "\\smali\\" + activityName.Replace(".", "\\") + ".smali";
            }
            else
            {
                for (int i = 0; i < 150; i++)
                {
                    if (File.Exists(string.Concat(new string[]
                    {
                        path,
                        "\\smali_classes",
                        i.ToString(),
                        "\\",
                        activityName.Replace(".", "\\"),
                        ".smali"
                    })))
                    {
                        result = string.Concat(new string[]
                        {
                            path,
                            "\\smali_classes",
                            i.ToString(),
                            "\\",
                            activityName.Replace(".", "\\"),
                            ".smali"
                        });
                        break;
                    }
                }
            }
            return result;
        }

        public static bool ReplaceInFile(string path, Dictionary<string, string> Patterns, bool formlogging = true)
        {
            if (path == "") { return false; }

            StringBuilder log = new StringBuilder();

            try
            {
                string fileContent = File.ReadAllText(path, Encoding.UTF8);
                int orig = fileContent.GetHashCode();
                foreach (var patternPair in Patterns)
                {
                    fileContent = Regex.Replace(fileContent, patternPair.Key, (m) =>
                    {
                        string newValue = Regex.Replace(m.Value, patternPair.Key, patternPair.Value);
                        if (Settings.writeDebug)
                        {
                            log.AppendLine("AllinOne version= " + mainf.version);
                            log.AppendLine("File= " + TrimPathToInput(path) + "\nPattern=\n" + patternPair.Key + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                            log.AppendLine("====================================================================================================================================================");
                        }
                        return newValue;
                    });
                }
                if (orig != fileContent.GetHashCode())
                {
                    using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(false), 65536))
                    {
                        sw.WriteLine(fileContent);
                    }
                    if (formlogging) { mainf.appendProgressTbox("    " + Language.log_patched + TrimPathToInput(path)); }
                    if (Settings.writeDebug)
                    {
                        WriteDebugLog(log.ToString());
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLog("Error in ReplaceInFile\nError message:" + e.Message + "\n" + e.ToString());
                return false;
            }
            return false;
        }

        public static void ReplaceInFileEX(string path, ref string fileContent, Dictionary<string, string> Patterns, bool IgnoreCase)
        {
            RegexOptions regopt = RegexOptions.None;
            StringBuilder log = new StringBuilder();
            if (IgnoreCase)
            {
                regopt = RegexOptions.IgnoreCase;
            }
            int orig = fileContent.GetHashCode();

            try
            {
                foreach (var patternPair in Patterns)
                {
                    fileContent = Regex.Replace(fileContent, patternPair.Key, (m) =>
                    {
                        string newValue = Regex.Replace(m.Value, patternPair.Key, patternPair.Value, regopt);
                        if (Settings.writeDebug)
                        {
                            log.AppendLine("AllinOne version= " + mainf.version);
                            log.AppendLine("File= " + TrimPathToInput(path) + "\nPattern=\n" + patternPair.Key + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                            log.AppendLine("====================================================================================================================================================");
                        }
                        return newValue;
                    }, regopt);
                }
                if (orig != fileContent.GetHashCode())
                {
                    if (Settings.writeDebug)
                    {
                        WriteDebugLog(log.ToString());
                    }
                }

            }
            catch (Exception e)
            {
                WriteLog("Error in ReplaceInFileEx\nError message:" + e.Message + "\n" + e.ToString());
            }
        }

        public static bool ReplaceInFiles(string path, Dictionary<string, string> patterns, string filemask, string dirmask, bool formlogging = true)
        {
            List<string> foundedFiles = new List<string>();
            int copy = 0;
            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);
            if (path == "") { return false; }
            foreach (string foundedDir in Directory.GetDirectories(path, dirmask))
            {
                foundedFiles.AddRange(Directory.GetFiles(foundedDir, filemask, SearchOption.AllDirectories));
            }

            foreach (string foundedfile in foundedFiles)
            {
                work.Add(new Task(() =>
                {
                    if (ReplaceInFile(foundedfile, patterns, formlogging))
                    {
                        Interlocked.Increment(ref copy);
                    }
                }));
            }

            work.Start();

            return copy > 0;
        }

        public static void ReplaceInSoLib(string soPath, Dictionary<string, string> values, bool formlogging = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = false;
            byte[] array = File.ReadAllBytes(soPath);
            foreach (KeyValuePair<string, string> keyValuePair in values)
            {
                byte[] array2 = Patcher.stringToBytes(keyValuePair.Key);
                byte[] array3 = Patcher.stringToBytes(keyValuePair.Value);
                List<int> list = Patcher.indexOfBlock(array, array2);
                if (list.Count > 0)
                {
                    flag = true;
                }
                if (Settings.writeDebug && flag)
                {
                    stringBuilder.AppendLine("AllinOne version= " + mainf.version);
                    stringBuilder.AppendLine("Patch= " + Patcher.patchName);
                    stringBuilder.AppendLine(string.Concat(new string[]
                    {
                        "File= ",
                        Patcher.TrimPathToInput(soPath),
                        "\nOld=\n",
                        keyValuePair.Key,
                        "\nNew=\n",
                        keyValuePair.Value
                    }));
                    stringBuilder.AppendLine("====================================================================================================================================================");
                }
                foreach (int num in list)
                {
                    for (int i = 0; i < array2.Length; i++)
                    {
                        array[num + i] = array3[i];
                    }
                }
            }
            if (flag)
            {
                if (formlogging)
                {
                    Patcher.mainf.appendProgressTbox("    "+ Language.log_patched + Patcher.TrimPathToInput(soPath));
                }
                File.WriteAllBytes(soPath, array);
            }
            Patcher.WriteDebugLog(stringBuilder.ToString());
        }

        public static void ReplaceInSoLibs(string path, Dictionary<string, string> values, bool formlogging = true)
        {
            TaskPool taskPool = new TaskPool(Settings.TaskCount, ref Patcher.mainf.taskCountLabel, ref Patcher.mainf.pBar);
            if (!(path == "") && Directory.Exists(path + "\\lib"))
            {
                string[] files = Directory.GetFiles(path + "\\lib", "*.so", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string foundedfile = files[i];
                    taskPool.Add(new Task(delegate ()
                    {
                        Patcher.ReplaceInSoLib(foundedfile, values, formlogging);
                    }));
                }
                taskPool.Start();
                return;
            }
        }

        public static string GetAppPackage(string path)
        {
            return Regex.Match(File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8), "package=\\\"([^\\\"]+)\\\"").Groups[1].Value;
        }

        public static string GetLauncherActivity(string path)
        {
            return Regex.Match(File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8), "<activity.+?android:name=\\\"([^\\\"]+?)\\\".*?(?<!/)>[\\s\\S]+?android\\.intent\\.category\\.LAUNCHER\\\"\\s*?/>").Groups[1].Value;
        }

        public static string GetLauncherActivityNoRegex(string path)
        {
            string result = "";
            string text = File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8);
            try
            {
                text = text.Remove(text.IndexOf("android.intent.category.LAUNCHER"), text.Length - text.IndexOf("android.intent.category.LAUNCHER"));
                text = text.Remove(0, text.LastIndexOf("<activity "));
                text = text.Remove(0, text.IndexOf("android:name=\"") + "android:name=\"".Length);
                result = text.Split(new char[]
                {
                    '"'
                })[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("GetLauncherActivityNoRegex\n" + ex.Message + "\n" + ex.ToString());
            }
            return result;
        }

        public static int getLineNumberInFile(string filePath, string text)
        {
            string text2 = File.ReadAllText(filePath, Encoding.UTF8);
            text2 = text2.Substring(0, text2.IndexOf(text));
            return text2.Split(new string[]
            {
                "\n"
            }, StringSplitOptions.None).Length;
        }

        public static int getLineNumberInFileContent(string fileContent, string text)
        {
            fileContent = fileContent.Substring(0, fileContent.IndexOf(text));
            return fileContent.Split(new string[]
            {
                "\n"
            }, StringSplitOptions.None).Length;
        }

        public static void openTextEditor(string filePath, int lineNumber)
        {
            new Task(() =>
            {
                try
                {
                    if (!File.Exists(Settings.textEditorPath)) { return; }

                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = Settings.textEditorPath;
                    p.StartInfo.Arguments = Settings.textEditorArgs.Replace("%LINENUMBER%", lineNumber.ToString()).Replace("%FILEPATH%", " \"" + filePath + " \"");//"-n" + lineNumber.ToString() + " \"" + filePath + " \"";
                    p.Start();
                }
                catch (Exception e)
                {
                    MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLog("Error in openTextEditor\nError message:" + e.Message + "\n" + e.ToString());
                }
            }).Start();
        }

        public static void deleteAutostart(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.AutostartPattern, true);
        }

        public static void remDebugInfo(string path)
        {
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_remove_dbg_info + Patcher.TrimPathToInput(path) + " :::::");
            Patcher.ReplaceInFiles(path, new Dictionary<string, string>
            {
                {
                    "[\\r\\n]{2}\\s{0,4}(\\.prologue|\\.source.+|\\.line\\s\\d+)",
                    ""
                }
            }, "*.smali", "smali*", false);
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_remove_dbg_info_done + ":::::");
        }

        public static void addDebugInfo(string path)
        {
            List<string> foundedFiles = new List<string>();
            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);
            mainf.appendProgressTbox(":::::" + Language.log_add_dbg_info + TrimPathToInput(path) + " :::::");
            int fileNum = 1;

            foreach (string foundedDir in Directory.GetDirectories(path, "smali*"))
            {
                foundedFiles.AddRange(Directory.GetFiles(foundedDir, "*.smali", SearchOption.AllDirectories));
            }

            foreach (string foundedfile in foundedFiles)
            {
                work.Add(new Task(() =>
                {
                    string[] fileContent = File.ReadAllLines(foundedfile, Encoding.UTF8);

                    using (StreamWriter sw = new StreamWriter(foundedfile, false, new UTF8Encoding(false), 65536))
                    {
                        int lineNum = 0;
                        bool skipSource = false;
                        bool doInsert = true;
                        bool inMethod = false;
                        foreach (string line in fileContent)
                        {
                            lineNum++;
                            if (line.Trim().StartsWith(".source") && skipSource)
                            {
                                continue; ;
                            }
                            if (line.Trim().StartsWith(".super"))
                            {
                                skipSource = true;
                                sw.WriteLine(line);
                                sw.WriteLine(".source \"SourceFile_" + fileNum.ToString() + "\"");
                                continue;
                            }

                            if ("".Equals(line.Trim()))
                            {
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".method"))
                            {
                                inMethod = true;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".end method"))
                            {
                                inMethod = false;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".packed-switch"))
                            {
                                doInsert = false;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".end packed-switch"))
                            {
                                doInsert = true;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".array-data"))
                            {
                                doInsert = false;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".end array-data"))
                            {
                                doInsert = true;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".sparse-switch"))
                            {
                                doInsert = false;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".end sparse-switch"))
                            {
                                doInsert = true;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".annotation"))
                            {
                                doInsert = false;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(".end annotation"))
                            {
                                doInsert = true;
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith("."))
                            {
                                sw.WriteLine(line);
                                continue;
                            }
                            if (line.Trim().StartsWith(":"))
                            {
                                sw.WriteLine(line);
                                continue;
                            }
                            if (inMethod)
                            {
                                if (doInsert)
                                {
                                    sw.WriteLine("    .line " + lineNum.ToString());
                                    sw.WriteLine(line);
                                }
                                else
                                {
                                    sw.WriteLine(line);
                                }
                            }
                            else
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                    Interlocked.Increment(ref fileNum);
                }));

            }

            work.Start();
            mainf.appendProgressTbox("    " + Language.log_patched + fileNum.ToString() + " files");
            mainf.appendProgressTbox(":::::" + Language.log_add_dbg_info_done + ":::::");
        }

        public static string raiseVersion(string str)
        {
            return Regex.Replace(str, @"\d", "9");
        }

        public static void disableAutoUpdate(string path)
        {
            StringBuilder log = new StringBuilder();
            string versionPattern = @"versionCode: ('?[^']+'?)[\s\S]+versionName: ('?[^']+'?)";

            Dictionary<string, string> pattern = new Dictionary<string, string>();
            string imlContent = File.ReadAllText(path + "\\apktool.yml", Encoding.UTF8);

            Match vers = Regex.Match(imlContent, versionPattern);
            string versionCode = vers.Groups[1].Value;
            string versionName = vers.Groups[2].Value;
            imlContent = Regex.Replace(imlContent, versionPattern, (m) =>
            {
                string newValue = Regex.Replace(m.Value, versionPattern, "versionCode: '2147483646'\n  versionName: " + raiseVersion(m.Groups[2].Value));
                if (Settings.writeDebug)
                {
                    log.AppendLine("File= " + TrimPathToInput(path + "\\apktool.yml") + "\nPattern=\n" + @"versionCode: ('?[^']+'?)[\s\S]+versionName: ('?[^']+'?)" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                    log.AppendLine("====================================================================================================================================================");
                }
                return newValue;
            });
            mainf.appendProgressTbox("    "+ Language.log_patched + TrimPathToInput(path + "\\apktool.yml"));
            using (StreamWriter sw = new StreamWriter(path + "\\apktool.yml", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine(imlContent);
            }
            pattern.Add("\"" + versionCode.Replace("'", "") + "\"", "\"2147483646\"");
            pattern.Add("\"" + versionName.Replace("'", "") + "\"", "\"" + raiseVersion(versionName.Replace("'", "")) + "\"");

            ReplaceInFiles(path, pattern, "*.smali", "smali*");
            WriteDebugLog(log.ToString());
        }

        public static void addSecondaryInfo(string path)
        {
            List<string> foundedFiles = new List<string>();
            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);
            mainf.appendProgressTbox(":::::" + Language.log_add_smali_secondary_info + TrimPathToInput(path) + " :::::");
            //key=id value={type, name, value}
            Dictionary<string, string[]> foundedInPublic = new Dictionary<string, string[]>();//key=id value={type, name}
            Dictionary<string, string> stringRuValues = new Dictionary<string, string>();//key=name value=value
            Dictionary<string, string> stringValues = new Dictionary<string, string>();//key=name value=value
            //парсинг strings
            string sContent = "";
            if (File.Exists(path + "\\res\\values-ru\\strings.xml"))
            {
                sContent = File.ReadAllText(path + "\\res\\values-ru\\strings.xml", Encoding.UTF8);
                foreach (Match m in Regex.Matches(sContent, @"<string name=\""(.+)\"">(.+)</string>"))
                {
                    stringRuValues.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }

            if (File.Exists(path + "\\res\\values\\strings.xml"))
            {
                sContent = File.ReadAllText(path + "\\res\\values\\strings.xml", Encoding.UTF8);
                foreach (Match m in Regex.Matches(sContent, @"<string name=\""(.+)\"">(.+)</string>"))
                {
                    stringValues.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }

            //конец парсинга

            sContent = File.ReadAllText(path + "\\res\\values\\public.xml", Encoding.UTF8);

            foreach (Match m in Regex.Matches(sContent, @"<public type=\""(.+)\"" name=\""(.+)\"" id=\""(.+)\"" />"))
            {
                foundedInPublic.Add(m.Groups[3].Value, new string[] { m.Groups[1].Value, m.Groups[2].Value });
            }

            foreach (string foundedDir in Directory.GetDirectories(path, "smali*"))
            {
                foundedFiles.AddRange(Directory.GetFiles(foundedDir, "*.smali", SearchOption.AllDirectories));
            }

            foreach (string foundedfile in foundedFiles)
            {
                work.Add(new Task(() =>
                {
                    List<string> replacedSubs = new List<string>();
                    string fileContent = File.ReadAllText(foundedfile, Encoding.UTF8);
                    int orig = fileContent.GetHashCode();
                    //парсинг ИД
                    foreach (Match m in Regex.Matches(fileContent, @"const.+,\s(0x[0-9a-f]{7,})"))
                    {
                        if (replacedSubs.Contains(m.Value)) { continue; }

                        if (foundedInPublic.ContainsKey(m.Groups[1].Value))//содержит ли ИД
                        {
                            if (stringRuValues.ContainsKey(foundedInPublic[m.Groups[1].Value][1]))//name
                            {
                                fileContent = fileContent.Replace(m.Value, "##AllInOne##!ID!TYPE=" + foundedInPublic[m.Groups[1].Value][0] + " NAME=" + foundedInPublic[m.Groups[1].Value][1] + " VALUE=" + stringRuValues[foundedInPublic[m.Groups[1].Value][1]] + "\n    " + m.Value);
                            }
                            else if (stringValues.ContainsKey(foundedInPublic[m.Groups[1].Value][1]))//name
                            {
                                fileContent = fileContent.Replace(m.Value, "##AllInOne##!ID!TYPE=" + foundedInPublic[m.Groups[1].Value][0] + " NAME=" + foundedInPublic[m.Groups[1].Value][1] + " VALUE=" + stringValues[foundedInPublic[m.Groups[1].Value][1]] + "\n    " + m.Value);
                            }
                            else
                            {
                                fileContent = fileContent.Replace(m.Value, "##AllInOne##!ID!TYPE=" + foundedInPublic[m.Groups[1].Value][0] + " NAME=" + foundedInPublic[m.Groups[1].Value][1] + "\n    " + m.Value);
                            }
                        }
                        replacedSubs.Add(m.Value);
                    }
                    //---
                    foreach (string[] pattern in new string[][] {
                    new string[] { @"const-string.+\""((?:.*?\\u[0-9a-fA-F]+.+?)+)\""" , "##AllInOne##!UNICODE! " },
                    new string[] { @"\.array-data \d+[\r\n]+((?:\s+-?0x[a-fA-F0-9]+t)+)[\r\n\s]+\.end array-data", "##AllInOne##!ARRAYDATA! " },
                    new string[] { @"const-string.+\""([a-zA-Z0-9/\+]+={0,3})\""", "##AllInOne##!BASE64! " } })
                    {
                        replacedSubs.Clear();

                        foreach (Match m in Regex.Matches(fileContent, pattern[0]))
                        {
                            if (replacedSubs.Contains(m.Value)) { continue; }

                            string tmp = convertUnicodeToText(m.Groups[1].Value);
                            if (!"".Equals(tmp))
                            {
                                fileContent = fileContent.Replace(m.Value, pattern[1] + tmp + "\n    " + m.Value);
                            }
                            replacedSubs.Add(m.Value);
                        }
                    }

                    if (orig != fileContent.GetHashCode())
                    {
                        using (StreamWriter sw = new StreamWriter(foundedfile, false, new UTF8Encoding(false), 65536))
                        {
                            sw.WriteLine(fileContent);
                        }

                        mainf.appendProgressTbox("    " + Language.log_patched + TrimPathToInput(foundedfile));
                    }
                }));

            }

            work.Start();
            mainf.appendProgressTbox(":::::" + Language.log_add_smali_secondary_info_done + ":::::");
        }

        public static void RepairGoogleMaps(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.GoogleMapsPattern, true);
        }

        public static void refLoggingPatch(string path)
        {
            bool copy = Patcher.ReplaceInFiles(path, Patterns.refLogPatch, "*.smali", "smali*", true);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\ref_log.smali", path + "\\smali\\com\\anymy\\ref_log.smali", copy);
            Patcher.moveToClassesNorNot(path, path + "\\smali\\com\\anymy\\ref_log.smali");
        }

        public static void SignatureBinPatch(string path)
        {
            StringBuilder log = new StringBuilder();

            string smaliName = "PmsHookApplication.smali";
            copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\" + smaliName, path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", true);

            string binContent = File.ReadAllText(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", Encoding.UTF8);
            string ManifestContent = File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8);
            string applicationname = Regex.Match(ManifestContent, @"<application(.*)android:name=\""([^\""]+)\""").Groups[2].Value;
            if ("".Equals(applicationname))
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<application(.*)>", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<application(.*)>", "<application$1 android:name=\"cc.binmt.signature.PmsHookApplication\">");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<application(.*)>" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });
            }
            else if ("android.app.Application".Equals(applicationname))
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>", "<$1 $2 android:name=\"cc.binmt.signature.PmsHookApplication\">");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });
            }
            else
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<application(.*)android:name=\""([^\""]+)\""", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<application(.*)android:name=\""([^\""]+)\""", "<application$1android:name=\"cc.binmt.signature.PmsHookApplication\"");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<application(.*)android:name=\""([^\""]+)\""" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });

                string superSmali = File.ReadAllText(getPathToSmali(path, applicationname), Encoding.UTF8);
                superSmali = Regex.Replace(superSmali, @"\.class(.+?)L(.+);", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"\.class(.+?)L(.+);", ".class public L$2;");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(getPathToSmali(path, applicationname)) + "\nPattern=\n" + @"\.class(.+?)L(.+);" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });

                using (StreamWriter sw = new StreamWriter(getPathToSmali(path, applicationname), false, new UTF8Encoding(false), 65536))
                {
                    sw.WriteLine(superSmali);
                }

                binContent = binContent.Replace("Landroid/app/Application", "L" + applicationname.Replace(".", "/"));
                if (Settings.writeDebug)
                {
                    log.AppendLine("File= " + TrimPathToInput(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali") + "\nPattern=\n" + "" + "\nOldText=\n" + "Landroid/app/Application" + "\nNewText=\n" + "L" + applicationname.Replace(".", "/"));
                    log.AppendLine("====================================================================================================================================================");
                }
            }

            using (StreamWriter sw = new StreamWriter(path + "\\AndroidManifest.xml", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine(ManifestContent);
            }

            string signatureData = getSignatureData(path + ".apk");

            binContent = binContent.Replace("### Signatures Data ###", signatureData.Replace("\r", "").Replace("\n", ""));
            if (Settings.writeDebug)
            {
                log.AppendLine("File= " + TrimPathToInput(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali") + "\nPattern=\n" + "" + "\nOldText=\n" + "### Signatures Data ###" + "\nNewText=\n" + signatureData.Replace("\r", "").Replace("\n", ""));
                log.AppendLine("====================================================================================================================================================");
            }

            using (StreamWriter sw = new StreamWriter(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine(binContent);
            }
            moveToClassesNorNot(path, path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali");
            WriteDebugLog(log.ToString());
        }

        public static void SignanureBinInstallerPatch(string path)
        {
            StringBuilder log = new StringBuilder();

            string smaliName = "PmsHookApplicationInstaller.smali";
            copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\" + smaliName, path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", true);
            string binContent = File.ReadAllText(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", Encoding.UTF8);
            string ManifestContent = File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8);
            string applicationname = Regex.Match(ManifestContent, @"<application(.*)android:name=\""([^\""]+)\""").Groups[2].Value;
            if ("".Equals(applicationname))
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<application(.*)>", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<application(.*)>", "<application$1 android:name=\"cc.binmt.signature.PmsHookApplication\">");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<application(.*)>" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });
            }
            else if ("android.app.Application".Equals(applicationname))
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>", "<$1 $2 android:name=\"cc.binmt.signature.PmsHookApplication\">");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<(application.*)(?:android|n\d+):name=\""[^\""]+\""(.*)>" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });
            }
            else
            {
                ManifestContent = Regex.Replace(ManifestContent, @"<application(.*)android:name=\""([^\""]+)\""", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"<application(.*)android:name=\""([^\""]+)\""", "<application$1android:name=\"cc.binmt.signature.PmsHookApplication\"");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(path + "\\AndroidManifest.xml") + "\nPattern=\n" + @"<application(.*)android:name=\""([^\""]+)\""" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });

                string superSmali = File.ReadAllText(getPathToSmali(path, applicationname), Encoding.UTF8);
                superSmali = Regex.Replace(superSmali, @"\.class(.+?)L(.+);", (m) =>
                {
                    string newValue = Regex.Replace(m.Value, @"\.class(.+?)L(.+);", ".class public L$2;");
                    if (Settings.writeDebug)
                    {
                        log.AppendLine("File= " + TrimPathToInput(getPathToSmali(path, applicationname)) + "\nPattern=\n" + @"\.class(.+?)L(.+);" + "\nOldText=\n" + m.Value + "\nNewText=\n" + newValue);
                        log.AppendLine("====================================================================================================================================================");
                    }
                    return newValue;
                });

                using (StreamWriter sw = new StreamWriter(getPathToSmali(path, applicationname), false, new UTF8Encoding(false), 65536))
                {
                    sw.WriteLine(superSmali);
                }

                binContent = binContent.Replace("Landroid/app/Application", "L" + applicationname.Replace(".", "/"));
                if (Settings.writeDebug)
                {
                    log.AppendLine("File= " + TrimPathToInput(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali") + "\nPattern=\n" + "" + "\nOldText=\n" + "Landroid/app/Application" + "\nNewText=\n" + "L" + applicationname.Replace(".", "/"));
                    log.AppendLine("====================================================================================================================================================");
                }
            }

            using (StreamWriter sw = new StreamWriter(path + "\\AndroidManifest.xml", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine(ManifestContent);
            }


            string signatureData = getSignatureData(path + ".apk");

            string instllr = "";
            switch (mainf.binInstallerCBox.Text)
            {
                case "Google":
                    instllr = "com.android.vending";
                    break;
                case "Amazon":
                    instllr = "com.amazon";
                    break;
            }
            binContent = binContent.Replace("### Installer ###", instllr);
            if (Settings.writeDebug)
            {
                log.AppendLine("File= " + TrimPathToInput(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali") + "\nPattern=\n" + "" + "\nOldText=\n" + "### Installer ###" + "\nNewText=\n" + instllr);
                log.AppendLine("====================================================================================================================================================");
            }

            binContent = binContent.Replace("### Signatures Data ###", signatureData.Replace("\r", "").Replace("\n", ""));
            if (Settings.writeDebug)
            {
                log.AppendLine("File= " + TrimPathToInput(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali") + "\nPattern=\n" + "" + "\nOldText=\n" + "### Signatures Data ###" + "\nNewText=\n" + signatureData.Replace("\r", "").Replace("\n", ""));
                log.AppendLine("====================================================================================================================================================");
            }

            using (StreamWriter sw = new StreamWriter(path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine(binContent);
            }
            moveToClassesNorNot(path, path + "\\smali\\cc\\binmt\\signature\\PmsHookApplication.smali");
            WriteDebugLog(log.ToString());
        }

        public static void InstallerGooglePatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.InstallerGoogle, "*.smali", "smali*", true);
        }

        public static void InstallerAmazonPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.InstallerAmazon, "*.smali", "smali*", true);
        }

        public static void EmulatorPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.EmulatorPatch, "*.smali", "smali*", true);
        }

        public static void ReceiverPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.ReceiverPatch, true);
        }

        public static void GoogleServicesAddictionPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.GoogleServicesAddictionPatch, "*.smali", "smali*", true);
        }

        public static void LicenseGooglePatch(string path)
        {
            if (path == "")
            {
                return;
            }
            string value = Regex.Match(File.ReadAllText(path + "\\AndroidManifest.xml", Encoding.UTF8), "package=\\\"([^\\\"]+)\\\"").Groups[1].Value;
            string value2 = Regex.Match(File.ReadAllText(path + "\\apktool.yml", Encoding.UTF8), "versionCode: \\'(.+)\\'").Groups[1].Value;
            if (Patcher.ReplaceInFiles(path, Patterns.LicensePatchGoogle, "*.smali", "smali*", true))
            {
                string value3 = string.Concat(new string[]
                {
                    ".class public LfixLicense;\n.super Landroid/os/Binder;\n\n.method public static reworkRequestCode(Ljava/lang/String;)Ljava/lang/String;\n    .locals 21\n\n    const-string v13, \"0|136040138|",
                    value,
                    "|",
                    value2,
                    "|ANlOHQOShF3uJUwv3Ql+fbsgEG9FD35Hag==|1352549288191\"\n    \n    if-eqz p0, :cond_7\n\n    :try_start_0\n    const-string v18, \"1|\"\n\n    move-object/from16 v0, p0\n\n    move-object/from16 v1, v18\n\n    invoke-virtual {v0, v1}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z\n\n    move-result v18\n\n    if-eqz v18, :cond_7\n\n    const-string v18, \"\\\\|\"\n\n    move-object/from16 v0, p0\n\n    move-object/from16 v1, v18\n\n    invoke-virtual {v0, v1}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;\n\n    move-result-object v6\n    \n    const-string v11, \"\"\n    \n    const/4 v12, 0x0\n    \n    const/4 v10, 0x0\n    \n    :goto_0\n    array-length v0, v6\n\n    move/from16 v18, v0\n\n    move/from16 v0, v18\n\n    if-lt v10, v0, :cond_1\n\n    new-instance v18, Ljava/io/File;\n\n    const-string v19, \"/mnt/sdcard/debug.on\"\n\n    invoke-direct/range {v18 .. v19}, Ljava/io/File;-><init>(Ljava/lang/String;)V\n\n    invoke-virtual/range {v18 .. v18}, Ljava/io/File;->exists()Z\n\n    move-result v18\n\n    if-eqz v18, :cond_0\n\n    sget-object v18, Ljava/lang/System;->out:Ljava/io/PrintStream;\n\n    move-object/from16 v0, v18\n\n    invoke-virtual {v0, v11}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V\n\n    :cond_0\n    :goto_1\n    return-object v11\n\n    :cond_1\n    if-nez v10, :cond_2\n\n    aget-object v18, v6, v10\n\n    const-string v19, \"1\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z\n\n    move-result v18\n\n    if-eqz v18, :cond_6\n\n    const-string v11, \"0\"\n\n    const/4 v12, 0x1\n\n    :cond_2\n    :goto_2\n    const/16 v18, 0x5\n\n    move/from16 v0, v18\n\n    if-ne v10, v0, :cond_3\n\n    if-eqz v12, :cond_3\n\n    const-string v18, \"31536000000\"\n\n    invoke-static/range {v18 .. v18}, Ljava/lang/Long;->valueOf(Ljava/lang/String;)Ljava/lang/Long;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/Long;->longValue()J\n\n    move-result-wide v2\n    \n    const-string v18, \"31622400000\"\n\n    invoke-static/range {v18 .. v18}, Ljava/lang/Long;->valueOf(Ljava/lang/String;)Ljava/lang/Long;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/Long;->longValue()J\n\n    move-result-wide v4\n    \n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v14, v18, v2\n    \n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v16, v18, v4\n    \n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v18, v18, v2\n\n    add-long v8, v18, v2\n    \n    new-instance v18, Ljava/lang/StringBuilder;\n\n    invoke-static {v11}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-direct/range {v18 .. v19}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    const-string v19, \"|\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static {v14, v15}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \":GR=10&VT=\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static/range {v16 .. v17}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \"&\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \"GT=\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static {v8, v9}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n\n    move-result-object v11\n\n    :cond_3\n    const/16 v18, 0x5\n\n    move/from16 v0, v18\n\n    if-ne v10, v0, :cond_4\n\n    if-nez v12, :cond_4\n\n    new-instance v18, Ljava/lang/StringBuilder;\n\n    invoke-static {v11}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-direct/range {v18 .. v19}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    const-string v19, \"|\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    aget-object v19, v6, v10\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n\n    move-result-object v11\n\n    :cond_4\n    if-eqz v10, :cond_5\n\n    const/16 v18, 0x5\n\n    move/from16 v0, v18\n\n    if-eq v10, v0, :cond_5\n\n    new-instance v18, Ljava/lang/StringBuilder;\n\n    invoke-static {v11}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-direct/range {v18 .. v19}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    const-string v19, \"|\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    aget-object v19, v6, v10\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n\n    move-result-object v11\n\n    :cond_5\n    add-int/lit8 v10, v10, 0x1\n\n    goto/16 :goto_0\n\n    :cond_6\n    aget-object v11, v6, v10\n\n    goto/16 :goto_2\n\n    :cond_7\n    move-object/from16 p0, v13\n\n    const-string v18, \"\\\\|\"\n\n    move-object/from16 v0, p0\n\n    move-object/from16 v1, v18\n\n    invoke-virtual {v0, v1}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;\n\n    move-result-object v6\n\n    const-string v11, \"\"\n\n    const/4 v10, 0x0\n\n    :goto_3\n    array-length v0, v6\n\n    move/from16 v18, v0\n\n    move/from16 v0, v18\n\n    if-lt v10, v0, :cond_8\n\n\n    new-instance v18, Ljava/io/File;\n\n    const-string v19, \"/mnt/sdcard/debug.on\"\n\n    invoke-direct/range {v18 .. v19}, Ljava/io/File;-><init>(Ljava/lang/String;)V\n\n    invoke-virtual/range {v18 .. v18}, Ljava/io/File;->exists()Z\n\n    move-result v18\n\n    if-eqz v18, :cond_0\n\n    sget-object v18, Ljava/lang/System;->out:Ljava/io/PrintStream;\n\n    move-object/from16 v0, v18\n\n    invoke-virtual {v0, v11}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V\n    :try_end_0\n    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0\n\n    goto/16 :goto_1\n\n    :catch_0\n    move-exception v7\n    \n    sget-object v18, Ljava/lang/System;->out:Ljava/io/PrintStream;\n\n    new-instance v19, Ljava/lang/StringBuilder;\n\n    const-string v20, \"Exception: \"\n\n    invoke-direct/range {v19 .. v20}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    move-object/from16 v0, v19\n\n    move-object/from16 v1, p0\n\n    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v19\n\n    invoke-virtual/range {v19 .. v19}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V\n\n    move-object/from16 v11, p0\n\n    goto/16 :goto_1\n\n    :cond_8\n    if-nez v10, :cond_9\n\n    :try_start_1\n    aget-object v11, v6, v10\n\n    :cond_9\n    const/16 v18, 0x5\n\n    move/from16 v0, v18\n\n    if-ne v10, v0, :cond_a\n\n    const-string v18, \"31536000000\"\n\n    invoke-static/range {v18 .. v18}, Ljava/lang/Long;->valueOf(Ljava/lang/String;)Ljava/lang/Long;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/Long;->longValue()J\n\n    move-result-wide v2\n\n    const-string v18, \"31622400000\"\n\n    invoke-static/range {v18 .. v18}, Ljava/lang/Long;->valueOf(Ljava/lang/String;)Ljava/lang/Long;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/Long;->longValue()J\n\n    move-result-wide v4\n\n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v14, v18, v2\n\n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v16, v18, v4\n\n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v18\n\n    add-long v18, v18, v2\n\n    add-long v8, v18, v2\n\n    new-instance v18, Ljava/lang/StringBuilder;\n\n    invoke-static {v11}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-direct/range {v18 .. v19}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    const-string v19, \"|\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static {v14, v15}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \":GR=10&VT=\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static/range {v16 .. v17}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \"&\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    const-string v19, \"GT=\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-static {v8, v9}, Ljava/lang/String;->valueOf(J)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n\n    move-result-object v11\n\n    :cond_a\n    if-eqz v10, :cond_b\n\n    const/16 v18, 0x5\n\n    move/from16 v0, v18\n\n    if-eq v10, v0, :cond_b\n\n    new-instance v18, Ljava/lang/StringBuilder;\n\n    invoke-static {v11}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;\n\n    move-result-object v19\n\n    invoke-direct/range {v18 .. v19}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V\n\n    const-string v19, \"|\"\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    aget-object v19, v6, v10\n\n    invoke-virtual/range {v18 .. v19}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;\n\n    move-result-object v18\n\n    invoke-virtual/range {v18 .. v18}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;\n    :try_end_1\n    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0\n\n    move-result-object v11\n\n    :cond_b\n    add-int/lit8 v10, v10, 0x1\n\n    goto/16 :goto_3\n.end method"
                });
                using (StreamWriter streamWriter = new StreamWriter(path + "\\smali\\fixLicense.smali", false, new UTF8Encoding(false), 65536))
                {
                    streamWriter.WriteLine(value3);
                }
            }
        }

        public static void LicenseAmazonPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.LicensePatchAmazon, "*.smali", "smali*", true);
        }

        public static void DeleteToastsPatch(string path)
        {
            Patcher.ReplaceInFiles(path, Patterns.ToastPatch, "*.smali", "smali*", true);
        }

        public static void RootPatch(string path)
        {
            Patcher.ReplaceInFiles(path, Patterns.RootPatch, "*.smali", "smali*", true);
        }

        public static void FullscreenPatch(string path)
        {
            Patcher.ReplaceInFiles(path + "\\res", Patterns.FullscreenPatch, "styles.xml", "values*", true);
        }

        public static void noInternetPatch(string path)
        {
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.noInternetPatch, true);
        }

        public static void noLocationPatch(string path)
        {
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.noLocationPatch, true);
        }

        public static void hideIconPatch(string path)
        {
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", Patterns.hideIconPatch, true);
        }

        public static void darkLightDHMAPatch(string path)
        {
            Patcher.ReplaceInFiles(path + "\\res\\", Patterns.darkLightPatchDhma, "*.xml", "*", true);
        }

        public static void darkLightDPatch(string path)
        {
            Patcher.ReplaceInFiles(path + "\\res\\", Patterns.darkLightPatchD, "*.xml", "*", true);
        }

        public static void darkLightLHMAPatch(string path)
        {
            Patcher.ReplaceInFiles(path + "\\res\\", Patterns.darkLightPatchLhma, "*.xml", "*", true);
        }

        public static void darkLightLPatch(string path)
        {
            Patcher.ReplaceInFiles(path + "\\res\\", Patterns.darkLightPatchL, "*.xml", "*", true);
        }

        public static void AddSavePatch(string path)
        {
            string launcherActivity = Patcher.GetLauncherActivity(path);
            string pathToSmali = Patcher.getPathToSmali(path, launcherActivity);
            if (!File.Exists(pathToSmali))
            {
                MessageBox.Show(pathToSmali + Language.errorMsgNotExist, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("Error in AddSavePatch\nError message:" + pathToSmali + " not exists");
                return;
            }
            bool copy = Patcher.ReplaceInFile(pathToSmali, Patterns.AddSavePatch, true);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\save.smali", path + "\\smali\\save.smali", copy);
            Patcher.moveToClassesNorNot(path, path + "\\smali\\save.smali");
        }

        public static void mockLocationPatch(string path)
        {
            Patcher.ReplaceInFiles(path, Patterns.mockLocationPatch, "*.smali", "smali*", true);
        }

        public static void splashInstallPatch(string path)
        {
            string text = Patcher.GetLauncherActivity(path);
            string appPackage = Patcher.GetAppPackage(path);
            if (text.Equals(string.Empty))
            {
                MessageBox.Show(Language.errorMsgNoLaunchActivity, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (text.StartsWith("."))
            {
                text = appPackage + text;
            }
            string text2 = ".class public L%PACKAGE%SSSplashAAActivity;\n.super Landroid/app/Activity;\n.source \"SSSplashAAActivity.java\"\n\n.method public constructor <init>()V\n    .locals 1\n\n    invoke-direct {p0}, Landroid/app/Activity;-><init>()V\n\n    return-void\n.end method\n\n.method protected onCreate(Landroid/os/Bundle;)V\n    .locals 4\n\n    invoke-super {p0, p1}, Landroid/app/Activity;->onCreate(Landroid/os/Bundle;)V\n\n    new-instance v0, Landroid/content/Intent;\n\n    const-class v1, L%MAIN_ACTIVITY%;\n\n    invoke-direct {v0, p0, v1}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V\n\n    invoke-virtual {p0, v0}, L%PACKAGE%SSSplashAAActivity;->startActivity(Landroid/content/Intent;)V\n\n    invoke-virtual {p0}, L%PACKAGE%SSSplashAAActivity;->finish()V\n\n    return-void\n.end method";
            string value = "<?xml version='1.0' encoding='utf-8' ?>\n<layer-list xmlns:android=\"http://schemas.android.com/apk/res/android\">\n\t<item android:drawable=\"@color/sssplashgray\" />\n\t<item>\n\t\t<bitmap android:gravity=\"center\" android:src=\"@drawable/sssplasssh\" />\n\t</item>\n</layer-list>";
            text2 = text2.Replace("%MAIN_ACTIVITY%", text.Replace(".", "/")).Replace("%PACKAGE%", appPackage.Replace(".", "/") + "/");
            if (Settings.writeDebug)
            {
                Patcher.WriteDebugLog("File= " + Patcher.TrimPathToInput(path + "\\smali\\" + appPackage.Replace(".", "\\") + "\\SSSplashAAActivity.smali") + "\nPattern=\n" + "\nOldText=\n%MAIN_ACTIVITY%\nNewText=\n" + text.Replace(".", "/"));
                Patcher.WriteDebugLog("File= " + Patcher.TrimPathToInput(path + "\\smali\\" + appPackage.Replace(".", "\\") + "\\SSSplashAAActivity.smali") + "\nPattern=\n" + "\nOldText=\n%PACKAGE%\nNewText=\n" + appPackage.Replace(".", "/") + "/");
            }
            using (StreamWriter streamWriter = new StreamWriter(path + "\\smali\\" + appPackage.Replace(".", "\\") + "\\SSSplashAAActivity.smali", false, new UTF8Encoding(false), 65536))
            {
                streamWriter.WriteLine(text2);
            }
            using (StreamWriter streamWriter2 = new StreamWriter(path + "\\res\\drawable\\ssscreen_sssplasssh.xml", false, new UTF8Encoding(false), 65536))
            {
                streamWriter2.WriteLine(value);
            }
            if (!File.Exists(path + "\\res\\values\\colors.xml"))
            {
                using (StreamWriter streamWriter3 = new StreamWriter(path + "\\res\\values\\colors.xml", false, new UTF8Encoding(false), 65536))
                {
                    streamWriter3.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n</resources>");
                }
            }
            if (!File.Exists(path + "\\res\\values\\styles.xml"))
            {
                using (StreamWriter streamWriter4 = new StreamWriter(path + "\\res\\values\\styles.xml", false, new UTF8Encoding(false), 65536))
                {
                    streamWriter4.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n</resources>");
                }
            }
            bool flag = Patcher.ReplaceInFile(path + "\\res\\values\\colors.xml", new Dictionary<string, string>
            {
                {
                    "<resources>",
                    "<resources>\n<color name=\"sssplashgray\">#ffffffff</color>"
                }
            }, true);
            bool flag2 = Patcher.ReplaceInFile(path + "\\res\\values\\styles.xml", new Dictionary<string, string>
            {
                {
                    "<resources>",
                    "<resources>\n<style name=\"SSSplashTTTheme\" parent=\"@android:style/Theme.NoTitleBar\"><item name=\"android:windowBackground\">@drawable/ssscreen_sssplasssh</item></style>"
                }
            }, true);
            bool flag3 = Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", new Dictionary<string, string>
            {
                {
                    "[\\r\\n\\s]*<category\\s*android:name=\\\"android\\.intent\\.category\\.LAUNCHER\\\"[^>]*>",
                    ""
                },
                {
                    "(<application[^>]+>)",
                    "$1\n        <activity android:name=\".SSSplashAAActivity\" android:theme=\"@style/SSSplashTTTheme\">\n\t\t\t<intent-filter>\n\t\t\t\t<action android:name=\"android.intent.action.MAIN\" />\n\t\t\t\t<category android:name=\"android.intent.category.LAUNCHER\" />\n\t\t\t</intent-filter>\n\t\t</activity>"
                }
            }, true);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\sssplasssh.png", path + "\\res\\drawable\\sssplasssh.png", flag && flag2 && flag3);
        }

        public static void splashRemovePatch(string path)
        {
            string text = Patcher.GetLauncherActivity(path);
            string appPackage = Patcher.GetAppPackage(path);
            if (text.Equals(string.Empty))
            {
                MessageBox.Show(Language.errorMsgNoLaunchActivity, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (text.StartsWith("."))
            {
                text = appPackage + text;
            }
            if (!File.Exists(path + "\\smali\\" + appPackage.Replace(".", "\\") + "\\SSSplashAAActivity.smali"))
            {
                MessageBox.Show(Language.errorMsgNoSplash, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            try
            {
                File.Delete(path + "\\res\\drawable\\ssscreen_sssplasssh.xml");
                File.Delete(path + "\\smali\\" + appPackage.Replace(".", "\\") + "\\SSSplashAAActivity.smali");
                File.Delete(path + "\\res\\drawable\\sssplasssh.png");
                goto IL_FE;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language.errorMsg, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Patcher.WriteLog("Error in splashRemovePatch\nError message:" + ex.Message + "\n" + ex.ToString());
            }
            return;
        IL_FE:
            Patcher.ReplaceInFile(path + "\\res\\values\\colors.xml", new Dictionary<string, string>
            {
                {
                    "[\\r\\n\\s]*<color name=\\\"sssplashgray\\\">#ffffffff</color>",
                    ""
                }
            }, true);
            Patcher.ReplaceInFile(path + "\\res\\values\\styles.xml", new Dictionary<string, string>
            {
                {
                    "[\\r\\n\\s]*<style name=\\\"SSSplashTTTheme\\\" parent=\\\"@android:style/Theme\\.NoTitleBar\\\">[\\r\\n\\s]*<item name=\\\"android:windowBackground\\\">@drawable/ssscreen_sssplasssh</item>[\\r\\n\\s]*</style>",
                    ""
                }
            }, true);
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", new Dictionary<string, string>
            {
                {
                    "[\\r\\n]+\\s+<activity.*android:name=\\\"\\.SSSplashAAActivity[^>]+>(?<!/>)(\\r|\\n|.)+?</activity>",
                    ""
                },
                {
                    "(<action android:name=\\\"android.intent\\.action\\.MAIN\\\"\\s*/>)",
                    "$1\n                <category android:name=\"android.intent.category.LAUNCHER\" />"
                }
            }, true);
            Patcher.ReplaceInFile(path + "\\res\\values\\public.xml", new Dictionary<string, string>
            {
                {
                    "<public type=\\\"style\\\" name=\\\"SSSplashTTTheme\\\" id=\\\".+?\\\" />",
                    ""
                },
                {
                    "<public type=\\\"color\\\" name=\\\"sssplashgray\\\" id=\\\".+?\\\" />",
                    ""
                },
                {
                    "<public type=\\\"drawable\\\" name=\\\"ssscreen_sssplasssh\\\" id=\\\".+?\\\" />",
                    ""
                },
                {
                    "<public type=\\\"drawable\\\" name=\\\"sssplasssh\\\" id=\\\".+?\\\" />",
                    ""
                }
            }, true);
        }

        public static void toastFirstRunPatch(string path)
        {
            string launcherActivity = Patcher.GetLauncherActivity(path);
            if (launcherActivity != string.Empty)
            {
                string text = File.ReadAllLines(Patcher.getPathToSmali(path, launcherActivity))[0];
                text = text.Remove(0, text.IndexOf(" L") + 1);
                string text2 = Patcher.mainf.toastMessageTBox.Text;
                if (text2 == "")
                {
                    return;
                }
                text2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text2));
                string value = "# direct methods\n\n.method private OnAppLoadedMessage()V\n    .locals 6\n\n    const/4 v2, 0x0\n\n    const-string v1, \"UI\"\n\n    invoke-virtual {p0, v1, v2}, Landroid/content/Context;->getSharedPreferences(Ljava/lang/String;I)Landroid/content/SharedPreferences;\n\n    move-result-object v1\n\n    const-string v2, \"Lib\"\n\n    const/4 v3, 0x1\n\n    invoke-interface {v1, v2, v3}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z\n\n    move-result v0\n\n    if-eqz v0, :cond_39\n\n    const/4 v0, 0x0\n\n    const-string v2, \"UI\"\n\n    invoke-virtual {p0, v2, v0}, Landroid/content/Context;->getSharedPreferences(Ljava/lang/String;I)Landroid/content/SharedPreferences;\n\n    move-result-object v1\n\n    invoke-interface {v1}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;\n\n    move-result-object v1\n\n    const-string v2, \"Lib\"\n\n    invoke-interface {v1, v2, v0}, Landroid/content/SharedPreferences$Editor;->putBoolean(Ljava/lang/String;Z)Landroid/content/SharedPreferences$Editor;\n\n    move-result-object v1\n\n    invoke-interface {v1}, Landroid/content/SharedPreferences$Editor;->commit()Z\n\n    const-string v1, \"" + text2 + "\" ##toast_message\n\n    const/4 v0, 0x0\n\n    invoke-static {v1, v0}, Landroid/util/Base64;->decode(Ljava/lang/String;I)[B\n\n    move-result-object v0\n\n    new-instance v1, Ljava/lang/String;\n\n    invoke-direct {v1, v0}, Ljava/lang/String;-><init>([B)V\n\n    const/4 v0, 0x1\n\n    invoke-static {p0, v1, v0}, Landroid/widget/Toast;->makeText(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;\n\n    move-result-object v0\n\n    invoke-virtual {v0}, Landroid/widget/Toast;->show()V\n\n    :goto_38\n    return-void\n\n    :cond_39\n    return-void\n\n    goto :goto_38\n.end method";
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("\\.method (.+)onCreate\\(Landroid/os/Bundle;\\)V[\\r\\n\\s]+\\.locals (\\d+)", ".method $1onCreate(Landroid/os/Bundle;)V\n    .locals $2\n\n    invoke-direct {p0}, " + text + "->OnAppLoadedMessage()V");
                dictionary.Add("\\# direct methods", value);
                Patcher.ReplaceInFile(Patcher.getPathToSmali(path, launcherActivity), dictionary, true);
            }
        }

        public static void changeMinSdkPatch(string path)
        {
            File.ReadAllText(path + "\\apktool.yml", Encoding.UTF8);
            string text = Patcher.mainf.minSdkCBox.Text;
            if (text.IndexOf("-") != -1)
            {
                text = text.Remove(text.IndexOf("-")).Trim();
                Dictionary<string, string> patterns = new Dictionary<string, string>
                {
                    {
                        "minSdkVersion: '\\d+'",
                        "minSdkVersion: '" + text + "'"
                    }
                };
                Patcher.ReplaceInFile(path + "\\apktool.yml", patterns, true);
                return;
            }
            MessageBox.Show("Error in MinSDK");
        }

        public static void installLocationPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string text = Patcher.mainf.installLocationCBox.Text;
            if (text.IndexOf("(") != -1)
            {
                text = text.Remove(text.IndexOf("("), text.Length - text.IndexOf("(")).Trim();
            }
            dictionary.Add("<manifest(.+)(android|n\\d+):installLocation=\\\"[^\\\"]+\\\"", "<manifest$1$2:installLocation=\"" + text + "\"");
            dictionary.Add("<manifest(?!.*?installLocation)([^>]+)>", "<manifest$1 android:installLocation=\"" + text + "\">");
            Patcher.ReplaceInFile(path + "\\AndroidManifest.xml", dictionary, true);
        }

        public static void collectAllStrings(string path)
        {
            string prefix = "new_string_";
            string template = "{0}:{1}=\"@string/{2}\"";
            int prefixNum = getLastPrefixNum(path);
            if (prefixNum > 0) { prefixNum++; }

            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);
            ConcurrentDictionary<string, string> foundedStrings = new ConcurrentDictionary<string, string>();
            mainf.appendProgressTbox(":::::" + Language.log_collecting_string + TrimPathToInput(path) + " :::::");

            foreach (string dirmask in new string[] { "layout*", "menu*", "xml*" })
            {
                foreach (string dir in Directory.GetDirectories(path + "\\res\\", dirmask))
                {
                    foreach (string filepath in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories))
                    {
                        work.Add(new Task(() =>
                        {
                            string fileContent = File.ReadAllText(filepath, Encoding.UTF8);
                            int foudedStringCount = 0;

                            foreach (Match match in Regex.Matches(fileContent, @"(android|n\d+):(text|title|summary)=\""((?!@)[^\""]+)\"""))
                            {
                                foudedStringCount++;

                                while (true)
                                {
                                    string key = prefix + prefixNum.ToString();
                                    if (foundedStrings.TryAdd(key, match.Groups[3].Value))
                                    {
                                        fileContent = fileContent.Replace(match.Value, string.Format(template, match.Groups[1].Value, match.Groups[2].Value, key));
                                        break;
                                    }
                                    else
                                    {
                                        Interlocked.Increment(ref prefixNum);
                                    }
                                }
                            }
                            if (foudedStringCount > 0)
                            {
                                mainf.appendProgressTbox(string.Format("    {0}:{1} strings", TrimPathToInput(filepath), foudedStringCount));
                                using (StreamWriter sw = new StreamWriter(filepath, false, new UTF8Encoding(false), 65536))
                                {
                                    sw.WriteLine(fileContent);
                                }
                            }
                        }));
                    }
                }
            }
            work.Start();
            List<KeyValuePair<string, string>> sortedFoundedStrings = foundedStrings.ToList();
            sortedFoundedStrings.Sort((x, y) =>
            {
                int a = int.Parse(x.Key.Substring(x.Key.LastIndexOf("_") + 1, x.Key.Length - x.Key.LastIndexOf("_") - 1));
                int b = int.Parse(y.Key.Substring(y.Key.LastIndexOf("_") + 1, y.Key.Length - y.Key.LastIndexOf("_") - 1));
                if (a > b) { return 1; }
                else if (a < b) { return -1; }
                else if (a == b) { return 0; }
                else return x.Key.CompareTo(y.Key);
            });
            if (foundedStrings.Count > 0)
            {
                string stringsContent = File.ReadAllText(path + "\\res\\values\\strings.xml", Encoding.UTF8);
                foreach (var pair in sortedFoundedStrings)
                {
                    string quote = "";
                    if (pair.Value.StartsWith(" ") || pair.Value.EndsWith(" "))
                    {
                        quote = "\"";
                    }
                    stringsContent = stringsContent.Replace("</resources>", string.Format("    <string name=\"{0}\">{1}{2}{3}</string>\n</resources>", pair.Key, quote, pair.Value, quote));
                }
                using (StreamWriter sw = new StreamWriter(path + "\\res\\values\\strings.xml", false, new UTF8Encoding(false), 65536))
                {
                    sw.WriteLine(stringsContent);
                }
            }

            mainf.appendProgressTbox(":::::" + Language.log_collecting_string_done + ":::::");
        }

        public static int getLastPrefixNum(string path)
        {
            List<int> list = new List<int>();
            foreach (object obj in Regex.Matches(File.ReadAllText(path + "\\res\\values\\strings.xml", Encoding.UTF8), "new_string_(\\d+)"))
            {
                Match match = (Match)obj;
                list.Add(int.Parse(match.Groups[1].Value));
            }
            list.Sort();
            if (list.Count != 0)
            {
                return list[list.Count - 1];
            }
            return 0;
        }

        public static Dictionary<string, Dictionary<string, string>> getAllIds(string path)
        {//словарь(путь к файлу, словарь(id, тип))
            if ("".Equals(path)) { return new Dictionary<string, Dictionary<string, string>>(); }
            TaskPool work = new TaskPool(Settings.TaskCount, lbl: ref mainf.taskCountLabel, pBar: ref mainf.pBar);

            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();

            foreach (string foundedDir in Directory.GetDirectories(path + "\\res\\", "layout*"))
            {
                foreach (string foundedFile in Directory.GetFiles(foundedDir, "*.xml", SearchOption.AllDirectories))
                {
                    work.Add(new Task(() =>
                    {
                        Dictionary<string, string> tmpIdType = new Dictionary<string, string>();

                        string fileContent = File.ReadAllText(foundedFile, Encoding.UTF8);

                        MatchCollection mCollection = Regex.Matches(fileContent, @"<([^\s]+?)\s.*?(?:android|n\d+):id=\""@id/(.+?)\"".*?/>");
                        if (mCollection.Count == 0) { return; }

                        foreach (Match m in mCollection)
                        {
                            if (!tmpIdType.ContainsKey(m.Groups[2].Value))
                            {
                                tmpIdType.Add(m.Groups[2].Value, m.Groups[1].Value);
                            }
                        }
                        lock (locker) { result.Add(foundedFile, tmpIdType); }
                    }));
                }
            }

            work.Start();

            return result;
        }

        public static Dictionary<string, Dictionary<int, string>> findInterestingPlaces(string path)
        {//словарь(путь к файлу, словарь(номер строки, место))
            if ("".Equals(path)) { return new Dictionary<string, Dictionary<int, string>>(); }
            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);

            Dictionary<string, string> interestPlacesPatterns = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(Program.pathToMyPluginDir + "\\places\\interestPlaces.txt"))
            {
                interestPlacesPatterns.Add(line, "");
            }

            if (Directory.Exists(path + "\\assets\\") && Settings.searchAssetsFiles)
            {
                List<string> assetsPat = new List<string>();

                foreach (string assets in Directory.GetDirectories(path + "\\assets\\", "*", SearchOption.AllDirectories))
                {
                    if (!assetsPat.Contains(Path.GetFileName(assets))) { assetsPat.Add(Path.GetFileName(assets)); }
                }
                foreach (string assets in Directory.GetFiles(path + "\\assets\\", "*", SearchOption.AllDirectories))
                {
                    if (!assetsPat.Contains(Path.GetFileNameWithoutExtension(assets))) { assetsPat.Add(Path.GetFileNameWithoutExtension(assets)); }
                }
                interestPlacesPatterns.Add(@"const-string [pv]\d+, \"".*(" + String.Join("|", assetsPat) + @").*", "");
            }
            if (Directory.Exists(path + "\\lib\\") && Settings.searchLibFiles)
            {
                List<string> assetsPat = new List<string>();

                foreach (string assets in Directory.GetFiles(path + "\\lib\\", "*", SearchOption.AllDirectories))
                {
                    if (!assetsPat.Contains(Path.GetFileNameWithoutExtension(assets))) { assetsPat.Add(Path.GetFileNameWithoutExtension(assets).Remove(0, 3)); }
                }
                interestPlacesPatterns.Add(@"const-string [pv]\d+, \"".*(" + String.Join("|", assetsPat) + @").*", "");
            }


            Dictionary<string, Dictionary<int, string>> result = new Dictionary<string, Dictionary<int, string>>();

            foreach (string foundedDir in Directory.GetDirectories(path, "smali*"))
            {
                foreach (string foundedFile in Directory.GetFiles(foundedDir, "*.smali", SearchOption.AllDirectories))
                {
                    work.Add(new Task(() =>
                    {
                        Dictionary<int, string> places = new Dictionary<int, string>();

                        string fileContent = File.ReadAllText(foundedFile, Encoding.UTF8);

                        Parallel.ForEach(interestPlacesPatterns, (pair) =>
                        {
                            MatchCollection mCollection = Regex.Matches(fileContent, pair.Key, RegexOptions.IgnoreCase);
                            if (mCollection.Count == 0) { return; }
                            foreach (Match m in mCollection)
                            {
                                int pos = getLineNumberInFileContent(fileContent, m.Value);
                                lock (locker)
                                {
                                    if (!places.ContainsKey(pos))
                                    {
                                        places.Add(pos, m.Value);
                                    }
                                }
                            }
                        });
                        lock (locker) { result.Add(foundedFile, places); }
                    }));
                }
            }

            work.Start();

            return result;
        }

        public static void hideAllChecked(Dictionary<string, Dictionary<string, string>> checkedItems)
        {
            List<string> list = new List<string>();
            TaskPool taskPool = new TaskPool(Settings.TaskCount, ref Patcher.mainf.taskCountLabel, ref Patcher.mainf.pBar);
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_hide_id + ":::::");
            foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in checkedItems)
            {
                foreach (KeyValuePair<string, string> keyValuePair2 in keyValuePair.Value)
                {
                    list.Add(keyValuePair2.Key);
                }
            }
            Dictionary<string, string> patterns = new Dictionary<string, string>
            {
                {
                    "(android|n\\d+):visibility=\\\"(?:visible|invisible)\\\"(.*)((?:android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")",
                    "$1:visibility=\"gone\"$2$3"
                },
                {
                    "((android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")(.*)(android|n\\d+):visibility=\\\"(?:visible|invisible)\\\"",
                    "$1$3$4:visibility=\"gone\""
                },
                {
                    "(?<!visibility.*)((android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")(?!.*visibility)",
                    "$1 $2:visibility=\"gone\""
                },
                {
                    "(android|n\\d+):layout_(height)=\\\"[^\\\"]+\\\"(.*)((?:android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")",
                    "$1:layout_$2=\"0.0dip\"$3$4"
                },
                {
                    "(android|n\\d+):layout_(width)=\\\"[^\\\"]+\\\"(.*)((?:android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")",
                    "$1:layout_$2=\"0.0dip\"$3$4"
                },
                {
                    "((?:android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")(.*)(android|n\\d+):layout_(height)=\\\"[^\\\"]+\\\"",
                    "$1$2$3:layout_$4=\"0.0dip\""
                },
                {
                    "((?:android|n\\d+):id=\\\"@id/(?:" + string.Join("|", list) + ")\\\")(.*)(android|n\\d+):layout_(width)=\\\"[^\\\"]+\\\"",
                    "$1$2$3:layout_$4=\"0.0dip\""
                }
            };
            using (Dictionary<string, Dictionary<string, string>>.Enumerator enumerator = checkedItems.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, Dictionary<string, string>> filepathPair = enumerator.Current;
                    taskPool.Add(new Task(delegate ()
                    {
                        Patcher.ReplaceInFile(filepathPair.Key, patterns, true);
                    }));
                }
            }
            taskPool.Start();
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_hide_id_done + ":::::");
        }

        public static void blockSensorsPatch(string path)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Sensors sensors = Patcher.deSerializeSensorsFromXml(Program.pathToMyPluginDir + "\\sensors\\" + Patcher.mainf.blockSensorsCBox.Text);
            if (sensors.ACCELEROMETER)
            {
                dictionary.Add("const(.+), 0x1[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.ACCELEROMETER_UNCALIBRATED)
            {
                dictionary.Add("const(.+), 0x23[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.ALL)
            {
                dictionary.Add("const(.+), 0xffffffff[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.AMBIENT_TEMPERATURE)
            {
                dictionary.Add("const(.+), 0xd[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.DEVICE_PRIVATE_BASE)
            {
                dictionary.Add("const(.+), 0x10000[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.GAME_ROTATION_VECTOR)
            {
                dictionary.Add("const(.+), 0xf[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.GEOMAGNETIC_ROTATION_VECTOR)
            {
                dictionary.Add("const(.+), 0x14[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.GRAVITY)
            {
                dictionary.Add("const(.+), 0x9[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.GYROSCOPE)
            {
                dictionary.Add("const(.+), 0x4[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.GYROSCOPE_UNCALIBRATED)
            {
                dictionary.Add("const(.+), 0x10[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.HEART_BEAT)
            {
                dictionary.Add("const(.+), 0x1f[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.HEART_RATE)
            {
                dictionary.Add("const(.+), 0x15[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.LIGHT)
            {
                dictionary.Add("const(.+), 0x5[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.LINEAR_ACCELERATION)
            {
                dictionary.Add("const(.+), 0xa[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.LOW_LATENCY_OFFBODY_DETECT)
            {
                dictionary.Add("const(.+), 0x22[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.MAGNETIC_FIELD)
            {
                dictionary.Add("const(.+), 0x2[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.MAGNETIC_FIELD_UNCALIBRATED)
            {
                dictionary.Add("const(.+), 0xe[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.MOTION_DETECT)
            {
                dictionary.Add("const(.+), 0x1e[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.ORIENTATION)
            {
                dictionary.Add("const(.+), 0x3[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.POSE_6DOF)
            {
                dictionary.Add("const(.+), 0x1c[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.PRESSURE)
            {
                dictionary.Add("const(.+), 0x6[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.PROXIMITY)
            {
                dictionary.Add("const(.+), 0x8[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.RELATIVE_HUMIDITY)
            {
                dictionary.Add("const(.+), 0xc[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.ROTATION_VECTOR)
            {
                dictionary.Add("const(.+), 0xb[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.SIGNIFICANT_MOTION)
            {
                dictionary.Add("const(.+), 0x11[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.STATIONARY_DETECT)
            {
                dictionary.Add("const(.+), 0x1d[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.STEP_COUNTER)
            {
                dictionary.Add("const(.+), 0x13[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.STEP_DETECTOR)
            {
                dictionary.Add("const(.+), 0x12[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            if (sensors.TEMPERATURE)
            {
                dictionary.Add("const(.+), 0x7[\\r\\n\\s]+(invoke-virtual.+hardware/SensorManager;->getDefaultSensor\\(I\\)Landroid/hardware/Sensor;)", "const$1, 0x0\n\n    $2");
            }
            Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
        }

        public static void deleteResourcesPatch(string path)
        {
            StringBuilder fsb = new StringBuilder();
            StringBuilder dsb = new StringBuilder();
            long totalsize = 0;

            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);
            mainf.appendProgressTbox(":::::" + Language.log_delete_resource + ":::::");
            string[] xmlFiles = new string[]
            {
                "arrays.xml",
                "attrs.xml",
                "bools.xml",
                "colors.xml",
                "dimens.xml",
                "drawables.xml",
                "fractions.xml",
                "ids.xml",
                "integers.xml",
                "plurals.xml",
                "strings.xml",
                "styles.xml"
            };
            string[] resDirMasks = new string[]
            {
                "drawable-*",
                "mipmap-*",
                "animator-*",
                "anim-*",
                "color-*",
                "font-*",
                "layout-*",
                "menu-*",
                "raw-*",
                "values-*",
                "xml-*"
            };
            if ("".Equals(mainf.deleteLangsCBox.Text))
            {
                MessageBox.Show(Language.delResError, Language.deleteRes, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //считывание исключений
            ExcludeRes eRes = deSerializeExcludeResFromXml(Program.pathToMyPluginDir + "\\deleteRes\\" + mainf.deleteLangsCBox.Text);
            //
            foreach (string dirMask in resDirMasks)
            {
                List<string> dirs = new List<string>(Directory.GetDirectories(path + "\\res", dirMask));
                filterExclude(ref dirs, eRes);
                if (dirs.Count == 0) { continue; }
                if ("values-*".Equals(dirMask))
                {
                    foreach (string name in xmlFiles)
                    {
                        work.Add(new Task(() =>
                        {
                            List<string> files = new List<string>();
                            foreach (string dir in dirs)
                            {
                                if (!File.Exists(dir + "\\" + name)) { continue; }

                                files.Add(dir + "\\" + name);
                            }
                            mergeXml(files.ToArray(), path + "\\res\\values\\" + name, false, true);
                            foreach (string file in files)
                            {
                                Interlocked.Add(ref totalsize, new FileInfo(file).Length);
                                File.Delete(file);
                                fsb.AppendLine(TrimPathToInput(file));
                                if (!Directory.EnumerateFileSystemEntries(Path.GetDirectoryName(file)).Any())
                                {
                                    Directory.Delete(Path.GetDirectoryName(file));
                                    dsb.AppendLine(TrimPathToInput(Path.GetDirectoryName(file)));
                                }
                            }
                        }));
                    }
                }
                else if ("drawable-*".Equals(dirMask) || "mipmap-*".Equals(dirMask))
                {
                    string dirname = dirMask.Replace("*", "");
                    string basicDirname = dirMask.Replace("-*", "");

                    List<string> basisFiles = new List<string>();
                    if (Directory.Exists(path + "\\res\\" + dirname + eRes.basis))
                    {
                        basisFiles = new List<string>(Directory.GetFiles(path + "\\res\\" + dirname + eRes.basis));
                    }
                    List<string> basicDirFiles = new List<string>();
                    if (Directory.Exists(path + "\\res\\" + basicDirname))
                    {
                        basicDirFiles = new List<string>(Directory.GetFiles(path + "\\res\\" + basicDirname));
                    }

                    foreach (string dir in dirs)
                    {
                        if (("drawable-" + eRes.basis).Equals(Path.GetFileName(dir)) || ("mipmap-" + eRes.basis).Equals(Path.GetFileName(dir))) { continue; }

                        foreach (string file in Directory.GetFiles(dir))
                        {
                            if (basisFiles.Contains(path + "\\res\\" + dirname + eRes.basis + "\\" + Path.GetFileName(file)) || basicDirFiles.Contains(path + "\\res\\" + basicDirname + "\\" + Path.GetFileName(file)))
                            {
                                Interlocked.Add(ref totalsize, new FileInfo(file).Length);
                                File.Delete(file);
                                fsb.AppendLine(TrimPathToInput(file));
                            }
                        }
                    }
                }
                else
                {
                    string dirname = dirMask.Replace("-*", "");

                    List<string> basisFiles = new List<string>();
                    if (Directory.Exists(path + "\\res\\" + dirname))
                    {
                        basisFiles = new List<string>(Directory.GetFiles(path + "\\res\\" + dirname));
                    }

                    foreach (string dir in dirs)
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            if (basisFiles.Contains(path + "\\res\\" + dirname + "\\" + Path.GetFileName(file)))
                            {
                                Interlocked.Add(ref totalsize, new FileInfo(file).Length);
                                File.Delete(file);
                                fsb.AppendLine(TrimPathToInput(file));
                            }
                        }
                    }
                }

                foreach (string dir in dirs)
                {
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                        dsb.AppendLine(TrimPathToInput(dir));
                    }
                }
            }
            //libs
            if (Directory.Exists(path + "\\lib"))
            {
                int libCount = 0;
                List<string> dirs = new List<string>(Directory.GetDirectories(path + "\\lib"));
                filterLibs(ref dirs, eRes);

                foreach (string dir in dirs)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        Interlocked.Add(ref totalsize, new FileInfo(file).Length);
                        File.Delete(file);
                        fsb.AppendLine(TrimPathToInput(file));
                        libCount++;
                    }

                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                        dsb.AppendLine(TrimPathToInput(dir));
                    }
                }
                mainf.appendProgressTbox("     -"+ Language.log_delete_resource_lib + libCount.ToString());
            }

            //
            if (work.count > 0)
            {
                work.Start();
            }
            //work.Start();
            using (StreamWriter sw = new StreamWriter(path + "\\DeleteResources.log", false, new UTF8Encoding(false), 65536))
            {
                sw.WriteLine("Apk: " + TrimPathToInput(path) + "\nDeleted directories:\n" + dsb.ToString() + "\nDeleted files:\n" + fsb.ToString() + "=====Total size of deleted files: ~" + Math.Round(totalsize / 1024.0 / 1024.0, 3).ToString() + "Mb =====");
            }
            mainf.appendProgressTbox(":::::" + Language.log_delete_resource_done + ":::::");
        }

        public static void mergeXml(string[] fromFilePath, string toFilePath, bool replace, bool appendComment = false)
        {
            if (fromFilePath.Length == 0) { return; }

            XmlDocument docTo = new XmlDocument();
            try
            {
                docTo.Load(toFilePath);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Language.corruptedFileError, toFilePath), Language.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLog("Error in mergeXml\nError message:" + e.Message + "\n" + e.ToString());
                return;
            }
            ConcurrentDictionary<string, KeyValuePair<XmlNode, string>> fromNodeList = new ConcurrentDictionary<string, KeyValuePair<XmlNode, string>>();

            int replacedCount = 0;
            int addedCount = 0;
            XmlDocument docFrom = new XmlDocument();
            foreach (string fromFile in fromFilePath)
            {
                try
                {
                    docFrom.Load(fromFile);
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format(Language.corruptedFileError, toFilePath), Language.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLog("Error in mergeXml\nError message:" + e.Message + "\n" + e.ToString());
                    continue;
                }
                Parallel.ForEach(docFrom.ChildNodes[1].ChildNodes.Cast<XmlNode>(), (node) =>
                {
                    if (node.NodeType == XmlNodeType.Comment) { return; }
                    fromNodeList.TryAdd(node.Attributes["name"].Value, new KeyValuePair<XmlNode, string>(node, Path.GetFileName(Path.GetDirectoryName(fromFile))));
                });
            }

            foreach (var fromNodePair in fromNodeList)
            {
                bool exist = false;
                foreach (XmlNode stringToNode in docTo.ChildNodes[1].ChildNodes)
                {
                    if (stringToNode.NodeType == XmlNodeType.Comment) { continue; }
                    if (stringToNode.Attributes["name"].Value.Equals(fromNodePair.Key))
                    {
                        if (replace)
                        {
                            stringToNode.InnerText = fromNodePair.Value.Key.InnerText;
                            replacedCount++;
                        }
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    if (appendComment)
                    {
                        XmlComment comment = docTo.CreateComment(fromNodePair.Value.Value);
                        docTo.ChildNodes[1].AppendChild(comment);
                    }

                    XmlNode tmp = docTo.ImportNode(fromNodePair.Value.Key, true);
                    docTo.ChildNodes[1].AppendChild(tmp);
                    addedCount++;
                }
            }
            if (replacedCount != 0 || addedCount != 0)
            {
                docTo.Save(toFilePath);
            }
            lock (locker)
            {
                mainf.appendProgressTbox("     -" + Language.log_from + Path.GetFileName(toFilePath) + "\n   -"+ Language.log_to + TrimPathToInput(toFilePath));
                mainf.appendProgressTbox("     " + Language.replaced + " " + replacedCount.ToString());
                mainf.appendProgressTbox("     " + Language.added + " " + addedCount.ToString());
            }
        }

        public static void cloneApkPatch(string path)
        {//abc.software.abcvolume
            string[] abc = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            Random rnd = new Random();
            string package = GetAppPackage(path);

            string newpackage = package;

            Dictionary<string, string> patterns = new Dictionary<string, string>();

            if (!"".Equals(mainf.cloneTBox.Text))//ручной ввод имени пакета
            {
                if (mainf.cloneTBox.Text.Length != package.Length)
                {
                    MessageBox.Show(Language.cloneError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                newpackage = mainf.cloneTBox.Text;
            }
            else//авто
            {
                string lastpart = newpackage.Substring(newpackage.LastIndexOf(".") + 1, newpackage.Length - newpackage.LastIndexOf(".") - 1);
                while (package.Equals(newpackage))
                {
                    newpackage = newpackage.Replace(lastpart, lastpart.Replace(abc[rnd.Next(0, abc.Length - 1)], abc[rnd.Next(0, abc.Length - 1)]));
                }
            }

            if (package.Split('.').Length != newpackage.Split('.').Length)
            {
                MessageBox.Show(Language.cloneError, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            mainf.appendProgressTbox(":::::"+ Language.log_clone + ":::::");

            string[] packageSp = package.Split('.');
            string[] newpackageSp = newpackage.Split('.');

            patterns.Add("L" + package.Replace(".", "/"), "L" + newpackage.Replace(".", "/"));
            patterns.Add(package.Replace(".", "\\."), newpackage);
            foreach (string dir in Directory.GetDirectories(path, "smali*"))
            {
                string newtmp = "\\";
                string oldtmp = "\\";
                for (int i = 0; i < packageSp.Length; i++)
                {
                    newtmp += newpackageSp[i] + "\\";
                    oldtmp += packageSp[i] + "\\";
                    if (newpackageSp[i].Equals(packageSp[i]))
                    {
                        continue;
                    }
                    if (Directory.Exists(dir + oldtmp))
                    {
                        Directory.Move(dir + oldtmp, dir + newtmp);
                        oldtmp = oldtmp.Replace("\\" + packageSp[i] + "\\", "\\" + newpackageSp[i] + "\\");
                    }
                }
            }
            //замена в либах
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add(stringToHex(package), stringToHex(newpackage));
            values.Add(stringToHex(package.Replace(".", "/")), stringToHex(newpackage.Replace(".", "/")));
            values.Add(stringToHex(package.Replace(".", "_")), stringToHex(newpackage.Replace(".", "_")));

            if (package.Split('.').Length > 2)
            {
                while (package.Split('.').Length != 2)
                {
                    package = package.Substring(0, package.LastIndexOf("."));
                    newpackage = newpackage.Substring(0, newpackage.LastIndexOf("."));
                    if (package.Equals(newpackage)) { continue; }
                    values.Add(stringToHex(package), stringToHex(newpackage));
                    values.Add(stringToHex(package.Replace(".", "/")), stringToHex(newpackage.Replace(".", "/")));
                    values.Add(stringToHex(package.Replace(".", "_")), stringToHex(newpackage.Replace(".", "_")));
                    patterns.Add(package.Replace(".", "\\."), newpackage);
                    patterns.Add(package.Replace(".", "/"), newpackage.Replace(".", "/"));
                }
            }

            ReplaceInFile(path + "\\AndroidManifest.xml", patterns);
            ReplaceInFiles(path, patterns, "*.smali", "smali*", false);
            ReplaceInFiles(path, patterns, "*.xml", "res", false);
            ReplaceInSoLibs(path, values);

            mainf.appendProgressTbox(":::::" + Language.log_clone_done + ":::::");
        }

        public static List<int> indexOfBlock(byte[] where, byte[] what)
        {
            List<int> result = new List<int>();//в этом списке будут храниться найденные позиции массива
            for (int i = 0; i < where.Length - what.Length + 1; ++i)
            {
                bool found = true;
                for (int j = 0; j < what.Length; ++j)
                {
                    if (where[i + j] != what[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) result.Add(i);
            }
            return result;//возвращаем результат
        }

        public static void StartAntiReklalytics(string path)
        {
            int copy = 0;
            List<string> filesToProcess = new List<string>();
            foreach (string mask in new string[] { "*.smali", "*.xml" })
            {
                string[] tmpdirs = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
                filesToProcess.AddRange(tmpdirs.Where(line =>
                !line.Contains(path + "\\_backup\\")
                && !line.Contains(path + "\\build\\")
                && !line.Contains(path + "\\original\\")
                && !line.Contains(path + "\\res\\anim")
                && !line.Contains(path + "\\res\\color")
                && !line.Contains(path + "\\res\\drawable")
                && !line.Contains(path + "\\res\\menu")
                && !line.Contains(path + "\\res\\xml")));
            }

            TaskPool work = new TaskPool(Settings.TaskCount, ref mainf.taskCountLabel, ref mainf.pBar);

            foreach (string founded in filesToProcess)
            {
                work.Add(new Task(() =>
                {
                    //===тут отсеивается то, что в любом случае не пропатчится (на основе чекбоксов)
                    if (mainf.analyticActivityCB.Checked || mainf.analyticFirebaseCB.Checked || mainf.analyticLayoutCB.Checked || mainf.analyticReceiverCB.Checked || mainf.analyticServiceCB.Checked)
                    {
                        if (!mainf.analyticLinksCB.Checked && !mainf.analyticMethodCB.Checked)
                        {
                            if (founded.EndsWith(".smali"))
                            {
                                return;
                            }
                        }
                    }
                    else if (mainf.analyticLinksCB.Checked || mainf.analyticMethodCB.Checked)
                    {
                        if (!mainf.analyticActivityCB.Checked && !mainf.analyticFirebaseCB.Checked && !mainf.analyticLayoutCB.Checked && !mainf.analyticReceiverCB.Checked && !mainf.analyticServiceCB.Checked)
                        {
                            if (founded.EndsWith(".xml"))
                            {
                                return;
                            }
                        }
                    }
                    //===
                    string fileContent = File.ReadAllText(founded, Encoding.UTF8);
                    int orig = fileContent.GetHashCode();
                    try
                    {
                        if (founded.EndsWith(".smali"))
                        {
                            if (mainf.analyticMethodCB.Checked)
                            {
                                ReplaceInFileEX(founded, ref fileContent, Patterns.methodsPatterns, false);
                                if (IsAdsModule(founded))
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.adsModulesOnly, true);
                                }

                                if (founded.Contains("\\ads\\AdView.smali"))
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.adviewPatterns, false);
                                }
                                else if (founded.Contains("\\ads\\NativeExpressAdView.smali"))
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.NativeExpressAdViewPatterns, false);
                                }
                            }

                            if (mainf.analyticLinksCB.Checked)
                            {
                                ReplaceInFileEX(founded, ref fileContent, Patterns.linksPattern, true);
                                ReplaceInFileEX(founded, ref fileContent, Patterns.linksExactMatchPattern, true);
                            }
                        }
                        else if (founded.EndsWith(".xml"))
                        {
                            ReplaceInFileEX(founded, ref fileContent, Patterns.XmlPatterns, false);

                            if (founded.EndsWith("\\AndroidManifest.xml"))
                            {
                                ReplaceInFileEX(founded, ref fileContent, Patterns.ManifestPatterns, true);

                                if (mainf.analyticActivityCB.Checked)
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.activityPatterns, true);
                                }

                                if (mainf.analyticServiceCB.Checked)
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.servicePatterns, true);
                                }
                                if (mainf.analyticReceiverCB.Checked)
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.receiverPatterns, true);
                                }

                                if (mainf.analyticFirebaseCB.Checked)
                                {
                                    ReplaceInFileEX(founded, ref fileContent, Patterns.firebasePatterns, true);
                                }
                            }

                            if (founded.Contains("\\res\\layout") && mainf.analyticLayoutCB.Checked)
                            {
                                ReplaceInFileEX(founded, ref fileContent, Patterns.LayoutPatterns, true);
                            }
                        }

                        if (orig != fileContent.GetHashCode())
                        {
                            using (StreamWriter sw = new StreamWriter(founded, false, new UTF8Encoding(false), 65536))
                            {
                                sw.WriteLine(fileContent);
                            }
                            if (founded.EndsWith(".smali")) { Interlocked.Increment(ref copy); }
                            mainf.appendProgressTbox("    " + Language.log_patched  + TrimPathToInput(founded));
                        }

                    }
                    catch (Exception e)
                    {
                        WriteLog("Error in StartAntiReklalytics\nError message:" + e.Message + "\n" + e.ToString());
                    }
                }));
            }

            work.Start();

            copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\PinkiePie.smali", path + "\\smali\\com\\PinkiePie.smali", copy > 0);
            moveToClassesNorNot(path, path + "\\smali\\com\\PinkiePie.smali");
        }

        public static bool IsAdsModule(string path)
        {
            bool result = false;
            foreach (string text in Patterns.AdvModules)
            {
                if (path.Contains(text.Replace("/", "\\")))
                {
                    result = true;
                    return result;
                }
            }
            return result;
        }

        public static void screenshotPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.ReplaceInFiles(path, Patterns.screenshotPatch, "*.smali", "smali*", true);

        }
        public static void backKillPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            string launcherActivity = Patcher.GetLauncherActivity(path);
            if (mainf.backKillCBox.SelectedIndex == 0)
            {
                if (launcherActivity != string.Empty)
                {
                    //двойной клик для выхода
                    string message = "Нажмите ещё раз, чтобы выйти";
                    string text = File.ReadAllLines(Patcher.getPathToSmali(path, launcherActivity))[0];
                    text = text.Remove(0, text.IndexOf(" L") + 1);
                    string field = "# static fields\n\n.field private static cokddop:J";
                    string value = "# virtual methods\n\n.method public onBackPressed()V\n    .registers 7\n\n    move-object v0, p0\n\n    sget-wide v2, " + text + "->cokddop:J\n\n    const/16 v4, 0x7d0\n\n    int-to-long v4, v4\n\n    add-long/2addr v2, v4\n\n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v4\n\n    cmp-long v2, v2, v4\n\n    if-lez v2, :cond_1d\n\n    const/4 v0, 0x1\n\n    invoke-static {v0}, Ljava/lang/System;->exit(I)V\n\n    :goto_16\n    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J\n\n    move-result-wide v2\n\n    sput-wide v2, " + text + "->cokddop:J\n\n    return-void\n\n    :cond_1d\n    move-object v2, v0\n\n    invoke-virtual {v2}, " + text + "->getBaseContext()Landroid/content/Context;\n\n   move-result-object v2\n\n    const-string v3,  \"" + message + "\" ##toast_message\n\n   const/4 v4, 0x0\n\n    invoke-static {v2, v3, v4}, Landroid/widget/Toast;->makeText(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;\n\n    move-result-object v2\n\n   invoke-virtual {v2}, Landroid/widget/Toast;->show()V\n\n   goto :goto_16\n.end method";
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("\\# static fields", field);
                    dictionary.Add("\\# virtual methods", value);
                    Patcher.ReplaceInFile(Patcher.getPathToSmali(path, launcherActivity), dictionary, true);
                }
            }
            if (mainf.backKillCBox.SelectedIndex == 1)
            {
                if (launcherActivity != string.Empty)
                {
                    //долгий тап для выхода
                    string value = "# virtual methods\n\n.method public onKeyLongPress(ILandroid/view/KeyEvent;)Z\n    .registers 10\n\n    move-object v0, p0\n\n    move v1, p1\n\n    move-object v2, p2\n\n    move v4, v1\n\n    const/4 v5, 0x4\n\n    if-ne v4, v5, :cond_e\n\n    const/4 v4, 0x0\n\n    invoke-static {v4}, Ljava/lang/System;->exit(I)V\n\n    const/4 v4, 0x1\n\n    move v0, v4\n\n    :goto_d\n    return v0\n\n    :cond_e\n    move-object v4, v0\n\n    move v5, v1\n\n    move-object v6, v2\n\n    invoke-super {v4, v5, v6}, Landroid/app/Activity;->onKeyUp(ILandroid/view/KeyEvent;)Z\n\n    move-result v4\n\n    move v0, v4\n\n    goto :goto_d\n.end method\n\n.method public onBackPressed()V\n    .registers 7\n\n    return-void\n.end method";
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("\\# virtual methods", value);
                    Patcher.ReplaceInFile(Patcher.getPathToSmali(path, launcherActivity), dictionary, true);
                }
            }
            if (mainf.backKillCBox.SelectedIndex == 2)
            {
                if (launcherActivity != string.Empty)
                {
                    //одиночный тап для выхода
                    string value = "# virtual methods\n\n.method public onBackPressed()V\n    .registers 7\n\n    const/4 v0, 0x1\n\n    invoke-static {v0}, Ljava/lang/System;->exit(I)V\n\n    return-void\n.end method";
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("\\# virtual methods", value);
                    Patcher.ReplaceInFile(Patcher.getPathToSmali(path, launcherActivity), dictionary, true);
                }
            }

        }
        public static void DeviceIdPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.deviceIdTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;[\\r\\n]+    move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+:try_end_(.+)[\\r\\n]+\\s+\\.catch (.+); \\{:try_start_(.+) .. :try_end_(.+)\\} :catch_(.+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/telephony/TelephonyManager;->getDeviceId()Ljava/lang/String;\n    :try_end_$2\n    .catch $3; {:try_start_$4 .. :try_end_$5} :catch_$6\n\n    const-string $7, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(I\\)Ljava\\/lang\\/String;[\\r\\n]+    move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(I\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+:try_end_(.+)[\\r\\n]+\\s+\\.catch (.+); \\{:try_start_(.+) .. :try_end_(.+)\\} :catch_(.+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/telephony/TelephonyManager;->getDeviceId()Ljava/lang/String;\n    :try_end_$2\n    .catch $3; {:try_start_$4 .. :try_end_$5} :catch_$6\n\n    const-string $7, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixDeviceID;->GetDeviceID()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(I\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixDeviceID;->GetDeviceID()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixDeviceID.smali", path + "\\smali\\fixDeviceID.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixDeviceID.smali");
            }
        }

        public static void DeviceNamePatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.deviceNameTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sdevice_name\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Global;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {$3, $4}, Landroid/provider/Settings$Secure;->getString(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n\n    const-string $9, \"" + Patcher.mainf.deviceNameTBox.Text + "\"");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sdevice_name\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Global;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $5, \"" + Patcher.mainf.deviceNameTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sdevice_name\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Global;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d)\\} \\:catch_(\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sdevice_name\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Global;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixDeviceID.smali", path + "\\smali\\fixDeviceID.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixDeviceID.smali");
            }
        }

        public static void IMEIPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.imeiTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(\\)Ljava\\/lang\\/String;[\\r\\n]+    move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.imeiTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+:try_end_(.+)[\\r\\n]+\\s+\\.catch (.+); \\{:try_start_(.+) .. :try_end_(.+)\\} :catch_(.+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/telephony/TelephonyManager;->getDeviceId()Ljava/lang/String;\n    :try_end_$2\n    .catch $3; {:try_start_$4 .. :try_end_$5} :catch_$6\n\n    const-string $7, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(I\\)Ljava\\/lang\\/String;[\\r\\n]+    move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.imeiTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(I\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+:try_end_(.+)[\\r\\n]+\\s+\\.catch (.+); \\{:try_start_(.+) .. :try_end_(.+)\\} :catch_(.+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/telephony/TelephonyManager;->getDeviceId()Ljava/lang/String;\n    :try_end_$2\n    .catch $3; {:try_start_$4 .. :try_end_$5} :catch_$6\n\n    const-string $7, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixDeviceID;->GetDeviceID()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getImei\\(I\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixDeviceID;->GetDeviceID()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixDeviceID.smali", path + "\\smali\\fixDeviceID.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixDeviceID.smali");
            }
        }

        public static void AndroidIdPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.androidIdTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {$3, $4}, Landroid/provider/Settings$Secure;->getString(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n\n    const-string $9, \"" + Patcher.mainf.androidIdTBox.Text + "\"");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $5, \"" + Patcher.mainf.androidIdTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d)\\} \\:catch_(\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAndroidID.smali", path + "\\smali\\fixAndroidID.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAndroidID.smali");
            }
        }

        public static void WifiMacPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.wifiMacTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/net\\/wifi\\/WifiInfo;->getMacAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.wifiMacTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/net\\/wifi\\/WifiInfo;->getMacAddress\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixWifiMacId;->GetWifiMacId()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixWifiMacId.smali", path + "\\smali\\fixWifiMacId.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixWifiMacId.smali");
            }
        }

        public static void BluetoothMacPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.bluetoothMacTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"bluetooth_address\"\n\n    invoke-static {$3, $4}, Landroid/provider/Settings$Secure;->getString(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n\n    const-string $9, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/bluetooth/BluetoothAdapter;->getAddress()Ljava/lang/String;\n    :try_end_$2\n    .catch Ljava/lang/Exception; {:try_start_$3 .. :try_end_$4} :catch_$5\n\n    const-string $6, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\n|\\r]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/bluetooth/BluetoothAdapter;->getAddress()Ljava/lang/String;\n\n    const-string $2, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d)\\} \\:catch_(\\d+)", "const-string $1, \"bluetooth_address\"\n\n    invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;\n    :try_end_$4\n    .catch Ljava/lang/Exception; {:try_start_$5 .. :try_end_$6} :catch_$7\n");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)", "invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;\n    :try_end_$2\n    .catch Ljava/lang/Exception; {:try_start_$3 .. :try_end_$4} :catch_$5");
                dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\n|\\r]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;", "const-string$1 $2, \"bluetooth_address\"\n\n    invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixBluetoothMac.smali", path + "\\smali\\fixBluetoothMac.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixBluetoothMac.smali");
            }
        }

        public static void SerialPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.serialTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->SERIAL:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.serialTBox.Text + "\"");
                dictionary.Add("invoke-static \\{\\}, Landroid\\/os\\/Build;->getSerial\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.serialTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->SERIAL:Ljava\\/lang\\/String;", "invoke-static {}, LfixSerial;->GetSerial()Ljava/lang/String;\n\n    move-result-object $1");
                dictionary.Add("invoke-static \\{\\}, Landroid\\/os\\/Build;->getSerial\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-static {}, LfixSerial;->GetSerial()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixSerial.smali", path + "\\smali\\fixSerial.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixSerial.smali");
            }
        }

        public static void ModelPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.modelTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->MODEL:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.modelTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->MODEL:Ljava\\/lang\\/String;", "invoke-static {}, LfixModel;->GetModel()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixModel.smali", path + "\\smali\\fixModel.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixModel.smali");
            }
        }

        public static void IDPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.idTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->ID:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.idTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->ID:Ljava\\/lang\\/String;", "invoke-static {}, LfixModel;->GetModel()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixModel.smali", path + "\\smali\\fixModel.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixModel.smali");
            }
        }

        public static void BrandPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.brandTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BRAND:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.brandTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BRAND:Ljava/lang/String;", "invoke-static {}, LfixBrand;->BRAND()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixBrand.smali", path + "\\smali\\fixBrand.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixBrand.smali");
            }
        }

        public static void IpPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.ipTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("iget-object ([pv]\\d+), ([pv]\\d+), L(\\S+);->ip:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.ipTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("iget-object ([pv]\\d+), ([pv]\\d+), L(\\S+);->ip:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void DevicePatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.deviceTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->DEVICE:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.deviceTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->DEVICE:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void BoardPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.boardTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BOARD:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.boardTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BOARD:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void ManufacturerPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.manufacturerTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->MANUFACTURER:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.manufacturerTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->MANUFACTURER:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void ProductPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.productTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->PRODUCT:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.productTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->PRODUCT:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void BssidPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.bssidTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/net/wifi/WifiInfo;->getBSSID\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.bssidTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/net/wifi/WifiInfo;->getBSSID\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void OperatorNamePatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.operatorNameTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperatorName\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorNameTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimOperatorName\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorNameTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperatorName\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimOperatorName\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void OperatorPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.operatorTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperator\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimOperator\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperator\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimOperator\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void SubscriberIdPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.subscriderIdTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSubscriberId\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.subscriderIdTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSubscriberId\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void SimSerialNumberPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.simSerialNumberTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimSerialNumber\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.simSerialNumberTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimSerialNumber\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void BluetoothAddressPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.bluetoothAddressTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/bluetooth/BluetoothAdapter;->getAddress\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.bluetoothAddressTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/bluetooth/BluetoothAdapter;->getAddress\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void CountryIsoPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.countryIsoTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkCountryIso\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.countryIsoTBox.Text + "\"");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimCountryIso\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.countryIsoTBox.Text + "\"");
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkCountryIso\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimCountryIso\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            }
        }

        public static void AccountPatch(string path)
        {
            if (!(path == "") && !Patcher.mainf.accountTBox.Text.Equals(""))
            {
                Patcher.ReplaceInFiles(path, new Dictionary<string, string>
                {
                    {
                        "iget-object ([pv]\\d+), ([pv]\\d+), Landroid\\/accounts\\/Account;->name:Ljava\\/lang\\/String;",
                        "const-string $1, \"" + Patcher.mainf.accountTBox.Text + "\""
                    }
                }, "*.smali", "smali*", true);
                return;
            }
        }

        public static void timeStopPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = false;
            if (Patcher.mainf.timeTBox.Text.Equals(""))
            {
                flag = true;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                string currentTime = Patcher.getCurrentTime();
                if (currentTime == "")
                {
                    return;
                }
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->currentTimeMillis\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + currentTime + "####");
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->nanoTime\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + currentTime + "####");
            }
            else
            {
                string text = Patcher.parseDateTime(Patcher.mainf.timeTBox.Text, path);
                if (text == "")
                {
                    MessageBox.Show(Language.errorMsgTime, Language.error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->currentTimeMillis\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + text + "####");
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->nanoTime\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + text + "####");
            }
            Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
        }

        public static void gpsPatch(string path)
        {
            if (path == "")
            {
                return;
            }
            bool flag = true;
            if (Patcher.mainf.gpsLatitudeTBox.Text.Equals("") && Patcher.mainf.gpsLongitudeTBox.Text.Equals(""))
            {
                flag = false;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (flag)
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLatitude\\(\\)D[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $2, " + Patcher.mainf.gpsLatitudeTBox.Text);
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLongitude\\(\\)D[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $2, " + Patcher.mainf.gpsLongitudeTBox.Text);
            }
            else
            {
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLatitude\\(\\)D", "invoke-static {}, LfixGps;->getLatitude()D");
                dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLongitude\\(\\)D", "invoke-static {}, LfixGps;->getLongitude()D");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            if (!flag)
            {
                Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixGps.smali", path + "\\smali\\fixGps.smali", copy);
                Patcher.moveToClassesNorNot(path, path + "\\smali\\fixGps.smali");
            }
        }

        public static void dawrepAllAuto(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_auto_replace + ":::::");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixDeviceID;->GetDeviceID()Ljava/lang/String;");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d)\\} \\:catch_(\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;", "const-string$1 $2, \"android_id\"\n\n    invoke-static {}, LfixAndroidID;->GetAndroidID()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/net\\/wifi\\/WifiInfo;->getMacAddress\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixWifiMacId;->GetWifiMacId()Ljava/lang/String;");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d)\\} \\:catch_(\\d+)", "const-string $1, \"bluetooth_address\"\n\n    invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;\n    :try_end_$4\n    .catch Ljava/lang/Exception; {:try_start_$5 .. :try_end_$6} :catch_$7\n");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)", "invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;\n    :try_end_$2\n    .catch Ljava/lang/Exception; {:try_start_$3 .. :try_end_$4} :catch_$5");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\n|\\r]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;", "const-string$1 $2, \"bluetooth_address\"\n\n    invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;", "invoke-static {}, LfixBluetoothMac;->GetBluetoothMac()Ljava/lang/String;");
            dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->SERIAL:Ljava\\/lang\\/String;", "invoke-static {}, LfixSerial;->GetSerial()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->MODEL:Ljava\\/lang\\/String;", "invoke-static {}, LfixModel;->GetModel()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BRAND:Ljava/lang/String;", "invoke-static {}, LfixBrand;->BRAND()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("iget-object ([pv]\\d+), ([pv]\\d+), L(\\S+);->ip:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->DEVICE:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BOARD:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->MANUFACTURER:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->PRODUCT:Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;\n\n    move-result-object $1");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/net/wifi/WifiInfo;->getBSSID\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperatorName\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperator\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSubscriberId\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimSerialNumber\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/bluetooth/BluetoothAdapter;->getAddress\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLatitude\\(\\)D", "invoke-static {}, LfixGps;->getLatitude()D");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLongitude\\(\\)D", "invoke-static {}, LfixGps;->getLongitude()D");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkCountryIso\\(\\)Ljava/lang/String;", "invoke-static {}, LfixAnonymous;->AnonMe()Ljava/lang/String;");
            string currentTime = Patcher.getCurrentTime();
            if (!"".Equals(currentTime))
            {
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->currentTimeMillis\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + currentTime + "####");
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->nanoTime\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + currentTime + "####");
            }
            bool copy = Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixDeviceID.smali", path + "\\smali\\fixDeviceID.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAndroidID.smali", path + "\\smali\\fixAndroidID.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixWifiMacId.smali", path + "\\smali\\fixWifiMacId.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixBluetoothMac.smali", path + "\\smali\\fixBluetoothMac.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixSerial.smali", path + "\\smali\\fixSerial.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixModel.smali", path + "\\smali\\fixModel.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixBrand.smali", path + "\\smali\\fixBrand.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixAnonymous.smali", path + "\\smali\\fixAnonymous.smali", copy);
            Patcher.copyFileOrNot(Program.pathToMyPluginDir + "\\smali\\fixGps.smali", path + "\\smali\\fixGps.smali", copy);
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixDeviceID.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAndroidID.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixWifiMacId.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixBluetoothMac.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixSerial.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixModel.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixBrand.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixAnonymous.smali");
            Patcher.moveToClassesNorNot(path, path + "\\smali\\fixGps.smali");
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_auto_replace_done +  ":::::");
        }

        public static void dawrepAllManual(string path)
        {
            if (path == "")
            {
                return;
            }
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_manual_replace + ":::::");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("invoke-virtual \\{[^\\}]+\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;[\\r\\n]+    move-result-object ([pv]\\d+)", "const-string $1, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/telephony\\/TelephonyManager;->getDeviceId\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+:try_end_(.+)[\\r\\n]+\\s+\\.catch (.+); \\{:try_start_(.+) .. :try_end_(.+)\\} :catch_(.+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/telephony/TelephonyManager;->getDeviceId()Ljava/lang/String;\n    :try_end_$2\n    .catch $3; {:try_start_$4 .. :try_end_$5} :catch_$6\n\n    const-string $7, \"" + Patcher.mainf.deviceIdTBox.Text + "\"");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"android_id\"\n\n    invoke-static {$3, $4}, Landroid/provider/Settings$Secure;->getString(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n\n    const-string $9, \"" + Patcher.mainf.androidIdTBox.Text + "\"");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sandroid_id\\S[\\n|\\r]+    invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $5, \"" + Patcher.mainf.androidIdTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/net\\/wifi\\/WifiInfo;->getMacAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.wifiMacTBox.Text + "\"");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\r\\n]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;-\\>getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"bluetooth_address\"\n\n    invoke-static {$3, $4}, Landroid/provider/Settings$Secure;->getString(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;\n    :try_end_$5\n    .catch Ljava/lang/Exception; {:try_start_$6 .. :try_end_$7} :catch_$8\n\n    const-string $9, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+\\:try_end_(\\d+)[\\r\\n]+\\s+\\.catch Ljava\\/lang\\/Exception; \\{\\:try_start_(\\d+) \\.\\. \\:try_end_(\\d+)\\} \\:catch_(\\d+)[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/bluetooth/BluetoothAdapter;->getAddress()Ljava/lang/String;\n    :try_end_$2\n    .catch Ljava/lang/Exception; {:try_start_$3 .. :try_end_$4} :catch_$5\n\n    const-string $6, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
            dictionary.Add("const-string([^\\s]*) ([pv]\\d+), \\Sbluetooth_address\\S[\\n|\\r]+\\s+invoke-static \\{([pv]\\d+), ([pv]\\d+)\\}, Landroid\\/provider\\/Settings\\$Secure;->getString\\(Landroid\\/content\\/ContentResolver;Ljava\\/lang\\/String;\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string$1 $2, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid\\/bluetooth\\/BluetoothAdapter;->getAddress\\(\\)Ljava\\/lang\\/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "invoke-virtual {$1}, Landroid/bluetooth/BluetoothAdapter;->getAddress()Ljava/lang/String;\n\n    const-string $2, \"" + Patcher.mainf.bluetoothMacTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->SERIAL:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.serialTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid\\/os\\/Build;->MODEL:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.modelTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BRAND:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.brandTBox.Text + "\"");
            dictionary.Add("iget-object ([pv]\\d+), ([pv]\\d+), L(\\S+);->ip:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.ipTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->DEVICE:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.deviceTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->BOARD:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.boardTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->MANUFACTURER:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.manufacturerTBox.Text + "\"");
            dictionary.Add("sget-object ([pv]\\d+), Landroid/os/Build;->PRODUCT:Ljava/lang/String;", "const-string $1, \"" + Patcher.mainf.productTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/net/wifi/WifiInfo;->getBSSID\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.bssidTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperatorName\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorNameTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkOperator\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.operatorTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSubscriberId\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.subscriderIdTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getSimSerialNumber\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.simSerialNumberTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/bluetooth/BluetoothAdapter;->getAddress\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.bluetoothAddressTBox.Text + "\"");
            dictionary.Add("iget-object ([pv]\\d+), ([pv]\\d+), Landroid\\/accounts\\/Account;->name:Ljava\\/lang\\/String;", "const-string $1, \"" + Patcher.mainf.accountTBox.Text + "\"");
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLatitude\\(\\)D[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $2, " + Patcher.mainf.gpsLatitudeTBox.Text);
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/location/Location;->getLongitude\\(\\)D[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $2, " + Patcher.mainf.gpsLongitudeTBox.Text);
            dictionary.Add("invoke-virtual \\{([pv]\\d+)\\}, Landroid/telephony/TelephonyManager;->getNetworkCountryIso\\(\\)Ljava/lang/String;[\\r\\n]+\\s+move-result-object ([pv]\\d+)", "const-string $2, \"" + Patcher.mainf.countryIsoTBox.Text + "\"");
            string text = Patcher.parseDateTime(Patcher.mainf.timeTBox.Text, path);
            if (!"".Equals(text))
            {
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->currentTimeMillis\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + text + "####");
                dictionary.Add("invoke-static \\{\\}, Ljava/lang/System;->nanoTime\\(\\)J[\\r\\n\\s]+move-result-wide ([pv]\\d+)", "const-wide $1, " + text + "####");
            }
            Patcher.ReplaceInFiles(path, dictionary, "*.smali", "smali*", true);
            Patcher.mainf.appendProgressTbox(":::::" + Language.log_manual_replace_done + ":::::");
        }

    }
}