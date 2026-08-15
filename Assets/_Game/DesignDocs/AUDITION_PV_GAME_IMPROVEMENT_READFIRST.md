---
title: 경기게임오디션 PV · 게임 비주얼 개선 READ FIRST
aliases:
  - Audition PV Master Plan
  - 경기게임오디션 영상 계획
tags:
  - dimensionbrawl
  - pv
  - visual-direction
  - read-first
status: active
updated: 2026-08-15
---

# 경기게임오디션 PV · 게임 비주얼 개선 READ FIRST

> [!important] 현재 활성 계획
> 이 문서는 경기게임오디션 제출 영상과 그 기반이 되는 게임 개선의 현재 기준 문서다.
> **주차별 계획은 사용하지 않는다.** 모든 작업은 `영상 편집 시작 전`과 `영상 편집 시작 후`의 두 단계로만 나눈다.
> 이후 장기 작업은 먼저 이 문서를 읽고, 명시적인 방향 변경이 있을 때만 본문과 Decision Log를 함께 갱신한다.

## 1. 최상위 결정

영상만 그럴듯하게 포장하지 않는다. 실제 제품 씬과 실제 게임 상태를 먼저 `Camera-ready`로 만들고, After Effects는 편집·타이포그래피·장면 전환·최종 샷 매칭에 사용한다.

현재 최우선 결과물은 **경기게임오디션용 약 60초 PV**다. 그러나 영상에 쓰는 카메라, 셰이더, Look State, 애니메이션, VFX, 오디오 이벤트와 캡처 도구는 실제 게임에 남겨 재사용한다.

전체 실행 순서:

`게임 룩 정상화 → 도시·올림푸스 실제 플레이 완성 → 촬영 원본 확보 → 편집 시작 → 러프컷 기반 선택적 게임 수정 → 최종 편집·제출`

첫 품질 기준은 60초 전체가 아니라 다음 **12초 골드 원본**이다.

`도시 와이드 → 도시 실제 전투 → 차원 전환 → C33 날개 전개 → C34 개안 → 회피·카운터`

이 12초를 무보정 원본만으로 통과시킨 후 같은 문법을 나머지 영상으로 확장한다.

## 2. 현재 제품·시각 진단

현재 병목은 모델링 수가 아니라 화면의 역할 분리와 액션 표현 밀도다.

1. **Volume 소유권 충돌**
   - Corridor와 Station이 같은 `DB_OlympusCorridor_InoriPresentationPostProcess`를 Global priority 95로 사용한다.
   - DOF 활성, Saturation `+18`, Bloom `0.04`가 환경 프로필과 충돌한다.
   - 일반 전투가 쌩화장처럼 보이고 배경이 애매하게 눌리는 구조적 원인이다.

2. **재질 역할 부족**
   - 대리석·금색 금속·타일 비중은 높지만 normal, roughness 변화, grime, 접촉 decal이 약하다.
   - 피부·머리·천·금속의 반사 언어도 충분히 분리되지 않는다.

3. **게임플레이 구도 부족**
   - 플레이어와 빈 바닥이 크고 보스가 작다.
   - 중앙 소환물과 장벽이 보스·목표를 가릴 수 있다.
   - 카메라 cue는 존재하지만 화면 점유율과 액션별 렌즈 문법이 부족하다.

4. **애니메이션 어휘 부족**
   - C33 날개 전개와 C34 개안은 강하다.
   - 실전 Hover, 충전, 발사 반동, 경직, 사망은 아직 동일 홀드 포즈처럼 읽힐 위험이 있다.

5. **VFX 단계 부족**
   - 핵심 공격은 `예고 → 충전 → 발사 → 근접 통과 → 충돌 → 잔류`가 모두 보여야 한다.
   - 탄과 trail만으로는 상용 액션 PV의 밀도가 나오지 않는다.

6. **환경 운동·접지 부족**
   - 안개 흐름, 먼지, 잔해, 조명 변화, 원경 운동, scorch/wet decal이 부족하다.
   - 중앙 No-Cross 벽은 가시성은 복구됐지만 큐브 덩어리 문법은 더 얇고 연속적인 에너지 경계로 정리할 필요가 있다.

