module ParserManual

open System
open System.Text
open AST

type ParseError = {
    Line: int
    Message: string
}

exception ParseException of ParseError

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
        let bytes = Convert.FromBase64String(padded)
        Encoding.UTF8.GetString(bytes)
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
    if String.IsNullOrWhiteSpace(text) then
        []
    else
        text.Split('&', StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun item ->
            match splitLast "==" item with
            | Some(encoded, valueText) ->
                decodeBase64Url line encoded, parseInt64 line valueText
            | None -> failAt line (sprintf "시그니처 파라미터 형식이 올바르지 않습니다: %s" item))
        |> Array.toList

let private parseCall line (text: string) =
    let path, query =
        let index = text.IndexOf('?')
        if index < 0 then text, ""
        else text.Substring(0, index), text.Substring(index + 1)

    let args =
        if String.IsNullOrWhiteSpace(query) then
            Map.empty
        else
            query.Split('&', StringSplitOptions.RemoveEmptyEntries)
            |> Array.map (fun item ->
                match splitLast "=" item with
                | Some(encoded, valueText) ->
                    decodeBase64Url line encoded, parseInt64 line valueText
                | None -> failAt line (sprintf "호출 인자 형식이 올바르지 않습니다: %s" item))
            |> Map.ofArray

    { Name = decodeBase64Url line path; Args = args }

let private hasReachableReturn body =
    let rec statementHasReturn stmt =
        match stmt with
        | Return _ -> true
        | While(_, block) -> block |> List.exists statementHasReturn
        | If(_, thenBlock, elseBlock) ->
            (thenBlock |> List.exists statementHasReturn)
            || (elseBlock |> Option.exists (List.exists statementHasReturn))
        | _ -> false

    body |> List.exists statementHasReturn

// ── 문자열 유틸 ──

let private tryStripPrefix (prefix: string) (text: string) =
    if text.StartsWith(prefix, StringComparison.Ordinal) then
        Some(text.Substring(prefix.Length))
    else None

let private tryStripSuffix (suffix: string) (text: string) =
    if text.EndsWith(suffix, StringComparison.Ordinal) then
        Some(text.Substring(0, text.Length - suffix.Length))
    else None

let private skipWhitespace (text: string) (pos: int) =
    let mutable i = pos
    while i < text.Length && Char.IsWhiteSpace(text.[i]) do
        i <- i + 1
    if i > pos then Some i else None

let private tryReadBracket (text: string) =
    if text.Length < 2 || text.[0] <> '[' then None
    else
        let close = text.IndexOf(']', 1)
        if close < 0 then None
        else Some(text.Substring(1, close - 1), text.Substring(close + 1))

let private tryReadDigits (text: string) =
    let mutable i = 0
    while i < text.Length && Char.IsDigit(text.[i]) do
        i <- i + 1
    if i > 0 then Some(text.Substring(0, i), text.Substring(i))
    else None

let private expectWs (rest: string) =
    if rest.Length > 0 && Char.IsWhiteSpace(rest.[0]) then
        Some(rest.TrimStart())
    else None

// ── 라인 파싱 ──

