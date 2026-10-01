# 일러스트 로드맵과 스테이지 메달

2026-10-01 / 내장 image_gen / imagegen 스킬

사용자의 지도 참고 이미지에서 구불구불한 길과 랜드마크를 통한 여행 구성을 참고했다. 사탕 정원 게임 배경을 재질·색상 참고로 사용하여 새로운 미니어처 월드 지도를 만들었다. 사탕 정원 지도에서 길의 위치를 고정한 채 나머지 월드의 지형·건물·식물을 다시 그렸다. 게임 보드용 기존 배경 5종은 별도로 유지한다.

모든 최종 PNG는 `Assets/_Project/Art/WorldMaps`에 보관한다. 지도 5장은 불투명, 메달 3장은 투명 PNG다. UI 좌표·숫자·현재 위치·반짝임은 Unity에서 구현한다. 완료 메달은 사용자의 지적에 따라 분홍색 하단까지 초록색으로 수정한 최종본을 적용했다.

## SugarGardenMap

```text
Use case: stylized-concept. Asset type: actual illustrated level-select WORLD MAP background for a premium candy puzzle game. The attached SugarGarden image is a MATERIAL, LIGHTING and COLOR reference only, NOT the composition. Create a completely NEW elevated bird's-eye / isometric storybook map, landscape 16:10, full bleed, crisp polished high quality 3D illustration, with the same glossy translucent pink candy and whipped-cream materials and soft inviting light. This must look like a richly art-directed miniature candy kingdom to travel through, full of memorable landmarks, not an empty garden perspective and not a UI mockup.
WORLD ONE: STRAWBERRY SUGAR GARDEN. A luscious pink fondant island with tiered soft hills, turquoise soda stream, a small wafer bridge, strawberry candy cottages in the bottom left, cotton candy tree grove on the left middle, a cake-and-macaron village in the middle right, and a beautiful small cream-and-pink castle toward the upper middle. Miniature sugar flowers, mint leaves, jelly boulders, lollipop trees and delicate frosting details form charming clusters between the route bends, with generous breathing room around the route.
A SINGLE clear continuous winding ivory frosting trail, with pink candy-stripe edging, winds across the island from the lower left to the upper middle castle. The road is a smooth elegant long S with THREE broad horizontal traversals joined by curved bends: begins near normalized (0.12,0.82), sweeps right via (0.35,0.82),(0.62,0.76) to (0.86,0.66), bends left via (0.76,0.48),(0.48,0.52),(0.20,0.43), then bends up and right via (0.17,0.25),(0.43,0.25),(0.66,0.20) ending at castle near (0.79,0.15). Coordinates use left-to-right X and top-to-bottom Y. Keep the entire trail clearly visible, thick enough to place small clickable level medallions on it later, about 3% image height in width. No forks, no extra disconnected roads, no marks or numbered circles on the path. Landmark buildings do not cover the road. Top 10% and bottom 8% are soft scenery for separate UI overlays.
Style: sophisticated soft pastel 3D confectionery diorama, rich miniature detail and believable layers, glossy candy highlights, creamy pink/peach/mint palette. No characters, no text, no words, no logos, no numbers, no interface panels, no watermark. Do NOT reproduce any existing game's landmarks or characters.
```

## OrangeOrchardMap

