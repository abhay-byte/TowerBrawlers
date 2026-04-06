# 07. UI, UX, and Pages

## Goal

Design a UI that is easy to read on mobile, clean on desktop, and playful enough to match the parody tone without becoming visually messy.

## 1. UX Principles

- one-glance readability
- large tap targets
- minimal menu depth
- strong faction flavor
- important information always visible
- jokes in the writing, not in the usability

## 2. Navigation Structure

The game should feel lightweight to navigate.

Recommended primary flow:

1. Splash
2. Title Screen
3. Main Menu
4. World Map
5. Pre-Battle Loadout
6. Battle Screen
7. Results Screen
8. Rewards or Upgrade Screen

## 3. Number of Main Pages

Recommended MVP page count: `8 core pages`

## 4. Page List

## 4.1 Splash Screen

Purpose:

- branding
- quick loading transition

Required elements:

- studio logo
- game logo
- short loading animation

## 4.2 Title Screen

Purpose:

- first impression
- tone setup

Required elements:

- animated towers in background
- comedic subtitle or rotating line
- tap to start
- settings shortcut

Example subtitle:

"A strategy game of architecture, outrage, and preventable escalation."

## 4.3 Main Menu

Purpose:

- central navigation hub

Required elements:

- Continue
- New Game
- Loadout
- Settings
- Credits

Optional elements:

- daily silly news ticker
- last unlocked item preview

## 4.4 World Map

Purpose:

- choose campaign missions
- show progression

Required elements:

- chapter path
- mission nodes
- rewards preview
- current chapter banner
- faction commentary

UX notes:

- missions should be selectable in one tap
- stars and rewards should be visible without opening submenus

## 4.5 Pre-Battle Loadout Screen

Purpose:

- choose troops
- choose items
- review mission gimmick

Required elements:

- selected faction
- troop cards
- item slots
- mission tips
- start battle button

UX notes:

- show role icons clearly
- avoid long stat walls
- use short playful descriptions

## 4.6 Battle Screen

Purpose:

- real-time gameplay

Required elements:

- player tower health
- enemy tower health
- Snack counter
- Drama meter
- phase indicator
- troop cards
- ability button
- build slot buttons
- pause button

Layout guidance:

- top bar: tower health and phase
- bottom bar: troop cards and ability
- side or bottom-right: structure build slots

Mobile rule:

- do not place critical controls near gesture-conflict edges

## 4.7 Results Screen

Purpose:

- show outcome
- deliver rewards
- allow retry or continue

Required elements:

- victory or defeat banner
- Coins earned
- item rewards
- funny summary line
- retry button
- continue button

Example summary line:

"Your diplomacy was loud, expensive, and surprisingly effective."

## 4.8 Upgrade or Reward Screen

Purpose:

- spend rewards
- unlock new content

Required elements:

- troop unlocks
- structure unlocks
- item unlocks
- upgrade descriptions

UX notes:

- keep upgrades grouped by category
- always show whether an unlock affects battle or meta progression

## 5. Optional Future Pages

These are not required for MVP but may be added later:

- Codex screen for lore entries
- Trophy room
- Skirmish custom setup
- Cosmetic collection page
- Replay viewer

## 6. Battle HUD Detailed Spec

### Top Area

- player tower HP left
- enemy tower HP right
- center phase timer

### Bottom Area

- troop cards
- current cost labels
- active ability button
- cooldown feedback

### Contextual Elements

- build slot highlights
- tooltip panels
- tutorial callouts
- damage popups

## 7. UI Style Direction

- bright, bold colors
- exaggerated icon shapes
- parchment and painted-sign textures
- faction-specific frames
- easy-to-read fonts

Do not use:

- tiny text
- cluttered fantasy ornament everywhere
- overly realistic medieval UI styling

## 8. Accessibility Requirements

- scalable text
- color plus icon coding
- strong contrast for health and resources
- subtitles for voiced jokes and cutscenes
- tutorial highlights with clear focus states

## 9. UX Success Criteria

The UI succeeds if players:

- understand the battle state at a glance
- can deploy units quickly on a phone
- know where to go next after a win
- remember the game as playful, not confusing
