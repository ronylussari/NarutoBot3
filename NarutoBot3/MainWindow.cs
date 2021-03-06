﻿using NarutoBot3.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace NarutoBot3
{
    public partial class MainWindow : Form
    {
        delegate void SetTextCallback(string text);
        delegate void SetBoolCallback(bool status);
        delegate void ChangeDataSource();

        searchAnimeAPI animeAPI = new searchAnimeAPI();
        ConnectWindow Connect = new ConnectWindow();
        enabledCommands enableCommandsWindow = new enabledCommands();
        Assignments assignmentsWindow = new Assignments();
        claims claimsWindow = new claims();
        nick nickWindow = new nick();
        operators operatorsWindow = new operators();
        rules rulesWindow = new rules();
        help helpWindow = new help();
        eta etaWindow = new eta();
        muted mutedWindow = new muted();
        RedditCredentials redditcredentials = new RedditCredentials();
        RleaseChecker releaseChecker = new RleaseChecker();


        Bot ircBot;
        public IrcClient client;

        System.Timers.Timer aTime;      //To check for manga releases
        System.Timers.Timer rTime;      //To check for random text
        System.Timers.Timer dTime;      //To check for connection lost

        string HOME_CHANNEL;
        string HOST;
        string NICK;
        int PORT;

        String line;

        string lastCommand;

        bool exitTheLoop = false;

        BackgroundWorker backgroundWorker1 = new BackgroundWorker();


        public void loadSettings()
        {
            switch (Settings.Default.randomTextInterval)
            {
                case 30:
                    t30.Checked = true;
                    t45.Checked = false;
                    t60.Checked = false;
                    break;
                case 45:
                    t30.Checked = false;
                    t45.Checked = true;
                    t60.Checked = false;
                    break;
                case 60:
                    t30.Checked = false;
                    t45.Checked = false;
                    t60.Checked = true;
                    break;
                default:
                    Settings.Default.randomTextInterval = 30;
                    t30.Checked = true;
                    t45.Checked = false;
                    t60.Checked = false;
                    break;
            }


            if (Settings.Default.cxKey.Length < 5 || Settings.Default.apikey.Length < 5 )
            {
                Settings.Default.aniSearchEnabled = false;
                Settings.Default.timeEnabled = false;
            }

            if(Settings.Default.malPass.Length < 2 || Settings.Default.malUser.Length < 2)
                Settings.Default.aniSearchEnabled = false;


            if (String.IsNullOrEmpty(Settings.Default.redditUser) || String.IsNullOrEmpty(Settings.Default.redditPass))
                Settings.Default.redditEnabled = false;

           

            Settings.Default.Save();

            aTime = new System.Timers.Timer(Settings.Default.checkInterval);
            aTime.Enabled = false;

            rTime = new System.Timers.Timer(Settings.Default.randomTextInterval * 60 * 1000);
            rTime.Enabled = Settings.Default.randomTextEnabled;
            rTime.Elapsed += (sender, e) => randomTextSender(sender, e);

            dTime = new System.Timers.Timer(Settings.Default.timeOutTimeInterval * 1000);
            dTime.Enabled = true;
            dTime.Elapsed += new ElapsedEventHandler(pingServer);

            Settings.Default.releaseEnabled = false;

            if (Settings.Default.silence == true)
            {
                silencedToolStripMenuItem.Checked = true;
                toolStripStatusLabelSilence.Text = "Bot is Silenced";
            }
            else
            {
                silencedToolStripMenuItem.Checked = false;
                toolStripStatusLabelSilence.Text = "";
            }

            HOME_CHANNEL = Settings.Default.Channel;
            HOST = Settings.Default.Server;
            NICK = Settings.Default.Nick;
            PORT = Convert.ToInt32(Settings.Default.Port);

            //bot.LoadSettings();
        }

        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker_MainBotCycle);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker1.WorkerSupportsCancellation = true;

            lastCommand = "";

            var result = Connect.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (connect()) //If connected with sfuccess, then start the bot
                {
                    exitTheLoop = false;
                    backgroundWorker1.RunWorkerAsync();
                    //isConnected = true;
                }
                else
                    MessageBox.Show("Connection Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool connect()//This is where the bot connects to the server and logs in
        {
            ChangeConnectingLabel("Connecting...");

            loadSettings();
            client = new IrcClient(HOME_CHANNEL, HOST, PORT, NICK);


            if (client.Connect())
            {
                exitTheLoop = false;
                return true;
            }
            else return false;
        }

        public void backgroundWorker_MainBotCycle(object sender, DoWorkEventArgs e)
        {
            //Main Loop
            String buffer;

            ircBot = new Bot(ref client, ref OutputBox);

            ircBot.Connected += new EventHandler<EventArgs>(nowConnected);
            ircBot.ConnectedWithServer += new EventHandler<EventArgs>(nowConnectedWithServer);

            ircBot.Created += new EventHandler<EventArgs>(userListCreated);
            ircBot.Joined += (senderr, ee) => userJoined(senderr, ee, ircBot.Who);
            ircBot.Left += (senderr, ee) => userLeft(senderr, ee, ircBot.Wholeft);
            ircBot.NickChanged += (senderr, ee) => userNickChange(senderr, ee, ircBot.Who, ircBot.NewNick);
            ircBot.Kicked += (senderr, ee) => userKicked(senderr, ee, ircBot.Who);
            ircBot.ModeChanged += (senderr, ee) => userModeChanged(senderr, ee, ircBot.Who, ircBot.Mode);

            ircBot.Timeout += new EventHandler<EventArgs>(timeout);

            ircBot.BotNickChanged += (senderr, ee) => eventChangeTitle(senderr, ee);

            ircBot.BotSilenced += new EventHandler<EventArgs>(botSilence);
            ircBot.BotUnsilenced += new EventHandler<EventArgs>(botUnsilence);

            ircBot.Quit += new EventHandler<EventArgs>(letsQuit);

            ircBot.DuplicatedNick += new EventHandler<EventArgs>(duplicatedNick);

            ircBot.PongReceived += (senderr, ee) => updateLag(sender, ee, ircBot.timeDifference);

            ircBot.LoadSettings();

            while (!exitTheLoop)
            { 
                buffer = "";
                try
                {
                    buffer = client.messageReader();
                    byte[] bytes = Encoding.Default.GetBytes(buffer);
                    line = Encoding.UTF8.GetString(bytes);

                    ircBot.BotMessage(line);
                }
                catch
                { }
            }
        }

        private void disconnect()
        {
            ChangeConnectingLabel("Disconnecting...");

            client.Disconnect();
            
            Thread.Sleep(250);

            UpdateDataSource();

            OutputClean();
            
            ChangeTitle("NarutoBot");

            exitTheLoop = true;
            
        }

        
        public void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeConnectingLabel("Disconnected");
        }

        public void isMangaOutEvent(object source, ElapsedEventArgs e)
        {
            String readHtml;
            string url = Settings.Default.baseURL.TrimEnd('/') + "/" + Settings.Default.chapterNumber;
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            HttpWebRequest request;

            if (!Settings.Default.releaseEnabled) return;

            try
            {
                //readHtml = webClient.DownloadString(url);
                request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.MaximumAutomaticRedirections = 4;
                request.MaximumResponseHeadersLength = 4;
                request.Timeout = 7 * 1000;   //7 seconds
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                readHtml = readStream.ReadToEnd();
            }
            catch       //Error
            {
                return;
            }

            if (!readHtml.Contains("is not released yet."))//Not yet
            {
                string message;

                message = privmsg(HOME_CHANNEL, "*");
                client.messageSender(message);
                message = privmsg(HOME_CHANNEL, "\x02" + "\x030,4Chapter " + Settings.Default.chapterNumber.ToString() + " appears to be out! \x030,4" + url + " [I'm a bot, so i can be wrong!]" + "\x02");
                client.messageSender(message);
                message = privmsg(HOME_CHANNEL, "*");
                client.messageSender(message);

                Settings.Default.releaseEnabled = false;
                Settings.Default.Save();

                aTime.Enabled = false;
            }

        }
        

        private void output2_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process myProcess = new Process();
            string url = e.LinkText;
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = url;
            myProcess.Start();

            myProcess.Close();
        }



        private void killToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ircBot.ReadKills();
        }


        public void ChangeConnectingLabel(String message)
        {
            try
            {
                l_Status.Text = message;
            }
            catch { }
        }
        public void ChangeSilenceLabel(String message)
        {
            if (statusStripBottom.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeSilenceLabel);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                toolStripStatusLabelSilence.Text = message;

            }
        }
        public void ChangeSilenceCheckBox(bool status)//toolStrip1
        {

            if (toolStripMenu.InvokeRequired)
            {
                SetBoolCallback d = new SetBoolCallback(ChangeSilenceCheckBox);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                silencedToolStripMenuItem.Checked = status;

            }
        }

        public void ChangeTitle(String title)
        {
            if (this.InvokeRequired)
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
            if (InputBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ChangeInput);
                this.Invoke(d, new object[] { title });
            }
            else
            {
                this.InputBox.Text = title;
            }
        }
        public void WriteMessage(String message) //Writes message on the TextBox (bot console)
        {
            if (OutputBox.InvokeRequired)
            {
                try
                {
                    MethodInvoker invoker = () => WriteMessage(message);
                    Invoke(invoker);
                }
                catch { }
            }
            else
            {
                this.OutputBox.AppendText(message + "\n");

            }

            //also, should make a log

        }
        public void WriteMessage(String message, Color color) //Writes message on the TextBox (bot console)
        {
            if (OutputBox.InvokeRequired)
            {
                try
                {
                    MethodInvoker invoker = () => WriteMessage(message, color);
                    Invoke(invoker);
                }
                catch { }
            }
            else
            {
                this.OutputBox.AppendText(message + "\n", color);
            }

            //also, should make a log

        }

        public void OutputClean()
        {
            if (OutputBox.InvokeRequired)
            {
                try
                {
                    MethodInvoker invoker = () => OutputClean();
                    Invoke(invoker);
                }
                catch { }
            }
            else
            {
                OutputBox.Clear();
            }
        }

        public void UpdateDataSource()
        {
            if (InterfaceUserList.InvokeRequired)
            {
                ChangeDataSource d = new ChangeDataSource(UpdateDataSource);
                this.Invoke(d);
            }
            else
            {
                client.userList.Sort();
                InterfaceUserList.DataSource = null;
                InterfaceUserList.DataSource = client.userList;
            }
        }


        private void contextParse(string text)
        {
            string[] split = text.Split(' ');
            switch (split[0])
            {
                case "Give":
                    ircBot.giveOps(split[1]);
                    ircBot.SaveOps();
                    break;
                case "Take":
                    ircBot.takeOps(split[1]);
                    ircBot.SaveOps();
                    break;
                case "Mute":
                    ircBot.muteUser(split[1]);
                    ircBot.SaveBan();
                    break;
                case "Unmute":
                    ircBot.unmuteUSer(split[1]);
                    ircBot.SaveBan();
                    break;
                case "Poke":
                    ircBot.pokeUser(split[1]);
                    break;
                case "Whois":
                    ircBot.whoisUser(split[1]);
                    break;
                case "Kick":
                    ircBot.kickUser(split[1]);
                    break;
            }
            ircBot.SaveOps();
        }

        //UI Events

        private void connectMenuItem1_Click(object sender, EventArgs e)//Connect to...
        {
            var result = Connect.ShowDialog();

            if (result == DialogResult.OK)
            {
                //Re-do Connect!
                if (client != null)
                {
                    if (client.isConnected)
                    {
                        DialogResult resultWarning;
                        resultWarning = MessageBox.Show("This bot is already connected./nDo you want to end the current connection?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                        if (resultWarning == System.Windows.Forms.DialogResult.OK)
                        {
                            disconnect();
                            Thread.Sleep(250);

                            ChangeConnectingLabel("Connecting...");

                            if (connect())//If connected with success, then start the bot
                            {
                                backgroundWorker1.RunWorkerAsync();
                            }
                            else
                            {
                                MessageBox.Show("Connection Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ChangeConnectingLabel("Disconnected");
                            }
                        }
                        else return;
                    }
                    else
                    {
                        ChangeConnectingLabel("Connecting...");

                        if (connect())//If connected with success, then start the bot
                        {
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            MessageBox.Show("Connection Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ChangeConnectingLabel("Disconnected");
                        }
                    }
                }
                else
                {
                    ChangeConnectingLabel("Connecting...");

                    HOME_CHANNEL = Settings.Default.Channel;
                    HOST = Settings.Default.Server;
                    NICK = Settings.Default.Nick;
                    PORT = Convert.ToInt32(Settings.Default.Port);

                    if (connect())//If connected with success, then start the bot
                    {
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Connection Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ChangeConnectingLabel("Disconnected");
                    }
                }
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
        }

        private void commandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableCommandsWindow.ShowDialog();
        }

        private void changeNickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nickWindow.ShowDialog();
            ircBot.changeNick(Settings.Default.Nick);
        }

        private void operatorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            operatorsWindow.ShowDialog();
            ircBot.ReadOps();
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                InputBox.Text = lastCommand;
            }

            if (e.KeyCode == Keys.Enter)
            {
                lastCommand = InputBox.Text;
                e.Handled = true;
                e.SuppressKeyPress = true;

                string message = "";

                if (!client.isConnected) return;
                if (String.IsNullOrEmpty(InputBox.Text)) return;

                char[] param = new char[1];
                param[0] = ' ';
                string[] parsed = InputBox.Text.Split(param, 2); //parsed[0] is the command, parsed[1] is the rest              

                if (parsed.Length >= 2)
                {
                    if (!String.IsNullOrEmpty(parsed[1]))
                    {
                        parsed[0] = parsed[0].ToLower();
                        if (parsed[0] == "/me")  //Action send
                        {
                            message = privmsg(HOME_CHANNEL, "\x01" + "ACTION " + parsed[1] + "\x01");
                        }
                        else
                            if (parsed[0] == "/whois" )  //Action send
                            {
                                message = "WHOIS " + parsed[1] + "\n";
                            }
                            else
                                if (parsed[0] == "/whowas" )  //Action send
                                {
                                    message = "WHOWAS " + parsed[1] + "\n";
                                }
                                else
                                    if (parsed[0] == "/nick" )  //Action send
                                    {
                                        changeNick(parsed[1]);
                                    }
                                    else
                                        if (parsed[0] == "/query" || parsed[0] == "/pm" || parsed[0] == "/msg" )  //Action send
                                        {

                                            string[] parsed2 = InputBox.Text.Split(param, 3);
                                            if (parsed2.Length >= 3)
                                                message = privmsg(parsed2[1], parsed2[2]);
                                        }
                                        else
                                            if (parsed[0] == "/ns" || parsed[0] == "/nickserv" )  //NickServ send
                                            {
                                                message = privmsg("NickServ", parsed[1]);
                                            }
                                            else
                                                if (parsed[0] == "/cs" || parsed[0] == "/chanserv")  //Chanserv send
                                                {
                                                    message = privmsg("ChanServ", parsed[1]);
                                                }
                                                else
                                                {
                                                    //Normal send
                                                    if (String.IsNullOrWhiteSpace(parsed[0]))                                             
                                                        message = privmsg(HOME_CHANNEL, InputBox.Text);
                                                    else if (parsed[0][0] != '/')
                                                    {
                                                        message = privmsg(HOME_CHANNEL, InputBox.Text);
                                                    }
                                                }
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
                        if (parsed[0][0] != '/')
                            message = privmsg(HOME_CHANNEL, InputBox.Text);
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
            ircBot.ReadHelp();
        }

        private void mutedUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mutedWindow.ShowDialog();
            ircBot.ReadBan();
        }

        private void rulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ircBot.ReadRules();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ircBot.ReadHelp();
        }

        private void nickGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ircBot.LoadNickGenStrings();
        }

        private void triviaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ircBot.ReadTrivia();
        }

        private void redditCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = redditcredentials.ShowDialog();

            if (result == DialogResult.OK)
            {
                Settings.Default.redditEnabled = true;
                ircBot.user = ircBot.reddit.LogIn(Settings.Default.redditUser, Settings.Default.redditPass);
                Settings.Default.Save();
            }


        }

        private void botSilence(object sender, EventArgs e)
        {
            ChangeSilenceCheckBox(true);
            Settings.Default.silence = true;
            ChangeSilenceLabel("Bot is Silenced");
            Settings.Default.Save();
        }

        private void timeout(object sender, EventArgs e)
        {
            try { disconnect(); }
            catch { }

            ChangeConnectingLabel("Re-Connecting...");
            WriteMessage("* The connection timed out. Will try to reconnect.");

            if (connect())//If connected with success, then start the bot
            {
                exitTheLoop = false;
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Connection Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ChangeConnectingLabel("Disconnected");
            }
        }

        private void botUnsilence(object sender, EventArgs e)
        {
            ChangeSilenceCheckBox(false);
            Settings.Default.silence = false;
            ChangeSilenceLabel("");

            Settings.Default.Save();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            contextMenuUserList.Items.Clear();
            string nick = InterfaceUserList.SelectedItem.ToString().Replace("@", string.Empty).Replace("+", string.Empty);

            contextMenuUserList.Items.Add("Give " + nick + " Ops");
            contextMenuUserList.Items.Add("Take " + nick + " Ops");
            contextMenuUserList.Items.Add("Mute " + nick);
            contextMenuUserList.Items.Add("Unmute " + nick);
            contextMenuUserList.Items.Add("Poke " + nick);
            contextMenuUserList.Items.Add("Whois " + nick);
            contextMenuUserList.Items.Add("Kick " + nick + " (if operator)");
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                InterfaceUserList.SelectedIndex = InterfaceUserList.IndexFromPoint(e.Location);
                if (InterfaceUserList.SelectedIndex != -1)
                {
                    contextMenuUserList.Show();

                }
            }
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            contextMenuUserList.Items.Clear();
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
                    string message = privmsg(HOME_CHANNEL, "I'm now checking " + Settings.Default.baseURL + Settings.Default.chapterNumber + " for the chapter every " + Settings.Default.checkInterval + " seconds.");
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

        private void userJoined(object sender, EventArgs e, string whoJoined)
        {
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
            ChangeConnectingLabel("Connected");
            client.Join();
            ChangeTitle(NICK + " @ " + HOME_CHANNEL + " - " + HOST + ":" + PORT);
        }
        private void nowConnectedWithServer(object sender, EventArgs e)
        {
            ChangeTitle(NICK + " @ " + HOME_CHANNEL + " - " + HOST + ":" + PORT + " (" + client.HOST_SERVER + ")");
        }

        private void userListCreated(object sender, EventArgs e)
        {

            UpdateDataSource();
        }
        public void randomTextSender(object source, ElapsedEventArgs e)
        {
            ircBot.randomTextSender(source, e);
        }

        public void eventChangeTitle(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(client.HOST_SERVER))
                ChangeTitle(client.NICK + " @ " + client.HOME_CHANNEL + " - " + client.HOST + ":" + client.PORT + " (" + client.HOST_SERVER + ")");
            else
                ChangeTitle(client.NICK + " @ " + client.HOME_CHANNEL + " - " + client.HOST + ":" + client.PORT);
        }

        public void letsQuit(object sender, EventArgs e)
        {
            disconnect();

        }

        public bool changeNick(string nick)
        {
            client.NICK = Settings.Default.Nick = nick;

            if (!String.IsNullOrEmpty(client.HOST_SERVER))
                ChangeTitle(client.NICK + " @ " + client.HOME_CHANNEL + " - " + client.HOST + ":" + client.PORT + " (" + client.HOST_SERVER + ")");
            else
                ChangeTitle(client.NICK + " @ " + client.HOME_CHANNEL + " - " + client.HOST + ":" + client.PORT);



            //do nick change to server
            if (client.isConnected)
            {
                string message = "NICK " + client.NICK + "\n";
                client.messageSender(message);
                return true;
            }
            else return false;
        }

        public void duplicatedNick(object sender, EventArgs e)
        {
            Random r = new Random();

            disconnect();

            Settings.Default.Nick = client.NICK + r.Next(10);
            Settings.Default.Save();


            if (connect())//If connected with success, then start the bot
            {

                backgroundWorker1.RunWorkerAsync();
                //isConnected = true;
            }

        }
        /** method stolen from an SO thread. sorry can't remember the author **/
        static void AddAddress(string address, string domain, string user)
        {
            string args = string.Format(@"http add urlacl url={0}", address) + " user=\"" + domain + "\\" + user + "\"";

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }

        private void pingServer(object sender, EventArgs e)
        {
            ircBot.pingSever();
        }

        private void updateLag(object sender, EventArgs e, TimeSpan diff)
        {
            try
            {
                int seconds = diff.Seconds * 60 + diff.Seconds;
                toolstripLag.Text = seconds + "." + diff.Milliseconds.ToString("000") +"s";
            }
            catch { }
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
