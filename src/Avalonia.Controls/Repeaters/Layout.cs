using System;

namespace Avalonia.Controls.Repeaters
{
    public class Layout : AvaloniaObject
    {
        public void InitializeForContext(LayoutContext context)
        {

        }

        public void UninitializeForContext(LayoutContext context)
        {

        }

        public Size Measure(LayoutContext context, Size availableSize)
        {
            throw new NotImplementedException();
        }

        public Size Arrange(LayoutContext context, Size finalSize)
        {
            throw new NotImplementedException();
        }

        protected void InvalidateMeasure()
        {

        }

        protected void InvalidateArrange()
        {

        }

        public event EventHandler ArrangeInvalidated;
        public event EventHandler MeasureInvalidated;
    }
}