7. **오디오 무게 부족**
   - 날개 전개, 개안, 보스 충전, 탄 통과, 충돌, 공간 공명, 사망에 전용 계층형 사운드가 부족하다.

연관 문서:

- [[CURRENT_GAME_CONTENT_GAP_DIAGNOSIS]]
- [[ACTION_FEEL_TARGETS]]
- [[CINEMACHINE_INGAME_CUTSCENE_REFERENCE_RESEARCH]]
- [[SCENE_GRAPHICS_STABILITY_AUDIT]]
- [[ART_ASSET_STORAGE_WORKFLOW]]
- [[INTRO_GATEPOD_INVASION_ARKDATA_GUARDRAIL]]

## 3. 단계 A — 영상 편집 시작 전

목표는 **AE 없이 캡처 원본만 봐도 상용 게임 화면으로 읽히는 촬영 소스**를 확보하는 것이다.

### 3.1 작업 기준과 소유권

- 현재 관련 전투·Phase 2·룩·투사체·벽 변경을 UI 작업과 분리해 체크포인트한다.
- 검증 가능한 작업 단위마다 작은 로컬 커밋을 남긴다. 원격 push는 개별 커밋마다 하지 않고, 여러 검증 완료 커밋을 의미 있는 체크포인트에서 묶어 수행한다.
- 다른 세션이 소유한 `_Game/UI/**`, Combat HUD prefab/presenter를 이 작업에서 수정하지 않는다.
- `.unity` 파일은 동시에 한 작업자만 편집한다.
- 가능한 변경은 독립 prefab/profile/asset으로 만들고 씬에는 참조만 연결한다.
- 외부 패키지는 본 프로젝트에 통째로 import하지 않는다. staging 프로젝트에서 검증하고 필요한 dependency만 승격한다.

### 3.2 Look State와 후처리

하나의 Global Volume으로 모든 화면을 해결하지 않는다.

| Look State | 역할 | 핵심 계약 |
|---|---|---|
| `GameplayBase` | 일반 전투 | DOF 끔, 중립 노출, 환경과 캐릭터 동시 가독성 |
| `CharacterFocus` | 얼굴·대화 | 명시적 요청형, 종료·Skip·Retry에서 복구 |
| `Phase2Cinematic` | 날개 전개·개안 | 얼굴과 날개 실루엣 우선 |
| `CombatImpact` | 회피·피격·궁극기 | 짧은 transient, 중첩·인터럽트 안전 |
| `Finisher` | 사망·차원 붕괴 | dissolve, 백색화, aftermath |

C34에서 A/B 검증된 `Phase2Cinematic` 후보 기준:

- 키라이트: 중립 백색 `RGB(1,1,1)`, intensity `1.42`
- 그림자: Soft, strength `0.5`
- Exposure: `+0.20`
- Bloom: threshold `1.0`, intensity `0.70`, scatter `0.85`
- Bloom 품질: Half, High Quality Filtering, max iterations `8`
- Chromatic Aberration: 약 `0.15`
- DOF: 개안 클로즈업에만 사용

이 값은 일반 gameplay에 그대로 복사하지 않고 Phase 2 기준점으로만 사용한다.

### 3.3 셰이더·재질

캐릭터:

- 얼굴: 자체 Face SDF와 부드러운 shadow ramp
- 피부: 얼굴·몸의 과한 반사 제거와 피부 전용 ramp
- 머리카락: 방향성 highlight와 역광 rim
- 눈: catchlight와 제한적인 HDR emission
- 천: 피부·금속과 구분되는 roughness
- 금속·무기·날개: mask 기반 specular 또는 MatCap
- 공통: 거리 기반 outline, 상태 기반 rim/emission/dissolve

환경:

- Marble: detail normal, roughness variation, 얼룩·균열
- Gold/Painted Metal: reflection probe, edge wear, 금속 mask
- Tile/Concrete: grout, macro variation, dirt/wet/scorch decal
- VFX shader: depth fade, soft particle, distortion vector, emissive pulse

기존 Unity Toon Shader를 기반으로 확장하고, ArkData나 타 게임의 HLSL·ramp·SDF·텍스처를 복사하지 않는다.

