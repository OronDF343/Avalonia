// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Specialized;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using static Avalonia.Utilities.MathUtilities;

namespace Avalonia.Controls.Presenters
{
    /// <summary>
    /// Displays items inside an <see cref="ItemsControl"/>.
    /// </summary>
    public class ItemsPresenter : ItemsPresenterBase, ILogicalScrollable
    {
        /// <summary>
        /// Defines the <see cref="VirtualizationMode"/> property.
        /// </summary>
        public static readonly StyledProperty<ItemVirtualizationMode> VirtualizationModeProperty =
            AvaloniaProperty.Register<ItemsPresenter, ItemVirtualizationMode>(
                nameof(VirtualizationMode),
                defaultValue: ItemVirtualizationMode.None);

        private bool _canHorizontallyScroll;
        private bool _canVerticallyScroll;

        /// <summary>
        /// Initializes static members of the <see cref="ItemsPresenter"/> class.
        /// </summary>
        static ItemsPresenter()
        {
            KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
                typeof(ItemsPresenter),
                KeyboardNavigationMode.Once);
        }

        /// <summary>
        /// Gets or sets the virtualization mode for the items.
        /// </summary>
        public ItemVirtualizationMode VirtualizationMode
        {
            get { return GetValue(VirtualizationModeProperty); }
            set { SetValue(VirtualizationModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        bool ILogicalScrollable.CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set
            {
                _canHorizontallyScroll = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        bool ILogicalScrollable.CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set
            {
                _canVerticallyScroll = value;
                InvalidateMeasure();
            }
        }
        /// <inheritdoc/>
        bool ILogicalScrollable.IsLogicalScrollEnabled => false;

        /// <inheritdoc/>
        Size IScrollable.Extent => Size.Empty;

        /// <inheritdoc/>
        Vector IScrollable.Offset
        {
            get => new Vector();
            set { }
        }

        /// <inheritdoc/>
        Size IScrollable.Viewport => Size.Empty;

        /// <inheritdoc/>
        Action ILogicalScrollable.InvalidateScroll { get; set; }

        /// <inheritdoc/>
        Size ILogicalScrollable.ScrollSize => new Size(1, 1);

        /// <inheritdoc/>
        Size ILogicalScrollable.PageScrollSize => new Size(0, 1);

        /// <inheritdoc/>
        bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect)
        {
            return false;
        }

        /// <inheritdoc/>
        IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl from)
        {
            return null;
        }

        public override void ScrollIntoView(object item)
        {
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        /// <inheritdoc/>
        protected override void PanelCreated(IPanel panel)
        {
            KeyboardNavigation.SetTabNavigation(
                (InputElement)Panel,
                KeyboardNavigation.GetTabNavigation(this));
        }

        protected override void ItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.ItemsChanged(e);
        }
    }
}
