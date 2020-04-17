// <copyright file="HexView.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Hexer.Data;

namespace Hexer.HexViewLibrary
{
    [Designer(typeof(HexViewLibrary.Design.HexViewRootDesigner), typeof(IRootDesigner))]
    public partial class HexView : UserControl
    {
        private readonly int WHEEL_DELTA = 120;
        private readonly Font cFont = new Font("Consolas", 11);
        private readonly Font tFont = new Font("Tahoma", 6);
        private readonly Font vFont = new Font("Segoe UI", 10);

        private readonly int hSpacing = 1;
        private readonly int vSpacing = 0;
        private readonly Interval<int> visibleAddresses = new Interval<int>(0, 0);
        private int xStart;
        private int yStart;
        private SizeF byteSize;

        private int startLine = 0;
        private int totalLines = 1;

        private int visibleLines = 1;

        private int hoverAddress = -1;

        private int selectedAddress = -1;

        private string fileName = string.Empty;
        private byte[] fileBytes = null;
        private int numBytesInLine = 32;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexView"/> class.
        /// </summary>
        public HexView()
        {
            this.InitializeComponent();

            foreach (var dt in DataType.GetKnownDataTypes())
            {
                var markAs = new ToolStripMenuItem(dt.Name);
                markAs.Click += this.MarkAs_Click;
                markAs.Tag = dt;
                this.markAsToolStripMenuItem.DropDownItems.Add(markAs);
            }

            this.SelectedAddress = -1;
            this.HoverAddress = -1;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserMouse, true);
            this.ComputeMetrics();
        }

        [Description("Triggered when hover address changes")]
        [Category("Hex")]
        public event EventHandler HoverAddressChanged = (sender, e) => { };

        [Description("Triggered when selected address changes")]
        [Category("Hex")]
        public event EventHandler SelectedAddressChanged = (sender, e) => { };

        public int HoverAddress
        {
            get => this.hoverAddress;

            set
            {
                this.hoverAddress = value;
                this.Refresh();
                this.HoverAddressChanged(this, new EventArgs());
            }
        }

        [Description("Hex file name")]
        [Category("Hex")]
        public string FileName
        {
            get => this.fileName;

            set
            {
                if (value != null)
                {
                    this.fileName = value;
                }
                else
                {
                    this.fileName = string.Empty;
                }

                if (this.fileName.Length > 0)
                {
                    this.LoadFile(this.fileName);
                }
            }
        }

        [Description("Number of bytes shown in a line")]
        [Category("Hex")]
        public int NumBytesInLine
        {
            get => this.numBytesInLine;
            set => this.numBytesInLine = value;
        }

        public int SelectedAddress
        {
            get => this.selectedAddress;

            set
            {
                this.selectedAddress = value;
                this.Refresh();
                this.SelectedAddressChanged(this, new EventArgs());
            }
        }

        private int StartLine
        {
            get => this.startLine;

            set
            {
                if (value < 0)
                {
                    this.startLine = 0;
                }
                else if (value >= this.totalLines - 1)
                {
                    this.startLine = this.totalLines - 1;
                }
                else
                {
                    this.startLine = value;
                }

                this.vScrollBar.Value = this.startLine;
                this.Refresh();
            }
        }

        private int LineHeight => (int)Math.Ceiling(this.byteSize.Height + this.vSpacing);

        private int ColumnWidth => (int)Math.Ceiling(this.byteSize.Width + this.hSpacing);

        private Size CellSize => new Size(this.ColumnWidth, this.LineHeight);

        public void SaveToFile(string fileName)
        {
            this.fileName = fileName;
            File.WriteAllBytes(fileName, this.fileBytes);
        }

        public void SaveToFile()
        {
            File.WriteAllBytes(this.fileName, this.fileBytes);
        }

        /// <inheritdoc/>
        public override void Refresh()
        {
            base.Refresh();
            this.ComputeVisible();
        }

        public PointF GetLocationOfAddress(int addr)
        {
            var startAddr = this.StartLine * 8 * this.numBytesInLine;
            addr -= startAddr;
            var xidx = addr / 8 % this.numBytesInLine;
            var yidx = addr / 8 / this.numBytesInLine;
            return new PointF(this.xStart + (xidx * this.ColumnWidth), this.yStart + (yidx * this.LineHeight));
        }

        public int GetAddressAt(Point loc)
        {
            var address = this.numBytesInLine * this.StartLine * 8; // start
            address += Math.Max(0, loc.Y - this.yStart) / this.LineHeight * this.numBytesInLine * 8; // lines
            var xoffset = Math.Max(0, loc.X - this.xStart) / this.ColumnWidth * 8; // x position
            address += Math.Min(xoffset, (this.numBytesInLine - 1) * 8);
            return address;
        }

