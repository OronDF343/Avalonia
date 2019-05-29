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

        public IControl SuggestedAnchor() => throw new NotImplementedException();
        public double HorizontalCacheLength() => throw new NotImplementedException();
        public void HorizontalCacheLength(double value) => throw new NotImplementedException();
        public double VerticalCacheLength() => throw new NotImplementedException();
        public void VerticalCacheLength(double value) => throw new NotImplementedException();
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
        public IControl MadeAnchor() => throw new NotImplementedException();
    };
}
