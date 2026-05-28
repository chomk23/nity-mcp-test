# For The Company — 진행 상태

> **Claude 안내**: 다른 컴퓨터에서 이 프로젝트를 이어받을 때 이 문서를 먼저 읽으세요. 디자인 피벗 이력, 현재 구현 상태, 다음 단계가 모두 정리되어 있습니다.

## 게임 컨셉

**《For The Company》** — 산업보안 1인 탐정 게임. 대기업 연구시설에서 산업스파이를 찾는 미스터리. 컨셉아트는 사용자 데스크탑 보유 (3x3 시설 평면도 + 9개 방 + 카드키 구역).

**핵심 게임 감정**: "누가 CCTV 껐지?" — 정보의 불확실성 + 의심

## 디자인 피벗 이력

1. **초기**: For The King 오마주, 협동 멀티 (5직업)
2. **1차 피벗**: 1인 탐정 모드 (단일 룸, 턴제)
3. **2차 피벗**: FTK식 오버월드 노드 맵 + 다중 인카운터
4. **3차 피벗**: 단일 씬 모달 인카운터 (Slay the Spire 방식)
5. **4차 피벗**: 컨셉아트 그대로 시설 평면도 + 실시간 자유 이동 + E키 근접 상호작용
6. **5차 피벗 (현재, 2026-05-22)**: 자유 탐색 → **선형 스토리 가이드 모드**. 자유 이동은 유지하되 경비원 NPC가 첫 안내 + 우상단 "현재 목표" HUD로 다음 행동 명시. 6단계 진행 (경비원 → 연구원 → 레이싱 → 네트워크관리자 → 시설관리자 → 지목). 자유도와 가이드의 균형.

## 현재 구현 상태

### 메인 씬: `Assets/_Project/Scenes/FacilityScene.unity`

- **맵**: 30x24 시설 평면도를 1.5배 확장 → 45x36 (Facility 부모 GameObject로 묶음)
- **방 8개 + 카드키 구역 1개** (컨셉아트 매치):
  - 연구실(파랑), 서버실(빨강, 큼), 보안통제실(보라)
  - 휴게실(초록), 카드키 구역(빨강, 잠금)
  - 창고(갈색), 데이터센터(청록, 큼), 전력실(노랑)
- **각 방 단일 문**, 방 사이 복도로 연결
- **빨간 지목 콘솔** 보안통제실 안에 배치
- **카드키 구역**과 **전력실** 분리 (각자 서쪽 문)

### 시스템

| 시스템 | 파일 | 역할 |
|---|---|---|
| 자유 이동 | `Scripts/Player/RealtimePlayerController.cs` | WASD + Shift 달리기 (1.8배) |
| 카메라 | `Scripts/Core/FollowCamera.cs` | 플레이어 추적 + 마우스 휠 줌 (0.5~1.8배) |
| 상호작용 인터페이스 | `Scripts/Player/IInteractable.cs` | E키 근접 상호작용 표준 |
| NPC 대화 | `Scripts/Player/NPCInteractable.cs` | 단서 +1~+2, 직업별 첫 대화 라인 |
| NPC 데이터 | `Scripts/Player/NPCActor.cs` | 데이터 + isSpy + suspicion |
| NPC 명부 | `Scripts/Managers/NPCRoster.cs` | 게임 시작 시 랜덤 스파이 지정 |
| 플레이어 상호작용 | `Scripts/Player/PlayerInteractor.cs` | 가장 가까운 IInteractable 발동 |
| 지목 콘솔 | `Scripts/Player/AccusationConsole.cs` | 단서 3개 이상 시 지목 모달 열기 |
| HUD | `Scripts/Systems/FacilityHUD.cs` | 단서/프롬프트/토스트/모달/승패/R 재시작 |
| 세션 | `Scripts/Core/GameSession.cs` | DontDestroyOnLoad, 단서/Outcome |

### NPC 3명 배치

- 🤍 **연구원** → 연구실 (-17, 1.8, 11)
- 🟢 **네트워크관리자** → 서버실 (0, 1.8, 11)
- 🟠 **시설관리자** → 전력실 (18, 1.8, -11)

### 컨트롤

- WASD: 이동
- Shift: 달리기 (1.8배)
- 마우스 휠: 줌인/줌아웃
- E: NPC 대화 또는 지목 콘솔 사용
- R: 게임 종료 시 재시작

## 게임 루프 (완전 작동)

