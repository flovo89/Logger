using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Properties;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Core
{
    [Export (typeof(IShellManager))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class ShellManager : IShellManager,
            IPartImportsSatisfiedNotification
    {
        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public ShellManager ()
        {
            this.SyncRoot = new object();

            this.ShellRelationships = new Dictionary<IShell, Tuple<IShell, bool>>();
        }

        #endregion




        #region Instance Properties/Indexer

        public string CachedCaptionText { get; private set; }

        public Icon CachedIconIcon { get; private set; }

        public ImageSource CachedIconImageSource { get; private set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(IRegionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<IRegionManager> RegionManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ISessionManager> SessionManager { get; private set; }

        [ImportMany (typeof(IShell), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<IShell>> Shells { get; private set; }

        [Import (typeof(IViewManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<IViewManager> ViewManager { get; private set; }

        private Dictionary<IShell, Tuple<IShell, bool>> ShellRelationships { get; }

        #endregion




        #region Instance Methods

        private Window DetermineMainWindow (IEnumerable<object> windowCandidates)
        {
            List<Window> windows = windowCandidates.OfType<Window>().ToList();
            List<Tuple<Window, int>> attributedMainWindows = ( from x in windows where x.GetAttribute<MainWindowAttribute>(true) != null select new Tuple<Window, int>(x, x.GetAttribute<MainWindowAttribute>(true).Priority) ).ToList();
            attributedMainWindows.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return attributedMainWindows.Count == 0 ? ( windows.Count == 0 ? null : windows[0] ) : attributedMainWindows[0].Item1;
        }

        private void HideDialogInternal (string childShellName, bool showStatus)
        {
            IShell[] childShells = this.GetShells(childShellName);

            if (childShells != null)
            {
                foreach (IShell childShell in childShells)
                {
                    if (this.ShellRelationships.ContainsKey(childShell))
                    {
                        IShell parentShell = this.ShellRelationships[childShell].Item1;

                        bool isModal = this.ShellRelationships[childShell].Item2;
                        bool staysModal = ( from x in this.ShellRelationships where object.ReferenceEquals(parentShell, x.Value.Item1) && !object.ReferenceEquals(childShell, x.Key) select x.Value.Item2 ).Any(y => y);

                        this.ShellRelationships.Remove(childShell);
                        childShell.SetParent(null);
                        childShell.Hide();

                        if (isModal && !staysModal)
                        {
                            parentShell.Enable();
                        }
                    }
                }
            }
        }

        private void InitializeShell (IShell shell)
        {
            if (shell != null)
            {
                shell.Initialize();
            }
        }

        private void UpdateCaptionText ()
        {
            Lazy<IShell>[] shells = this.Shells.ToArray();
            foreach (Lazy<IShell> shell in shells)
            {
                shell.Value.SetCaption(this.CachedCaptionText);
            }
        }

        private void UpdateIcon ()
        {
            Lazy<IShell>[] shells = this.Shells.ToArray();
            foreach (Lazy<IShell> shell in shells)
            {
                if (this.CachedIconIcon != null)
                {
                    shell.Value.SetIcon(this.CachedIconIcon);
                }
                else
                {
                    shell.Value.SetIcon(this.CachedIconImageSource);
                }
            }
        }

        private void UpdateShells ()
        {
            this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Updating and initializing shells");

            Lazy<IShell>[] shells = this.Shells.ToArray();
            foreach (Lazy<IShell> shell in shells)
            {
                shell.Value.Initialize();
            }

            bool updateRegions = false;

            DependencyObject[] dependencyShells = ( from x in shells select x.Value ).OfType<DependencyObject>().ToArray();
            foreach (DependencyObject dependencyShell in dependencyShells)
            {
                IRegionManager currentRegionManager = Microsoft.Practices.Prism.Regions.RegionManager.GetRegionManager(dependencyShell);
                if (!object.ReferenceEquals(this.RegionManager.Value, currentRegionManager))
                {
                    this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting region manager for shell: {0} -> [{1}]", dependencyShell.GetType().Name, this.RegionManager.Value.GetType().Name);
                    Microsoft.Practices.Prism.Regions.RegionManager.SetRegionManager(dependencyShell, this.RegionManager.Value);
                    updateRegions = true;
                }
            }

            if (updateRegions)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Updating regions");
                Microsoft.Practices.Prism.Regions.RegionManager.UpdateRegions();
            }

            List<object> windowCandidates = new List<object>();
            windowCandidates.AddRange(from x in shells select x.Value);

            Window mainShellWindow = this.DetermineMainWindow(windowCandidates);
            if (mainShellWindow == null)
            {
                throw new InvalidOperationException(Resources.Bootstrapper_MainWindowInstanceIsRequired);
            }

            this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting main window: {0}", mainShellWindow.GetType().Name);
            this.SessionManager.Value.Application.MainWindow = mainShellWindow;
        }

        #endregion




        #region Interface: IPartImportsSatisfiedNotification

        public void OnImportsSatisfied ()
        {
            lock (this.SyncRoot)
            {
                this.UpdateShells();
                this.UpdateCaptionText();
                this.UpdateIcon();
            }
        }

        #endregion




        #region Interface: IShellManager

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        public void Disable (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Disable();
                    }
                }
            }
        }

        public void DisableAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Disable();
                }
            }
        }

        public void Enable (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Enable();
                    }
                }
            }
        }

        public void EnableAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Enable();
                }
            }
        }

        public Window GetMainWindow ()
        {
            return this.DetermineMainWindow(from x in this.Shells select x.Value);
        }

        public IShell GetShell (string shellName)
        {
            if (shellName == null)
            {
                throw new ArgumentNullException(nameof(shellName));
            }

            if (shellName.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(shellName));
            }

            lock (this.SyncRoot)
            {
                IShell shell = this.SessionManager.Value.Container.GetExportedValue<object>(shellName) as IShell;
                this.InitializeShell(shell);
                return shell;
            }
        }

        public IShell[] GetShells (string shellName)
        {
            if (shellName == null)
            {
                throw new ArgumentNullException(nameof(shellName));
            }

            if (shellName.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(shellName));
            }

            lock (this.SyncRoot)
            {
                IShell[] shells = this.SessionManager.Value.Container.GetExportedValues<object>(shellName).ToArray().OfType<IShell>().ToArray();
                foreach (IShell shell in shells)
                {
                    this.InitializeShell(shell);
                }
                return shells;
            }
        }

        public void Hide (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Hide();
                    }
                }
            }
        }

        public void HideAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Hide();
                }
            }
        }

        public void HideDialog (string childShellName)
        {
            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Hiding dialog: {0}", childShellName);
                this.HideDialogInternal(childShellName, false);
            }
        }

        public void Maximize (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Maximize();
                    }
                }
            }
        }

        public void MaximizeAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Maximize();
                }
            }
        }

        public void Minimize (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Minimize();
                    }
                }
            }
        }

        public void MinimizeAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Minimize();
                }
            }
        }

        public void MoveAllToBackground ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.MoveToBackground();
                }
            }
        }

        public void MoveAllToForeground ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.MoveToForeground();
                }
            }
        }

        public void MoveAllToPrimaryScreen ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.MoveToPrimaryScreen();
                }
            }
        }

        public void MoveAllToScreen (int screenIndex)
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.MoveToScreen(screenIndex);
                }
            }
        }

        public void MoveAllToScreen (Screen screen)
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.MoveToScreen(screen);
                }
            }
        }

        public void MoveToBackground (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.MoveToBackground();
                    }
                }
            }
        }

        public void MoveToForeground (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.MoveToForeground();
                    }
                }
            }
        }

        public void MoveToPrimaryScreen (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.MoveToPrimaryScreen();
                    }
                }
            }
        }

        public void MoveToScreen (string shellName, int screenIndex)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.MoveToScreen(screenIndex);
                    }
                }
            }
        }

        public void MoveToScreen (string shellName, Screen screen)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.MoveToScreen(screen);
                    }
                }
            }
        }

        public void SetAllCaption (string caption)
        {
            lock (this.SyncRoot)
            {
                this.CachedCaptionText = caption;
                this.UpdateCaptionText();
            }
        }

        public void SetAllHideMouseCursor (bool hideMouseCursor)
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.SetHideMouseCursor(hideMouseCursor);
                }
            }
        }

        public void SetAllIcon (Icon icon)
        {
            lock (this.SyncRoot)
            {
                this.CachedIconIcon = icon;
                this.CachedIconImageSource = null;
                this.UpdateIcon();
            }
        }

        public void SetAllIcon (ImageSource icon)
        {
            lock (this.SyncRoot)
            {
                this.CachedIconImageSource = icon;
                this.CachedIconIcon = null;
                this.UpdateIcon();
            }
        }

        public void SetAllRectangle (Rect rectangle)
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.SetRectangle(rectangle);
                }
            }
        }

        public void SetAllState (WindowState state)
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.SetState(state);
                }
            }
        }

        public void SetCaption (string shellName, string caption)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetCaption(caption);
                    }
                }
            }
        }

        public void SetHideMouseCursor (string shellName, bool hideMouseCursor)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetHideMouseCursor(hideMouseCursor);
                    }
                }
            }
        }

        public void SetIcon (string shellName, Icon icon)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetIcon(icon);
                    }
                }
            }
        }

        public void SetIcon (string shellName, ImageSource icon)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetIcon(icon);
                    }
                }
            }
        }

        public void SetParent (string shellName, string parentShellName)
        {
            lock (this.SyncRoot)
            {
                IShell parentShell = this.GetShell(parentShellName);
                IShell[] shells = this.GetShells(shellName);

                if (( parentShell != null ) && ( shells != null ))
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetParent(parentShell);
                    }
                }
            }
        }

        public void SetRectangle (string shellName, Rect rectangle)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetRectangle(rectangle);
                    }
                }
            }
        }

        public void SetState (string shellName, WindowState state)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.SetState(state);
                    }
                }
            }
        }

        public void Show (string shellName)
        {
            lock (this.SyncRoot)
            {
                IShell[] shells = this.GetShells(shellName);

                if (shells != null)
                {
                    foreach (IShell shell in shells)
                    {
                        shell.Show();
                    }
                }
            }
        }

        public void ShowAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<IShell>[] shells = this.Shells.ToArray();
                foreach (Lazy<IShell> shell in shells)
                {
                    shell.Value.Show();
                }
            }
        }

        public void ShowDialog (string parentShellName, string childShellName, bool modal)
        {
            lock (this.SyncRoot)
            {
                IShell parentShell = this.GetShell(parentShellName);
                IShell[] childShells = this.GetShells(childShellName);

                if (( parentShell != null ) && ( childShells != null ))
                {
                    this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Showing dialog: {0} <- {1} ({2})", childShellName, parentShellName, modal ? "modal" : "non-modal");

                    this.HideDialogInternal(childShellName, true);

                    foreach (IShell childShell in childShells)
                    {
                        this.ShellRelationships.Add(childShell, new Tuple<IShell, bool>(parentShell, modal));
                        childShell.SetParent(parentShell);
                        childShell.Show();
                    }

                    if (modal)
                    {
                        parentShell.Disable();
                    }
                }
            }
        }

        #endregion
    }
}
