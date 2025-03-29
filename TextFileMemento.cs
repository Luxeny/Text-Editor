using System;

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