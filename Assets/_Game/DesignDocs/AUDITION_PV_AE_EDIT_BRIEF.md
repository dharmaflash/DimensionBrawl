# Dimension Brawl 경기게임오디션 PV — 촬영·AE 편집 브리프

updated: 2026-08-17 KST

## 1. 목적과 우선순위

이 문서는 `AUDITION_PV_GAME_IMPROVEMENT_READFIRST.md`를 실행 가능한 촬영·편집 브리프로 고정한다. 상충할 때는 READ FIRST가 우선한다.

- 결과물: 실제 제품 플레이를 중심으로 한 약 60초 경기게임오디션 제출 PV.
- 순서: 게임 룩 정상화 → City/Olympus 실제 플레이 촬영 → QHD60 촬영 원본과 증거 고정 → 러프컷 → 러프컷 기반 Unity 보정 → After Effects 최종 편집 → 제출 QA.
- 삭제할 수 없는 네 증거: City 실제 gameplay, Olympus 실제 gameplay, C33/C34 기억점, boss finisher.
- AI 생성 영상은 gameplay 대체에 쓰지 않는다. 필요하면 전체의 0–10% 범위에서 추상적인 연결부나 end-card 진입 분위기에만 사용한다.
- 참고 영상·강의의 공유 프로젝트, 이미지, 템플릿, 음원은 복사하지 않는다. 권리가 확인된 프로젝트 원본과 자체 제작 그래픽만 사용한다.

## 2. 60초 정본 구성

구간 경계는 러프컷에서 소폭 조정할 수 있지만, 역할 순서와 0–60초 총 길이는 유지한다.

| Shot | 기준 구간 | 화면 역할 | 편집 핵심 |
|---|---:|---|---|
| PV_S010 | 0–4초 | City 경보, skyline, 차원 이상 | 차가운 wide hook, 짧은 경보음, 이상징후 방향을 한눈에 읽힘 |
| PV_S020 | 4–10초 | HUD-on City 이동·사격 | 5초 이상 연속 실제 gameplay, 안정적인 시점과 조작 가독성 |
| PV_S030 | 10–16초 | 실제 피격 → perfect dodge → summon chain | 충돌 hard cut, 입력과 결과의 인과가 보이는 action bundle |
| PV_S040 | 16–20초 | City 균열 → Olympus | 균열을 transition source로 사용, 다음 공간을 먼저 암시 |
| PV_S050 | 20–24초 | boss low angle / silhouette | 낮은 시점, 크기와 위압감, HUD-off |
| PV_S060 | 24–28초 | C33 wing deploy → C34 eye open | 두 기억점을 명확한 reveal beat로 분리 |
| PV_S070 | 28–40초 | Phase 2 대표 패턴 3종 | gameplay와 cinematic close-up 교차, 공격 방향 유지 |
| PV_S080 | 40–50초 | perfect dodge → summon defense → tier-3 ultimate | 후반 가속, ultimate 전후 대비와 실제 action proof 유지 |
| PV_S090 | 50–55초 | boss finisher → collapse → aftermath | 한 프레임 hard cut, 자세 붕괴와 바닥 hold를 충분히 보여 줌 |
| PV_S100 | 55–60초 | logo, slogan, audition end card | 현재 QHD layout placeholder를 기준으로 AE에서 최종 타이포 완성 |

## 3. 편집 시작 전 Gate

다음이 모두 닫히기 전에는 AE 본편 러프컷을 시작하지 않는다.

- 2560×1440, 60fps, 누락 없는 source sequence와 전후 180–300프레임 handles.
- 핵심 shot은 서로 다른 실제 capture invocation 3개, 비핵심 shot은 최소 1개.
- HUD-on, HUD-off, linked clean plate 및 frame ledger·Git/dependency·seed·camera·state·Timeline 증거.
- 승인 take와 linked clean plate의 전체 source range에 대한 QHD/hash, error-magenta, material/HUD runtime evidence, Rec.709 editorial original.
- 선택 구간 25% 검토에서 얼굴, boss, 공격 방향, 충돌점을 읽을 수 있고 black mesh·broken trail 회귀가 없음.
- current 12초 ungraded gold conform 재구성 및 검증.
- 임시 music, 핵심 SFX, 최소 VO와 cue mapping. 청취 대기는 명시적인 hold로 남긴다.
- 실제 선택 asset/font/audio/AI의 item-level rights/provenance 및 dependency closure.
- `D:/DimensionBrawl_PV/02_selects/PREEDIT_60S/preedit_60s_shot_gate_manifest.json` authoritative PASS.

