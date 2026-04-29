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

// ── 파싱 단위 테스트 ──

[<Fact>]
let ``파싱 - 변수 선언`` () =
    for name, parse in parsers () do
        let ast = parse "이번에 [횟수] (정원 5) 을 개설했습니다"
        Assert.Equal<Block>([Declare("횟수", 5L)], ast.TopLevel)

[<Fact>]
let ``파싱 - 한자리와 숫자 자리 구분`` () =
    for name, parse in parsers () do
        let ast = parse "[횟수] 한자리 남았습니다\n[횟수] 1자리 남았습니다"
        Assert.Equal<Block>([Decrement "횟수"; Assign("횟수", 1L)], ast.TopLevel)

// ── 실행 테스트 ──

let private hiCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72

[함수형 특강] 많은 관심 부탁드립니다

[함수형 특강] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=73
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33
"""

let private starCode = """
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/SzhTIOyVjOyVhOuztOq4sA==5&64-E66mU7J24IOuqqOuNuOungQ==42

[K8S 알아보기] 아직 마감되지 않아 한번 더 공지드립니다
    [도메인 모델링] 많은 관심 부탁드립니다
    [K8S 알아보기] 한자리 남았습니다
참고 부탁드립니다

[K8S 알아보기] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J207JuQ7KSA?SzhTIOyVjOyVhOuztOq4sA=5
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
let ``실행 - HI 출력`` () =
    for name, parse in parsers () do
        let state = runWith parse hiCode
        Assert.Equal("HI!", state.StandardOutput)
        Assert.Equal(0, state.ExitCode)

[<Fact>]
let ``실행 - 별 N개 출력`` () =
    for name, parse in parsers () do
        let state = runWith parse starCode
        Assert.Equal("*****", state.StandardOutput)

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
let ``실행 - 2-counter machine 덧셈`` () =
    for name, parse in parsers () do
        let state = runWith parse counterCode
        Assert.Equal("7", state.StandardOutput)

// ── 3개 파서 AST 동일성 검증 ──

[<Fact>]
let ``파서 동일성 - 모든 예시에서 3개 파서가 같은 AST를 생성`` () =
    let codes = [ hiCode; starCode; xoCode; multiplyCode; counterCode ]
    for code in codes do
        let asts = parsers () |> Array.map (fun (name, parse) -> name, parse code)
        let _, baseline = asts.[0]
        for i in 1 .. asts.Length - 1 do
            let name, ast = asts.[i]
            Assert.Equal<Program>(baseline, ast)
