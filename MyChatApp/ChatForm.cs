using Azure;
using Markdig;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualBasic;
using System.Text;

namespace MyChatApp
{
    public partial class ChatForm : Form
    {
        ToolRepository _toolRepo;
        AIChatProviders _aiChatProviders;
        AIChat _aiChat;
        MyChatAppSettings _appSettings;

        public ChatForm(MyChatAppSettings appSettings)
        {
            InitializeComponent();
            _appSettings = appSettings;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize the WebView2 control
            InitWebView();

            _toolRepo = new ToolRepository(_appSettings);
            _aiChatProviders = new AIChatProviders(_appSettings, _toolRepo);
            _aiChat = new AIChat(_aiChatProviders);

            _toolRepo.ProgressChanged += (s, e) => this.BeginInvoke(() => toolsProgress.Value = e);
            _toolRepo.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));
            _toolRepo.McpClients.ListChanged += (s, e) => this.BeginInvoke(() => RefreshTools());

            _aiChat.ActiveChatChanged += _aiChat_ActiveChatChanged;
            _aiChat.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));

            RefreshChatHistory();
            _aiChat.ChatHistories.ListChanged += (s, e) => this.BeginInvoke(() => RefreshChatHistory());
            _aiChat.ChatTitleChanged += (s, e) => this.BeginInvoke(() => RefreshChatHistory());

            RefreshModels();
            _aiChatProviders.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));

            _aiChat.LoadChatHistories();

            timer1.Start();
        }

        private void RefreshChatHistory()
        {
            chatHistory.BeginUpdate();
            chatHistory.Items.Clear();
            foreach (var chat in _aiChat.ChatHistories)
            {
                chatHistory.Items.Add(chat);
            }
            chatHistory.EndUpdate();
            chatHistory.SelectedIndex = (chatHistory.Items.Count > 0) ? 0 : -1;
        }

        private void RefreshTools()
        {
            toolsDropDown.DropDownItems.Clear();
            toolsDropDown.DropDownItems.Add("All");
            foreach (var tool in _toolRepo.GetAvailableTools())
            {
                toolsDropDown.DropDownItems.Add(new ToolStripMenuItem(tool.Name) { CheckOnClick = true });
            }
        }

        private void RefreshModels()
        {
            modelCombo.BeginUpdate();
            modelCombo.Items.Clear();
            foreach (var model in _aiChatProviders.AvailableProviders)
            {
                modelCombo.Items.Add(model);
            }
            modelCombo.EndUpdate();
            modelCombo.SelectedIndex = 0;
            _aiChat.ActiveModel = modelCombo.Text;
        }

        private async void _aiChat_ActiveChatChanged(object? sender, Microsoft.SemanticKernel.ChatCompletion.ChatHistory e)
        {
            //InitWebView();
            var _ = await chatContent.ExecuteScriptAsync($"clearDynamicDivs();");

            for (int i = 0; i < e.Count; i++)
            {
                var userMessageText = e[i].Content;
                if (userMessageText == "" || e[i].Role == AuthorRole.Tool)
                {
                    continue;
                }
                if (e[i].Role == AuthorRole.User)
                {
                    // Escape quotes for JavaScript
                    var escaped = userMessageText.Replace("\"", "\\\"");
                    await chatContent.ExecuteScriptAsync($"addHtml(`{escaped}`);");
                }
                else
                {
                    // Convert Markdown to HTML
                    // Configure the pipeline with all advanced extensions active
                    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                    string htmlChunk = Markdown.ToHtml(userMessageText, pipeline);

                    // Escape quotes for JavaScript
                    var escaped = htmlChunk.Replace("\"", "\\\"");

                    // Inject into WebView2
                    await chatContent.ExecuteScriptAsync($"updateHtml(`{escaped}`);");
                }
            }
        }

        private async void InitWebView()
        {
            await chatContent.EnsureCoreWebView2Async();

            string baseHtml = """
                <html>
                <head>
                    <style>
                        body {
                            font-family: 'Segoe UI', sans-serif;
                            background-color: #e9e9eb;
                            padding: 16px;
                        }
                        .user-msg, .bot-msg {
                            margin: 10px;
                            padding: 12px 16px;
                            border-radius: 12px;
                            line-height: 1.5;
                        }
                        .user-msg {
                            background-color: #e0e0e0;
                            align-self: flex-end;
                            text-align: left;
                        }
                        .bot-msg {
                            background-color: #ffffff;
                            border: 1px solid #ddd;
                            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
                        }
                        .chat-container {
                            display: flex;
                            flex-direction: column;
                        }
                        table {
                          width: 100%;
                          border-collapse: collapse;
                          margin: 12px 0;
                          font-family: 'Segoe UI', sans-serif;
                          font-size: 14px;
                          background-color: #fff;
                          border: 1px solid #ddd;
                          box-shadow: 0 2px 5px rgba(0,0,0,0.05);
                        }
    
                        th, td {
                          border: 1px solid #ddd;
                          padding: 10px 12px;
                          text-align: left;
                        }
    
                        th {
                          background-color: #f3f3f3;
                          font-weight: 600;
                          color: #333;
                        }
    
                        tr:nth-child(even) {
                          background-color: #fafafa;
                        }
    
                        tr:hover {
                          background-color: #f0f8ff;
                        }
                        code {
                            background-color: #f6f8fa;
                            color: #24292e;
                            font-family: Consolas, Monaco, 'Courier New', monospace;
                            font-size: 14px;
                            padding: 2px 6px;
                            border-radius: 4px;
                            border: 1px solid #e1e4e8;
                        }

                        pre {
                            background-color: #f6f8fa;
                            border: 1px solid #e1e4e8;
                            border-radius: 6px;
                            padding: 12px;
                            overflow-x: auto;
                            font-size: 14px;
                            margin: 12px 0;
                        }
                        pre code {
                            color: #24292e;
                            background: none;
                            font-family: Consolas, Monaco, 'Courier New', monospace;
                        }

                        pre code {
                            display: block;
                            counter-reset: line;
                        }
                        pre code span {
                            display: block;
                            counter-increment: line;
                        }
                        pre code span::before {
                            content: counter(line);
                            display: inline-block;
                            width: 2em;
                            margin-right: 10px;
                            color: #6a737d;
                        }
                    </style>
                    <script>
                        let replyDiv = null;
                        function addHtml(content) {
                            const div = document.createElement("div");
                            div.innerHTML = content;
                            div.className = "user-msg";
                            document.body.appendChild(div);
                            window.scrollTo(0, document.body.scrollHeight);
                            replyDiv = document.createElement("div");
                            document.body.appendChild(replyDiv);
                        }
                        function updateHtml(content) {
                            replyDiv.innerHTML = content;
                            replyDiv.className = "bot-msg";
                            window.scrollTo(0, document.body.scrollHeight);
                        }
                        function clearDynamicDivs() {
                            const dynamicDivs = document.querySelectorAll("div");
                            dynamicDivs.forEach(div => {
                                    div.remove();
                            });
                            replyDiv = null;
                        }
                    </script>
                </head>
                <body>
                </body>
                </html>
    """;

            chatContent.NavigateToString(baseHtml);
        }
        private async void btnSend_Click(object sender, EventArgs e)
        {
            GetReplyFromAI();
        }
        private void userMessage_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (e.Modifiers == Keys.Shift) // Check if Shift is pressed
                    {
                        userMessage.AppendText(Environment.NewLine); // Add a new line
                    }
                    else
                    {
                        e.SuppressKeyPress = true; // Prevent the ding sound
                        GetReplyFromAI();
                    }
                    break;
            }
        }
        private async void GetReplyFromAI()
        {
            if (string.IsNullOrWhiteSpace(userMessage.Text))
            {
                MessageBox.Show("Please enter a message.");
                return;
            }

            var userMessageText = userMessage.Text;
            // Escape quotes for JavaScript
            string escaped = userMessageText.Replace("\"", "\\\"");
            await chatContent.ExecuteScriptAsync($"addHtml(`{escaped}`);");

            // Clear the input field
            userMessage.Clear();

            var fullResponse = "";

            try
            {
                // Get the response from the AI model
                await foreach (var chunk in _aiChat.GetResponseAsync(userMessageText, chkStreaming.Checked, _fileAttachment, modelCombo.Text, chkUseTools.Checked))
                {
                    fullResponse += chunk;

                    // Configure the pipeline with all advanced extensions active
                    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                    string htmlChunk = Markdown.ToHtml(fullResponse, pipeline);

                    // Escape quotes for JavaScript
                    escaped = htmlChunk.Replace("\"", "\\\"");

                    // Inject into WebView2
                    await chatContent.ExecuteScriptAsync($"updateHtml(`{escaped}`);");
                }
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Error: " + ex.Message);
            }
            finally
            {
                // Clear the file attachment after sending the message
                _fileAttachment = null;
            }
        }

        private void btnNewChat_Click(object sender, EventArgs e)
        {
            _aiChat.CreateNewChat();
            chatHistory.SelectedIndex = 0;
        }

        private void chatHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChatDetails selectedItem = chatHistory.SelectedItem as ChatDetails;
            _aiChat.SelectChat(selectedItem);
        }

        private void chatHistory_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        public void DisplayStatusMessage(string message)
        {
            // Display the status message in a label or status bar
            // For example, you can use a Label control named statusLabel
            statusText.Text = message;
        }
        string _fileAttachment;
        private void btnAttach_Click(object sender, EventArgs e)
        {
            var dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;
                if (File.Exists(filePath))
                {
                    _fileAttachment = filePath;
                    DisplayStatusMessage($"Attached file: {Path.GetFileName(filePath)}");
                }
                else
                {
                    DisplayStatusMessage("File does not exist.");
                }
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A simple LLM Chat app developed with Semantic Kernel\n 15 July 2025.", "About MyChatApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _aiChat.SaveChatHistories();
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            await _aiChat.CreateTitlesAsync();
            timer1.Start();
        }

        private void modelCombo_SelectedChanged(object sender, EventArgs e)
        {
            _aiChat.ActiveModel = modelCombo.SelectedItem?.ToString() ?? string.Empty;
        }
    }
}