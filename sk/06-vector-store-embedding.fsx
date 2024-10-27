#load "Builder.fsx"

open Builder
open sk.Config
open Microsoft.Extensions.VectorData
open FSharp.Control
open System.Text.Json
open Microsoft.SemanticKernel.Embeddings
open Microsoft.SemanticKernel.Connectors.AzureOpenAI
open Microsoft.SemanticKernel.Connectors.InMemory

type Glossary =
    { [<VectorStoreRecordKey>]
      Key: uint64

      [<VectorStoreRecordData>]
      Term: string

      [<VectorStoreRecordData>]
      Definition: string

      [<VectorStoreRecordVector(Dimensions = 1536)>]
      DefinitionEmbedding: System.Nullable<System.ReadOnlyMemory<System.Single>> }

let collection2 =
    InMemoryVectorStoreRecordCollection<uint64, Glossary>("skglossary")

collection2.CreateCollectionIfNotExistsAsync().RunSynchronously

let glossaryEntries: Glossary list =
    [ { Key = uint64 1
        Term = "API"
        Definition =
          "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
        DefinitionEmbedding = System.Nullable() }
      { Key = uint64 2
        Term = "Connectors"
        Definition =
          "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
        DefinitionEmbedding = System.Nullable() }
      { Key = uint64 3
        Term = "RAG"
        Definition =
          "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user's question (prompt)."
        DefinitionEmbedding = System.Nullable() } ]

let foo: IEmbeddingGenerationService<string, System.Single> =
    AzureOpenAITextEmbeddingGenerationService(embedModelName, AzEndpoint, Key)

let ts =
    glossaryEntries
    |> List.map (fun entry ->
        foo.GenerateEmbeddingAsync(entry.Definition).Result
        |> fun x ->
            { Key = entry.Key
              Term = entry.Term
              Definition = entry.Definition
              DefinitionEmbedding = x })

collection2.UpsertBatchAsync(ts)
|> TaskSeq.iter (fun x -> printfn $"Got it: %A{x}")
|> _.Result

// Get records by key
// In order to ensure our records were upserted correctly, we can get these records by a key with collection.GetAsync or collection.GetBatchAsync methods.
// Both methods accept GetRecordOptions class as a parameter, where you can specify if you want to include vector properties in your response or not. Taking into account that the vector dimension value can be high, if you don't need to work with vectors in your code, it's recommended to not fetch them from the database. That's why GetRecordOptions.IncludeVectors property is false by default.

let options = GetRecordOptions(IncludeVectors = true)

collection2.GetBatchAsync([ 1UL; 2UL; 3UL ], options)
|> TaskSeq.iter (fun record ->
    printfn $"Key: {record.Key}"
    printfn $"Term: {record.Term}"
    printfn $"Definition: {record.Definition}"
    printfn $"Definition Embedding: {JsonSerializer.Serialize(record.DefinitionEmbedding)}")
|> _.Result
