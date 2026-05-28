# 🎮 For The Company

> **산업보안 1인 탐정 게임** — 차세대 보안 칩 설계도가 외부로 유출됐다. 15분 안에 산업스파이를 찾아내라.

Unity 6 + URP로 만든 **사이버펑크 / 다크 네온 터미널 톤**의 단편 미스터리 게임입니다.
사내 보안 교육 콘텐츠를 게이미피케이션한 1인 시점 추리 게임으로, 환경 조사 · NPC 대화 · 미니게임을 통해 진짜 산업스파이를 추적합니다.

---

## 📖 스토리

> *"...출근하자마자 시설로 직행이라니. 무슨 사건이길래."*
> *"보안조사관 7년차에 이런 긴급 호출은 처음이야."*

대기업 연구시설 **'For The Company'** — 차세대 보안 칩 설계도가 외부로 유출됐다.
내부 감사 결과 시설 안의 누군가가 정보를 빼돌리고 있다.

용의자는 셋:
- 🔬 **연구원** — 칩 설계 핵심 인력
- 💻 **네트워크관리자** — 외부 통신 권한 보유
- 🔑 **시설관리자** — 카드키 발급 담당

당신은 보안조사관. **15분** 안에 단서를 모아 보안통제실에서 동료 **AI로봇 한세**에게 **단 한 번만** 진짜 스파이를 지목할 수 있다.

---

## ✨ 핵심 기능

### 🎬 시네마틱 오프닝
- 검은 화면에서 페이드 인 + 보안조사관이 시작 위치까지 워크
- 동시에 1인칭 속마음 5라인 자동 재생
- 컷씬 중 인풋 차단 (몰입감 강화)

### 🎨 SecureSense 디자인 시스템
- **다크 베이스 + 네온 액센트** (Green / Cyan / Magenta / Violet / Yellow)
- 가짜 OS 윈도우 chrome · 스캔라인 · 글리치 효과
- 네온사인 깜빡이는 메인 타이틀 ("FOR THE COMPANY")
- 사이버펑크 네온 십자 마우스 커서
- 모든 UI hover 효과 (버튼 · 텍스트 · 카드)

### 🗣 대화 시스템
- Typewriter 효과로 글자 하나씩 등장 (Undertale 스타일 비프 사운드)
- 화자 라벨 자동 분리 (NPC ↔ "나")
- 선택지 분기 → 응답 → 자연스러운 트랜지션
- NPC별 동적 대화 (스파이 본인은 회피적 톤, 무고한 NPC는 적극적)
- 대화 시 사선 카메라 시점 자동 전환

### 🔍 7단계 선형 스토리
```
1. 경비원 첫 브리핑       (선택지 3개 분기)
2. 연구원 대화 + 보안교육 (자동 트리거)
3. 경비원 중간 브리핑     (스파이 힌트)
4. 보안 레이싱 미니게임
5. 네트워크관리자 + 보안교육
6. 시설관리자 + 보안교육
7. 보안통제실에서 산업스파이 지목
```

### 📚 보안 교육 모듈 (실전 보안 콘텐츠)
- **20개 문제 풀** — 3개 카테고리 (외부 매체 / 네트워크 / 출입 보안)
- NPC 대화 종료 시 **연속 3문제 자동 출제** (풀에서 중복 없이 랜덤)
- 매 출제마다 선택지 순서 셔플 (정답 위치 매번 다름)
- 실제 보안 교육 주제: 피싱 · 2FA · USB 정책 · 카드키 분실 · CCTV · VPN 등

### 🏎 보안 레이싱 미니게임
- 휴게실 컴퓨터에서 발동
- 8-bit 종스크롤 레이싱 (HTML5 / WebView 임베드)
- 60초 안에 보안 문제 풀며 1등 노리기
- 1등 시 +5 단서 + 자동 단계 진행

### 🗂 Investigation Board 인벤토리
- 단서 카드 그리드 자동 배치 (태그별 색 분류: DATA · NET · BADGE · COMMS · LOG ...)
- **3명 용의자 카드 클릭 → 관련 단서로 네온 점선 자동 연결**
- 우측 SUSPECT FILE: 의심도 막대 · 연결 단서 수 · 동적 안내
- 카드 마우스 hover → 전체 내용 툴팁 표시

