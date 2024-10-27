#r "nuget: Microsoft.SemanticKernel, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.OpenAI, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.AzureOpenAI, 1.25.0"
#r "nuget: Microsoft.SemanticKernel.Connectors.InMemory, 1.25.0-preview"
#r "nuget: Microsoft.Extensions.VectorData.Abstractions, 9.0.0-preview.1.24523.1"
#r "nuget: System.Linq.Async, 6.0.1"
#r "nuget: FSharp.Control.TaskSeq, 0.4.0"
#r "nuget: System.Numerics.Tensors, 9.0.0-rc.2.24473.5"
#r "nuget: SkiaSharp, 3.0.0-preview.5.4"

#I @"Plugins"
#load "Config.fs"

open Microsoft.SemanticKernel
open System
open System.IO
open System.Net.Http
open SkiaSharp
open sk.Config
open Microsoft.SemanticKernel.Embeddings
open Microsoft.SemanticKernel.Connectors.AzureOpenAI

type Deployments =
    | Local
    | Azure

let AzEndpoint = "https://rag-test-gail.openai.azure.com"
let ModelName = "gpt-4o"
let embedModelName = "text-embedding-ada-002"
let dalleModelName = "dall-e-3"
let dalleEndpoint = "https://sniad-m2q8pasc-eastus.openai.azure.com"
// let ModelVersion = "2024-08-06"
// let EmbModelVersion = "2023-05-15"
// let DallEModelVersion = "3.0"

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

    |> _.AddAzureOpenAITextToImage(deploymentName = dalleModelName, endpoint = dalleEndpoint, apiKey = DalEKey)
        .Build()

let textEmbedding: IEmbeddingGenerationService<string, Single> =
    AzureOpenAITextEmbeddingGenerationService(embedModelName, AzEndpoint, Key)


let ShowImage (url: string) (width: int) (height: int) =
    let info = new SKImageInfo(width, height)
    let surface = SKSurface.Create(info)
    let canvas = surface.Canvas
    canvas.Clear(SKColors.White)

    task {
        use httpClient = new HttpClient()
        use! stream = httpClient.GetStreamAsync(url)
        use memStream = new MemoryStream()
        do! stream.CopyToAsync(memStream)
        memStream.Seek(0L, SeekOrigin.Begin) |> ignore
        let webBitmap = SKBitmap.Decode(memStream)
        canvas.DrawBitmap(webBitmap, 0.0f, 0.0f, null)
        surface.Draw(canvas, 0.0f, 0.0f, null)
    }
    |> _.Result

    canvas.Save() // in example it was `canvas.Snapshot().Display()
