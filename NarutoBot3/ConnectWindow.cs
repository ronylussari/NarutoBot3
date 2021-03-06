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
    public partial class ConnectWindow : Form
    {
        public bool gotData { get; set; } 
        public ConnectWindow()
        {
            InitializeComponent();

            t_Server.Text = Settings.Default.Server;
            t_Channel.Text = Settings.Default.Channel;
            t_BotNick.Text = Settings.Default.Nick;
            t_port.Text = Settings.Default.Port;
        }

        private void b_Conect_Click(object sender, EventArgs e)
        {
            Settings.Default.Channel = t_Channel.Text;
            Settings.Default.Nick = t_BotNick.Text;
            Settings.Default.Server = t_Server.Text;
            Settings.Default.Port = t_port.Text;
            Settings.Default.Save();


            //do connect after this
            this.DialogResult=System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        private void t_Channel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Settings.Default.Channel = t_Channel.Text;
                Settings.Default.Nick = t_BotNick.Text;
                Settings.Default.Server = t_Server.Text;
                Settings.Default.Port = t_port.Text;
                Settings.Default.Save();


                //do connect after this
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void t_port_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Settings.Default.Channel = t_Channel.Text;
                Settings.Default.Nick = t_BotNick.Text;
                Settings.Default.Server = t_Server.Text;
                Settings.Default.Port = t_port.Text;
                Settings.Default.Save();


                //do connect after this
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void t_Server_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Settings.Default.Channel = t_Channel.Text;
                Settings.Default.Nick = t_BotNick.Text;
                Settings.Default.Server = t_Server.Text;
                Settings.Default.Port = t_port.Text;
                Settings.Default.Save();


                //do connect after this
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void t_BotNick_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Settings.Default.Channel = t_Channel.Text;
                Settings.Default.Nick = t_BotNick.Text;
                Settings.Default.Server = t_Server.Text;
                Settings.Default.Port = t_port.Text;
                Settings.Default.Save();


                //do connect after this
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
    }
}
