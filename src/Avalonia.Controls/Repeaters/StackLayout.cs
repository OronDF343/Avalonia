using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    public class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        public static readonly AvaloniaProperty<Orientation> OrientationProperty
            = StackPanel.OrientationProperty.AddOwner<StackLayout>();

        public static readonly AvaloniaProperty<double> SpacingProperty
            = StackPanel.SpacingProperty.AddOwner<StackLayout>();

        private readonly OrientationBasedMeasures _orientation = new OrientationBasedMeasures();
        private double _itemSpacing;

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public double Spacing
        {
            get => GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            var state = context.LayoutState;
            var stackState = state as StackLayoutState;
            
            if (stackState == null)
            {
                if (state != null)
                {
                    throw new InvalidOperationException("LayoutState must derive from StackLayoutState.");
                }

                // Custom deriving layouts could potentially be stateful.
                // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
                stackState = new StackLayoutState();
            }

            stackState.InitializeForContext(context, this);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            var stackState = (StackLayoutState)context.LayoutState;
            stackState.UninitializeForContext(context);
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            var desiredSize = GetFlowAlgorithm(context).Measure(
                availableSize,
                context,
                false,
                0,
                _itemSpacing,
                _orientation.ScrollOrientation,
                LayoutId);

            return new Size(desiredSize.Width, desiredSize.Height);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            var value = GetFlowAlgorithm(context).Arrange(
               finalSize,
               context,
               FlowLayoutAlgorithm.LineAlignment.Start,
               LayoutId);

            ((StackLayoutState)context.LayoutState).OnArrangeLayoutEnd();

            return new Size(value.Width, value.Height);
        }

        protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
            // Always invalidate layout to keep the view accurate.
            InvalidateLayout();
        }

        private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context)
        {
            return ((StackLayoutState)context.LayoutState).FlowAlgorithm;
        }
    }
}