```
시작 (중앙 복도, Console 로그에 스파이 누군지 표시 — 디버그용)
  ↓ WASD/Shift 이동
NPC와 대화 (E)
  ↓ 단서 +1~+2 누적
보안통제실 (보라 방) 의 빨간 지목 콘솔로 이동
  ↓ E 누르면 지목 모달
3명 NPC 중 선택
  ↓
정답 → 승리 / 오답 → 패배 (실제 스파이 공개)
  ↓ R 또는 모달 버튼
씬 리로드 + 새 스파이 랜덤 배정
```

## 콘텐츠 시스템 (완료)

### NPC 대화 스파이별 분기 (2026-05-21 추가)

- [NPCInteractable.cs](Assets/_Project/Scripts/Player/NPCInteractable.cs) 안에 `ResolveLine(speaker, spy, firstTime)` 메서드
- **3 스파이 × 3 NPC × 2 (첫/반복) = 18개 대사 분기**
- 무고한 NPC는 진짜 스파이를 미묘하게 암시, 스파이 NPC는 회피/방어 발언
- NPCRoster.Spy를 통해 현재 게임의 스파이 정체 파악

### 환경 단서 + 보안 퀴즈 미니게임 (2026-05-21 추가)

- [ClueObject.cs](Assets/_Project/Scripts/Player/ClueObject.cs) — IInteractable, 색깔 큐브 GameObject
- [SecurityQuizController.cs](Assets/_Project/Scripts/Systems/SecurityQuizController.cs) — `[RuntimeInitializeOnLoadMethod]`로 씬 로드 후 자동 스폰. 6개 단서 + 4지선다 퀴즈
- FacilityHUD에 **퀴즈 모달** + **정답 토스트** 추가
- 6개 단서 위치:
  - 연구실 정체불명 USB (-20, 0.6, 14)
  - 서버실 모니터 (3, 0.6, 14)
  - 데이터센터 트래픽 단말 (3, 0.6, -14)
  - 휴게실 메모 (-15, 0.6, -2)
  - 창고 수상한 택배 (-20, 0.6, -14)
  - 카드키 발급 로그 (18, 0.6, 2)
- 각 단서 — 보안교육 4지선다 → 정답이면 +2 단서 + 비밀 텍스트, 오답이면 재시도

### NPC 머리 위 이름표 (2026-05-21 추가)

- FacilityHUD.DrawNPCNameplates() — `Camera.main.WorldToScreenPoint` 로 NPC 머리(+2.4 unit) 위치를 화면 좌표로 변환
- "연구원", "네트워크관리자", "시설관리자" 라벨 항상 표시

### 단서 총량 (현재)

- NPC 대화 × 3 = 첫 +2 × 3 = 최대 +6
- 환경 단서 × 6 = +2 × 6 = +12
- **최대 ~18 단서**, 지목 최소 단서 = 3개

## 콘텐츠 시스템 (계속)

### Security Race 미니게임 — UnityWebBrowser 임베드 (2026-05-22 완료)

기존 standalone HTML(`security-race.html`)을 **옵션 A: UnityWebBrowser(UWB) CEF 임베드**로 통합. 외부 브라우저 폴백 없이 Unity 게임 화면 안에서 실행.

- [RacingConsole.cs](Assets/_Project/Scripts/Player/RacingConsole.cs) — 휴게실 시안색 캐비닛(-13, 0.75, 2), E키 상호작용
- [RacingMissionController.cs](Assets/_Project/Scripts/Systems/RacingMissionController.cs) — Phase 상태, JS 브릿지로 자동 1등 감지 (`OnHtmlRaceFinished(rank)`), 보상 +5 단서, 1회 클리어 한정
- [RacingWebViewBridge.cs](Assets/_Project/Scripts/Systems/RacingWebViewBridge.cs) — Canvas show/hide(`CanvasGroup.alpha`), CEF pre-warm(씬 시작 시 백그라운드 로드 → 첫 진입 시 즉시 반응), Hide() 시 `location.reload()`로 페이지 초기화
- [RacingWebViewSetup.cs](Assets/_Project/Scripts/Editor/RacingWebViewSetup.cs) — `For The Company → Create Racing WebView Canvas` 메뉴로 1280×720 RawImage+WebBrowserUIBasic+CanvasGroup+EventSystem 자동 구성
- [security-race.html](Assets/StreamingAssets/security-race.html) — JS 브릿지(`uwb.ExecuteJsMethod('OnRaceFinished', rank)`) + 가로 레이아웃(`.race-row` flex, race-viewport 좌 / question-card 우)
- 패키지: `dev.voltstro.unitywebbrowser` + `engine.cef` + `engine.cef.win.x64` (2.2.8), Voltstro UPM scoped registry

