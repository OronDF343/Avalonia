using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Repeaters
{
    public class StackLayoutState
    {
        private const int BufferSize = 100;
        private readonly FlowLayoutAlgorithm _flowAlgorithm = new FlowLayoutAlgorithm();
        private readonly List<double> _estimationBuffer = new List<double>();
        private double _totalElementSize;
        private double _maxArrangeBounds;
        private int _totalElementsMeasured;

        internal void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
        {
            _flowAlgorithm.InitializeForContext(context, callbacks);

            if (_estimationBuffer.Count == 0)
            {
                _estimationBuffer.AddRange(Enumerable.Repeat(0.0, BufferSize));
            }

            //context.LayoutStateCore(this);
        }

        internal void UninitializeForContext(VirtualizingLayoutContext context)
        {
            _flowAlgorithm.UninitializeForContext(context);
        }

        internal void OnElementMeasured(int elementIndex, double majorSize, double minorSize)
        {
            int estimationBufferIndex = elementIndex % _estimationBuffer.Count;
            bool alreadyMeasured = _estimationBuffer[estimationBufferIndex] != 0;

            if (!alreadyMeasured)
            {
                _totalElementsMeasured++;
            }

            _totalElementSize -= _estimationBuffer[estimationBufferIndex];
            _totalElementSize += majorSize;
            _estimationBuffer[estimationBufferIndex] = majorSize;

            _maxArrangeBounds = Math.Max(_maxArrangeBounds, minorSize);
        }

        internal void OnArrangeLayoutEnd() => _maxArrangeBounds = 0;
    }
}
