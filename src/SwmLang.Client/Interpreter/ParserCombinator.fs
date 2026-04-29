module ParserCombinator

open System
open FParsec
open AST
open ParserHelper

type XParser<'T> = Parser<'T, unit>

// ── FParsec 기본 조합자 ──

let private ws1: XParser<unit> = skipMany1Satisfy Char.IsWhiteSpace
let private rest: XParser<string> = restOfLine false
let private eofLine: XParser<unit> = eof

let private bracketIdentifier: XParser<string> =
    between (pchar '[') (pchar ']') (many1Satisfy (fun c -> c <> ']'))

let private unsignedNumberText: XParser<string> =
    many1Satisfy Char.IsDigit

let private signedNumber: XParser<int64> =
    pint64

let private exact p = p .>> eofLine

// ── 라인 파서 ──

let private lineParsers (line: SourceLine) : XParser<LineNode> =
    let functionStart =
        pipe2
            (pstring "안녕하세요" >>. ws1 >>. many1Satisfy (fun c -> not (Char.IsWhiteSpace c)))
            (ws1 >>. pstring "멘토입니다")
            (fun name _ -> FunctionStart name)

    let signature =
        pstring "멘토" >>. ws1 >>. pstring "소개:" >>. ws1 >>. pstring "https://notion.so/" >>. rest
        |>> Signature

    let returnLine =
        bracketIdentifier .>> ws1 .>> pstring "마감되었습니다. 감사합니다!"
        |>> (Return >> Statement)

    let declareLine =
        pipe2
            (pstring "이번에" >>. ws1 >>. bracketIdentifier)
            (ws1 >>. pstring "(정원" >>. ws1 >>. signedNumber .>> pstring ")" .>> ws1 .>> pstring "을" .>> ws1 .>> pstring "개설했습니다")
            (fun name value -> Statement(Declare(name, value)))

    let incrementLine =
        bracketIdentifier .>> ws1 .>> pstring "신청" .>> ws1 .>> pstring "바랍니다"
        |>> (Increment >> Statement)

    let decrementLine =
        bracketIdentifier .>> ws1 .>> pstring "한자리" .>> ws1 .>> pstring "남았습니다"
        |>> (Decrement >> Statement)

    let assignSeatLine =
        pipe2
            (bracketIdentifier .>> ws1)
            (unsignedNumberText .>> pstring "자리" .>> ws1 .>> pstring "남았습니다")
            (fun name value -> Statement(Assign(name, parseInt64 line value)))

    let assignRemainLine =
        pipe2
            (bracketIdentifier .>> ws1 .>> pstring "잔여" .>> ws1)
            (unsignedNumberText .>> pstring "명입니다")
            (fun name value -> Statement(Assign(name, parseInt64 line value)))

    let assignShortLine =
        pipe2
            (bracketIdentifier .>> ws1)
            (unsignedNumberText .>> pstring "명" .>> ws1 .>> pstring "부족합니다")
            (fun name value -> Statement(Assign(name, -(parseInt64 line value))))

    let putCharLine =
        bracketIdentifier .>> ws1 .>> pstring "많은" .>> ws1 .>> pstring "관심" .>> ws1 .>> pstring "부탁드립니다"
        |>> (PutChar >> Statement)

    let printIntLine =
        bracketIdentifier .>> ws1 .>> pstring "현재" .>> ws1 .>> pstring "인원" .>> ws1 .>> pstring "공유드립니다"
        |>> (PrintInt >> Statement)

    let whileLine =
        bracketIdentifier .>> ws1 .>> pstring "아직" .>> ws1 .>> pstring "마감되지" .>> ws1 .>> pstring "않아" .>> ws1 .>> pstring "한번" .>> ws1 .>> pstring "더" .>> ws1 .>> pstring "공지드립니다"
        |>> (fun name -> Statement(While(name, [])))

    let ifLine =
        bracketIdentifier .>> ws1 .>> pstring "인원" .>> ws1 .>> pstring "미달이라"
        |>> (fun name -> Statement(If(name, [], None)))

    let elseLine =
        pstring "인원이" >>. ws1 >>. pstring "미달이더라도" >>% Else

    let blockEndLine =
        pstring "참고" >>. ws1 >>. pstring "부탁드립니다" >>% BlockEnd

    let callLine =
        pstring "https://swmaestro.ai/" >>. rest
        |>> (fun callText -> Statement(CallIgnore(parseCall line callText)))

    let callAssignLine =
        pipe2
            (bracketIdentifier .>> ws1 .>> pstring "신청" .>> ws1 .>> pstring "링크:" .>> ws1 .>> pstring "https://swmaestro.ai/")
            rest
            (fun name callText -> Statement(CallAssign(name, parseCall line callText)))

    choice [
        attempt functionStart
        attempt signature
        attempt declareLine
        attempt incrementLine
        attempt decrementLine
        attempt assignSeatLine
        attempt assignRemainLine
        attempt assignShortLine
        attempt putCharLine
        attempt printIntLine
        attempt whileLine
        attempt ifLine
        attempt elseLine
        attempt blockEndLine
        attempt callAssignLine
        attempt callLine
        attempt returnLine
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
