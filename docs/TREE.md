# 프로젝트 폴더 구조

작성 기준: 2026-10-01  
프로젝트 루트: `C:\project\minseok5408\practice_puzzle_game`  
작업 순서: [TODO.md](TODO.md)

**현재 구현한 구조와 앞으로 사용할 목표 구조를 분리**했다. 2026-10-01에 보드·드래그·연쇄와 점수 목표·결과 UI, 매치 그룹·특수 블록 파일을 추가했다. 나머지는 TODO의 해당 단계에서 실제로 생성한다. 파일·폴더 이름은 영어로 작성하고 설명은 한국어로 유지한다.

## 1. 현재 확인한 구조

50개 스테이지·로드맵·진행 저장 구현 후 상태다. 각 에셋의 `.meta`와 Unity 자동 생성 폴더는 생략했다.

```text
practice_puzzle_game/
├─ AGENTS.md
├─ README.md
├─ .gitignore
├─ .gitattributes
├─ .git/                                 # GitHub origin 연결, dev·test·master 브랜치
├─ Assets/
│  ├─ _Project/
│  │  ├─ Scenes/
│  │  │  ├─ Boot.unity                  # 시작 로딩 후 WorldMap 진입
│  │  │  ├─ WorldMap.unity              # 5개 월드·클리어 순서대로 해제
│  │  │  ├─ Game.unity
│  │  │  └─ Sandbox/SetupCheck.unity
│  │  ├─ Scripts/
│  │  │  ├─ Core/
│  │  │  │  ├─ PuzzleGame.Core.asmdef
│  │  │  │  ├─ Board/
│  │  │  │  │  ├─ GridPosition.cs
│  │  │  │  │  ├─ PieceColor.cs
│  │  │  │  │  ├─ SpecialPieceType.cs
│  │  │  │  │  ├─ PieceState.cs
│  │  │  │  │  ├─ CellState.cs
│  │  │  │  │  ├─ BoardState.cs
│  │  │  │  │  ├─ BoardGenerator.cs
│  │  │  │  │  ├─ MatchFinder.cs
│  │  │  │  │  ├─ MatchResult.cs
│  │  │  │  │  ├─ SpecialPieceRules.cs
│  │  │  │  │  ├─ SpecialEffectResolver.cs
│  │  │  │  │  ├─ MoveFinder.cs
│  │  │  │  │  ├─ BoardResolver.cs
│  │  │  │  │  ├─ ResolutionStep.cs
│  │  │  │  │  └─ BoardShuffler.cs
│  │  │  │  └─ Levels/
│  │  │  │     ├─ LevelRules.cs
│  │  │  │     └─ LevelProgress.cs
│  │  │  └─ Runtime/
│  │  │     ├─ PuzzleGame.Runtime.asmdef
│  │  │     ├─ Board/
│  │  │     │  ├─ BoardController.cs
│  │  │     │  ├─ BoardView.cs
│  │  │     │  ├─ BoardEffects.cs          # 타격 시간·빛 이동·폭발 입자 풀
│  │  │     │  ├─ PieceView.cs
│  │  │     │  ├─ CellView.cs
│  │  │     │  └─ BoardInput.cs
│  │  │     ├─ Config/PieceCatalog.cs
│  │  │     ├─ Startup/
│  │  │     │  ├─ StartupDataLoader.cs    # 실제 사용자 정보 로더 연결 계약
│  │  │     │  ├─ PrototypeStartupLoader.cs # 현재 약 5초 임시 로딩
│  │  │     │  ├─ StartupLoadingScreen.cs # 완료/실패/취소·게임 진입
│  │  │     │  └─ LoadingScreenView.cs    # 그림·진행 바·문구·화면 비율
│  │  │     ├─ Levels/
│  │  │     │  ├─ LevelDefinition.cs
│  │  │     │  ├─ LevelCatalog.cs        # 50개 레벨·5개 배경 참조
│  │  │     │  ├─ CampaignProgress.cs    # progress.json·백업·검증
│  │  │     │  ├─ CampaignState.cs       # 씬 사이에 유지하는 진행도
│  │  │     │  └─ LevelSession.cs
│  │  │     ├─ UI/
│  │  │     │  ├─ HUDView.cs
│  │  │     │  ├─ CampaignHUD.cs         # 월드 배경·수집 목표·맵 복귀
│  │  │     │  ├─ WorldMapView.cs        # 경로·완료/잠김·스테이지 선택
│  │  │     │  ├─ ResultPopup.cs
│  │  │     │  ├─ SettingsPopup.cs        # 설정 버튼/ESC·화면 적용·일시정지·종료
│  │  │     │  ├─ DisplaySettings.cs      # 해상도 목록·settings.json 저장/복구
│  │  │     │  ├─ CandyLayout.cs
│  │  │     │  └─ CandyButtonMotion.cs
│  │  │     └─ Debug/
│  │  │        ├─ SetupCheck.cs
│  │  │        ├─ BoardSmokeCheck.cs
│  │  │        ├─ LevelSmokeCheck.cs
│  │  │        ├─ LoadingSmokeCheck.cs    # 시작 시간·진행·화면 비율 검사
│  │  │        ├─ CampaignSmokeCheck.cs   # 실제 클리어·저장·재실행 검사
│  │  │        └─ EffectsSmokeCheck.cs     # 7개 연출의 실제 실행·프레임 캡처
│  │  ├─ Prefabs/Board/
│  │  │  ├─ Piece.prefab
│  │  │  └─ Cell.prefab
│  │  ├─ Input/PuzzleInput.inputactions
│  │  ├─ Data/Pieces/PieceCatalog.asset
│  │  ├─ Data/Levels/LevelCatalog.asset
│  │  ├─ Data/Levels/Definitions/Level_001.asset ~ Level_050.asset
│  │  ├─ UI/Fonts/                       # 각 원본 글꼴과 OFL.txt 동봉
│  │  │  ├─ Jua/Jua-Regular.ttf
│  │  │  ├─ BagelFatOne/BagelFatOne-Regular.ttf
│  │  │  ├─ CandyBody.asset               # Jua TMP 글꼴
│  │  │  ├─ CandyTitle.asset              # Bagel Fat One TMP 글꼴
│  │  │  └─ NanumGothic/, PuzzleUI.asset  # 이전 글꼴 보관
│  │  ├─ UI/Theme/                       # 코드로 생성한 둥근 패널·원·별
│  │  ├─ Art/
│  │  │  ├─ Backgrounds/                # SugarGarden, OrangeOrchard, IceSoda,
│  │  │  │                              # GrapeNightGarden, RainbowPalace
│  │  │  ├─ WorldMaps/                  # 전용 지도 5장·현재 보석 버튼 3장·이전 발판 3장 보관
│  │  │  ├─ Loading/                    # loading_screen_a~d.png 원본 시안
│  │  │  │  └─ loading_screen_a_background.png # 실제 UI용 배경 레이어
│  │  │  ├─ Sprites/Candies/CandyAtlas.png # 3×2 투명 사탕 시트, Unity 다중 스프라이트
│  │  │  ├─ Sprites/Candies/CandyRowAtlas.png     # 가로 크림 줄무늬 6종
│  │  │  ├─ Sprites/Candies/CandyColumnAtlas.png  # 세로 크림 줄무늬 6종
│  │  │  ├─ Sprites/Candies/CandyWrappedAtlas.png # 봉지 사탕 6종
│  │  │  ├─ Sprites/Candies/CandyRainbow.png      # 색 제거 무지개 사탕
│  │  │  ├─ Sprites/Pieces/               # 이전 도형 에셋 보관
│  │  │  ├─ Sprites/Board/Cell.png
│  │  │  └─ Materials/                   # 도형별 재질과 셀 재질
│  │  ├─ Editor/
│  │  │  ├─ PuzzleGame.Editor.asmdef
│  │  │  └─ Build/
│  │  │     ├─ BuildCommands.cs
│  │  │     ├─ StageSetup.cs
│  │  │     ├─ CandyThemeSetup.cs
│  │  │     ├─ SpecialCandySetup.cs
│  │  │     ├─ CandySpriteAlignment.cs     # 투명 여백을 제외한 그림 중심 보정
│  │  │     ├─ LoadingSetup.cs             # Boot 씬·로딩 UI·빌드 순서 생성
│  │  │     ├─ CampaignSetup.cs            # 50개 레벨·로드맵·게임 UI 생성
│  │  │     ├─ WorldMapSetup.cs            # 일러스트 길의 좌표·메달·지도 UI
│  │  │     ├─ CampaignValidation.cs       # 50개 초기 보드·클리어 경로 검사
│  │  │     └─ SettingsSetup.cs            # 기존 Game 씬에 설정 UI 추가
│  │  └─ Tests/
│  │     ├─ EditMode/
│  │     │  ├─ PuzzleGame.EditModeTests.asmdef
│  │     │  ├─ BoardGeneratorTests.cs
│  │     │  ├─ MatchFinderTests.cs
│  │     │  ├─ MoveFinderTests.cs
│  │     │  ├─ BoardResolverTests.cs
│  │     │  ├─ SpecialPieceRulesTests.cs
│  │     │  └─ LevelProgressTests.cs
│  │     └─ PlayMode/
│  │        ├─ PuzzleGame.PlayModeTests.asmdef
│  │        ├─ BoardInteractionTests.cs
│  │        ├─ BoardDragTests.cs
│  │        ├─ LevelSessionTests.cs
│  │        ├─ StartupLoadingTests.cs     # 5초/빠른 완료/늦은 완료·실패·취소
│  │        └─ DisplaySettingsTests.cs     # 해상도 목록·저장·손상 복구
│  ├─ TextMesh Pro/                      # Unity 번들 TMP Essential Resources
│  ├─ Settings/                          # 기존 URP·입력 설정 유지
│  ├─ Scenes/                            # 이동 후 빈 기존 폴더
│  └─ Scripts/                           # 이동 후 빈 기존 폴더
├─ Packages/
├─ ProjectSettings/
├─ docs/
│  ├─ installation.md
│  ├─ integration.md
│  ├─ TODO.md
│  ├─ TREE.md
│  ├─ testing.md
│  ├─ asset-licenses.md
│  ├─ loading-screen.md                  # 화면 교체·사용자 정보 로더 연결
│  ├─ loading-art-prompts.md             # a~d 시안·실제 배경 생성 프롬프트
│  └─ generated-assets.md
└─ Builds/                               # 로컬 출력, Git 제외
   ├─ Windows/0.9.4/practice_puzzle_game.exe # 현재 Windows 개발 빌드, 이전 버전도 보관
   ├─ Preview/Loading/                  # 로딩 초기·진행·16:9/16:10/세로 화면
   ├─ Preview/Board.png                  # 이전 보드
   ├─ Preview/Candy/                     # 16:10·16:9·세로 화면 및 결과창
   ├─ Preview/Specials/                  # 특수 표식·색 제거 교환·결과창
   ├─ Preview/SpecialArt/                # 새 특수 캔디 19종·색 제거 교환·결과창
   └─ Validation/
```

