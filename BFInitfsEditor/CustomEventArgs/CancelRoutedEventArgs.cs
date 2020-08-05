using System;
using System.ComponentModel;
using System.Windows;

namespace BFInitfsEditor.CustomEventArgs
{
    public class CancelRoutedEventArgs : RoutedEventArgs
    {
        private readonly CancelEventArgs _CancelArgs;

        public CancelRoutedEventArgs(RoutedEvent @event, CancelEventArgs cancelArgs)
            : base(@event)
        {
            _CancelArgs = cancelArgs;
        }

        // override the InvokeEventHandler because we are going to pass it CancelEventArgs
        // not the normal RoutedEventArgs
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            CancelEventHandler handler = (CancelEventHandler)genericHandler;
            handler(genericTarget, _CancelArgs);
        }

        // the result
        public bool Cancel
        {
            get
            {
                return _CancelArgs.Cancel;
            }
        }
    }
}