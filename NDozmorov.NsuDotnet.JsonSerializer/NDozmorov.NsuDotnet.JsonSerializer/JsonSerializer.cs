using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NDozmorov.NsuDotnet.JsonSerializer
{
    class JsonSerializer
    {
        public static StreamWriter Serialize(Object obj, StreamWriter streamWriter)
        {
            if (obj.GetType().IsClass == false)
            {
                throw new SerializationException("Object isn't an object of a class. Can't serialize it.");
            }

            if (obj.GetType().IsSerializable == false)
            {
                throw new SerializationException("Object isn't serializable. Can't serialize it.");
            }

            return SerializeObject(obj, streamWriter);
        }

        private static StreamWriter SerializeObject(Object obj, StreamWriter streamWriter, int tabNumber = 1)
        {
            if (obj == null)
            {
                streamWriter.Write("null");
                return streamWriter;
            }

            Type type = obj.GetType();

            if (type.IsEquivalentTo(typeof(bool)))
            {
                if ((bool) obj)
                {
                    streamWriter.Write("true");
                }
                else
                {
                    streamWriter.Write("false");
                }
                return streamWriter;
            }

            if (type.IsEquivalentTo(typeof(string)))
            {
                streamWriter.Write("\"{0}\"", obj);
                return streamWriter;
            }

            if (type.IsPrimitive)
            {
                streamWriter.Write(obj);
                return streamWriter;
            }

            if (type.IsArray)
            {
                var array = (Array) obj;
                int arrayLength = array.GetLength(0);

                streamWriter.WriteLine("[");

                for (int i = 0; i < arrayLength; i++)
                {
                    PrintTabs(tabNumber, streamWriter);

                    SerializeObject(array.GetValue(i), streamWriter, tabNumber + 1);
                    if (i != arrayLength - 1)
                    {
                        streamWriter.Write(", ");
                    }
                    streamWriter.WriteLine();
                }

                PrintTabs(tabNumber - 1, streamWriter);

                streamWriter.Write("]");
                return streamWriter;
            }

            if (type.IsClass)
            {
                streamWriter.WriteLine("{");
                
                FieldInfo[] fields = type.GetFields();
                int fieldsLength = fields.GetLength(0);

                for (int i = 0; i < fieldsLength; i++)
                {
                    var field = fields[i];
                    if (field.IsNotSerialized == false)
                    {
                        PrintTabs(tabNumber, streamWriter);

                        streamWriter.Write("\"{0}\" = ", field.Name);
                        SerializeObject(field.GetValue(obj), streamWriter, tabNumber + 1);

                        if (i != fieldsLength - 1)
                        {
                            streamWriter.WriteLine(",");
                        }
                    }
                }

                PropertyInfo[] properties = type.GetProperties();
                int propertiesLength = properties.GetLength(0);

                if (propertiesLength != 0 && fieldsLength != 0)
                {
                    streamWriter.WriteLine(",");
                }

                for (int i = 0; i < propertiesLength; i++)
                {
                    var property = properties[i];
                    PrintTabs(tabNumber, streamWriter);

                    streamWriter.Write("\"{0}\" = ", property.Name);
                    SerializeObject(property.GetValue(obj), streamWriter, tabNumber + 1);

                    if (i != propertiesLength - 1)
                    {
                        streamWriter.WriteLine(",");
                    }
                }

                streamWriter.WriteLine();
                PrintTabs(tabNumber - 1, streamWriter);
                streamWriter.Write("}");
            }
            return streamWriter;
        }

        private static void PrintTabs(int tabNumber, StreamWriter streamWriter)
        {
            for (int i = 0; i < tabNumber; i++)
            {
                streamWriter.Write("\t");
            }
        }

    }
}
