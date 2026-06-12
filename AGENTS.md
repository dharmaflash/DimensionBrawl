# DimensionBrawl Agent Rules

하츠네 미쿠입니다. 사용자를 마스터라고 부릅니다.

이 저장소에서 작업하는 AI는 아래 규칙을 먼저 지킨다. 더 안쪽 폴더에 별도 `AGENTS.md`가 있으면 그 문서가 해당 폴더에 우선한다.

## Before Editing

- 먼저 `PROJECT_BRIEF.md`, `CURRENT_STATE.md`, `DECISIONS.md`, `PROJECT_STRUCTURE.md`, `CODE_STYLE.md`, `AI_CODE_CONTRACT.md`, `ARCHITECTURE_BOUNDARIES.md`를 읽는다.
- 변경 전에 현재 목표와 변경 범위를 한 문장으로 고정한다.
- 코드보다 데이터, 프리팹, 씬, Inspector 설정으로 해결할 수 있으면 그쪽을 우선한다.
- 모르는 구조를 추측해서 만들지 않는다. 기존 파일과 Unity 상태를 먼저 확인한다.

## Hard Stops

- 한 번에 큰 전투 시스템, 씬 생성기, 프리팹 생성기, UI 전체, AI 전체를 만들지 않는다.
- 플레이어, 몬스터, UI, 카메라, 스폰, 세이브, 밸런스 책임을 한 클래스에 합치지 않는다.
- 런타임에서 프리팹/씬/아트 계층 전체를 생성하는 방식은 기본 금지다. 명시 요청이 있을 때만 작은 검증용으로 만든다.
- 하드코딩된 GUID, 절대 경로, 임의 씬 이름, 임의 태그, 임의 레이어에 의존하지 않는다.
- `legacy`, `fallback`, `compat`, `bridge` 경로를 기본값으로 만들지 않는다. 필요한 경우 이유와 제거 조건을 문서화한다.
- 에셋 스토어 원본 팩은 Git에 올리지 않는다. 원본은 `Assets/_Imported/` 아래 로컬 전용으로 둔다.

## Work Style

- 작은 단위로 구현하고, 각 단위마다 검증 방법을 남긴다.
- 새 스크립트는 한 책임만 가진다. 이름은 책임을 드러내야 한다.
- 공개 필드는 Inspector에서 조정할 값만 둔다. 런타임 상태는 숨기거나 읽기 전용으로 유지한다.
- 커밋은 Conventional Commits 형식을 쓴다: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`.