### 3.4 카메라·애니메이션

화면 점유율 목표:

- 플레이어 화면 높이 `25~32%`
- 보스 본체 `25~40%`
- 전개 날개 화면 너비 `60~80%`
- 소환물은 목표와 보스를 가리지 않도록 측면 또는 상단에 배치

필수 카메라 문법:

- 기본 어깨너머 gameplay
- 퍼펙트 회피 측면 강조
- Skill/보스 대공격 wide
- Phase 2와 Finisher용 Timeline camera
- 종료·Skip·사망·Disable 후 FOV/target/position/AudioListener 완전 복구

필수 동작:

- 아카자: Hover/호흡, 충전, 발사 반동, 중경직, 파괴·사망
- 이노리: 조준, 발사 반동, 회피, Skill, Finisher pose
- 날개 구조: 공격 원점과 인과가 읽히는 관절 운동

### 3.5 VFX·액션 표현

각 대표 공격은 아래 모든 단계가 있어야 한다.

`Telegraph → Charge → Release → Travel/Near Miss → Impact → Aftermath`

필수 대상:

- 아카자 대표 패턴 3종
- 이노리 기본 사격
- 퍼펙트 회피
- 소환 방어·카운터
- 보스 사망·공간 붕괴

VFX가 판정과 피해를 소유하지 않고 실제 combat event를 구독한다. 반복 재생 후 pool·material instance·Volume state가 남지 않아야 한다.

### 3.6 도시 Hero Pocket

도시는 소극적인 teaser가 아니라 최종 영상의 약 `15~18초`를 담당한다.

- `City Builder Urban`에서 도로 한 블록 또는 옥상 한 곳을 staging한다.
- 전경 잔해, 중경 전투 공간, 원경 skyline의 세 층을 만든다.
- 실제 이동·사격·회피가 가능한 연속 gameplay를 확보한다.
- 연기, 비상등, 바람, 간판, 먼지와 ambience를 추가한다.
- 도시→올림푸스는 차원 균열 또는 공격 충돌 white-out으로 연결한다.
- `SciFi Neon City`는 단기 범위가 아니라 이후 정식 도시 스테이지 후보로 둔다.

### 3.7 오디오 원재료

편집을 시작하기 전 임시라도 아래 stem을 준비한다.

- Music
- VO
- SFX
- Ambience

필수 cue:

- 도시·올림푸스 ambience
- 총기 기계음/발사음/원거리 tail
- 회피·소환·피격
- 보스 충전·발사·사망
- 날개 전개·개안 sting
- 교신 1~2문장, 이노리 1문장, 보스 1문장 수준의 임시 VO

ElevenLabs 생성물은 prompt, model, 생성일, 계정 plan, 사용권, 원본 WAV와 편집본을 기록한다. 실존 성우를 동의 없이 모사하지 않는다.

### 3.8 촬영 시스템

- 결정론적 `PV Capture Director`
- 씬·상태·seed·카메라·HUD 모드를 기록하는 Shot Manifest
- HUD-on / HUD-off / clean plate
- 각 핵심 숏 최소 3 take
- 숏 전후 3~5초 handle
- 편집 원본 `2560×1440 / 60fps / Rec.709` 이상
- 동일 샷의 Git SHA, Timeline, 사용 asset, 승인 take 기록
- 자동 contact sheet, 누락 frame, error-magenta, 해상도 검사

## 4. 편집 시작 Gate

다음 조건이 모두 충족되기 전에는 본격적인 AE 편집을 시작하지 않는다.

- [ ] 도시→전환→날개 전개→개안→회피·카운터의 12초 무보정 골드 원본 확보
- [ ] 도시와 올림푸스 각각 5~7초 이상의 HUD-on 연속 gameplay 확보
- [ ] 보스 대표 패턴 3종과 Finisher 촬영 가능
- [ ] 얼굴·보스·공격 방향·충돌점이 25% 축소 화면에서 읽힘
- [ ] pink shader, null material, 검은 mesh, 깨진 trail이 없음
- [ ] 임시 음악·핵심 SFX·최소 VO가 존재
- [ ] 60초 Shot Manifest의 필수 숏이 최소 한 take씩 존재
- [ ] 사용 asset·font·audio·AI 생성물의 권리/provenance가 기록됨

