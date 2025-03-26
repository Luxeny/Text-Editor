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

// Рреализация Memento
public class TextFileMemento
{
    public string Content { get; }
    public DateTime Timestamp { get; }

    public TextFileMemento(string content)
    {
        Content = content;
        Timestamp = DateTime.Now;
    }
}

public class FileManager
{
    private readonly string _workspacePath = "./workspace/";

    public FileManager()
    {
        InitializeWorkspace();
    }

    private void InitializeWorkspace()
    {
        Directory.CreateDirectory(_workspacePath);
        Directory.CreateDirectory(Path.Combine(_workspacePath, "binary"));
        Directory.CreateDirectory(Path.Combine(_workspacePath, "xml"));
    }

    // Бинарная сериализация
    public void SaveBinary(TextFile file)
    {
        string path = Path.Combine(_workspacePath, "binary", $"{file.Name}.bin");
        using (var stream = File.Create(path))
        {
            new BinaryFormatter().Serialize(stream, file);
        }
        file.IsSaved = true;
    }

    // XML сериализация
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

    public TextFile LoadBinary(string fileName)
    {
        string path = Path.Combine(_workspacePath, "binary", $"{fileName}.bin");
        using (var stream = File.OpenRead(path))
        {
            return (TextFile)new BinaryFormatter().Deserialize(stream);
        }
    }

    public TextFile LoadXml(string fileName)
    {
        string path = Path.Combine(_workspacePath, "xml", $"{fileName}.xml");
        var serializer = new XmlSerializer(typeof(TextFile));
        using (var reader = new StreamReader(path))
        {
            return (TextFile)serializer.Deserialize(reader);
        }
    }

    public List<string> GetSavedFiles()
    {
        var files = new List<string>();
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "binary"), "*.bin"));
        files.AddRange(Directory.GetFiles(Path.Combine(_workspacePath, "xml"), "*.xml"));
        return files;
    }
}

public class TextEditor
{
    private readonly FileManager _fileManager = new FileManager();
    private TextFile _currentFile;
    private readonly Stack<TextFileMemento> _history = new Stack<TextFileMemento>();

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            ShowMenu();
            HandleChoice();
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("=== ТЕКСТОВЫЙ РЕДАКТОР ===");
        Console.WriteLine("1. Создать файл");
        Console.WriteLine("2. Открыть файл");
        Console.WriteLine("3. Редактировать файл");
        Console.WriteLine("4. Сохранить файл (бинарный)");
        Console.WriteLine("5. Сохранить файл (XML)");
        Console.WriteLine("6. Откатить изменения");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    private void HandleChoice()
    {
        switch (Console.ReadLine())
        {
            case "1": CreateFile(); break;
            case "2": OpenFile(); break;
            case "3": EditFile(); break;
            case "4": SaveFile(binary: true); break;
            case "5": SaveFile(binary: false); break;
            case "6": Undo(); break;
            case "0": Environment.Exit(0); break;
            default: Console.WriteLine("Неверный ввод"); break;
        }
        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }

    private void CreateFile()
    {
        Console.Write("Имя файла: ");
        string name = Console.ReadLine();
        Console.Write("Содержимое: ");
        string content = Console.ReadLine();
        
        _currentFile = new TextFile(name, content);
        SaveState();
        Console.WriteLine("Файл создан");
    }

    private void OpenFile()
    {
        var files = _fileManager.GetSavedFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("Нет сохраненных файлов");
            return;
        }

        for (int i = 0; i < files.Count; i++)
        {
            Console.WriteLine($"{i+1}. {Path.GetFileName(files[i])}");
        }

        Console.Write("Выберите файл: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= files.Count)
        {
            string filePath = files[index-1];
            _currentFile = filePath.EndsWith(".bin") 
                ? _fileManager.LoadBinary(Path.GetFileNameWithoutExtension(filePath))
                : _fileManager.LoadXml(Path.GetFileNameWithoutExtension(filePath));
            
            SaveState();
            Console.WriteLine($"Файл открыт: {_currentFile.Content}");
        }
    }

    private void EditFile()
    {
        if (_currentFile == null)
        {
            Console.WriteLine("Нет открытого файла");
            return;
        }

        Console.WriteLine($"Текущее содержимое: {_currentFile.Content}");
        Console.Write("Новое содержимое: ");
        _currentFile.Content = Console.ReadLine();
        _currentFile.IsSaved = false;
        SaveState();
    }

    private void SaveFile(bool binary)
    {
        if (_currentFile == null)
        {
            Console.WriteLine("Нет открытого файла");
            return;
        }

        if (binary)
            _fileManager.SaveBinary(_currentFile);
        else
            _fileManager.SaveXml(_currentFile);
        
        Console.WriteLine($"Файл сохранен в {(binary ? "бинарном" : "XML")} формате");
    }

    private void Undo()
    {
        if (_history.Count <= 1 || _currentFile == null)
        {
            Console.WriteLine("Невозможно откатить изменения");
            return;
        }

        // Удаляем текущее состояние
        _history.Pop();
        
        // Восстанавливаем предыдущее
        var previousState = _history.Peek();
        _currentFile.Content = previousState.Content;
        _currentFile.IsSaved = false;
        
        Console.WriteLine($"Изменения отменены (состояние на {previousState.Timestamp:HH:mm:ss})");
        Console.WriteLine($"Текущее содержимое: {_currentFile.Content}");
    }

    private void SaveState()
    {
        if (_currentFile != null)
        {
            _history.Push(new TextFileMemento(_currentFile.Content));
        }
    }
}

class Program
{
    static void Main()
    {
        new TextEditor().Run();
    }
}
