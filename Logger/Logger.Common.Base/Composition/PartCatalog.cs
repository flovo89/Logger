using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;




namespace Logger.Common.Base.Composition
{
    public sealed class PartCatalog : ComposablePartCatalog
    {
        #region Instance Constructor/Destructor

        public PartCatalog (IEnumerable<ComposablePartDefinition> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            this.PartsInternal = parts;
        }

        #endregion




        #region Instance Properties/Indexer

        private IEnumerable<ComposablePartDefinition> PartsInternal { get; }

        #endregion




        #region Overrides

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return this.PartsInternal.AsQueryable();
            }
        }

        #endregion
    }
}
