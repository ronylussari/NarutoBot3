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
    public partial class eta : Form
    {
        public eta()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Default.eta = textBox1.Text;
            Settings.Default.Save();
            this.Close();
        }

        private void eta_Shown(object sender, EventArgs e)
        {
            textBox1.Text = Settings.Default.eta;
        }
    }
}
