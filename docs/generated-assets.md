# 생성 이미지 기록

생성일: 2026-10-01  
도구: 내장 `image_gen` (`imagegen` 스킬)  
용도: Unity에 실제 적용한 사탕 테마. 참고 이미지는 분위기 참고용이며 아래 에셋은 새로 생성했다.

| 파일 | 사용 |
| --- | --- |
| `Assets/_Project/Art/Sprites/Candies/CandyAtlas.png` | 투명 RGBA 1536×1024, 3×2 배열. Unity에서 512×512 스프라이트 6개로 분리 |
| `Assets/_Project/Art/Backgrounds/SugarGarden.png` | 사탕 정원 배경. 화면 비율에 맞춰 채우고 가장자리를 잘라 표시 |

원본 생성 파일은 별도로 유지하며, 게임은 프로젝트 내부 복사본만 참조한다. UI의 둥근 패널·원·별은 `CandyThemeSetup`의 코드로 생성한다. 설치형 유료 에셋·플러그인은 추가하지 않았다.

## 0.9.0 월드 배경

기존 SugarGarden을 참고해 OrangeOrchard·IceSoda·GrapeNightGarden·RainbowPalace 네 배경을 새로 생성해 `Art/Backgrounds`에 저장했다. 로드맵과 게임이 동일한 월드 그림을 사용한다. [전체 최종 프롬프트·경로](campaign-art-prompts.md).

## 0.9.1 일러스트 로드맵

사용자의 지도 참고 이미지에 맞춰 로드맵 전용 그림 5종과 보석 메달 3종을 새로 만들었다. `Art/WorldMaps`의 사탕 마을·과수원·얼음 소다·포도 밤정원·무지개 궁전 지도는 그림 속 크림 길과 런타임 스테이지 좌표를 함께 사용한다. 하단 월드 이동·현재 위치·빛나는 설탕 입자를 연결했다. 초록 완료 메달의 분홍 하단은 초록/민트색으로 다시 수정했다. 이전 `Art/Backgrounds` 그림은 실제 게임 보드의 배경으로 유지한다. [전체 생성/수정 프롬프트](world-map-art-prompts.md).

## 0.9.2 지면과 어울리는 사탕 발판

월드당 10개로 축소하면서 정면 금테 메달을 사용하지 않고 낮은 타원형 아이싱 쿠키 발판 3종으로 교체했다. `StagePlatformReady.png`, `StagePlatformLocked.png`, `StagePlatformComplete.png`는 지도와 같은 조명·시점을 참고해 내장 image_gen으로 생성한 투명 PNG다. 접지 그림자가 포함된다. 숫자는 별도 Bagel Fat One 텍스트로 렌더링한다. 이전 금테 메달 파일은 대안으로 보관한다. [발판 최종 프롬프트](world-map-platform-prompts.md).

## 0.9.4 로드맵 보석 버튼 선택

사용자가 다시 첨부한 초록·연보라·분홍 보석 PNG를 각각 `LevelMedallionComplete.png`, `LevelMedallionLocked.png`, `LevelMedallion.png`에 원본 그대로 저장했다. 각 파일은 1254×1254 투명 PNG다. 64×64 버튼으로 축소한 목업을 확인한 뒤 숫자를 윗면 중심으로 보정해 실제 지도에 적용했다. 원본 이미지를 변형하지 않고 Unity의 크기와 TMP 정렬을 조정한다. 이전 `StagePlatform*.png` 발판은 대안으로 보관한다.

## 0.6.0 사용자 선택 디자인·중심 정렬

포장 사탕은 사용자가 다시 첨부하고 선택한 넓은 포장 날개 디자인으로 교체했다. 입력은 `codex-clipboard-7fadf9ab-4369-4057-b1bc-74ab77d9012a.png`(1536×1024 RGBA)이며 `CandyWrappedAtlas.png`로 원본 바이트를 그대로 복사했다. 아래 0.5.1의 짧은 포장 끝 비율 보정 결과 대신 사용한다.

그림 자체는 이동하거나 다시 그리지 않고 `CandySpriteAlignment`가 알파 32 이상 영역의 경계 중심을 Unity pivot에 적용한다. 기존 일반 캔디의 초록·파랑·보라는 512픽셀 칸 중심보다 약 24~27픽셀 위에 그려져 있었다. 일반·특수 25종에 같은 기준을 적용하며, 향후 테마/특수 아트 재적용 시에도 유지된다. 런타임 폭발의 빛·확산 원·설탕 조각은 `BoardEffects`의 코드로 생성한 마스크와 애니메이션이다.

