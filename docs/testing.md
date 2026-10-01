# 사탕 테마·점수 목표·특수 블록 검증

검증일: 2026-10-01  
Unity: 6.3 LTS `6000.3.25f1`  
환경: Windows 11, x64 개발 빌드

## 0.7.0 설정 버튼·ESC·화면 설정·종료

1. 기존 Game 씬의 점수판 아래에 설정 버튼과 크림/보라색 설정창을 추가했다. 해상도 목록·창모드 체크박스·적용·계속하기·게임 종료를 연결했다. ESC는 드래그를 취소하고 설정을 열며, 해상도 목록→설정창 순으로 닫는다.
2. PlayMode **26개 통과**. 기존 20개에 설정 버튼의 실제 마우스 클릭·모달 입력 차단·ESC/드래그 충돌·드롭다운 닫기·연쇄 일시정지/재개·적용 전 선택 취소·이전 timeScale 복원·해상도 목록 중복/모니터 범위·설정 저장/손상 백업 복구 검사를 추가했다. Core는 변경하지 않아 직전 EditMode 61개 통과 기록을 유지했다.
3. 최종 Windows **0.7.0** 실행 파일에서 **960×600 창모드 → 1280×720 창모드 → 1280×800 테두리 없는 전체 화면 → 960×600 창모드** 전환 후 실제 `Screen.width/height/fullScreenMode`와 저장 파일을 확인했다. 설정 중 점수·이동수를 보존하고 폭발 연쇄를 같은 단계부터 재개했다.
4. 별도 프로세스로 재실행해 저장된 960×600 창모드 복원을 확인했다. 두 실행 모두 실제 게임 종료 버튼을 마우스로 눌러 프로세스 **exit code 0**으로 종료했다.
5. 1280×800·800×1000 렌더 캡처로 기본 화면/설정 버튼·설정창·해상도 목록·체크박스·한글 표시를 확인했다. 모니터 설정 검사는 실제 플레이어 API로 수행하며, 세로 캡처는 반응형 UI의 오프스크린 렌더 검사다.
6. 진단은 `-puzzleSettingsPath`로 프로젝트 Builds 아래 별도 JSON을 사용해 플레이어의 실제 설정 파일과 분리했다. 설정은 원자적 파일 교체와 `.bak` 복구를 사용한다. macOS 실기기 검증은 대기다.

빌드: `Builds/Windows/0.7.0/practice_puzzle_game.exe`  
자동 검사: `Builds/Validation/settings/PlayMode.xml`  
최종 실행 결과: `Builds/Validation/settings/windows-smoke-final.txt`, `reload-smoke.txt`  
화면: `Builds/Preview/Settings/Final/`, `Builds/Preview/Settings/Portrait/`  
실행 검사: `-batchmode -puzzleSettingsSmokeTest -puzzleSettingsPath <진단 JSON 절대 경로> -puzzleSmokeReport <결과 절대 경로> -puzzleCaptureFolder <화면 폴더>`  
재실행 검사: 같은 JSON 경로와 `-puzzleSettingsReloadTest` 사용. 이 검사들은 실제 종료 버튼으로 프로세스를 끝낸다.

## 0.6.0 폭발 연출·캔디 중심 보정

