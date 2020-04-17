// <copyright file="HexViewRootDesigner.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System.ComponentModel.Design;
using System.Diagnostics;
using System.Windows.Forms.Design;

namespace Hexer.HexViewLibrary.Design
{
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class HexViewRootDesigner : DocumentDesigner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HexViewRootDesigner"/> class.
        /// </summary>
        public HexViewRootDesigner()
        {
            Trace.WriteLine("HexViewRootDesigner ctor");
        }

        /// <inheritdoc/>
        public override void Initialize(System.ComponentModel.IComponent component)
        {
            base.Initialize(component);

            if (this.GetService(typeof(IComponentChangeService)) is IComponentChangeService cs)
            {
                cs.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Component is HexView)
            {
                this.Control.Refresh();
            }
        }
    }
}
