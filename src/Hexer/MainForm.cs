// <copyright file="MainForm.cs" company="Random Developers on the Internet">
// Copyright © 2015 Peter Thoman. Copyright © 2020 Neil McNeight.
// All rights reserved. Licensed under the GPLv3 license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Hexer.Data;
using Hexer.HexViewLibrary;

namespace Hexer
{
    public partial class MainForm : Form, IMessageFilter
    {
        public MainForm()
        {
            this.InitializeComponent();
            Application.AddMessageFilter(this);
            this.selectedAddressInspector.DataChanged += this.SelectedAddressInspector_DataChanged;
            this.Refresh();
        }

        private void SelectedAddressInspector_DataChanged(DataFragment data)
        {
            this.hexView.ApplyEdit(data);
            this.selectedAddressInspector.Target = this.hexView.GetSelectedData();
        }

        /// <inheritdoc/>
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                // WM_MOUSEWHEEL, find the control at screen position m.LParam
                var hWnd = WindowFromPoint(Cursor.Position);

                if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
                {
                    SendMessage(hWnd, WM_MOUSEWHEEL, m.WParam, m.LParam);
                    return true;
                }
            }

            return false;
        }

        // P/Invoke declarations
        private const int WM_MOUSEWHEEL = 0x20a;

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gt = new GoToForm();
            if (gt.ShowDialog() == DialogResult.OK)
            {
                this.hexView.NavigateToAddress(gt.Address);
            }
        }

        private void goToSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.hexView.NavigateToAddress();
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sf = new SearchForm();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                if (this.hexView.Search(sf.ToSearch))
                {
                    this.statusStrip.Text = "Found search data";
                }
                else
                {
                    this.statusStrip.Text = "No occurance found";
                }
            }
        }

        private void hexView_SelectedAddressChanged(object sender, EventArgs e)
        {
            this.selectedAddressInspector.Target = this.hexView.GetSelectedData();
        }

        private void hexView_HoverAddressChanged(object sender, EventArgs e)
        {
            this.hoverAddressInspector.Target = this.hexView.GetHoverData();
        }

        // --------------------------------------------------------------------- File Menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                this.hexView.FileName = fd.FileName;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                FileName = this.hexView.FileName,
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                this.hexView.SaveToFile(sfd.FileName);
                this.toolStripStatusLabel.Text = "Saved to " + sfd.FileName;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.hexView.SaveToFile();
            this.toolStripStatusLabel.Text = "Saved to " + this.hexView.FileName;
        }

        // --------------------------------------------------------------------- Markers Menu
        private void ConfigureMarkersDialog(FileDialog fd)
        {
            fd.Filter = "Hexer marker files (*.hmf)|*.hmf";
            fd.DefaultExt = "hmf";
        }

        private void openMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            this.ConfigureMarkersDialog(ofd);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MarkerRepository.Instance.LoadFromFile(ofd.FileName);
                this.hexView.Refresh();
                this.toolStripStatusLabel.Text = "Loaded Markers from " + ofd.FileName;
            }
        }

        private void saveMarkersAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            this.ConfigureMarkersDialog(sfd);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MarkerRepository.Instance.SaveToFile(sfd.FileName);
                this.toolStripStatusLabel.Text = "Saved Markers to " + sfd.FileName;
            }
        }

        private void saveMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MarkerRepository.Instance.HasFileName())
            {
                MarkerRepository.Instance.SaveToFile();
            }
            else
            {
                this.saveMarkersAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void showMarkerWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
