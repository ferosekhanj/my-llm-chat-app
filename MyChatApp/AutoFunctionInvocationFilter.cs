using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    /// <summary>Shows available syntax for auto function invocation filter.</summary>
    class AutoFunctionInvocationFilter(ILogger logger) : IAutoFunctionInvocationFilter
    {
        public static AutoFunctionInvocationFilter Instance;
        
        public static AutoFunctionInvocationFilter GetInstance(ILogger logger)
        {
            if (Instance == null)
            {
                Instance = new AutoFunctionInvocationFilter(logger);
            }
            return Instance;
        }

        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            // Example: get function information
            var functionName = context.Function.Name;

            // Example: get chat history
            var chatHistory = context.ChatHistory;

            // Example: get information about all functions which will be invoked
            var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last());

            // In function calling functionality there are two loops.
            // Outer loop is "request" loop - it performs multiple requests to LLM until user ask will be satisfied.
            // Inner loop is "function" loop - it handles LLM response with multiple function calls.

            // Workflow example:
            // 1. Request to LLM #1 -> Response with 3 functions to call.
            //      1.1. Function #1 called.
            //      1.2. Function #2 called.
            //      1.3. Function #3 called.
            // 2. Request to LLM #2 -> Response with 2 functions to call.
            //      2.1. Function #1 called.
            //      2.2. Function #2 called.

            // context.RequestSequenceIndex - it's a sequence number of outer/request loop operation.
            // context.FunctionSequenceIndex - it's a sequence number of inner/function loop operation.
            // context.FunctionCount - number of functions which will be called per request (based on example above: 3 for first request, 2 for second request).
            // Example: get request sequence index
            logger.LogDebug($"Request sequence index: {context.RequestSequenceIndex}");

            // Example: get function sequence index
            logger.LogDebug($"Function sequence index: {context.FunctionSequenceIndex}");

            // Example: get total number of functions which will be called
            logger.LogDebug($"Total number of functions: {context.FunctionCount}");

            // Example: get function arguments
            var functionArguments = context.Arguments;
            // create a function call string with arguments
            var functionCallString = "(";
            foreach (var arg in functionArguments)
            {
                functionCallString += $"{arg.Key}: {arg.Value}, ";
            }
            functionCallString = functionCallString?.TrimEnd(',', ' ') + ")";
            logger.LogInformation($"Function call arguments: {functionName}{functionCallString}");

            // Calling next filter in pipeline or function itself.
            // By skipping this call, next filters and function won't be invoked, and function call loop will proceed to the next function.
            await next(context);

        }
    }
}