## 4. 참고 영상에서 가져올 편집 문법

### Snowbreak 공식 PV — Titan Ymir

참고: https://www.youtube.com/watch?v=ztlor9pF_Ys

- 폐허 wide → boss silhouette/detail → 캐릭터별 실제 action bundle → 위기 재등장 → 팀/ultimate → logo의 상승 구조.
- 전반은 평균 약 1.5–2초 컷, 후반 climax는 0.4–0.9초 컷으로 점진 가속한다.
- boss wide/low angle, 캐릭터 close-up, 안정적인 shoulder gameplay를 역할에 따라 구분한다.
- 흰 explosion, portal, glowing core 같은 화면 속 사건을 transition 동기로 사용한다.
- 색은 City의 cold grey/cyan과 Olympus의 warm/orange·purple accent를 대비시키되, shot 간 노출과 피부·재질은 일치시킨다.
- HUD와 설명 super를 과다하게 겹치지 않는다. dialogue/feature copy가 action bundle의 쉼표 역할을 하게 한다.

### 디자이너 덕디 — 게임 광고 제작 workflow

참고: https://www.youtube.com/watch?v=3ghETAbSv4Y
보조 글: https://deokdi.tistory.com/32

AI 영상 생성 부분은 사용하지 않고 아래 편집·그래픽 원리만 자체 소스로 재구성한다.

- Roto Brush 또는 자체 matte로 캐릭터/배경을 분리해 깊이를 만들고, main copy와 주인공의 전후 관계를 명확히 한다.
- 밝은 background variation, speed line, 가는 선형 타이포로 움직임과 정보 밀도를 만든다.
- 하나의 grid system을 반복 사용하고, 승인된 skill icon·arrow·feature super로 시선을 유도한다. dummy text는 사용하지 않는다.
- crop된 여백에는 split plane, video mask/repeat, 서로 다른 blur 강도, highlight line을 조합한 절제된 glass/crystal layer를 사용한다.
- 굵기가 다른 arrow와 내부 grid를 교차시키고, arrow 내부에 다음 scene을 먼저 보여 주는 concept transition을 City→Olympus에 적용한다.
- feature text 내부에는 Fractal Noise 기반 digital texture를 약하게 넣는다.
- 3D depth가 필요하면 프로젝트가 소유한 Unity asset만 사용한다.
- dark↔bright 장면을 교대로 배치하고, 제목 글자 형태는 날카로운 action 인상을 주되 25% 가독성을 우선한다.
- blur, shake, 1–2프레임 white/black impact, Blind 계열 flash, Lens Distortion은 순간 충돌과 transition에만 쓴다. 구조적 게임 결함을 감추는 용도로 쓰지 않는다.
- sound를 러프컷 초기부터 배치하고, 대표 shot을 먼저 완성한 뒤 전체 흐름·ending·BI를 반복 검토한다.

## 5. After Effects 실행 설계

### 러프컷

1. 승인 take와 Rec.709 editorial original만 import한다.
2. 2560×1440/60fps/60초 master comp를 만들고, 먼저 music·impact·rift·finisher cue를 marker로 놓는다.
3. S010–S100 역할 순서대로 picture-only assembly를 만든다.
4. City 약 25%, Olympus와 boss 약 65%, end card 약 10%의 체감 비중을 유지한다.
5. S020과 Olympus gameplay에는 각각 5초 이상 HUD-on 연속 shot을 보존한다.
6. gameplay 인과를 해치는 speed ramp나 프레임 보간은 사용하지 않는다.

### 최종 polish

- City→Olympus: dimension-rift matte, arrow/grid concept transition, 짧은 white/black impact.
- action: collision hard cut, 제한된 time remap, directional blur, 2D shake. impact 전후의 실제 접촉 프레임을 보존한다.
- typography: title/feature super/logo/end card, Pretendard 기반 grid, 25% 가독성, digital texture는 저강도.
- compositing: roto/matte depth, split plane, repeat panel, restrained glassmorphism, highlight line.
- look: shot별 exposure/white balance/color match 후 restrained glow/halation과 최종 grain. magenta/null/black 오류를 효과로 숨기지 않는다.
- audio: music, ambience, gun/dodge/summon/hit, boss charge/fire/death, wing/eye cue, comms·Inori·boss VO를 picture beat에 맞춘다.
- mix target: 최종 -14 LUFS Integrated, -1 dBTP. dialogue/important SFX에서 music ducking을 자동화한다.

## 6. 메모리·안정성 규칙