드래그 선택·교환, 실패 복귀, 일반 매치 제거·낙하·보충·연쇄와 이동 불가 셔플까지 연결했다. 점수·색상 수집 목표·성공/실패·재시작·특수 블록 생성/발동/조합·50개 스테이지·로드맵·진행 저장을 구현했다. 장애물·튜토리얼·자동 힌트·소리는 미구현이다. `CellState`의 확장용 데이터가 있어도 장애물 게임 기능이 구현된 것은 아니다. 남은 작업은 [TODO.md](TODO.md)의 0.9.4 기준 요약을 따른다.

`MatchResult`는 연속 구간·교차 그룹·좌표를 보관한다. `SpecialPieceRules`는 생성 종류·위치와 특수 교환 여부를 결정하고, `SpecialEffectResolver`는 생성 칸을 보호하며 효과 큐로 중복 발동을 막는다. `ResolutionStep`에 생성·발동 기록을 추가했고 `BoardResolver`는 첫 해결 단계에만 교환 좌표를 받는다. `PieceCatalog`는 색과 특수 종류를 함께 조회하며 `PieceView`가 전용 캔디 그림으로 교체한다. `SpecialCandySetup`은 특수 스프라이트 19종·재질 4개를 가져오고 카탈로그에 연결한다. `SpecialPieceView`와 벡터 표식은 0.5.1에서 제거했다. 새 특수 블록도 같은 ID를 유지하고 실제 제거된 ID만 채점한다.