```text
Use case: style-transfer. Asset: production level-select world map background. Edit the provided original candy map into a new world. Crucial invariants: preserve the EXACT camera, perspective, 16:10 landscape framing, island silhouette, and the EXACT winding road's position, shape, width and every bend from its lower-left beginning through the two S bends to its upper-center palace entrance. Twenty UI level medallions will be placed at fixed road coordinates, so DO NOT move, reroute, obstruct or fork the road. Preserve the relative building-cluster locations, but redesign their architecture and replace flora/materials with the new theme. Keep the same extraordinarily polished, charming, glossy miniature 3D confectionery illustration quality, intricate sugar flowers, soft lighting, beautiful layered depth. Make this a rich coherent world, not a simple hue shift. ORANGE ORCHARD KINGDOM: warm apricot and tangerine fondant terrain; orange-fruit glass cottages instead of strawberry cottages at the lower left; bright citrus candy orchard instead of cotton candy grove on left middle; marmalade mill with cream swirls and candied-orange slice roofs in middle right; elegant orange blossom sugar palace at the upper center. Honey-gold soda river and pale mint accents. The road remains pale vanilla cream with warm orange striped edging. No numbers, no letters, no text, no characters, no icons, no UI panels, no logos, no watermark. Keep the road completely open for later UI buttons.
```

## IceSodaMap

```text
Use case: style-transfer. Asset: production level-select world map background. Edit the provided original candy map into a new world. Crucial invariants: preserve the EXACT camera, perspective, 16:10 landscape framing, island silhouette, and the EXACT winding road's position, shape, width and every bend from its lower-left beginning through the two S bends to its upper-center palace entrance. Twenty UI level medallions will be placed at fixed road coordinates, so DO NOT move, reroute, obstruct or fork the road. Preserve the relative building-cluster locations, but redesign their architecture and replace flora/materials with the new theme. Keep the same extraordinarily polished, charming, glossy miniature 3D confectionery illustration quality, intricate sugar flowers, soft lighting, beautiful layered depth. Make this a rich coherent world, not a simple hue shift. ICE SODA LAGOON: glistening pale turquoise and powder-blue sugar terrain; rounded blue crystal candy cottages at the lower left; frosty soda bubble trees instead of cotton candy grove on left middle; fizzy sherbet village and tiny crystal soda fountain at middle right; beautiful pale blue crystal and whipped-cream palace at upper center. Cyan soda rivers, floating bubbles, gentle cool glow and cozy lavender accents. Road pale vanilla white with aqua candy-striped edging. Refreshing and cheerful, no harsh realistic icy peaks. No numbers, no letters, no text, no characters, no icons, no UI panels, no logos, no watermark. Keep the road completely open for later UI buttons.
```

## GrapeNightGardenMap

```text
Use case: style-transfer. Asset: production level-select world map background. Edit the provided original candy map into a new world. Crucial invariants: preserve the EXACT camera, perspective, 16:10 landscape framing, island silhouette, and the EXACT winding road's position, shape, width and every bend from its lower-left beginning through the two S bends to its upper-center palace entrance. Twenty UI level medallions will be placed at fixed road coordinates, so DO NOT move, reroute, obstruct or fork the road. Preserve the relative building-cluster locations, but redesign their architecture and replace flora/materials with the new theme. Keep the same extraordinarily polished, charming, glossy miniature 3D confectionery illustration quality, intricate sugar flowers, soft lighting, beautiful layered depth. Make this a rich coherent world, not a simple hue shift. GRAPE TWILIGHT GARDEN: softly luminous lavender and plum fondant terrain under magical early twilight, NOT dark; grape-bunch jelly cottages in lower left; vine-and-violet blossom candy grove on left middle; elegant grape candy village with tiny glowing lanterns at middle right; lilac sugar palace and grape-glass roofs at upper center. Lavender soda rivers, delicate warm sugar lights, grape candy vines. Road clearly lit pearl cream with violet candy-striped edging. Rich purples and warm cream highlights, readable bright map. No numbers, no letters, no text, no characters, no icons, no UI panels, no logos, no watermark. Keep the road completely open for later UI buttons.
```

## RainbowPalaceMap

