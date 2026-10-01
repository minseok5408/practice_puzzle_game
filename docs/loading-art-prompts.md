# 로딩 화면 생성 기록

## 시작 로딩 화면 0.8.0

내장 `image_gen`으로 게임 화면·일반 캔디·포장 캔디를 참고해 로딩 시안 네 장을 생성했다. 사용자가 첫 번째 시안을 선택하고 나머지도 이름을 정해 보관하도록 요청했다.

- `Assets/_Project/Art/Loading/loading_screen_a.png`: 사탕 정원, 선택한 기본 시안.
- `Assets/_Project/Art/Loading/loading_screen_b.png`: 사탕 회전 링.
- `Assets/_Project/Art/Loading/loading_screen_c.png`: 포장 하트 사탕.
- `Assets/_Project/Art/Loading/loading_screen_d.png`: 3매치 미리보기.
- `Assets/_Project/Art/Loading/loading_screen_a_background.png`: a에서 진행 바·문구만 지운 게임용 배경. 그림은 원본 생성 결과를 그대로 복사했으며 코드로 재가공하지 않았다.

실제 진행 바와 반짝임·로딩 문구는 기존 UI 스프라이트와 TMP로 따로 표시한다. a~d 시안의 전체 해상도 원본을 Git에 보관한다. 생성 이미지는 모두 불투명 배경이다.

### 로딩 시안 최종 프롬프트

**loading_screen_a**

```text
Use case: ui-mockup.
Create ONE polished landscape 16:9 PC game startup loading screen concept, full bleed, no monitor/device frame, no comparison collage.
This is for the existing game SUGAR GARDEN and will be visible for 5 seconds.
Input image 1 is reference only: the actual game screenshot, use its pastel pink candy garden, warm cream, lavender, and friendly rounded typography.
Input image 2 is reference only: exact existing six candy designs/materials — glossy translucent red heart, orange faceted oval gem, yellow teardrop, green rounded square with diagonal ridge, blue circular orb, purple six-petal flower.
Input image 3 is reference only: the existing deluxe wrapped candy variants with clear colored crinkled wrappers and pale gold ties.
Preserve these candy identities closely: realistic juicy translucent hard candy with bright white reflections, subtle tiny bubbles and saturated colors. Do NOT replace them with flat icons, generic jelly beans, matte cartoon candies, faces, or mascots.
Create a distinct new loading-screen composition from these references, not a screenshot of the gameplay HUD. Carefully balanced, production-quality casual puzzle UI, strong hierarchy, readable lettering, uncluttered.
Exact main title: "SUGAR GARDEN". Exact loading label: "로딩 중...". Rounded bold typography, legible Korean.
No buttons, no fake operating system UI, no watermarks, no third party game logos, no percentage numbers, no countdown numerals, no editor notes.
The image is a single still frame partway through the loading animation. Concept 1 — GARDEN WELCOME. Warm luminous pastel pink candy garden stretching into the distance, soft sugar hills, candy trees and small flowers consistent with screenshot. Main title large in the upper middle, cream lettering with restrained plum outline and soft pink depth. A beautiful centered horizontal arc of the six exact normal candies below the title; they look like they could bob gently in sequence. At lower middle a wide rounded cream track with raspberry pink candy-glaze fill at about 60 percent, a tiny sparkle on its moving edge, and the Korean loading label beneath. Keep good breathing room around the title and loader; slightly soften background behind text. Welcoming daylight, broad cinematic landscape with sharply finished candies. No wrapped candies needed here.
```

**loading_screen_b**

```text
Use case: ui-mockup.
Create ONE polished landscape 16:9 PC game startup loading screen concept, full bleed, no monitor/device frame, no comparison collage.
This is for the existing game SUGAR GARDEN and will be visible for 5 seconds.
Input image 1 is reference only: the actual game screenshot, use its pastel pink candy garden, warm cream, lavender, and friendly rounded typography.
Input image 2 is reference only: exact existing six candy designs/materials — glossy translucent red heart, orange faceted oval gem, yellow teardrop, green rounded square with diagonal ridge, blue circular orb, purple six-petal flower.
Input image 3 is reference only: the existing deluxe wrapped candy variants with clear colored crinkled wrappers and pale gold ties.
Preserve these candy identities closely: realistic juicy translucent hard candy with bright white reflections, subtle tiny bubbles and saturated colors. Do NOT replace them with flat icons, generic jelly beans, matte cartoon candies, faces, or mascots.
Create a distinct new loading-screen composition from these references, not a screenshot of the gameplay HUD. Carefully balanced, production-quality casual puzzle UI, strong hierarchy, readable lettering, uncluttered.
Exact main title: "SUGAR GARDEN". Exact loading label: "로딩 중...". Rounded bold typography, legible Korean.
No buttons, no fake operating system UI, no watermarks, no third party game logos, no percentage numbers, no countdown numerals, no editor notes.
The image is a single still frame partway through the loading animation. Concept 2 — CANDY ORBIT. Deliberately simple and clean warm ivory background softly washed with lavender and blush pink near corners, subtle soft-focus candy garden edges. Main title medium-large plum at upper center. In the center arrange the six exact normal candies with equal scale and spacing in a circular ring, each distinct color once, around a small central cream circle reading "로딩 중..." in plum. A delicate pink sugar-light arc traces roughly 60 percent of the circle behind the candies, suggesting a clockwise progress ring; tiny restrained sparkles indicate rotation. Soft oval shadows, lots of uncluttered negative space. This is a stylish airy cream layout clearly different from a detailed scenic background. No bottom progress bar.
```