Gate 판단은 날짜가 아니라 위 증거로 한다.

## 5. 단계 B — 영상 편집 시작 후

편집이 시작되면 막연한 게임 개선을 중단한다. 새 변경은 반드시 `PV_S030` 같은 구체적인 Shot ID 또는 제품 P0/P1에 연결한다.

### 5.1 러프컷

- 임시 음악과 SFX를 먼저 배치한다.
- 도시 약 `25%`, 올림푸스·보스 약 `65%`, 카드 약 `10%`를 기준으로 시작한다.
- HUD-on 연속 gameplay 숏 두 개를 유지한다.
- 실제 gameplay와 cinematic을 교차하고 전체 길이와 음악 박자를 먼저 잠근다.

### 5.2 수정 반환 규칙

| 러프컷 문제 | 반환 위치 |
|---|---|
| 얼굴·보스가 안 읽힘 | Unity lighting/shader/camera |
| 공격 원인·방향이 불명확 | Animation/VFX |
| 공간이 비어 보임 | Environment motion/foreground/decal |
| 타격이 가벼움 | Hit-stop/shake/VFX/SFX |
| 동일 문제가 여러 숏에 반복 | 게임 시스템 |
| 컷 연결만 어색함 | After Effects transition |

다음 중 하나라도 해당하면 AE로 감추지 않고 Unity로 돌려보낸다.

- 주체를 읽기 위해 tracking mask가 필요함
- 구조적으로 약 `±0.5EV` 이상의 보정이 필요함
- AE Glow를 끄면 스킬이 읽히지 않음
- 같은 문제가 여러 shot에서 반복됨
- 편집 전후가 전혀 다른 제품처럼 보임

### 5.3 After Effects 역할

- 도시→올림푸스 차원 균열 transition
- 공격 충돌점 hard cut
- 1~2 frame white/black impact
- speed ramp
- title, feature super, logo, end card
- shot 간 미세한 노출·색 일치
- 제한적인 glow/halation
- 최종 Rec.709, grain, delivery graphic

AI/Higgsfield shot은 실제 gameplay를 대신하지 않는다. 도시 원경, 차원 내부 추상 연결, end-card 직전 분위기 숏 등 전체의 `0~10%` 안에서만 사용한다.

### 5.4 Picture Lock 이후

- 명백한 오류 외에는 Unity 재촬영을 금지한다.
- 최종 VO·음악·SFX를 교체하고 ducking을 적용한다.
- Mix target: 약 `-14 LUFS Integrated`, `-1 dBTP`.
- 헤드폰·스피커·휴대폰 화면에서 확인한다.
- H.264 제출본과 보존용 master, 무자막/무음/clean 버전을 보관한다.
- 모든 pixel·audio·font·asset의 provenance를 최종 확인한다.

## 6. 현재 60초 구성 기준

| 구간 | 내용 | 목적 |
|---|---|---|
| 0~4초 | 도시 경보·skyline·차원 이상 | 세계와 위협 훅 |
| 4~10초 | HUD-on 도시 이동·사격 | 실제 gameplay 증명 |
| 10~16초 | 피격·회피·소환 연계 | 핵심 시스템 |
| 16~20초 | 차원 균열로 올림푸스 전환 | 공간 전환 |
| 20~24초 | 보스 low angle·실루엣 | 위압감 |
| 24~28초 | C33 날개 전개→C34 개안 | 기억점 |
| 28~40초 | Phase 2 대표 패턴 3종 | 보스전 밀도 |
| 40~50초 | 퍼펙트 회피→소환 방어→궁극기 | 절정 |
| 50~55초 | 보스 붕괴·aftermath | 여운 |
| 55~60초 | 로고·슬로건·오디션 end card | 종료 |

이 구성은 편집 러프컷 결과에 따라 초 단위로 조정할 수 있지만, `도시 실제 gameplay`, `Olympus 실제 gameplay`, `C33/C34 기억점`, `피니시` 네 증거는 제거하지 않는다.

## 7. ArkData·외부 자료 사용 원칙

