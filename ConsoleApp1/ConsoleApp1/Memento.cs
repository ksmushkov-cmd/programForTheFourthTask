using System;

namespace TextEditorApp {
  public class TextMemento {
    public string Content { get; private set; }
    public DateTime Timestamp { get; private set; }

    public TextMemento(string content) {
      Content = content;
      Timestamp = DateTime.Now;
    }

    public void Print() {
      string preview = Content.Length > 30 ? Content.Substring(0, 30) + "..." : Content;
      Console.WriteLine($"[{Timestamp:HH:mm:ss}] {preview}");
    }
  }
}