**loading_screen_c**

```text
Use case: ui-mockup.
Create ONE polished landscape 16:9 PC game startup loading screen concept, full bleed, no monitor/device frame, no comparison collage.
This is for the existing game SUGAR GARDEN and will be visible for 5 seconds.
Input image 1 is reference only: the actual game screenshot, use its pastel pink candy garden, warm cream, lavender, and friendly rounded typography.
Input image 2 is reference only: exact existing six candy designs/materials — glossy translucent red heart, orange faceted oval gem, yellow teardrop, green rounded square with diagonal ridge, blue circular orb, purple six-petal flower.
Input image 3 is reference only: the existing deluxe wrapped candy variants with clear colored crinkled wrappers and pale gold ties.
Preserve these candy identities closely: realistic juicy translucent hard candy with bright white reflections, subtle tiny bubbles and saturated colors. Do NOT replace them with flat icons, generic jelly beans, matte cartoon candies, faces, or mascots.
Create a distinct new loading-screen composition from these references, not a screenshot of the gameplay HUD. Carefully balanced, production-quality casual puzzle UI, strong hierarchy, readable lettering, uncluttered.
Exact main title: "SUGAR GARDEN". Exact loading label: "로딩 중...". Rounded bold typography, legible Korean.
No buttons, no fake operating system UI, no watermarks, no third party game logos, no percentage numbers, no countdown numerals, no editor notes.
The image is a single still frame partway through the loading animation. Concept 3 — WRAPPED HEART HERO. Pastel pink and lavender candy garden is gently blurred into a soft dreamlike background. At upper center the title in large cream-and-plum rounded lettering. Centerpiece is ONE large, beautifully rendered red heart wrapped candy matching input image 3 exactly in identity, transparent ruby-red crinkled wrapper twists on both sides and pale gold bands, bright juicy heart visible inside. It floats slightly above a soft shadow, with a few tiny sugar sparkles suggesting a gentle breathing/pulsing animation. The wrapped candy occupies about one third of screen width, not the whole screen. Beneath it a cream-framed elegant slim pill progress bar with a pink-to-lavender glossy fill at 60 percent and the exact loading label beneath. Include a few softly blurred normal blue and purple candies at far corners only if useful for framing. Refined readable composition with a premium candy emphasis.
```

**loading_screen_d**

```text
Use case: ui-mockup.
Create ONE polished landscape 16:9 PC game startup loading screen concept, full bleed, no monitor/device frame, no comparison collage.
This is for the existing game SUGAR GARDEN and will be visible for 5 seconds.
Input image 1 is reference only: the actual game screenshot, use its pastel pink candy garden, warm cream, lavender, and friendly rounded typography.
Input image 2 is reference only: exact existing six candy designs/materials — glossy translucent red heart, orange faceted oval gem, yellow teardrop, green rounded square with diagonal ridge, blue circular orb, purple six-petal flower.
Input image 3 is reference only: the existing deluxe wrapped candy variants with clear colored crinkled wrappers and pale gold ties.
Preserve these candy identities closely: realistic juicy translucent hard candy with bright white reflections, subtle tiny bubbles and saturated colors. Do NOT replace them with flat icons, generic jelly beans, matte cartoon candies, faces, or mascots.
Create a distinct new loading-screen composition from these references, not a screenshot of the gameplay HUD. Carefully balanced, production-quality casual puzzle UI, strong hierarchy, readable lettering, uncluttered.
Exact main title: "SUGAR GARDEN". Exact loading label: "로딩 중...". Rounded bold typography, legible Korean.
No buttons, no fake operating system UI, no watermarks, no third party game logos, no percentage numbers, no countdown numerals, no editor notes.
The image is a single still frame partway through the loading animation. Concept 4 — FIRST MATCH PREVIEW. Warm cream background framed by gentle pastel pink candy garden elements on far left and right. Title medium-large and plum at upper center. At center a small elegant horizontal strip of ONLY THREE rounded lavender game cells with a cream/pink border, each containing the exact glossy red heart candy from reference image 2. A tasteful tiny starburst and sugar fragments surround the middle heart, with a pink light trail linking the three hearts, depicting a fun match-three pop animation still. No full game board or score HUD. Under this little match demonstration put the exact short Korean tip "같은 사탕 3개를 맞춰 보세요" in plum rounded readable type. Near the lower center a rounded cream progress track with pink fill at 60 percent and the separate exact loading label "로딩 중..." under it. Keep all text comfortably spaced and clean, candy art rich and translucent, premium playful visual.
```

**loading_screen_a_background**

```text
Edit this approved SUGAR GARDEN loading screen into its production background layer. Preserve the existing image exactly as closely as possible: composition, dimensions, title lettering and decorations, candy garden, lighting, all six glossy candies, their sizes and positions. Change ONLY the lower progress-bar area and the Korean loading label beneath it: remove the entire progress bar including its cream outline, pink fill, lavender track, highlight star and shadow, and remove the "로딩 중..." text. Seamlessly continue the surrounding soft pink sugared ground through these removed areas, leaving calm negative space for a real animated Unity progress bar and live text to be overlaid later. Keep the lower corner foreground flowers and candy borders. Do not move, redraw, recolor, simplify or change the title or six candies. No new elements or text. Return the same wide 16:9 image, full bleed, opaque background.
```
