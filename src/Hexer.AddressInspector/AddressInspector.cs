// <copyright file="AddressInspector.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

using Hexer.Data;

namespace Hexer.AddressInspectorLibrary
{
    [Designer(typeof(AddressInspectorLibrary.Design.AddressInspectorRootDesigner), typeof(IRootDesigner))]
    public partial class AddressInspector : UserControl
    {
        // These fields back the public properties.
        private string caption = "Memory";
        private bool editable = true;
        private DataFragment target = null;

        public delegate void EditedData(DataFragment df);

        [Description("Fired when data is edited")]
        [Category("Hex")]
        public event EditedData DataChanged = df => { };

        [Description("Caption")]
        [Category("Hex")]
        public string Caption
        {
            get => this.caption;

            set
            {
                this.caption = value;
                this.addressLabel.Text = this.caption + " address:";
            }
        }

        [Description("Is value editable?")]
        [Category("Hex")]
        public bool Editable
        {
            get => this.editable;

            set
            {
                this.editable = value;
                foreach (var c in this.Controls)
                {
                    if (c is TextBox && c != this.addressTextBox)
                    {
                        var ct = c as TextBox;
                        ct.ReadOnly = !this.editable;
                    }
                }
            }
        }

        [Description("Target memory fragment")]
        [Category("Hex")]
        public DataFragment Target
        {
            get => this.target;

            set
            {
                this.target = value;
                if (this.target != null)
                {
                    this.RecomputeStrings();
                }
            }
        }

        private void RecomputeStrings()
        {
            using (new SuspendDrawing(this))
            {
                this.addressTextBox.Text = DataType.AddressToString(this.target.Address);
                foreach (var dt in DataType.GetKnownDataTypes())
                {
                    var tb = this.Controls[dt.Name + "TextBox"] as TextBox;
                    tb.Text = dt.DecodeToString(this.target);
                }
            }

            this.Refresh();
        }

        public AddressInspector()
        {
            this.InitializeComponent();

            var y = 29;
            var lblX = 5;
            var textBoxX = 50;
            foreach (var dt in DataType.GetKnownDataTypesAndSeperators())
            {
                if (dt.Separator)
                {
                    var lbl = new Label
                    {
                        AutoSize = true,
                        Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0),
                        ForeColor = System.Drawing.SystemColors.GrayText,
                        Location = new System.Drawing.Point(textBoxX, y),
                        Name = dt.Name.ToLower() + "Label",
                        Text = dt.Name,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    };
                    this.Controls.Add(lbl);
                    y += 15;
                }
                else
                {
                    // text box
                    var textBox = new TextBox();
                    textBox.Anchor |= System.Windows.Forms.AnchorStyles.Right;
                    textBox.Location = new System.Drawing.Point(textBoxX, y);
                    textBox.Name = dt.Name + "TextBox";
                    textBox.Size = new System.Drawing.Size(208, 20);
                    textBox.KeyDown += this.TextBox_KeyDown;
                    textBox.Tag = dt;
                    textBox.AcceptsReturn = false;
                    this.Controls.Add(textBox);

                    // label
                    var lbl = new Label
                    {
                        AutoSize = false,
                        Location = new System.Drawing.Point(lblX, y + 3),
                        Name = dt.Name + "Label",
                        Size = new System.Drawing.Size(textBoxX - lblX - 4, 13),
                        Text = dt.Name,
                        TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    };
                    this.Controls.Add(lbl);
                    y += 20;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var tb = sender as TextBox;
                var dt = tb.Tag as DataType;

                var editedFragment = dt.EncodeString(this.target.Address, tb.Text);
                if (editedFragment.Length == dt.NumBytes)
                {
                    this.DataChanged(editedFragment);
                }
            }
        }
    }
}
