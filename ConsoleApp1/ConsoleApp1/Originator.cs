using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TextEditorApp {

  [Serializable]
  public class TextFileOriginator : IOriginator {
    public string FileName { get; set; }
    public string Content { get; set; }
    public string FilePath { get; set; }
    public DateTime LastModified { get; set; }

    public TextFileOriginator() { }

    public TextFileOriginator(string path) {
      FilePath = path;
      FileName = Path.GetFileName(path);

      if (File.Exists(path)) {
        Content = File.ReadAllText(path);
        LastModified = File.GetLastWriteTime(path);
      } else {
        Content = string.Empty;
        LastModified = DateTime.Now;
      }
    }

    public void Save() {
      File.WriteAllText(FilePath, Content);
      LastModified = DateTime.Now;
    }
    
    /// Create a snapshot of the current state (Memento)
    public object CreateMemento() {
      return new TextMemento(Content ?? "");
    }
    
    /// Restore state from snapshot (Memento)
    public void RestoreFromMemento(object memento) {
      if (memento is TextMemento textMemento) {
        Content = textMemento.Content;
      }
    }

    public void BinarySerialize(string savePath) {
      using (FileStream fs = new FileStream(savePath, FileMode.Create)) {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fs, this);
      }
    }

    public static TextFileOriginator BinaryDeserialize(string loadPath) {
      using (FileStream fs = new FileStream(loadPath, FileMode.Open)) {
        BinaryFormatter formatter = new BinaryFormatter();
        return (TextFileOriginator)formatter.Deserialize(fs);
      }
    }

    public void XmlSerialize(string savePath)  {
      XmlSerializer serializer = new XmlSerializer(typeof(TextFileOriginator));
      using (StreamWriter writer = new StreamWriter(savePath)) {
        serializer.Serialize(writer, this);
      }
    }

    public static TextFileOriginator XmlDeserialize(string loadPath) {
      XmlSerializer serializer = new XmlSerializer(typeof(TextFileOriginator));
      using (StreamReader reader = new StreamReader(loadPath)) {
        return (TextFileOriginator)serializer.Deserialize(reader);
      }
    }

    public override string ToString() {
      return $"{FileName} (changed:{LastModified})";
    }
  }
}
