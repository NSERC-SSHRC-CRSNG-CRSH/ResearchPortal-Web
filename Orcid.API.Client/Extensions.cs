using Orcid.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
namespace Orcid.API.Client
{
    public static class Extensions
    {


        public static DateTime? ToDateTime(this fuzzydate fuzzydate)
        {
            if (fuzzydate == null)
            {
                return null;
            }

            string year = fuzzydate.year?.Text.FirstOrDefault();
            string month = fuzzydate.month?.Text.FirstOrDefault();
            string day = fuzzydate.day?.Text.FirstOrDefault();
            if (string.IsNullOrEmpty(year))
            {
                return null;
            }
            if (string.IsNullOrEmpty(month))
            {
                return new DateTime(Convert.ToInt32(year));
            }
            if (string.IsNullOrEmpty(day))
            {
                return new DateTime(Convert.ToInt32(year));
            }
            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
        }

        #region deserialize XML
        /// <summary>
        /// Deserialize the object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T DeserializeXml<T>(string xml)
        {
            return (T) DeserializeXml(xml, typeof(T)) ;
        }

        private static object DeserializeXml(string xml, Type type)
        {
            if (type.IsSerializable)
            {
                return DeserializeSerializableXml(xml, type);
            }
            if (Attribute.IsDefined(type, typeof(DataContractAttribute)))
            {
                return DeserializeDataContractXml(xml, type);
            }
            throw new Exception(string.Format("Unable to deserialize XML for type: '{0}'", type.FullName));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeSerializableXml<T>(string xml)
        {
            Type type = typeof(T);
            object toRet = DeserializeSerializableXml(xml, type);

            return (T)toRet;
        }
        private static object DeserializeSerializableXml(string xml, Type type)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(type);
                XmlRootAttribute xmlRootAttribute = type.GetCustomAttribute<XmlRootAttribute>();
                if (xmlRootAttribute == null)
                {
                    xmlRootAttribute = new XmlRootAttribute();
                    XmlTypeAttribute xmlTypeAttribute = type.GetCustomAttribute<XmlTypeAttribute>();
                    if (xmlTypeAttribute != null && !string.IsNullOrEmpty(xmlTypeAttribute.Namespace))
                    {
                        xmlRootAttribute.IsNullable = true;
                        xmlRootAttribute.Namespace = xmlTypeAttribute.Namespace;
                        serializer = new XmlSerializer(type, xmlRootAttribute);
                    }
                    else
                    {
                        serializer = new XmlSerializer(type);
                    }

                }
                else
                {
                    serializer = new XmlSerializer(type);
                }
                using (TextReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xml)), Encoding.UTF8))
                {
                    return serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static T DeserializeDataContractXml<T>(string xml)
        {
            Type type = typeof(T);
            object toRet = DeserializeDataContractXml(xml, type);
            return (T)toRet;
        }

        /// <summary>
        /// Given a type deserialize it to an Object
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object DeserializeDataContractXml(string xml, Type type)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    DataContractSerializer deserializer = new DataContractSerializer(type);
                    return deserializer.ReadObject(reader);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        ///  Given an xml file return the de-searialized object
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static T DeserializeXmlFile<T>(string file, Dictionary<string, string> replaceMappings)
        {
            string contents = File.ReadAllText(file);
            foreach (string key in replaceMappings.Keys)
            {
                contents = contents.Replace(key, replaceMappings[key]);
            }
            return DeserializeXml<T>(contents);
        }


        /// <summary>
        ///  Given an xml file return the de-searialized object
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static T DeserializeXmlFile<T>(string file)
        {
            string contents = File.ReadAllText(file);
            return DeserializeXml<T>(contents);
        }

        #endregion deserialize XML


        #region serialize XML

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeXML<T>(this T o)
        {
            if (Object.ReferenceEquals(o, null))
            {
                return "";
            }
            if (Attribute.IsDefined(o.GetType(), typeof(DataContractAttribute)))
            {
                return o.SerializeDataContractXML();
            }
            else if (o.GetType().IsSerializable)
            {
                return o.SerializeSerializableXML();
            }
            else
            {
                return string.Format("Error Serializing Object of type: [{0}], it is neither Serializable or DataContract decorated.", typeof(T).FullName);
            }

        }

        public static string SerializeSerializableXML<T>(this T o)
        {
            try
            {
                if (Object.ReferenceEquals(o, null))
                {
                    return "";
                }
                Stream stream = new MemoryStream();

                XmlSerializer serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(stream, o);
                stream.Flush(); // flush to memory
                stream.Seek(0, SeekOrigin.Begin); // seek to the beginning

                // extract the text.
                TextReader tr = new StreamReader(stream);
                return tr.ReadToEnd();
            }
            catch (Exception ex)
            {
                //ILog log = log4net.LogManager.GetLogger(typeof(T));
                //log.Warn(string.Format("Error Serializing Object of type: [{0}]", typeof(T).FullName), ex);
                return string.Format("Error Serializing Object of type: [{0}], with message: '{1}'", o.GetType().FullName, ex.Message);
            }
        }

        /// <summary>
        /// Serialize the Entity to Xml
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeDataContractJSON<T>(this T o)
        {
            if (Object.ReferenceEquals(o, null))
            {
                return "";
            }

            DataContractJsonSerializer dcSerializer = new DataContractJsonSerializer(o.GetType());

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dcSerializer.WriteObject(memoryStream, o);
                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Serialize the Entity to Xml
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeDataContractXML<T>(this T o)
        {
            if (Object.ReferenceEquals(o, null))
            {
                return "";
            }

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    ",
                OmitXmlDeclaration = true
            };

            DataContractSerializer dcSerializer = new DataContractSerializer(o.GetType());
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                dcSerializer.WriteObject(writer, o);
            }
            return sb.ToString();
        }


        /// <summary>
        /// SerializeXML the object to an xml file.
        /// </summary>
        /// <param name="filename"></param>
        public static void SerializeXML<T>(this T o, string filename)
        {
            TextWriter stream = File.CreateText(filename);
            stream.Write(o.SerializeXML());
            stream.Flush();
            stream.Close();
        }

        #endregion serialize XML

    }
}