`BoardSmokeCheck.cs`는 에디터·개발 빌드에서 명시적인 `-puzzleSmokeTest` 인자를 받았을 때만 가상 마우스로 드래그 1회와 보드 안정화를 확인한다. `BuildCommands.cs`와 PlayMode 테스트는 초기 빌드·화면 검증을 위해 계획보다 먼저 추가했다.

`LevelRules`·`LevelProgress`가 점수·색상별 수집량·이동수·결과를 계산한다. `LevelSession`은 이 상태를 UI에 알리고 재시작·다음 레벨·클리어 저장을 연결한다. `BoardController`는 유효 교환 시작·제거 단계·연쇄 완료·중단을 전달한다. `HUDView`와 `ResultPopup`은 Game 씬의 LevelCanvas에 연결했다. `LevelDefinition`에는 번호·고정 ID·seed·이동수·점수/수집 목표가 있고, 고정 초기 배치·장애물 데이터·격자 편집기는 이후 확장한다.

`LevelSmokeCheck.cs`는 명시적 `-puzzleLevelSmokeTest` 인자로 성공·실패·실제 재시작 버튼을 검사한다. `-puzzleSpecialSmokeTest`는 특수 표식 캡처와 색 제거 드래그 검사도 추가한다. 두 진단은 에디터·개발 빌드에만 포함된다. `StageSetup.cs`는 첫 UI·폰트·레벨 데이터 연결용 편집기 도구다. 한글 폰트와 TMP 번들 리소스의 출처는 [asset-licenses.md](asset-licenses.md)에 기록한다.

사탕 테마는 `CandyThemeSetup`에서 스프라이트 가져오기·프리팹·씬 UI를 연결한다. `CandyLayout`은 보드 프레임의 실제 화면 좌표에 점수판 높이를 맞추며, 큰 영문 제목과 아이템용 빈 슬롯 4개를 점수판 안에 배치한다. 슬롯은 씬의 `LevelCanvas/ScoreCard/ItemSlots/ItemSlot1~4`에 있으며 아직 아이템 스크립트·클릭 동작은 없다. 창 비율에 맞춰 패널·보드·배경을 조정한다. `CandyButtonMotion`은 버튼 호버·눌림 효과다. 기존 `PieceCatalog`를 통해 그림을 교체했으며 Core 게임 규칙은 변경하지 않았다.

## 2. 목표 루트 구조

`[유지]`는 현재 경로를 유지하고, `[예정]`은 필요 단계에서 만든다. `[선택]`은 기능을 채택할 때만 만든다. 트리의 대표 파일명은 설계 기준이며 에셋 수는 이후 늘어난다.

