
## make this into faithful remaster blackmyth wukong style
[given a ps1 style image from final fantasy]
![[Pasted image 20251122204814.png]]

![[Pasted image 20251122204823.png]]


## redesign this interface to make it look better
[given web interface image reference with ui]
- can preserve all the text from og screenshot

## heat/top view/depth map/edge detection
	an [img] split into 4 quads
	1st quad: a heat map of the photo
	2nd quad: a top view of the photo
	3rd quad: a depth map of the photo
	4th quad: an edge canny estimation of the photo

## turn this fundam into its og components prior to assembly
	action character prior to assembly
	given a asssembled 3d model(pic)

## from this floor plan, make a photo of this room with a cozy minimalist design.
![[Pasted image 20251122204522.png]]
![[Pasted image 20251122204547.png]]
## make a 2D floor plan of this apartment.
![[Pasted image 20251122204634.png]]
![[Pasted image 20251122204647.png]]


Nice — here are ready-to-paste **Nano Banana** (Gemini image) prompts for a two-floor horror-game floor plan with lots of door interactions. I give a main, highly-detailed prompt plus shorter quick variants and a few tuning tips you can paste directly into Nano Banana.

> Quick note: “Nano Banana” = Google’s Gemini image editor / image model (aka Gemini 2.5 / Nano Banana). Use the detailed prompt for one-shot high-quality output and the short variants for quick iterations. ([Gemini](https://gemini.google/overview/image-generation/?utm_source=chatgpt.com "Nano Banana Pro - Gemini AI image generator & photo editor"))

# 1 — Full, detailed prompt (paste this first)

```
Top-down, orthographic architectural floor plan for a two-story horror game. Produce a single high-resolution image that clearly shows BOTH floors: stack the second floor directly above the ground floor (aligned), with scale grid and north arrow. Style: clean vector blueprint with subtle horror mood (dim vignette & light film grain). Include: narrow corridors, dead-ends, large central foyer, service tunnels, attic access, elevator shaft, staircases, balcony, and backstage maintenance rooms. Populate with many door types and annotate each with a small icon and short label: "HINGED (swing arc) — press E", "SLIDING (double arrow) — swipe", "WINDOW (breakable)", "BOARDed WINDOW", "METAL SHUTTER (requires code)", "TRAPDOOR (hidden)", "SECRET (bookshelf)". Draw door icons next to each opening: hinged = arc, sliding = double arrow, window = thin rectangle. Color-coded overlays: red = danger/monster spawn, yellow = interactive doors, blue = safe/locked rooms. Use legible architectural fonts, room labels (Entrance, Foyer, Kitchen, Isolation Wing, Boiler Room, Nursery, Lab, Attic), door interaction tags, and player spawn + objective markers. Keep furniture minimal for readability. Output: clean 3:2 PNG, 4096 px, crisp vector lines, no human figures, blueprint palette (ivory paper, dark ink), slight cinematic shadow for depth.
```

# 2 — Short quick prompt (fast test)

```
2-floor top-down horror game floor plan, blueprint style, many door interactions labeled (hinged, sliding, window, boarded, trapdoor, secret), stairs and elevator, room names, door icons, color-coded interactive legend, clean vector lines, 16:10, high detail.
```

# 3 — Isometric gameplay view (for level-blockout look)

```
Isometric 3D level-blockout of a 2-floor haunted manor, visible cutaway showing both floors, doors highlighted and labeled by type (hinged, sliding, sliding glass, boarded window, trapdoor), broken windows, narrow corridors, stair shafts, elevator, atmospheric lighting, moody horror color grading, low poly but detailed interactable icons.
```

# 4 — Door detail / icon sheet (closeups you can combine with plan)

```
Create a small sheet of door interaction icons and closeups: hinged wooden door (swing arc & key icon), sliding metal door (double arrow & friction text), reinforced door (padlock + steel), breakable window (shattered effect), boarded window (nails), trapdoor (dashed outline), secret bookshelf door (hidden seam). Each icon labeled with interaction text and recommended input (E / swipe / code / explosives).
```

# 5 — Alternate: side-by-side floors (if you prefer separate images)

```
Produce two separate top-down blueprint panels side-by-side: Left = Ground Floor plan; Right = Second Floor plan. Both include stairs/elevator alignment, doors annotated by type and interaction, room labels, legend, scale bar, and player/objective markers. High resolution, vector blueprint aesthetic.
```

### Tuning tips (fast)

- If you want _architectural precision_ add: `orthographic, CAD-level lineweight, scale 1:100, include measurement ticks`.
    
- To emphasize _gameplay interactions_ ask for: `annotations with short interaction text (e.g., "Slide: swipe", "Break: throwable object")`.
    
- To iterate faster: start with the short prompt → tweak `"color-coded overlays"` or `"legend placement"` → re-run the detailed prompt.
    
- Negative prompt ideas: `no people, avoid photorealistic textures, no photographic backgrounds` (helps keep it map/blueprint-like).
    

### How to get variations

- Change composition: “stacked” vs “side-by-side” vs “separate panels per floor”.
    
- Change style: `blueprint/vector` (clear for designers) or `isometric/blockout` (for visualizing 3D play).
    
- Ask for multiple export assets in one prompt: `Export: full plan + legend + door-icon sheet`.
    

Want me to pick one of these and shrink/tune it to a specific aspect ratio or engine UI (Unity minimap / tilemap grid)?




## make a diagram showing this neural network.
![[Pasted image 20251122204737.png]]![[Pasted image 20251122204748.png]]