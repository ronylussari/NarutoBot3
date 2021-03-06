﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NarutoBot3.Properties;

namespace NarutoBot3
{
    public partial class operators : Form
    {
        static List<string> ops = new List<string>();
        static List<string> tmp = new List<string>();

        bool foundRepeated = false;

        public operators()
        {
            InitializeComponent();
            readOPS(); 
        }

        public void readOPS()
        {
            ops.Clear();
            StreamReader sr = new StreamReader("ops.txt");
            try
            {
                while (sr.Peek() >= 0)
                {
                    ops.Add(sr.ReadLine());
                }
            }
            catch
            {
            }

            sr.Close();

            
            listOperators.DataSource = null;
            listOperators.DataSource = ops;
        }

        static private void SaveOPS()
        {
            using (StreamWriter newTask = new StreamWriter("ops.txt", false))
            {
                foreach (string op in ops)
                {
                    newTask.WriteLine(op);
                }
            }
        
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (string op in ops)
            {
                if (op == t_opeartor.Text)
                {
                    foundRepeated = true;
                }
            
            }
            if (!foundRepeated)
            {
                ops.Add(t_opeartor.Text);
                listOperators.DataSource = ops;
                SaveOPS();
                readOPS();
            }
            else
                MessageBox.Show("User is already an operator", "Duplicated", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tmp.Clear();
            foreach (string op in ops)
            {
                if (op!=listOperators.SelectedItem.ToString())
                {
                    tmp.Add(op);
                
                }
            }

            ops.Clear();
            foreach (string op2 in tmp)
            {
                ops.Add(op2);
            }

            SaveOPS();
            readOPS();
        }

        private void operators_Shown(object sender, EventArgs e)
        {
            readOPS();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            t_opeartor.Text = "";
            this.Close();
        }
    }
}