## 특수 캔디 0.5.1 — 기존 캔디를 참고해 새로 그린 이미지

사용자 요청에 따라 0.5.0의 벡터 화살표·폭발 원·별 표식을 제거하고 실제 캔디 그림으로 교체했다. 내장 `image_gen`의 이미지 편집을 사용했으며, 네 작업 모두 기존 `CandyAtlas.png`를 참고/편집 입력으로 전달했다. 일반 캔디 원본은 유지한다. 생성 결과의 RGBA 알파를 그대로 보존하고 Unity에서 스프라이트 영역과 표시 크기만 설정한다.

| 파일 | 스프라이트 | 적용 |
| --- | --- | --- |
| `Assets/_Project/Art/Sprites/Candies/CandyRowAtlas.png` | 1536×1024, 3×2, 6종 | 가로 크림 줄무늬, 가로 4매치→좌우 제거 |
| `Assets/_Project/Art/Sprites/Candies/CandyColumnAtlas.png` | 1536×1024, 3×2, 6종 | 세로 크림 줄무늬, 세로 4매치→상하 제거 |
| `Assets/_Project/Art/Sprites/Candies/CandyWrappedAtlas.png` | 1536×1024, 3×2, 6종 | 색과 모양을 유지한 봉지 사탕, T/L/십자→3×3 제거 |
| `Assets/_Project/Art/Sprites/Candies/CandyRainbow.png` | 1254×1254, 1종 | 무지개 소용돌이 사탕, 5개 이상→색 제거 |

`SpecialCandySetup.Apply`가 시트마다 512×512 영역을 분리하고 19개 특수 조합을 `PieceCatalog.asset`에 연결한다. 특수 캔디는 별도 재질 4개를 사용하며 화살표나 별 모양의 런타임 선은 생성하지 않는다.

### 최종 프롬프트

**CandyRowAtlas**

```text
Use case: precise-object-edit. Edit target: the attached original six-candy sprite atlas. Create a production-ready derivative sprite atlas for the SAME game. Preserve the EXACT SIX colors, characteristic silhouettes, arrangement, relative scale, near-frontal camera, glossy thick translucent candy material, internal bubbles, rich colored edges, large creamy upper-left specular highlights, and polished 3D-rendered quality of the reference. Landscape 1536x1024, regular THREE columns by TWO rows, each cell exactly 512x512. Each complete candy centered inside its own cell, filling approximately 76% of its cell, equal perceived weight, wide clean transparent gutters, no overlapping cells. Top row: red heart, orange faceted oval, yellow teardrop. Bottom row: green pillow square, blue thick-rimmed round orb, purple six-lobed flower. TRUE transparent RGBA background, clean alpha cutout, no colored background haze, no floor, no ground shadow, no checkerboard. No text, letters, logos, arrows, line-art outlines, symbols, badges, UI, or flat graphic overlay. All details must be physically sculpted edible confectionery with the same 3D shading as the candy. Change each candy into a HORIZONTAL LINE-CLEAR special candy. Wrap THREE broad creamy ivory fondant stripes HORIZONTALLY left-to-right across the colored candy's belly, following and curving around its 3D surface. The stripes are embedded and smoothly rounded, with thickness and realistic same-source highlights, not painted graphic lines. Keep plenty of the vivid original base color visible between the three clean parallel horizontal stripes. Preserve the recognizable original silhouette and candy size; stripes must read as horizontal at tiny game size. Six candy objects only.
```

**CandyColumnAtlas**