```text
practice_puzzle_game/
├─ Assets/
│  ├─ _Project/                          # [구현 시작] 직접 만드는 게임의 코드와 에셋
│  ├─ Settings/                          # [유지] 템플릿의 URP·렌더러·입력 설정
│  └─ ThirdParty/                        # [선택] 외부 에셋의 원본·라이선스
├─ Packages/                             # [유지] 의존 패키지와 확정 버전
├─ ProjectSettings/                      # [유지] Unity 프로젝트 설정
├─ docs/                                 # [유지] 진행·구조·설치 문서
│  ├─ installation.md
│  ├─ integration.md
│  ├─ TODO.md
│  ├─ TREE.md
│  ├─ game-design.md                     # [예정] 최종 테마·규칙·범위
│  ├─ testing.md                         # [생성] OS별 검사 결과와 버그 재현
│  ├─ release.md                         # [예정] 빌드·Steam·출시·롤백 절차
│  ├─ asset-licenses.md                  # [예정] 그림·음원·폰트 출처와 조건
│  └─ CHANGELOG.md                       # [예정] 버전별 사용자 변경 사항
├─ ArtSource/                            # [예정] 편집 가능한 그림·음원 원본
│  ├─ Graphics/
│  └─ Audio/
├─ Marketing/                            # [예정] 상점·홍보 자료
│  └─ Steam/
│     ├─ Capsules/
│     ├─ Screenshots/
│     ├─ Trailers/
│     └─ StoreText/
├─ Tools/                                # [예정] Unity 밖에서 쓰는 배포 도구 설정
│  └─ Steam/
│     ├─ app_build.vdf
│     ├─ depot_windows.vdf
│     ├─ depot_macos.vdf
│     └─ README.md
├─ Builds/                               # [예정·Git 제외] 배포용 출력
│  ├─ Windows/<version>/
│  └─ macOS/<version>/
├─ .gitignore                            # [생성] Unity 자동 생성 파일 제외
├─ .gitattributes                        # [생성] 줄바꿈 규칙, 필요 시 LFS
└─ README.md                             # [생성] 프로젝트 소개·여는 법·문서 링크
```

`Library`·`Temp` 등 자동 생성 폴더는 실제로 계속 존재할 수 있지만 목표 트리에서는 생략했다. 외부 패키지가 지정 경로에 설치되는 경우 억지로 ThirdParty로 옮기지 않는다. Package Manager 패키지는 `Packages`에서 관리한다.

## 3. 게임 에셋 구조: Assets/_Project

```text
_Project/
├─ Scenes/
│  ├─ Boot.unity                         # 공통 초기화, 첫 실행 씬
│  ├─ MainMenu.unity                     # 시작·설정·크레딧·종료
│  ├─ LevelSelect.unity                  # 스테이지 선택·해금 상태
│  ├─ Game.unity                         # 보드와 플레이 HUD
│  └─ Sandbox/
│     └─ SetupCheck.unity                # 기존 SampleScene을 옮긴 테스트 씬
├─ Scripts/                              # 자세한 트리는 다음 절
├─ Prefabs/
│  ├─ Board/
│  │  ├─ Board.prefab                    # 보드 루트와 표현 컴포넌트
│  │  ├─ Cell.prefab                     # 고정된 바닥·장애물 표시
│  │  └─ Piece.prefab                    # 움직이는 일반·특수 블록
│  ├─ UI/
│  │  ├─ HUD.prefab
│  │  ├─ LevelButton.prefab
│  │  ├─ PausePopup.prefab
│  │  ├─ ResultPopup.prefab
│  │  ├─ SettingsPopup.prefab
│  │  └─ ConfirmPopup.prefab
│  └─ VFX/
│     ├─ MatchEffect.prefab
│     ├─ SpecialEffect.prefab
│     └─ ClearEffect.prefab
├─ Data/                                 # Inspector에서 편집하는 제작 데이터
│  ├─ Config/GameConfig.asset
│  ├─ Pieces/PieceCatalog.asset
│  ├─ Levels/
│  │  ├─ LevelCatalog.asset
│  │  └─ Definitions/
│  │     ├─ Level_001.asset
│  │     ├─ Level_002.asset
│  │     └─ ...                          # 출시 개수만큼 추가
│  ├─ Audio/AudioCatalog.asset
│  └─ Localization/
│     ├─ Korean.json                    # 문자열 키와 번역의 배열
│     └─ English.json                   # 영어 지원 단계에서 추가
├─ Art/
│  ├─ Sprites/
│  │  ├─ Pieces/                        # 색·기호·특수 블록
│  │  ├─ Board/                         # 바닥·Frost 장애물·테두리
│  │  ├─ UI/                            # 버튼·아이콘·팝업
│  │  └─ Backgrounds/
│  ├─ Atlases/
│  ├─ Materials/
│  ├─ Animations/
│  └─ VFX/
├─ Audio/
│  ├─ Music/
│  ├─ SFX/
│  └─ Mixers/MainMixer.mixer
├─ UI/
│  ├─ Fonts/                            # 원본 글꼴·TMP Font Asset·fallback
│  └─ Theme/                            # 재사용할 색·폰트 등 UI 스타일 에셋
├─ Input/
│  └─ PuzzleInput.inputactions          # 게임용 입력 액션
├─ Settings/
│  └─ BuildProfiles/
│     ├─ Windows.asset                  # Unity UI에서 생성할 빌드 프로필
│     └─ macOS.asset
├─ Editor/                               # 플레이어에 포함하지 않을 제작 도구
│  ├─ PuzzleGame.Editor.asmdef
│  ├─ Levels/
│  │  ├─ LevelEditorWindow.cs
│  │  └─ LevelDefinitionValidator.cs
│  └─ Build/
│     └─ BuildCommands.cs               # 반복 빌드가 필요할 때 추가
└─ Tests/
   ├─ EditMode/
   │  ├─ PuzzleGame.EditModeTests.asmdef
   │  ├─ Fixtures/BoardFixtures.cs
   │  ├─ BoardGeneratorTests.cs
   │  ├─ MatchFinderTests.cs
   │  ├─ BoardResolverTests.cs
   │  ├─ MoveFinderTests.cs
   │  ├─ SpecialPieceRulesTests.cs
   │  ├─ ObjectiveTrackerTests.cs
   │  ├─ LevelValidationTests.cs
   │  └─ SaveServiceTests.cs
   └─ PlayMode/
      ├─ PuzzleGame.PlayModeTests.asmdef
      ├─ BoardInteractionTests.cs
      └─ SceneFlowTests.cs
```

