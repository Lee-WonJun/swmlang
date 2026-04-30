# SwmLang에 오신 것을 환영합니다

Software Maestro Programming Language, `SwmLang`은 SW마에스트로 멘티들이 익숙하고 수도 없이 봐온 형식으로 코드를 작성하며, 개발에 대한 의지와 열의를 다시 불태우기 위한 High-Level Language입니다.

낯선 기호와 차가운 문법 대신 익숙한 표현 속에서 값의 변화, 조건, 반복, 함수 호출을 다루며, 프로그래밍의 기본 원리를 조금 더 친근하고 장엄하게 학습할 수 있도록 설계되었습니다.

```swm
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
```

신청 링크, 정원, 잔여 인원 같은 표현으로 변수와 제어 흐름을 다룹니다. 값은 `int64` 정수 하나만 사용하며, 함수 호출은 SW마에스트로 링크처럼 생긴 URL로 작성합니다.

## 사용해보기

웹 플레이그라운드에서 바로 실행할 수 있습니다.

https://lee-wonjun.github.io/swmlang/

로컬에서 실행하려면 다음 명령을 사용합니다.

```bash
dotnet run --project src/SwmLang.Client/SwmLang.Client.fsproj --urls http://localhost:5197
```

브라우저에서 `http://localhost:5197`을 열면 됩니다.

테스트는 다음 명령으로 실행합니다.

```bash
dotnet test SwmLang.sln
```

## 문법

### 변수 선언

```swm
이번에 [도메인 모델링] (정원 3) 을 개설했습니다
```

`도메인 모델링` 변수를 선언하고 `3`으로 초기화합니다. 변수명은 대괄호 안에 작성하며 공백을 포함할 수 있습니다.

### 변수 증감

```swm
[도메인 모델링] 신청 바랍니다
[도메인 모델링] 한자리 남았습니다
```

`신청 바랍니다`는 값을 1 증가시키고, `한자리 남았습니다`는 값을 1 감소시킵니다.

### 변수 대입

```swm
[자유멘토링] 잔여 10명입니다
[자유멘토링] 10자리 남았습니다
[자유멘토링] 7명 부족합니다
```

`잔여 N명입니다`와 `N자리 남았습니다`는 값을 `N`으로 바꿉니다. `N명 부족합니다`는 값을 `-N`으로 바꿉니다.

### 출력

```swm
[도메인 모델링] 현재 인원 공유드립니다
[도메인 모델링] 많은 관심 부탁드립니다
```

`현재 인원 공유드립니다`는 정수를 출력합니다. `많은 관심 부탁드립니다`는 값의 하위 8비트를 ASCII 문자로 출력합니다.

### 조건문

```swm
[창업 경험담] 인원 미달이라
    [자유멘토링] 잔여 88명입니다
인원이 미달이더라도
    [자유멘토링] 잔여 79명입니다
참고 부탁드립니다
```

`[X] 인원 미달이라`는 `X == 0`일 때 첫 번째 블록을 실행합니다. `인원이 미달이더라도` 뒤의 블록은 `X != 0`일 때 실행되며, 필요 없으면 생략할 수 있습니다.

### 반복문

```swm
[K8S 알아보기] 아직 마감되지 않아 한번 더 공지드립니다
    [도메인 모델링] 많은 관심 부탁드립니다
    [K8S 알아보기] 한자리 남았습니다
참고 부탁드립니다
```

`[X] 아직 마감되지 않아 한번 더 공지드립니다`는 `X != 0`인 동안 블록을 반복합니다.

### 함수 정의

```swm
안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72

[함수형 특강] 많은 관심 부탁드립니다
[함수형 특강] 마감되었습니다. 감사합니다!
```

함수는 `안녕하세요 {함수명} 멘토입니다`로 시작합니다. 다음 줄의 `멘토 소개` 링크에는 파라미터 이름과 기본값을 적습니다.

파라미터 이름은 base64url로 인코딩하고, 기본값은 `==` 뒤에 적습니다. 여러 파라미터는 `&`로 연결합니다.

```text
멘토 소개: https://notion.so/{파라미터_base64url}=={기본값}&{파라미터_base64url}=={기본값}
```

함수 본문은 `[X] 마감되었습니다. 감사합니다!`를 만나면 `X` 값을 반환합니다.

