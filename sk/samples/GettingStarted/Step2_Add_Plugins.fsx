#load "../../Builder.fsx"
#r "nuget: Microsoft.OpenApi, 1.6.22"
#r "nuget: Microsoft.SemanticKernel, 1.25.0"

open System
open System.Text.Json.Serialization
open System.ComponentModel

open Microsoft.SemanticKernel

open Builder

[<AutoOpen>]
module Internals =
    /// <summary>
    /// A <see cref="JsonConverter"/> is required to correctly convert enum values.
    /// </summary>
    [<JsonConverter(typeof<JsonStringEnumConverter>)>]
    type WidgetType =
        | [<Description("A widget that is useful.")>] Useful = 0
        | [<Description("A widget that is decorative.")>] Decorative = 1

    /// <summary>
    /// A <see cref="JsonConverter"/> is required to correctly convert enum values.
    /// </summary>
    [<JsonConverter(typeof<JsonStringEnumConverter>)>]
    type WidgetColor =
        | [<Description("A widget that is decorative.")>] Red = 0
        | [<Description("Use when creating a green item.")>] Green = 1
        | [<Description("Use when creating a blue item.")>] Blue = 2

    type WidgetDetails =
        { SerialNumber: string
          Type: WidgetType
          Colors: WidgetColor[] }

    /// <summary>
    /// A plugin that returns the current time.
    /// </summary>
    type TimeInformation() =
        [<KernelFunction>]
        [<Description("Retrieves the current time in UTC.")>]
        member _.GetCurrentUtcTime() = DateTime.UtcNow.ToString("R")

    /// <summary>
    /// A plugin that creates widgets.
    /// </summary>
    type WidgetFactory() =
        [<KernelFunction>]
        [<Description("Creates a new widget of the specified type and colors")>]
        member _.CreateWidget
            (
                [<Description("The type of widget to be created")>] widgetType: WidgetType,
                [<Description("The colors of the widget to be created")>] widgetColors: WidgetColor[]
            ) : WidgetDetails =
            let colors =
                widgetColors
                |> Array.fold (fun st en -> String.Concat [ st; "-"; en.ToString() ]) String.Empty

            { SerialNumber = $"{widgetType}{colors}-{Guid.NewGuid()}"
              Type = widgetType
              Colors = widgetColors }

open Internals
open Microsoft.SemanticKernel.Connectors.OpenAI

let kern = K Local

kern.Plugins.AddFromType<TimeInformation>().AddFromType<WidgetFactory>()
let kernel = kern.Build()

let runPrompt (k: Kernel) (set: KernelArguments option) (prompt: string) =
    match set with
    | None -> k.InvokePromptAsync(prompt).Result |> printfn "%A"
    | Some kArgs -> k.InvokePromptAsync(prompt, kArgs).Result |> printfn "%A"

let run = runPrompt kernel
// Example 1. Invoke the kernel with a prompt that asks the AI for information it cannot provide and may hallucinate
run None "How many days until Christmas?" |> printfn "%A"
// Example 2. Invoke the kernel with a templated prompt that invokes a plugin and display the result
run None "The current time is {{TimeInformation.GetCurrentUtcTime}}. How many days until Christmas?"
// Example 3. Invoke the kernel with a prompt and allow the AI to automatically invoke functions
let executionSettings =
    KernelArguments(OpenAIPromptExecutionSettings(FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()))

let r = run (Some executionSettings)
r "How many days until Christmas? Explain your thinking."
// Example 4. Invoke the kernel with a prompt and allow the AI to automatically invoke functions that use enumerations
r "Create a handy lime colored widget for me."
r "Create a beautiful scarlet colored widget for me."
r "Create an attractive maroon and navy colored widget for me."