### 🎵 절차적 사운드 시스템
- 외부 사운드 파일 없이 코드로 직접 합성 (sin/square/triangle wave)
- 효과음 8종: hover · click · 대화 진행 · 단계 진행 · 정답 (arpeggio) · 오답 (부저) · 모달 오픈 · typewriter 비프
- BGM 3트랙 (메인 메뉴 · 게임플레이 · 엔딩) — 페이드 크로스페이드

### ⚙ 일시정지 메뉴 (ESC)
- 게임 계속하기 / 게임 설명 / 설정 / 메인 메뉴 / 종료
- **마스터 볼륨 슬라이더** (0~100%, 실시간 반영)
- 다른 모달이 떠있으면 ESC 가로채지 않음 (지능적 우선순위)

### 🏆 엔드 화면
- VICTORY / DEFEAT 큰 라벨
- **통계 패널**: 사용 시간 · 단서 수집 · 출처별 분류 (NPC 증언 / 보안교육 / 미니게임)
- **MOTIVE 박스**: NPC별 유출 동기 서사 (3가지 패턴 분기)

---

## 🕹 조작 방법

| 키 | 동작 |
|---|---|
| `WASD` | 이동 |
| `Shift` | 달리기 (1.8배 속도) |
| `마우스 휠` | 카메라 줌인/아웃 |
| `Space` | NPC 대화 · 단서 조사 · 지목 · 대화 진행 |
| `I` | 인벤토리 (Investigation Board) |
| `ESC` | 일시정지 메뉴 |
| `R` | 게임 종료 후 재시작 |
| `M` | 메인 메뉴 |

---

## 🛠 기술 스택

### 엔진 · 언어
- **Unity 6** (6000.4.7f1) + URP (Universal Render Pipeline)
- **C#** + .NET Standard 2.1
- **New Input System** (Keyboard.current API)
- **IMGUI / OnGUI** (UI 렌더링)

### 패키지 · 에셋
- **UnityWebBrowser** (CEF 엔진) — HTML5 미니게임 임베드
- **Kenney Blocky Characters** — NPC 모델 (절차적 걷기/제스처 애니메이션)
- **ScifiOfficeLite** (Terresquall) — Sci-Fi 가구/벽/바닥
- **Synty Polygon Office** — 추가 환경 에셋

### 개발 도구
- Git + Git LFS (대용량 에셋 관리)
- Unity MCP for Claude (자동 코드/씬 조작)

---

## 📂 프로젝트 구조

```
Assets/_Project/
├─ Scenes/
│  ├─ MainMenuScene.unity      ← 시작 화면 + 인트로
│  └─ FacilityScene.unity      ← 메인 게임플레이
├─ Scripts/
│  ├─ Core/                    ← GameSession, FollowCamera
│  ├─ Player/                  ← Player·NPC·인터랙션
│  ├─ Systems/                 ← UI·대화·퀴즈·BGM·SFX·인벤토리
│  ├─ Managers/                ← NPCRoster·Encounter
│  └─ Editor/                  ← 자동 빌드 스크립트
├─ Audio/Resources/BGM/        ← 메인메뉴·게임플레이·엔딩 트랙
└─ Prefabs/                    ← SciFiFacility 등
```

---

## 🎓 개발 컨셉

회사 보안교육 콘텐츠를 **게임으로 풀어낸 사이드 프로젝트**입니다.
실제 보안 사례(피싱·USB 분실·카드키 도난·VPN 등)를 게임 메커니즘에 녹여,
플레이어가 자연스럽게 보안 의식을 학습할 수 있도록 설계했습니다.

**디자인 컨셉**: Claude Design ([claude.ai/design](https://claude.ai/design))으로 작성한
`security-education-ui-remix` ("SecureSense") 디자인 시스템을 Unity OnGUI로 1:1 구현.

---

## 📝 라이센스

- **코드**: 개인 학습용 · 자유 사용
- **외부 에셋**: 각 에셋 라이센스 따름
  - Kenney Blocky Characters: CC0
  - ScifiOfficeLite: Terresquall 무료 라이센스
  - BGM: pixabay CC0

---
