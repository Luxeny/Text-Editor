using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class FileManager
{
    private readonly string _workspacePath;

    public FileManager(string workspacePath = "./workspace/")
    {
        _workspacePath = workspacePath;
        InitializeWorkspace();
    }

    private void InitializeWorkspace()
    {
        Directory.CreateDirectory(_workspacePath);
        Directory.CreateDirectory(Path.Combine(_workspacePath, "binary"));
        Directory.CreateDirectory(Path.Combine(_workspacePath, "xml"));
    }

    public string GetBinaryPath(string fileName) =>
      Path.Combine(_workspacePath, "binary", $"{fileName}.bin");

    public string GetXmlPath(string fileName) =>
      Path.Combine(_workspacePath, "xml", $"{fileName}.xml");

    public List<string> GetAllFiles()
    {
        var files = new List<string>();
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "binary"), "*.bin"));
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "xml"), "*.xml"));
        return files;
    }

    public List<string> SearchFiles(List<string> keywords)
    {
        var result = new List<string>();
        foreach (var file in GetAllFiles())
        {
            TextFile textFile;
            if (file.EndsWith(".bin"))
            {
                textFile = LoadBinary(file);
            }
            else
            {
                textFile = LoadXml(file);
            }

            foreach (var keyword in keywords)
            {
                if (textFile.Content.Contains(keyword))
                {
                    result.Add(file);
                    break;
                }
            }
        }
        return result;
    }

    public void SaveBinary(TextFile file)
    {
        string path = GetBinaryPath(file.Name);

        using (FileStream stream = File.Create(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, file);
        }

        file.IsSaved = true;
        file.FilePath = path;
    }

    public TextFile LoadBinary(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (TextFile)formatter.Deserialize(stream);
        }
    }

    public void SaveXml(TextFile file)
    {
        string path = GetXmlPath(file.Name);
        XmlSerializer serializer = new XmlSerializer(typeof(TextFile));

        using (TextWriter writer = new StreamWriter(path))
        {
            serializer.Serialize(writer, file);
        }

        file.IsSaved = true;
        file.FilePath = path;
    }

    public TextFile LoadXml(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
        using (TextReader reader = new StreamReader(filePath))
        {
            return (TextFile)serializer.Deserialize(reader);
        }
    }
}