# 사탕 테마·점수 목표·특수 블록 검증

검증일: 2026-10-02  
Unity: 6.3 LTS `6000.3.25f1`  
환경: Windows 11, x64 개발 빌드

## 0.11.0 기능·UI 7개 확장 — 2026-10-02

- EditMode **92/92**, 프로젝트 PlayMode **88/88**, 합계 **180개 통과**. 마지막 설정 배치 수정 후 관련 **11/11**도 재검사했다. 별의 기본 이동 기준·추가 이동 제외, 목표 기반 힌트의 보드/난수 보존, v1/v2/v3→v4 이전, 실제 완료한 아이템 사용/취소/P 구분, 키보드 선택·교환, 수동 힌트, 중도 이탈 확인, 단일 설정창의 일시정지·해상도 적용, 실패 목표 부족량을 포함한다.
- Windows **0.11.0** 빌드와 실제 포인터·키보드 입력 검사를 통과했다. 방향키/Enter로 유효 교환 1회, H 수동 힌트, 1번 아이템 설명 후 무료 취소, 재시작 확인 취소, 설정 4개 탭, 잠긴 미리보기의 시작 차단, 첫 보상/수령 후 표시와 지도 해금을 확인했다.
- `frozen-2`의 기존 정상 교환 경로로 **1-5는 15회 이동/11회 남음·별 3개·두 메달·4종 보상**, **5-9는 31회 이동/0회 남음·별 2개·아이템 미사용 메달·망치 보상**을 확인했다. v4 JSON을 다시 읽어 별·메달 저장을 검증했다. 다른 스테이지의 완료 상태는 진입을 위한 검사 데이터이며 이번 실행에서 50개를 모두 플레이한 것은 아니다.
- 한국어/영어, **1280×800·960×600·800×1000**의 HUD·입장창·성공/실패 결과·통합 설정을 캡처해 확인했다. 세로 HUD의 재시작/남은 이동 겹침을 해소하고, 입장창 보상 그림을 목표 위로 옮겼다. 좁은 설정창의 선택 버튼은 카드 너비에 비례해 즉시 재배치하도록 수정하고 380/540 UI 단위 너비의 경계 검사를 추가했다. 긴 목표·설정 본문은 스크롤하며 작업 버튼은 고정한다.
- 개인 설정·진행 파일과 분리된 검사 경로만 사용했다. 사람의 체감 평가, Mac 실기기, Steam 출시 준비는 사용자 요청에 따라 후속 작업으로 남긴다.

검사 기록: `Builds/Validation/upgrades/{editmode.xml,playmode-final.xml,dialog-layout.xml,build-final.log}`와 `native-final/{report.txt,player.log,progress.json,settings.json}`. 최종 화면: `Builds/Preview/UpgradesFinal/`. 실행 파일: `Builds/Windows/0.11.0/practice_puzzle_game.exe`.

재현: 개발 실행 파일에 `-batchmode -puzzleUpgradeSmokeTest -puzzleProgressPath <격리된 진행 JSON 절대 경로> -puzzleSettingsPath <격리된 설정 JSON 절대 경로> -puzzleWinningRoutes <Builds/Validation/frozen/winning-routes.txt 절대 경로> -puzzleSmokeReport <결과 파일 절대 경로> -puzzleCaptureFolder <캡처 폴더 절대 경로>`를 전달한다. 일반 플레이에는 검사 인자를 넣지 않는다. 글리프·버전 재설정은 `Puzzle Game > Prepare UI and Feature Upgrades`이며 레벨 데이터는 재생성하지 않는다.

## 0.10.0 얼음 속 캔디 고정 — frozen-2 (2026-10-02)