ArkData에서 가져올 것은 결과물이 아니라 **여러 표현 채널을 한 상태와 한 프레임에 묶는 방식**이다.

우선 연구 대상:

1. `Skill → Frame → Event → Parameter` 표현 이벤트
2. cut/short/long Camera Shot Preset
3. Camera priority·중첩·복구
4. Animation/Camera/VFX/Audio/Material/ScreenFX 동기화
5. 5단계 공격 VFX 문법
6. Hit-stop/Shake/Flash/Sound impact stack
7. 상태 기반 Rim/Emission/Dissolve
8. Face SDF/Ramp/Rim/MatCap 문법
9. Post-process capture→ramp→restore lifecycle
10. 결정론적 PV Runner

금지선:

- 상용 게임의 코드, HLSL, JSON 구조, 정확한 카메라 transform·곡선·수치를 복사하지 않는다.
- 모델, 텍스처, ramp, SDF, LUT, animation, audio를 직접 가져오지 않는다.
- 기능·입력·출력·lifecycle·검증 기준만 clean-room 명세로 작성해 DimensionBrawl 코드로 재구현한다.

Drive·Asset Store 후보:

| 자료 | 단기 사용 |
|---|---|
| City Builder Urban | 도시 Hero Pocket |
| Stylized Shoot Hit | 피격·충돌 후보 |
| UNI VFX Fire/Smoke | 도시 ambience·원경 사건 |
| Action RPG SFX | 총기·충돌·기계음 후보 |
| RPG Magic SFX | 차원·보스 충전 후보 |
| Magic Missiles | 현재 Phase 2 투사체 계열 |
| Magica Cloth 2 | 캐릭터 secondary motion |
| SciFi Neon City | 단기 제외, 이후 정식 도시 후보 |

Asset Store 구매 계정·영수증·버전·사용 위치를 확인한 것만 외부 제출물에 사용한다.

## 8. 공통 완료 등급

모든 기능과 자산은 세 등급으로 판정한다.

| 등급 | 의미 |
|---|---|
| `Playable` | 실제 게임 규칙과 lifecycle로 작동 |
| `Camera-ready` | 무보정 capture가 영상에 사용 가능 |
| `Mobile-ready` | 목표 기기 성능과 대체 품질 통과 |

현재 제출 전 우선순위는 `Playable + Camera-ready`다. `Mobile-ready`를 의도적으로 파괴하지 않되, 최종 실기기 최적화와 정식 스테이지 제품화는 영상 제출 후에도 같은 자산과 시스템을 이어서 수행한다.

## 9. 즉시 실행 순서

- [ ] 관련 변경만 UI 작업과 분리해 체크포인트
- [ ] 일반 gameplay의 priority 95 Global Volume 문제 정상화
- [ ] 도시·올림푸스·보스·충돌 기준 frame 6장 고정
- [ ] 카메라 화면 점유율 수정
- [ ] 12초 골드 원본 Shot Manifest 작성
- [ ] City Builder Urban staging 및 dependency 선별
- [ ] 보스 패턴 3종의 표현 채널 누락표 작성
- [ ] 임시 음악·SFX·VO bed 준비
- [ ] 편집 시작 Gate 통과 판정

## 10. 장기 작업 도구·접근 레지스트리

> [!warning] 재검증 규칙
> 아래는 2026-08-15에 확인되었거나 마스터가 직접 제공한 작업 환경이다. 장기 작업을 재개할 때 경로·로그인·버전·라이선스가 여전히 유효한지 먼저 재검증한다. 로그인 상태가 보인다는 사실만으로 구매·공개·업로드 권한을 확대 해석하지 않는다.

### 10.1 로컬 제작 앱