        public DataFragment GetDataAt(Point loc)
        {
            var addr = this.GetAddressAt(loc);
            return new DataFragment(addr, this.fileBytes, addr / 8);
        }

        public DataFragment GetDataAt(int address)
        {
            if (this.fileBytes == null)
            {
                return new DataFragment();
            }

            if (address < 0 || address >= this.fileBytes.Length * 8)
            {
                return new DataFragment();
            }

            return new DataFragment(address, this.fileBytes, address / 8);
        }

        public DataFragment GetSelectedData() { return this.GetDataAt(this.SelectedAddress); }

        public DataFragment GetHoverData() { return this.GetDataAt(this.HoverAddress); }

        public void NavigateToAddress(int address)
        {
            if (address < 0)
            {
                address = 0;
            }

            if (address / 8 >= this.fileBytes.Length)
            {
                address = this.fileBytes.Length - 1;
            }

            this.SelectedAddress = address;

            if (!this.visibleAddresses.Contains(this.SelectedAddress))
            {
                this.StartLine = (address / 8 / this.numBytesInLine) - (this.visibleLines / 2);
            }
        }

        public void NavigateToAddress()
        {
            this.NavigateToAddress(this.SelectedAddress);
        }

        public bool Search(DataFragment toSearch)
        {
            var target = new byte[toSearch.Length];
            Array.Copy(toSearch.Data, target, toSearch.Length);
            var idx = StartingIndex(this.fileBytes, target);
            if (idx >= 0)
            {
                this.NavigateToAddress(idx * 8);
            }

            return idx >= 0;
        }

        public void ApplyEdit(DataFragment data)
        {
            if (data.Length > 0)
            {
                Array.Copy(data.Data, 0, this.fileBytes, data.Address / 8, data.Length);
                this.NavigateToAddress();
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.FillRectangle(Brushes.Black, e.ClipRectangle);

            Bitmap markerBmp = null;
            var skipAddresses = 0;
            var skippedAddresses = 0;
            if (this.fileBytes != null)
            {
                var y = this.yStart;
                var line = this.StartLine;
                while (y < e.ClipRectangle.Bottom)
                {
                    var lineAddress = this.numBytesInLine * line * 8;
                    var addrString = "0x" + Convert.ToString(lineAddress, 16).PadLeft(8, '0').ToUpper();
                    g.DrawString(addrString, this.cFont, Brushes.Aqua, new PointF(this.hSpacing, y));

                    for (var i = 0; i < this.numBytesInLine; ++i)
                    {
                        var pos = i + (this.numBytesInLine * line);
                        if (pos >= this.fileBytes.Length)
                        {
                            break;
                        }

                        var address = pos * 8;
                        var point = new Point(this.xStart + (i * this.ColumnWidth), y);

                        // in a marker, draw bg/line and skip rest
                        if (skipAddresses > 0)
                        {
                            skipAddresses--;
                            skippedAddresses++;

                            // blit remaining portions of marker
                            var origin = new Point(this.ColumnWidth * skippedAddresses, 0);
                            g.DrawImage(markerBmp, new Rectangle(point, this.CellSize), new Rectangle(origin, this.CellSize), GraphicsUnit.Pixel);
                            continue;
                        }

                        var marker = MarkerRepository.Instance.GetMarker(address);

                        // handle markers
                        if (marker != null)
                        {
                            markerBmp = this.DrawMarker(marker, address);
                            skipAddresses = marker.NumBytes - 1;
                            skippedAddresses = 0;

                            // blit first portion of marker
                            g.DrawImage(markerBmp, new Rectangle(point, this.CellSize), new Rectangle(new Point(0, 0), this.CellSize), GraphicsUnit.Pixel);
                            continue;
                        }

                        // highlighting and selection
                        if (address == this.hoverAddress)
                        {
                            g.FillRectangle(Brushes.Blue, new RectangleF(point, this.byteSize));
                        }
                        else if (this.hoverAddress >= 0 && address > this.hoverAddress && address - this.hoverAddress < 8 * 8 && !MarkerRepository.Instance.isMarker(this.HoverAddress))
                        {
                            g.FillRectangle(Brushes.DarkBlue, new RectangleF(point, this.byteSize));
                        }

                        if (address == this.selectedAddress)
                        {
                            g.DrawRectangle(new Pen(Brushes.Red, 1.0f), Rectangle.Round(new RectangleF(point, this.byteSize)));
                        }
                        else if (this.selectedAddress >= 0 && address > this.selectedAddress && address - this.selectedAddress < 8 * 8 && !MarkerRepository.Instance.isMarker(this.SelectedAddress))
                        {
                            g.DrawRectangle(new Pen(Brushes.DarkRed, 1.0f), Rectangle.Round(new RectangleF(point, this.byteSize)));
                        }

                        // handle normal bytes
                        var byt = this.fileBytes[pos];
                        var byteString = Convert.ToString(byt, 16).PadLeft(2, '0').ToUpper();
                        var brush = Brushes.LightGray;
                        if (byt == 0)
                        {
                            brush = Brushes.Gray;
                        }

                        g.DrawString(byteString, this.cFont, brush, point);
                    }

                    line++;
                    y += this.LineHeight;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var offset = e.Delta / this.WHEEL_DELTA;
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                offset *= 15;
            }

            this.StartLine -= offset;
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Application.DoEvents();
            base.OnMouseMove(e);
            var p = new Point(e.X, e.Y);
            var hAddr = this.GetAddressAt(p);
            var m = MarkerRepository.Instance.GetMarkerCovering(hAddr);
            if (m != null)
            {
                hAddr = m.Address;
            }

            this.HoverAddress = hAddr;
        }

        /// <inheritdoc/>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var p = new Point(e.X, e.Y);
            this.SelectedAddress = this.GetAddressAt(p);
            var m = MarkerRepository.Instance.GetMarkerCovering(this.SelectedAddress);
            if (m != null)
            {
                this.SelectedAddress = m.Address;
                if (e.Button == MouseButtons.Right)
                {
                    this.markerMenuStrip.Show(this.PointToScreen(p));
                }
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    this.contextMenuStrip.Show(this.PointToScreen(p));
                }
            }
        }