1. EditMode **61개 통과**, PlayMode **20개 통과**. 최초 타격·연쇄 타격 ID의 합집합이 실제 제거와 일치하고 새 특수 생성 칸을 제외하는지, 무지개 2개가 전체 보드를 대상으로 삼는지 검사했다. 실행 검사에는 일반 팡 중단 시 보드·점수·이동수·연출 복구, 방향별 가까운 칸 우선 타격, 마지막 연쇄 뒤 정리와 정확한 위치 복귀를 추가했다.
2. Windows **0.6.0** 개발 빌드 성공. 7개 고정 보드에서 일반 3매치·좌우 줄·상하 줄·포장 폭발·무지개 색 제거·5매치 생성·무지개 2개 전체 제거를 실제 실행했다. 모든 경우에 이동수 1회 차감, 점수, 안정된 64개 블록, 화면/모델 일치, 남은 효과 없음 확인.
3. 최종 빌드에서 마우스 드래그·특수 캔디 19종·색 제거 교환·마지막 이동 성공·실패·입력 잠금·실제 다시 하기 버튼 검사를 다시 통과했다.
4. 일반·특수 25종의 알파 32 이상 경계 중심을 pivot으로 저장했다. 일반 초록 `(0.50977, 0.55273)`, 파랑 `(0.49512, 0.54688)`, 보라 `(0.49121, 0.54688)`로 확인했다. 임포터를 dirty로 표시해 재임포트에 값이 실제 저장되도록 했다. 변경 전 일반 캔디 pivot은 모두 `(0.5, 0.5)`였다.
5. 포장 사탕은 사용자가 선택한 첨부 PNG와 프로젝트 PNG의 SHA-256이 일치한다. 투명 배경을 보존했다. 검증 복사본과 원본의 Scripts·Editor·Tests·캔디 PNG/메타·PieceCatalog 파일 해시도 일치한다.
6. 960×600, 30fps 실제 렌더 프레임으로 연출을 확인했다. 첫 캡처 뒤 약한 섬광·파편을 보완하고 최종 빌드로 7개 실행 검사를 반복했다. 프레임을 순서대로 묶은 WebP는 게임에서 캡처한 애니메이션이다.

빌드: `Builds/Windows/0.6.0/practice_puzzle_game.exe`  
자동 검사: `Builds/Validation/effects/EditMode.xml`, `PlayMode.xml`  
최종 실행 결과: `Builds/Validation/effects/effects-smoke-final.txt`, `windows-smoke-final.txt`  
정렬/포장 캡처: `Builds/Preview/Effects/Board/LevelPlaying.png`, `SpecialPieces.png`  
애니메이션: `Builds/Preview/Effects/Final/{Normal,Row,Column,Bomb,Rainbow,FiveMatch,DoubleRainbow}.webp`  
연출 검사 인자: `-batchmode -puzzleEffectsSmokeTest -puzzleSmokeReport <절대 경로> -puzzleCaptureFolder <절대 경로>`  
캡처 폴더 인자를 생략하면 이미지 파일 없이 실행 검사만 수행한다. 일반 실행에는 진단 인자를 사용하지 않는다. 소리는 이번 구현에 포함하지 않았으며 macOS 실기기 검증은 대기다.

## 0.5.1 특수 캔디 전용 그림

1. 내장 image_gen으로 가로 크림 줄무늬 6종·세로 크림 줄무늬 6종·봉지 사탕 6종·무지개 소용돌이 사탕 1종을 새로 그렸다. 기존 일반 캔디는 유지한다. 생성 프롬프트와 파일 경로는 [generated-assets.md](generated-assets.md)에 기록했다.
2. `PieceCatalog`가 색과 특수 종류를 함께 조회하고, 특수 생성 시 `PieceView`의 스프라이트와 재질을 교체한다. `SpecialPieceView`와 LineRenderer 표식은 제거했다.
3. PlayMode **18개 통과**. 생성 후 스프라이트 교체·재질의 텍스처·벡터 표식 없음·실제 색 제거 드래그·중단 복구·결과·재시작을 검사했다. Core 규칙은 변경하지 않아 직전 EditMode 59개 통과 기록을 유지하며 이번 작업에서는 다시 실행하지 않았다.
4. Windows 0.5.1 개발 빌드와 실제 실행 파일 검사를 통과했다. 특수 캔디 19종을 한 보드에 배치해 시각적으로 확인하고 색 제거 드래그·연쇄·점수·성공/실패·재시작을 검사했다.
5. 첫 캡처에서 봉지 사탕 본체가 작아 보여 포장 끝을 줄이고 본체를 크게 다시 그렸다. RGBA 알파와 기존 스프라이트 참조는 보존한다. 최종 이미지로 빌드와 실행 화면 검사를 반복했다.
6. macOS 실기기 검증은 대기다.

빌드: `Builds/Windows/0.5.1/practice_puzzle_game.exe`  
결과: `Builds/Validation/special-art/PlayMode.xml`, `windows-smoke.txt`  
화면: `Builds/Preview/SpecialArt/SpecialPieces.png`, `SpecialAfterSwap.png`, 초기/드래그/성공/실패 캡처

## 0.5.0 매치 그룹·특수 블록