| 도구 | 현재 준비 상태 | 사용 시점·역할 |
|---|---|---|
| Obsidian 1.12.7 | `DesignDocs` Vault가 열려 있음 | 본 READ FIRST, 결정 기록, Shot Manifest, 권리 원장 |
| Unity 6000.3.5f2 | DimensionBrawl 기준 Editor | 게임 개선, Timeline, Recorder, 최종 source capture |
| Unity Recorder 5.1.6 | 프로젝트 package 설치 확인 | 결정론적 1440p60/4K60 capture preset 제작 |
| Cinemachine 3.1.7 | 프로젝트 package 설치 확인 | Shot preset, camera blend, target framing |
| Timeline 1.8.10 | 프로젝트 package 설치 확인 | Intro, Phase transition, finisher, PV source sequence |
| After Effects 2026 | 로컬 설치 확인 | 러프컷 이후 transition, typography, shot match, delivery graphics |
| Adobe Media Encoder 2026 | 로컬 설치 확인 | H.264 제출본과 보존용 encode |
| Photoshop 2026 | 로컬 설치 확인 | texture/ramp/mask/title graphic 보조 작업 |
| OBS 32.1.2 | 설치 확인, 기존 profile은 1440 canvas→1080 output·약 6Mbps라 master 부적합 | 실제 수동 gameplay capture용 별도 `DB_PV_CAPTURE` profile 필요 |

