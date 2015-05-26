using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;

// ReSharper disable AssignNullToNotNullAttribute

namespace MvvmTools.Utilities
{
    public static class KeyboardUtilities
    {
        internal static void PressKey([NotNull] Visual targetVisual, Key key)
        {
            if (targetVisual == null) throw new ArgumentNullException("targetVisual");

            var target = Keyboard.FocusedElement;    // Target element
            var routedEvent = Keyboard.KeyDownEvent; // Event to send

            target.RaiseEvent(
              new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(targetVisual),
                0,
                key)
              { RoutedEvent = routedEvent }
            );
        }
    }
}
