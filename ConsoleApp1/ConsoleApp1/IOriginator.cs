using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditorApp {
  public interface IOriginator {
    object CreateMemento();
    void RestoreFromMemento(object memento);

  }
}
