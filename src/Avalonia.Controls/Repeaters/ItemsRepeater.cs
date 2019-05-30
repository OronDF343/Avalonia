using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;

namespace Avalonia.Controls.Repeaters
{
    public class ItemsRepeater : Control
    {
        public static readonly AvaloniaProperty<IBrush> BackgroundProperty =
            Border.BackgroundProperty.AddOwner<ItemsRepeater>();
        public static readonly AvaloniaProperty<double> HorizontalCacheLengthProperty =
            AvaloniaProperty.Register<ItemsRepeater, double>(nameof(HorizontalCacheLength), 2.0);
        public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
            AvaloniaProperty.Register<ItemsControl, IDataTemplate>(nameof(ItemTemplate));
        public static readonly DirectProperty<ItemsControl, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<ItemsControl, IEnumerable>(nameof(Items), o => o.Items, (o, v) => o.Items = v);
        public static readonly AvaloniaProperty LayoutProperty;
        public static readonly AvaloniaProperty<double> VerticalCacheLengthProperty =
            AvaloniaProperty.Register<ItemsRepeater, double>(nameof(VerticalCacheLength), 2.0);
        private static readonly AttachedProperty<VirtualizationInfo> VirtualizationInfoProperty =
            AvaloniaProperty.RegisterAttached<ItemsRepeater, IControl, VirtualizationInfo>("VirtualizationInfo");

        private readonly Controls _children = new Controls();
        private bool _isLayoutInProgress;
        private LayoutContext _layoutContext;
        private object _layoutState;
        private NotifyCollectionChangedEventArgs _processingItemsSourceChange;
        private ViewManager _viewManager;
        private ViewportManager _viewportManager;

        public ItemsRepeater()
        {
            _viewportManager = new ViewportManager(this);
            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Once);
            OnLayoutChanged(null, Layout);
        }

        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public Layout Layout { get; set; }
        public IEnumerable Items { get; set; }

        public IDataTemplate ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public double HorizontalCacheLength
        {
            get => GetValue(HorizontalCacheLengthProperty);
            set => SetValue(HorizontalCacheLengthProperty, value);
        }

        public double VerticalCacheLength
        {
            get => GetValue(VerticalCacheLengthProperty);
            set => SetValue(VerticalCacheLengthProperty, value);
        }

        public ItemsSourceView ItemsSourceView { get; }

        private LayoutContext LayoutContext
        {
            get
            {
                if (_layoutContext == null)
                {
                    _layoutContext = new RepeaterLayoutContext(this);
                }

                return _layoutContext;
            }
        }

        public int GetElementIndex(IControl element)
        {
            throw new NotImplementedException();
        }

        public IControl TryGetElement(int index)
        {
            throw new NotImplementedException();
        }

        public IControl GetOrCreateElement(int index)
        {
            throw new NotImplementedException();
        }

        internal static VirtualizationInfo GetVirtualizationInfo(IControl element)
        {
            var result = element.GetValue(VirtualizationInfoProperty);

            if (result == null)
            {
                result = new VirtualizationInfo();
                element.SetValue(VirtualizationInfoProperty, result);
            }

            return result;
        }

        private void ClearElementImpl(IControl element)
        {
            // Clearing an element due to a collection change
            // is more strict in that pinned elements will be forcibly
            // unpinned and sent back to the view generator.
            var isClearedDueToCollectionChange =
                _processingItemsSourceChange != null &&
                (_processingItemsSourceChange.Action == NotifyCollectionChangedAction.Remove ||
                    _processingItemsSourceChange.Action == NotifyCollectionChangedAction.Replace ||
                    _processingItemsSourceChange.Action == NotifyCollectionChangedAction.Reset);

            _viewManager.ClearElement(element, isClearedDueToCollectionChange);
            _viewportManager.OnElementCleared(element);
        }

        private void OnLayoutChanged(Layout oldValue, Layout newValue)
        {
            if (_isLayoutInProgress)
            {
                throw new InvalidOperationException("Layout cannot be changed during layout.");
            }

            _viewManager.OnLayoutChanging();

            if (oldValue != null)
            {
                oldValue.UninitializeForContext(LayoutContext);
                oldValue.MeasureInvalidated -= InvalidateMeasureForLayout;
                oldValue.ArrangeInvalidated -= InvalidateArrangeForLayout;
        
                // Walk through all the elements and make sure they are cleared
                for (var i = 0; i < _children.Count; ++i)
                {
                    var element = _children[i];
                    if (GetVirtualizationInfo(element).IsRealized)
                    {
                        ClearElementImpl(element);
                    }
                }

                _layoutState = null;
            }

            if (newValue != null)
            {
                newValue.InitializeForContext(LayoutContext);
                newValue.MeasureInvalidated += InvalidateMeasureForLayout;
                newValue.ArrangeInvalidated += InvalidateArrangeForLayout;
            }

            bool isVirtualizingLayout = newValue != null && newValue is VirtualizingLayout;
            _viewportManager.OnLayoutChanged(isVirtualizingLayout);
            InvalidateMeasure();
        }

        private void InvalidateArrangeForLayout(object sender, EventArgs e) => InvalidateMeasure();

        private void InvalidateMeasureForLayout(object sender, EventArgs e) => InvalidateArrange();

        //public event EventHandler<ItemsRepeaterElementClearingEventArgs> ElementClearing;
        //public event EventHandler<ItemsRepeaterElementIndexChangedEventArgs> ElementIndexChanged;
        //public event EventHandler<ItemsRepeaterElementPreparedEventArgs> ElementPrepared;
    }
}
