# 시작 로딩 화면

기본 디자인은 사용자가 선택한 첫 번째 사탕 정원 시안 `loading_screen_a`다.

## 그림 보관

`Assets/_Project/Art/Loading/`에 원본 시안 네 장을 모두 보관한다.

| 파일 | 디자인 |
| --- | --- |
| `loading_screen_a.png` | 사탕 정원·사탕 6종·가로 진행 바. 기본 선택 |
| `loading_screen_b.png` | 사탕 6종 회전 링 |
| `loading_screen_c.png` | 포장 하트 사탕 |
| `loading_screen_d.png` | 3매치 미리보기 |
| `loading_screen_a_background.png` | a에서 진행 바·로딩 문구만 분리한 실제 게임 배경 |

원본 시안에는 진행 바와 문구가 그려져 있다. 실행 화면은 별도 배경 위에 Unity UI로 실제 진행 바·반짝임·문구를 표시한다. 다른 시안으로 바꿀 때도 원본을 유지하고 별도 배경과 해당 배치를 만든다. Boot 씬의 `LoadingCanvas/Composition/Background` Image에서 사용하는 그림을 확인할 수 있다. a~d 원본은 현재 빌드 씬에서 직접 참조하지 않는다.

## 현재 동작

1. 실행 파일의 첫 씬은 `Boot`, 다음은 `WorldMap`이며 열린 스테이지를 선택하면 `Game`에 진입한다.
2. 저장된 창 크기·전체 화면 설정을 복원하고 로딩 화면을 표시한다.
3. `PrototypeStartupLoader`가 실제 시간 약 5초 동안 임시 로딩 작업을 수행한다. `Time.timeScale`의 영향을 받지 않는다.
4. 작업이 완료되면 WorldMap 씬을 비동기로 읽어 진입한다. 씬을 읽는 시간은 5초에 조금 더해질 수 있다.
5. 로드맵에서 진행 기록을 불러오고 설정을 사용할 수 있다. 보드는 스테이지 선택 후 Game에서 생성한다. 게임의 다시 하기는 로딩 화면을 다시 띄우지 않는다.
6. 작업 실패 시 게임으로 넘어가지 않고 다시 시도·게임 종료를 표시한다. 씬 종료 시 진행 중 작업을 취소한다.

Unity에서 전체 흐름을 확인하려면 `Boot.unity`를 열고 실행한다. `Game.unity` 직접 실행은 보드 개발용이다. `Puzzle Game → Prepare Loading Screen`은 기본 a 구성의 Boot 씬을 다시 생성하므로 수동으로 수정한 Boot 배치를 덮어쓴다.

## 향후 로그인 사용자 정보 로딩

5초는 화면의 최소 표시 시간이 아니라 **임시 로더의 처리 시간**이다. 로딩 화면은 시간으로 강제 종료하지 않고 로더가 반환하는 Task의 완료를 기다린다.

- `StartupDataLoader`를 상속해 `LoadAsync(IProgress<float>, CancellationToken)`을 구현한다.
- 로그인 확인과 사용자 정보 조회·적용을 끝낸 뒤 Task를 완료한다. 진행률은 0~1로 알린다. 진행률 1만 알리고 작업이 끝나지 않으면 WorldMap으로 넘어가지 않는다.
- 실패는 예외로 알리고, 네트워크 요청에 전달된 취소 토큰을 연결한다. 재시도 시 이전 요청이 중복 적용되지 않게 처리한다.
- Boot의 `StartupLoadingScreen` 컴포넌트에서 Data Loader 참조를 새 로더로 교체한다.
- 새 로더가 1초에 끝나면 약 1초 후, 8초에 끝나면 약 8초 후 로드맵 씬 로딩을 시작한다. 별도 5초 제한은 없다.

현재 로그인·계정·서버 API는 구현하지 않았다. 로딩 연결 지점만 준비했다.
