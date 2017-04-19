using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

using Logger.Common.Base.Imaging;
using Logger.Common.Base.Windows;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;

using Cursors = System.Windows.Input.Cursors;




namespace Logger.Core.Modularity
{
    [InheritedExport (typeof(IShell))]
    public abstract class Shell : Window,
            IShell
    {
        #region Constants

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(IViewModel), typeof(Shell), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, Shell.OnViewModelChanged));

        #endregion




        #region Static Methods

        private static void OnViewModelChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ( (Shell)sender ).DataContext = e.NewValue;
        }

        #endregion




        #region Instance Constructor/Destructor

        [ImportingConstructor]
        protected Shell ()
        {
            this.IsInitializedInternal = false;
        }

        #endregion




        #region Instance Properties/Indexer

        private bool IsInitializedInternal { get; set; }

        [Import (typeof(ILogManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISessionManager> SessionManager { get; private set; }

        #endregion




        #region Instance Methods

        private void InitializeInternal ()
        {
            this.BeginInit();
            this.Initialize();
            this.EndInit();
        }

        #endregion




        #region Virtuals

        protected virtual void Disable ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Disabling shell: {0}", this.GetType().Name);
            this.EnableWindow(false);
        }

        protected virtual void Enable ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Enabling shell: {0}", this.GetType().Name);
            this.EnableWindow(true);
        }

        protected virtual void Initialize ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Initializing shell: {0}", this.GetType().Name);

            this.InitializeName();

            if (this.ViewModel != null)
            {
                this.ViewModel.Initialize();
            }
        }

        protected virtual void InitializeName ()
        {
            this.Name = this.GetType().Name;
        }

        protected virtual void Maximize ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Maximizing shell: {0}", this.GetType().Name);
            this.MaximizeWindow();
        }

        protected virtual void Minimize ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Minimizing shell: {0}", this.GetType().Name);
            this.MinimizeWindow();
        }

        protected virtual void MoveToBackground ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Moving shell to background: {0}", this.GetType().Name);
            this.MoveWindowToBackground();
        }

        protected virtual void MoveToForeground ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Moving shell to foreground: {0}", this.GetType().Name);
            this.MoveWindowToForeground();
        }

        protected virtual void MoveToPrimaryScreen ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Moving shell to primary screen: {0} -> [P]", this.GetType().Name);
            this.MoveWindowToPrimaryScreen();
        }

        protected virtual void MoveToScreen (int screenIndex)
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Moving shell to screen: {0} -> [{1}]", this.GetType().Name, screenIndex);
            this.MoveWindowToScreen(screenIndex);
        }

        protected virtual void MoveToScreen (Screen screen)
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Moving shell to screen: {0} -> [{1}]", this.GetType().Name, screen == null ? "[null]" : screen.DeviceName);
            this.MoveWindowToScreen(screen);
        }

        protected virtual void OnActivate ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Activating shell: {0}", this.GetType().Name);

            if (this.ViewModel != null)
            {
                this.ViewModel.OnNavigatedTo(null);
            }
        }

        protected virtual void OnDeactivate ()
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Deactivating shell: {0}", this.GetType().Name);

            if (this.ViewModel != null)
            {
                this.ViewModel.OnNavigatedFrom(null);
            }
        }

        protected virtual void SetHideMouseCursor (bool hideMouseCursor)
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Hiding mouse cursor of shell: {0} -> [{1}]", this.GetType().Name, hideMouseCursor);
            this.ForceCursor = hideMouseCursor;
            this.Cursor = hideMouseCursor ? Cursors.None : null;
        }

        protected virtual void SetRectangle (Rect rectangle)
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Setting rectangle of shell: {0} -> [{1}]", this.GetType().Name, rectangle);
            this.Top = rectangle.Top;
            this.Left = rectangle.Left;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        protected virtual void SetState (WindowState state)
        {
            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Setting state of shell: {0} -> [{1}]", this.GetType().Name, state);
            this.WindowState = state;
        }

        #endregion




        #region Overrides

        protected override void OnActivated (EventArgs e)
        {
            base.OnActivated(e);

            this.OnActivate();
        }

        protected override void OnClosing (CancelEventArgs e)
        {
            base.OnClosing(e);

            this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Attempting to close shell: {0}", this.GetType().Name);

            e.Cancel = true;
        }

        protected override void OnDeactivated (EventArgs e)
        {
            base.OnDeactivated(e);

            this.OnDeactivate();
        }

        #endregion




        #region Interface: IShell

        public Window AssociatedWindow => this;

        public IViewModel ViewModel
        {
            get
            {
                return (IViewModel)this.GetValue(Shell.ViewModelProperty);
            }
            set
            {
                this.SetValue(Shell.ViewModelProperty, value);
            }
        }

        void IShell.Disable ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.Disable));
        }

        void IShell.Enable ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.Enable));
        }

        public Rect GetRectangle ()
        {
            return new Rect(this.Left, this.Top, this.Width, this.Height);
        }

        public Screen GetScreen ()
        {
            return WindowExtensions.GetScreen(this);
        }

        public int GetScreenIndex ()
        {
            return WindowExtensions.GetScreenIndex(this);
        }

        public WindowState GetState ()
        {
            return this.WindowState;
        }

        void IShell.Hide ()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Hiding shell: {0}", this.GetType().Name);
                this.Hide();
            }));
        }

        void IShell.Initialize ()
        {
            if (!this.IsInitializedInternal)
            {
                this.IsInitializedInternal = true;
                this.SessionManager.Value.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(this.InitializeInternal));
            }
        }

        void IShell.Maximize ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.Maximize));
        }

        void IShell.Minimize ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.Minimize));
        }

        void IShell.MoveToBackground ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.MoveToBackground));
        }

        void IShell.MoveToForeground ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.MoveToForeground));
        }

        void IShell.MoveToPrimaryScreen ()
        {
            this.Dispatcher.BeginInvoke(new Action(this.MoveToPrimaryScreen));
        }

        void IShell.MoveToScreen (int screenIndex)
        {
            this.Dispatcher.BeginInvoke(new Action<int>(this.MoveToScreen), screenIndex);
        }

        void IShell.MoveToScreen (Screen screen)
        {
            this.Dispatcher.BeginInvoke(new Action<Screen>(this.MoveToScreen), screen);
        }

        void IShell.OnActivate ()
        {
            this.OnActivate();
        }

        void IShell.OnDeactivate ()
        {
            this.OnDeactivate();
        }

        void IShell.SetCaption (string caption)
        {
            caption = caption ?? string.Empty;
            this.Dispatcher.BeginInvoke(new Action<string>(x => this.Title = x), caption);
        }

        void IShell.SetHideMouseCursor (bool hideMouseCursor)
        {
            this.Dispatcher.BeginInvoke(new Action<bool>(this.SetHideMouseCursor), hideMouseCursor);
        }

        void IShell.SetIcon (ImageSource icon)
        {
            this.Dispatcher.BeginInvoke(new Action<ImageSource>(x => this.Icon = x), icon);
        }

        void IShell.SetIcon (Icon icon)
        {
            this.Dispatcher.BeginInvoke(new Action<Icon>(x => this.Icon = x == null ? null : x.ToBitmapSource()), icon);
        }

        void IShell.SetParent (IShell parentShell)
        {
            this.Dispatcher.BeginInvoke(new Action<IShell>(x =>
            {
                this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Setting parent of shell: {0} <- {1}", this.GetType().Name, x == null ? "[null]" : x.ToString());
                this.Owner = x as Window;
            }), parentShell);
        }

        void IShell.SetRectangle (Rect rectangle)
        {
            this.Dispatcher.BeginInvoke(new Action<Rect>(this.SetRectangle), rectangle);
        }

        void IShell.SetState (WindowState state)
        {
            this.Dispatcher.BeginInvoke(new Action<WindowState>(this.SetState), state);
        }

        void IShell.Show ()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.LogManager.Value.Log(typeof(Shell).Name, LogLevel.Debug, "Showing shell: {0}", this.GetType().Name);
                this.Show();
            }));
        }

        #endregion
    }
}
