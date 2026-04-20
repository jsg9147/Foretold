# Foretold - Project Context

## 프로젝트 개요
- 장르: 덱빌딩 + 그리드 전략 (턴제)
- 엔진: Unity
- 경로: `C:\Unity Project\Foretold`
- GitHub: `jsg9147/Foretold` (push 권한 이슈로 보류 중)

## 폴더 구조 (Assets 안에 생성 예정)
```
Assets/
├── _Game/
│   ├── 01_Scripts/
│   │   ├── 01_Core/         ← GameManager, 공통 유틸
│   │   ├── 02_Battle/       ← 그리드, 턴 매니저
│   │   ├── 03_Cards/        ← 카드 데이터 & 로직
│   │   ├── 04_Enemies/      ← 적 AI & 데이터
│   │   └── 05_UI/           ← UI 컨트롤러
│   ├── 02_ScriptableObjects/
│   │   ├── 01_Cards/
│   │   └── 02_Enemies/
│   ├── 03_Prefabs/
│   ├── 04_Scenes/
│   └── 05_Sprites/
└── ThirdParty/              ← 외부 구매 에셋
```

## 현재 진행 상태
- [x] .gitignore 생성
- [x] 폴더 구조 확정
- [x] Unity에서 폴더 구조 직접 생성
- [ ] Git 초기화 (`git init` in project root)
- [x] GridManager.cs 작성 (Unit.cs, TeamType.cs 스텁 포함)
- [x] TurnManager.cs 작성
- [x] CardData ScriptableObject 작성 (CardEffect.cs, CardType.cs, EffectType.cs 포함)
- [x] HandManager.cs 작성
- [x] CardExecutor.cs 작성
- [x] EnemyData ScriptableObject + EnemyAI.cs 작성
- [x] BattleManager.cs 작성

## 다음 작업 시작 포인트
Unity 상단 메뉴 Foretold > Generate Starter Cards 실행 → 카드 에셋 5종 생성 확인
→ 다음: 씬 세팅 (BattleScene 생성, 매니저 오브젝트 배치, 프리팹 연결)

## 기술 스택 / 설계 방향
- 그리드 기반 전투 시스템
- ScriptableObject로 카드/적 데이터 관리
- 턴제 전투

## 설치된 서드파티 패키지
- **DOTween Pro** — 카드/유닛 이동 애니메이션, 트윈 전반
- **UniTask** — 코루틴 대신 async/await, CancellationToken으로 취소 처리
- **Odin Inspector** — 인스펙터 커스터마이징 (`[ListDrawerSettings]`, `[PreviewField]` 등)

## 코딩 규칙
- **주석**: 모든 public 메서드·프로퍼티에 `<summary>` XML 주석 필수. 복잡한 로직엔 인라인 주석 추가.
- **클린 아키텍처 원칙**:
  - 단일 책임 원칙 (SRP) — 클래스 하나는 하나의 역할만
  - 의존성 역전 (DIP) — 구체 클래스 직접 참조 대신 이벤트/인터페이스 활용
  - 매니저 간 직접 호출 최소화 — 이벤트(`Action`, `UnityEvent`)로 통신
  - 데이터(ScriptableObject)와 로직(MonoBehaviour) 분리 유지

## 참고
- Notion 태스크 보드: https://www.notion.so/494db652e09245dd94d8ad65a16b173b
- Git 계정 이슈: `jsg9147` 계정으로 Fork 앱 재로그인 필요 (또는 Personal Access Token 사용)

## CLAUDE.md 관리 규칙
- 200줄 이하로 유지 (불필요한 내용은 정리)
- 작업 완료 후 진행 상태 및 다음 작업 포인트 갱신