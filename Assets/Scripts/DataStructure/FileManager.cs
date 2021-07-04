using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using ZeroFormatter;

public class FileManager<T> : MonoBehaviour
{
    public static T OpenFile(string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        // formatter.Binder = new typeconvertor();
        FileStream stream = new FileStream(filePath, FileMode.Open);
        T data = default(T);
        try
        {
            data = (T)formatter.Deserialize(stream);
        }
        catch
        {
            stream.Close();
            throw null;
        }
        stream.Close();
        return data;
    }
    public static void SaveFile(string name, T data)
    {
        string filePath = Application.persistentDataPath + "/" + name + ".va";
        FileStream stream = new FileStream(filePath, FileMode.Create);
        ZeroFormatterSerializer.Serialize<T>(stream, data);
        stream.Close();
    }

    public static T LoadFile(string filePath)
    {
        FileStream stream = new FileStream(filePath, FileMode.Open);
        T data = ZeroFormatterSerializer.Deserialize<T>(stream);
        stream.Close();
        return data;
    }
    public static void SaveFile_ZF(string name, T data)
    {
        string filePath = Application.persistentDataPath + "/" + name + ".va";
        FileStream stream = new FileStream(filePath, FileMode.Create);
        ZeroFormatterSerializer.Serialize<T>(stream, data);
        stream.Close();
    }

    public static T LoadFile_ZF(string filePath)
    {
        if (Path.GetExtension(filePath) == ".pa")
        {
            return OpenFile(filePath);
        }
         FileStream stream = new FileStream(filePath, FileMode.Open);
        T data = ZeroFormatterSerializer.Deserialize<T>(stream);
        stream.Close();
        return data;
    }
}