### 함수 호출

```swm
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
```

`https://swmaestro.ai/{함수명_base64url}` 형식으로 함수를 호출합니다. 인자는 query string으로 전달합니다.

반환값을 변수에 저장하려면 신청 링크로 작성합니다.

```swm
[결과] 신청 링크: https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
```

### 반환과 종료 코드

```swm
[결과] 마감되었습니다. 감사합니다!
```

함수 안에서는 `결과` 값을 호출한 곳으로 반환합니다. top-level에서는 프로그램 실행을 끝내고 `결과 & 0xFF`를 exit code로 사용합니다.

## 문법 요약

| 구문 | 의미 |
|------|------|
| `이번에 [X] (정원 N) 을 개설했습니다` | 변수 `X = N` 선언 |
| `[X] 신청 바랍니다` | `X++` |
| `[X] 한자리 남았습니다` | `X--` |
| `[X] 잔여 N명입니다` | `X = N` |
| `[X] N자리 남았습니다` | `X = N` |
| `[X] N명 부족합니다` | `X = -N` |
| `[X] 현재 인원 공유드립니다` | 정수 출력 |
| `[X] 많은 관심 부탁드립니다` | ASCII 문자 출력 |
| `[X] 인원 미달이라` | `X == 0` 조건문 시작 |
| `인원이 미달이더라도` | else 블록 시작 |
| `[X] 아직 마감되지 않아 한번 더 공지드립니다` | `X != 0` 반복문 시작 |
| `참고 부탁드립니다` | 조건문/반복문 종료 |
| `[X] 마감되었습니다. 감사합니다!` | 반환 또는 top-level 종료 |

## 예시

### Hello ASW!! 출력

`examples/hi.swm`은 문자 하나를 출력하는 `이원준` 함수를 정의하고, `Hello ASW!!`에 해당하는 ASCII 값을 순서대로 전달해 문자열을 출력합니다.

```swm
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
```

### 별 찍기

`examples/star.swm`은 반복문으로 현재 정원을 복사하고 복구하면서 별을 삼각형 모양으로 출력합니다.

```swm
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
```

### XO 분기

`examples/xo.swm`은 인자가 0이면 `X`, 0이 아니면 `O`를 반환하는 조건문 예제입니다.

```swm
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
```

### 곱셈

`examples/multiply.swm`은 중첩 반복문으로 `3 * 4 = 12`를 계산합니다.

```swm
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
```

### 2-Counter Machine 덧셈

`examples/counter.swm`은 한 변수에서 값을 하나씩 빼면서 다른 변수에 더해 `3 + 4 = 7`을 만듭니다.

```swm
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
```

SwmLang의 기본 연산은 increment, decrement, zero-test while로 구성되어 2-counter machine의 명령어 집합과 대응합니다. 따라서 SwmLang은 튜링 완전합니다.

### 다중 함수 호출

`examples/multi.swm`은 `김민수` 함수가 `이원준` 함수를 여러 번 호출하고, top-level에서 마지막 호출을 이어 붙여 `Hello ASW!!`를 출력합니다.

```swm
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
```

### Exit Code

`examples/exit.swm`은 top-level에서 값을 반환해 exit code를 설정합니다. 정원 3에서 신청을 두 번 받아 최종 exit code는 `5`입니다.

```swm
이번에 [도메인 모델링] (정원 3) 을 개설했습니다
[도메인 모델링] 신청 바랍니다
[도메인 모델링] 신청 바랍니다
[도메인 모델링] 마감되었습니다. 감사합니다!
```

## 이디엄

- 파일 확장자는 `.swm`을 권장합니다.
- 변수명은 실제 SW마에스트로 세션명처럼 작성할 수 있습니다. 예: `[함수형 특강]`, `[도메인 모델링]`
- 문자 출력은 ASCII 값을 변수에 넣은 뒤 `많은 관심 부탁드립니다`를 사용합니다.
- 함수명과 URL 인자 이름은 UTF-8 문자열을 base64url로 인코딩합니다. padding은 생략해도 됩니다.

## 프로젝트 구조

```text
SwmLang.sln
src/SwmLang.Client/Interpreter/
  AST.fs
  Parser.fs
  ParserManual.fs
  ParserCombinator.fs
  Interpreter.fs
SwmLang.Test/
examples/
docs/
```