- EditMode **87/87**, 프로젝트 PlayMode **83/83**, 합계 **170개 통과**. 외부 패키지 Ignore 2개는 별도다. 양방향 교환·실제 드래그 차단, 망치 대상 선택, 1~3겹의 타격 흡수와 동일 캔디 보존, 해동 전 특수 발동 금지, 얼음 기준 낙하 분리, 셔플 중 모델/그림 위치 고정, 중단 복구·환불, 막힌 보드 처리를 포함한다.
- 전체 50개 스테이지·각 3개 보충 seed에서 **1,614회** 전략 플레이를 실행해 모든 조건에서 클리어 경로를 찾았다. 일부 조건은 기본 표본에서 경로가 없어 전략을 추가 탐색했다. [캠페인 CSV](balance-frozen-2.csv), 재현 입력 `Builds/Validation/frozen/winning-routes.txt`.
- 별도의 아이템 비교 **3,456회**를 완료했다. 현재 31회 이동에서 5-6은 아이템 없음 **38/96**, 최대 2개 **60/96**; 5-9는 **26/96**, **59/96** 성공했다. 33/35회 대안도 비교했으나 이번 수정에서는 이동수·목표·아이템 지급을 변경하지 않았다. [아이템 비교 CSV](balance-items-frozen-2.csv)의 `current`는 **31회**, `moves+2`/`moves+4`는 미적용 대안이다. 이전 `quality-1` CSV의 `current` 29회와 혼동하지 않는다.
- 위 성공률은 자동 전략의 표본이며 사람의 성공률이 아니다. 후반 5-6·5-9의 실제 난이도는 새 규칙으로 다시 관찰한다. 로컬 플레이 기록은 `rulesVersion=frozen-2`로 구분한다.
- 검사 파일: `Builds/Validation/frozen/{editmode.xml,playmode.xml,campaign.log,balance-audit.log}`. 이전 절의 바닥형 얼음 규칙에 대한 실행 수치·교환 경로는 역사 기록이며 현재 동작의 검증으로 사용하지 않는다.
- Windows 0.10.0 재빌드 후 실제 입력 경로로 양방향 얼음 교환 차단·망치 3회 해동·셔플 재고 보존·막힌 보드 종료를 통과했다. 1-5 **15회 이동/11회 남음**, 5-6 **29회/2회**, 5-9 **31회/0회**, 5-10 **21회/8회**에 아이템 없이 클리어했다. 매 프레임 남아 있는 얼음 속 캔디의 ID와 화면 위치 고정을 확인했다. `reason=noMoves`로 막힘 실패 기록도 확인했다.
- 한국어/영어 도움말은 1280×800·960×600에서, 실패/셔플 불가 안내는 1280×800에서 실제 렌더를 확인했다. 잘림·버튼 겹침·글리프 누락은 없었다. 실행 결과 `Builds/Validation/frozen/native-batch/report.txt`, 로그·시도 기록은 같은 폴더, 화면은 `Builds/Preview/Frozen/`에 있다. 테스트는 격리된 설정·진행 파일을 사용했다.

Windows 실행 재현은 `-batchmode -puzzleFrozenSmokeTest -puzzleProgressPath <격리된 진행 JSON 절대 경로> -puzzleSettingsPath <격리된 설정 JSON 절대 경로> -puzzleWinningRoutes <frozen/winning-routes.txt 절대 경로> -puzzleSmokeReport <결과 파일 절대 경로> -puzzleCaptureFolder <캡처 폴더 절대 경로>`를 사용한다. 숨겨진 창에 일반 모드로 입력을 보내면 포커스 때문에 클릭이 처리되지 않을 수 있으므로 자동 검사는 batchmode로 실행한다. 일반 실행에는 검사 인자를 넣지 않는다.

## 0.10.0 난이도·안내·접근성·소리 개선 — 2026-10-02

- EditMode **78/78**, PlayMode 전체 **77/77** 통과(패키지 Ignore 2개 별도). 이후 목표 숫자와 진행 막대의 겹침·튜토리얼 재시작 복원 검사 2개를 추가하고 관련 **11/11**을 재검사했다. 서로 다른 프로젝트 검사 합계는 **157개(EditMode 78 + PlayMode 79)**다.
- 동일 초기 보드·보충 seed의 **3,456회** 자동 비교에서 5-6·5-9의 29회/31회/33회와 아이템 미사용/최대 2개를 비교했다. 31회를 채택했고, Windows에서 기존 정상 교환 경로로 5-6은 25회 이동(6회 남음), 5-9는 27회 이동(4회 남음) 후 아이템 없이 클리어했다. [원자료](balance-items-before-quality-1.csv), [분석 조건](quality-pass.md).
- 집계 검사는 사람/자동/이전 기록과 아이템 사용 유무를 분리하고 P 제외·중복 제거·중도 이탈·피드백 연결·손상 줄/레벨/버전 검증을 포함한다. 실제 사용자 기록이 없어 사람 성공률은 산출하지 않았다.
- 새 그림 도움말 4페이지의 실제 아트 참조·팝업 일시정지 복원, 숫자 표식의 중복 생성 방지·같은 색 대응·이전 설정 기본값과 저장 왕복을 검사했다. 960×600 가로/800×1000 세로에서 한글·영문 보드/도움말/설정을 검수했다. 화면 검사에서 발견한 목표 숫자와 진행 막대의 겹침을 수정하고 두 비율의 기하 검사도 추가했다.
- 효과음 20회 중복 요청 억제, 최대 3개·결과음 우선·일시정지 중 재생 선택을 검사했다. 최종 Windows 음원 파형은 음악 144초/peak 0.1400/RMS 0.0216/루프 경계 차이 0.00009, 주요 효과음 peak 0.2300이었다. 파형 상한 확인은 청취 평가를 대신하지 않는다.
- 최종 Windows 빌드에서 **601초 / 264회 반복**, **씬 전환 528회·재시작 528회·일반 교환 264회·아이템 사용 264회**를 통과했다. 4종을 교대로 시험하기 위해 각 회차 시작 재고는 격리된 검사 파일에서 7개로 설정하고, 완료 사용 후 6개가 재시작에도 유지되는지 검사했다. 화면 해상도/창 모드 유지와 보드·오디오·진행 서비스·EventSystem의 단일 인스턴스를 확인했다. 사용자 저장 파일은 사용하지 않았다.
- 10회 준비 이후 GC 후 관리 메모리는 **8.37~8.96 MB**, Unity 할당은 **109.51~110.50 MB**였다. 후반부 Windows 프로세스 표본의 작업 집합은 **603.77~604.85 MB**, 핸들은 **1258~1262개**였다. Unity 할당과 OS 프로세스 메모리는 서로 다른 지표다. 게임 구간 프레임 p95는 0.13~0.14 ms였지만 숨긴 창에서의 자동화 수치이므로 GPU 성능/FPS로 해석하지 않는다. 첫 준비 구간 최대 228.14 ms를 포함한 원자료를 그대로 보관했다.
- v1·v2 저장의 완료/점수 유지·v3 변환과 재저장 시 시작 재고 중복 지급 방지, 손상된 기본 파일에서 백업의 정확한 점수/재고 복원을 실제 Windows 파일로 검사했다. 종료 후 독립 프로세스에서 1280×800 창·영어·숫자 표시·50개 진행 배열·재고 각 7개가 복원됐다. 50개 완료 표시는 검사 해금용 데이터이며 실제로 50개를 이번 실행에서 플레이했다는 뜻은 아니다.
- 종료된 검사 로그를 `PlaytestReport.Run`으로 내보내는 과정도 통과했다. `Builds/Reports/Playtest/stages.csv`에는 5-6/5-9 × 아이템 유무의 4개 자동화 집단이 있으며 손상/중복 줄은 0개였다. P 제외·피드백·출처 혼합 방지는 별도의 회귀 검사에서 검증했다.

