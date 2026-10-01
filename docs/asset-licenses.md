# 에셋 출처

기록일: 2026-10-01

| 에셋 | 출처·조건 | 보관 위치 |
| --- | --- | --- |
| Bagel Fat One | [Google Fonts](https://fonts.google.com/specimen/Bagel+Fat+One), [SIL OFL 1.1](https://github.com/google/fonts/blob/main/ofl/bagelfatone/OFL.txt) | `UI/Fonts/BagelFatOne`, `CandyTitle.asset` |
| Jua | [Google Fonts](https://fonts.google.com/specimen/Jua), [SIL OFL 1.1](https://github.com/google/fonts/blob/main/ofl/jua/OFL.txt) | `UI/Fonts/Jua`, `CandyBody.asset` |
| 나눔고딕 Regular | [Google Fonts 원본](https://github.com/google/fonts/tree/main/ofl/nanumgothic), SIL OFL 1.1 | `Assets/_Project/UI/Fonts/NanumGothic`의 TTF·OFL.txt |
| PuzzleUI TMP 폰트 | 나눔고딕 원본으로 생성한 게임용 폰트 에셋 | `Assets/_Project/UI/Fonts/PuzzleUI.asset` |
| TMP Essential Resources | 설치된 Unity uGUI 패키지의 번들 리소스 | `Assets/TextMesh Pro`, 포함된 LiberationSans OFL·EmojiOne Attribution 유지 |
| 사탕 6종·정원 배경 | 내장 image_gen으로 새로 생성, [프롬프트·파일 기록](generated-assets.md) | `Art/Sprites/Candies`, `Art/Backgrounds` |
| 시작 로딩 시안 a~d·a 배경 | 기존 게임 그림을 참고해 내장 image_gen으로 생성, [전체 프롬프트](loading-art-prompts.md) | `Art/Loading` |
| 2~5월드 배경 4종 | 기존 SugarGarden을 참고해 내장 image_gen으로 생성, [전체 프롬프트](campaign-art-prompts.md) | `Art/Backgrounds/OrangeOrchard.png`, `IceSoda.png`, `GrapeNightGarden.png`, `RainbowPalace.png` |
| 일러스트 지도 5종·스테이지 보석 메달 3종 | 기존 게임의 재질·테마를 참고해 내장 image_gen으로 생성, 초록 메달 하단 재수정, [최종 프롬프트](world-map-art-prompts.md) | `Art/WorldMaps` |
| 사탕 발판 3종 | 지도 시점·조명을 참고해 내장 image_gen으로 생성, [최종 프롬프트](world-map-platform-prompts.md) | `Art/WorldMaps/StagePlatformReady.png`, `StagePlatformLocked.png`, `StagePlatformComplete.png` |
| 특수 캔디 19종 | 기존 사탕 시트를 참고해 내장 image_gen으로 새로 그린 파생 이미지, [프롬프트·파일 기록](generated-assets.md) | `Art/Sprites/Candies/CandyRowAtlas.png`, `CandyColumnAtlas.png`, `CandyWrappedAtlas.png`, `CandyRainbow.png` |
| 둥근 패널·원·별 | 프로젝트 코드로 생성 | `UI/Theme` |
| 블록·셀 임시 그림 | 프로젝트에서 절차적으로 생성 | `Assets/_Project/Art/Sprites` |

나눔고딕은 게임과 함께 배포할 수 있으며 저작권 고지·라이선스를 함께 보관한다. 원본 글꼴은 수정하지 않았다. [원본 라이선스](https://github.com/google/fonts/blob/main/ofl/nanumgothic/OFL.txt)

Windows 빌드에는 `Font-LICENSE.txt`와 `ThirdPartyNotices`를 함께 복사한다. 추후 Mac 배포에도 같은 고지를 포함한다.

원본 TTF SHA-256: `76f45ef4a6bcff344c837c95a7dcc26e017e38b5846d5ae0cdcb5b86be2e2d31`

Bagel Fat One은 제목, Jua는 점수·버튼·설명에 사용한다. 원본 TTF는 수정하지 않았으며 TMP 에셋은 렌더링용이다. 유료 게임 배포에도 각 저작권 고지와 OFL 라이선스를 동봉한다. Windows 빌드의 `ThirdPartyNotices/BagelFatOne-OFL.txt`, `Jua-OFL.txt`로 복사한다.