`Assets/Settings`는 기존 렌더링 설정을 유지하는 곳이다. `Assets/_Project/Settings`에는 앞으로 만드는 게임 전용 설정만 넣는다. 두 경로의 목적을 구분한다.

`Data` 에셋은 씬·프리팹의 직렬화 필드에서 참조한다. 폴더에 파일을 넣었다고 실행 파일에 자동으로 연결되는 것은 아니다. 초기에는 Resources·Addressables를 추가하지 않고 명시적 참조로 연결한다.

## 4. 코드 구조: Scripts/Core

게임의 규칙과 상태를 담당한다. `MonoBehaviour`, `GameObject`, `Transform`, `UnityEditor`를 사용하지 않는다. UI·소리·디스크 저장도 여기에서 실행하지 않는다.

```text
Scripts/Core/
├─ PuzzleGame.Core.asmdef
├─ Board/
│  ├─ GridPosition.cs                    # x·y 좌표, 인접 여부와 값 비교
│  ├─ PieceColor.cs                      # 일반 6종·None(색 제거용)의 enum
│  ├─ SpecialPieceType.cs                # None·Row·Column·Bomb·ColorClear
│  ├─ PieceState.cs                     # 블록 ID·색·특수 종류
│  ├─ CellState.cs                      # 바닥 종류·내구도
│  ├─ BoardState.cs                     # 크기·셀·블록 배열의 실제 상태
│  ├─ BoardGenerator.cs                 # 초기 매치 없는 생성, seed 처리
│  ├─ MatchFinder.cs                    # 모든 가로·세로 매치 탐색
│  ├─ MatchResult.cs                    # 매치 그룹·교차·중복 제거 좌표
│  ├─ MoveFinder.cs                     # 규칙에 맞는 가능한 교환 탐색
│  ├─ BoardResolver.cs                  # 교환·복귀·제거·낙하·보충 조율
│  ├─ ResolutionStep.cs                 # 단계별 제거·이동·생성 결과 데이터
│  ├─ BoardShuffler.cs                  # 블록 셔플, 바닥과 진행도 유지
│  ├─ SpecialPieceRules.cs              # 생성 위치·우선순위·조합 규칙
│  └─ SpecialEffectResolver.cs          # 특수 효과 큐와 중복 발동 방지
└─ Levels/
   ├─ LevelRules.cs                     # 제작 데이터를 변환한 시작 규칙
   ├─ LevelProgress.cs                  # 현재 점수·이동수·결과
   ├─ ObjectiveDefinition.cs            # 점수·색 수집·장애물 목표 값
   └─ ObjectiveTracker.cs               # 제거 결과로 목표 진행도 계산
```

1. 보드 데이터가 정답이며 화면의 Transform 위치로 매치를 판정하지 않는다.
2. 난수는 생성·보충·셔플에 같은 실행용 난수원을 전달한다. 버그 기록에는 seed뿐 아니라 버전과 입력 순서도 남긴다.
3. BoardResolver는 단계마다 결과 데이터를 반환한다. Runtime이 그 단계를 화면에 재생하고 다음 단계를 요청한다.
4. MoveFinder와 실제 교환 검증은 같은 특수 교환 규칙을 사용한다.
5. 움직이는 PieceState의 ID와 고정된 셀 좌표를 구분한다. 재사용한 화면 객체의 ID는 새 데이터와 다시 연결한다.
6. 처음에는 BoardResolver 안에서 제거·낙하·보충을 메서드로 나눈다. 코드가 커졌을 때만 별도 파일로 분리한다.
7. 줄 제거·주변 폭발은 색을 유지해 일반 매치에 참여한다. 색 제거는 `PieceColor.None`으로 일반 매치에서 제외한다. 빈칸은 블록 없음으로 표현하며 색 없음과 구분한다.

## 5. 코드 구조: Scripts/Runtime

Unity의 입력·표현과 Core를 연결한다. 범용 서비스 프레임워크나 상태별 클래스를 미리 만들지 않는다.

