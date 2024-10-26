#r "nuget: Microsoft.SemanticKernel"
#r "nuget: Microsoft.SemanticKernel.Connectors.OpenAI"
#load "Domain.fs"
#load "Config.fs"

open Microsoft.SemanticKernel
open System
open sk.Config
open sk.Domain



let K (t: Deployments) =
    match t with
    | Local ->
        Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(modelId = "llama3.2", endpoint = Uri "http://localhost:1234/v1", apiKey = null)
            .Build()
    | Azure ->
        Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName = skConfig.ModelName,
                endpoint = skConfig.Endpoint,
                apiKey = skConfig.Key
            )
            .Build()