기록: `Builds/Validation/quality/{EditMode.xml,PlayMode-final.xml,Quality-final.xml,build-final.log}`, `Builds/Validation/quality/final/`. 최종 화면: `Builds/Preview/QualityFinal/`. 실행 파일: `Builds/Windows/0.10.0/practice_puzzle_game.exe`.

남은 사람 검증: 처음 접한 사용자의 특수 캔디·얼음·아이템 이해도, 청취 반복감·혼합 음량, 다른 PC/배율에서의 활성 창 GPU 성능과 수 시간 실사용. Mac·Steam 배포는 이번 범위에서 제외했다.

## 0.10.0 소비형 아이템 4종 — 2026-10-02

- EditMode **78/78**, 프로젝트 PlayMode **71/71** 통과(전체 73개 중 패키지 Ignore 2개). UI/오류 복구 보강 후 PlayMode 전체를 재검사했다. 신규 규칙 6개와 통합 6개로 총 149개 프로젝트 검사를 통과했다.
- 망치 한 칸·폭탄 중앙 9칸/모서리 4칸, 기존 특수 캔디 연쇄, 중복 얼음 피해 방지, 점수/수집/얼음 목표 합산과 중단 복원을 검사했다. 아이템은 이동수를 쓰지 않으며 이동 +5를 받아도 실제 교환 수는 증가하지 않는다. 실패 이후 추가 이동 사용은 거부한다.
- v2→v3 변환은 완료·최고 기록·선택을 유지한다. 시작 재고가 매 로드마다 다시 지급되지 않으며 잘못된 재고는 백업에서 복구한다. 단계별 보상 순서/5단계 보상/상한 99/재도전 중복/P 제외를 검사했다. 저장 실패 시 효과·차감이 발생하지 않고, 오류를 해결한 뒤 같은 실행에서 다시 사용할 수 있다.
- 최종 Windows 빌드에서 실제 포인터로 설명창·대상 선택·4종 사용을 진행했다. Esc 취소는 설정창을 열지 않으며 재고를 쓰지 않는다. 폭탄 포인터 미리보기가 정확히 9칸을 선택하고 바닥 색을 강조하는지 확인했다. 사용 후 4종 재고는 각각 2개, 이동은 +5만 증가했고 재시작해도 재고가 유지됐다.
- 스테이지 1-5의 기존 정상 클리어 경로를 두 번 재생했다. 첫 번째에만 4종 보상, 두 번째에는 보상 없음. 1-6의 실제 P 키 클리어는 아이템 보상을 지급하지 않았다. 빈 재고 설명창은 사용 버튼을 비활성화했다.
- 아이템 효과 도중 앱을 정상 종료한 뒤 새 프로세스로 재실행해 **미완료 아이템 환불·재고·보상 플래그 저장**을 확인했다. 사용자 저장과 분리한 파일만 사용했다. 강제 프로세스 종료/전원 손실의 진행 중 보드 복구는 지원 범위가 아니다.
- 한국어/영어 설명·보상, 1280×800·1600×900·800×1000 화면을 캡처해 검수했다. 4개 목표·아이템·설정/재시작을 분리했고, 세로 화면의 두 버튼 높이를 동일하게 맞췄다. 아이템 아이콘은 생성한 입체 RGBA 그림 4종이다.

기록: `Builds/Validation/items/EditMode.xml`, `PlayMode-final.xml`, `setup-final.log`, `build-final.log`, `native-final.txt`, `reload-final.txt`.

