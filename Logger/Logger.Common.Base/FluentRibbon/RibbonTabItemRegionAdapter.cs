using System.Collections.Specialized;

using Fluent;

using Logger.Common.Base.Collections.Generic;
using Logger.Common.Base.Prism;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Common.Base.FluentRibbon
{
    public sealed class RibbonTabItemRegionAdapter : RegionAdapterBase<RibbonTabItem>
    {
        #region Instance Constructor/Destructor

        public RibbonTabItemRegionAdapter (IRegionBehaviorFactory factory)
                : base(factory)
        {
        }

        #endregion




        #region Instance Methods

        private void OnViewsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e, IRegion region, RibbonTabItem regionTarget)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is RibbonGroupBox)
                    {
                        RibbonGroupBox ribbonGroupBox = item as RibbonGroupBox;
                        regionTarget.Groups.Add(ribbonGroupBox);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    if (item is RibbonGroupBox)
                    {
                        RibbonGroupBox ribbonGroupBox = item as RibbonGroupBox;
                        regionTarget.Groups.Remove(ribbonGroupBox);
                    }
                }
            }

            regionTarget.Groups.Sort(ViewSortHintComparer<RibbonGroupBox>.InvariantCultureIgnoreCase);
        }

        #endregion




        #region Overrides

        protected override void Adapt (IRegion region, RibbonTabItem regionTarget)
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
