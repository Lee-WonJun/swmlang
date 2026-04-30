module Tests

open Xunit
open AST
open Interpreter

let private parsers () =
    [| "모나딕",    Parser.parse
       "수동",      ParserManual.parse
       "조합자",    ParserCombinator.parse |]

let run code =
    let ast = Parser.parse code
    interpret ast ignore

let private runWith parse code =
    let ast = parse code
    interpret ast ignore

// ── 파싱 단위 테스트 (모든 구문) ──

[<Fact>]
let ``파싱 - 변수 선언`` () =
    for _, parse in parsers () do
        let ast = parse "이번에 [횟수] (정원 5) 을 개설했습니다"
        Assert.Equal<Block>([Declare("횟수", 5L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 변수 선언 (음수)`` () =
    for _, parse in parsers () do
        let ast = parse "이번에 [잔액] (정원 -3) 을 개설했습니다"
        Assert.Equal<Block>([Declare("잔액", -3L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 증가`` () =
    for _, parse in parsers () do
        let ast = parse "[횟수] 신청 바랍니다"
        Assert.Equal<Block>([Increment "횟수"], ast.TopLevel)

[<Fact>]
let ``파싱 - 감소 (한자리)`` () =
    for _, parse in parsers () do
        let ast = parse "[횟수] 한자리 남았습니다"
        Assert.Equal<Block>([Decrement "횟수"], ast.TopLevel)

[<Fact>]
let ``파싱 - 한자리와 숫자 자리 구분`` () =
    for _, parse in parsers () do
        let ast = parse "[횟수] 한자리 남았습니다\n[횟수] 1자리 남았습니다"
        Assert.Equal<Block>([Decrement "횟수"; Assign("횟수", 1L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 대입 (N자리 남았습니다)`` () =
    for _, parse in parsers () do
        let ast = parse "[세션] 42자리 남았습니다"
        Assert.Equal<Block>([Assign("세션", 42L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 대입 (잔여 N명입니다)`` () =
    for _, parse in parsers () do
        let ast = parse "[세션] 잔여 10명입니다"
        Assert.Equal<Block>([Assign("세션", 10L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 대입 (N명 부족합니다)`` () =
    for _, parse in parsers () do
        let ast = parse "[세션] 7명 부족합니다"
        Assert.Equal<Block>([Assign("세션", -7L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 문자 출력`` () =
    for _, parse in parsers () do
        let ast = parse "[글자] 많은 관심 부탁드립니다"
        Assert.Equal<Block>([PutChar "글자"], ast.TopLevel)

[<Fact>]
let ``파싱 - 정수 출력`` () =
    for _, parse in parsers () do
        let ast = parse "[값] 현재 인원 공유드립니다"
        Assert.Equal<Block>([PrintInt "값"], ast.TopLevel)

[<Fact>]
let ``파싱 - while 루프`` () =
    for _, parse in parsers () do
        let ast = parse "[x] 아직 마감되지 않아 한번 더 공지드립니다\n[x] 신청 바랍니다\n참고 부탁드립니다"
        Assert.Equal<Block>([While("x", [Increment "x"])], ast.TopLevel)

[<Fact>]
let ``파싱 - if (then만)`` () =
    for _, parse in parsers () do
        let ast = parse "[x] 인원 미달이라\n[x] 신청 바랍니다\n참고 부탁드립니다"
        Assert.Equal<Block>([If("x", [Increment "x"], None)], ast.TopLevel)

[<Fact>]
let ``파싱 - if/else`` () =
    for _, parse in parsers () do
        let ast = parse "[x] 인원 미달이라\n[a] 신청 바랍니다\n인원이 미달이더라도\n[b] 신청 바랍니다\n참고 부탁드립니다"
        Assert.Equal<Block>([If("x", [Increment "a"], Some [Increment "b"])], ast.TopLevel)

[<Fact>]
let ``파싱 - 함수 호출`` () =
    for _, parse in parsers () do
        let ast = parse "https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72"
        let expected = CallIgnore { Name = "이원준"; Args = Map.ofList ["함수형 특강", 72L] }
        Assert.Equal<Block>([expected], ast.TopLevel)

[<Fact>]
let ``파싱 - 함수 호출 + 대입`` () =
    for _, parse in parsers () do
        let ast = parse "[결과] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72"
        let expected = CallAssign("결과", { Name = "이원준"; Args = Map.ofList ["함수형 특강", 72L] })
        Assert.Equal<Block>([expected], ast.TopLevel)

[<Fact>]
let ``파싱 - 함수 정의`` () =
    for _, parse in parsers () do
        let code = "안녕하세요 이원준 멘토입니다\n멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72\n[함수형 특강] 마감되었습니다. 감사합니다!"
        let ast = parse code
        Assert.True(ast.Functions.ContainsKey("이원준"))
        let fn = ast.Functions.["이원준"]
        Assert.Equal<(string * int64) list>(["함수형 특강", 72L], fn.Parameters)
        Assert.Equal<Block>([Return "함수형 특강"], fn.Body)

[<Fact>]
let ``파싱 - 반환`` () =
    for _, parse in parsers () do
        let code = "안녕하세요 이원준 멘토입니다\n멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==0\n[함수형 특강] 마감되었습니다. 감사합니다!"
        let ast = parse code
        let fn = ast.Functions.["이원준"]
        Assert.Equal<Block>([Return "함수형 특강"], fn.Body)

[<Fact>]
let ``파싱 - 공백 포함 변수명`` () =
    for _, parse in parsers () do
        let ast = parse "이번에 [K8S 알아보기] (정원 3) 을 개설했습니다"
        Assert.Equal<Block>([Declare("K8S 알아보기", 3L)], ast.TopLevel)

// ── 실행 테스트 ──

let private hiCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72

[함수형 특강] 많은 관심 부탁드립니다

[함수형 특강] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=101
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=108
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=108
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=111
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=32
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=65
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=83
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=87
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33
"""

let private starCode = """
이번에 [도메인 모델링] (정원 42) 을 개설했습니다
이번에 [함수형 특강] (정원 10) 을 개설했습니다
이번에 [K8S 알아보기] (정원 5) 을 개설했습니다
이번에 [아키텍처 철학] (정원 1) 을 개설했습니다
이번에 [자유멘토링] (정원 0) 을 개설했습니다
이번에 [커리어 상담] (정원 0) 을 개설했습니다

[K8S 알아보기] 아직 마감되지 않아 한번 더 공지드립니다
    [아키텍처 철학] 아직 마감되지 않아 한번 더 공지드립니다
        [자유멘토링] 신청 바랍니다
        [커리어 상담] 신청 바랍니다
        [아키텍처 철학] 한자리 남았습니다
    참고 부탁드립니다

    [커리어 상담] 아직 마감되지 않아 한번 더 공지드립니다
        [아키텍처 철학] 신청 바랍니다
        [커리어 상담] 한자리 남았습니다
    참고 부탁드립니다

    [자유멘토링] 아직 마감되지 않아 한번 더 공지드립니다
        [도메인 모델링] 많은 관심 부탁드립니다
        [자유멘토링] 한자리 남았습니다
    참고 부탁드립니다

    [함수형 특강] 많은 관심 부탁드립니다
    [아키텍처 철학] 신청 바랍니다
    [K8S 알아보기] 한자리 남았습니다
참고 부탁드립니다
"""

let private xoCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7LC97JeFIOqyve2XmOuLtA==0

이번에 [자유멘토링] (정원 0) 을 개설했습니다

[창업 경험담] 인원 미달이라
    [자유멘토링] 잔여 88명입니다
인원이 미달이더라도
    [자유멘토링] 잔여 79명입니다
참고 부탁드립니다

[자유멘토링] 마감되었습니다. 감사합니다!


이번에 [오후 5시] (정원 0) 을 개설했습니다
이번에 [오후 6시] (정원 0) 을 개설했습니다

[오후 5시] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7LC97JeFIOqyve2XmOuLtA=0
[오후 6시] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7LC97JeFIOqyve2XmOuLtA=5

[오후 5시] 많은 관심 부탁드립니다
[오후 6시] 많은 관심 부탁드립니다
"""

let private multiplyCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZSE66Gt7YWM7YGsIOuPhOuplOyduA==3&7IukIOyCrOyaqeyekOulvCDrgYzslrTrqqjsnLzripQg6riw7ZqN==4

이번에 [자유멘토링] (정원 0) 을 개설했습니다

[프롭테크 도메인] 아직 마감되지 않아 한번 더 공지드립니다
    이번에 [아키텍처 철학] (정원 0) 을 개설했습니다
    [아키텍처 철학] 잔여 4명입니다
    
    [아키텍처 철학] 아직 마감되지 않아 한번 더 공지드립니다
        [자유멘토링] 신청 바랍니다
        [아키텍처 철학] 한자리 남았습니다
    참고 부탁드립니다
    
    [프롭테크 도메인] 한자리 남았습니다
참고 부탁드립니다

[자유멘토링] 마감되었습니다. 감사합니다!


이번에 [커리어 상담] (정원 0) 을 개설했습니다
[커리어 상담] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7ZSE66Gt7YWM7YGsIOuPhOuplOyduA=3&7IukIOyCrOyaqeyekOulvCDrgYzslrTrqqjsnLzripQg6riw7ZqN=4
[커리어 상담] 현재 인원 공유드립니다
"""

let private counterCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==0&7LC97JeFIOqyve2XmOuLtA==0

[함수형 특강] 아직 마감되지 않아 한번 더 공지드립니다
    [창업 경험담] 신청 바랍니다
    [함수형 특강] 한자리 남았습니다
참고 부탁드립니다

[창업 경험담] 마감되었습니다. 감사합니다!


이번에 [결과] (정원 0) 을 개설했습니다
[결과] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=3&7LC97JeFIOqyve2XmOuLtA=4
[결과] 현재 인원 공유드립니다
"""

[<Fact>]
let ``실행 - Hello ASW!! 출력`` () =
    for name, parse in parsers () do
        let state = runWith parse hiCode
        Assert.Equal("Hello ASW!!", state.StandardOutput)
        Assert.Equal(0, state.ExitCode)

[<Fact>]
let ``실행 - 별 삼각형 출력`` () =
    for name, parse in parsers () do
        let state = runWith parse starCode
        Assert.Equal("*\n**\n***\n****\n*****\n", state.StandardOutput)

[<Fact>]
let ``실행 - 분기 XO`` () =
    for name, parse in parsers () do
        let state = runWith parse xoCode
        Assert.Equal("XO", state.StandardOutput)

[<Fact>]
let ``실행 - 곱셈 예제`` () =
    for name, parse in parsers () do
        let state = runWith parse multiplyCode
        Assert.Equal("12", state.StandardOutput)

[<Fact>]
let ``실행 - top-level return은 exit code`` () =
    let code = "이번에 [종료] (정원 257) 을 개설했습니다\n[종료] 마감되었습니다. 감사합니다!\n[종료] 현재 인원 공유드립니다"
    for name, parse in parsers () do
        let state = runWith parse code
        Assert.Equal(1, state.ExitCode)
        Assert.Equal("", state.StandardOutput)

[<Fact>]
let ``실행 - exit code 예제`` () =
    let code = "이번에 [도메인 모델링] (정원 3) 을 개설했습니다\n[도메인 모델링] 신청 바랍니다\n[도메인 모델링] 신청 바랍니다\n[도메인 모델링] 마감되었습니다. 감사합니다!"
    for _, parse in parsers () do
        let state = runWith parse code
        Assert.Equal(5, state.ExitCode)
        Assert.Equal("", state.StandardOutput)

[<Fact>]
let ``실행 - 2-counter machine 덧셈`` () =
    for name, parse in parsers () do
        let state = runWith parse counterCode
        Assert.Equal("7", state.StandardOutput)

// ── 3개 파서 AST 동일성 검증 ──

let private multiCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72

[함수형 특강] 많은 관심 부탁드립니다

[함수형 특강] 마감되었습니다. 감사합니다!


안녕하세요 김민수 멘토입니다
멘토 소개: https://notion.so/7Luk66as7Ja0IOyDgeuLtA==0

https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=101
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=108
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=108
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=111
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=32
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=65
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=83
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=87
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33

[커리어 상담] 마감되었습니다. 감사합니다!


https://swmaestro.ai/6rmA66-87IiY
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33
"""

[<Fact>]
let ``실행 - 다중 함수 호출`` () =
    for _, parse in parsers () do
        let state = runWith parse multiCode
        Assert.Equal("Hello ASW!!", state.StandardOutput)

[<Fact>]
let ``파서 동일성 - 모든 예시에서 3개 파서가 같은 AST를 생성`` () =
    let codes = [ hiCode; starCode; xoCode; multiplyCode; counterCode; multiCode ]
    for code in codes do
        let asts = parsers () |> Array.map (fun (name, parse) -> name, parse code)
        let _, baseline = asts.[0]
        for i in 1 .. asts.Length - 1 do
            let name, ast = asts.[i]
            Assert.Equal<Program>(baseline, ast)