```text
Use case: style-transfer. Asset: production level-select world map background. Edit the provided original candy map into a new world. Crucial invariants: preserve the EXACT camera, perspective, 16:10 landscape framing, island silhouette, and the EXACT winding road's position, shape, width and every bend from its lower-left beginning through the two S bends to its upper-center palace entrance. Twenty UI level medallions will be placed at fixed road coordinates, so DO NOT move, reroute, obstruct or fork the road. Preserve the relative building-cluster locations, but redesign their architecture and replace flora/materials with the new theme. Keep the same extraordinarily polished, charming, glossy miniature 3D confectionery illustration quality, intricate sugar flowers, soft lighting, beautiful layered depth. Make this a rich coherent world, not a simple hue shift. RAINBOW PALACE: pearlescent fondant terrain with refined pastel rainbow tiers; opal rainbow candy cottages in lower left; pastel multicolor lollipop grove on left middle; elegant macaron and rainbow crystal village at middle right; a spectacular pearl-white and iridescent rainbow sugar palace at upper center. Opalescent cyan soda rivers and soft rainbow waterfalls, gold sugar accents, subtle prism sparkles. Road luminous cream with pastel rainbow candy-striped edging. A beautiful celebratory final world with sophisticated color balance. No numbers, no letters, no text, no characters, no icons, no UI panels, no logos, no watermark. Keep the road completely open for later UI buttons.
```

## LevelMedallionLocked

```text
Use case: precise-object-edit. This image is the edit target. Create one state variant of the same game UI medallion. Preserve exact silhouette, dimensions, centered position, raised bead rim, perspective and lighting. Change the center material and color to soft desaturated lavender-gray pearl, subdued satin gloss instead of intense pink, with slightly muted champagne rim; it should look like an inactive collectible awaiting unlock. Keep the center completely empty for a level number. No symbols, no text, no numbers, no additional objects. Preserve genuine transparent background and the same margins. Single medallion.
```

## LevelMedallionComplete

```text
Use case: precise-object-edit. This image is the edit target. Create one state variant of the same game UI medallion. Preserve exact silhouette, dimensions, centered position, raised bead rim, perspective and lighting. Change the center material and color to luminous mint/emerald green translucent candy, with warm cream and gold rim unchanged; it should look like an earned polished candy jewel. Keep the center completely empty for a level number. No symbols, no text, no numbers, no additional objects. Preserve genuine transparent background and the same margins. Single medallion.
```

## LevelMedallion

```text
Use case: stylized-concept. Asset type: one reusable transparent PNG UI sprite for a premium candy world map. A single exquisite small candy level medallion viewed almost straight-on with a slight top-down perspective, circular vanilla-cream rim with fine warm golden sugar beading around the edge, subtly faceted glossy raspberry pink jelly center, a gentle broad curved highlight at upper left, and a shallow darker pink extrusion below giving satisfying candy thickness. The central 65% is flat uncluttered vibrant raspberry pink, empty to overlay a white level number later. Premium soft 3D mobile-game confectionery rendering, coordinated with glossy candy illustration, polished but simple silhouette at 60px. Full circle fits canvas, centered with only 8% padding. Isolated on a genuinely transparent background. NO TEXT, NO NUMBERS, NO SYMBOLS, NO STARS, NO scene, NO multiple icons, NO drop shadow extending far outside. Output exactly one medallion.
```

## LevelMedallionComplete (하단 색상 수정)

```text
Use case: precise-object-edit. Edit target: the attached GREEN candy medallion. Fix the color inconsistency ONLY. The lower extrusion / thick beveled base / sidewall currently has bright raspberry pink and dark magenta inherited from a different state. Replace EVERY pink, red, raspberry and magenta pixel of the medallion body, including the left and right outer rim, lower half sidewall, bottom lip and thin edge, with harmonious green shades: mint highlights, emerald midtones and deep teal-green shadows. The whole candy body must read as one green emerald candy. Keep the central emerald gemstone exactly as is. Keep the entire vanilla-cream raised rim and golden sugar beads exactly as is. Preserve silhouette, size, perspective, margin, lighting, detail and actual transparency. NO remaining pink or magenta anywhere on the medallion. Do not add text, numbers, symbols or other objects.
```