- Unity와 After Effects를 동시에 실행하지 않는다.
- capture/test는 한 프로세스씩 실행하고 완전 종료를 확인한 뒤 다음 작업으로 간다.
- PNG sequence는 한 파일 또는 작은 chunk로 처리하고 전체 QHD sequence를 메모리에 올리지 않는다.
- AE에서는 proxy 우선으로 편집하고 최종 render 전에 QHD originals를 reconnect한다.
- long comp는 segment별 pre-render/cache를 쓰고, 동시에 여러 sequence를 RAM preview하지 않는다.
- 실패 capture와 성공 capture를 섞지 않으며, manifest가 terminal-last로 확정되기 전에는 승인 take로 취급하지 않는다.

현재 환경 고정값:

- After Effects 2026 v26.3, Media Encoder 2026 v26.3.2, RAM 약 31GB.
- AE 첫 실행 직후 disk cache를 C: Temp에서 `D:/DimensionBrawl_PV/90_cache/AE26_3`로 옮기고 100–150GB로 제한한다.
- Adobe 공용 cache는 `D:/DimensionBrawl_PV/90_cache/AdobeCommon`을 사용한다.
- 편집·precomp 단계에서는 Multi-Frame Rendering을 끄고 다른 앱용 RAM 10–12GB를 남긴다.
- 프로젝트/자동저장/proxy는 각각 `D:/DimensionBrawl_PV/04_ae_project/project`, `autosave`, `proxies`에 둔다.
- master/submission/QA는 각각 `D:/DimensionBrawl_PV/05_delivery/local_master`, `submission`, `qa`에 둔다.
- Saber는 설치되어 있지 않으므로 사용하지 않는다. native Glow, Beam/Stroke, Fractal Noise, Lens/Blur, Lumetri, Mocha AE로 동일한 역할을 만든다.
- 승인 take 한 개씩 1280×720/60fps ProRes Proxy를 만들고, 최종 렌더 때만 QHD60 Rec.709 originals로 교체한다.

## 7. 제출 QA와 산출물

- 제21회 경기게임오디션 공고 기준 제출 영상은 1분 이내이며 MP4, MOV 등 재생 가능한 영상 파일이어야 한다.
- 제출 파일명: `2026경기게임오디션_게임영상_팀명` 형식. 실제 팀명은 제출 패키지 동결 시 입력한다.
- 게임소개서 등 전체 제출서류 합계는 1GB 이하여야 하며, 참가접수와 이메일 서류제출은 2026-08-20 14:00까지 모두 끝나야 한다.
- 영상: 3600프레임/60초 archive master를 보존하되, 제출 H.264 MP4는 컨테이너·AAC padding까지 1분을 넘지 않도록 3594프레임/59.9초로 안전하게 마감한다. no-subtitle, no-audio, clean 버전도 함께 보존한다.
- 검토: PC monitor, mobile, 이어폰, 스피커에서 밝기·색·타이포·음량·impact를 확인한다.
- frame QA: 시작/종료, transition, subtitle, logo, action contact frame을 25%와 100%에서 검사한다.
- 권리 QA: 실제 사용된 asset/font/audio/AI 항목만 최종 timeline-wide ledger에 연결한다.
- 공고 근거: `2026년 경기게임오디션(제21회) 참가사 모집 공고문(배포용).hwp`, SHA-256 `c081619e7135fc77590edb8e0acd4de0b7560cf6390d367865180879f6a4aa55`. 공개 사본: https://www.gwtp.or.kr/gwtp/bbsNew_view.php?bbs_data=aWR4PTMzMTQmc3RhcnRQYWdlPSZsaXN0Tm89JnRhYmxlPWNzX2Jic19kYXRhX25ldyZjb2RlPXN1YjAxZyZzZWFyY2hfaXRlbT0mc2VhcmNoX29yZGVyPSZ1cmw9c3ViMDFnJmtleXZhbHVlPXN1YjAx%7C%7C
- 외부 제출, 이메일 전송, 업로드는 완성 파일과 최종 명세를 사용자에게 보여 준 뒤 실행한다.

## 8. 현재 체크포인트

- 12초 historical conform은 기술 검증 PASS지만 current capture contract와 human approval을 다시 묶어야 한다.
- S100 QHD layout placeholder는 존재하며 최종 slogan/audition wording과 AE final graphic은 picture-lock 단계 항목이다.
- 60초 authoritative manifest, handle-backed current captures, automated evidence, Rec.709 editorial originals, human review, audio/right closure가 완료되어야 AE 본편으로 넘어간다.
