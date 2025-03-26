using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

// Класс для текстового файла с поддержкой сериализации
[Serializable]
public class TextFile
{
  public string Name { get; set; }
  public string Content { get; set; }
  public bool IsSaved { get; set; }
  public string FilePath { get; set; }

  public TextFile(string name, string content)
  {
    Name = name;
    Content = content;
    IsSaved = false;
    FilePath = string.Empty;
  }

  public TextFile() { }
}

// Memento для отката изменений
public class TextFileMemento
{
  public string Content { get; }
  public DateTime Date { get; }

  public TextFileMemento(string content)
  {
    Content = content;
    Date = DateTime.Now;
  }
}

// Класс для работы с файлами
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
      TextFile tf;
      if (file.EndsWith(".bin"))
        tf = LoadBinary(file);
      else
        tf = LoadXml(file);

      foreach (var keyword in keywords)
      {
        if (tf.Content.Contains(keyword))
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

// Редактор текстовых файлов
public class TextEditor
{
  private readonly FileManager _fileManager;
  private TextFile _currentFile;
  private readonly Stack<TextFileMemento> _history = new Stack<TextFileMemento>();

  public TextEditor()
  {
    _fileManager = new FileManager();
  }

  public void CreateFile()
  {
    Console.Write("Введите имя файла: ");
    string name = Console.ReadLine();
    Console.Write("Введите содержимое: ");
    string content = Console.ReadLine();

    _currentFile = new TextFile(name, content);
    SaveState();
    Console.WriteLine($"Файл '{name}' создан (не сохранен)");
  }

  public void OpenFile()
  {
    var files = _fileManager.GetAllFiles();
    if (files.Count == 0)
    {
      Console.WriteLine("Нет доступных файлов.");
      return;
    }

    Console.WriteLine("Доступные файлы:");
    for (int i = 0; i < files.Count; i++)
    {
      Console.WriteLine($"{i + 1}. {files[i]}");
    }

    Console.Write("Выберите файл: ");
    if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= files.Count)
    {
      string filePath = files[index - 1];
      _currentFile = filePath.EndsWith(".bin") ?
        _fileManager.LoadBinary(filePath) :
        _fileManager.LoadXml(filePath);
      SaveState();
      Console.WriteLine($"Файл открыт:\n{_currentFile.Content}");
    }
    else
    {
      Console.WriteLine("Неверный выбор.");
    }
  }

  public void EditFile()
  {
    if (_currentFile == null)
    {
      Console.WriteLine("Нет открытого файла.");
      return;
    }

    Console.WriteLine($"Текущее содержимое:\n{_currentFile.Content}");
    Console.Write("Введите новое содержимое: ");
    _currentFile.Content = Console.ReadLine();
    _currentFile.IsSaved = false;
    SaveState();
    Console.WriteLine("Изменения сохранены в памяти (не в файле)");
  }

  public void SaveFile()
  {
    if (_currentFile == null)
    {
      Console.WriteLine("Нет открытого файла.");
      return;
    }

    Console.WriteLine("Выберите формат сохранения:");
    Console.WriteLine("1. Бинарный");
    Console.WriteLine("2. XML");
    Console.Write("Ваш выбор: ");

    if (int.TryParse(Console.ReadLine(), out int choice))
    {
      switch (choice)
      {
        case 1:
          _fileManager.SaveBinary(_currentFile);
          break;
        case 2:
          _fileManager.SaveXml(_currentFile);
          break;
        default:
          Console.WriteLine("Неверный выбор.");
          return;
      }
      Console.WriteLine("Файл сохранен.");
    }
  }

  public void SearchFiles()
  {
    Console.Write("Введите ключевые слова через запятую: ");
    string input = Console.ReadLine();
    var keywords = new List<string>(input.Split(','));

    var results = _fileManager.SearchFiles(keywords);
    if (results.Count == 0)
    {
      Console.WriteLine("Файлы не найдены.");
      return;
    }

    Console.WriteLine("Найденные файлы:");
    foreach (var file in results)
    {
      Console.WriteLine(file);
    }
  }

  public void ListFiles()
  {
    var files = _fileManager.GetAllFiles();
    if (files.Count == 0)
    {
      Console.WriteLine("Нет сохраненных файлов.");
      return;
    }

    Console.WriteLine("Сохраненные файлы:");
    foreach (var file in files)
    {
      Console.WriteLine(file);
    }
  }

  public void Undo()
  {
    if (_history.Count <= 1 || _currentFile == null)
    {
      Console.WriteLine("Невозможно откатить изменения.");
      return;
    }

    _history.Pop(); // Удаляем текущее состояние
    var previousState = _history.Peek();
    _currentFile.Content = previousState.Content;
    Console.WriteLine("Изменения отменены. Текущее содержимое:");
    Console.WriteLine(_currentFile.Content);
  }

  private void SaveState()
  {
    if (_currentFile != null)
    {
      _history.Push(new TextFileMemento(_currentFile.Content));
    }
  }

  public void ShowMenu()
  {
    while (true)
    {
      Console.Clear();
      Console.WriteLine("---------------------------------");
      Console.WriteLine("=== РЕДАКТОР ТЕКСТОВЫХ ФАЙЛОВ ===");
      Console.WriteLine("---------------------------------");
      Console.WriteLine("\nМеню:\n1) Создать файл" +
        "\n2) Открыть файл" +
        "\n3) Редактировать файл" +
        "\n4) Сохранить файл" +
        "\n5) Найти файлы по ключевым словам" +
        "\n6) Просмотреть все файлы" +
        "\n7) Откатить изменения" +
        "\n0) Выход");
      Console.Write("Выберите действие: ");

      if (int.TryParse(Console.ReadLine(), out int choice))
      {
        switch (choice)
        {
          case 1: CreateFile(); break;
          case 2: OpenFile(); break;
          case 3: EditFile(); break;
          case 4: SaveFile(); break;
          case 5: SearchFiles(); break;
          case 6: ListFiles(); break;
          case 7: Undo(); break;
          case 0: return;
          default: Console.WriteLine("Неверный выбор."); break;
        }
      }
      Console.WriteLine("\nНажмите любую клавишу для продолжения...");
      Console.ReadKey();
    }
  }
}

class Program
{
  static void Main(string[] args)
  {
    TextEditor editor = new TextEditor();
    editor.ShowMenu();
  }
}
