using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

using Microsoft.Practices.Prism.Regions;

using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;




namespace Logger.Common.AvalonDock
{
    public sealed class DockingManagerRegionAdapter : RegionAdapterBase<DockingManager>
    {
        #region Instance Constructor/Destructor

        public DockingManagerRegionAdapter (IRegionBehaviorFactory factory)
                : base(factory)
        {
            this.ContentLookupTable = new Dictionary<object, Tuple<LayoutDocument, IRegion>>();
            this.PaneViewInfoChangedHandler = this.PaneViewInfoChangedMethod;
            this.RequestBringIntoViewHandler = this.RequestBringIntoView;
            this.CloseHandler = this.CloseMethod;
            this.ActiveDocumentHandler = this.ActiveDocumentMethod;
        }

        #endregion




        #region Instance Properties/Indexer

        private EventHandler ActiveDocumentHandler { get; }

        private EventHandler<CancelEventArgs> CloseHandler { get; }

        private Dictionary<object, Tuple<LayoutDocument, IRegion>> ContentLookupTable { get; }

        private PropertyChangedEventHandler PaneViewInfoChangedHandler { get; }

        private RequestBringIntoViewEventHandler RequestBringIntoViewHandler { get; }

        #endregion




        #region Instance Methods

        private void ActiveDocumentMethod (object sender, EventArgs e)
        {
            LayoutDocument content = sender as LayoutDocument;
            object item = content.Content;

            if (item is IPaneViewInfo)
            {
                IPaneViewInfo paneViewInfo = item as IPaneViewInfo;
                bool isActive = content.IsActive;
                paneViewInfo.UpdateIsActive(isActive);
            }
        }

        private void CloseMethod (object sender, CancelEventArgs e)
        {
            LayoutDocument content = sender as LayoutDocument;
            object item = content.Content;

            if (this.ContentLookupTable.ContainsKey(item))
            {
                this.ContentLookupTable[item].Item2.Remove(item);
            }
        }

        private LayoutDocumentPane FindDocumentPane (DockingManager regionTarget, IRegion region, object item)
        {
            return this.FindDocumentPaneRecursive(regionTarget, region, item, regionTarget.Layout);
        }

        private LayoutDocumentPane FindDocumentPaneRecursive (DockingManager regionTarget, IRegion region, object item, ILayoutContainer container)
        {
            if (container is LayoutDocumentPane)
            {
                return (LayoutDocumentPane)container;
            }

            foreach (ILayoutElement child in container.Children)
            {
                if (child is ILayoutContainer)
                {
                    LayoutDocumentPane pane = this.FindDocumentPaneRecursive(regionTarget, region, item, (ILayoutContainer)child);
                    if (pane != null)
                    {
                        return pane;
                    }
                }
            }

            return null;
        }

        private void OnViewsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e, IRegion region, DockingManager regionTarget)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    LayoutDocumentPane pane = this.FindDocumentPane(regionTarget, region, item);

                    this.RemoveContentWrapper(item);

                    LayoutDocument newContentPane = new LayoutDocument();
                    newContentPane.Content = item;

                    if (item is IPaneViewInfo)
                    {
                        IPaneViewInfo paneViewInfo = item as IPaneViewInfo;

                        newContentPane.CanClose = paneViewInfo.IsCloseable;
                        newContentPane.Title = paneViewInfo.Title;
                        newContentPane.IconSource = paneViewInfo.Icon;

                        paneViewInfo.UpdateIsActive(true);

                        paneViewInfo.RequestBringIntoView += this.RequestBringIntoViewHandler;
                    }
                    else if (item is FrameworkElement)
                    {
                        FrameworkElement frameworkElement = item as FrameworkElement;

                        frameworkElement.RequestBringIntoView += this.RequestBringIntoViewHandler;
                    }

                    if (item is INotifyPropertyChanged)
                    {
                        ( (INotifyPropertyChanged)item ).PropertyChanged += this.PaneViewInfoChangedHandler;
                    }

