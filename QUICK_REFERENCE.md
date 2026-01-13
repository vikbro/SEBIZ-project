# Quick Summary: Godot Web Build Issues & Fixes

## The 5 Main Problems ❌➡️✅

### 1. No Engine Initialization
**Problem:** Canvas + Script tag ≠ Running game
```tsx
// ❌ Before (doesn't work)
<canvas id="godot-canvas" />
<script src="/games/{id}/index.js" />

// ✅ After (proper initialization)
script.onload = () => {
    const engine = new Engine(GODOT_CONFIG);
    engine.startGame();
};
```

### 2. Wrong Canvas ID
**Problem:** Godot exports with `id="canvas"`, not `id="godot-canvas"`
```tsx
// ❌ Before
<canvas id="godot-canvas" />

// ✅ After
<canvas id="canvas" />
```

### 3. Files Nested Wrong
**Problem:** Files in `/games/web/web/` but served from `/games/web/`
```
❌ /games/web/web/index.js
✅ /games/web/index.js
```

### 4. Missing Status UI
**Problem:** No loading overlay or progress feedback
```tsx
// ✅ Added
<div id="status">
    <img id="status-splash" src="/games/web/index.png" />
    <progress id="status-progress" />
</div>
```

### 5. Script Loading Race
**Problem:** JSX `<script>` tag doesn't guarantee execution order
```tsx
// ✅ Fixed with dynamic creation
const script = document.createElement('script');
script.onload = () => { /* initialize */ };
document.body.appendChild(script);
```

---

## Essential File Structure

```
wwwroot/games/web/
├── index.js              ← Engine loader
├── index.wasm            ← Game executable
├── index.pck             ← Game data
├── index.png             ← Splash image
└── index.audio.worklet.js ← Audio support
```

---

## The GODOT_CONFIG Object

```typescript
const GODOT_CONFIG = {
    args: [],
    canvasResizePolicy: 2,              // Auto-resize canvas
    ensureCrossOriginIsolationHeaders: true,
    executable: 'index',                // Matches files: index.pck, index.wasm
    experimentalVK: false,
    fileSizes: {                        // MUST match your files
        'index.pck': 9168512,
        'index.wasm': 52126319,
    },
    focusCanvas: true,                  // Auto-focus for input
    gdextensionLibs: [],
};
```

---

## Initialization Flow

```
1. Script loads (index.js)
   ↓
2. Engine class becomes available on window
   ↓
3. Create new Engine(GODOT_CONFIG)
   ↓
4. engine.initProgressEvent(progressBar)
   ↓
5. engine.startGame()
   ↓
6. Game runs in <canvas id="canvas">
```

---

## For Multiple Games (Later)

Structure:
```
wwwroot/games/
├── game-001/index.js
├── game-001/index.wasm
├── game-001/index.pck
├── game-002/index.js
├── game-002/index.wasm
├── game-002/index.pck
```

Update code:
```typescript
script.src = `/games/${gameId}/index.js`;
fileSizes: {
    'index.pck': getFileSizeForGame(gameId, 'pck'),
    'index.wasm': getFileSizeForGame(gameId, 'wasm'),
}
```

---

## Testing

```
✅ Game splash image loads
✅ Progress bar appears during initialization
✅ Game canvas renders at full-screen
✅ Input works (click, keyboard, touch)
✅ Back button navigates away
✅ No console errors
```

---

## Files Modified

- [Play.tsx](frontend/MongoDbProductClient/src/Component/Play.tsx) - Complete rewrite with proper Godot initialization
- Game files extracted to `/SEBIZ/wwwroot/games/web/` directory

---

## What's Already Working ✅

- CORS headers configured (AllowAnyOrigin)
- Static files middleware configured
- `/games/` path properly served via wwwroot
- JWT authentication working
- Backend API routes accessible
