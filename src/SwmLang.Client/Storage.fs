module Storage

let hi = """안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/7ZWo7IiY7ZiVIO2KueqwlQ==72

[함수형 특강] 많은 관심 부탁드립니다

[함수형 특강] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=72
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=73
https://swmaestro.ai/7J207JuQ7KSA?7ZWo7IiY7ZiVIO2KueqwlQ=33
"""

let star = """안녕하세요 이원준 멘토입니다
멘토 소개: https://notion.so/SzhTIOyVjOyVhOuztOq4sA==5&64-E66mU7J24IOuqqOuNuOungQ==42

[K8S 알아보기] 아직 마감되지 않아 한번 더 공지드립니다
    [도메인 모델링] 많은 관심 부탁드립니다
    [K8S 알아보기] 한자리 남았습니다
참고 부탁드립니다

[K8S 알아보기] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J207JuQ7KSA?SzhTIOyVjOyVhOuztOq4sA=5
"""

let xo = """안녕하세요 이원준 멘토입니다
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

let multiply = """안녕하세요 이원준 멘토입니다
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

let counter = """안녕하세요 이원준 멘토입니다
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

let examples = [|
    "HI! 출력", hi
    "별 찍기", star
    "XO 분기", xo
    "곱셈 (3×4)", multiply
    "2-Counter 덧셈", counter
|]