화면: `Builds/Preview/ItemsFinal/ItemsKo.png`, `BombTargetKo.png`, `HammerHelpKo.png`, `ItemRewardKo.png`, `ItemsEnFinal.png`, `ItemsEnPortrait.png`, `MovesHelpPortrait.png`, `EmptyItemEn.png`.

실행 파일: `Builds/Windows/0.10.0/practice_puzzle_game.exe`. 구현 규칙과 저장 이전은 [campaign.md](campaign.md), 아이콘 최종 프롬프트는 [generated-assets.md](generated-assets.md)에 기록했다.

## 0.10.0 해상도 유지·P 키 이스터에그

- 해상도 복원이 `StartupLoadingScreen.Awake`, `LocalStartupLoader`, 매 씬의 `SettingsPopup.Start`에 중복되어 있었다. `DisplaySettings`의 프로세스 시작 콜백 한 곳으로 옮겼다. 맵/스테이지 로드는 현재 창 크기나 모드를 변경하지 않는다. 설정 적용은 비동기 창 크기 변경을 기다린 뒤 실제 적용값을 저장하고, 그동안 닫기로 저장 과정이 취소되지 않게 했다.
- P 키는 현재 스테이지의 모든 목표를 완료한다. 진행 중인 교환/연쇄를 취소·복원하고 얼음을 정리한 후 일반 결과 처리로 저장·해금·소리·성공/완주 화면을 갱신한다. 이미 얻은 점수를 낮추거나 이동수를 추가 차감하지 않는다. 로컬 기록의 결과는 `easter_egg`로 구분한다.
- EditMode **72/72**, 프로젝트 PlayMode **65/65** 통과(PlayMode 전체 67개 중 패키지 Ignore 2개). 추가 검사는 P 실제 키 입력, 설정 중 무시, 수집/얼음 목표, 연쇄 중단, 키를 누른 채 다음 스테이지 진입, 최종 스테이지 왕관, 재시작을 포함한다.
- Windows 새 빌드에서 **20회 씬 전환** 동안 `Screen.width/height/fullScreenMode`를 매 프레임 검사했다. 지도와 게임 양쪽에서 1280×720 창, 960×600 창, 1280×800 전체 화면, 960×600 창으로 적용한 값과 저장 파일이 유지됐다. 수동 창 크기 1024×768도 씬 전환으로 되돌아가지 않았다. 언어 변경 역시 화면 크기에 영향이 없었다.
- 같은 실행 파일에서 실제 P 키 입력으로 1-1 성공·진행 저장·1-2 해금을 확인했다. 별도 프로세스 두 번에서 저장한 960×600 창 및 1280×800 전체 화면으로 복원하고 각각 맵→게임→맵 이동까지 확인했다. 사용자 저장 대신 격리된 검사 파일을 사용했다.

실행 파일: `Builds/Windows/0.10.0/practice_puzzle_game.exe`. 기록: `Builds/Validation/display-persistence/{EditMode.xml,PlayMode.xml,build.log,native.txt,reload-windowed.txt,reload-fullscreen.txt}`. 실제 창 검사는 `-puzzleDisplaySmokeTest`, 재실행 검사는 `-puzzleDisplayReloadTest`와 격리된 `-puzzleProgressPath`/`-puzzleSettingsPath`/`-puzzleSmokeReport` 인자를 사용한다.

## 0.10.0 결과 연출·추가 아트 개선

- 성공은 금빛 별, 실패는 재도전 하트, 50스테이지 완주는 보석 왕관으로 구분한다. 결과창의 배지·제목·점수·안내와 작업 버튼 영역을 고정했다. 다음 스테이지/피드백은 전체 폭, 다시 하기/뒤로 가기는 같은 폭이다.
- 결과창은 짧은 등장·배지 안착, 성공에는 기존 사탕 그림을 재사용한 작은 입자를 적용했다. 입자는 배지 영역 안에서만 움직이며 텍스트/버튼 입력을 가리지 않는다. 목표 완료 시 카드가 짧게 반응한다. 효과 감소는 모든 새 움직임/입자를 즉시 생략하고, 언어/음량 변경은 축하를 다시 재생하지 않는다.
- 최종 프로젝트 PlayMode **62/62 통과**. 전체 64개 중 패키지 Ignore 2개를 제외했고 실패는 0개다. 결과별 아트·재시작 초기화·설정 변경 중단/재생 방지를 추가했다. Core 규칙은 이번 비주얼 수정에서 변경하지 않았다.
- Windows 0.10.0 빌드 성공. 1-5/5-10 실제 교환 경로, 한국어/영어 성공·실패와 실제 마우스 재시작, 설정·미리보기·저장·초기화/백업 검사를 통과했다. 1280×800, 960×600, 1600×900, 800×1000에서 게임·결과창을 확인했다. 완주 화면은 효과 감소 상태에서도 정적으로 표시된다.

