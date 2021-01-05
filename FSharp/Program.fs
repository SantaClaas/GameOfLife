// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System

// Define a function to construct a message to print

module Program =
    let rec doWithEveryItem f =
        function
        | erstesElement :: rest ->
            f erstesElement
            doWithEveryItem f rest
        | [] -> ()
let from whom = sprintf "from %s" whom

let handle f =
    try
        f ()
    with
    | :? ArgumentException as e -> printfn "%O" e 

[<EntryPoint>]
let main argv =
    let a = [ 1; 2; 34; 5 ]
    Program.doWithEveryItem (printfn "Hallo %i") a
    let message = from "F#" // Call the function
    printfn "Hello world %s" message
    0 // return an integer exit code
