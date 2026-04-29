module ParserHelper

open System
open System.Text
open AST

type ParseError = {
    Line: int
    Message: string
}

exception ParseException of ParseError

type SourceLine = {
    Number: int
    Text: string
}

type LineNode =
    | FunctionStart of string
    | Signature of string
    | Statement of Statement
    | Else
    | BlockEnd

let failAt (line: SourceLine) message =
    raise (ParseException { Line = line.Number; Message = message })

let decodeBase64Url (line: SourceLine) (encoded: string) =
    try
        let s = encoded.Replace('-', '+').Replace('_', '/')
        let padded =
            match s.Length % 4 with
            | 2 -> s + "=="
            | 3 -> s + "="
            | _ -> s
        Encoding.UTF8.GetString(Convert.FromBase64String(padded))
    with ex ->
        failAt line (sprintf "base64url 디코딩 실패: %s (%s)" encoded ex.Message)

let splitLast (separator: string) (text: string) =
    let index = text.LastIndexOf(separator, StringComparison.Ordinal)
    if index < 0 then None
    else Some(text.Substring(0, index), text.Substring(index + separator.Length))

let parseInt64 (line: SourceLine) (text: string) =
    match Int64.TryParse(text) with
    | true, value -> value
    | false, _ -> failAt line (sprintf "정수 리터럴이 아닙니다: %s" text)

let parseSignature line text =
    if String.IsNullOrWhiteSpace(text) then []
    else
        text.Split('&', StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun item ->
            match splitLast "==" item with
            | Some(encoded, v) -> decodeBase64Url line encoded, parseInt64 line v
            | None -> failAt line (sprintf "시그니처 파라미터 형식이 올바르지 않습니다: %s" item))
        |> Array.toList

let parseCall line (text: string) =
    let path, query =
        match text.IndexOf('?') with
        | -1 -> text, ""
        | i  -> text.Substring(0, i), text.Substring(i + 1)

    let args =
        if String.IsNullOrWhiteSpace(query) then Map.empty
        else
            query.Split('&', StringSplitOptions.RemoveEmptyEntries)
            |> Array.map (fun item ->
                match splitLast "=" item with
                | Some(encoded, v) -> decodeBase64Url line encoded, parseInt64 line v
                | None -> failAt line (sprintf "호출 인자 형식이 올바르지 않습니다: %s" item))
            |> Map.ofArray

    { Name = decodeBase64Url line path; Args = args }

let hasReachableReturn body =
    let rec check = function
        | Return _ -> true
        | While(_, block) -> block |> List.exists check
        | If(_, t, e) -> (t |> List.exists check) || (e |> Option.exists (List.exists check))
        | _ -> false
    body |> List.exists check

/// 라인별 파싱 결과를 받아 블록/함수/프로그램 구조를 조립
let buildProgram (lines: (SourceLine * LineNode) list) =
    let rec parseBlock stopAt index =
        let rec loop index acc =
            if index >= lines.Length then
                acc |> List.rev, index, None
            else
                let _, node = lines.[index]
                if stopAt |> List.exists (fun stop -> stop node) then
                    acc |> List.rev, index, Some node
                else
                    let stmt, next = parseStatement index
                    loop next (stmt :: acc)
        loop index []

    and parseStatement index =
        let line, node = lines.[index]
        match node with
        | Statement(While(name, [])) ->
            let body, endIdx, stop = parseBlock [ ((=) BlockEnd) ] (index + 1)
            match stop with
            | Some _ -> While(name, body), endIdx + 1
            | None   -> failAt line "while 블록 종료문이 없습니다"

        | Statement(If(name, [], None)) ->
            let thenBlock, stopIdx, stop =
                parseBlock [ ((=) Else); ((=) BlockEnd) ] (index + 1)
            match stop with
            | Some Else ->
                let elseBlock, endIdx, endStop = parseBlock [ ((=) BlockEnd) ] (stopIdx + 1)
                match endStop with
                | Some _ -> If(name, thenBlock, Some elseBlock), endIdx + 1
                | None   -> failAt line "if else 블록 종료문이 없습니다"
            | Some BlockEnd ->
                If(name, thenBlock, None), stopIdx + 1
            | _ -> failAt line "if 블록 종료문이 없습니다"

        | Statement stmt -> stmt, index + 1

        | FunctionStart _ | Signature _ | Else | BlockEnd ->
            failAt line (sprintf "예상하지 못한 블록/함수 구문입니다: %s" line.Text)

    let rec parseFunctionBody index acc =
        if index >= lines.Length then acc |> List.rev, index
        else
            let stmt, next = parseStatement index
            let acc = stmt :: acc
            match stmt with
            | Return _ -> acc |> List.rev, next
            | _        -> parseFunctionBody next acc

    let rec parseTop index topLevel functions =
        if index >= lines.Length then
            { Functions = functions |> List.map (fun fn -> fn.Name, fn) |> Map.ofList
              TopLevel = topLevel |> List.rev }
        else
            let line, node = lines.[index]
            match node with
            | FunctionStart name ->
                if index + 1 >= lines.Length then failAt line "함수 시그니처가 없습니다"
                let sigLine, sigNode = lines.[index + 1]
                match sigNode with
                | Signature sigText ->
                    let body, next = parseFunctionBody (index + 2) []
                    if not (hasReachableReturn body) then
                        failAt line (sprintf "함수 '%s'에 종료문이 없습니다" name)
                    let fn = { Name = name; Parameters = parseSignature sigLine sigText; Body = body }
                    parseTop next topLevel (fn :: functions)
                | _ -> failAt sigLine "함수 시그니처가 아닙니다"
            | _ ->
                let stmt, next = parseStatement index
                parseTop next (stmt :: topLevel) functions

    parseTop 0 [] []