기록: `Builds/Validation/visual-polish/{Final-PlayMode.xml,results-build.log,results-native.txt,results-level-ko.txt,results-level-en.txt,results-campaign.txt}`. 별도 캠페인 검사에서 실제 마우스 다음 스테이지/지도 이동·잠금/해금도 통과했다. 최종 화면: `Builds/Preview/VisualPolishFinal/`. 최종 실행 파일은 `Builds/Windows/0.10.0/practice_puzzle_game.exe`다. 생성 모드·최종 프롬프트·원본 저장 위치는 [generated-assets.md](generated-assets.md)에 기록했다.

## 0.10.0 목표 패널·입체 얼음·힌트 개선

- 수집/얼음 카드의 진행 막대·완료 체크와 완료 취소 복원을 검증했다. 점수 목표 달성도 같은 초록색으로 구분한다. 가로 화면의 두 열과 세로 화면의 네 열에서 다시 하기 버튼과 목표 카드가 겹치지 않도록 조정했다.
- 선으로 그린 Frost 표시를 제거하고 입체 얼음 PNG 3종을 적용했다. 같은 스프라이트 영역을 셀에 맞추고 캔디 앞뒤에 합성한다. 내구도별 아트 변경, 파괴 후 숨김, 취소 후 복원과 LineRenderer 부재를 검사했다. 미리보기/목표 아이콘도 같은 얼음 아트를 사용한다.
- 힌트가 두 칸과 교환 방향을 표시하면서 사탕 좌표/선택을 바꾸지 않는지, 설정·일시정지·포커스 상실 때 사라지는지 검증했다.
- 중간 프로젝트 PlayMode **59/59 통과**, 입체 얼음 교체 후 관련 **9/9 통과**. Windows 0.10.0 빌드에서 1-5/5-10 실제 교환 경로 클리어, 한국어/영어·1280×800/1600×900/800×1000 화면을 확인했다.

기록: `Builds/Validation/visual-polish/{Goals-PlayMode.xml,Ice-PlayMode.xml,ice-build.log,ice-native.txt}`. 최종 입체 얼음 화면: `Builds/Preview/IceArt/{FrostHint,FrostPortrait,FinalStageEn,GoalsPortraitEn,GoalsComplete}.png`.

## 0.10.0 설정·목표 미리보기 개선

- 추가 설정을 소리, 언어/효과, 기타 탭으로 분리했다. 탭별 창 크기와 하단 닫기를 고정하고 음악/효과음은 0~100% 슬라이더·음소거 스위치로 조절한다. 드래그 중 UI를 재생성하지 않으며 음소거해도 기존 음량을 보존한다. 한국어/영어를 직접 선택하고 효과 감소 설명·자동 저장 상태를 표시한다. 도움말/크레딧의 뒤로 가기는 기타 탭으로 돌아온다.
- 스테이지 미리보기는 월드, 이동 횟수, 최고 기록(새 기록은 첫 도전), 실제 점수·색상별 사탕·얼음 목표를 분리한다. 게임과 같은 사탕 스프라이트를 사용하고 시작/취소는 스크롤 밖 같은 폭의 고정 버튼이다. 초기화 확인은 취소에 기본 포커스를 둔다.
- 프로젝트 PlayMode **57/57 통과**. Test Runner 전체 59개 중 패키지의 Ignore 검사 2개를 제외했다. 음소거 후 음량 복원, UI 유지, 언어/효과 변경과 중첩 일시정지, 미리보기 실제 목표·최고 기록·그림·시작/취소를 검증했다. 화면 검사에서 손잡이 세로 늘어남을 수정한 뒤 실제 포인터 클릭/드래그를 포함한 관련 **3/3 검사**를 다시 통과했다.
- Windows 0.10.0 빌드의 한국어/영어 설정·음소거·언어/효과·기타·미리보기·크레딧을 확인했다. 1280×800, 1600×900, 800×1000에서 복합 목표와 고정 하단 버튼을 검수했다. 격리된 파일에 음량/언어/효과 설정을 저장하고 다시 읽었으며, 1-5·5-10 실제 교환 경로·초기화 취소·백업을 검사했다.
- 최종 빌드에서 실제 마우스로 지도 선택 → 미리보기의 고정 시작 버튼 → 1-1 클리어 → 다음 스테이지 → 지도 복귀를 통과했다. ESC 설정·잠금 클릭 무시·진행 저장도 정상이다.
- 검사: `Builds/Validation/dialogs/PlayMode.xml`, `Controls-final.xml`, `build-final.log`, `native-final.txt`, `campaign.txt`. 최종 화면: `Builds/Preview/DialogsFinal/`, `DialogsNavigation/`. 씬 연결 도구는 `Puzzle Game > Prepare Settings and Preview`이며 레벨 밸런스는 변경하지 않는다.

## 0.10.0 공통 버튼·팝업 규격

