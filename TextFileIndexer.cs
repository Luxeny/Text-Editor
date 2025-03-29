using System;
using System.Collections.Generic;
using System.Linq;

public class TextFileIndexer
{
    private readonly FileManager _fileManager;
    private readonly Dictionary<string, HashSet<string>> _keywordIndex;

    public TextFileIndexer(FileManager fileManager)
    {
        _fileManager = fileManager;
        _keywordIndex = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
    }

    public void BuildIndex()
    {
        _keywordIndex.Clear();
        foreach (var file in _fileManager.GetAllFiles())
        {
            TextFile textFile = file.EndsWith(".bin") 
                ? _fileManager.LoadBinary(file) 
                : _fileManager.LoadXml(file);
    
            if (textFile == null || string.IsNullOrEmpty(textFile.Content))
                continue;
    
            var words = textFile.Content.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (!_keywordIndex.ContainsKey(word))
                {
                    _keywordIndex[word] = new HashSet<string>();
                }
                _keywordIndex[word].Add(file);
            }
        }
    }

    public List<string> SearchIndex(List<string> keywords)
    {
        var result = new HashSet<string>();
        foreach (var keyword in keywords)
        {
            if (_keywordIndex.TryGetValue(keyword, out var files))
            {
                result.UnionWith(files);
            }
        }
        return result.ToList();
    }
}
