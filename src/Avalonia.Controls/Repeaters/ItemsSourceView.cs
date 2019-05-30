using System;
using System.Collections.Specialized;

namespace Avalonia.Controls.Repeaters
{
    public class ItemsSourceView : INotifyCollectionChanged
    {
        public ItemsSourceView(object source)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }

        public bool HasKeyIndexMapping { get; }


        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public object GetAt(int index)
        {
            throw new NotImplementedException();
        }

        public string KeyFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public int IndexFromKey(string key)
        {
            throw new NotImplementedException();
        }
    }
}
