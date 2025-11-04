using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Controls.Interfaces
{
    public interface ITreeModel
    {
        string DisplayName { get; }
        bool HasChildren();
    }
}
