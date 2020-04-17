// <copyright file="SuspendDrawing.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Forms;

namespace Hexer.AddressInspectorLibrary
{
    public class SuspendDrawing : IDisposable
    {
        private const int WM_SETREDRAW = 0x000B;
        private readonly Control control;
        private readonly NativeWindow window;

        public SuspendDrawing(Control control)
        {
            this.control = control;
            var msgSuspendUpdate = Message.Create(this.control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            this.window = NativeWindow.FromHandle(this.control.Handle);
            this.window.DefWndProc(ref msgSuspendUpdate);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var wparam = new IntPtr(1);  // Create a C "true" boolean as an IntPtr
            var msgResumeUpdate = Message.Create(this.control.Handle, WM_SETREDRAW, wparam, IntPtr.Zero);
            this.window.DefWndProc(ref msgResumeUpdate);
            this.control.Invalidate();
        }
    }
}
