module SwmLang.Client.Main

open System.Net.Http
open Microsoft.AspNetCore.Components
open Elmish
open Bolero
open Parser
open Interpreter

type Model = {
    code: string
    output: string
}

let initModel = {
    code = snd Storage.examples.[0]
    output = ""
}

type Message =
    | SetCode of string
    | RunCode
    | ClearOutput
    | WriteOutput of string
    | LoadExample of int

let update (_http: HttpClient) message model =
    match message with
    | SetCode code ->
        { model with code = code }, Cmd.none
    | LoadExample index ->
        if index >= 0 && index < Storage.examples.Length then
            { model with code = snd Storage.examples.[index]; output = "" }, Cmd.none
        else
            model, Cmd.none
    | RunCode ->
        { model with output = "" }, Cmd.ofEffect (fun dispatch ->
            try
                let program = parse model.code
                let state = interpret program (WriteOutput >> dispatch)
                dispatch (WriteOutput (sprintf "\n\nexit code %d" state.ExitCode))
            with ex ->
                dispatch (WriteOutput ex.Message))
    | ClearOutput ->
        { model with output = "" }, Cmd.none
    | WriteOutput text ->
        { model with output = model.output + text }, Cmd.none

type Main = Template<"wwwroot/main.html">

let view model dispatch =
    Main.Home()
        .Code(model.code, fun code -> dispatch (SetCode code))
        .RunCode(fun _ -> dispatch RunCode)
        .ClearOutput(fun _ -> dispatch ClearOutput)
        .Output(model.output)
        .LoadExample(fun event -> dispatch (LoadExample (int (string event.Value))))
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.none) (update this.HttpClient) view