```text
Use case: precise-object-edit. Edit target: the attached original six-candy sprite atlas. Create a production-ready derivative sprite atlas for the SAME game. Preserve the EXACT SIX colors, characteristic silhouettes, arrangement, relative scale, near-frontal camera, glossy thick translucent candy material, internal bubbles, rich colored edges, large creamy upper-left specular highlights, and polished 3D-rendered quality of the reference. Landscape 1536x1024, regular THREE columns by TWO rows, each cell exactly 512x512. Each complete candy centered inside its own cell, filling approximately 76% of its cell, equal perceived weight, wide clean transparent gutters, no overlapping cells. Top row: red heart, orange faceted oval, yellow teardrop. Bottom row: green pillow square, blue thick-rimmed round orb, purple six-lobed flower. TRUE transparent RGBA background, clean alpha cutout, no colored background haze, no floor, no ground shadow, no checkerboard. No text, letters, logos, arrows, line-art outlines, symbols, badges, UI, or flat graphic overlay. All details must be physically sculpted edible confectionery with the same 3D shading as the candy. Change each candy into a VERTICAL LINE-CLEAR special candy. Wrap THREE broad creamy ivory fondant stripes VERTICALLY top-to-bottom down the colored candy's belly, following and curving around its 3D surface. The stripes are embedded and smoothly rounded, with thickness and realistic same-source highlights, not painted graphic lines. Keep plenty of the vivid original base color visible between the three clean parallel vertical stripes. Preserve the recognizable original silhouette and candy size; stripes must read as vertical at tiny game size. Six candy objects only.
```

**CandyWrappedAtlas**

```text
Use case: precise-object-edit. Edit target: the attached original six-candy sprite atlas. Create a production-ready derivative sprite atlas for the SAME game. Preserve the EXACT SIX colors, characteristic silhouettes, arrangement, relative scale, near-frontal camera, glossy thick translucent candy material, internal bubbles, rich colored edges, large creamy upper-left specular highlights, and polished 3D-rendered quality of the reference. Landscape 1536x1024, regular THREE columns by TWO rows, each cell exactly 512x512. Each complete candy centered inside its own cell, filling approximately 76% of its cell, equal perceived weight, wide clean transparent gutters, no overlapping cells. Top row: red heart, orange faceted oval, yellow teardrop. Bottom row: green pillow square, blue thick-rimmed round orb, purple six-lobed flower. TRUE transparent RGBA background, clean alpha cutout, no colored background haze, no floor, no ground shadow, no checkerboard. No text, letters, logos, arrows, line-art outlines, symbols, badges, UI, or flat graphic overlay. All details must be physically sculpted edible confectionery with the same 3D shading as the candy. Change each candy into an AREA-BURST special candy: the original colored and shaped gummy is enclosed in a beautiful small translucent candy wrapper with two short twisted ends at left and right, like luxurious individually wrapped fruit sweets. Wrapper is tinted the SAME main color as its candy, with crisp glossy folds, rounded puffy volume, a delicate pale creamy ribbon cinched around the center, and soft bright highlights. The original heart/gem/drop/pillow/orb/flower core remains visible and recognizable through the wrapper. Entire wrapper including twists must stay inside the 76% cell footprint; ample transparent padding, no elements touching. Clear compact wrapped silhouette, vivid edible candy, NOT a bomb, NOT an icon, NOT a flat drawing. Six wrapped candy objects only.
```

**CandyWrappedAtlas 최종 비율 보정**

첫 봉지 사탕 시트를 편집 입력으로 사용해 짧은 포장 끝과 큰 캔디 본체로 보정했다. 최종 파일 경로는 같은 `CandyWrappedAtlas.png`이며 원본 생성 결과는 생성 보관 폴더에 유지한다.

```text
Use case: precise-object-edit. EDIT the provided wrapped candy sprite atlas. Make ONE targeted proportion correction to ALL SIX candies: the colored central candy body must be MUCH LARGER, while the two wrapper ends become TINY SHORT TWISTS. Currently the central bodies are too small and the wide bow-like ends dominate. Enlarge each candy body by about 30% so the BODY alone fills about 74% of its 512x512 square cell in height and 68-74% in width. Shrink each side's wrapper twist to just 6% of cell width: small compact tight crumpled nubs tucked close against the candy, with a slim cream collar. Entire candy plus short ends should fit within 90% cell width. Center each whole candy EXACTLY in its own 512x512 cell, including row centers y=256 and y=768; place at x=256,768,1280. Preserve all six colors and recognizable body shapes: red heart, orange faceted oval, yellow teardrop / green pillow, blue orb, purple six-petal flower. Keep the exact same beautiful translucent glossy 3D candy material, highlights, rich colors, cream neck collars, camera and rendering style. No letters or symbols, no extra decoration. 1536x1024 atlas with THREE columns and TWO rows. True transparent alpha background with completely clear margins and gutters, preserve transparency. Six candies only. Make them feel substantial and approximately as tall as ordinary unwrapped game candies, with discrete little wrapper twists on their sides.
```