        private static bool IsSubArrayEqual(byte[] x, byte[] y, int start)
        {
            for (var i = 0; i < y.Length; i++)
            {
                if (x[start++] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static int StartingIndex(byte[] x, byte[] y)
        {
            var max = 1 + x.Length - y.Length;
            for (var i = 0; i < max; i++)
            {
                if (IsSubArrayEqual(x, y, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void MarkAs_Click(object sender, EventArgs e)
        {
            var ts = sender as ToolStripMenuItem;
            var dt = ts.Tag as DataType;
            MarkerRepository.Instance.AddMarker(this.SelectedAddress, dt);
        }

        private void deleteMarkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MarkerRepository.Instance.RemoveMarker(this.SelectedAddress);
        }

        private void editMarkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MarkerRepository.Instance.EditMarker(this.SelectedAddress, this);
        }

        private void ComputeMetrics()
        {
            var g = this.CreateGraphics();
            this.byteSize = g.MeasureString("00", this.cFont);
            this.xStart = (int)g.MeasureString("0x00000000", this.cFont).Width + (this.hSpacing * 3);
            this.yStart = 1;
        }

        private void ComputeVisible()
        {
            if (this.LineHeight == 0)
            {
                return;
            }

            this.visibleLines = (this.Height - this.yStart) / this.LineHeight;
            this.visibleAddresses.Min = this.StartLine * 8 * this.numBytesInLine;
            this.visibleAddresses.Max = this.visibleAddresses.Min + (this.visibleLines * this.numBytesInLine * 8);
        }

        private void LoadFile(string fileName)
        {
            this.fileBytes = File.ReadAllBytes(fileName);
            this.totalLines = this.fileBytes.Length / this.NumBytesInLine;
            this.vScrollBar.Minimum = 0;
            this.vScrollBar.Maximum = this.totalLines;
            this.vScrollBar.Value = 0;
            this.SelectedAddress = -1;
            this.HoverAddress = -1;
            this.Refresh();
        }

        private Bitmap DrawMarker(DataMarker marker, int address)
        {
            // draw marker to offscreen surface
            var origin = new Point(0, 0);
            var mWidth = this.ColumnWidth * marker.NumBytes;
            var markerBmp = new Bitmap(mWidth, this.LineHeight);
            var mg = Graphics.FromImage(markerBmp);
            var markerRect = new Rectangle(origin, new Size(mWidth, this.LineHeight));

            // highlighting and selection
            if (address == this.hoverAddress)
            {
                mg.FillRectangle(Brushes.DarkBlue, markerRect);
            }

            if (address == this.selectedAddress)
            {
                mg.FillRectangle(Brushes.DarkRed, markerRect);
            }

            // strings
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
            };
            mg.DrawString(marker.Type.DecodeToString(new DataFragment(address, this.fileBytes, address / 8, marker.NumBytes)), this.vFont, Brushes.White, markerRect, sf);
            origin.Y += 11;
            mg.DrawString(marker.Type.ShortName, this.tFont, Brushes.Orange, origin);

            // draw line
            var measure = mg.MeasureString(marker.Type.ShortName, this.tFont);
            origin.X += (int)measure.Width + this.hSpacing;
            origin.Y = this.LineHeight - 3;
            var point2 = new PointF(mWidth, origin.Y);
            mg.DrawLine(new Pen(Brushes.DarkOrange), origin, point2);

            return markerBmp;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.StartLine = this.vScrollBar.Value;
        }
    }
}
