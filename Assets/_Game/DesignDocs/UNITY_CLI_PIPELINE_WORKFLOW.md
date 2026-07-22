# Unity CLI + Pipeline 작업 환경

기준일: 2026-07-22

이 프로젝트의 자동화 기준은 Unity Editor `6000.3.5f2`, Unity CLI
`1.0.0-beta.2`, Unity Pipeline package `0.3.1-exp.1`이다. CLI와 Pipeline은
현재 beta/experimental 기능이므로, 버전을 무심코 올리지 말고 아래의 고정값을
유지한다.

## 설치된 구성

- CLI 실행 파일: `%LOCALAPPDATA%\Unity\bin\unity.exe`
- 프로젝트: `C:\Git\DimensionBrawl`
- Editor: `6000.3.5f2`
- UPM package: `com.unity.pipeline: 0.3.1-exp.1`
- Pipeline server: Editor가 열려 있을 때 `127.0.0.1`의 빈 포트
  (`7800`~`7849`, 이번 검증에서는 `7800`)
- Codex MCP: 사용자 `config.toml`의 `unity` 서버가 이 프로젝트 경로에 고정됨

새로 연 터미널에서는 `unity`를 바로 사용할 수 있다. 이미 열려 있던 터미널이
PATH 갱신을 받지 못했다면 다음처럼 전체 경로를 사용한다.

```powershell
& "$env:LOCALAPPDATA\Unity\bin\unity.exe" --version
```

## 일상 작업

Pipeline이 필요한 세션은 Editor를 자동화 모드로 연다.

```powershell
unity open "C:\Git\DimensionBrawl" `
  --editor-version 6000.3.5f2 `
  --args "-automated"
```

Editor와 Pipeline 상태를 확인하고, 실제 연결을 가볍게 검증한다.

```powershell
unity pipeline list --format json
unity command eval 'return UnityEngine.Application.unityVersion;' `
  --project-path "C:\Git\DimensionBrawl" `
  --format json
```

Codex MCP 구성은 다음 공식 명령으로 다시 만들 수 있다. Codex가 이미 실행 중이면
새 MCP 항목을 읽기 위해 새 작업 또는 앱 재시작이 필요할 수 있다.

```powershell
unity mcp configure codex `
  --project-path "C:\Git\DimensionBrawl" `
  --yes
```

## 테스트

Editor가 열려 있는 동안 Pipeline으로 PlayMode 테스트를 실행할 때는 반드시
비동기 실행을 사용한다. PlayMode 진입 시 도메인 reload가 발생하므로 동기 HTTP
호출은 연결이 끊길 수 있다.

```powershell
unity command run_tests `
  --project-path "C:\Git\DimensionBrawl" `
  --mode playmode `
  --filter OlympusCourtyardDrillStagePlayModeTests `
  --async_tests true `
  --format json

unity command test_status `
  --project-path "C:\Git\DimensionBrawl" `
  --format json
```

전체 PlayMode 회귀는 Editor를 닫은 뒤 독립 CLI 테스트로 실행한다.

```powershell
unity test "C:\Git\DimensionBrawl" `
  --mode PlayMode `
  --output "C:\tmp\DimensionBrawl-UnityCLI-PlayMode.xml" `
  --editor-version 6000.3.5f2 `
  --timeout 1200 `
  --format json `
  --non-interactive
```

Courtyard 제품 경로 격리 후 위 명령으로 전체 PlayMode 테스트 `479/479` 통과를 확인했다.

구조화된 출력에서는 최상위 `success`뿐 아니라
`data.result.success`와 `errors`도 함께 확인한다. 명령 전달 자체는 성공했지만
Editor 내부 컴파일 또는 평가가 실패할 수 있기 때문이다.

## `runInBackground` 설정

Pipeline package는 Editor가 최소화되거나 포커스를 잃어도 로컬 서버와 dispatcher가
계속 동작하도록 `Application.runInBackground = true`를 설정한다. 그래서
`ProjectSettings/ProjectSettings.asset`의 `runInBackground` 값도 `1`로 추적한다.
자동화 중 이 값을 되돌리면 Editor를 다시 열 때 같은 변경이 생길 수 있다.

## 업데이트와 롤백

CLI나 Pipeline 업데이트는 별도 변경으로 수행하고 전체 PlayMode 회귀를 다시 돌린다.

```powershell
unity pipeline list-versions
unity pipeline upgrade --project-path "C:\Git\DimensionBrawl"
```

Pipeline을 제거할 때는 Unity Package Manager에서 `com.unity.pipeline`을 제거하거나,
이 환경을 추가한 Git 커밋의 `Packages/manifest.json`,
`Packages/packages-lock.json`, `ProjectSettings/ProjectSettings.asset` 변경을 되돌린다.
Codex 연결만 제거하려면 사용자 `config.toml`의 `[[mcp.servers]]` 중
`name = "unity"`인 블록을 제거한다. 구성 전 설정은 같은 폴더의
`config.toml.before-unity-mcp-20260722.bak`에 보존되어 있다.
CLI 자체는 다음 명령으로 제거한다.

```powershell
unity self-uninstall
```
