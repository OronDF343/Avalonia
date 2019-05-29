using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    internal class FlowLayoutAlgorithm : OrientationBasedMeasures
    {
        private ElementManager _elementManager = new ElementManager();
        private VirtualizingLayoutContext _context;
        private IFlowLayoutAlgorithmDelegates _algorithmCallbacks;

        public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
        {
            _algorithmCallbacks = callbacks;
            _context = context;
            _elementManager.SetContext(context);
        }

        public void UninitializeForContext(VirtualizingLayoutContext context)
        {
            if (IsVirtualizingContext())
            {
                // This layout is about to be detached. Let go of all elements
                // being held and remove the layout state from the context.
                _elementManager.ClearRealizedRange();
            }

            context.LayoutState = null;
        }

        private bool IsVirtualizingContext()
        {
            if (_context != null)
            {
                var rect = _context.RealizationRect;
                bool hasInfiniteSize = double.IsInfinity(rect.Height) || double.IsInfinity(rect.Width);
                return !hasInfiniteSize;
            }
            return false;
        }
    }
}
