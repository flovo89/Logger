using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;




namespace Logger.Common.Composition
{
    public class FilteredCatalog : ComposablePartCatalog,
            INotifyComposablePartCatalogChanged
    {
        #region Instance Constructor/Destructor

        public FilteredCatalog (ComposablePartCatalog catalog, Predicate<Tuple<ComposablePartDefinition, ExportDefinition>> filterPredicate)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (filterPredicate == null)
            {
                throw new ArgumentNullException(nameof(filterPredicate));
            }

            this.Catalog = catalog;
            this.FilterPredicate = filterPredicate;

            this.ChangedHandler = (sender, e) =>
            {
                EventHandler<ComposablePartCatalogChangeEventArgs> handler = this.Changed;
                if (handler != null)
                {
                    handler(this, e);
                }
            };

            this.ChangingHandler = (sender, e) =>
            {
                EventHandler<ComposablePartCatalogChangeEventArgs> handler = this.Changing;
                if (handler != null)
                {
                    handler(this, e);
                }
            };

            if (this.Catalog is INotifyComposablePartCatalogChanged)
            {
                ( (INotifyComposablePartCatalogChanged)this.Catalog ).Changed += this.ChangedHandler;
                ( (INotifyComposablePartCatalogChanged)this.Catalog ).Changing += this.ChangingHandler;
            }
        }

        #endregion




        #region Instance Properties/Indexer

        public ComposablePartCatalog Catalog { get; }

        public Predicate<Tuple<ComposablePartDefinition, ExportDefinition>> FilterPredicate { get; }

        private EventHandler<ComposablePartCatalogChangeEventArgs> ChangedHandler { get; }

        private EventHandler<ComposablePartCatalogChangeEventArgs> ChangingHandler { get; }

        #endregion




        #region Overrides

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return this.Catalog.Parts;
            }
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports (ImportDefinition definition)
        {
            IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> baseExports = base.GetExports(definition);
            IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> exports = from x in baseExports where this.FilterPredicate(x) select x;
            return exports;
        }

        protected override void Dispose (bool disposing)
        {
            if (this.Catalog is INotifyComposablePartCatalogChanged)
            {
                ( (INotifyComposablePartCatalogChanged)this.Catalog ).Changed -= this.ChangedHandler;
                ( (INotifyComposablePartCatalogChanged)this.Catalog ).Changing -= this.ChangingHandler;
            }

            this.Catalog.Dispose();

            base.Dispose(disposing);
        }

        #endregion




        #region Interface: INotifyComposablePartCatalogChanged

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        #endregion
    }
}
