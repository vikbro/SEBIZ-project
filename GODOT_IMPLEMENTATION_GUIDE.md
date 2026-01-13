# Godot Web Build Integration - Implementation Guide

## Problems Found and Fixed

### 1. **Missing Godot Engine Initialization**
   - **Problem**: The original code just added a `<canvas>` tag and a script reference without initializing the Godot Engine
   - **Solution**: Implemented proper Godot Engine initialization with `GODOT_CONFIG` object before calling `engine.startGame()`

### 2. **Incorrect File Structure**
   - **Problem**: Game files were nested in `/games/web/web/` instead of `/games/web/`
   - **Solution**: Extracted and reorganized files so they're directly in `/games/web/` directory

### 3. **Canvas ID Mismatch**
   - **Problem**: Code referenced `#godot-canvas` but Godot's HTML5 export uses `#canvas`
   - **Solution**: Changed the canvas ID to match Godot's expected `id="canvas"`

### 4. **Missing Status Overlay**
   - **Problem**: No loading screen or progress indicator
   - **Solution**: Added the `#status` div with splash image and progress bar matching Godot's HTML structure

### 5. **Script Loading Race Condition**
   - **Problem**: Game script was loaded as JSX `<script>` tag which doesn't guarantee execution order
   - **Solution**: Dynamically create and append the script to ensure proper loading and configuration order

## How Godot HTML5 Export Works

Godot's HTML5 export requires:

1. **`index.js`** - The main engine loader (contains the Engine class)
2. **`index.wasm`** - WebAssembly binary for game execution
3. **`index.pck`** - Game data/resources packed file
4. **`index.png`** - Loading splash image
5. **HTML structure** with:
   - `<canvas id="canvas">` - The game canvas
   - `#status` div - Loading overlay
   - `#status-progress` - Progress bar
   - `#status-notice` - Error messages

## The Implementation (Play.tsx)

### Key Features:

1. **Proper Script Loading**
```typescript
const script = document.createElement('script');
script.id = 'godot-engine-script';
script.src = '/games/web/index.js';
```

2. **Engine Configuration**
```typescript
const GODOT_CONFIG = {
    args: [],
    canvasResizePolicy: 2,  // Auto-resize canvas
    ensureCrossOriginIsolationHeaders: true,
    executable: 'index',    // Matches index.wasm, index.pck
    experimentalVK: false,
    fileSizes: {
        'index.pck': 9168512,
        'index.wasm': 52126319,
    },
    focusCanvas: true,      // Auto-focus for input
    gdextensionLibs: [],
};
```

3. **Engine Initialization**
```typescript
const Engine = (window as any).Engine;
const engine = new Engine(GODOT_CONFIG);
engine.initProgressEvent(statusProgressElement);
engine.startGame();  // Starts the game
```

4. **Full-Screen Game Container**
- Game takes up full viewport (100vh)
- Black background matching Godot's default
- Back button positioned absolutely above the canvas
- No Tailwind to avoid CSS conflicts with Godot rendering

## File Structure

```
wwwroot/
└── games/
    └── web/                    ← All game files here
        ├── index.js            ← Engine loader
        ├── index.wasm          ← WebAssembly binary
        ├── index.pck           ← Game data
        ├── index.png           ← Splash image
        ├── index.icon.png      ← Favicon
        ├── index.html          ← Reference (not used)
        └── index.audio.worklet.js
```

## How to Handle Multiple Games (Future)

Currently, the game is hardcoded to `/games/web/`. When you have unique IDs per game:

```typescript
// Instead of:
script.src = '/games/web/index.js';

// Use:
script.src = `/games/${gameId}/index.js`;
```

And restructure your game storage:
```
wwwroot/games/
├── game-id-1/
│   ├── index.js
│   ├── index.wasm
│   └── index.pck
├── game-id-2/
│   ├── index.js
│   ├── index.wasm
│   └── index.pck
```

## Testing

1. Navigate to a game play page
2. Game canvas should load with splash image
3. Progress bar should appear
4. Game should initialize and run
5. Back button should navigate away

## Troubleshooting

### Game doesn't load
- Check browser console for errors
- Verify files exist at `/games/web/index.*`
- Check CORS headers are configured

### Canvas is blank
- Ensure `#canvas` element exists in DOM
- Check that Godot config matches your game's exported settings
- Verify fileSizes match actual file sizes

### Audio not working
- Verify `index.audio.worklet.js` is present
- Check browser audio autoplay policies

### Performance issues
- Adjust `canvasResizePolicy` (2 = auto-resize, 0 = fixed)
- Consider lower canvas resolution for low-end devices
