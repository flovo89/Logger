using System;
using System.Windows.Input;




namespace Logger.Common.Windows
{
    public sealed class TemporaryCursor : IDisposable
    {
        #region Static Methods

        public static TemporaryCursor Hourglass ()
        {
            return new TemporaryCursor(Cursors.Wait);
        }

        #endregion




        #region Instance Constructor/Destructor

        private TemporaryCursor (Cursor cursor)
        {
            this.PreviousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = cursor;
        }

        #endregion




        #region Instance Properties/Indexer

        private Cursor PreviousCursor { get; }

        #endregion




        #region Interface: IDisposable

        public void Dispose ()
        {
            Mouse.OverrideCursor = this.PreviousCursor;
        }

        #endregion
    }
}
