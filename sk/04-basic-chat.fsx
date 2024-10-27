#load "Builder.fsx"

open Builder
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.Connectors.OpenAI

let kernel = K Local

let executionSettings =
    OpenAIPromptExecutionSettings(MaxTokens = 2000, Temperature = 0.7, TopP = 0.5)

let skPrompt =
    @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

{{$history}}
User: {{$userInput}}
ChatBot:"

let chatFunction = kernel.CreateFunctionFromPrompt(skPrompt, executionSettings)

let mutable history = ""

let Chat input =

    let arguments =
        KernelArguments(source = Map.ofList [ ("history", history); ("userInput", input) ])

    let botAnswer = chatFunction.InvokeAsync(kernel, arguments).Result

    let stepFactory h usIn botAn =
        [ h; $"User: {usIn}"; $"AI: {botAn}"; "" ] |> String.concat "\n"

    let newHistory = stepFactory history input botAnswer
    history <- newHistory
    arguments["history"] <- newHistory
    printfn "%A" history

Chat("I would like a non-fiction book suggestion about Greece history. Please only list one book.")
Chat("that sounds interesting, what are some of the topics I will learn about?")
Chat("Which topic from the ones you listed do you think most people find interesting?")
Chat("could you list some more books I could read about the topic(s) you mentioned?")

printfn "%A" history
