// <copyright file="GoToForm.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Forms;

using Hexer.Data;

namespace Hexer
{
    public partial class GoToForm : Form
    {
        public int Address { get; internal set; }

        public GoToForm()
        {
            this.InitializeComponent();
        }

        private void targetTextBox_TextChanged(object sender, EventArgs e)
        {
            this.Address = DataType.StringToAddress(this.targetTextBox.Text);
        }
    }
}