- `CandyUIStyle`에서 강조/보조/취소/위험 버튼의 색상·폰트·그림자를 관리한다. 팝업 작업 버튼은 높이 48px, 행 간격 12px, 바깥 여백 24px를 사용한다. 지도 보석과 반응형 HUD는 기존 크기·배치를 유지하면서 선택·누름 표현을 공유한다.
- 설정·보조 팝업의 제목과 40px 닫기 아이콘을 고정 영역에 배치했다. 중첩 팝업은 배경이 지나치게 어두워지지 않도록 처리한다. 결과창의 다음/피드백 버튼과 재시작/뒤로 가기의 좌우 끝·높이를 맞추고 실패 점수 겹침 수정을 유지했다.
- 프로젝트 PlayMode **54/54 통과**. Test Runner 전체 56개 중 외부 패키지 검사 2개는 Ignore 상태다. 새 검사 2개는 닫기 버튼을 통한 중첩 일시정지 복원과 효과 감소 상태의 키보드 선택 표시·비활성화를 검증한다.
- Windows 실행에서 한국어/영어·16:10/16:9/세로 화면의 추가 설정·미리보기·크레딧·초기화·튜토리얼·성공/실패/완주를 확인했다. 실제 드래그·성공/실패 후 재시작, Frost/완주 경로, 설정 저장, ESC, 지도 선택·다음 스테이지 이동을 통과했다. 설정창 내부 여백의 최종 보정 후 빌드와 캠페인 마우스 검사를 다시 실행했다.
- 기록: `Builds/Validation/common-ui/PlayMode.xml`, `build-final.log`, `native.txt`, `result.txt`, `campaign-final.txt`. 화면: `Builds/Preview/CommonUI/`, `CommonUIResults/`, `CommonUIFinalNavigation/`.

## 0.10.0 실패 결과창 겹침 수정

- `Feedback` 버튼을 점수와 겹치던 높이에서 실패 안내 아래로 이동했다. 실패 시 숨기는 `NextStage` 버튼 영역을 사용하며, 재시작/뒤로 가기 버튼과도 간격을 확보했다. 저장된 Game 씬과 생성 도구를 함께 수정했다.
- Windows 빌드 성공. 기존 실제 마우스 검사로 성공·실패 판정, 결과창 입력 잠금, 두 차례 재시작을 한국어/영어 설정에서 통과했다. 한국어 1280×800과 영어 960×600 렌더 화면에서 점수·두 줄 안내·피드백 버튼의 겹침이 없는 것을 확인했다.
- 기록: `Builds/Validation/failure-layout/build.log`, `native-ko.txt`, `native-en.txt`. 화면: `Builds/Preview/FailureLayout/Ko/LevelLost.png`, `Builds/Preview/FailureLayout/En/LevelLost.png`.

## 0.10.0 결과창 버튼 정렬·문구 보정

- `다음 스테이지` 버튼의 좌우 끝을 아래 두 버튼의 전체 폭과 맞췄다. 복귀 버튼은 `뒤로 가기` / `Back`으로 변경했고 씬 생성 도구에도 반영했다.
- Windows 빌드를 갱신하고 1-1 실제 클리어 화면에서 정렬·문구를 확인했다. 기존 캠페인 실행 검사로 다음 스테이지 클릭·진행 저장·지도 복귀를 통과했다.
- 기록: `Builds/Validation/result-ui/build.log`, `native.txt`. 화면: `Builds/Preview/ResultUI/Stage1-1Won.png`.

## 0.10.0 Frost·진행 편의·소리·언어 통합

- EditMode **71/71 통과**. Frost 단일 피해, 특수 효과 중첩, 새 특수 생성 칸 보호, 셔플 위치 유지, 복합 목표·중복 집계·마지막 이동 성공·취소 복구를 기존 규칙 검사에 추가했다.
- 프로젝트 PlayMode **52/52 통과**. 잘못된 레벨 배치, 이전 설정 파일의 기본값, 음량·언어·접근성 저장, 미리보기 확인/취소, 실제 Frost 턴 중단 복구, 이펙트 감소 상태의 해결, 무입력 힌트와 입력/일시정지/포커스 취소, 중첩 팝업 일시정지·실시간 문구 변경을 검사했다. Test Runner 전체 54개 중 외부 패키지의 Windows 네이티브 입력 통합 검사 2개는 해당 패키지가 Ignore 처리했다.
- 첫 최종 화면 검사에서 튜토리얼 배너가 마지막 행을 가리는 점과 긴 옵션 창의 닫기 버튼이 아래로 밀리는 점을 수정했다. 첫 5개 레벨은 안내가 사라져도 보드 좌표가 바뀌지 않도록 공간을 유지한다. 이 수정 후 연속 드래그·재시작·설정 입력을 포함한 52개 검사를 다시 통과했다.
- 변경한 50개 레벨 × 3개 보충 난수 조건을 1,600회 자동 플레이했다. 모든 조건에서 승리 경로를 확보했다. 초기 결과에서 남는 이동수가 많아 후반 월드의 이동수를 조정한 뒤 재검사했다. 최종 월드별 자동 성공률은 98.4/95.9/93.8/86.9/77.2%다. 이는 사람의 성공률을 의미하지 않는다.
- Windows **0.10.0** 개발 빌드에서 1-1, Frost가 있는 1-5, 마지막 5-10을 Core 검사와 같은 실제 교환 순서로 클리어했다. 실제 마우스의 잠긴 칸 무시·입장 전 미리보기·시작 클릭, ESC 설정, 순차 해금·다음 스테이지·진행 저장을 확인했다. 별도 프로세스로 1-1 완료·최고 점수·1-2 선택을 복원했다.
- 실제 로더는 이 Windows 검사 장치에서 약 **0.35~0.40초** 후 지도에 진입했다. 수치는 장치와 캐시 상태에 따라 달라진다. 최소 5초 지연이 없으며 데이터가 준비되기 전에 게임에 진입하지 않는다. 제어된 PlayMode 로더로 빠른/늦은 완료·실패/재시도·취소도 유지했다.
- 플레이어의 한국어/영어 문구, 음악/효과음 개별 음량, 이펙트 감소 설정 저장과 읽기, 크레딧, 취소한 초기화의 무변경, 확정 초기화의 본 파일·백업 초기화를 확인했다. `playtest.jsonl`의 실제 시도 기록도 생성했다.
- 1280×800, 1600×900, 800×1000의 실제 렌더 캡처를 확인했다. Frost 내구도 구분·첫 안내·힌트·영어 미리보기·추가 설정 스크롤/X·완주 화면을 확인했고 글리프 누락은 없었다.

