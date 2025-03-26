using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class TextFile
{
    public string Name { get; set; }
    public string Content { get; set; }

    public TextFile(string name, string content)
    {
        Name = name;
        Content = content;
    }
}

public class FileManager
{
    private readonly string _workspacePath = "./workspace/";

    public FileManager()
    {
        Directory.CreateDirectory(_workspacePath);
    }

    public void SaveBinary(TextFile file)
    {
        string path = Path.Combine(_workspacePath, $"{file.Name}.bin");
        using (FileStream stream = File.Create(path))
        {
            new BinaryFormatter().Serialize(stream, file);
        }
    }

    public TextFile LoadBinary(string fileName)
    {
        string path = Path.Combine(_workspacePath, $"{fileName}.bin");
        using (FileStream stream = File.OpenRead(path))
        {
            return (TextFile)new BinaryFormatter().Deserialize(stream);
        }
    }

    public List<string> GetSavedFiles()
    {
        return new List<string>(Directory.GetFiles(_workspacePath, "*.bin"));
    }
}

public class TextEditor
{
    private readonly FileManager _fileManager = new FileManager();
    private TextFile _currentFile;

    public void CreateFile()
    {
        Console.Write("Введите имя файла: ");
        string name = Console.ReadLine();
        Console.Write("Введите содержимое: ");
        string content = Console.ReadLine();

        _currentFile = new TextFile(name, content);
        Console.WriteLine($"Файл '{name}' создан");
    }

    public void SaveFile()
    {
        if (_currentFile == null)
        {
            Console.WriteLine("Нет открытого файла.");
            return;
        }

        _fileManager.SaveBinary(_currentFile);
        Console.WriteLine("Файл сохранен.");
    }

    public void OpenFile()
    {
        var files = _fileManager.GetSavedFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("Нет сохраненных файлов.");
            return;
        }

        Console.WriteLine("Сохраненные файлы:");
        for (int i = 0; i < files.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
        }

        Console.Write("Выберите файл: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= files.Count)
        {
            _currentFile = _fileManager.LoadBinary(Path.GetFileNameWithoutExtension(files[index - 1]));
            Console.WriteLine($"Файл открыт:\n{_currentFile.Content}");
        }
        else
        {
            Console.WriteLine("Неверный выбор.");
        }
    }

    public void ShowMenu()
    {
        while (true)
        {
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Создать файл");
            Console.WriteLine("2. Открыть файл");
            Console.WriteLine("3. Сохранить файл");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: CreateFile(); break;
                    case 2: OpenFile(); break;
                    case 3: SaveFile(); break;
                    case 0: return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }
            }
        }
    }
}

class Program
{
    static void Main()
    {
        new TextEditor().ShowMenu();
    }
}
