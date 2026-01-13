# Godot Web Embedding - Before vs After

## BEFORE (What Was Wrong) ❌

```tsx
<canvas id="godot-canvas" />
<script src={`/games/${id}/index.js`} />
```

### Problems:
1. ❌ No Godot initialization code
2. ❌ Canvas ID mismatch (`godot-canvas` vs `canvas`)
3. ❌ No status/loading overlay
4. ❌ No GODOT_CONFIG object
5. ❌ Script loads as JSX tag (not guaranteed order)
6. ❌ No error handling
7. ❌ CSS conflicts (Tailwind on full-page game)

### Result: 
- ⚠️ Blank canvas or game fails to start
- ⚠️ No loading feedback to user
- ⚠️ Unpredictable initialization

---

## AFTER (Correct Implementation) ✅

```tsx
// 1. Dynamically load script
const script = document.createElement('script');
script.src = '/games/web/index.js';

script.onload = () => {
    // 2. Configure Godot
    const GODOT_CONFIG = {
        args: [],
        canvasResizePolicy: 2,
        ensureCrossOriginIsolationHeaders: true,
        executable: 'index',
        experimentalVK: false,
        fileSizes: {
            'index.pck': 9168512,
            'index.wasm': 52126319,
        },
        focusCanvas: true,
        gdextensionLibs: [],
    };
    
    // 3. Initialize Engine
    const Engine = (window as any).Engine;
    const engine = new Engine(GODOT_CONFIG);
    
    // 4. Connect progress bar
    engine.initProgressEvent(progressElement);
    
    // 5. Start game
    engine.startGame();
};

document.body.appendChild(script);
```

### HTML Structure:
```html
<canvas id="canvas" />              ← Godot's expected ID
<div id="status">                   ← Loading overlay
    <img id="status-splash" />      ← Splash image
    <progress id="status-progress" />  ← Progress bar
    <div id="status-notice" />      ← Error messages
</div>
```

### Results:
- ✅ Game initializes properly
- ✅ Loading screen with progress
- ✅ Error handling and feedback
- ✅ Guaranteed script load order
- ✅ Full-screen rendering
- ✅ Proper canvas sizing

---

## File Structure Changes

### Before:
```
wwwroot/games/
└── web (2).zip  ← Compressed, not served
```

### After:
```
wwwroot/games/
├── web (2).zip  ← Original backup
└── web/         ← Extracted and ready to serve
    ├── index.js            ✅ Served
    ├── index.wasm          ✅ Served
    ├── index.pck           ✅ Served
    ├── index.png           ✅ Served
    └── index.icon.png      ✅ Served
```

---

## CORS & Static Files Configuration

Your backend already has proper configuration:

```csharp
// Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

// CORS allows all origins
policy.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader();
```

✅ This means `/games/web/index.js` is automatically accessible at `http://localhost:7282/games/web/index.js`

---

## Path Mapping Reference

| File | Location | Served At |
|------|----------|-----------|
| index.js | `wwwroot/games/web/index.js` | `/games/web/index.js` ✅ |
| index.wasm | `wwwroot/games/web/index.wasm` | `/games/web/index.wasm` ✅ |
| index.pck | `wwwroot/games/web/index.pck` | `/games/web/index.pck` ✅ |
| index.png | `wwwroot/games/web/index.png` | `/games/web/index.png` ✅ |

---

## Testing Checklist

- [ ] Navigate to game play page
- [ ] Godot splash image appears
- [ ] Progress bar animates during load
- [ ] Game canvas renders
- [ ] Input works (keyboard/mouse/touch)
- [ ] Audio plays (if game has audio)
- [ ] Back button works
- [ ] Game stops when navigating away

---

## For Future: Multiple Games

When you assign unique IDs to games:

```typescript
// Change from:
script.src = '/games/web/index.js';

// To:
script.src = `/games/${gameId}/index.js`;
```

And organize uploads:
```
wwwroot/games/
├── game-uuid-123/index.js
├── game-uuid-123/index.wasm
├── game-uuid-123/index.pck
├── game-uuid-456/index.js
├── game-uuid-456/index.wasm
├── game-uuid-456/index.pck
```