                    this.ContentLookupTable.Add(item, new Tuple<LayoutDocument, IRegion>(newContentPane, region));

                    newContentPane.IsActiveChanged += this.ActiveDocumentHandler;
                    newContentPane.Closing += this.CloseHandler;

                    pane.Children.Add(newContentPane);

                    newContentPane.IsActive = true;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Dictionary<LayoutDocumentPane, List<int>> indicesToRemove = new Dictionary<LayoutDocumentPane, List<int>>();

                foreach (object item in e.OldItems)
                {
                    LayoutDocumentPane pane = this.FindDocumentPane(regionTarget, region, item);

                    this.ContentLookupTable.Remove(item);

                    for (int i1 = 0; i1 < pane.Children.Count; i1++)
                    {
                        LayoutDocument content = pane.Children[i1] as LayoutDocument;

                        if (content != null)
                        {
                            if (content.Content != null)
                            {
                                if (content.Content.Equals(item))
                                {
                                    if (!indicesToRemove.ContainsKey(pane))
                                    {
                                        indicesToRemove.Add(pane, new List<int>());
                                    }
                                    indicesToRemove[pane].Add(i1);

                                    content.Closing -= this.CloseHandler;
                                    content.IsActiveChanged -= this.ActiveDocumentHandler;

                                    if (content.Content is INotifyPropertyChanged)
                                    {
                                        ( (INotifyPropertyChanged)item ).PropertyChanged -= this.PaneViewInfoChangedHandler;
                                    }

                                    if (content.Content is IPaneViewInfo)
                                    {
                                        IPaneViewInfo paneViewInfo = content.Content as IPaneViewInfo;
                                        paneViewInfo.UpdateIsActive(false);
                                        paneViewInfo.RequestBringIntoView -= this.RequestBringIntoViewHandler;
                                    }
                                    else if (item is FrameworkElement)
                                    {
                                        FrameworkElement frameworkElement = item as FrameworkElement;
                                        frameworkElement.RequestBringIntoView -= this.RequestBringIntoViewHandler;
                                    }

                                    content.Content = null;
                                }
                            }
                        }
                    }
                }

                foreach (object item in e.OldItems)
                {
                    this.RemoveContentWrapper(item);
                }
            }
        }

        private void PaneViewInfoChangedMethod (object sender, PropertyChangedEventArgs e)
        {
            LayoutDocument content = null;
            if (this.ContentLookupTable.ContainsKey(sender))
            {
                content = this.ContentLookupTable[sender].Item1;
            }

            if (sender is IPaneViewInfo && ( content != null ))
            {
                IPaneViewInfo paneViewInfo = sender as IPaneViewInfo;

                switch (e.PropertyName)
                {
                    case nameof(paneViewInfo.Icon):
                    {
                        content.IconSource = paneViewInfo.Icon;
                        break;
                    }

                    case nameof(paneViewInfo.Title):
                    {
                        content.Title = paneViewInfo.Title;
                        break;
                    }

                    case nameof(paneViewInfo.IsCloseable):
                    {
                        content.CanClose = paneViewInfo.IsCloseable;
                        break;
                    }
                }
            }
        }

        private void RemoveContentWrapper (object item)
        {
            FrameworkElement frameworkItem = item as FrameworkElement;
            if (frameworkItem != null)
            {
                LayoutDocument parentDocumentContent = frameworkItem.Parent as LayoutDocument;
                if (parentDocumentContent != null)
                {
                    parentDocumentContent.Content = null;
                }
            }
        }

        private void RequestBringIntoView (object sender, EventArgs eventArgs)
        {
            if (this.ContentLookupTable.ContainsKey(sender))
            {
                this.ContentLookupTable[sender].Item1.IsActive = true;
            }
        }

        #endregion




        #region Overrides

        protected override void Adapt (IRegion region, DockingManager regionTarget)
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
