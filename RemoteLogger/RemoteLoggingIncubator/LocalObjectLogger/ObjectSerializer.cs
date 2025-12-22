using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LocalObjectLogger
{
    public static class ObjectSerializer
    {
        private static int _counter;
        private static readonly string _logDirectoryPath;

        private static Dictionary<string, int> _fileIndices = [];

        public static void ResetFileCounter()
        {
            _counter = 0;
        }

        private static List<string> _skipNamespaces = new()
        {
            "System.Reflection",
            "Google.Protobuf.Reflection"
        };

        private static List<string> _skipTypes = new()
        {
            "Semilab.SAM.Interfaces.ILog"
        };

        static ObjectSerializer()
        {
            _logDirectoryPath = Path.Combine(Environment.CurrentDirectory, "MyLog");
            if (!Directory.Exists(_logDirectoryPath))
            {
                Directory.CreateDirectory(_logDirectoryPath);
            }
        }

        public static void ResetFileNames()
        {
            lock (_lock)
            {
                _fileIndices.Clear();
            }
        }

        public static bool WriteToFileStartWithTimeStamp(string fileName, object objectToWrite)
        {
            return WriteToFile(fileName, objectToWrite, true, false, false, false);
        }

        public static bool WriteToFileStartWithIndex(string fileName, object objectToWrite)
        {
            return WriteToFile(fileName, objectToWrite, false, true, false, false);
        }

        public static bool WriteToFileStartWithIndexTxt(string fileName, object objectToWrite)
        {
            return WriteToFile(fileName, objectToWrite, false, true, false, true);
        }

        public static bool WriteToFileIndexByName(string fileName, object objectToWrite)
        {
            return WriteToFile(fileName, objectToWrite, false, false, true, false);
        }

        public static bool WriteToFile(string fileName, object objectToWrite)
        {
            return WriteToFile(fileName, objectToWrite, false, false, false, false);
        }

        private static object _lock = new();

        private static bool WriteToFile(string fileName, object objectToWrite, bool withTimeStamp, bool withIndex, bool indexByName, bool isTxt)
        {
            try
            {
                if (withTimeStamp)
                {
                    string timeStamp = DateTime.Now.ToString("HH-mm-ss_fff");
                    fileName = $"{timeStamp}_{fileName}";
                }

                if (withIndex)
                {
                    fileName = $"{_counter++}_{fileName}";
                }

                if (indexByName)
                {
                    lock (_lock)
                    {
                        if (_fileIndices.TryGetValue(fileName, out int currentIndex))
                        {
                            currentIndex++;
                            _fileIndices[fileName] = currentIndex;
                        }
                        else
                        {
                            currentIndex = 0;
                            _fileIndices.Add(fileName, currentIndex);
                        }

                        fileName = $"{fileName}_{currentIndex}";
                    }
                }

                if (objectToWrite == null)
                {
                    File.WriteAllText(Path.Combine(_logDirectoryPath, $"_error_{fileName}.txt"), $"{nameof(objectToWrite)} was null.");
                    return true;
                }

                if (isTxt)
                {
                    File.WriteAllText(Path.Combine(_logDirectoryPath, $"{fileName}.txt"), objectToWrite.ToString());
                    return true;
                }

                // Custom settings to include all fields (public and private)
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new SafeContractResolver
                    {
                        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                    },
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    // ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    MaxDepth = 10,
                    Error = (sender, args) =>
                    {
                        // Newtonsoft.Json tries to access it due to the aggressive settings (BindingFlags.NonPublic), and fails because it’s either:
                        // Not accessible via reflection
                        // Throws an exception when accessed

                        // Skip problematic members
                        args.ErrorContext.Handled = true;
                    }
                };

                // Check if the object has any serializable properties
                var contract = settings.ContractResolver.ResolveContract(objectToWrite.GetType());
                if (contract is JsonObjectContract objContract && objContract.Properties.Any())
                {
                    //Console.WriteLine($"Serializing type: {objectToWrite.GetType().FullName}");
                    //Console.WriteLine($"Properties found: {objContract.Properties.Count}");
                    //foreach (var prop in objContract.Properties)
                    //{
                    //    Console.WriteLine($"  - {prop.PropertyName} ({prop.PropertyType})");
                    //}
                }
                else
                {
                    //Console.WriteLine($"Warning: No serializable properties found for type {objectToWrite.GetType().Name}");
                }

                // Serialize to JSON string
                string json = JsonConvert.SerializeObject(objectToWrite, settings);
                File.WriteAllText(Path.Combine(_logDirectoryPath, $"{fileName}.json"), json);
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(Path.Combine(_logDirectoryPath, $"_error_{fileName}.json"), $"{ex}");
                    //#if IsNET
                    byte[] objectInBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(objectToWrite);
                    File.WriteAllBytes(Path.Combine(_logDirectoryPath, $"{fileName}.txt"), objectInBytes);
                    return true;
                    //#endif
                    return false;
                }
                catch (Exception ex2)
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(_logDirectoryPath, $"_error_{fileName}.txt"), $"{ex2}");
                    }
                    catch
                    {
                        // nothing we can do.
                    }
                }
                return false;
            }

            return true;
        }

        public class SafeContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);

                return props.Where(p =>
                {
                    var propType = p.PropertyType;

                    if (typeof(Delegate).IsAssignableFrom(propType))
                    {
                        return false;
                    }
#if IsNET
#endif
                    if (propType != null)
                    {
                        if (propType.Namespace != null)
                        {
                            string nameSpace = propType.Namespace;
                            foreach (string skipNamespace in _skipNamespaces)
                            {
                                if (nameSpace.StartsWith(skipNamespace))
                                {
                                    return false;
                                }
                            }
                        }

                        string fullName = propType.FullName;
                        if (fullName != null)
                        {
                            foreach (string typeToSkip in _skipTypes)
                            {
                                if (fullName.StartsWith(typeToSkip))
                                {
                                    return false;
                                }
                            }

                            if (fullName.Contains("Logger"))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            fullName = $"Name was missing for this property {propType.Name}";
                        }

                        //File.AppendAllText(filePath, $"{fullName}{Environment.NewLine}");
                    }
                    else
                    {
                        File.AppendAllText(filePath, $"PropertyType was null{p}{Environment.NewLine}");
                    }

                    return true;
                }).ToList();
            }
        }
        static string filePath = @"c:\Temp\file.txt";

        class DefaultContractResolverExtended : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                List<MemberInfo> memberInfos = base.GetSerializableMembers(objectType);

                return memberInfos;
            }
        }
    }
}
