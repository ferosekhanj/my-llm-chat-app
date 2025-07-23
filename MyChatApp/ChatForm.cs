using Azure;
using Markdig;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualBasic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MyChatApp
{
    public partial class ChatForm : Form
    {
        private readonly ILogger<ChatForm> _logger;
        ToolRepository _toolRepo;
        AIChatProviders _aiChatProviders;
        AIChat _aiChat;
        MyChatAppSettings _appSettings;

        public ChatForm(MyChatAppSettings appSettings)
        {
            _logger = AppLogger.GetLogger<ChatForm>();
            InitializeComponent();
            _appSettings = appSettings;
            _logger.LogInformation("ChatForm initialized");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger.LogInformation("Form loading started");
            
            // Initialize the WebView2 control
            InitWebView();

            _toolRepo = new ToolRepository(_appSettings);
            _aiChatProviders = new AIChatProviders(_appSettings, _toolRepo);
            _aiChat = new AIChat(_aiChatProviders);

            _logger.LogInformation("Core components initialized");

            _toolRepo.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));
            _toolRepo.ToolsLoaded += (s, e) => this.BeginInvoke(() => RefreshTools());

            _aiChat.ActiveChatChanged += _aiChat_ActiveChatChanged;
            _aiChat.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));

            RefreshChatHistory();
            _aiChat.ChatHistories.ListChanged += (s, e) => this.BeginInvoke(() => RefreshChatHistory());
            _aiChat.ChatTitleChanged += (s, e) => this.BeginInvoke(() => RefreshChatHistory());

            RefreshModels();
            _aiChatProviders.StatusChanged += (s, e) => this.BeginInvoke(() => DisplayStatusMessage(e));

            _aiChat.LoadChatHistories();

            timer1.Start();
            _logger.LogInformation("Form loading completed");
        }

        private void RefreshChatHistory()
        {
            _logger.LogDebug("Refreshing chat history. Chat count: {ChatCount}", _aiChat.ChatHistories.Count);
            chatHistory.BeginUpdate();
            chatHistory.Items.Clear();
            foreach (var chat in _aiChat.ChatHistories)
            {
                chatHistory.Items.Add(chat);
            }
            chatHistory.EndUpdate();
            chatHistory.SelectedIndex = (chatHistory.Items.Count > 0) ? 0 : -1;
            _logger.LogDebug("Chat history refreshed successfully");
        }

        private void RefreshTools()
        {
            var toolCount = _toolRepo.GetAvailableTools().Count;
            _logger.LogInformation("Refreshing tools. Available tool count: {ToolCount}", toolCount);
            
            toolsDropDown.DropDownItems.Clear();
            toolsDropDown.DropDownItems.Add(new ToolStripMenuItem("All") { CheckOnClick = true });
            foreach (var tool in _toolRepo.GetAvailableTools())
            {
                toolsDropDown.DropDownItems.Add(new ToolStripMenuItem(tool.Name) { CheckOnClick = true });
            }
            _logger.LogDebug("Tools refreshed successfully");
        }

        private void RefreshModels()
        {
            var providerCount = _aiChatProviders.AvailableProviders.Count();
            _logger.LogInformation("Refreshing models. Available provider count: {ProviderCount}", providerCount);
            
            modelCombo.BeginUpdate();
            modelCombo.Items.Clear();
            foreach (var model in _aiChatProviders.AvailableProviders)
            {
                modelCombo.Items.Add(model);
            }
            modelCombo.EndUpdate();
            modelCombo.SelectedIndex = 0;
            _aiChat.ActiveModel = modelCombo.Text;
            _logger.LogDebug("Models refreshed successfully. Active model: {ActiveModel}", _aiChat.ActiveModel);
        }

        private IList<string> GetSelectedTools()
        {
            _logger.LogDebug("Getting selected tools");
            var selectedTools = new List<string>();
            // Check if "All" is selected
            if (toolsDropDown.DropDownItems[0].Text == "All" && ((ToolStripMenuItem)toolsDropDown.DropDownItems[0]).Checked)
            {
                var allTools = _toolRepo.GetAvailableTools().Select(t => t.Name).ToList();
                _logger.LogDebug("All tools selected. Count: {ToolCount}", allTools.Count);
                return allTools;
            }
            foreach (ToolStripMenuItem item in toolsDropDown.DropDownItems)
            {
                if (item.Checked && item.Text != "All")
                {
                    selectedTools.Add(item.Text);
                }
            }
            _logger.LogDebug("Selected tools: {SelectedTools}", string.Join(", ", selectedTools));
            return selectedTools;
        }

        private async void _aiChat_ActiveChatChanged(object? sender, Microsoft.SemanticKernel.ChatCompletion.ChatHistory e)
        {
            _logger.LogInformation("Active chat changed. Message count: {MessageCount}", e.Count);
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
                    await chatContent.ExecuteScriptAsync($"addUserHtml(`{escaped}`);");
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
                    await chatContent.ExecuteScriptAsync($"addAIHtml(`{escaped}`);");
                }
            }
            _logger.LogDebug("Chat history loaded successfully");
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
                        function addUserHtml(content) {
                            const div = document.createElement("div");
                            div.innerHTML = content;
                            div.className = "user-msg";
                            document.body.appendChild(div);
                            window.scrollTo(0, document.body.scrollHeight);
                        }
                        function addAIHtml(content) {
                            replyDiv = document.createElement("div");
                            replyDiv.innerHTML = content;
                            replyDiv.className = "bot-msg";
                            document.body.appendChild(replyDiv);
                            window.scrollTo(0, document.body.scrollHeight);
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
                _logger.LogWarning("User attempted to send empty message");
                MessageBox.Show("Please enter a message.");
                return;
            }

            var userMessageText = userMessage.Text;
            _logger.LogInformation("User sending message. Length: {MessageLength}", userMessageText.Length);
            
            // Escape quotes for JavaScript
            string escaped = userMessageText.Replace("\"", "\\\"");
            await chatContent.ExecuteScriptAsync($"addUserHtml(`{escaped}`);");
            await chatContent.ExecuteScriptAsync($"addAIHtml(`{"..."}`);");

            // Clear the input field
            userMessage.Clear();

            var fullResponse = "";
            var selectedTools = GetSelectedTools();
            _logger.LogInformation("Starting AI response generation. Model: {Model}, UseTools: {UseTools}, ToolCount: {ToolCount}", 
                modelCombo.Text, chkUseTools.Checked, selectedTools.Count);

            try
            {
                // Get the response from the AI model
                await foreach (var chunk in _aiChat.GetResponseAsync(userMessageText, chkStreaming.Checked, _fileAttachment, modelCombo.Text, chkUseTools.Checked, selectedTools))
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
                _logger.LogInformation("AI response completed. Response length: {ResponseLength}", fullResponse.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI response");
                DisplayStatusMessage("Error: " + ex.Message);
            }
            finally
            {
                // Clear the file attachment after sending the message
                if (_fileAttachment != null)
                {
                    _logger.LogDebug("Clearing file attachment: {FileName}", Path.GetFileName(_fileAttachment));
                }
                _fileAttachment = null;
            }
        }

        private void btnNewChat_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("User creating new chat");
            _aiChat.CreateNewChat();
            chatHistory.SelectedIndex = 0;
            _logger.LogDebug("New chat created successfully");
        }

        private void chatHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChatDetails? selectedItem = chatHistory.SelectedItem as ChatDetails;
            if (selectedItem != null)
            {
                _logger.LogDebug("Chat selected: {ChatName}", selectedItem.Name);
                _aiChat.SelectChat(selectedItem);
            }
        }

        private void chatHistory_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        public void DisplayStatusMessage(string message)
        {
            _logger.LogDebug("Status message: {Message}", message);
            // Display the status message in a label or status bar
            // For example, you can use a Label control named statusLabel
            statusText.Text = message;
        }
        string? _fileAttachment;
        private void btnAttach_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("User clicking attach file button");
            var dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;
                if (File.Exists(filePath))
                {
                    _fileAttachment = filePath;
                    _logger.LogInformation("File attached: {FileName}, Size: {FileSize} bytes", 
                        Path.GetFileName(filePath), new FileInfo(filePath).Length);
                    DisplayStatusMessage($"Attached file: {Path.GetFileName(filePath)}");
                }
                else
                {
                    _logger.LogWarning("Selected file does not exist: {FilePath}", filePath);
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
            _logger.LogInformation("ChatForm closing - saving chat histories");
            _aiChat.SaveChatHistories();
            _logger.LogDebug("ChatForm closed successfully");
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            _logger.LogDebug("Timer tick - creating titles for modified chats");
            await _aiChat.CreateTitlesAsync();
            timer1.Start();
        }

        private void modelCombo_SelectedChanged(object sender, EventArgs e)
        {
            var selectedModel = modelCombo.SelectedItem?.ToString() ?? string.Empty;
            _logger.LogInformation("Model changed to: {ModelName}", selectedModel);
            _aiChat.ActiveModel = selectedModel;
        }
    }
}