let private parseLine (line: SourceLine) : LineNode =
    let text = line.Text

    // 안녕하세요 <name> 멘토입니다
    match tryStripPrefix "안녕하세요" text with
    | Some rest ->
        match expectWs rest with
        | Some rest ->
            match tryStripSuffix "멘토입니다" rest with
            | Some nameRest ->
                let name = nameRest.TrimEnd()
                if name.Length > 0 && not (name.Contains(' ')) then
                    FunctionStart name
                else failAt line (sprintf "알 수 없는 구문입니다: %s" text)
            | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
        | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | None ->

    // 멘토 소개: https://notion.so/...
    match tryStripPrefix "멘토" text with
    | Some rest when rest.Length > 0 && Char.IsWhiteSpace(rest.[0]) ->
        let rest = rest.TrimStart()
        match tryStripPrefix "소개:" rest with
        | Some rest ->
            let rest = rest.TrimStart()
            match tryStripPrefix "https://notion.so/" rest with
            | Some sigText -> Signature sigText
            | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
        | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | _ ->

    // 이번에 [X] (정원 N) 을 개설했습니다
    match tryStripPrefix "이번에" text with
    | Some rest ->
        match expectWs rest with
        | Some rest ->
            match tryReadBracket rest with
            | Some(name, rest) ->
                match expectWs rest with
                | Some rest ->
                    match tryStripPrefix "(정원" rest with
                    | Some rest ->
                        match expectWs rest with
                        | Some rest ->
                            match tryStripSuffix "을 개설했습니다" rest with
                            | Some rest ->
                                let rest = rest.TrimEnd()
                                match tryStripSuffix ")" rest with
                                | Some numText ->
                                    Statement(Declare(name, parseInt64 line numText))
                                | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                            | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                        | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                    | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
            | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
        | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | None ->

    // 인원이 미달이더라도
    match tryStripPrefix "인원이" text with
    | Some rest ->
        match expectWs rest with
        | Some rest when rest = "미달이더라도" -> Else
        | _ -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | None ->

    // 참고 부탁드립니다
    match tryStripPrefix "참고" text with
    | Some rest ->
        match expectWs rest with
        | Some rest when rest = "부탁드립니다" -> BlockEnd
        | _ -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | None ->

    // https://swmaestro.ai/...
    match tryStripPrefix "https://swmaestro.ai/" text with
    | Some callText -> Statement(CallIgnore(parseCall line callText))
    | None ->

    // [X] ...
    match tryReadBracket text with
    | Some(name, rest) ->
        match expectWs rest with
        | Some rest ->
            // [X] 마감되었습니다. 감사합니다!
            if rest = "마감되었습니다. 감사합니다!" then
                Statement(Return name)
            // [X] 신청 바랍니다
            elif rest = "신청 바랍니다" then
                Statement(Increment name)
            // [X] 한자리 남았습니다
            elif rest = "한자리 남았습니다" then
                Statement(Decrement name)
            // [X] 많은 관심 부탁드립니다
            elif rest = "많은 관심 부탁드립니다" then
                Statement(PutChar name)
            // [X] 현재 인원 공유드립니다
            elif rest = "현재 인원 공유드립니다" then
                Statement(PrintInt name)
            // [X] 아직 마감되지 않아 한번 더 공지드립니다
            elif rest = "아직 마감되지 않아 한번 더 공지드립니다" then
                Statement(While(name, []))
            // [X] 인원 미달이라
            elif rest = "인원 미달이라" then
                Statement(If(name, [], None))
            // [X] 잔여 N명입니다
            elif rest.StartsWith("잔여", StringComparison.Ordinal) then
                let rest = rest.Substring(2).TrimStart()
                match tryStripSuffix "명입니다" rest with
                | Some numText -> Statement(Assign(name, parseInt64 line numText))
                | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
            // [X] 신청 링크: https://swmaestro.ai/...
            elif rest.StartsWith("신청", StringComparison.Ordinal) then
                let rest2 = rest.Substring(2).TrimStart()
                match tryStripPrefix "링크:" rest2 with
                | Some rest3 ->
                    let rest3 = rest3.TrimStart()
                    match tryStripPrefix "https://swmaestro.ai/" rest3 with
                    | Some callText -> Statement(CallAssign(name, parseCall line callText))
                    | None ->
                        // [X] 신청 바랍니다 — already handled above
                        failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                | None ->
                    failAt line (sprintf "알 수 없는 구문입니다: %s" text)
            else
                // [X] N자리 남았습니다 / [X] N명 부족합니다
                match tryReadDigits rest with
                | Some(digits, afterDigits) ->
                    if afterDigits.StartsWith("자리", StringComparison.Ordinal) then
                        let afterDigits = afterDigits.Substring(2).TrimStart()
                        if afterDigits = "남았습니다" then
                            Statement(Assign(name, parseInt64 line digits))
                        else failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                    elif afterDigits.StartsWith("명", StringComparison.Ordinal) then
                        let afterDigits = afterDigits.Substring(1).TrimStart()
                        if afterDigits = "부족합니다" then
                            Statement(Assign(name, -(parseInt64 line digits)))
                        else failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                    else failAt line (sprintf "알 수 없는 구문입니다: %s" text)
                | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
        | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)
    | None -> failAt line (sprintf "알 수 없는 구문입니다: %s" text)

