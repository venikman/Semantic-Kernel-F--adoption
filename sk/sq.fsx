#r "nuget: System.Data.SQLite"
#r "nuget: Dapper"
#r "nuget: Dapper.FSharp"

open System.Data.SQLite
open Dapper
open Dapper.FSharp
open Dapper.FSharp.SQLite

// Define your data model
type Person = { Id: int; Name: string }

// Initialize Dapper.FSharp
OptionTypes.register ()

// Create a connection to the SQLite database
let conn = new SQLiteConnection("Data Source=mydatabase.db;Version=3;")
conn.Open()

// Create the table if it doesn't exist
let createTableSql =
    """
CREATE TABLE IF NOT EXISTS Person (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
)
"""

conn.Execute(createTableSql) |> ignore

// Insert a record using Dapper.FSharp
let newPerson = { Id = 0; Name = "Alice" }

insert {
    into table<Person>
    value newPerson
}
|> conn.InsertAsync
|> Async.AwaitTask
|> Async.RunSynchronously
|> ignore

// Query the inserted record
let people =
    select {
        for p in table<Person> do
            selectAll
    }
    |> conn.SelectAsync<Person>
    |> Async.AwaitTask
    |> Async.RunSynchronously

printfn "People in database: %A" people
