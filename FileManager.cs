using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class FileManager
{
    private readonly string _workspacePath;
    private readonly Dictionary<string, TextFile> _fileCache;
    private FileSystemWatcher _watcher;

    public FileManager(string workspacePath = "./workspace/")
    {
        _workspacePath = workspacePath;
        _fileCache = new Dictionary<string, TextFile>();
        InitializeWorkspace();
        InitializeWatcher();
    }

    private void InitializeWorkspace()
    {
        Directory.CreateDirectory(_workspacePath);
        Directory.CreateDirectory(Path.Combine(_workspacePath, "binary"));
        Directory.CreateDirectory(Path.Combine(_workspacePath, "xml"));
    }

    private void InitializeWatcher()
    {
        _watcher = new FileSystemWatcher(_workspacePath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };
        _watcher.Changed += (s, e) => OnFileChanged?.Invoke(e.FullPath);
        _watcher.EnableRaisingEvents = true;
    }

    public event Action<string> OnFileChanged;
    public void ClearCache() => _fileCache.Clear();

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

    public TextFile LoadBinary(string filePath)
    {
        try
        {
            if (_fileCache.TryGetValue(filePath, out var cachedFile)) 
                return cachedFile;

            using (FileStream stream = File.OpenRead(filePath))
            {
                var formatter = new BinaryFormatter();
                var file = (TextFile)formatter.Deserialize(stream);
                _fileCache[filePath] = file;
                return file;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки бинарного файла: {ex.Message}");
            return null;
        }
    }

    public TextFile LoadXml(string filePath)
    {
        try
        {
            if (_fileCache.TryGetValue(filePath, out var cachedFile))
                return cachedFile;

            var serializer = new XmlSerializer(typeof(TextFile));
            using (TextReader reader = new StreamReader(filePath))
            {
                var file = (TextFile)serializer.Deserialize(reader);
                _fileCache[filePath] = file;
                return file;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки XML файла: {ex.Message}");
            return null;
        }
    }

    public void SaveBinary(TextFile file)
    {
        try
        {
            string path = GetBinaryPath(file.Name);
            using (FileStream stream = File.Create(path))
            {
                new BinaryFormatter().Serialize(stream, file);
            }
            _fileCache[path] = file;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения бинарного файла: {ex.Message}");
        }
    }
    
    public void SaveXml(TextFile file)
    {
        try
        {
            string path = GetXmlPath(file.Name);
            using (TextWriter writer = new StreamWriter(path))
            {
                new XmlSerializer(typeof(TextFile)).Serialize(writer, file);
            }
            _fileCache[path] = file;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения XML файла: {ex.Message}");
        }
    }
}
