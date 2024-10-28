#load "Builder.fsx"
// To use Bing Search you simply need a Bing Search API key. You can get the API key by creating a Bing Search resource in Azure.
// https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/create-bing-search-service-resource

open Builder
open sk.Config
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.Data
open Microsoft.SemanticKernel.Plugins.Web.Bing
open Microsoft.SemanticKernel.PromptTemplates.Handlebars

let kernel = (K Local).Build()
let textSearch = new BingTextSearch(apiKey = BingKey)
let query = "What is the Semantic Kernel?"
let args = KernelArguments(source = Map.ofList [ ("query", query) ])

// --------------------
// let searchPlugin = textSearch.CreateWithSearch("SearchPlugin")
// kernel.Plugins.Add(searchPlugin)
// let query = "What is the Semantic Kernel?"
// let prompt = "{{SearchPlugin.Search $query}}. {{$query}}"
// let args = KernelArguments(source = Map.ofList [ ("query", query) ])
// kernel.InvokePromptAsync(prompt, args) |> _.Result |> printfn "%A"
// --------------
// let searchPlugin = textSearch.CreateWithGetTextSearchResults "SearchPlugin"
// kernel.Plugins.Add searchPlugin

// let promptTemplateFactory = new HandlebarsPromptTemplateFactory()
// let promptTemplate =
//     """
// {{#with (SearchPlugin-GetTextSearchResults query)}}
//     {{#each this}}
//     Name: {{Name}}
//     Value: {{Value}}
//     Link: {{Link}}
//     -----------------
//     {{/each}}
// {{/with}}

// {{query}}

// Include citations to the relevant information where it is referenced in the response.
// """
// kernel
//     .InvokePromptAsync(
//         promptTemplate,
//         args,
//         templateFormat = HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
//         promptTemplateFactory = promptTemplateFactory
//     )
//     .Result
// |> printfn "%s"
//--------------------------------
let filter = TextSearchFilter().Equality("site", "devblogs.microsoft.com")
let searchOptions = new TextSearchOptions(Filter = filter)

let searchPluginF =
    KernelPluginFactory.CreateFromFunctions(
        "SearchPluginF",
        "Search Microsoft Developer Blogs site only",
        [ textSearch.CreateGetTextSearchResults(searchOptions = searchOptions) ]
    )

kernel.Plugins.Add searchPluginF

let promptTemplateF =
    """
{{#with (SearchPluginF-GetTextSearchResults query)}}  
    {{#each this}}  
    Name: {{Name}}
    Value: {{Value}}
    Link: {{Link}}
    -----------------
    {{/each}}  
{{/with}}  

{{query}}

Include citations to the relevant information where it is referenced in the response.
"""

let promptTemplateFactoryF = HandlebarsPromptTemplateFactory()

kernel
    .InvokePromptAsync(
        promptTemplateF,
        args,
        templateFormat = HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
        promptTemplateFactory = promptTemplateFactoryF
    )
    .Result
|> printfn "%s"
