using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    public abstract class VirtualizingLayout : Layout
    {
        protected virtual void InitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected virtual void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected abstract Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize);

        protected virtual Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize) => finalSize;

        protected virtual void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args) => InvalidateMeasure();
    }
}
