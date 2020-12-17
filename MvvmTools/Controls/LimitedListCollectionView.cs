using System;
using System.Collections;
using System.Windows.Data;

namespace MvvmTools.Controls
{
    public class LimitedListCollectionView : CollectionView, IEnumerable
    {
        public int Limit { get; set; }

        public LimitedListCollectionView(IEnumerable list)
            : base(list)
        {
            Limit = int.MaxValue;
        }

        public override int Count => Math.Min(base.Count, Limit);

        public override bool MoveCurrentToLast()
        {
            return base.MoveCurrentToPosition(Count - 1);
        }

        public override bool MoveCurrentToNext()
        {
            if (base.CurrentPosition == Count - 1)
                return base.MoveCurrentToPosition(base.Count);
            return base.MoveCurrentToNext();
        }

        public override bool MoveCurrentToPrevious()
        {
            if (base.IsCurrentAfterLast)
                return base.MoveCurrentToPosition(Count - 1);
            return base.MoveCurrentToPrevious();
        }

        public override bool MoveCurrentToPosition(int position)
        {
            if (position < Count)
                return base.MoveCurrentToPosition(position);
            return base.MoveCurrentToPosition(base.Count);
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            do
            {
                yield return CurrentItem;
            } while (MoveCurrentToNext());
        }

        #endregion
    }
}
