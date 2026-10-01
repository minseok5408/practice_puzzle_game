# 연동

1. **프로젝트 생성:** Unity Hub → 프로젝트(Projects) → 새 프로젝트(New project).
   - **에디터:** Unity 6.3 LTS (`6000.3.25f1`)
   - **템플릿:** Universal 2D
   - **프로젝트 이름:** `practice_puzzle_game`
   - **위치:** `C:\project\minseok5408`
   - **AI Assistant 사용:** 체크 해제
   - **Unity CLI 사용:** 체크 해제
   - **소스 제어 공급자:** 없음
2. **프로젝트 생성(Create project)**을 누르고 Unity 편집기가 열릴 때까지 기다립니다.
3. 상단 **Edit → Preferences…** → **External Tools(외부 도구)** → **External Script Editor(외부 스크립트 에디터)**를 **Visual Studio Code**로 선택합니다.
4. 같은 창에서 **Languages(언어)** → **Editor Language (Experimental)** 체크박스가 있으면 켜고, **한국어(Korean)**를 선택합니다.
5. 재시작 안내가 나오면 Unity를 재시작합니다.
6. 프로젝트 창의 **Assets → 우클릭 → 생성(Create) → 폴더(Folder)**로 `Scripts` 폴더를 만듭니다.
7. `Scripts` 안에서 **우클릭 → 생성(Create) → 스크립팅(Scripting) → MonoBehaviour 스크립트**를 선택하고 이름을 `SetupCheck`로 지정합니다.
8. `SetupCheck.cs`를 더블클릭해 VS Code에서 열고 아래 코드로 바꾼 뒤 저장합니다.

   ```csharp
   using UnityEngine;

   public class SetupCheck : MonoBehaviour
   {
       private void Start()
       {
           Debug.Log("첫 Unity 스크립트 실행 성공!");
       }
   }
   ```

9. Unity의 **계층 구조(Hierarchy) → 빈 공간 우클릭 → 빈 오브젝트 생성(Create Empty)**을 누르고 이름을 `SetupCheck`로 지정합니다.
10. 프로젝트 창의 `SetupCheck.cs`를 계층 구조의 `SetupCheck` 오브젝트 위로 드래그해 연결합니다.
11. 상단 **▶ 실행** → **콘솔(Console)**에서 `"첫 Unity 스크립트 실행 성공!"` 로그를 확인합니다.
12. 상단 **■ 종료** 버튼으로 실행을 종료합니다.
