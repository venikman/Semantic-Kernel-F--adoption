#load "Builder.fsx"

open Builder
open System
open System.IO
open System.Text
open System.Net.Http
open SkiaSharp
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.TextToImage
open Microsoft.SemanticKernel.Embeddings
open Microsoft.SemanticKernel.Connectors.OpenAI
open System.Numerics.Tensors
open Microsoft.SemanticKernel.Connectors.AzureOpenAI
open sk.Config

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

    canvas.Save()

let kernel = K Local

let dallE = kernel.GetRequiredService<ITextToImageService>()

let textEmbedding: IEmbeddingGenerationService<string, System.Single> =
    AzureOpenAITextEmbeddingGenerationService(embedModelName, AzEndpoint, Key)

let prompt =
    @"
Think about an artificial object correlated to number {{$input}}.
Describe the image with one detailed sentence. The description cannot contain numbers."

let executionSettings =
    OpenAIPromptExecutionSettings(MaxTokens = 2000, Temperature = 0.2, TopP = 0.5)

let genImgDescription = kernel.CreateFunctionFromPrompt(prompt, executionSettings)
let random = Random().Next(0, 200)
let arguments = KernelArguments(source = Map.ofList [ ("input", random) ])
let imageDescriptionResult = kernel.InvokeAsync(genImgDescription, arguments).Result
let imageDescription = imageDescriptionResult.ToString()

let imageUrl = dallE.GenerateImageAsync(imageDescription.Trim(), 1024, 1024).Result
printfn $"Image URL: {imageUrl}"
ShowImage imageUrl 1024 1024

let wordWrap (text: string) (maxLineLength: int) =
    let result = StringBuilder()
    let mutable i = 0
    let mutable last = 0
    let space = [| ' '; '\r'; '\n'; '\t' |]

    while i < text.Length do
        i <-
            if last + maxLineLength > text.Length then
                text.Length
            else
                let index =
                    text.LastIndexOfAny(
                        [| ' '; ','; '.'; '?'; '!'; ':'; ';'; '-'; '\n'; '\r'; '\t' |],
                        Math.Min(text.Length - 1, last + maxLineLength)
                    )

                if index = -1 then text.Length else index + 1

        if i <= last then
            i <- Math.Min(last + maxLineLength, text.Length)

        result.AppendLine(text.Substring(last, i - last).Trim(space)) |> ignore
        last <- i

    result.ToString()

let guess = "Pokemon"

let origEmbedding =
    textEmbedding.GenerateEmbeddingsAsync(Array.singleton imageDescription).Result

let guessEmbedding =
    textEmbedding.GenerateEmbeddingsAsync(Array.singleton guess).Result

let similarity =
    TensorPrimitives.CosineSimilarity(origEmbedding.[0].Span, guessEmbedding.[0].Span)

printfn "Your description:\n%s\n" ((wordWrap guess 90))
printfn "Real description:\n%s\n" (wordWrap (imageDescription.Trim()) 90)
printfn "Score: %.2f\n\n" similarity
