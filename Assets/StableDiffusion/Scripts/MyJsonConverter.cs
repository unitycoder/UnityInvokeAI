// https://anduin.aiursoft.cn/post/2020/10/13/how-to-serialize-json-object-in-c-without-newtonsoft-json

using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace UnityLibrary
{
    public static class SimpleJsonConverter
    {
        /// Deserialize an from json string
        public static T Deserialize<T>(string body)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(body);
                writer.Flush();
                stream.Position = 0;
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
            }
        }

        /// Serialize an object to json
        public static string Serialize<T>(T item)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(ms, item);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}