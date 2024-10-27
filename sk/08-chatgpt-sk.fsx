#load "Builder.fsx"

// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/08-chatGPT-with-DALL-E-3.ipynb
//  While the text completion API expects input a prompt and returns a simple string, the chat completion API expects in input a Chat history and returns a new message:
// messages=[
//     { "role": "system",    "content": "You are a helpful assistant."},
//     { "role": "user",      "content": "Who won the world series in 2020?"},
//     { "role": "assistant", "content": "The Los Angeles Dodgers won the World Series in 2020."},
//     { "role": "user",      "content": "Where was it played?"}
// ]

// When deciding between which one to use, know that ChatGPT models (i.e. gpt-3.5-turbo) are optimized for chat applications and have been fine-tuned for instruction-following and dialogue. As such, for creating semantic plugins with the Semantic Kernel, users may still find the TextCompletion model better suited for certain use cases.

open Builder
open Microsoft.SemanticKernel.TextToImage
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI

let kernel = K Local

let dallE = kernel.GetRequiredService<ITextToImageService>()
let chatGPT = kernel.GetRequiredService<IChatCompletionService>()

let systemMessage =
    @"You're chatting with a user. Instead of replying directly to the user provide a description of a cartoon image that expresses what you want to say. The user won't see your message, they will see only the image. Describe the image with details in one sentence. Make it so other LLM can you use answer as input where you do not violate any safety rules."

let chat = new ChatHistory(systemMessage)

let rec talk stop =
    match stop with
    | true -> printfn $"Done."
    | false ->
        let userInput = getInputAsync "Your message: " |> Async.RunSynchronously
        printfn "You entered: %s" userInput

        chat.AddUserMessage userInput

        let assistantReply =
            chatGPT
                .GetChatMessageContentAsync(chat, new OpenAIPromptExecutionSettings())
                .Result

        chat.AddAssistantMessage assistantReply.Content

        printfn $"\nBot:"
        let imageUrl = dallE.GenerateImageAsync(assistantReply.Content, 1024, 1024).Result
        printfn $"URL: \n{imageUrl}"
        ShowImage imageUrl 1024 1024 |> ignore
        printfn $"[{assistantReply}]\n"
        talk false

talk false
