#r "nuget: Microsoft.SemanticKernel"
#r "nuget: Microsoft.Extensions.Logging"
#r "nuget: Microsoft.Extensions.Logging.Abstractions"
#r "nuget: Microsoft.Extensions.DependencyInjection"
#I @"Plugins"
#load "Domain.fs"
#load "Config.fs"

open sk.Config
open Microsoft.SemanticKernel
open System.IO
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.DependencyInjection

let myLoggerFactory = NullLoggerFactory.Instance

let kBuilder =
    Kernel
        .CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName = skConfig.ModelName,
            endpoint = skConfig.Endpoint,
            apiKey = skConfig.Key
        )

kBuilder.Services.AddSingleton(implementationInstance = myLoggerFactory)
let K = kBuilder.Build()

// --------------------------
let funPluginDirectoryPath = Path.Combine("sk", "Plugins")
let plugins = K.ImportPluginFromPromptDirectory(funPluginDirectoryPath)

// Arguments conept: https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Functions/Arguments.cs
let arguments =
    KernelArguments(
        executionSettings = PromptExecutionSettings(ExtensionData = dict [ ("input", "time travel to dinosaur age") ])
    )

let result = K.InvokeAsync(plugins["Joke"], arguments).Result

printfn "%A" result
