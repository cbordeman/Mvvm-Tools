using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace MvvmTools.ViewModels
{
    public class CheckListUserControlViewModel<T> : BindableBase
    {
        private readonly string _allOrAnyText;

        public CheckListUserControlViewModel(IEnumerable<CheckedItemViewModel<T>> items, string allOrAnyText)
        {
            _allOrAnyText = allOrAnyText;

            if (Items != null)
            {
                Items.CollectionChanged -= ItemsOnCollectionChanged;
                foreach (INotifyPropertyChanged item in _items)
                    item.PropertyChanged -= ItemOnPropertyChanged;
            }
            if (items == null)
            {
                Items = new ObservableCollection<CheckedItemViewModel<T>>();
            }
            else
            {
                Items = new ObservableCollection<CheckedItemViewModel<T>>(items);
                Items.CollectionChanged += ItemsOnCollectionChanged;
                Resubscribe(null, _items);
            }
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Resubscribe(e.OldItems, e.NewItems);
        }

        private void Resubscribe(IList oldItems, IList newItems)
        {
            if (oldItems != null)
                foreach (INotifyPropertyChanged item in oldItems)
                    item.PropertyChanged -= ItemOnPropertyChanged;

            if (newItems != null)
                foreach (INotifyPropertyChanged item in newItems)
                    item.PropertyChanged += ItemOnPropertyChanged;
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckedItemViewModel<T>.IsChecked))
            {
                NotifyPropertyChanged(nameof(CheckedItems));
                NotifyPropertyChanged(nameof(CheckedItemsCommaSeparated));
            }
        }

        #region Items
        private ObservableCollection<CheckedItemViewModel<T>> _items;
        public ObservableCollection<CheckedItemViewModel<T>> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }
        #endregion Items

        #region CheckedItems
        public IReadOnlyCollection<T> CheckedItems
        {
            get { return new ReadOnlyCollection<T>(_items.Where(i => i.IsChecked).Select(i => i.Value).ToList()); }
        }
        #endregion CheckedItems

        public string CheckedItemsCommaSeparated => !CheckedItems.Any() || Items.All(i => i.IsChecked) ? _allOrAnyText : string.Join(", ", CheckedItems);
    }
}
