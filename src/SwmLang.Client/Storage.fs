module Storage

let hi = """안녕하세요 인사출력 멘토입니다
멘토 소개: https://notion.so/6riA7J6Q==72

[글자] 많은 관심 부탁드립니다

[글자] 마감되었습니다. 감사합니다!


https://swmaestro.ai/7J247IKs7Lac66Cl?6riA7J6Q=72
https://swmaestro.ai/7J247IKs7Lac66Cl?6riA7J6Q=73
https://swmaestro.ai/7J247IKs7Lac66Cl?6riA7J6Q=33
"""

let star = """안녕하세요 별찍기 멘토입니다
멘토 소개: https://notion.so/7Zqf7IiY==5&6riA7Kec==42

[횟수] 아직 마감되지 않아 한번 더 공지드립니다
    [글짜] 많은 관심 부탁드립니다
    [횟수] 한자리 남았습니다
참고 부탁드립니다

[횟수] 마감되었습니다. 감사합니다!


https://swmaestro.ai/67OE7LCN6riw?7Zqf7IiY=5
"""

let xo = """안녕하세요 판정 멘토입니다
멘토 소개: https://notion.so/6rCS==0

이번에 [반환] (정원 0) 을 개설했습니다

[값] 인원 미달이라
    [반환] 잔여 88명입니다
인원이 미달이더라도
    [반환] 잔여 79명입니다
참고 부탁드립니다

[반환] 마감되었습니다. 감사합니다!


이번에 [결과1] (정원 0) 을 개설했습니다
이번에 [결과2] (정원 0) 을 개설했습니다

[결과1] 신청 링크: https://swmaestro.ai/7YyQ7KCV?6rCS=0
[결과2] 신청 링크: https://swmaestro.ai/7YyQ7KCV?6rCS=5

[결과1] 많은 관심 부탁드립니다
[결과2] 많은 관심 부탁드립니다
"""

let multiply = """안녕하세요 곱셈 멘토입니다
멘토 소개: https://notion.so/YQ====3&Yg====4

이번에 [결과] (정원 0) 을 개설했습니다

[a] 아직 마감되지 않아 한번 더 공지드립니다
    이번에 [임시] (정원 0) 을 개설했습니다
    [임시] 잔여 4명입니다
    
    [임시] 아직 마감되지 않아 한번 더 공지드립니다
        [결과] 신청 바랍니다
        [임시] 한자리 남았습니다
    참고 부탁드립니다
    
    [a] 한자리 남았습니다
참고 부탁드립니다

[결과] 마감되었습니다. 감사합니다!


이번에 [답] (정원 0) 을 개설했습니다
[답] 신청 링크: https://swmaestro.ai/6rOx7IWI?YQ===3&Yg===4
[답] 현재 인원 공유드립니다
"""

let examples = [|
    "HI!", hi
    "Stars", star
    "XO", xo
    "Multiply", multiply
|]