```text
Scripts/Runtime/
├─ PuzzleGame.Runtime.asmdef
├─ Bootstrap/
│  ├─ GameBootstrap.cs                  # 설정·저장·공통 서비스 초기화
│  └─ SceneFlow.cs                      # 씬 이동, 선택한 레벨 전달
├─ Board/
│  ├─ BoardController.cs                # 입력 허용·진행 상태·코루틴 조율
│  ├─ BoardView.cs                      # 보드 생성·좌표 배치·화면 갱신
│  ├─ PieceView.cs                      # 스프라이트·선택·이동·제거 표현
│  ├─ CellView.cs                       # 고정 바닥·Frost 상태 표현
│  └─ BoardInput.cs                     # 클릭·드래그→보드 좌표, UI 입력 차단
├─ Levels/
│  ├─ LevelDefinition.cs                # 레벨 ScriptableObject 타입
│  ├─ LevelCatalog.cs                   # 순서·ID 조회용 ScriptableObject 타입
│  ├─ LevelLoader.cs                    # 에셋을 Core 실행 데이터로 복사·변환
│  └─ LevelSession.cs                   # 시작·성공·실패·기록 반영
├─ Config/
│  ├─ GameConfig.cs                     # 공통 속도·점수·힌트 등 설정 타입
│  └─ PieceCatalog.cs                   # 블록 종류와 Sprite·Material·색 매핑
├─ UI/
│  ├─ MainMenuView.cs
│  ├─ LevelSelectView.cs
│  ├─ LevelButtonView.cs
│  ├─ HUDView.cs
│  ├─ PausePopup.cs
│  ├─ ResultPopup.cs
│  ├─ SettingsPopup.cs
│  ├─ ConfirmPopup.cs
│  ├─ TutorialPresenter.cs              # 레벨별 첫 조작 안내
│  └─ LocalizedText.cs                  # 문자열 키로 UI 문구 갱신
├─ Services/
│  ├─ Save/
│  │  ├─ SaveData.cs                    # 직렬화 가능한 진행도·버전 DTO
│  │  ├─ SettingsData.cs                # 장치별 설정 DTO
│  │  ├─ SaveService.cs                 # JSON 읽기·쓰기·백업·오류 처리
│  │  └─ SaveMigration.cs               # 저장 형식 변경 시 변환
│  ├─ Audio/
│  │  ├─ AudioCatalog.cs                # 효과음 ID·클립 매핑 에셋 타입
│  │  └─ AudioService.cs                # 재생·볼륨·동시 효과음 수 제어
│  └─ Localization/
│     ├─ LocalizationTable.cs           # JSON 항목 배열의 직렬화 타입
│     └─ LocalizationService.cs         # 언어 선택·키 조회·누락 fallback
└─ Debug/
   ├─ SetupCheck.cs                     # 기존 스크립트 이동 완료
   └─ BoardSmokeCheck.cs                # CLI 인자로 실행하는 개발 빌드 진단
```

| 담당 | 하는 일 | 맡기지 않을 일 |
| --- | --- | --- |
| BoardController | Core 호출, 처리 상태, 입력 잠금과 연출 순서 | 직접 점수·매치 규칙을 다시 구현 |
| BoardView·PieceView·CellView | 전달된 상태·단계의 표시 | 보드의 정답 상태를 독자적으로 변경 |
| LevelSession | 시작·종료, 결과 1회 확정, 저장 요청 | 블록 애니메이션 |
| LevelLoader | 제작 데이터→실행 데이터 변환 | 원본 LevelDefinition을 플레이 중 수정 |
| UI 스크립트 | 표시 갱신, 버튼 입력 전달 | 저장 파일 직접 수정·매치 판정 |
| SaveService | OS 경로·파일·JSON·복구 | Unity 씬이나 보드 객체 통째로 직렬화 |
| GameBootstrap | 공통 서비스 1회 초기화와 수명 관리 | 모든 게임 기능을 한 클래스에 구현 |

현재 보드 입력은 `BoardInput`의 Point·Press·Cancel 액션으로 처리하며, Game 씬의 BoardRoot에 입력 에셋이 연결되어 있다. 원본 InputActionAsset을 복제해 활성화하고 비활성화 시 정리한다. `SettingsPopup`은 ESC 액션을 별도로 소유하며 보드 입력보다 먼저 설정 열기/닫기를 처리한다. 설정 중 보드 입력과 진행 시간을 멈추되 BoardController를 비활성화하지 않는다. PlayMode 입력 검사는 `InputTestFixture`로 실제 장치와 격리한다.

BoardController의 초기 상태는 `Idle / Swapping / Resolving / Shuffling / Finished`로 충분하다. 일시정지는 별도 상태값으로 관리하고, 메뉴를 닫은 뒤 기존 처리 단계가 안전하게 이어지게 한다.

## 6. 코드 의존성과 실행 흐름

### 어셈블리 참조

```text
Runtime ──────────────→ Core
Editor ───────────────→ Runtime, Core
EditModeTests ────────→ Core, Runtime, Editor(레벨 검증 테스트 시)
PlayModeTests ────────→ Runtime, Core
```

1. `PuzzleGame.Core`: No Engine References를 사용하며 자체 규칙만 포함한다.
2. `PuzzleGame.Runtime`: Core와 사용하는 패키지 어셈블리를 명시적으로 참조한다. 예: Input System, uGUI, TextMeshPro.
3. `PuzzleGame.Editor`: Include Platforms를 Editor로 제한하고 UnityEditor 코드는 여기 둔다.
4. 테스트 어셈블리는 Unity Test Framework의 테스트 설정으로 생성한다. NUnit과 테스트 코드를 일반 출시 런타임에 넣지 않는다.
5. Core가 Runtime을 참조하거나 Runtime이 Editor·Tests를 참조하지 않게 한다.
6. asmdef는 역할별 이 정도만 둔다. 작은 기능마다 새 어셈블리를 만들지 않는다.

어셈블리는 코드의 컴파일·참조 범위를 나누는 Unity 설정이다. [Unity 공식 설명](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-intro.html)

### 플레이 실행 순서

