using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace WinRT_Phone_Implicit_Client
{
    /// <summary>
    /// ContinuationManager is used to detect if the most recent activation was due
    /// to a continuation 
    /// </summary>
    public class ContinuationManager
    {
        IContinuationActivatedEventArgs args = null;
        bool handled = false;
        Guid id = Guid.Empty;

        /// <summary>
        /// Sets the ContinuationArgs for this instance. Using default Frame of current Window
        /// Should be called by the main activation handling code in App.xaml.cs
        /// </summary>
        /// <param name="args">The activation args</param>
        internal void Continue(IContinuationActivatedEventArgs args)
        {
            Continue(args, Window.Current.Content as Frame);
        }

        /// <summary>
        /// Sets the ContinuationArgs for this instance. Should be called by the main activation
        /// handling code in App.xaml.cs
        /// </summary>
        /// <param name="args">The activation args</param>
        /// <param name="rootFrame">The frame control that contains the current page</param>
        internal void Continue(IContinuationActivatedEventArgs args, Frame rootFrame)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (this.args != null && !handled)
                throw new InvalidOperationException("Can't set args more than once");

            this.args = args;
            this.handled = false;
            this.id = Guid.NewGuid();

            if (rootFrame == null)
                return;

            switch (args.Kind)
            {


                case ActivationKind.WebAuthenticationBrokerContinuation:
                    var wabPage = rootFrame.Content as IWebAuthenticationContinuable;
                    if (wabPage != null)
                    {
                        wabPage.ContinueWebAuthentication(args as WebAuthenticationBrokerContinuationEventArgs);
                    }
                    break;
            }
        }

        /// <summary>
        /// Marks the contination data as 'stale', meaning that it is probably no longer of
        /// any use. Called when the app is suspended (to ensure future activations don't appear
        /// to be for the same continuation) and whenever the continuation data is retrieved 
        /// (so that it isn't retrieved on subsequent navigations)
        /// </summary>
        internal void MarkAsStale()
        {
            this.handled = true;
        }

        /// <summary>
        /// Retrieves the continuation args, if they have not already been retrieved, and 
        /// prevents further retrieval via this property (to avoid accidentla double-usage)
        /// </summary>
        public IContinuationActivatedEventArgs ContinuationArgs
        {
            get
            {
                if (handled)
                    return null;
                MarkAsStale();
                return args;
            }
        }

        /// <summary>
        /// Unique identifier for this particular continuation. Most useful for components that 
        /// retrieve the continuation data via <see cref="GetContinuationArgs"/> and need
        /// to perform their own replay check
        /// </summary>
        public Guid Id { get { return id; } }

        /// <summary>
        /// Retrieves the continuation args, optionally retrieving them even if they have already
        /// been retrieved
        /// </summary>
        /// <param name="includeStaleArgs">Set to true to return args even if they have previously been returned</param>
        /// <returns>The continuation args, or null if there aren't any</returns>
        public IContinuationActivatedEventArgs GetContinuationArgs(bool includeStaleArgs)
        {
            if (!includeStaleArgs && handled)
                return null;
            MarkAsStale();
            return args;
        }
    }

    /// <summary>
    /// Implement this interface if your page invokes the web authentication
    /// broker
    /// </summary>
    interface IWebAuthenticationContinuable
    {
        /// <summary>
        /// This method is invoked when the web authentication broker returns
        /// with the authentication result
        /// </summary>
        /// <param name="args">Activated event args object that contains returned authentication token</param>
        void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args);
    }

}
