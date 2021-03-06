﻿using System.Collections.Specialized;

using Fluent;

using Logger.Common.Collections.Generic;
using Logger.Common.Prism;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Common.FluentRibbon
{
    public sealed class RibbonRegionAdapter : RegionAdapterBase<Ribbon>
    {
        #region Instance Constructor/Destructor

        public RibbonRegionAdapter (IRegionBehaviorFactory factory)
                : base(factory)
        {
        }

        #endregion




        #region Instance Methods

        private void OnViewsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e, IRegion region, Ribbon regionTarget)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is RibbonTabItem)
                    {
                        RibbonTabItem ribbonTabItem = item as RibbonTabItem;
                        regionTarget.Tabs.Add(ribbonTabItem);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    if (item is RibbonTabItem)
                    {
                        RibbonTabItem ribbonTabItem = item as RibbonTabItem;
                        regionTarget.Tabs.Remove(ribbonTabItem);
                    }
                }
            }

            regionTarget.Tabs.Sort(ViewSortHintComparer<RibbonTabItem>.InvariantCultureIgnoreCase);
        }

        #endregion




        #region Overrides

        protected override void Adapt (IRegion region, Ribbon regionTarget)
        {
            region.Views.CollectionChanged += (sender, e) => this.OnViewsCollectionChanged(sender, e, region, regionTarget);
        }

        protected override IRegion CreateRegion ()
        {
            return new AllActiveRegion();
        }

        #endregion
    }
}
