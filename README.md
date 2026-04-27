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

## 테스트

```bash
dotnet test SwmLang.sln
```

현재 테스트는 명세의 `HI!`, 별 출력, `XO`, 곱셈, top-level exit code, `한자리`/`1자리` 구분을 검증합니다.

WSL에서 Windows SDK를 사용할 때:

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' test SwmLang.sln
```

## 웹 플레이그라운드

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' run --project src/SwmLang.Client/SwmLang.Client.fsproj --urls http://localhost:5197
```

브라우저에서 `http://localhost:5197`을 열면 됩니다.
