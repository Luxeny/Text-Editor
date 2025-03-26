using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

[Serializable]
public class TextFile
{
    public string Name { get; set; }
    public string Content { get; set; }
    public bool IsSaved { get; set; }
    
    public TextFile(string name, string content)
    {
        Name = name;
        Content = content;
        IsSaved = false;
    }
}

public class FileManager
{
    private readonly string _workspacePath = "./workspace/";

    public FileManager()
    {
        Directory.CreateDirectory(_workspacePath);
        Directory.CreateDirectory(Path.Combine(_workspacePath, "binary"));
        Directory.CreateDirectory(Path.Combine(_workspacePath, "xml"));
    }

    public void SaveBinary(TextFile file)
    {
        string path = Path.Combine(_workspacePath, "binary", $"{file.Name}.bin");
        using (var stream = File.Create(path))
        {
            new BinaryFormatter().Serialize(stream, file);
        }
        file.IsSaved = true;
    }

    public void SaveXml(TextFile file)
    {
        string path = Path.Combine(_workspacePath, "xml", $"{file.Name}.xml");
        var serializer = new XmlSerializer(typeof(TextFile));
        using (var writer = new StreamWriter(path))
        {
            serializer.Serialize(writer, file);
        }
        file.IsSaved = true;
    }

    public List<string> GetSavedFiles()
    {
        var files = new List<string>();
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "binary"), "*.bin"));
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "xml"), "*.xml"));
        return files;
    }

    public TextFile LoadFile(string filePath)
    {
        if (filePath.EndsWith(".bin"))
        {
            using (var stream = File.OpenRead(filePath))
            {
                return (TextFile)new BinaryFormatter().Deserialize(stream);
            }
        }
        else
        {
            var serializer = new XmlSerializer(typeof(TextFile));
            using (var reader = new StreamReader(filePath))
            {
                return (TextFile)serializer.Deserialize(reader);
            }
        }
    }
}

public class TextEditor
{
    private readonly FileManager _fileManager = new FileManager();
    private TextFile _currentFile;
    private string _lastContent;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Текстовый редактор");
            Console.WriteLine("1. Создать файл");
            Console.WriteLine("2. Открыть файл");
            Console.WriteLine("3. Сохранить файл");
            Console.WriteLine("4. Показать содержимое");
            Console.WriteLine("0. Выход");
            
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    CreateFile();
                    break;
                case "2":
                    OpenFile();
                    break;
                case "3":
                    SaveFile();
                    break;
                case "4":
                    ShowContent();
                    break;
                case "0":
                    return;
            }
        }
    }

    private void CreateFile()
    {
        Console.Write("Имя файла: ");
        var name = Console.ReadLine();
        Console.Write("Содержимое: ");
        var content = Console.ReadLine();
        
        _currentFile = new TextFile(name, content);
        _lastContent = content;
        Console.WriteLine("Файл создан");
        Console.ReadKey();
    }

    private void OpenFile()
    {
        var files = _fileManager.GetSavedFiles();
        for (int i = 0; i < files.Count; i++)
        {
            Console.WriteLine($"{i+1}. {Path.GetFileName(files[i])}");
        }
        
        Console.Write("Выберите файл: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= files.Count)
        {
            _currentFile = _fileManager.LoadFile(files[index-1]);
            _lastContent = _currentFile.Content;
            Console.WriteLine("Файл загружен");
        }
        Console.ReadKey();
    }

    private void SaveFile()
    {
        if (_currentFile == null)
        {
            Console.WriteLine("Нет открытого файла");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("1. Бинарный формат");
        Console.WriteLine("2. XML формат");
        var choice = Console.ReadLine();
        
        try
        {
            if (choice == "1") _fileManager.SaveBinary(_currentFile);
            else if (choice == "2") _fileManager.SaveXml(_currentFile);
            _lastContent = _currentFile.Content;
            Console.WriteLine("Файл сохранён");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        Console.ReadKey();
    }

    private void ShowContent()
    {
        if (_currentFile == null)
        {
            Console.WriteLine("Нет открытого файла");
        }
        else
        {
            Console.WriteLine($"Содержимое файла {_currentFile.Name}:");
            Console.WriteLine(_currentFile.Content);
            Console.WriteLine($"Статус: {(_currentFile.IsSaved ? "Сохранён" : "Не сохранён")}");
        }
        Console.ReadKey();
    }
}

class Program
{
    static void Main()
    {
        new TextEditor().Run();
    }
}
