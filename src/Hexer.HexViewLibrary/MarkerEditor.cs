// <copyright file="MarkerEditor.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Forms;

using Hexer.Data;

namespace Hexer.HexViewLibrary
{
    public partial class MarkerEditor : Form
    {
        private readonly DataMarker marker;
        private readonly HexView hexview;

        public MarkerEditor(DataMarker marker, HexView hexview)
        {
            this.InitializeComponent();
            this.marker = marker;
            this.hexview = hexview;

            this.markerAtTextBox.Text = DataType.AddressToString(marker.Address);
            this.noteTextBox.Text = marker.Note;
            this.dataTypeComboBox.Items.AddRange(DataType.GetKnownDataTypes().ToArray());
            this.dataTypeComboBox.SelectedIndex = this.dataTypeComboBox.FindString(marker.Type.Name);
            this.sizeNumericUpDown.Value = marker.NumBytes;
            this.sizeNumericUpDown.Enabled = marker.Type.VariableNumBytes;
            this.valueTextBox.Text = marker.Type.DecodeToString(hexview.GetDataAt(marker.Address));
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            this.marker.Note = this.noteTextBox.Text;
            this.marker.Type = DataType.FromString(this.dataTypeComboBox.Text);
            this.sizeNumericUpDown.Enabled = this.marker.Type.VariableNumBytes;
            this.marker.NumBytes = (int)this.sizeNumericUpDown.Value;

            this.hexview.ApplyEdit(this.marker.Type.EncodeString(this.marker.Address, this.valueTextBox.Text));
        }

        private void dataTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.marker.Type = DataType.FromString(this.dataTypeComboBox.Text);
            this.sizeNumericUpDown.Value = this.marker.Type.NumBytes;
            this.sizeNumericUpDown.Enabled = this.marker.Type.VariableNumBytes;
            this.valueTextBox.Text = this.marker.Type.DecodeToString(this.hexview.GetDataAt(this.marker.Address));
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.applyButton_Click(sender, e);
            this.Close();
        }
    }
}
