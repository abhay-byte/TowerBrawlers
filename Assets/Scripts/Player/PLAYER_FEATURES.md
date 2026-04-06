# Player Controller Features

## New Features Added

### 1. Sprite Flipping
- **Auto-flips** based on horizontal movement direction
- Moving **left** → Sprite flips (`flipX = true`)
- Moving **right** → Sprite unflips (`flipX = false`)
- No manual setup needed - works automatically!

### 2. Attack Movement Lock
- **Movement is disabled** while attack animation plays
- Player **cannot move** until attack animation finishes
- Smooth gameplay - no canceling attacks mid-animation

---

## Setup Required: Animation Event

For the attack movement lock to work, you need to add an **Animation Event** to your attack animation:

### Steps:

1. **Open your Attack animation clip** in the Animation window
   - Find the animation file in your project (e.g., `Player_Attack.anim`)
   - Double-click to open

2. **Add Animation Event at the end**:
   - Scrub to the **last frame** of the attack animation
   - Click the **"Add Event"** button (icon looks like a small flag with +)
   - Or right-click on the timeline and select "Add Animation Event"

3. **Configure the Event**:
   - In the **Function** dropdown, select: `OnAttackAnimationEnd`
   - Leave other fields empty
   - Click **Close**

4. **Save** the animation clip

### Visual Guide:

```
[Animation Timeline]
|---Frame 1---|---Frame 5---|---Frame 10---|
                                  ↑
                          Add Event Here!
                          Function: OnAttackAnimationEnd
```

---

## Inspector Setup

Make sure your Player GameObject has:

✅ **SpriteRenderer** component (auto-detected)
✅ **Animator** with attack trigger
✅ **Rigidbody2D** (Kinematic recommended for top-down)
✅ **InputActionAsset** assigned to "Input Actions" field

The `SpriteRenderer` field will auto-populate if it's on the same GameObject.

---

## Testing

1. **Press Play**
2. **Move left/right** - sprite should flip automatically
3. **Press Attack** (Spacebar) - movement locks during animation
4. **Animation ends** - movement re-enables

---

## Troubleshooting

### Sprite Not Flipping?
- Check if `SpriteRenderer` is assigned in Inspector
- Verify you're moving horizontally (not just vertical)

### Movement Not Locking During Attack?
- **Most Common**: Missing Animation Event
- Check Console for errors
- Verify the attack animation has the `OnAttackAnimationEnd` event

### Attack Feels Delayed?
- Adjust your attack animation speed
- Or reduce `Attack Cooldown` in Inspector
