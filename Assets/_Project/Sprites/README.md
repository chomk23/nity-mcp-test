# For The Company - 스프라이트 에셋 가이드

생성된 모든 PNG는 투명 배경이며 Unity에 드롭인 가능합니다. 아래 임포트 설정을 따라주세요.

## 폴더 구조

```
Assets/_Project/Sprites/
├── Characters/                  # NPC/플레이어 풀바디 스프라이트 (1024x1536)
│   ├── Char_Security.png
│   ├── Char_Researcher.png
│   ├── Char_NetworkAdmin.png
│   ├── Char_FacilityManager.png
│   └── Char_Insider.png
└── UI/
    ├── Icons/Roles/
    │   └── UI_RoleIcons_Sheet.png   # 5개 역할 배지 (가로 시트)
    ├── Panels/
    │   └── UI_HUD_Mockup.png        # HUD 레이아웃 시안 (참고용)
    └── Tiles/
        └── UI_TileMarkers_Sheet.png # 6개 타일 마커 (가로 시트)
```

## Unity 임포트 설정

### 1) 캐릭터 스프라이트 (Char_*.png)

Project 창에서 각 PNG 선택 → Inspector에서:

- **Texture Type**: `Sprite (2D and UI)`
- **Sprite Mode**: `Single`
- **Pixels Per Unit**: `512` (3D 씬에서 약 2~3 유닛 높이)
- **Pivot**: `Bottom` (지면 기준 정렬)
- **Filter Mode**: `Bilinear`
- **Compression**: `High Quality`
- **Generate Mip Maps**: 끔
- `Apply` 클릭

3D 씬에서 NPC에 사용하려면:
- 빈 GameObject → `Sprite Renderer` 추가 → Sprite 슬롯에 드롭
- 또는 `Quad`에 머티리얼로 할당 (URP/Unlit, Surface=Transparent)

### 2) Role 아이콘 시트 (UI_RoleIcons_Sheet.png)

5개 배지가 가로 한 줄로 배치된 시트입니다.

- **Texture Type**: `Sprite (2D and UI)`
- **Sprite Mode**: `Multiple`
- **Pixels Per Unit**: `100`
- **Filter Mode**: `Bilinear`
- `Sprite Editor` 열기 → `Slice` → `Type: Grid By Cell Count` → `Column: 5, Row: 1` → `Slice` → `Apply`
- 자동으로 `UI_RoleIcons_Sheet_0` ~ `_4` 5장이 생성됩니다.
- 권장 이름 변경:
  - `_0` → `Icon_Security`
  - `_1` → `Icon_Researcher`
  - `_2` → `Icon_NetworkAdmin`
  - `_3` → `Icon_FacilityManager`
  - `_4` → `Icon_Insider`

### 3) 타일 마커 시트 (UI_TileMarkers_Sheet.png)

6개 마커가 가로 한 줄로 배치되어 있습니다.

- **Sprite Mode**: `Multiple`
- `Sprite Editor` → `Grid By Cell Count` → `Column: 6, Row: 1` → `Slice`
- 권장 이름:
  - `_0` → `Tile_Spawn` (녹색 ↑)
  - `_1` → `Tile_Objective` (앰버 타겟)
  - `_2` → `Tile_Event` (적색 !)
  - `_3` → `Tile_Locked` (보라 자물쇠)
  - `_4` → `Tile_Patrol` (오렌지 눈)
  - `_5` → `Tile_Bonus` (시안 별)

### 4) HUD 목업 (UI_HUD_Mockup.png)

레이아웃 참고용 시안입니다 (직접 사용 X).
실제 UI 구축 시:
- Canvas (Screen Space - Overlay) 생성
- 패널/바/라벨을 시안과 동일한 위치/색상으로 배치
- 컬러 키:
  - 패널 BG: `#1A1F26` (다크 건메탈)
  - 패널 Border: `#33D1FF` (시안 네온) / `#FFB347` (앰버)
  - HP 바: `#FF4757` 그래디언트
  - AP 바: `#33D1FF` 그래디언트
  - Suspicion: `#FF8A33`
  - Data Integrity: `#33D1FF`

## PlayerData ScriptableObject에 연결

`Assets/_Project/Scripts/Data/PlayerData.cs`에 추가된 필드:
- `Sprite portrait` — 캐릭터 풀바디 스프라이트
- `Sprite roleIcon` — 직업 배지 아이콘

기존 4개 PlayerData 에셋(`PlayerData_Security`, `PlayerData_Researcher`, `PlayerData_NetworkAdmin`, `PlayerData_FacilityManager`)을 선택해 Inspector에서:
- `Portrait` 슬롯에 해당 `Char_*.png` 드롭
- `Role Icon` 슬롯에 슬라이스된 `Icon_*` 스프라이트 드롭

## 추후 개선 권장사항

- **스프라이트 외곽 다듬기**: 캐릭터 PNG는 모서리에 미세한 안티앨리어싱 잔여 픽셀이 있을 수 있음. 필요 시 Photoshop/GIMP로 알파 매트 정리.
- **3D 모델로 전환 시**: Meshy AI에 캐릭터 PNG를 올려 FBX 추출 → Unity Import.
- **Role 아이콘 분리본**: 시트 슬라이스 대신 5개 개별 PNG가 필요하면 요청.
- **상태별 포트레이트**: 평상/의심/공포 등 표정 변형이 필요하면 edit으로 변형 생성 가능.