1. EditMode **59개 통과**. 기존 30개에 직선·교차·겹침 그룹, 생성 우선순위·위치·생존, 줄 제거·폭발·색 제거, 직접 조합·연쇄·중복 발동 방지, 특수 블록 셔플 보존 검사를 추가했다.
2. 50개 seed × 10회 이동의 회귀 검사를 특수 교환까지 처리하도록 확장했다. 각 이동 뒤 64개 고유 ID·빈칸 없음·즉시 매치 없음·유효 이동 존재를 확인했다.
3. PlayMode **18개 통과**. 기존 15개에 4매치 생성 시 기존 화면 객체 갱신과 중단 복구, 색 제거 사탕의 실제 마우스 드래그, 마지막 이동 특수 조합 성공 검사를 추가했다.
4. 생성 칸은 해당 제거 단계에서 살아남고 점수에 포함하지 않는다. 효과가 겹쳐도 같은 ID를 한 번만 제거·발동하며, 전체 연쇄에서 이동수는 한 번만 줄어든다.
5. 검증은 별도 소스 복사본에서 수행했다. 코드·테스트·Editor 파일과 `.meta`의 해시가 원본과 일치함을 확인했다. Windows 8.3 축약 경로로 연 Unity에서는 씬 스크립트 연결이 실패하여 **전체 사용자 경로**로 실행했고 18개 모두 통과했다.
6. macOS 실기기·특수 블록 추가 후의 난이도와 장시간 실제 플레이는 아직 미검증이다. 기존 0.3.0의 점수 분포는 특수 블록 추가 전 기록이다.
7. Windows 0.5.0 개발 빌드 성공. 실행 파일에서 특수 색 제거의 실제 마우스 드래그·이동수 1회 차감·점수·보드 안정화·화면/모델 일치와 기존 성공/실패/재시작 검사를 통과했다. 1280×800 캡처로 방향 화살표·폭발 원·무지개 별이 사탕 위에 구분되어 표시됨을 확인했다.

자동 검사: `Builds/Validation/specials/EditMode.xml`, `PlayMode.xml`  
빌드: `Builds/Windows/0.5.0/practice_puzzle_game.exe`  
실행 결과: `Builds/Validation/specials/windows-smoke.txt`  
화면: `Builds/Preview/Specials/SpecialPieces.png`, `SpecialAfterSwap.png`, 기존 초기/성공/실패/드래그 캡처  
특수 실행 검사: `-batchmode -puzzleSpecialSmokeTest -puzzleSmokeReport <절대 경로> -puzzleCaptureFolder <절대 경로>`  
특수 실행 검사는 고정 보드의 네 가지 표식과 색 제거 드래그를 확인한 뒤 기존 성공/실패/재시작 검사도 수행한다. 일반 실행에는 진단 인자가 필요 없다.

## 0.4.3 영문 제목·아이템 빈 슬롯

1. 한글 게임 제목을 제거하고 Jua의 SUGAR GARDEN을 확대했다.
2. 점수판 하단에 빈 슬롯 4개를 배치했다. 아이콘·수량·클릭·아이템 효과는 아직 연결하지 않았다.
3. 보드 크기와 가로 화면의 좌우 패널 상하 정렬을 유지했다.
4. Windows 개발 빌드 성공. 실제 실행 파일의 16:10·16:9·세로 화면을 캡처해 제목·슬롯·점수·버튼의 겹침이 없음을 확인했다.
5. 세 비율 모두 드래그·점수·마지막 이동 성공·실패·결과 입력 잠금·실제 재시작 버튼 검사를 통과했다.
6. 화면 배치 변경이므로 새 단위 테스트는 추가하지 않았다. macOS 실기기 검증은 대기다.

빌드: `Builds/Windows/0.4.3/practice_puzzle_game.exe`  
검사 결과: `Builds/Validation/item-slots/`  
화면: `Builds/Preview/ItemSlots/`

## 0.4.2 점수판 정렬·글꼴 교체

1. 가로 화면의 점수판 상단·하단을 오른쪽 보드 프레임에 맞춘다.
2. 슈가 가든 제목을 점수판 내부로 옮기고 스테이지·이동수·점수·버튼 간격을 재배치했다.
3. 제목·결과 제목은 Bagel Fat One, 점수·이동수·버튼·설명은 Jua를 사용한다. 폰트 생성 시 필수 한글·숫자·영문 글리프를 확인한다.
4. Windows 0.4.2 실행 파일에서 16:10·16:9·세로 화면과 드래그·성공/실패·재시작을 확인했다.
5. 원본 글꼴과 OFL.txt를 보관하고 빌드의 ThirdPartyNotices에 라이선스를 복사한다.
6. 화면 배치·글꼴 변경으로 새 단위 테스트는 추가하지 않았다.

