#load "Builder.fsx"

open Builder
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.Connectors.OpenAI
open Microsoft.SemanticKernel.ChatCompletion
open System.Text.Json
open System.IO

let kernel = K Local
let pluginDirectoryPath = Path.Combine("sk", "Plugins")
kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath, "SummarizePlugin")
kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath, "WritePlugin")

let ask =
    "Tomorrow is Valentine's day. I need to come up with a few date ideas. My significant other likes poems so write them in the form of a poem."

let openAISettings =
    OpenAIPromptExecutionSettings(FunctionChoiceBehavior = FunctionChoiceBehavior.Auto())

// let result =
//     kernel
//         .InvokePromptAsync(ask, KernelArguments(executionSettings = openAISettings))
//         .Result

// printfn "%A" result
let chatCompletionService = kernel.GetRequiredService<IChatCompletionService>()
let chatHistory = ChatHistory()
chatHistory.AddUserMessage ask

let chatCompletionResult =
    chatCompletionService
        .GetChatMessageContentAsync(chatHistory, openAISettings, kernel)
        .Result

printfn $"Result: {chatCompletionResult}"
printfn $"Chat history: {JsonSerializer.Serialize(chatHistory)}"
