using System;

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