현재 빌드: `Builds/Windows/0.4.2/practice_puzzle_game.exe`  
검사 결과: `Builds/Validation/panel-align/`  
화면: `Builds/Preview/PanelAlign/`

## 0.4.1 드래그 표시·보드 확대

1. 사탕 프리팹과 테마 생성 코드에서 원형 선택 배경을 제거했다. 선택 시 확대와 앞쪽 표시만 유지한다.
2. 하단 안내 문구를 제거하고 스테이지 이름을 점수판 내부로 이동했다.
3. 16:10 화면에서 보드의 실제 표시 너비·높이를 약 21% 확대했다. 좁은 창에서도 정보와 보드를 분리한다.
4. 기존 PlayMode 15개를 통과했다. Windows 실행 파일의 드래그·성공/실패·재시작과 16:10·16:9·세로 창 화면을 확인했다.
5. 개발용 화면 캡처에 사탕을 잡고 중간까지 이동한 `CandyDragging.png`를 추가했다.

현재 빌드: `Builds/Windows/0.4.1/practice_puzzle_game.exe`  
검사 결과: `Builds/Validation/drag-visual/`  
수정 화면: `Builds/Preview/DragVisual/`

## 사탕 테마 0.4.0 추가 검사

1. 사탕 그림·새 UI 적용 후 기존 PlayMode 15개를 다시 실행해 모두 통과했다.
2. Windows 0.4.0 빌드 성공. 실제 실행 파일에서 드래그·점수·성공/실패·다시하기를 확인했다.
3. 1280×800(16:10), 1600×900(16:9), 900×1200(세로)의 초기 화면과 결과창을 캡처했다.
4. 세로 화면에서 정보 패널이 위로 이동하며 보드·다시하기 버튼과 겹치지 않음을 확인했다.
5. 투명 사탕 시트의 6개 스프라이트가 서로 구분되고, 한글과 버튼이 표시됨을 이미지로 확인했다.
6. Core 규칙을 변경하지 않아 기존 EditMode 30개 통과 결과를 유지했다. 이번 테마 작업에서는 PlayMode와 실행 파일 검사를 다시 수행했다.
7. macOS 실기기 검사·최종 아트 품질 및 플레이 감각 평가는 아직 남아 있다.

현재 실행 파일: `Builds/Windows/0.4.0/practice_puzzle_game.exe`  
테마 검사: `Builds/Validation/candy/PlayMode.xml`, `windows-smoke.txt`, `Wide-smoke.txt`, `Portrait-smoke.txt`  
화면: `Builds/Preview/Candy/`의 초기·성공·실패·가로·세로 이미지

개발 전용 캡처에 `-puzzleCaptureWidth <너비> -puzzleCaptureHeight <높이>`를 추가하면 해당 비율로 검사한다. 일반 게임 실행에는 이 인자가 필요 없다.

## 0.3.0 기능 구현 시 결과

| 검사 | 결과 |
| --- | --- |
| Unity C# 컴파일 | 통과 |
| EditMode | 30개 통과 |
| PlayMode | 15개 통과 |
| Windows 0.3.0 개발 빌드 | 생성 성공 |
| Windows 실행 파일 | 드래그·점수·성공/실패·결과 입력 잠금·실제 재시작 버튼 검사 통과, 종료 코드 0 |
| 한글 화면 | 초기 화면·성공·실패 이미지 확인 |
| 기본 목표 확인 | 같은 초기 보드에서 128가지 20회 이동 경로 검사 |
| Mac 실기기 입력·빌드·실행 | 아직 미검증 |
| 특수 블록·색상 수집·저장 | 아직 미구현 |

총 45개 자동 테스트를 통과했다. 원본 프로젝트가 Unity에 열려 있어 별도 소스 복사본에서 검사한 뒤 검증한 변경 파일을 반영했다.

## 규칙·입력 검사