WebView 활성 동안 [RealtimePlayerController](Assets/_Project/Scripts/Player/RealtimePlayerController.cs) WASD 차단, [FollowCamera](Assets/_Project/Scripts/Core/FollowCamera.cs) 줌 차단, FacilityHUD의 NPC 이름표 숨김 — 모두 `RacingWebViewBridge.Instance.IsShowing` 기반.

## 다음 단계 후보

### 메인 메뉴 + 인트로 컷씬 (2026-05-22 완료)

- [MainMenuController.cs](Assets/_Project/Scripts/Systems/MainMenuController.cs) — OnGUI 메뉴 (FOR THE COMPANY 타이틀 + 인트로 + 시작/종료) + 3장 인트로 컷씬 (기밀 보고서 → 용의자 3명 → 당신의 임무)
- [MainMenuSetup.cs](Assets/_Project/Scripts/Editor/MainMenuSetup.cs) — "For The Company → Create Main Menu Scene" 메뉴, MainMenuScene.unity 자동 생성 + Build Settings에 0번 등록 (FacilityScene 1번)
- [FacilityHUD.cs](Assets/_Project/Scripts/Systems/FacilityHUD.cs) 결말 모달에 "메인 메뉴 (M)" 버튼 + M키 단축키
- 스파이 정체 콘솔 노출은 `#if UNITY_EDITOR`로 감쌈 — 빌드된 게임에선 "스파이 1명 무작위 지정" 익명 메시지만 출력 ([NPCRoster.cs:46](Assets/_Project/Scripts/Managers/NPCRoster.cs:55), [GameSession.cs:50](Assets/_Project/Scripts/Core/GameSession.cs:54))

### 씬 전환 시 자동 스폰 fix (2026-05-22)

`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`는 첫 씬에서만 실행되므로 MainMenu→Facility 전환 시 사물 스폰이 안 됨. [SecurityQuizController](Assets/_Project/Scripts/Systems/SecurityQuizController.cs), [RacingMissionController](Assets/_Project/Scripts/Systems/RacingMissionController.cs), [RacingWebViewBridge](Assets/_Project/Scripts/Systems/RacingWebViewBridge.cs) 모두 `SceneManager.sceneLoaded` 이벤트로 매 씬 진입 시 `EnsureSpawned()` 호출 (FacilityScene 이름 체크).

### SecureSense 디자인 시스템 적용 (2026-05-25)

