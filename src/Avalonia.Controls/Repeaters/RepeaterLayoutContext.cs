using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    internal class RepeaterLayoutContext : LayoutContext
    {
        private readonly ItemsRepeater _owner;

        public RepeaterLayoutContext(ItemsRepeater owner)
        {
            _owner = owner;
        }
    }
}
