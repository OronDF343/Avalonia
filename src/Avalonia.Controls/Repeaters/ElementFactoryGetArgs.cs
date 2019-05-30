using System;

namespace Avalonia.Controls.Repeaters
{
    public sealed class ElementFactoryGetArgs : EventArgs
    {
        public object Data { get; set; }
        public IControl Parent { get; set; } 
    }
}
