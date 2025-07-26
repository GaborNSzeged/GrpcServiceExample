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
        private static readonly string _logDirectoryPath;

        static ObjectSerializer()
        {
            _logDirectoryPath = Path.Combine(Environment.CurrentDirectory, "MyLog");
            if (!Directory.Exists(_logDirectoryPath))
            {
                Directory.CreateDirectory(_logDirectoryPath);
            }
        }

        public static bool WriteToFile(string fileName, object objectToWrite)
        {
            try
            {
                if (objectToWrite == null)
                {
                    File.WriteAllText(Path.Combine(_logDirectoryPath, $"_error_{fileName}.txt"), $"{nameof(objectToWrite)} was null.");
                    return true;
                }

                // Custom settings to include all fields (public and private)
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new SafeContractResolver
                    {
                        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    },
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        // Newtonsoft.Json tries to access it due to the aggressive settings (BindingFlags.NonPublic), and fails because it’s either:
                        // Not accessible via reflection
                        // Throws an exception when accessed

                        // Skip problematic members
                        args.ErrorContext.Handled = true;
                    }
                };

                // Serialize to JSON string
                string json = JsonConvert.SerializeObject(objectToWrite, settings);
                File.WriteAllText(Path.Combine(_logDirectoryPath, $"{fileName}.json"), json);
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(Path.Combine(_logDirectoryPath, $"_error_{fileName}.json"), $"{ex}");
#if IsNET
                    byte[] objectInBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(objectToWrite);
                    File.WriteAllBytes(Path.Combine(_logDirectoryPath, $"{fileName}.txt"), objectInBytes);
                    return true;
#endif
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

                    // Skip delegates, reflection types, or Protobuf internals
                    if (typeof(Delegate).IsAssignableFrom(propType))
                    {
                        return false;
                    }
#if IsNET
#endif
                    if (propType?.Namespace != null && (
                            propType.Namespace.StartsWith("System.Reflection") ||
                            propType.Namespace.StartsWith("Google.Protobuf.Reflection")
                        ))
                    {
                        return false;
                    }

                    return true;
                }).ToList();
            }
        }

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
