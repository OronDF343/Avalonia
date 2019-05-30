using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    internal enum ElementOwner
    {
        // All elements are originally owned by the view generator.
        ElementFactory,
        // Ownership is transferred to the layout when it calls GetElement.
        Layout,
        // Ownership is transferred to the pinned pool if the element is cleared (outside of
        // a 'remove' collection change of course).
        PinnedPool,
        // Ownership is transfered to the reset pool if the element is cleared by a reset and
        // the data source supports unique ids.
        UniqueIdResetPool,
        // Ownership is transfered to the animator if the element is cleared due to a
        // 'remove'-like collection change.
        Animator
    }

    internal class VirtualizationInfo
    {
        private const int PhaseNotSpecified = int.MinValue;

        private uint _pinCounter;
        private string _uniqueId;
        private Rect _arrangeBounds;
        private int _phase = PhaseNotSpecified;
        private bool _keepAlive = false;
        private bool m_autoRecycleCandidate = false;
        private object _data;

        public Rect ArrangeBounds { get; set; }
        public bool AutoRecycleCandidate { get; set; }
        public int Index { get; }
        public bool IsPinned => _pinCounter > 0;
        public bool IsHeldByLayout => Owner == ElementOwner.Layout;
        public bool IsRealized => IsHeldByLayout || Owner == ElementOwner.PinnedPool;
        public bool IsInUniqueIdResetPool => Owner == ElementOwner.UniqueIdResetPool;
        public bool KeepAlive { get; set; }
        public ElementOwner Owner { get; private set; } = ElementOwner.ElementFactory;
}
}
