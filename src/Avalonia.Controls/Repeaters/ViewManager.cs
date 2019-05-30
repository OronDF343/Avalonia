using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Repeaters
{
    internal sealed class ViewManager
    {
        private const int FirstRealizedElementIndexDefault = int.MaxValue;
        private const int LastRealizedElementIndexDefault = int.MinValue;

        private readonly ItemsRepeater _owner;
        private readonly List<PinnedElementInfo> _pinnedPool = new List<PinnedElementInfo>();
        private readonly UniqueIdElementPool _resetPool = new UniqueIdElementPool();
        private IControl _lastFocusedElement;
        private bool _isDataSourceStableResetPending;
        private ElementFactoryGetArgs m_elementFactoryGetArgs;
        private ElementFactoryRecycleArgs _elementFactoryRecycleArgs;
        private int _firstRealizedElementIndexHeldByLayout = FirstRealizedElementIndexDefault;
        private int _lastRealizedElementIndexHeldByLayout = LastRealizedElementIndexDefault;

        public ViewManager(ItemsRepeater owner)
        {
            _owner = owner;
        }

        public IControl GetElement(int index, bool forceCreate, bool suppressAutoRecycle)
        {
            var element = forceCreate ? null : GetElementIfAlreadyHeldByLayout(index);
            if (!element)
            {
                // check if this is the anchor made through repeater in preparation 
                // for a bring into view.
                var madeAnchor = _owner.MadeAnchor();
                if (madeAnchor != null)
                {
                    var anchorVirtInfo = ItemsRepeater.TryGetVirtualizationInfo(madeAnchor);
                    if (anchorVirtInfo->Index() == index)
                    {
                        element = madeAnchor;
                    }
                }
            }
            if (!element) { element = GetElementFromUniqueIdResetPool(index); };
            if (!element) { element = GetElementFromPinnedElements(index); }
            if (!element) { element = GetElementFromElementFactory(index); }

            var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(element);
            if (suppressAutoRecycle)
            {
                virtInfo->AutoRecycleCandidate(false);
            }
            else
            {
                virtInfo->AutoRecycleCandidate(true);
                virtInfo->KeepAlive(true);
            }

            return element;
        }

        public void ClearElement(IControl element, bool isClearedDueToCollectionChange)
        {
            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
            var index = virtInfo.Index;
            bool cleared =
                ClearElementToUniqueIdResetPool(element, virtInfo) ||
                ClearElementToPinnedPool(element, virtInfo, isClearedDueToCollectionChange);

            if (!cleared)
            {
                ClearElementToElementFactory(element);
            }

            // Both First and Last indices need to be valid or default.
            if (index == _firstRealizedElementIndexHeldByLayout && index == _lastRealizedElementIndexHeldByLayout)
            {
                // First and last were pointing to the same element and that is going away.
                InvalidateRealizedIndicesHeldByLayout();
            }
            else if (index == _firstRealizedElementIndexHeldByLayout)
            {
                // The FirstElement is going away, shrink the range by one.
                ++_firstRealizedElementIndexHeldByLayout;
            }
            else if (index == _lastRealizedElementIndexHeldByLayout)
            {
                // Last element is going away, shrink the range by one at the end.
                --_lastRealizedElementIndexHeldByLayout;
            }
            else
            {
                // Index is either outside the range we are keeping track of or inside the range.
                // In both these cases, we just keep the range we have. If this clear was due to 
                // a collection change, then in the CollectionChanged event, we will invalidate these guys.
            }
        }

        public void ClearElementToElementFactory(IControl element)
        {
            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
            var clearedIndex = virtInfo.Index;
            _owner.OnElementClearing(element);

            if (_elementFactoryRecycleArgs == null)
            {
                // Create one.
                _elementFactoryRecycleArgs = new ElementFactoryRecycleArgs();
            }

            var context = _elementFactoryRecycleArgs;
            context.Element = element;
            context.Parent = _owner;

            _owner.ItemTemplateShim().RecycleElement(context);

            context.Element = null;
            context.Parent = null;

            virtInfo.MoveOwnershipToElementFactory();
            _phaser.StopPhasing(element, virtInfo);
            if (_lastFocusedElement == element)
            {
                // Focused element is going away. Remove the tracked last focused element
                // and pick a reasonable next focus if we can find one within the layout 
                // realized elements.
                MoveFocusFromClearedIndex(clearedIndex);
            }

        }

        private void MoveFocusFromClearedIndex(int clearedIndex)
        {
            IControl focusedChild = null;
            var focusCandidate = FindFocusCandidate(clearedIndex, focusedChild);
            if (focusCandidate != null)
            {
                //var focusState = _lastFocusedElement?.FocusState ?? FocusState.Programmatic;

                // If the last focused element has focus, use its focus state, if not use programmatic.
                //focusState = focusState == FocusState.Unfocused ? FocusState.Programmatic : focusState;
                focusCandidate.Focus();

                _lastFocusedElement = focusedChild;
                // Add pin to hold the focused element.
                UpdatePin(focusedChild, true /* addPin */);
            }
            else
            {
                // We could not find a candiate.
                _lastFocusedElement = null;
            }
        }

        IControl FindFocusCandidate(int clearedIndex, IControl focusedChild)
        {
            // Walk through all the children and find elements with index before and after the cleared index.
            // Note that during a delete the next element would now have the same index.
            int previousIndex = int.MinValue;
            int nextIndex = int.MaxValue;
            IControl nextElement = null;
            IControl previousElement = null;
            var children = _owner.Children;

            for (var i = 0u; i < children.Count; ++i)
            {
                var child = children.GetAt(i);
                var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(child);
                if (virtInfo && virtInfo->IsHeldByLayout())
                {
                    const int currentIndex = virtInfo->Index();
                    if (currentIndex < clearedIndex)
                    {
                        if (currentIndex > previousIndex)
                        {
                            previousIndex = currentIndex;
                            previousElement = child;
                        }
                    }
                    else if (currentIndex >= clearedIndex)
                    {
                        // Note that we use >= above because if we deleted the focused element, 
                        // the next element would have the same index now.
                        if (currentIndex < nextIndex)
                        {
                            nextIndex = currentIndex;
                            nextElement = child;
                        }
                    }
                }
            }

            // Find the next element if one exists, if not use the previous element.
            // If the container itself is not focusable, find a descendent that is.
            IControl focusCandidate = null;
            if (nextElement != null)
            {
                focusCandidate = nextElement as IControl;
                if (focusCandidate != null)
                {
                    var firstFocus = FocusManager.FindFirstFocusableElement(nextElement);

                    if (firstFocus != null)
                    {
                        focusCandidate = firstFocus as IControl;
                    }
                }
            }

            if (focusCandidate == null && previousElement != null)
            {
                focusCandidate = previousElement as IControl;
                if (previousElement != null)
                {
                    var lastFocus = FocusManager.FindLastFocusableElement(previousElement);

                    if (lastFocus != null)
                    {
                        focusCandidate = lastFocus as IControl;
                    }
                }
            }

            return focusCandidate;
        }

        public int GetElementIndex(VirtualizationInfo virtInfo)
        {
            if (virtInfo == null)
            {
                throw new ArgumentException("Element is not a child of this ItemsRepeater.");
            }

            return virtInfo.IsRealized || virtInfo.IsInUniqueIdResetPool ? virtInfo.Index : -1;
        }

        public void PrunePinnedElements()
        {
            EnsureEventSubscriptions();

            // Go through pinned elements and make sure they still have
            // a reason to be pinned.
            for (var i = 0; i < _pinnedPool.Count; ++i)
            {
                var elementInfo = _pinnedPool[i];
                var virtInfo = elementInfo.VirtualizationInfo;

                //MUX_ASSERT(virtInfo->Owner() == ElementOwner.PinnedPool);

                if (!virtInfo.IsPinned)
                {
                    _pinnedPool.RemoveAt(i);
                    --i;

                    // Pinning was the only thing keeping this element alive.
                    ClearElementToElementFactory(elementInfo.PinnedElement);
                }
            }
        }

        public void UpdatePin(IControl element, bool addPin)
        {
            var parent = element.VisualParent;
            var child = (IVisual)element;

            while (parent != null)
            {
                if (parent is ItemsRepeater repeater)
                {
                    var virtInfo = ItemsRepeater.GetVirtualizationInfo((IControl)child);
                    if (virtInfo.IsRealized)
                    {
                        if (addPin)
                        {
                            virtInfo.AddPin();
                        }
                        else if (virtInfo.IsPinned)
                        {
                            if (virtInfo.RemovePin() == 0)
                            {
                                // ElementFactory is invoked during the measure pass.
                                // We will clear the element then.
                                repeater.InvalidateMeasure();
                            }
                        }
                    }
                }

                child = parent;
                parent = child.VisualParent;
            }
        }

        public void OnLayoutChanging()
        {
            if (_owner.ItemsSourceView?.HasKeyIndexMapping == true)
            {
                _isDataSourceStableResetPending = true;
            }
        }

        private bool ClearElementToUniqueIdResetPool(IControl element, VirtualizationInfo virtInfo)
        {
            if (_isDataSourceStableResetPending)
            {
                _resetPool.Add(element);
                virtInfo.MoveOwnershipToUniqueIdResetPoolFromLayout();
            }

            return _isDataSourceStableResetPending;
        }

        private bool ClearElementToPinnedPool(IControl element, VirtualizationInfo virtInfo, bool isClearedDueToCollectionChange)
        {
            if (_isDataSourceStableResetPending)
            {
                _resetPool.Add(element);
                virtInfo.MoveOwnershipToUniqueIdResetPoolFromLayout();
            }

            return _isDataSourceStableResetPending;
        }

        private struct PinnedElementInfo
        {
            public PinnedElementInfo(IControl element)
            {
                PinnedElement = element;
                VirtualizationInfo = ItemsRepeater.GetVirtualizationInfo(element);
            }

            public IControl PinnedElement { get; }
            public VirtualizationInfo VirtualizationInfo { get; }
        }
    }
}
