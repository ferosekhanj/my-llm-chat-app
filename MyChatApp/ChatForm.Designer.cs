namespace MyChatApp
{
    partial class ChatForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            splitContainer1 = new SplitContainer();
            groupBox3 = new GroupBox();
            chatHistory = new ListBox();
            splitContainer2 = new SplitContainer();
            groupBox1 = new GroupBox();
            chatContent = new Microsoft.Web.WebView2.WinForms.WebView2();
            splitContainer3 = new SplitContainer();
            groupBox2 = new GroupBox();
            userMessage = new TextBox();
            btnAbout = new Button();
            chkUseTools = new CheckBox();
            btnAttach = new Button();
            chkStreaming = new CheckBox();
            btnNewChat = new Button();
            btnSend = new Button();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            modelCombo = new ToolStripComboBox();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel2 = new ToolStripLabel();
            toolsDropDown = new ToolStripDropDownButton();
            toolsProgress = new ToolStripProgressBar();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripLabel3 = new ToolStripLabel();
            statusText = new ToolStripLabel();
            openFileDialog1 = new OpenFileDialog();
            panel1 = new Panel();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chatContent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            groupBox2.SuspendLayout();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(groupBox3);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1178, 661);
            splitContainer1.SplitterDistance = 160;
            splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(chatHistory);
            groupBox3.Dock = DockStyle.Fill;
            groupBox3.Location = new Point(0, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(160, 661);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Chat HIstory";
            // 
            // chatHistory
            // 
            chatHistory.Dock = DockStyle.Fill;
            chatHistory.FormattingEnabled = true;
            chatHistory.Location = new Point(3, 19);
            chatHistory.Name = "chatHistory";
            chatHistory.Size = new Size(154, 639);
            chatHistory.TabIndex = 0;
            chatHistory.SelectedIndexChanged += chatHistory_SelectedIndexChanged;
            chatHistory.SelectedValueChanged += chatHistory_SelectedValueChanged;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer3);
            splitContainer2.Size = new Size(1014, 661);
            splitContainer2.SplitterDistance = 503;
            splitContainer2.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chatContent);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1014, 503);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Chat Console";
            // 
            // chatContent
            // 
            chatContent.AllowExternalDrop = true;
            chatContent.CreationProperties = null;
            chatContent.DefaultBackgroundColor = Color.White;
            chatContent.Dock = DockStyle.Fill;
            chatContent.Location = new Point(3, 19);
            chatContent.Name = "chatContent";
            chatContent.Size = new Size(1008, 481);
            chatContent.TabIndex = 0;
            chatContent.ZoomFactor = 1D;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(groupBox2);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(btnAbout);
            splitContainer3.Panel2.Controls.Add(chkUseTools);
            splitContainer3.Panel2.Controls.Add(btnAttach);
            splitContainer3.Panel2.Controls.Add(chkStreaming);
            splitContainer3.Panel2.Controls.Add(btnNewChat);
            splitContainer3.Panel2.Controls.Add(btnSend);
            splitContainer3.Size = new Size(1014, 154);
            splitContainer3.SplitterDistance = 807;
            splitContainer3.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(userMessage);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(0, 0);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(807, 154);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Type your message";
            // 
            // userMessage
            // 
            userMessage.Dock = DockStyle.Fill;
            userMessage.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            userMessage.Location = new Point(3, 19);
            userMessage.Multiline = true;
            userMessage.Name = "userMessage";
            userMessage.Size = new Size(801, 132);
            userMessage.TabIndex = 0;
            userMessage.KeyDown += userMessage_KeyDown;
            // 
            // btnAbout
            // 
            btnAbout.Location = new Point(29, 113);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(53, 23);
            btnAbout.TabIndex = 6;
            btnAbout.Text = "About";
            btnAbout.UseVisualStyleBackColor = true;
            btnAbout.Click += btnAbout_Click;
            // 
            // chkUseTools
            // 
            chkUseTools.AutoSize = true;
            chkUseTools.Location = new Point(27, 46);
            chkUseTools.Name = "chkUseTools";
            chkUseTools.Size = new Size(75, 19);
            chkUseTools.TabIndex = 5;
            chkUseTools.Text = "Use Tools";
            chkUseTools.UseVisualStyleBackColor = true;
            // 
            // btnAttach
            // 
            btnAttach.Location = new Point(144, 80);
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(31, 23);
            btnAttach.TabIndex = 4;
            btnAttach.Text = "+";
            btnAttach.UseVisualStyleBackColor = true;
            btnAttach.Click += btnAttach_Click;
            // 
            // chkStreaming
            // 
            chkStreaming.AutoSize = true;
            chkStreaming.Checked = true;
            chkStreaming.CheckState = CheckState.Checked;
            chkStreaming.Location = new Point(26, 22);
            chkStreaming.Name = "chkStreaming";
            chkStreaming.Size = new Size(133, 19);
            chkStreaming.TabIndex = 2;
            chkStreaming.Text = "Streaming Response";
            chkStreaming.UseVisualStyleBackColor = true;
            // 
            // btnNewChat
            // 
            btnNewChat.Location = new Point(85, 80);
            btnNewChat.Name = "btnNewChat";
            btnNewChat.Size = new Size(53, 23);
            btnNewChat.TabIndex = 1;
            btnNewChat.Text = "New";
            btnNewChat.UseVisualStyleBackColor = true;
            btnNewChat.Click += btnNewChat_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(26, 80);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(53, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.Bottom;
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, modelCombo, toolStripSeparator1, toolStripLabel2, toolsDropDown, toolsProgress, toolStripSeparator2, toolStripLabel3, statusText });
            toolStrip1.Location = new Point(0, 661);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1178, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(46, 22);
            toolStripLabel1.Text = "Models";
            // 
            // modelCombo
            // 
            modelCombo.Name = "modelCombo";
            modelCombo.Size = new Size(121, 25);
            modelCombo.SelectedChanged += modelCombo_SelectedChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(66, 22);
            toolStripLabel2.Text = "Tools(mcp)";
            // 
            // toolsDropDown
            // 
            toolsDropDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolsDropDown.Image = (Image)resources.GetObject("toolsDropDown.Image");
            toolsDropDown.ImageTransparentColor = Color.Magenta;
            toolsDropDown.Name = "toolsDropDown";
            toolsDropDown.Size = new Size(83, 22);
            toolsDropDown.Text = "Select Items";
            // 
            // toolsProgress
            // 
            toolsProgress.Name = "toolsProgress";
            toolsProgress.Size = new Size(100, 22);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // toolStripLabel3
            // 
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new Size(42, 22);
            toolStripLabel3.Text = "Status:";
            // 
            // statusText
            // 
            statusText.Name = "statusText";
            statusText.Size = new Size(76, 22);
            statusText.Text = "Please Wait...";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1178, 661);
            panel1.TabIndex = 2;
            // 
            // timer1
            // 
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1178, 686);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Name = "ChatForm";
            Text = "JFK LLM Chat";
            FormClosing += ChatForm_FormClosing;
            Load += Form1_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chatContent).EndInit();
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainer3;
        private TextBox userMessage;
        private Button btnSend;
        private Button btnNewChat;
        private ListBox chatHistory;
        private Microsoft.Web.WebView2.WinForms.WebView2 chatContent;
        private ToolStrip toolStrip1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel statusText;
        private ToolStripProgressBar toolsProgress;
        private CheckBox chkStreaming;
        private OpenFileDialog openFileDialog1;
        private Button btnAttach;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox modelCombo;
        private ToolStripLabel toolStripLabel2;
        private CheckBox chkUseTools;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel toolStripLabel3;
        private ToolStripDropDownButton toolsDropDown;
        private Button btnAbout;
        private Panel panel1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private System.Windows.Forms.Timer timer1;
    }
}
