using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace DrinkServiceImplementation
{
    public class ListSaver
    {
        string m_Path;
        public ListSaver(string Path)
        {
            m_Path = Path;

            if (!Directory.Exists(m_Path))
            {
                Directory.CreateDirectory(m_Path);
            }
        }
        public void SaveObject(object Obj,string FileName)
        {
            lock (this)
            {
                //Console.WriteLine("Saving " + FileName);
                XmlSerializer serialiser = new XmlSerializer(Obj.GetType());
                StreamWriter writer = new StreamWriter(m_Path + "/" + FileName);
                serialiser.Serialize(writer, Obj);
                writer.Close();
                Console.WriteLine("Saving " + FileName + " Done");
            }
        }
        public bool PathExists(string FileName)
        {
            return File.Exists(m_Path + "/" + FileName);
        }
        public List<T> GetList<T>(string FileName,bool NewIfPathDoesntExist)
        {
            lock (this)
            {
                //Console.WriteLine("Getting " + FileName);
                bool FileExists = File.Exists(m_Path + "/" + FileName);
                if (!FileExists && NewIfPathDoesntExist)
                {
                    return new List<T>();
                }
                else if (!FileExists)
                {
                    throw new Exception("File Doesn't exist");
                }
                XmlSerializer serialiser = new XmlSerializer(typeof(List<T>));
                StreamReader reader = new StreamReader(m_Path + "/" + FileName);
                object deserialised = serialiser.Deserialize(reader);
                reader.Close();
                return (List<T>)deserialised;
                Console.WriteLine("Getting " + FileName + " done");
            }
        }

        public T GetObject<T>(string FileName)
        {
            
            
            //Console.WriteLine("Getting " + FileName);
            bool FileExists = File.Exists(m_Path + "/" + FileName);
            if (!FileExists)
            {
                return  default(T);
            }
            
            XmlSerializer serialiser = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(m_Path + "/" + FileName);
            object deserialised = serialiser.Deserialize(reader);
            reader.Close();
            return (T)deserialised;
            Console.WriteLine("Getting " + FileName + " done");
        }

        

    }
}
