﻿using NarutoBot3.Properties;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace NarutoBot3
{
    public partial class MainWindow
    {
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        void SaveOPS()
        {
            using (StreamWriter newTask = new StreamWriter("ops.txt", false))
            {
                foreach (string op in ops)
                {
                    newTask.WriteLine(op);
                }
            }


        }
        void readOPS()
        {
            ops.Clear();
            try
            {
                StreamReader sr = new StreamReader("ops.txt");
                while (sr.Peek() >= 0)
                {
                    ops.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch
            {
            }
        }

        void SaveBAN()
        {
            using (StreamWriter newTask = new StreamWriter("banned.txt", false))
            {
                foreach (string rl in ban)
                {
                    newTask.WriteLine(rl);
                }
            }
        }
        void readBAN()
        {
            ban.Clear();
            try
            {
                StreamReader sr = new StreamReader("banned.txt");
                while (sr.Peek() >= 0)
                {
                    ban.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch
            {
            }
        }

        void SaveRLS()
        {
            using (StreamWriter newTask = new StreamWriter("rules.txt", false))
            {
                foreach (string rl in rls)
                {
                    newTask.WriteLine(rl);
                }
            }


        }
        void readRLS()
        {
            rls.Clear();
            try
            {
                StreamReader sr = new StreamReader("rules.txt");
                while (sr.Peek() >= 0)
                {
                    rls.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch
            {
            }
        }

        void SaveHLP()
        {
            using (StreamWriter newTask = new StreamWriter("help.txt", false))
            {
                foreach (string hp in hlp)
                {
                    newTask.WriteLine(hp);
                }
            }


        }
        void readHLP()
        {
            hlp.Clear();
            try
            {
                StreamReader sr = new StreamReader("help.txt");
                while (sr.Peek() >= 0)
                {
                    hlp.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch
            {
            }
        }

        void readTRI()//Reads the trivia stuff
        {
            tri.Clear();
            triviaNumber = 0;

            try
            {
                StreamReader sr = new StreamReader("trivia.txt");
                while (sr.Peek() >= 0)
                {
                    tri.Add(sr.ReadLine());
                    triviaNumber++;
                }
                sr.Close();
            }
            catch
            {
            }
        }

        void readGREET()
        {
            string nick, greeting, line;
            string[] split;
            bool enabled=false;
            bool found = false;

            if (File.Exists("greetings.txt"))
            {
                try
                {
                    StreamReader sr = new StreamReader("greetings.txt");
                    while (sr.Peek() >= 0)
                    {
                        line = sr.ReadLine();
                        split = line.Split(new char[] { ':' }, 3);
                        nick = split[0];
                        greeting = split[2];

                        switch (split[1])
                        { 
                            case "False":
                                enabled = false;
                                break;
                            case "True":
                                enabled = true;
                                break;
                            default:
                                enabled = false;
                                break;  
                        }

                        foreach (Greeting g in greet)
                        {
                            if (g.Nick == nick)
                                found = true;
                        
                        }

                        if (!found)
                        {
                            Greeting g = new Greeting(nick, greeting, enabled);
                            greet.Add(g);
                        }

                    }
                    sr.Close();
                }
                catch
                {
                }
            
            }
        }


        void addGreet(string CHANNEL, string args, string nick)
        {
            bool found = false;

            foreach (Greeting g in greet)
            {
                if (g.Nick == nick)
                {
                    found = true;
                    g.Greetingg = args;
                    SaveGreets();
                }
            }

            if (!found)
            {
                Greeting g = new Greeting(nick, args, true);
                greet.Add(g);
                SaveGreets();       
            }
  

        }

        void SaveGreets()
        {
            using (StreamWriter newTask = new StreamWriter("greetings.txt", false))
            {
                foreach (Greeting gg in greet)
                {
                    newTask.WriteLine(gg.Nick + ":" +gg.Enabled.ToString() +":" + gg.Greetingg);
                }
            }
        
        }

        void greetToogle(IrcClient c, string CHANNEL, string nick)
        {
            string message = notice(nick, "You didn't set a greeting yet");;
            string state="disabled";
            bool found = false;


            foreach (Greeting g in greet)
            {
                if (g.Nick == nick && !found)
                {
                    found = true;
                    g.Enabled = !g.Enabled;
                    if(g.Enabled) state="enabled";

                    message = notice(nick, "Your greeting is now " + state);
                    SaveGreets();  
                }
            }

            c.messageSender(message);
        }

        void loadNickGenStrings()//These are for the nick gen
        {
            lineNumber = 0;
            nickGenStrings = new List<string>();
            nickGenStrings.Clear();

            try
            {
                StreamReader sr = new StreamReader("text.txt");
                while (sr.Peek() >= 0)
                {
                    nickGenStrings.Add(sr.ReadLine());
                    lineNumber++;
                }
                sr.Close();
            }
            catch
            {
            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                if (End < 0) End = strSource.Length;
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.Length;
                return strSource.Substring(Start, End - Start);
            }
        }

        public void ChangeLabel(String message)
        {
            try
            {
                l_Status.Text = message;
            }
            catch { }
        }
        public void ChangeLabel2(String message)
        {
            if (statusStrip1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeLabel2);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                toolStripStatusLabelSilence.Text = message;

            }
        }
        public void ChangeChecked(string message)//toolStrip1
        {

            if (toolStrip1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeChecked);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (message == "true")
                    silencedToolStripMenuItem.Checked = true;
                else
                    if (message == "false")
                        silencedToolStripMenuItem.Checked = false;
            }


        }

        public void ChangeTitle(String title)
        {
            if (output2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeTitle);
                this.Invoke(d, new object[] { title });
            }
            else
            {
                this.Text = title;
            }

        }
        public void ChangeInput(String title)
        {
            if (output2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeInput);
                this.Invoke(d, new object[] { title });
            }
            else
            {
                this.input.Text = title;
            }
        }
        public void WriteMessage(String message) //Writes message on the TextBox (bot console)
        {
            if (output2.InvokeRequired)
            {
                try
                {
                    MethodInvoker invoker = () => WriteMessage(message);
                    Invoke(invoker);
                    //SetTextCallback d = new SetTextCallback(WriteMessage);
                    //this.Invoke(d, new object[] { message });
                }
                catch { }
            }
            else
            {
                this.output2.AppendText(message + "\n");
            }

            //also, should make a log

        }
        public void WriteMessage(String message, Color color) //Writes message on the TextBox (bot console)
        {
            if (output2.InvokeRequired)
            {
                try
                {
                    MethodInvoker invoker = () => WriteMessage(message, color);
                    Invoke(invoker);

                //    SetTextCallback d = new SetTextCallback(WriteMessage);
                //    this.Invoke(d, new object[] { message, color });
                }
                catch { }
            }
            else
            {
                this.output2.AppendText(message + "\n", color);
            }

            //also, should make a log

        }
        public void OutputClear(string bah)
        {
            if (output2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(OutputClear);
                this.Invoke(d, new object[] { " " });
            }
            else
            {
                var lines = this.output2.Lines;
                var newLines = lines.Skip(1);

                this.output2.Lines = newLines.ToArray();


                //this.output.Clear();
            }

        }
        public void UpdateDataSource()
        {
            if (listBox1.InvokeRequired)
            {
                ChangeDataSoure d = new ChangeDataSoure(UpdateDataSource);
                this.Invoke(d);

            }
            else
            {
                listBox1.DataSource = null;
                listBox1.DataSource = client.userList;

            }
        }

        bool isMuted(string nick)
        {
            foreach (string user in ban)
            {
                if (String.Compare(user, nick, true) == 0)
                    return true;
            }

            return false;
        }
        bool muteUser(string nick)
        {
            if (isMuted(nick))
                return false;
            else
            {
                ban.Add(nick);
                return true;
            }
        }
        bool unmuteUSer(string nick)
        {
            if (isMuted(nick))
            {
                ban.Remove(nick);
                return true;
            }
            else return false;

        }
        bool isOperator(string nick)
        {
            foreach (string user in ops)
            {
                if (String.Compare(user, nick, true) == 0)
                    return true;
            }

            return false;

        }
        bool giveOps(string nick)
        {
            if (!isOperator(nick))
            {
                ops.Add(nick);
                return true;
            }
            else return false;

        }
        bool takeOps(string nick)
        {
            if (isOperator(nick))
            {
                ops.Remove(nick);
                return true;
            }
            else return false;
        }
        public bool changeNick(string nick)
        {
            NICK = Settings.Default.Nick = nick;
            ChangeTitle(NICK + " @ " + HOME_CHANNEL + " - " + HOST + ":" + PORT);
            //do nick change to server
            if (client.isConnected)
            {
                string message = "NICK " + NICK + "\n";
                client.messageSender(message);
                return true;
            }
            else return false;
        }

        private void contextParse(string text)
        {
            string[] split = text.Split(' ');
            switch (split[0])
            {
                case "Give":
                    giveOps(split[1]);
                    SaveOPS();
                    break;
                case "Take":
                    takeOps(split[1]);
                    SaveOPS();
                    break;
                case "Mute":
                    muteUser(split[1]);
                    SaveBAN();
                    break;
                case "Unmute":
                    unmuteUSer(split[1]);
                    SaveBAN();
                    break;
                case "Poke":
                    pokeUser(split[1]);
                    SaveBAN();
                    break;
                case "Whois":
                    whoisUser(split[1]);
                    SaveBAN();
                    break;
            }
            SaveOPS();

        }
        private void pokeUser(string nick)
        {
            string message = privmsg(HOME_CHANNEL, "\x01" + "ACTION stabs " + nick + " with a sharp knife" + "\x01");
            client.messageSender(message);

        }
        private void whoisUser(string nick)
        {
            string message = "WHOIS " + nick + "\n";
            client.messageSender(message);

        }

        //UI Events

        private void connectMenuItem1_Click(object sender, EventArgs e)//Connect to...
        {
            var result = Connect.ShowDialog();

            if (result == DialogResult.OK)
            {
                //Re-do Connect!
                if (client.isConnected)
                {
                    DialogResult resultWarning;
                    resultWarning = MessageBox.Show("This bot is already connected./nDo you want to end the current connection?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        disconnect();

                        ChangeLabel("Connecting...");

                        HOME_CHANNEL = Settings.Default.Channel;
                        HOST = Settings.Default.Server;
                        NICK = Settings.Default.Nick;
                        PORT = Convert.ToInt32(Settings.Default.Port);

                        connect();
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else return;
                }
                ChangeLabel("Connecting...");

                HOME_CHANNEL = Settings.Default.Channel;
                HOST = Settings.Default.Server;
                NICK = Settings.Default.Nick;
                PORT = Convert.ToInt32(Settings.Default.Port);

                connect();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) //Quit Button
        {
            if (client.isConnected) disconnect();
            
            this.Close();
        }

        private void silencedToolStripMenuItem_Click(object sender, EventArgs e)  //Toogle Silence
        {
            if (Settings.Default.silence == true)
            {
                silencedToolStripMenuItem.Checked = false;
                Settings.Default.silence = false;
                toolStripStatusLabelSilence.Text = "";
            }
            else
            {
                silencedToolStripMenuItem.Checked = true;
                Settings.Default.silence = true;
                toolStripStatusLabelSilence.Text = "Bot is Silenced";
            }
            Settings.Default.Save();
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e) //Disconnect Button
        {
            if (client.isConnected)
                disconnect();
            ChangeLabel("Disconnected");
            //isConnected = false;
        }

        private void commandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableCommandsWindow.ShowDialog();
        }

        private void changeNickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nickWindow.ShowDialog();
            NICK = Settings.Default.Nick;
            ChangeTitle(NICK + " @ " + HOME_CHANNEL + " - " + HOST + ":" + PORT);
            //do nick change to server
            if (client.isConnected)
            {
                string message = "NICK " + NICK + "\n";
                client.messageSender(message);

            }
        }

        private void operatorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            operatorsWindow.ShowDialog();
            readOPS();
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                input.Text = lastCommand;
            }

            if (e.KeyCode == Keys.Enter)
            {
                lastCommand = input.Text;
                e.Handled = true;
                e.SuppressKeyPress = true;

                string message="";

                if (!client.isConnected) return;
                if (input.Text == "") return;

                char[] param = new char[1];
                param[0] = ' ';
                string[] parsed = input.Text.Split(param, 2); //parsed[0] is the command, parsed[1] is the rest              

                if (parsed.Length >= 2)
                {
                    parsed[0] = parsed[0].ToLower();
                    if (parsed[0] == "/me" && parsed[1] != "")  //Action send
                    {
                        message = privmsg(HOME_CHANNEL, "\x01" + "ACTION " + parsed[1] + "\x01");
                    }
                    else
                        if (parsed[0] == "/whois" && parsed[1] != "")  //Action send
                        {
                            message = "WHOIS " + parsed[1] + "\n";
                        }
                        else
                            if (parsed[0] == "/whowas" && parsed[1] != "")  //Action send
                            {
                                message = "WHOWAS " + parsed[1] + "\n";
                            }
                            else
                                if (parsed[0] == "/nick" && parsed[1] != "")  //Action send
                                {
                                    changeNick(parsed[1]);
                                }
                                else
                                    if (parsed[0] == "/query" || parsed[0] == "/pm" && parsed[1] != "" || parsed[0] == "/msg" && parsed[1] != "")  //Action send
                                    {

                                        string[] parsed2 = input.Text.Split(param, 3);
                                        if (parsed2.Length >= 3)
                                            message = privmsg(parsed2[1], parsed2[2]);
                                    }
                                    else
                                        if (parsed[0] == "/ns" || parsed[0] == "/nickserv" && parsed[1] != "")  //NickServ send
                                        {
                                            message = privmsg("NickServ", parsed[1]);
                                        }
                                        else
                                            if (parsed[0] == "/cs" || parsed[0] == "/chanserv" && parsed[1] != "")  //Chanserv send
                                            {
                                                message = privmsg("ChanServ", parsed[1]);
                                            }
                                            else
                                            {
                                                //Normal send
                                                message = privmsg(HOME_CHANNEL, input.Text);

                                            }
                }
                else
                    if (parsed[0] == "/me" || parsed[0] == "/whois" || parsed[0] == "/whowas" || 
                        parsed[0] == "/query" || parsed[0] == "/pm" || parsed[0] == "/msg" ||
                         parsed[0] == "/ns" || parsed[0] == "/nickserv" || parsed[0] == "/cs" || parsed[0] == "/chanserv")  //Action send
                    {
                        WriteMessage("Not enough arguments");
                    }
                    else
                    {
                        //Normal send
                        message = privmsg(HOME_CHANNEL, input.Text);
                    }


                client.messageSender(message);
                message = "";
                ChangeInput("");

            }
        }

        private void rulesTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rulesWindow.ShowDialog();
        }

        private void assignmentsURLToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            assignmentsWindow.Show();
        }

        private void claimsURLToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            claimsWindow.Show();
        }

        private void changeETAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            etaWindow.ShowDialog();
        }

        private void helpTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helpWindow.ShowDialog();
            readHLP();
        }

        private void mutedUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mutedWindow.ShowDialog();
            readBAN();
        }

        private void rulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readRLS();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readHLP();
        }

        private void nickGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadNickGenStrings();
        }

        private void triviaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readTRI();
        }

        private void redditCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = redditcredentials.ShowDialog();

            if (result == DialogResult.OK)
            {
                Settings.Default.redditEnabled = true;
                user = reddit.LogIn(Settings.Default.redditUser, Settings.Default.redditPass);
                Settings.Default.Save();
            }


        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            contextMenuStrip1.Items.Clear();
            string nick = listBox1.SelectedItem.ToString().Replace("@", string.Empty).Replace("+", string.Empty);

            //MenuItem gOps = new MenuItem(user,

            contextMenuStrip1.Items.Add("Give " + nick + " Ops");
            contextMenuStrip1.Items.Add("Take " + nick + " Ops");
            contextMenuStrip1.Items.Add("Mute " + nick);
            contextMenuStrip1.Items.Add("Unmute " + nick);
            contextMenuStrip1.Items.Add("Poke " + nick);
            contextMenuStrip1.Items.Add("Whois " + nick);
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.Location);
                if (listBox1.SelectedIndex != -1)
                {
                    contextMenuStrip1.Show();

                }
            }
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            contextMenuStrip1.Items.Clear();
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            contextParse(e.ClickedItem.Text);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }
        public void releaseCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = releaseChecker.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (Settings.Default.releaseEnabled)
                {
                    string message = privmsg(HOME_CHANNEL, "I'm now checking " + Settings.Default.baseURL + " for the chapter " + Settings.Default.chapterNumber + " every " + Settings.Default.checkInterval + " seconds.");
                    aTime.Interval = Settings.Default.checkInterval * 1000;
                    aTime.Elapsed += new ElapsedEventHandler(isMangaOutEvent);
                    aTime.Enabled = true;
                    client.messageSender(message);
                }
                else
                {
                    aTime.Enabled = false;
                }

            }

        }

        private void t30_Click(object sender, EventArgs e)
        {
            Settings.Default.randomTextInterval = 30;
            t30.Checked = true;
            t45.Checked = false;
            t60.Checked = false;

            rTime.Interval = Settings.Default.randomTextInterval * 60 * 1000;

        }

        private void t45_Click(object sender, EventArgs e)
        {
            Settings.Default.randomTextInterval = 45;
            t30.Checked = false;
            t45.Checked = true;
            t60.Checked = false;

            rTime.Interval = Settings.Default.randomTextInterval * 60 * 1000;
        }

        private void t60_Click(object sender, EventArgs e)
        {
            Settings.Default.randomTextInterval = 60;
            t30.Checked = false;
            t45.Checked = false;
            t60.Checked = true;

            rTime.Interval = Settings.Default.randomTextInterval * 60 * 1000;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            animeAPI.ShowDialog();
            if (Settings.Default.cxKey.Length < 5 || Settings.Default.apikey.Length < 5)
            {
                Settings.Default.aniSearchEnabled = false;
                Settings.Default.timeEnabled = false;
                Settings.Default.Save();
            }
        }

        //private char getUserMode(string user)
        //{
        //    foreach (string u in userList)
        //    {
        //        if (u.Contains(user))
        //        {
        //            switch (u.Substring(0, 1))
        //            {
        //                case "@":
        //                    return '@';
        //                case "+":
        //                    return '+';
        //                case "%":
        //                    return '%';
        //                case "~":
        //                    return '~';
        //                case "&":
        //                    return '&';
        //                default:
        //                    return '0';
        //            }
        //        }
        //    }
        //    return '0';
        //}
        public string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        public string privmsg(string destinatary, string message)
        {
            string result;

            result = "PRIVMSG " + destinatary + " :" + message + "\r\n";

            if (NICK.Length > 15)
                WriteMessage(NICK.Truncate(16) + ":" + message);
            else if (NICK.Length >= 8)                       //Write the message on the bot console
                WriteMessage(NICK + "\t: " + message);
            else
                WriteMessage(NICK + "\t\t: " + message);

            return result;
        }

        public string notice(string destinatary, string message)
        {
            string result;

            result = "NOTICE " + destinatary + " :" + message + "\r\n";

            if (NICK.Length > 15)
                WriteMessage(NICK.Truncate(16) + ":" + message);
            else if (NICK.Length >= 8)                       //Write the message on the bot console
                WriteMessage(NICK + "\t: " + message);
            else
                WriteMessage(NICK + "\t\t: " + message);

            return result;
        }
        private void userJoined(object sender, EventArgs e, string whoJoined)
        {
            foreach (Greeting g in greet)
            {
                if (g.Nick == whoJoined.Replace("@", string.Empty).Replace("+", string.Empty) && g.Enabled == true)
                {
                    string message = privmsg(HOME_CHANNEL, g.Greetingg);
                    client.messageSender(message);
                }
            }

            WriteMessage("** " + whoJoined + " joined", Color.Green);
            UpdateDataSource();
        }

        private void userLeft(object sender, EventArgs e, string whoLeft)
        {
            WriteMessage("** " + whoLeft + " parted", Color.Red);
            UpdateDataSource();
        }
        private void userNickChange(object sender, EventArgs e, string whoJoined, string newNick)
        {
            WriteMessage("** " + whoJoined + " is now known as " + newNick, Color.Yellow);
            UpdateDataSource();
        }

        private void userModeChanged(object sender, EventArgs e, string user, string mode)
        {

            switch (mode)
            {
                case ("+o"):
                    WriteMessage("** " + user + " was opped", Color.Blue);
                    break;
                case ("-o"):
                    WriteMessage("** " + user + " was deopped", Color.Blue);
                    break;
                case ("+v"):
                    WriteMessage("** " + user + " was voiced", Color.Blue);
                    break;
                case ("-v"):
                    WriteMessage("** " + user + " was devoiced", Color.Blue);
                    break;
                case ("+h"):
                    WriteMessage("** " + user + " was half opped", Color.Blue);
                    break;
                case ("-h"):
                    WriteMessage("** " + user + " was half deopped", Color.Blue);
                    break;
                case ("+q"):
                    WriteMessage("** " + user + " was given Owner permissions", Color.Blue);
                    break;
                case ("-q"):
                    WriteMessage("** " + user + " was removed as a Owner", Color.Blue);
                    break;
                case ("+a"):
                    WriteMessage("** " + user + " was given Admin permissions", Color.Blue);
                    break;
                case ("-a"):
                    WriteMessage("** " + user + " was removed as an Admin", Color.Blue);
                    break;
            }

            UpdateDataSource();
        }

        private void userKicked(object sender, EventArgs e, string userkicked)
        {
            WriteMessage("** " + userkicked + " was kicked", Color.Red);
            UpdateDataSource();
        }

        private void nowConnected(object sender, EventArgs e)
        {
            //isConnected = true;
            ChangeLabel("Connected");
            client.Join();
            ChangeTitle(NICK + " @ " + HOME_CHANNEL + " - " + HOST + ":" + PORT);
        }

        private void userListCreated(object sender, EventArgs e)
        {
            UpdateDataSource();
        
        }



    }
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }


}
