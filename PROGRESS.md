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
5. **4차 피벗 (현재)**: 컨셉아트 그대로 시설 평면도 + 실시간 자유 이동 + E키 근접 상호작용

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

## 다음 단계 후보

### 우선순위 1: 콘텐츠 (스파이별 맞춤 스토리)

현재 NPC 대화는 직업별 고정 라인. 스파이 정체에 따라 분기되지 않음.

**구상**:
- 3 스파이 × 3 NPC = 9개 대화 분기
- 각 NPC가 (자신이 결백할 때) 진짜 스파이를 미묘하게 암시
- 환경 단서 (서버 로그, 메모, USB) 5~7개 추가
- 거짓 단서 (red herring) 섞기

**구현 방향**: NPCInteractable에 `Dictionary<RoleType, string>` 형태로 스파이별 대화 분기, 또는 ScriptableObject SO로 대화 데이터 분리.

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

## 사이드 씬 (참고)

- `Assets/_Project/Scenes/GameScene.unity` — 1차 피벗 단일 룸 탐정 MVP (백업, 사용 안 함)
- `Assets/_Project/Scenes/OverworldScene.unity` — 2차 피벗 노드 맵 시안 (폐기, 사용 안 함)

## 기술 스택

- Unity 6 + URP
- New Input System (`Keyboard.current.<key>.isPressed`)
- OnGUI (IMGUI) HUD — 임시. 추후 UI Toolkit 또는 Canvas/TMP로 교체 가능
- Unity MCP for Claude (자동 코드/씬 조작)

## 사용자 환경

- 메인 작업: `C:\Users\CHOMK\nity-mcp-test\` (Windows)
- Unity 프로젝트 루트는 메인 디렉토리 (worktree 사용 X)
- GitHub: `chomk23/nity-mcp-test` 리모트 연결
