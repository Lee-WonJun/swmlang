module Interpreter

open System
open System.Collections.Generic
open AST

type SoftwareState = {
    Variables: Dictionary<string, int64>
    mutable StandardOutput: string
    mutable ExitCode: int
}

type private Frame = Dictionary<string, int64>

type private Signal =
    | Normal
    | Returned of int64
    | Exited of int

let private getVar (frame: Frame) name =
    match frame.TryGetValue(name) with
    | true, value -> value
    | false, _ -> failwithf "정의되지 않은 변수입니다: %s" name

let private setVar (frame: Frame) name value =
    if frame.ContainsKey(name) then frame.[name] <- value
    else frame.Add(name, value)

let interpret (program: Program) onOutput =
    let state = {
        Variables = Dictionary()
        StandardOutput = ""
        ExitCode = 0
    }

    let write text =
        onOutput text
        state.StandardOutput <- state.StandardOutput + text

    let rec callFunction (call: Call) =
        let fn =
            match program.Functions.TryFind(call.Name) with
            | Some fn -> fn
            | None -> failwithf "정의되지 않은 함수입니다: %s" call.Name

        let frame = Dictionary()
        let parameterNames = fn.Parameters |> List.map fst |> Set.ofList
        for argName in call.Args.Keys do
            if not (parameterNames.Contains argName) then
                failwithf "함수 '%s'에 정의되지 않은 인자입니다: %s" fn.Name argName

        for name, defaultValue in fn.Parameters do
            let value = call.Args |> Map.tryFind name |> Option.defaultValue defaultValue
            frame.Add(name, value)

        match execBlock frame false fn.Body with
        | Returned value -> value
        | Normal -> failwithf "함수 '%s'가 종료문 없이 끝났습니다" fn.Name
        | Exited value -> int64 value

    and execBlock frame isTopLevel statements =
        let rec loop remaining =
            match remaining with
            | [] -> Normal
            | statement :: rest ->
                match execStatement frame isTopLevel statement with
                | Normal -> loop rest
                | signal -> signal

        loop statements

    and execStatement frame isTopLevel statement =
        match statement with
        | Declare(name, value) ->
            setVar frame name value
            Normal
        | Increment name ->
            setVar frame name (getVar frame name + 1L)
            Normal
        | Decrement name ->
            setVar frame name (getVar frame name - 1L)
            Normal
        | Assign(name, value) ->
            setVar frame name value
            Normal
        | PutChar name ->
            let value = getVar frame name
            let ch = char (int (value &&& 0xFFL))
            write (string ch)
            Normal
        | PrintInt name ->
            write (string (getVar frame name))
            Normal
        | While(name, body) ->
            let mutable signal = Normal
            while signal = Normal && getVar frame name <> 0L do
                signal <- execBlock frame isTopLevel body
            signal
        | If(name, thenBlock, elseBlock) ->
            if getVar frame name = 0L then
                execBlock frame isTopLevel thenBlock
            else
                elseBlock
                |> Option.map (execBlock frame isTopLevel)
                |> Option.defaultValue Normal
        | CallIgnore call ->
            callFunction call |> ignore
            Normal
        | CallAssign(name, call) ->
            setVar frame name (callFunction call)
            Normal
        | Return name ->
            let value = getVar frame name
            if isTopLevel then
                state.ExitCode <- int (value &&& 0xFFL)
                Exited state.ExitCode
            else
                Returned value

    let topFrame = Dictionary()
    match execBlock topFrame true program.TopLevel with
    | Exited code -> state.ExitCode <- code
    | _ -> ()

    for item in topFrame do
        state.Variables.Add(item.Key, item.Value)

    state

let interpretAsync program onOutput = async {
    return interpret program onOutput
}