**CandyRainbow**

```text
Use case: precise-object-edit. The attached six-candy sheet is the EDIT TARGET and style reference. Transform the sheet into ONE new special candy sprite for this exact game: a single premium RAINBOW GLASS MARBLE CANDY for a five-in-a-row color-clear effect. Square composition 1024x1024. One entire perfectly centered spherical hard candy, filling 78% of the square with even generous transparent padding. Match the reference's near-frontal view, luscious thick translucent edible glass/sugar material, rich jewel-toned edges, gentle internal microbubbles, clean broad creamy upper-left studio highlights, highly polished rounded 3D volume. The sphere is made of six broad vividly colored curved candy ribbons that swirl organically into a mesmerizing pinwheel at its center: strawberry red, orange, lemon yellow, lime green, blue, purple. Colors are physically fused IN the candy, with some translucency and subtle depth, not painted graphics. Thin milky ivory seams between colored sugar ribbons, tasteful and edible, like a luxurious rainbow swirl jawbreaker. Keep the globe round and compact; no stick or wrapper. Silhouette and lighting feel like the blue orb in the reference, with the new multicolor sculpted swirl. It must be readable at a 60-pixel game scale and look like one delicious candy, not a magic orb or icon. TRUE transparent RGBA background, clean alpha cutout, no haze, no colored backdrop, no floor, no drop shadow, no star, no badge, no arrows, no text, no checkerboard, no sparkles outside the silhouette, no extra candy objects.
```

## 시작 로딩 화면 0.8.0

시안 `loading_screen_a`~`loading_screen_d`와 기본 a의 실제 게임 배경을 `Assets/_Project/Art/Loading/`에 보관한다. 사용자가 첫 번째 시안을 선택했다. 내장 `image_gen` 입력·전체 프롬프트·파생 배경은 [loading-art-prompts.md](loading-art-prompts.md)에 기록했다. 실제 진행률과 문구는 Unity UI로 표시한다.

## CandyAtlas 최종 프롬프트

Create a production-ready 2D game sprite atlas for an original premium candy match-3 game. A landscape 3 columns by 2 rows regular grid, exactly SIX individual candy pieces on TRUE transparent background. Image composition aspect 3:2. Each grid cell is square; each candy centered in its cell at x=1/6,1/2,5/6 and y=1/4,3/4, filling 70% of cell with generous transparent margins, NO overlap. Row1 left to right: glossy strawberry RED heart gummy; glowing ORANGE faceted oval hard candy; bright YELLOW lemon teardrop candy. Row2: fresh GREEN rounded square pillow gummy with subtle molded diagonal crease; vivid BLUE round candy with beautiful thick glassy rim; PURPLE six-lobed flower gummy. All precisely matching 3/4 near-frontal camera angle, all equal perceived visual weight, polished 3D rendered candy shop aesthetic, saturated edible translucent material, broad creamy specular highlights top-left, beveled round edges, rich colored undersides, tiny controlled gleams, lush shiny volume, soft minimal contact shadow within each cell. Crisp silhouette readable as 60-pixel game pieces. Elegant handcrafted art, not flat vector shapes. The six silhouettes must differ visibly. Only six candies, NO text, NO logos, NO UI, NO checkerboard, NO background or floor. Original game artwork.

## SugarGarden 최종 프롬프트

Landscape 16:10 background illustration for a premium original cozy candy puzzle game, high quality softly rendered 3D storybook confectionery garden. Pastel strawberry milk pink sky, creamy peach soft sunlight, a dreamy lavender horizon with rounded marshmallow clouds, distant soft mint and peach rolling hills, a few oversize candy trees and sugar leaves framing only the far left and right outer edges, foreground pink cream swirls in bottom corners. Luxurious soft lighting, silky materials, gentle depth of field. MAIN CENTER 75 percent of image quiet open subtly shaded pale blush-lilac empty space for a puzzle board UI overlay. Side decorations restrained, no clutter behind gameplay, welcoming and bright, beautifully art-directed, pastel but not washed out. NO text, NO logos, NO lettering, NO game board, NO UI, NO characters. Render full-bleed background.