```text
GameBootstrap
  → 저장·설정·데이터 준비
  → SceneFlow로 Game 진입
  → LevelLoader가 제작 데이터를 실행 상태로 변환
  → BoardController가 입력을 받음
  → Core가 교환·매치·제거·낙하·보충을 계산
  → BoardView가 ResolutionStep을 재생
  → ObjectiveTracker·LevelProgress 갱신
  → 안정화 후 LevelSession이 성공/실패를 확정
  → HUD·결과 팝업 표시, SaveService로 진행 저장
```

서비스 전달은 Inspector 참조나 명시적인 초기화 메서드를 사용한다. GameObject 검색, 전역 static 값, 거대한 GameManager에 모든 기능을 모으지 않는다.

## 7. 씬과 프리팹 연결 기준

| 씬 | 주요 루트 오브젝트 | 배포 포함 |
| --- | --- | --- |
| Boot | AppRoot(GameBootstrap·SceneFlow·공통 서비스) | 포함, 시작 씬 |
| MainMenu | Camera, Canvas, EventSystem | 포함 |
| LevelSelect | Camera, Canvas, EventSystem | 포함 |
| Game | Camera, Global Light 2D, LevelSession, BoardRoot, Canvas, EventSystem | 포함 |
| Sandbox/SetupCheck | 기존 설치 확인용 카메라·조명·SetupCheck | 제외 |

1. AppRoot만 필요에 따라 DontDestroyOnLoad로 유지하고 중복 생성을 차단한다.
2. 씬은 단일 교체 방식으로 시작한다. 처음부터 복잡한 Additive 씬 구성을 도입하지 않는다.
3. Game 씬에 보드 프리팹·카탈로그·HUD를 연결한다. 프리팹 안에서 별도 씬 오브젝트를 직접 참조하지 않는다.
4. UI EventSystem은 활성 씬에 한 개를 유지하고 Input System용 UI 입력 모듈을 사용한다.
5. 보드 셀 좌표와 화면 단위를 BoardView 한 곳에서 변환한다.
6. ScriptableObject 에셋은 설정 원본이다. 실행 중 상태는 Core 객체와 SaveData에 둔다.
7. 게임 완성 후에도 설치 확인 씬은 Sandbox에 보관하되 빌드 씬 목록에서 제외한다.

## 8. 제작 데이터와 사용자 저장 구분

| 종류 | 위치 | 예시 | 수정 주체 |
| --- | --- | --- | --- |
| 게임 규칙 기본값 | `Assets/_Project/Data/Config` | 애니메이션 시간·점수 | 개발자 |
| 레벨 제작 데이터 | `Assets/_Project/Data/Levels` | 목표·이동수·초기 배치 | 개발자 |
| 블록 표현 | `Assets/_Project/Data/Pieces` | 색·특수 종류→Sprite | 개발자 |
| 사용자 진행도 | `Application.persistentDataPath` | progress.json | 게임 |
| 사용자 설정 | `Application.persistentDataPath` | settings.json | 게임 |
| 저장 복구본 | 동일한 사용자 저장 폴더 | progress.json.bak | 게임 |

```text
Application.persistentDataPath/          # 프로젝트 안의 폴더가 아님
├─ progress.json
├─ progress.json.bak
├─ settings.json
└─ settings.json.bak
```

1. Windows와 macOS의 실제 경로를 직접 붙이지 않고 API로 얻는다.
2. 현재 진행도 v2는 고정 번호 1~50과 배열을 사용한다. 번호와 `level_001`~`level_050` ID의 대응은 변경하지 않는다. 레벨 재정렬·추가 시 버전 변경과 마이그레이션이 필요하다.
3. JsonUtility를 사용할 경우 Dictionary·다차원 배열을 그대로 저장하지 않고 직렬화 가능한 항목 배열/리스트로 변환한다.
4. 플레이 중 보드는 첫 출시 저장 대상에서 제외하고 완료 기록을 저장한다.
5. Cloud 채택 시 진행도만 우선 동기화한다. 설정·백업·임시 파일은 의도 없이 함께 동기화하지 않는다.
6. 개인 저장 파일은 Git·Steam depot에 포함하지 않는다.

실제 경로 규칙: [Unity persistentDataPath 문서](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/application/persistentdatapath).

## 9. Steam 기능을 추가할 때의 선택 구조

기본 게임은 Steam API 없이 동작한다. 업적 등 실제 API 기능을 도입할 때만 아래 구조를 추가한다. Auto-Cloud만 사용한다면 게임 내부 API 어댑터가 필요하지 않을 수 있다.

```text
Assets/
├─ _Project/
│  └─ Integrations/
│     └─ Steam/
│        ├─ PuzzleGame.Steam.asmdef
│        ├─ SteamBootstrap.cs
│        └─ SteamAchievements.cs
└─ ThirdParty/
   └─ Steamworks/                        # 선택한 배포 방식이 Assets 설치일 때
```

