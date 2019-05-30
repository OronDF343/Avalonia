using System;

namespace Avalonia.Controls.Repeaters
{
    internal class ViewportManager
    {
        private readonly ItemsRepeater _owner;

        public ViewportManager(ItemsRepeater owner)
        {
            _owner = owner;
        }

        public IControl SuggestedAnchor { get; }
        public double HorizontalCacheLength { get; set; }
        public double VerticalCacheLength { get; set; }
        public Rect GetLayoutVisibleWindow() => throw new NotImplementedException();
        public Rect GetLayoutRealizationWindow() => throw new NotImplementedException();
        public void SetLayoutExtent(Rect extent) => throw new NotImplementedException();
        public Point GetOrigin() => throw new NotImplementedException();
        public void OnLayoutChanged(bool isVirtualizing) => throw new NotImplementedException();
        public void OnElementPrepared(IControl element) => throw new NotImplementedException();
        public void OnElementCleared(IControl element) => throw new NotImplementedException();
        public void OnOwnerMeasuring() => throw new NotImplementedException();
        public void OnOwnerArranged() => throw new NotImplementedException();
        public void OnMakeAnchor(IControl anchor, bool isAnchorOutsideRealizedRange) => throw new NotImplementedException();
        public void OnBringIntoViewRequested(RequestBringIntoViewEventArgs args) => throw new NotImplementedException();
        public void ResetScrollers() => throw new NotImplementedException();
        public IControl MadeAnchor { get; }
    };
}
