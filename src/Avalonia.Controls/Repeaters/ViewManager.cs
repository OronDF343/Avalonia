using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    internal sealed class ViewManager
    {
        private readonly ItemsRepeater _owner;
        ElementFactoryRecycleArgs _elementFactoryRecycleArgs;
        UniqueIdElementPool m_resetPool;
        bool _isDataSourceStableResetPending;

        public ViewManager(ItemsRepeater owner)
        {
            _owner = owner;
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
            MUX_ASSERT((m_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault) ||
                (m_firstRealizedElementIndexHeldByLayout != FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout != LastRealizedElementIndexDefault));

            if (index == m_firstRealizedElementIndexHeldByLayout && index == m_lastRealizedElementIndexHeldByLayout)
            {
                // First and last were pointing to the same element and that is going away.
                InvalidateRealizedIndicesHeldByLayout();
            }
            else if (index == m_firstRealizedElementIndexHeldByLayout)
            {
                // The FirstElement is going away, shrink the range by one.
                ++m_firstRealizedElementIndexHeldByLayout;
            }
            else if (index == m_lastRealizedElementIndexHeldByLayout)
            {
                // Last element is going away, shrink the range by one at the end.
                --m_lastRealizedElementIndexHeldByLayout;
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

            auto context = m_ElementFactoryRecycleArgs.get();
            context.Element(element);
            context.Parent(*m_owner);

            m_owner->ItemTemplateShim().RecycleElement(context);

            context.Element(nullptr);
            context.Parent(nullptr);

            virtInfo->MoveOwnershipToElementFactory();
            m_phaser.StopPhasing(element, virtInfo);
            if (m_lastFocusedElement == element)
            {
                // Focused element is going away. Remove the tracked last focused element
                // and pick a reasonable next focus if we can find one within the layout 
                // realized elements.
                MoveFocusFromClearedIndex(clearedIndex);
            }

            REPEATER_TRACE_PERF(L"ElementCleared");

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
                m_resetPool.Add(element);
                virtInfo->MoveOwnershipToUniqueIdResetPoolFromLayout();
            }

            return m_isDataSourceStableResetPending;
        }

        private bool ClearElementToPinnedPool(IControl element, VirtualizationInfo virtInfo, bool isClearedDueToCollectionChange)
        {
            if (_isDataSourceStableResetPending)
            {
                m_resetPool.Add(element);
                virtInfo->MoveOwnershipToUniqueIdResetPoolFromLayout();
            }

            return m_isDataSourceStableResetPending;
        }
    }
}