// ── 블록/프로그램 구조 파싱 ──

let private parseLines (code: string) =
    code.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
    |> Array.mapi (fun index text -> { Number = index + 1; Text = text.Trim() })
    |> Array.filter (fun line -> line.Text <> "")
    |> Array.map (fun line -> line, parseLine line)
    |> Array.toList

let parse code =
    let lines = parseLines code

    let rec parseBlock stopAt index =
        let rec loop index acc =
            if index >= lines.Length then
                acc |> List.rev, index, None
            else
                let line, node = lines.[index]
                if stopAt |> List.exists (fun stop -> stop node) then
                    acc |> List.rev, index, Some node
                else
                    let statement, nextIndex = parseStatement index
                    loop nextIndex (statement :: acc)

        loop index []

    and parseStatement index =
        let line, node = lines.[index]
        match node with
        | Statement(While(name, [])) ->
            let body, endIndex, stop = parseBlock [ (fun node -> node = BlockEnd) ] (index + 1)
            match stop with
            | Some _ -> While(name, body), endIndex + 1
            | None -> failAt line "while 블록 종료문이 없습니다"
        | Statement(If(name, [], None)) ->
            let thenBlock, stopIndex, stop = parseBlock [ (fun node -> node = Else); (fun node -> node = BlockEnd) ] (index + 1)
            match stop with
            | Some Else ->
                let elseBlock, endIndex, endStop = parseBlock [ (fun node -> node = BlockEnd) ] (stopIndex + 1)
                match endStop with
                | Some _ -> If(name, thenBlock, Some elseBlock), endIndex + 1
                | None -> failAt line "if else 블록 종료문이 없습니다"
            | Some BlockEnd -> If(name, thenBlock, None), stopIndex + 1
            | _ -> failAt line "if 블록 종료문이 없습니다"
        | Statement statement -> statement, index + 1
        | FunctionStart _
        | Signature _
        | Else
        | BlockEnd ->
            failAt line (sprintf "예상하지 못한 블록/함수 구문입니다: %s" line.Text)

    let rec parseFunctionBody index acc =
        if index >= lines.Length then
            acc |> List.rev, index
        else
            let statement, nextIndex = parseStatement index
            let nextAcc = statement :: acc
            match statement with
            | Return _ -> nextAcc |> List.rev, nextIndex
            | _ -> parseFunctionBody nextIndex nextAcc

    let rec parseTop index topLevel functions =
        if index >= lines.Length then
            let functionMap =
                functions
                |> List.map (fun fn -> fn.Name, fn)
                |> Map.ofList
            { Functions = functionMap; TopLevel = topLevel |> List.rev }
        else
            let line, node = lines.[index]
            match node with
            | FunctionStart name ->
                if index + 1 >= lines.Length then failAt line "함수 시그니처가 없습니다"
                let sigLine, sigNode = lines.[index + 1]
                match sigNode with
                | Signature signatureText ->
                    let body, nextIndex = parseFunctionBody (index + 2) []
                    if not (hasReachableReturn body) then failAt line (sprintf "함수 '%s'에 종료문이 없습니다" name)
                    let fn = { Name = name; Parameters = parseSignature sigLine signatureText; Body = body }
                    parseTop nextIndex topLevel (fn :: functions)
                | _ -> failAt sigLine "함수 시그니처가 아닙니다"
            | _ ->
                let statement, nextIndex = parseStatement index
                parseTop nextIndex (statement :: topLevel) functions

    parseTop 0 [] []
