using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    public class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        public static readonly AvaloniaProperty<Orientation> OrientationProperty
            = StackPanel.OrientationProperty.AddOwner<StackLayout>();

        public static readonly AvaloniaProperty<double> SpacingProperty
            = StackPanel.SpacingProperty.AddOwner<StackLayout>();

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
    }
}
