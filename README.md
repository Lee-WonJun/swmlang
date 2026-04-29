# swmlang

Software Maestro Programming Language.

`swmlang`은 SW마에스트로 멘토방 공지 메시지처럼 보이는 esolang입니다. 이 구현은 `../CobangLang`과 같은 F#/.NET + xUnit 테스트 스택을 사용합니다.

## 구조

```text
SwmLang.sln
src/SwmLang.Client/Interpreter/
  AST.fs
  Parser.fs
  Interpreter.fs
SwmLang.Test/
examples/
```

## 실행 모델

- 확장자: `.swm`
- 값 타입: `int64`
- 변수 스코프: 함수 지역, top-level도 별도 frame
- 함수 정의는 파싱 시 등록되고, top-level 코드가 엔트리포인트입니다.
- top-level의 `[X] 마감되었습니다. 감사합니다!`는 `X & 0xFF`를 exit code로 사용합니다.
- 식별자 인코딩: base64url (패딩 없음, `+/` 대신 `-_`)

## 문법

### 변수

| 구문 | 의미 |
|------|------|
| `이번에 [X] (정원 N) 을 개설했습니다` | 변수 X를 N으로 선언/초기화 |
| `[X] 신청 바랍니다` | X++ |
| `[X] 한자리 남았습니다` | X-- |
| `[X] 잔여 N명입니다` | X = N |
| `[X] N자리 남았습니다` | X = N |
| `[X] N명 부족합니다` | X = -N |

### 출력

| 구문 | 의미 |
|------|------|
| `[X] 많은 관심 부탁드립니다` | `putchar(X & 0xFF)` |
| `[X] 현재 인원 공유드립니다` | `print(X)` (정수 출력) |

### 제어 흐름

**while** — X가 0이 아닌 동안 반복:

```
[X] 아직 마감되지 않아 한번 더 공지드립니다
    ...본문...
참고 부탁드립니다
```

**if/else** — X가 0이면 then, 아니면 else:

```
[X] 인원 미달이라
    ...then...
인원이 미달이더라도
    ...else...
참고 부탁드립니다
```

### 함수

**정의** — `안녕하세요 함수명 멘토입니다`로 시작, 시그니처에 파라미터 기본값 지정:

```
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/파라미터1_base64url==기본값&파라미터2_base64url==기본값
    ...본문...
[반환변수] 마감되었습니다. 감사합니다!
```

**호출** — URL path가 함수명, query string이 인자:

```
https://swmaestro.ai/함수명_base64url?파라미터1_base64url=값&파라미터2_base64url=값
```

**호출 + 대입**:

```
[결과] 신청 링크: https://swmaestro.ai/함수명_base64url?파라미터_base64url=값
```

**반환** — top-level에서는 exit code (`X & 0xFF`):

```
[X] 마감되었습니다. 감사합니다!
```

## 예시

### HI! 출력 (`examples/hi.swm`)

함수 `이원준(함수형 특강=72)`을 정의하고, putchar로 문자를 출력한 뒤 반환. top-level에서 H(72), I(73), !(33)을 인자로 세 번 호출합니다.

### 별 찍기 (`examples/star.swm`)

함수 `이원준(K8S 알아보기=5, 도메인 모델링=42)`을 정의. while 루프로 `*`(ASCII 42)를 5번 출력합니다.

### XO 분기 (`examples/xo.swm`)

함수 `이원준(창업 경험담=0)`: 인자가 0이면 88(`X`), 아니면 79(`O`)를 반환. top-level에서 0과 5로 호출하여 `XO`를 출력합니다.

### 곱셈 (`examples/multiply.swm`)

함수 `이원준(프롭테크 도메인=3, 실 사용자를 끌어모으는 기획=4)`: 이중 while 루프로 곱셈을 구현. 3×4=12를 출력합니다.

### 2-Counter Machine 덧셈 (`examples/counter.swm`)

함수 `이원준(함수형 특강=0, 창업 경험담=0)`: while 함수형 특강≠0 동안 창업 경험담++, 함수형 특강--. 호출 시 함수형 특강=3, 창업 경험담=4를 전달하여 3+4=7을 출력합니다.

swmlang의 프리미티브(increment, decrement, zero-test while)는 2-counter machine의 명령어 집합과 정확히 대응하므로, 이 언어는 **튜링 완전**합니다.

## 테스트

```bash
dotnet test SwmLang.sln
```

WSL에서 Windows SDK를 사용할 때:

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' test SwmLang.sln
```

## 웹 플레이그라운드

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' run --project src/SwmLang.Client/SwmLang.Client.fsproj --urls http://localhost:5197
```

브라우저에서 `http://localhost:5197`을 열면 됩니다.
