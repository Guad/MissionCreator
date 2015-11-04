using System.Collections.Generic;
using RAGENativeUI;

namespace ContentCreator.Editor.NestedMenus
{
    public interface INestedMenu
    {
        void Process();
        List<UIMenu> Children { get; set; }
    }
}