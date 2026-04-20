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
- [ ] GridManager.cs 작성
- [ ] TurnManager.cs 작성
- [ ] CardData ScriptableObject 작성

## 다음 작업 시작 포인트
Unity에서 위 폴더 구조 생성 후 → GridManager.cs 작성 시작

## 기술 스택 / 설계 방향
- 그리드 기반 전투 시스템
- ScriptableObject로 카드/적 데이터 관리
- 턴제 전투

## 참고
- Notion 태스크 보드: https://www.notion.so/494db652e09245dd94d8ad65a16b173b
- Git 계정 이슈: `jsg9147` 계정으로 Fork 앱 재로그인 필요 (또는 Personal Access Token 사용)

## CLAUDE.md 관리 규칙
- 200줄 이하로 유지 (불필요한 내용은 정리)
- 작업 완료 후 진행 상태 및 다음 작업 포인트 갱신
