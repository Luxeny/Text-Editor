using System;
using System.Collections.Generic;

public class TextEditor
{
    private readonly FileManager _fileManager;
    private readonly TextFileIndexer _indexer;
    private TextFile _currentFile;
    private readonly Stack<TextFileMemento> _history = new Stack<TextFileMemento>();

    public TextEditor()
    {
        _fileManager = new FileManager();
        _fileManager.OnFileChanged += (path) => _indexer.BuildIndex();
        _indexer = new TextFileIndexer(_fileManager);
        _indexer.BuildIndex();
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
        for (int i = 0; i < files.Count; ++i)
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
                
            if (_currentFile != null)
            {
                SaveState();
                Console.WriteLine($"Файл открыт:\n{_currentFile.Content}");
            }
            else
            {
                Console.WriteLine("Не удалось загрузить файл.");
            }
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
            _indexer.BuildIndex(); 
            Console.WriteLine("Файл сохранен.");
        }
    }

    public void SearchFiles()
    {
        Console.Write("Введите ключевые слова через запятую: ");
        string input = Console.ReadLine();
        var keywords = new List<string>(input.Split(',').Select(k => k.Trim())); 
        var results = _indexer.SearchIndex(keywords); 
        
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
              "\n8) Обновить индекс поиска" +
              "\n0) Выход");
            Console.Write("Выберите действие: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        CreateFile(); 
                        break;
                    case 2: 
                        OpenFile(); 
                        break;
                    case 3: 
                        EditFile(); 
                        break;
                    case 4: 
                        SaveFile(); 
                        break;
                    case 5: 
                        SearchFiles(); 
                        break;
                    case 6: 
                        ListFiles(); 
                        break;
                    case 7: 
                        Undo(); 
                        break;
                    case 8:
                        _indexer.BuildIndex();
                        Console.WriteLine("Индекс поиска обновлён.");
                        break;
                    case 0: 
                        return;
                    default: 
                        Console.WriteLine("Неверный выбор."); 
                        break;
                }
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}
