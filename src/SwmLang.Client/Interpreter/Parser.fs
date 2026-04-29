module Parser

open System
open FParsec
open AST
open ParserHelper

type XParser<'T> = Parser<'T, unit>

// ── FParsec 기본 조합자 ──

let private ws: XParser<unit> = skipMany1Satisfy Char.IsWhiteSpace
let private rest: XParser<string> = restOfLine false

let private bracket: XParser<string> =
    between (pchar '[') (pchar ']') (many1Satisfy (fun c -> c <> ']'))

let private digits: XParser<string> =
    many1Satisfy Char.IsDigit

let private keywords (words: string list) : XParser<unit> =
    words
    |> List.map skipString
    |> List.reduce (fun a b -> a >>. ws >>. b)

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

let parse code = parseLines code |> buildProgram