Claude Design (claude.ai/design)에서 보낸 디자인 번들 `security-education-ui-remix` 기반:
- 다크 베이스(#050608) + 네온 그린/시안/마젠타/바이올렛 액센트
- JetBrains Mono + Space Grotesk (모노 터미널 미학)
- 스캔라인, 글리치, 펄스 dot, 가짜 OS 윈도우 chrome

[UITheme.cs](Assets/_Project/Scripts/Systems/UITheme.cs) — 디자인 토큰 시스템
- 색상 팔레트 (Bg0~4, Ink, Neon × 6, 상태색)
- 헬퍼: `DrawRect`, `DrawBorder`, `DrawCard`, `DrawPulseDot`, `DrawProgressBar`, `DrawWinBar`, `DrawScanlines`, `DrawGridBg`
- UI 컴포넌트: `NeonButton`, `GhostButton`, `DrawTag`
- 공용 GUIStyle 캐시: Title, Mono, MonoSmall, InkDimLabel, NeonLabel

[MainMenuController.cs](Assets/_Project/Scripts/Systems/MainMenuController.cs) — Mission Dossier 톤 리디자인
- 가짜 OS 윈도우 (mac dot 3개) + 그리드 배경 + 스캔라인
- 글리치 효과 타이틀 (시안/마젠타 오프셋 + 메인 흰색)
- 네온 그린 "INITIATE INVESTIGATION" 버튼
- 인트로 3장: CLASSIFIED 형식 + 챕터 dot 인디케이터

[FacilityHUD.cs](Assets/_Project/Scripts/Systems/FacilityHUD.cs) — 게임 내 HUD 전체 리디자인
- **DrawMiniMap** (좌상단): 시설 평면도 미니맵 + 방 사각형/이름 + 플레이어 펄스 점 + NPC 점 (의심도 색)
- **DrawInfoPanel** (우상단, 통합): 수사 진행 중 헤더 + 단서·시간 2열 + 현재 목표 + 진행 dot + 위치 힌트
- **DrawDialogueBox**: 하단 1120 너비 + 네온 그린 상단 라인 + NPC 이름표 + 터미널 본문
- **DrawDialogueChoices**: 우측 선택지 (01·02·03 번호 + 시안 강조 띠 + 호버 그린)
- **DrawInventoryPanel**: evidence-archive.dossier 윈도우 + 카드별 출처 태그 (ENV·LOG / TESTIMONY / MISSION)
- **DrawQuestAdvanceToast**: 그린 보더 + STAGE CLEARED 헤더 + 페이드아웃
- **DrawInteractionPrompt**: 시안 보더 + 펄스 dot + ▸ 화살표 프롬프트
- **DrawToast**: NeonYellow INTEL // RELAY 토스트 (NPC 대화 결과)
- **DrawAccusationModal**: verdict.terminal + 매젠타 보더 + FINAL ACCUSATION
- **DrawEndScreen**: mission-result.dossier + VICTORY/DEFEAT (그린/레드)
- **DrawHint**: 하단 바 (// [WASD] MOVE · [SPACE] INTERACT ...)
- **DrawNPCNameplates**: 의심도 따라 보더 색 변경 (Line/NeonYellow/Danger)
- **DrawQuizModal**: security-training.module + NeonViolet + 선택지 0X 번호
- **DrawRacingModal**: security-race.module + 시안 보더 + 1ST/2ND/3RD 결과

### Sci-Fi 바닥·벽 머터리얼 변경 (2026-05-24)

- [SciFiFloorsSetup.cs](Assets/_Project/Scripts/Editor/SciFiFloorsSetup.cs) — 메뉴 "Setup Sci-Fi Floors". 기존 방 색깔 바닥 8개(Floor_Research, Floor_Server 등) SetActive(false) + 20m Epoxy Ground 9장(3×3) + 방별 카펫 8장 자동 배치. SciFiFacility는 안 건드림.
- [SciFiWallsSetup.cs](Assets/_Project/Scripts/Editor/SciFiWallsSetup.cs) — 메뉴 "Setup Sci-Fi Walls". 외곽 벽 4개(Wall_North/East/South/West) + 내부 벽 W_* 모두 MeshRenderer sharedMaterial을 `ScifiOfficeLite/Walls/Wall Set 2.mat`로 일괄 교체. 위치/사이즈 그대로, 색감만 sci-fi 톤.

### Sci-Fi Facility prefab 워크플로우 전환 (2026-05-24)

SciFiFacilitySetup 메뉴를 prefab instantiate 방식으로 단순화.

- 사용자가 Unity Editor에서 시설 가구 배치를 직접 수정한 후 SciFiFacility GameObject를 `Assets/_Project/Prefabs/SciFiFacility.prefab`으로 저장
- [SciFiFacilitySetup.cs](Assets/_Project/Scripts/Editor/SciFiFacilitySetup.cs)는 그 prefab만 instantiate — 80여 개 코드 좌표 제거하고 단순화
- 가구 변경: Hierarchy에서 수정 → Inspector "Overrides → Apply All" 클릭하면 prefab에 자동 반영

### Sci-Fi Facility — 외부 에셋 prefab 기반 시설 (2026-05-24)

[SciFiFacilitySetup.cs](Assets/_Project/Scripts/Editor/SciFiFacilitySetup.cs) — "For The Company → Setup Sci-Fi Facility" 메뉴로 한 번에 시설 전체 가구/바닥/조명을 [ScifiOfficeLite](Assets/ScifiOfficeLite/) (Terresquall, Free Sci-Fi Office Pack)의 sci-fi prefab들로 교체.

- 부모 GameObject `SciFiFacility` 아래로 묶음
- **바닥**: 시설 중앙에 20m Epoxy Ground 한 장 (기존 색깔 바닥 위 살짝 떠 있음)
- **연구실**: Table Dark Oak + Office Chair + PC 2 + TV 32" + Drawer
- **서버실**: Server Rack 4대 + Mechanical Arm 2개
- **데이터센터**: Server Rack 3대 + 모니터 벽 2개
- **휴게실**: Table White Wood + 의자 2개 + TV + Stool (RacingConsole 옆)
- **창고**: Shelf with Crates 2개 + Shelf without Crates
- **전력실**: Server Rack 2 + Mechanical Arm + Drawer
- **보안통제실**: Drawer Table Long + 모니터 벽 2개 + PC + Office Chair
- **카드키 구역**: Drawer Table Long + PC
- **조명**: 각 방 천장에 Ceiling Light 8개 (서버실/전력실/보안통제실은 Bright 버전)

[RoomFurniture.cs](Assets/_Project/Scripts/Systems/RoomFurniture.cs) — `SciFiFacility` GameObject가 씬에 있으면 primitive 가구 자동 스폰 안 함 (둘 동시 사용 방지).

벽은 아직 교체 안 됨 (기존 회색 벽 유지). 사용자 피드백 후 추가.

### 방 디테일 — 가구·서버랙·모니터 자동 스폰 (2026-05-23)

[RoomFurniture.cs](Assets/_Project/Scripts/Systems/RoomFurniture.cs) — RuntimeInitialize + sceneLoaded로 FacilityScene 진입 시 약 28개 primitive 가구를 자동 배치. 부모 `RoomFurniture` GameObject 아래로 묶음. Collider 제거(플레이어 통과).

| 방 | 가구 |
|---|---|
| 연구실 | 책상 + 모니터 + 의자 + 책장 |
| 서버실 | 서버랙 4대 + 시안 LED |
| 데이터센터 | 서버랙 3대 + 트래픽 모니터 벽 |
| 휴게실 | 소파 + 테이블 + 보라 자판기 |
| 창고 | 종이박스 팰릿 3개 |
| 전력실 | 변압기 2대 + 케이블 박스 |
| 보안통제실 | 거대 모니터 벽 + 책상 + 의자 |
| 카드키 구역 | 발급 단말 + 시안 LED |

각 가구 색상은 어두운 톤(검정·갈색·짙은 회색) + 일부 강조(시안 LED, 보라 자판기) — Bloom과 어울리는 사이버 분위기.

### 시각 폴리시 — URP Post-Processing + 라이팅 (2026-05-23)

[PostProcessingSetup.cs](Assets/_Project/Scripts/Editor/PostProcessingSetup.cs) — "For The Company → Setup Post Processing" 메뉴로 한 번에 자동 구성.

- VolumeProfile asset 생성 (`Assets/_Project/Settings/FacilityPostProcessing.asset`)
- Global Volume GameObject + Profile 할당
- Camera.UniversalAdditionalCameraData.renderPostProcessing=true + SMAA
- Directional Light: 따뜻한 흰색, Soft Shadows, 50°/-40° 각도
- Bloom(0.6) + Color Adjustments(콘트라스트+15, 채도+20) + Vignette(0.32) + Tonemapping(Neutral)

각 씬에서 메뉴 한 번씩 실행. MainMenuScene과 FacilityScene 별도 적용 가능.

### 대화 확장 + NPC 이동 + 카드키 (2026-05-23)

스토리 풍부함 + 시설이 살아있는 느낌 + 단계간 게이트 추가.

**NPC 대화 다양성** — [NPCInteractable.cs](Assets/_Project/Scripts/Player/NPCInteractable.cs)
- ResolveLine 단일 문장 → BuildFirstTalkLines/Choices 다중 라인(3줄) + 분기 선택지 9 케이스(3 NPC × 3 스파이)
- 캐릭터 톤 차별화: 연구원=학자, 네트워크관리자=기술자/로그/패킷, 시설관리자=현장/CCTV/카드키
- 자기가 스파이일 때 회피·변명·다른 사람 책임 전가, 무고할 때 구체적 시간·장소 단서

**경비원 증언 인벤토리 기록** — [GuardNPC.cs](Assets/_Project/Scripts/Player/GuardNPC.cs) Briefing 종료 콜백에서 AddClue(ClueSource.NPC)

**NPC 자동 이동** — [NPCPatrol.cs](Assets/_Project/Scripts/Player/NPCPatrol.cs)
- NPCRoster.Awake에서 모든 NPC에 자동 부착
- 시작 위치 주변 3 waypoint를 천천히(1.4 u/s) 왔다 갔다, 도착 시 2.2초 idle
- 대화 중(이 NPC가 대화 대상)이면 정지, 결말 후 정지

**카드키 + 전력실 잠금** — [LockedDoor.cs](Assets/_Project/Scripts/Player/LockedDoor.cs) + [GameSession.hasFacilityCardkey](Assets/_Project/Scripts/Core/GameSession.cs)
- 전력실 입구(12, 1.2, -11)에 빨간 잠금문 자동 스폰 (BoxCollider로 길 막음)
- [QuestManager.ApplyStageSideEffects](Assets/_Project/Scripts/Systems/QuestManager.cs): MeetNetworkAdmin 단계 완료 시 hasFacilityCardkey=true + 인벤토리에 "보안 카드키 획득" 추가
- LockedDoor가 GameSession 매 프레임 체크 → 카드키 발급되면 자동 collider 비활성 + 녹색으로 변경 + 살짝 축소(문 열린 느낌)

**의도된 흐름**: 네트워크관리자 대화 → 카드키 발급 → 전력실 잠금 해제 → 시설관리자 대화 가능. 단계 4와 5 사이의 자연스러운 게이트.

**NPC 대화 → 환경 단서 자동 트리거** ([NPCInteractable.AutoOpenRelatedClueAfterDelay](Assets/_Project/Scripts/Player/NPCInteractable.cs))
- 첫 대화 종료 후 0.7초 뒤 자동으로 NPC 방 관련 환경 단서 퀴즈 모달이 열림
- 매핑: 연구원→research_usb, 네트워크관리자→server_log, 시설관리자→cardkey_log
- 다른 모달 떠 있으면 자동 오픈 안 함 (안전장치)
- ESC 누르면 닫을 수 있고 단서 큐브로 다시 가서 풀이 가능 — 자유도 유지

### 인벤토리 — 수집한 단서 누적 + I키 토글 (2026-05-23)

- [GameSession](Assets/_Project/Scripts/Core/GameSession.cs): `ClueEntry`/`ClueSource`(Environment/NPC/Minigame) + `CollectedClues` 리스트 + `AddClue(title, text, source)`
- [SecurityQuizController.Answer](Assets/_Project/Scripts/Systems/SecurityQuizController.cs) 정답 시 → `Environment` 단서 추가
- [NPCInteractable.ApplyTalkRewards](Assets/_Project/Scripts/Player/NPCInteractable.cs) 첫 대화 종료 시 → `NPC` 증언 추가
- [RacingMissionController.ReportRank(1)](Assets/_Project/Scripts/Systems/RacingMissionController.cs) → `Minigame` 단서 추가
- [FacilityHUD.DrawInventoryPanel](Assets/_Project/Scripts/Systems/FacilityHUD.cs) — I키 토글, 화면 중앙 860×680 패널, 카드 리스트 (출처별 색깔 띠: 노랑=환경, 파랑=증언, 초록=미니게임), 스크롤 지원
- 인벤토리 열림 중에는 RealtimePlayerController/FollowCamera/PlayerInteractor 모두 입력 차단
- 다른 모달(대화/퀴즈/레이싱/지목) 떠 있으면 I키 토글 무시

### 스토리 가이드 강화 — 차례 안내 + 단서 단계 잠금 (2026-05-23)

스토리 모드의 선형 진행이 더 명확하도록 두 가지 보강.

- [NPCInteractable.ShowOutOfTurnGuidance](Assets/_Project/Scripts/Player/NPCInteractable.cs) — 차례가 아닌 NPC와 대화 시도 시 "잠시만요, 지금은 저와 이야기할 차례가 아닌 것 같습니다. 현재 목표: ..." 안내. 단서/의심도/단계 진행 모두 적용 안 됨, HasBeenTalkedTo도 변하지 않아 차례가 되면 본격 대화 가능
- [ClueData.stageRequired + unlockHint](Assets/_Project/Scripts/Player/ClueObject.cs) — 6개 환경 단서가 QuestManager 단계에 묶임. 잠금 시 큐브가 회색으로 변하고 PromptText는 `잠금 — XX 만난 후`
  - research_usb → MeetResearcher(1) 부터
  - lounge_memo → RacingMission(2) 부터
  - server_log / data_traffic → MeetNetworkAdmin(3) 부터
  - storage_box / cardkey_log → MeetFacilityManager(4) 부터
- [ClueObject.Update](Assets/_Project/Scripts/Player/ClueObject.cs) — 단계 변경 즉시 색 자동 갱신 (회색 ↔ 원색)

**의도된 흐름**: 플레이어가 시설을 자유롭게 돌아다녀도 차례 아닌 NPC는 가이드만 제공, 단서 큐브도 회색으로 잠겨 있어 진행 순서가 시각적으로 명확. 단계 진행할 때마다 새 단서가 unlock되며 색이 켜짐.

### RPG 스타일 하단 대화창 (2026-05-22 완료)

영상(Roblox Studio RPG 대화 시스템) 참고하여 NPC 대화를 화면 하단 가로 박스로 전환.

- [DialogueSystem.cs](Assets/_Project/Scripts/Systems/DialogueSystem.cs) — 라인 큐 + 타이프라이터(45 chars/sec) + Space/Enter/E로 진행. 타이핑 중이면 즉시 완성, 완성 상태면 다음 라인. OnEnded 콜백 (보상/단계 진행 트리거)
- [FacilityHUD.DrawDialogueBox()](Assets/_Project/Scripts/Systems/FacilityHUD.cs) — 화면 하단 1100×180 박스 슬라이드 인 (ease-out cubic 0.25s) + 좌상단 NPC 이름 헤더 (박스 위로 살짝 튀어나옴) + 본문 wordWrap + 우하단 `▼ Space — 다음` 깜빡임 진행 표시
- [NPCInteractable.Talk()](Assets/_Project/Scripts/Player/NPCInteractable.cs), [GuardNPC.Interact()](Assets/_Project/Scripts/Player/GuardNPC.cs) — Toast 대신 DialogueSystem 호출, 대화 종료 콜백에서 단서/의심도/QuestManager 진행 적용
- 대화 중에는 [RealtimePlayerController](Assets/_Project/Scripts/Player/RealtimePlayerController.cs), [FollowCamera](Assets/_Project/Scripts/Core/FollowCamera.cs), [PlayerInteractor](Assets/_Project/Scripts/Player/PlayerInteractor.cs) 모두 입력 차단 — DialogueSystem이 Space/Enter/E 가로챔
- [FacilityHUD.DrawToast()](Assets/_Project/Scripts/Systems/FacilityHUD.cs), DrawInteractionPrompt — 대화 중에는 숨김
- DrawQuestAdvanceToast — 대화창 종료 대기 후 표시 (순차 흐름)

### 스토리 모드 — 선형 퀘스트 가이드 (2026-05-22 완료, 5차 피벗)

- [QuestManager.cs](Assets/_Project/Scripts/Systems/QuestManager.cs) — 6단계 enum + 현재 단계 추적 + `TryAdvance(stage)` / `TryAdvanceByRole(role)` + 단계별 목표 텍스트와 위치 힌트 + 진행 직후 토스트용 LastAdvanceText
  - 단계: Briefing → MeetResearcher → RacingMission → MeetNetworkAdmin → MeetFacilityManager → Accusation → Done
- [GuardNPC.cs](Assets/_Project/Scripts/Player/GuardNPC.cs) — **경비원 NPC** 중앙복도 (0, 1.1, 4)에 자동 스폰. 진한 파랑 캡슐. 스파이 후보 아님 (NPCRoster 미등록). 단계별로 다른 안내 대사 + Briefing 단계 트리거
- [FacilityHUD.cs](Assets/_Project/Scripts/Systems/FacilityHUD.cs) DrawObjectivePanel — **우상단 현재 목표 패널** (320×96, 단계 X/6 + 목표 + 위치 힌트) + DrawQuestAdvanceToast (단계 완료 시 중앙 토스트 4초)
- [NPCInteractable.cs](Assets/_Project/Scripts/Player/NPCInteractable.cs) — 대화 시 자기 역할이 현재 단계와 일치하면 QuestManager.TryAdvanceByRole 호출 (Researcher/NetworkAdmin/FacilityManager)
- [RacingMissionController.cs](Assets/_Project/Scripts/Systems/RacingMissionController.cs) — 1등 클리어 시 RacingMission 단계 진행
- [AccusationConsole.cs](Assets/_Project/Scripts/Player/AccusationConsole.cs) — `IsStoryReady()` 체크: QuestManager.CurrentStage가 Accusation일 때만 사용 가능 ("모든 용의자 조사 후 활성화")
- [PlayerInteractor.cs](Assets/_Project/Scripts/Player/PlayerInteractor.cs) — GuardNPC.LastResult도 화면에 표시되도록 분기 추가
- [GameSession.cs](Assets/_Project/Scripts/Core/GameSession.cs) — totalTime 300 → **420초(7분)** (스토리 진행 여유)

**플레이 흐름**:
1. 시작 → 우상단 "현재 목표: 중앙복도의 경비원에게 브리핑 받기"
2. 경비원 대화 → 사건 브리핑 + 다음 목표 "연구실의 연구원과 대화"
3. 자유 이동(다른 방 가도 됨, 그러나 다른 NPC 대화는 단계 진행 X) → 연구원과 대화 → 다음 목표
4. 휴게실 → 보안 레이싱 1등 → 다음 목표
5. 네트워크관리자 → 시설관리자 순으로 진행
6. 마지막 단계 도달 → 보안통제실 빨간 콘솔 활성화 → 지목
7. 단계 진행마다 우상단 패널 업데이트 + 중앙 토스트로 알림

자유도는 유지 (어디든 갈 수 있고 환경 단서 풀어 의심도 더 누적 가능), 다만 **메인 진행은 명확히 안내**됨.

### 게임플레이 깊이 — 시간 제한 + 의심도 시각화 (2026-05-22 완료)

- [GameSession.cs](Assets/_Project/Scripts/Core/GameSession.cs): `totalTime`(기본 300초/5분), `TimeRemaining`, `TimerActive` 필드 + 매 프레임 카운트다운. 0초 도달 시 자동 `DeclareLose("시간 초과 — 실제 스파이는 X")`. `StopTimer()/ResumeTimer()` 일시정지 지원
- [NPCInteractable.Talk()](Assets/_Project/Scripts/Player/NPCInteractable.cs): 의심도 메커닉 보정 — 무고한 NPC 첫 대화 → 진짜 스파이 의심도 +2 (단서 제공) / 진짜 스파이 본인 대화 → 자신 +1 (회피 신호)
- [SecurityQuizController.Answer()](Assets/_Project/Scripts/Systems/SecurityQuizController.cs): 환경 단서 정답 시 → 진짜 스파이 의심도 +2
- [FacilityHUD.DrawStatusBar()](Assets/_Project/Scripts/Systems/FacilityHUD.cs): 좌상단에 `남은 시간 MM:SS` 표시 — 60초 이하 노랑, 30초 이하 빨강
- [FacilityHUD.DrawNPCNameplates()](Assets/_Project/Scripts/Systems/FacilityHUD.cs): NPC 머리 위 이름표 아래에 의심도 막대 (0이면 안 보임, 1~3 회색, 4~6 노랑, 7+ 빨강, max 10)

**의도된 플레이 경험**:
- 5분 안에 단서 모으고 스파이 지목 — 결정 강제
- 의심도 막대로 "누가 의심받는지" 한눈에 확인 — 단서 진행을 시각적으로 피드백
- 무고한 NPC들 대화 → 진짜 스파이 막대만 빠르게 차오름 (자연스러운 정답 힌트)
- 만약 한 NPC만 회피적이라 그 사람 막대가 천천히 차면 → 그 사람이 스파이일 가능성 (역설적 단서)

### 우선순위 2: 시각 폴리시

- 캐릭터 모델 교체 (Meshy AI로 컨셉아트 → 3D)
- 방 디테일 (가구·컴퓨터·서버랙)
- 라이팅 / UI 폴리시

### 우선순위 3: 게임플레이 확장

- 시간/턴 제한
- 카드키 시스템 실제 작동 (전력실 잠금)
- NPC 자동 이동 (방 사이 어슬렁)
- 의심도 시각화 (NPC 머리 위 마커)
- 메인 메뉴 / 인트로 / 빌드

### 우선순위 2: 폴리시
- 사운드 (발소리, 문 열기, 상호작용 효과음)
- 메인 메뉴 씬 (시작/종료)
- 의심도 시각화 (NPC 머리 위 마커?)
- 카메라 페이드/전환

### 우선순위 3: 확장
- 더 많은 NPC (5명)
- 카드키 시스템 실제 작동 (전력실 잠금 해제)
- 시간 제한 (실시간 또는 턴 제한)
- 로그라이크 요소 (랜덤 단서 배치, 영구 업그레이드)

## 씬 구성

- `Assets/_Project/Scenes/MainMenuScene.unity` — 시작 화면 + 인트로
- `Assets/_Project/Scenes/FacilityScene.unity` — 메인 게임플레이
- (구 피벗 씬 GameScene·OverworldScene은 미사용으로 삭제됨)

## 기술 스택

- Unity 6 + URP
- New Input System (`Keyboard.current.<key>.isPressed`)
- OnGUI (IMGUI) HUD — 임시. 추후 UI Toolkit 또는 Canvas/TMP로 교체 가능
- Unity MCP for Claude (자동 코드/씬 조작)

## 사용자 환경

- 메인 작업: `C:\Users\CHOMK\nity-mcp-test\` (Windows)
- Unity 프로젝트 루트는 메인 디렉토리 (worktree 사용 X)
- GitHub: `chomk23/nity-mcp-test` 리모트 연결
