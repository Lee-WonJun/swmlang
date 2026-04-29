module Parser

open System
open System.Text
open FParsec
open AST

type ParseError = {
    Line: int
    Message: string
}

exception ParseException of ParseError

type XParser<'T> = Parser<'T, unit>

type private SourceLine = {
    Number: int
    Text: string
}

type private LineNode =
    | FunctionStart of string
    | Signature of string
    | Statement of Statement
    | Else
    | BlockEnd

let private failAt line message =
    raise (ParseException { Line = line.Number; Message = message })

let private decodeBase64Url line (encoded: string) =
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

let private splitLast (separator: string) (text: string) =
    let index = text.LastIndexOf(separator, StringComparison.Ordinal)
    if index < 0 then None
    else Some(text.Substring(0, index), text.Substring(index + separator.Length))

let private parseInt64 line (text: string) =
    match Int64.TryParse(text) with
    | true, value -> value
    | false, _ -> failAt line (sprintf "정수 리터럴이 아닙니다: %s" text)

let private parseSignature line text =
    if String.IsNullOrWhiteSpace(text) then []
    else
        text.Split('&', StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun item ->
            match splitLast "==" item with
            | Some(encoded, v) -> decodeBase64Url line encoded, parseInt64 line v
            | None -> failAt line (sprintf "시그니처 파라미터 형식이 올바르지 않습니다: %s" item))
        |> Array.toList

let private parseCall line (text: string) =
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

let private hasReachableReturn body =
    let rec check = function
        | Return _ -> true
        | While(_, block) -> block |> List.exists check
        | If(_, t, e) -> (t |> List.exists check) || (e |> Option.exists (List.exists check))
        | _ -> false
    body |> List.exists check

// ── FParsec 기본 조합자 ──

let private ws: XParser<unit> = skipMany1Satisfy Char.IsWhiteSpace
let private rest: XParser<string> = restOfLine false

/// [이름] 형태의 대괄호 식별자
let private bracket: XParser<string> =
    between (pchar '[') (pchar ']') (many1Satisfy (fun c -> c <> ']'))

/// 부호 없는 숫자 문자열
let private digits: XParser<string> =
    many1Satisfy Char.IsDigit

/// 공백으로 구분된 키워드 시퀀스를 하나의 파서로 결합
let private keywords (words: string list) : XParser<unit> =
    words
    |> List.map (fun w -> skipString w)
    |> List.reduce (fun a b -> a >>. ws >>. b)

/// 파서 결과를 줄 끝까지 정확히 매칭
let private exact p = p .>> eof

// ── 라인 파서 ──

let private lineParsers (line: SourceLine) : XParser<LineNode> =

    // 안녕하세요 <name> 멘토입니다
    let functionStart = parse {
        do! keywords ["안녕하세요"]
        do! ws
        let! name = many1Satisfy (fun c -> not (Char.IsWhiteSpace c))
        do! ws >>. skipString "멘토입니다"
        return FunctionStart name
    }

    // 멘토 소개: https://notion.so/<시그니처>
    let signature =
        keywords ["멘토"; "소개:"] >>. ws >>. skipString "https://notion.so/" >>. rest
        |>> Signature

    // [X] 마감되었습니다. 감사합니다!
    let returnStmt =
        bracket .>> ws .>> skipString "마감되었습니다. 감사합니다!"
        |>> (Return >> Statement)

    // 이번에 [X] (정원 N) 을 개설했습니다
    let declare = parse {
        do! skipString "이번에" >>. ws
        let! name = bracket
        do! ws >>. skipString "(정원" >>. ws
        let! value = pint64
        do! skipString ")" >>. ws
        do! keywords ["을"; "개설했습니다"]
        return Statement(Declare(name, value))
    }

    // [X] 신청 바랍니다
    let increment =
        bracket .>> ws .>> keywords ["신청"; "바랍니다"]
        |>> (Increment >> Statement)

    // [X] 한자리 남았습니다
    let decrement =
        bracket .>> ws .>> keywords ["한자리"; "남았습니다"]
        |>> (Decrement >> Statement)

    // [X] N자리 남았습니다
    let assignSeat = parse {
        let! name = bracket .>> ws
        let! n = digits
        do! keywords ["자리"; "남았습니다"]
        return Statement(Assign(name, parseInt64 line n))
    }

    // [X] 잔여 N명입니다
    let assignRemain = parse {
        let! name = bracket .>> ws
        do! keywords ["잔여"] >>. ws
        let! n = digits
        do! skipString "명입니다"
        return Statement(Assign(name, parseInt64 line n))
    }

    // [X] N명 부족합니다
    let assignShort = parse {
        let! name = bracket .>> ws
        let! n = digits
        do! keywords ["명"; "부족합니다"]
        return Statement(Assign(name, -(parseInt64 line n)))
    }

    // [X] 많은 관심 부탁드립니다
    let putChar =
        bracket .>> ws .>> keywords ["많은"; "관심"; "부탁드립니다"]
        |>> (PutChar >> Statement)

    // [X] 현재 인원 공유드립니다
    let printInt =
        bracket .>> ws .>> keywords ["현재"; "인원"; "공유드립니다"]
        |>> (PrintInt >> Statement)

    // [X] 아직 마감되지 않아 한번 더 공지드립니다
    let whileStmt =
        bracket .>> ws .>> keywords ["아직"; "마감되지"; "않아"; "한번"; "더"; "공지드립니다"]
        |>> (fun name -> Statement(While(name, [])))

    // [X] 인원 미달이라
    let ifStmt =
        bracket .>> ws .>> keywords ["인원"; "미달이라"]
        |>> (fun name -> Statement(If(name, [], None)))

    // 인원이 미달이더라도
    let elseStmt =
        keywords ["인원이"; "미달이더라도"] >>% Else

    // 참고 부탁드립니다
    let blockEnd =
        keywords ["참고"; "부탁드립니다"] >>% BlockEnd

    // https://swmaestro.ai/<호출>
    let call =
        skipString "https://swmaestro.ai/" >>. rest
        |>> (fun t -> Statement(CallIgnore(parseCall line t)))

    // [X] 신청 링크: https://swmaestro.ai/<호출>
    let callAssign = parse {
        let! name = bracket
        do! ws >>. keywords ["신청"; "링크:"] >>. ws
        do! skipString "https://swmaestro.ai/"
        let! t = rest
        return Statement(CallAssign(name, parseCall line t))
    }

    choice [
        attempt functionStart
        attempt signature
        attempt declare
        attempt increment
        attempt decrement
        attempt assignSeat
        attempt assignRemain
        attempt assignShort
        attempt putChar
        attempt printInt
        attempt whileStmt
        attempt ifStmt
        attempt elseStmt
        attempt blockEnd
        attempt callAssign
        attempt call
        attempt returnStmt
    ]
    |> exact

let private parseLine line =
    match runParserOnString (lineParsers line) () (sprintf "line %d" line.Number) line.Text with
    | Success(result, _, _) -> result
    | Failure(msg, _, _) -> failAt line (sprintf "알 수 없는 구문입니다: %s (%s)" line.Text msg)

let private parseLines (code: string) =
    code.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
    |> Array.mapi (fun i text -> { Number = i + 1; Text = text.Trim() })
    |> Array.filter (fun line -> line.Text <> "")
    |> Array.map (fun line -> line, parseLine line)
    |> Array.toList

// ── 블록/프로그램 구조 파싱 ──

let parse code =
    let lines = parseLines code

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
