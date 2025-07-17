using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    public class ChatDetails 
    {
        public string Name { get; set;}
        public ChatHistory ChatHistory { get; set; }
        public override string ToString() => Name;
    }
}
