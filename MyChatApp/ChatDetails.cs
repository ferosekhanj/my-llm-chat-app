using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    public class ChatDetails 
    {
        public string Name { get; set;} = "New Chat";
        public ChatHistory ChatHistory { get; private set; } = new ChatHistory();
        public ChatHistory ShortChatHistory { get; private set; } = new ChatHistory();
        public bool IsModified { get; set; } = true;
        public bool IsTitleGenerated { get; set; } = false;
        public override string ToString() => Name;

        public void Add(ChatMessageContent message)
        {
            ChatHistory.Add(message);
            ShortChatHistory.Add(message);
            IsModified = true;
        }

        public void AddUserMessage(string message)
        {
            ChatHistory.AddUserMessage(message);
            ShortChatHistory.AddUserMessage(message);
            IsModified = true;
        }

        public void AddAssistantMessage(string message)
        {
            ChatHistory.AddAssistantMessage(message);
            ShortChatHistory.AddAssistantMessage(message);
            IsModified = true;
        }

        public void SetReducedHistory(ChatHistory history)
        {
            ShortChatHistory = history;
            IsModified = true;
        }
    }
}
