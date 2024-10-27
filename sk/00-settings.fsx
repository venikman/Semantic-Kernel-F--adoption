#load "Builder.fsx"

open Builder
open Microsoft.SemanticKernel
open System.IO
open sk.Config

let K =
    Kernel
        .CreateBuilder()
        .AddAzureOpenAIChatCompletion(deploymentName = ModelName, endpoint = AzEndpoint, apiKey = Key)
        .Build()

let funPluginDirectoryPath = Path.Combine("sk", "Plugins", "FunPlugin")
let plugins = K.ImportPluginFromPromptDirectory(funPluginDirectoryPath)

// Arguments conept: https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Functions/Arguments.cs
let arguments =
    KernelArguments(
        executionSettings = PromptExecutionSettings(ExtensionData = dict [ ("input", "time travel to dinosaur age") ])
    )

let result = K.InvokeAsync(plugins["Joke"], arguments).Result

printfn "%A" result
