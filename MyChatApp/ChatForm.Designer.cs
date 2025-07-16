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
            splitContainer1 = new SplitContainer();
            chatHistory = new ListBox();
            splitContainer2 = new SplitContainer();
            chatContent = new Microsoft.Web.WebView2.WinForms.WebView2();
            splitContainer3 = new SplitContainer();
            userMessage = new TextBox();
            chkStreaming = new CheckBox();
            btnNewChat = new Button();
            btnSend = new Button();
            toolStrip1 = new ToolStrip();
            toolsProgress = new ToolStripProgressBar();
            toolStripSeparator1 = new ToolStripSeparator();
            toolsCombo = new ToolStripComboBox();
            statusText = new ToolStripLabel();
            cmbModels = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chatContent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            toolStrip1.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(chatHistory);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1088, 622);
            splitContainer1.SplitterDistance = 148;
            splitContainer1.TabIndex = 0;
            // 
            // chatHistory
            // 
            chatHistory.Dock = DockStyle.Fill;
            chatHistory.FormattingEnabled = true;
            chatHistory.Location = new Point(0, 0);
            chatHistory.Name = "chatHistory";
            chatHistory.Size = new Size(148, 622);
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
            splitContainer2.Panel1.Controls.Add(chatContent);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer3);
            splitContainer2.Size = new Size(936, 622);
            splitContainer2.SplitterDistance = 487;
            splitContainer2.TabIndex = 0;
            // 
            // chatContent
            // 
            chatContent.AllowExternalDrop = true;
            chatContent.CreationProperties = null;
            chatContent.DefaultBackgroundColor = Color.White;
            chatContent.Dock = DockStyle.Fill;
            chatContent.Location = new Point(0, 0);
            chatContent.Name = "chatContent";
            chatContent.Size = new Size(936, 487);
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
            splitContainer3.Panel1.Controls.Add(userMessage);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(cmbModels);
            splitContainer3.Panel2.Controls.Add(chkStreaming);
            splitContainer3.Panel2.Controls.Add(btnNewChat);
            splitContainer3.Panel2.Controls.Add(btnSend);
            splitContainer3.Size = new Size(936, 131);
            splitContainer3.SplitterDistance = 745;
            splitContainer3.TabIndex = 0;
            // 
            // userMessage
            // 
            userMessage.Dock = DockStyle.Fill;
            userMessage.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            userMessage.Location = new Point(0, 0);
            userMessage.Multiline = true;
            userMessage.Name = "userMessage";
            userMessage.Size = new Size(745, 131);
            userMessage.TabIndex = 0;
            userMessage.KeyDown += userMessage_KeyDown;
            // 
            // chkStreaming
            // 
            chkStreaming.AutoSize = true;
            chkStreaming.Checked = true;
            chkStreaming.CheckState = CheckState.Checked;
            chkStreaming.Location = new Point(19, 50);
            chkStreaming.Name = "chkStreaming";
            chkStreaming.Size = new Size(133, 19);
            chkStreaming.TabIndex = 2;
            chkStreaming.Text = "Streaming Response";
            chkStreaming.UseVisualStyleBackColor = true;
            // 
            // btnNewChat
            // 
            btnNewChat.Location = new Point(100, 80);
            btnNewChat.Name = "btnNewChat";
            btnNewChat.Size = new Size(75, 23);
            btnNewChat.TabIndex = 1;
            btnNewChat.Text = "New Chat";
            btnNewChat.UseVisualStyleBackColor = true;
            btnNewChat.Click += btnNewChat_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(19, 80);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.Bottom;
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolsProgress, toolStripSeparator1, toolsCombo, statusText });
            toolStrip1.Location = new Point(0, 597);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1088, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolsProgress
            // 
            toolsProgress.Name = "toolsProgress";
            toolsProgress.Size = new Size(100, 22);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolsCombo
            // 
            toolsCombo.Name = "toolsCombo";
            toolsCombo.Size = new Size(121, 25);
            // 
            // statusText
            // 
            statusText.Name = "statusText";
            statusText.Size = new Size(76, 22);
            statusText.Text = "Please Wait...";
            // 
            // cmbModels
            // 
            cmbModels.FormattingEnabled = true;
            cmbModels.Location = new Point(18, 18);
            cmbModels.Name = "cmbModels";
            cmbModels.Size = new Size(157, 23);
            cmbModels.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1088, 622);
            Controls.Add(toolStrip1);
            Controls.Add(splitContainer1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chatContent).EndInit();
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
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
        private ToolStripComboBox toolsCombo;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel statusText;
        private ToolStripProgressBar toolsProgress;
        private CheckBox chkStreaming;
        private ComboBox cmbModels;
    }
}