권장 capture storage는 프로젝트 밖 `D:\DimensionBrawl_PV\`다. `00_brief / 01_capture_video / 01_capture_audio / 02_selects / 03_AE / 04_graphics / 05_exports / 99_licenses` 구조를 사용한다. D:는 대용량 source/cache용, repo에는 capture 원본을 넣지 않는다.

Capture PC는 RTX 5060 Ti를 사용한다. Android SDK/NDK/JDK와 Unity 내장 adb는 준비돼 있지만 2026-08-14 감사 당시 연결 기기는 0대였다. `Mobile-ready` 판정 전 실제 기기 연결과 최신 Phase 2 포함 APK 재빌드가 필요하다.

### 10.2 열린 브라우저·서비스

| 창·서비스 | 마스터가 준비한 상태 | 사용 원칙 |
|---|---|---|
| 김강일 Chrome · ElevenLabs Music | `뮤직 | ElevenLabs` 창이 열려 있음을 확인 | 대본/beat lock 후 BGM, ambience, SFX, original/consented VO 생성 |
| 이데올로기 Chrome/Cloud · Unity Asset Store | Asset Store account/assets 창이 열려 있음을 확인 | 필요한 asset의 보유 여부·버전·URP/Unity6 호환성·license 확인 |
| Higgsfield | 마스터가 로그인 상태를 준비했다고 명시 | 실제 gameplay를 대체하지 않는 0~10% 연결/분위기 shot만 사용한다. 생성 전 현재 plan의 상업 이용·다운로드·워터마크 조건을 확인하고 prompt·model·생성일·원본/출력·사용 Shot ID를 `99_licenses`에 기록한다. |
| Google Drive | 아래 asset folder 접근 제공 | package inventory와 원본 보관; import 전 구매·사용권 재확인 |
| YouTube reference tabs | Snowbreak PV와 광고 제작 workflow reference | 결과 복제가 아니라 구조·작업 순서·밀도 분석 |

장기 작업에서 browser/Chrome/computer-use 도구를 사용할 수 있다. 단, 결제·구매 확정, 외부 공개, 메시지 발송, 권한 변경은 해당 행동 직전에 범위와 대상을 확인한다.

공개 웹 조사 결과는 URL·제목/제작자·접근일·적용 판단을 관련 연구 문서나 권리 원장에 남긴다. 명시적 라이선스가 없는 코드·이미지·영상·오디오·폰트·템플릿은 참고 분석만 하고 결과물에 포함하지 않는다.

Codex 작업 경로:

- Browser skill: 공개 reference, 문서, 영상 구조 조사
- Chrome skill: 마스터가 열어 둔 로그인 세션과 tab을 보존하며 사용
- Computer Use: Obsidian, After Effects, Media Encoder 등 Windows 앱 조작
- Google Drive connector: 제공 folder의 metadata/inventory와 허가된 파일 접근
- Local shell/Unity batch: repo, ArkData, automated capture/test
- Image generation/AI video: original graphic 또는 명시된 연결 shot에만 제한적으로 사용

### 10.3 외부 경로·링크

- ArkData 연구 저장소: `\\10.100.140.18\ArkData`
- 제공 Drive folder: <https://drive.google.com/drive/folders/1KWDbEqxRU_2hjB91j6idyFoDq4LqJexF?hl=ko>
- Snowbreak PV reference: <https://www.youtube.com/watch?v=ztlor9pF_Ys>
- 영상 제작 workflow reference: <https://www.youtube.com/watch?v=3ghETAbSv4Y>
- 현재 Obsidian Vault: `C:\Git\DimensionBrawl\Assets\_Game\DesignDocs`
- 본 문서: `AUDITION_PV_GAME_IMPROVEMENT_READFIRST.md`
- TPK Unity 6 변환 참고본: `C:\ThePhantomKnowledge-1.0.0f3\ThePhantomKnowledge-1.0.0f3`
- TPK Unity 2017 원본 참고본: `D:\ThePhantomKnowledge-1.0.0f3\ThePhantomKnowledge-1.0.0f3`

ArkData는 camera animation, VFX, cutscene, event/code architecture를 연구하는 저장소다. 결과물에는 원본 자산·코드·수치·곡선을 직접 포함하지 않고 clean-room 명세만 사용한다.

TPK 원본은 Built-in RP/Unity 2017 자료이고 현재 프로젝트는 Unity 6/URP17이다. legacy shader·PostProcessing v1·MovieProxy·native plugin·전체 Timeline을 다시 통째로 가져오지 않는다. 이미 이식·URP 정리된 Akaza/C33/C34 asset을 우선 사용하며, 추가 shot은 필요한 camera/actor animation/timing dependency만 격리한다. UCL/Unity Companion License 적용 범위와 표기를 `ThirdPartyNotices` 및 권리 원장에 남긴다.

### 10.4 Drive에 확인된 asset 후보

2026-08-15 read-only inventory 기준:

| 파일 | 대략 크기 | 계획 |
|---|---:|---|
| City Builder Urban | 3.93 GB | 단기 도시 Hero Pocket 1순위 |
| SciFi Neon City | 4.32 GB | 이후 정식 도시 후보; Unity6 URP 검증 선행 |
| Stylized Shoot Hit Vol1 | 4.3 MB | hit/impact 후보 |
| UNI VFX Realistic Explosions Fire Smoke | 177.5 MB | 도시 ambience·원경 사건 후보 |
| RPG Magic SFX Pack 3 Elemental AAA | 625 MB | 차원·보스·소환 SFX 후보 |
| Action RPG SFX Pack v2 | 178.3 MB | 총기·충돌·기계음 후보 |
| Magic Missiles | 32.5 MB | Phase 2 projectile visual source; 이미 일부 활용 |
| Magica Cloth 2 | 86.7 MB | 캐릭터 secondary motion; 현재 사용 중 |
| Realistic 6D Lighting Explosions Pack | 759.7 MB | 선택적 원경 폭발; art-style 검증 필요 |
| Hyper Casual FX Pack Vol2 | 9.7 MB | 현재 문법과 달라 우선 제외 |
| CombatGirlsKatanaCharacterPack | 96 MB | 현재 PV 핵심 범위에서 제외 |

Drive의 파일 존재만으로 제출 권리가 증명되지는 않는다. invoice/account ownership, Standard EULA/seat, 사용 시점의 약관을 `99_licenses`에 보관한다.

## 11. 이미 준비된 게임·촬영 자산

장기 작업을 다시 시작할 때 아래를 재제작하지 말고 먼저 현재 상태를 검증한다.

### 11.1 인엔진 콘텐츠

- Inori/GatePod Olympus intro 약 36.574초: 폭격, 각성, 얼굴/손/전신 reveal, 전투 handoff 기반이 존재한다.
- Olympus Station Akaza Phase 2 intro 약 3.9667초: C33 날개 전개 `1.6s` + C34 개안 `2.3667s`가 제품 Timeline으로 연결돼 있다.
- Akaza Phase 2 gameplay: HoverLance, SummonCurtain, SpiralVolley, CrushNet, basic fire, summon pressure, death/terminal cleanup 기반이 존재한다.
- Phase 2 projectile는 임시 opaque material에서 URP Particles/Unlit 계열로 교체됐고 profile별 cyan/violet/mint/red 구분 proof가 존재한다.
- Phase 2 gameplay boss는 combined mesh 경로로 renderer/submesh 비용을 줄인 구조가 존재한다. 최종 모바일 성능은 별도 실기 검증 대상이다.
- 중앙 No-Cross wall은 visible core가 복구됐고 약 31/32 particle budget과 loop continuity proof가 있다. 다음 작업은 가시성 복구가 아니라 blocky art polish다.
- C33/C34 중립 white key와 C34 soft shadow, source-soft Volume 후보가 존재한다.
- 현재 UI는 별도 세션에서 진행 중이다. 본 장기 계획은 UI 구현을 덮지 않고 read-model/표시 계약만 사용한다.

### 11.2 재사용 가능한 시스템

- `ActionCameraController`: additive cue, aim, micro shake 기반
- `ActionCinematicCueProfile/Director`: priority, interrupt, input/time lock, multi-shot signal 기반
- `CombatVfxCueProfile/Player`: VFX/audio/pool 기반
- `PerfectDodgeScreenDomainRendererFeature`: URP17 RenderGraph ScreenFX 선례
- Stage/Encounter/Result lifecycle와 Phase 2 flow
- 2560×1440 Game View preset
- 다수의 graphics batch capture와 PlayMode/EditMode regression test

이 기반을 교체하는 새 거대 framework보다 adapter, validator, manifest, lifecycle stack으로 확장한다.

### 11.3 기존 proof·임시 출력

아래 경로는 작업 근거로 유용하지만 `C:\tmp`이므로 영구 보관소가 아니다. 중요한 결과는 필요 시 재생성하고 최종 승인본은 `D:\DimensionBrawl_PV\02_selects` 또는 `99_licenses/evidence`로 승격한다.

- `C:\tmp\DimensionBrawl-StationAkazaPhase2Intro\`
- `C:\tmp\DimensionBrawl-AkazaC34LightingAB\`
- `C:\tmp\DimensionBrawl-AkazaC34BloomAB\`
- `C:\tmp\DimensionBrawl-AkazaC34NeutralKeyAB\`
- `C:\tmp\DimensionBrawl-AkazaProjectileMaterialProof-*\`
- `C:\tmp\DimensionBrawl-AkazaProjectileProfileProof-*\`

기존 Station transition capture는 회귀 검증용 `640×360 / 30fps / 무음` 기반이지 최종 PV Recorder preset이 아니다.

### 11.4 장기 작업 재개 체크리스트

- [ ] 이 READ FIRST와 최신 Decision Log 읽기
- [ ] `git status`와 다른 활성 세션의 파일 소유권 확인
- [ ] Unity/Obsidian/Chrome의 현재 창·version·login 상태 재확인
- [ ] 제품 scene과 최신 golden capture가 일치하는지 확인
- [ ] Drive/Asset Store asset의 license와 Unity6/URP17 호환성 재확인
- [ ] ArkData clean-room 경계 확인
- [ ] `C:\tmp` proof에 의존하지 않고 재생성 가능한지 확인
- [ ] 편집 시작 Gate의 현재 체크 상태 확인
- [ ] 새 작업이 `Playable / Camera-ready / Mobile-ready` 중 어느 등급을 목표로 하는지 명시

## 12. Decision Log

### 2026-08-15

- 주차별 계획을 폐기하고 `영상 편집 시작 전 / 영상 편집 시작 후` 두 단계로 변경했다.
- 도시를 단순 tease가 아니라 실제 gameplay를 포함한 약 15~18초 분량으로 적극 사용한다.
- 첫 품질 목표를 60초 전체가 아닌 12초 무보정 골드 원본으로 고정했다.
- AE는 구조적 게임 비주얼 결함을 숨기지 않는다.
- ArkData는 clean-room 동작 명세와 표현 구조 연구에만 사용한다.
- 현재 최고 우선순위는 `체크포인트 → Global Volume 정상화 → 기준 frame → 카메라 → 12초 골드 원본`이다.
- 열린 제작 도구, 로그인 서비스, 외부 경로, Drive 후보 asset, 기존 proof와 준비된 게임 자산을 장기 작업 레지스트리로 고정했다.
