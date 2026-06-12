# Project Brief

## Goal

`DimensionBrawl`은 새 기준점에서 다시 시작하는 Unity 액션 게임 프로토타입이다. 지금 목표는 거대한 시스템이 아니라, 안정적으로 확장 가능한 작은 전투 루프를 만드는 것이다.

## Current Direction

- 플레이어: 여검사 1종
- 기본 적: sci-fi 병사 1종
- 보스/대형 적 후보: 드래곤 1종
- 플레이어 애니메이션: 기본 이동 계열과 한손 무기 공격 계열을 후보로 사용
- 참고 자료: `Assets/_Game/DesignDocs/`, `Assets/_Game/DesignData/`

## First Playable Target

첫 플레이어블은 아래만 포함한다.

1. 플레이어 이동
2. 회피
3. 기본 공격
4. 적 1종 스폰
5. 피격/체력/사망
6. 간단한 전투 종료 조건

## Explicit Non-Goals

- 카드 UI
- 소환 시스템 전체
- 복잡한 보스 페이즈
- 자동으로 씬/프리팹을 대량 생성하는 런타임 시스템
- 이전 프로젝트의 legacy/fallback 복구
- 모든 에셋을 한 번에 게임용 구조로 전환

## Asset Policy

- 원본 에셋 팩은 `Assets/_Imported/` 아래에 둔다.
- `Assets/_Imported/`는 Git에서 무시한다.
- 게임에서 실제 사용할 프리팹, 머티리얼, 애니메이션 선택본만 `Assets/_Game/` 아래로 승격한다.