1. 기존 초기 보드·매치·교환 복귀·낙하·보충·연쇄·셔플 검사를 유지했다.
2. 드래그 상하좌우·바로 옆 한 칸 교환·짧은 이동·취소·보드 밖 놓기·포커스 상실·UI 입력 차단을 검사했다.
3. 유효 교환에서만 이동수를 1회 줄이고, 실패 교환에서는 이동수와 점수가 그대로인지 검사했다.
4. 실제 제거된 블록당 10점, 교차 매치·연쇄에서 같은 블록 ID의 중복 점수 방지를 확인했다.
5. 마지막 이동의 모든 연쇄가 끝난 뒤 성공/실패를 판정하며, 목표 달성을 이동수 소진보다 우선하는지 검사했다.
6. 결과 표시 후 보드 입력 잠금, 실제 결과창 버튼으로 연속 두 번 재시작, 보드·점수·이동수 초기화를 확인했다.
7. 처리 도중 비활성화하면 진행 중인 교환의 점수·이동수·보드를 복구하는지 검사했다.
8. 처리 도중 프로그램에서 재시작해도 이전 연쇄가 새 스테이지에 반영되지 않는지 확인했다.
9. 기존 화면 비율·스프라이트 재질 검사를 유지하고 한글 HUD·결과창 표시를 추가했다.

PlayMode는 가상 마우스 이벤트를 실제 BoardInput과 InputSystemUIInputModule에 전달한다. 테스트마다 입력 장치와 UI 액션을 격리하며, 버튼 콜백을 직접 호출해 통과시키지 않는다.

기본 규칙은 **20회, 목표 1,000점, 블록당 10점**이다. seed 5408에서 128가지 이동 경로를 20회씩 계산한 점수 범위는 740~1,290점이었다. 성공과 실패가 모두 가능함을 확인한 것으로, 최종 난이도 검증은 실제 플레이로 진행한다.

## 직접 확인

1. Unity 실행 모드를 멈추고 `Assets/_Project/Scenes/Game.unity`를 다시 연다.
2. 실행하면 상단에 점수 0·목표 1,000·남은 이동 20이 표시된다.
3. 블록을 누른 채 상하좌우로 끌고 놓는다. 유효 교환에서 이동이 1회 줄고 제거 후 점수가 오른다.
4. 목표 달성 또는 이동 소진 후 결과창과 다시하기 버튼을 확인한다.
5. 하단 다시하기 버튼으로도 처음부터 시작할 수 있다. 연쇄 처리 중에는 비활성화된다.
6. `Assets/_Project/Data/Levels/Definitions/Level_001.asset`에서 이동수·목표·블록당 점수를 변경할 수 있다.
7. Unity Test Runner에서 프로젝트의 EditMode·PlayMode 테스트를 실행한다.
8. **Puzzle Game → Build Windows Prototype**으로 Windows 개발 빌드를 만든다.

실행 파일 자동 검사는 `-batchmode -puzzleLevelSmokeTest -puzzleSmokeReport <결과 파일 절대 경로>`로 실행한다. `-puzzleCaptureFolder <폴더 절대 경로>`를 추가하면 화면 이미지도 생성한다. 검사 후 자동 종료하므로 일반 플레이에는 이 인자를 넣지 않는다.

자동 성공/실패 검사는 빠르게 경계 조건을 재현하도록 1회 이동과 별도 목표값을 사용한다. 결과 화면 이미지의 목표 30·100,000은 이 검사용 값이며, 기본 스테이지 목표는 1,000이다. 화면 캡처는 개발 검사 전용 오프스크린 렌더링이며 일반 실행 화면은 Screen Space Overlay 캔버스를 사용한다.

## 로컬 결과물

1. `Builds/Windows/0.3.0/practice_puzzle_game.exe`
2. `Builds/Validation/level/EditMode.xml`
3. `Builds/Validation/level/PlayMode.xml`
4. `Builds/Validation/level/windows-smoke.txt`
5. `Builds/Validation/level/balance.txt`
6. `Builds/Preview/LevelPlaying.png`
7. `Builds/Preview/LevelWon.png`
8. `Builds/Preview/LevelLost.png`

이전 빌드와 보드·드래그 검사 결과는 유지했다. 실행 파일은 같은 폴더의 데이터·DLL·글꼴 라이선스와 함께 사용한다. 자동 검사는 실제 플레이 감각과 Mac 트랙패드 확인을 대체하지 않는다.