1. Steam 어셈블리만 선택한 C# 래퍼와 Runtime을 참조한다. Core·Runtime에서 Steam 어셈블리를 직접 참조하지 않는다.
2. SteamAchievements는 게임 결과 이벤트를 구독하고 Steam API에 전달한다.
3. SDK가 없는 기본 프로젝트도 컴파일되도록 통합 파일과 참조를 함께 추가·제거하거나 일관된 조건부 컴파일을 적용한다.
4. 네이티브 라이브러리는 래퍼가 요구하는 위치에 설치하고 OS·CPU Import Settings를 검사한다.
5. 개발용 AppID 파일·로그인 정보·인증서 개인키를 출시 결과물에 잘못 넣지 않는다.
6. SteamPipe 업로드 SDK 전체는 개발 장치에 별도 보관한다. `Tools/Steam`에는 재현 가능한 설정과 설명만 둔다.

## 10. 기존 파일 이동 기록

| 기존 위치 | 변경 위치 | 상태 |
| --- | --- | --- |
| `Assets/Scenes/SampleScene.unity` | `Assets/_Project/Scenes/Sandbox/SetupCheck.unity` | 완료, GUID 유지 |
| `Assets/Scripts/SetupCheck.cs` | `Assets/_Project/Scripts/Runtime/Debug/SetupCheck.cs` | 완료, GUID 유지 |
| `Assets/Settings/*` | 현재 위치 유지 | 이동 없음 |
| `Assets/Settings/InputSystem_Actions.inputactions` | 원본 유지, `_Project/Input/PuzzleInput.inputactions` 별도 생성 | 완료 |

1. 이동 전 실행 모드를 종료하고 씬을 저장한다.
2. 이동·이름 변경은 Unity 프로젝트 창에서 한다. 외부에서 옮기면 에셋과 `.meta`를 반드시 함께 옮긴다.
3. SetupCheck 클래스명은 유지하고 스크립트 연결·카메라·URP 참조를 확인한다.
4. Build Profiles의 씬 목록을 새 경로와 목적에 맞게 확인한다.
5. 옛 Scenes·Scripts 폴더는 다른 에셋이 없는지 확인한 뒤 정리한다.
6. 원본 입력 에셋을 재사용하거나 복사할 때 기존 씬 참조를 검사한다. 동일 입력에 이중 구독하지 않는다.

에셋 GUID가 들어 있는 `.meta`는 참조 유지에 필요하다. [Unity 에셋 메타데이터](https://docs.unity3d.com/6000.3/Documentation/Manual/AssetMetadata.html)

## 11. Git 보관과 제외 기준

| 대상 | 처리 |
| --- | --- |
| Assets와 그 안의 .meta | 보관 |
| Packages/manifest.json, packages-lock.json | 보관, 두 OS에서 같은 패키지 버전 사용 |
| ProjectSettings | 보관 |
| docs, README, Tools의 비밀정보 없는 설정 | 보관 |
| ArtSource, Marketing | 용량·권리에 따라 Git/LFS 또는 별도 백업, 유일한 원본 방치 금지 |
| Library, Temp, Logs, obj, UserSettings | 제외 |
| Builds, 빌드 압축 파일, Steam 업로드 캐시 | 제외 |
| .csproj, .sln, .slnx, IDE 캐시 | 제외 |
| .vscode | 필요한 공통 설정만 선택 보관, 개인 절대 경로 제외 |
| 비밀번호, 로그인 토큰, 개인 저장, 인증서 개인키 | 제외 |

1. 소스 백업과 출시 결과물 보관은 구분한다. 제외한 빌드도 출시 버전별 별도 보관이 필요하다.
2. 대용량 파일을 LFS로 관리한다면 Windows·Mac 양쪽에서 동일한 설정을 사용한다.
3. 텍스트 파일은 UTF-8로 저장하고 줄바꿈 정책을 맞춘다.
4. 대소문자만 다른 파일·폴더를 만들지 않는다. 경로는 실제 이름과 정확하게 일치시킨다.
5. Packages·ProjectSettings·.meta의 충돌을 무조건 한쪽 파일로 덮지 말고 변경 의도를 확인한다.

## 12. 단계별로 실제 생성할 범위

| TODO 단계 | 생성할 범위 |
| --- | --- |
| 1 | .gitignore, .gitattributes, README, Git 백업 |
| 2 | _Project/Scenes와 빈 Game 씬, Scripts/Core·Runtime, Tests/EditMode, 필요한 asmdef, 기존 테스트 파일 이동 |
| 3 | Settings/BuildProfiles, 두 OS의 Builds 출력 |
| 4~8 | Core/Board·Levels의 기본 파일, Runtime/Board·Levels, 블록 프리팹·임시 그림 |
| 9 | Marketing/Steam, docs/release.md |
| 10~11 | 특수 규칙·효과 처리, 바닥 표시, 관련 테스트 |
| 12 | Data/Levels, LevelDefinition·Catalog·Loader, Editor 제작 도구 |
| 13~14 | Boot·메뉴·선택 씬, UI 프리팹, Save 서비스 |
| 15~16 | Art·Audio·UI·Localization, 레벨 콘텐츠, 라이선스 기록 |
| 17 | PlayMode 테스트, 테스트 기록, 회귀 검사 보완 |
| 18 | 선택한 Steam 기능·컨트롤러 파일만 추가 |
| 19~22 | 최종 Build Profiles, Tools/Steam, 버전별 빌드, 출시·패치 기록 |

1차 폴더부터 만들고 필요한 클래스만 채운다. 목표 트리의 모든 파일을 빈 껍데기로 한꺼번에 생성하지 않는다. 구조나 이름을 바꾸면 이 문서와 TODO의 해당 경로도 함께 갱신한다.
