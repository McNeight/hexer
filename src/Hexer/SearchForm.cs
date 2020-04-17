// <copyright file="SearchForm.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Forms;

using Hexer.Data;

namespace Hexer
{
    public partial class SearchForm : Form
    {
        private static readonly string SEARCH_TYPE_KEY = "SearchType";

        public SearchForm()
        {
            this.InitializeComponent();

            this.dtComboBox.Items.AddRange(DataType.GetKnownDataTypes().ToArray());

            // string dts = Program.regKey.GetValue(SEARCH_TYPE_KEY, "int8") as string;
            // dtComboBox.SelectedIndex = dtComboBox.FindString(dts);
        }

        public DataFragment ToSearch
        {
            get;
            internal set;
        }

        private void dtComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            // if (dtComboBox.SelectedIndex != -1) Program.regKey.SetValue(SEARCH_TYPE_KEY, dtComboBox.SelectedItem.ToString());
        }

        private void valueTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                if (this.dtComboBox.SelectedIndex > 0)
                {
                    this.dtComboBox.SelectedIndex--;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (this.dtComboBox.SelectedIndex < this.dtComboBox.Items.Count - 1)
                {
                    this.dtComboBox.SelectedIndex++;
                }
            }
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            this.ToSearch = (this.dtComboBox.Items[this.dtComboBox.SelectedIndex] as DataType).EncodeString(0, this.valueTextBox.Text);
        }
    }
}