빌드: `Builds/Windows/0.10.0/practice_puzzle_game.exe`  
기록: `Builds/Validation/expansion/EditMode.xml`, `PlayMode-final.xml`, `build-final.log`, `native-verified.txt`, `campaign-smoke.txt`, `reload-smoke.txt`, `loading-smoke.txt`  
난이도: [balance-0.10.0.csv](balance-0.10.0.csv), `Builds/Validation/expansion/winning-routes.txt`  
화면: `Builds/Preview/ExpansionFinal/`

모든 플레이어 검사는 `-puzzleProgressPath`, `-puzzleSettingsPath`로 프로젝트 Builds 아래 검사 파일을 명시했다. 실제 사용자 진행·설정을 초기화하지 않았다. 신규 통합 진단은 `-puzzleExpansionSmokeTest`이며 `-puzzleWinningRoutes`, `-puzzleCaptureFolder`, `-puzzleSmokeReport`를 함께 지정한다. 일반 플레이에는 진단 인자를 붙이지 않는다.

사람의 체감 난이도·초보자 이해도·실제 청취 품질, Mac 실기기와 장시간 출시 품질은 후속 검증이다. 기본 음량은 음악 35%, 효과음 65%이고 동시 효과음은 최대 8개로 제한한다.
## 0.9.4 보석 버튼·윗면 중앙 숫자

선택된 초록/분홍/연보라 보석 버튼을 64×64로 적용했다. 숫자의 정렬을 TMP `MidlineGeoAligned`로 바꾸고 텍스트 중심을 버튼 높이의 55%에 맞춰 아래 테두리를 제외한 둥근 윗면에 배치했다. 1~10과 완료·도전·잠금 세 가지 상태를 실제 지도 캡처로 확인했다. 기존 106×76 클릭 영역을 유지한다.

Windows 빌드 및 실제 플레이어 검사를 통과했다. 로딩→지도, ESC 설정, 잠금 클릭 무시, 버튼 그림 바깥의 확장 영역 클릭, 1-1 실제 클리어·저장·1-2 해금을 확인했다. Direct3D 11과 격리된 진행 파일을 사용했고 게임 규칙 변경은 없다.

실행 파일: `Builds/Windows/0.9.4/practice_puzzle_game.exe`. 기록: `Builds/Validation/jewel-map-final/{setup.log,build.log,native-smoke.txt}`. 최종 화면: `Builds/Preview/JewelButtonsFinal/UnlockedMap.png`. 승인 전 목업은 `Builds/Preview/JewelButtons/JewelButtonsMockup.png`에 보관한다.

## 0.9.3 지도 발판 크기 조정

발판을 106×76에서 74.2×53.2로 줄였다(가로·세로 30% 축소). 숫자·현재 위치 표시·빛도 함께 줄이고, 음수 raycast padding으로 기존 106×76 클릭 영역을 유지한다.

Windows 빌드와 실제 플레이어 검사를 통과했다. 축소한 첫 발판의 중심에서 오른쪽 45만큼 떨어진 지점(그림 사각형 바깥)을 마우스로 눌러 입장했고, 잠금·클리어·저장·다음 스테이지·지도 해금을 확인했다. 별도 프로세스로 다시 실행해 완료 기록·최고 점수·선택 위치를 복원하고 1-2에 이어서 진입했다. 실제 지도 캡처에서 크기와 숫자 가독성을 확인했다. 런타임 검사는 Direct3D 11과 격리된 저장 경로를 사용했다. 레벨 규칙 변경은 없다.

5개 월드 지도에서도 축소한 발판을 캡처해 배치와 숫자를 확인했다. 실행 파일: `Builds/Windows/0.9.3/practice_puzzle_game.exe`. 검사 기록: `Builds/Validation/world-map-v4/{setup.log,build.log,native-smoke.txt,reload-smoke.txt,preview-smoke.txt}`. 화면: `Builds/Preview/WorldMapV4/FreshMap.png`, `Builds/Preview/WorldMapV4/Worlds`.

