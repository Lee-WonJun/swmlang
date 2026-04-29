module ParserManual

open System
open AST
open ParserHelper

// ── 문자열 유틸 ──

let private tryStripPrefix (prefix: string) (text: string) =
    if text.StartsWith(prefix, StringComparison.Ordinal) then
        Some(text.Substring(prefix.Length))
    else None

let private tryStripSuffix (suffix: string) (text: string) =
    if text.EndsWith(suffix, StringComparison.Ordinal) then
        Some(text.Substring(0, text.Length - suffix.Length))
    else None

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

let private fail line text = failAt line (sprintf "알 수 없는 구문입니다: %s" text)

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
                if name.Length > 0 && not (name.Contains(' ')) then FunctionStart name
                else fail line text
            | None -> fail line text
        | None -> fail line text
    | None ->

    // 멘토 소개: https://notion.so/...
    match tryStripPrefix "멘토" text with
    | Some rest when rest.Length > 0 && Char.IsWhiteSpace(rest.[0]) ->
        let rest = rest.TrimStart()
        match tryStripPrefix "소개:" rest with
        | Some rest ->
            match tryStripPrefix "https://notion.so/" (rest.TrimStart()) with
            | Some sigText -> Signature sigText
            | None -> fail line text
        | None -> fail line text
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
                                match tryStripSuffix ")" (rest.TrimEnd()) with
                                | Some numText -> Statement(Declare(name, parseInt64 line numText))
                                | None -> fail line text
                            | None -> fail line text
                        | None -> fail line text
                    | None -> fail line text
                | None -> fail line text
            | None -> fail line text
        | None -> fail line text
    | None ->

    // 인원이 미달이더라도
    match tryStripPrefix "인원이" text with
    | Some rest ->
        match expectWs rest with
        | Some rest when rest = "미달이더라도" -> Else
        | _ -> fail line text
    | None ->

    // 참고 부탁드립니다
    match tryStripPrefix "참고" text with
    | Some rest ->
        match expectWs rest with
        | Some rest when rest = "부탁드립니다" -> BlockEnd
        | _ -> fail line text
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
            if rest = "마감되었습니다. 감사합니다!" then
                Statement(Return name)
            elif rest = "신청 바랍니다" then
                Statement(Increment name)
            elif rest = "한자리 남았습니다" then
                Statement(Decrement name)
            elif rest = "많은 관심 부탁드립니다" then
                Statement(PutChar name)
            elif rest = "현재 인원 공유드립니다" then
                Statement(PrintInt name)
            elif rest = "아직 마감되지 않아 한번 더 공지드립니다" then
                Statement(While(name, []))
            elif rest = "인원 미달이라" then
                Statement(If(name, [], None))
            elif rest.StartsWith("잔여", StringComparison.Ordinal) then
                let rest = rest.Substring(2).TrimStart()
                match tryStripSuffix "명입니다" rest with
                | Some numText -> Statement(Assign(name, parseInt64 line numText))
                | None -> fail line text
            elif rest.StartsWith("신청", StringComparison.Ordinal) then
                let rest2 = rest.Substring(2).TrimStart()
                match tryStripPrefix "링크:" rest2 with
                | Some rest3 ->
                    match tryStripPrefix "https://swmaestro.ai/" (rest3.TrimStart()) with
                    | Some callText -> Statement(CallAssign(name, parseCall line callText))
                    | None -> fail line text
                | None -> fail line text
            else
                match tryReadDigits rest with
                | Some(digits, afterDigits) ->
                    if afterDigits.StartsWith("자리", StringComparison.Ordinal) then
                        let afterDigits = afterDigits.Substring(2).TrimStart()
                        if afterDigits = "남았습니다" then Statement(Assign(name, parseInt64 line digits))
                        else fail line text
                    elif afterDigits.StartsWith("명", StringComparison.Ordinal) then
                        let afterDigits = afterDigits.Substring(1).TrimStart()
                        if afterDigits = "부족합니다" then Statement(Assign(name, -(parseInt64 line digits)))
                        else fail line text
                    else fail line text
                | None -> fail line text
        | None -> fail line text
    | None -> fail line text

// ── 공개 API ──

let private parseLines (code: string) =
    code.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
    |> Array.mapi (fun i text -> { Number = i + 1; Text = text.Trim() })
    |> Array.filter (fun line -> line.Text <> "")
    |> Array.map (fun line -> line, parseLine line)
    |> Array.toList

let parse code = parseLines code |> buildProgram
