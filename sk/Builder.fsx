#r "nuget: Microsoft.SemanticKernel, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.OpenAI, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.AzureOpenAI, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.InMemory, 1.25.0-preview"
#r "nuget: Microsoft.Extensions.VectorData.Abstractions, 9.0.0-preview.1.24523.1"
#r "nuget: System.Linq.Async, 6.0.1"
#r "nuget: FSharp.Control.TaskSeq, 0.4.0"

#I @"Plugins"
#load "Config.fs"

open Microsoft.SemanticKernel
open System
open sk.Config

type Deployments =
    | Local
    | Azure


let AzEndpoint = "https://rag-test-gail.openai.azure.com"
let ModelName = "gpt-4o"
let embedModelName = "text-embedding-ada-002"
// let ModelVersion = "2024-08-06"
// let EmbModelVersion = "2023-05-15"


let K (t: Deployments) =
    match t with
    | Local ->

        Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(modelId = "llama3.2", endpoint = Uri "http://localhost:1234/v1", apiKey = null)
    | Azure ->
        Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName = ModelName, endpoint = AzEndpoint, apiKey = Key)

    |> _.AddAzureOpenAITextEmbeddingGeneration(embedModelName).Build()
