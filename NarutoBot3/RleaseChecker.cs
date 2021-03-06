﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NarutoBot3.Properties;

namespace NarutoBot3
{
    public partial class RleaseChecker : Form
    {
        public RleaseChecker()
        {
            InitializeComponent();
        }

        private void RleaseChecker_Shown(object sender, EventArgs e)
        {
            cb_Enable.Checked = Settings.Default.releaseEnabled;

            tbURL.Text = Settings.Default.baseURL;
            tb_chNumber.Text = Settings.Default.chapterNumber;
            tb_interval.Text = Settings.Default.checkInterval.ToString();
        }

        private void bt_OK_Click(object sender, EventArgs e)
        {
            Settings.Default.releaseEnabled = cb_Enable.Checked;
            Settings.Default.baseURL=tbURL.Text;
            Settings.Default.chapterNumber=tb_chNumber.Text;
            Settings.Default.checkInterval = Convert.ToInt32(tb_interval.Text);
            Settings.Default.Save();
        }
    }
}