## 0.9.2 50개 스테이지·사탕 발판 지도·진행 저장

1. 월드당 10개, 총 **50개**의 초기 보드가 서로 다르며 즉시 매치 없이 유효 이동을 갖는지 확인했다. Core의 특수 효과·보충·셔플·수집 규칙으로 **50개 모두 실제 클리어 경로**를 찾았다. 경로와 CSV를 저장했다. 전략 표본의 결과이므로 사람의 승률이나 체감 난이도를 의미하지 않는다.
2. 최종 PlayMode **44개 전부 통과**: 기존 조작·설정·로딩 회귀와 캠페인 잠금·선택·다음·최종 스테이지·저장 복구, 이전 v1의 100개 기록을 새 v2 50개로 변환하는 7개 경계 사례를 포함한다. Core는 수집 목표 구현 시 EditMode **65개 통과**했고 이후 지도 변경에서 규칙 코드는 바꾸지 않았다.
3. Windows **0.9.2** 빌드와 실제 플레이어 검사 통과. Boot → 로드맵, ESC 설정, 잠긴 발판 클릭 무시, 첫 발판 마우스 선택, 저장한 실제 교환 경로로 1-1 클리어, 즉시 JSON 저장, 1-2 진입, 로드맵 해금을 검사했다. 사용자 저장 대신 격리된 JSON 경로를 사용했다.
4. 로드맵은 전용 일러스트 5종의 크림 길과 같은 좌표계로 배치한다. 정면 금테 메달을 낮은 아이싱 쿠키 발판으로 교체하고 Bagel Fat One 숫자 1~10을 크게 표시했다. 1280×800의 첫 지도·클리어·해금 화면을 눈으로 확인했다.

실행 파일: `Builds/Windows/0.9.2/practice_puzzle_game.exe`

검사 기록: `Builds/Validation/world-map-v3/{PlayMode.xml,balance.csv,winning-routes.txt,native-smoke.txt}`. Core 기록: `Builds/Validation/campaign/EditMode.xml`.

캡처: `Builds/Preview/WorldMapV3`. Mac 실기기·실제 계정 로그인·기기 간 동기화는 미검증/미구현이며 체감 난이도 조정은 실제 플레이로 진행한다.

### 이전 캠페인 작업

0.9.0에서 100개·5월드·로드맵·저장을 구현하고 Windows 재실행 복원을 확인했다. 0.9.1에서 전용 지도·금테 메달로 교체하고 캠페인 6개 검사를 다시 통과했다. 이후 사용자 요청에 따라 0.9.2의 50개·발판 디자인으로 변경했다. 이전 기록과 그림은 보관한다.

## 0.8.0 시작 로딩 화면

1. PlayMode **31개 통과**. 기존 26개와 신규 5개로 기본 5초 실시간 대기, timeScale=0에서도 진행, 5초 이전 완료, 5초 이후에도 미완료 작업 대기, 실패/재시도, 씬 종료 취소와 늦게 오는 콜백 무시를 확인했다. 재시도 취소 보강 후 로딩 검사 5개를 다시 통과했다. Core 변경은 없다.
2. Windows **0.8.0** 빌드·실행 성공. 시작 씬 Boot에서 진행 바가 증가하고 **5.13초 뒤 Game**으로 전환했다. 준비 전에 보드가 생성되지 않으며 완료 후 보드 입력·재시작·설정이 정상 동작한다.
3. 1280×720, 1280×800, 800×1000 캡처를 눈으로 확인했다. 제목·사탕·진행 바·문구가 잘리지 않게 비율을 유지하며 남는 공간은 분홍색으로 채운다. 16:10·세로 캡처는 Windows에서의 오프스크린 UI 검사이며 Mac 실기기 검증을 의미하지 않는다.
4. 실제 플레이어에서 로딩 후 기존 설정 동작(버튼/ESC·해상도·창/전체 화면·일시정지/재개·파일 저장·실제 종료)을 다시 검사했다. 진단은 별도 `display-smoke.json`을 사용한다.
5. 시안 a~d 원본과 실제 a 배경을 소스에 보관한다. 빌드 에셋 목록에는 사용하는 `loading_screen_a_background.png`만 포함되고 시안 원본 네 장은 포함되지 않는다.

빌드: `Builds/Windows/0.8.0/practice_puzzle_game.exe`

자동 검사: `Builds/Validation/loading/PlayMode.xml`, `StartupFinal.xml`

실행 결과: `Builds/Validation/loading/windows-smoke.txt`, `settings-smoke.txt`, `settings-reload.txt`

화면: `Builds/Preview/Loading/{LoadingEarly,LoadingWide,LoadingMacRatio,LoadingNarrow}.png`

시작 검사 인자: `-batchmode -puzzleLoadingSmokeTest -puzzleSettingsPath <별도 설정 JSON> -puzzleSmokeReport <결과 경로> -puzzleCaptureFolder <화면 폴더>`

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
