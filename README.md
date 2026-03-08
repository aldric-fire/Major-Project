# Major-Project-V1.0 — Cybersecurity Awareness Game

A multiplayer cybersecurity awareness game built in Unity using **Alteruna** for networking. Players explore a scene, press **E** on interactable objects (laptops, NPCs, USB drives, terminals), which opens challenge UI panels. They answer cybersecurity questions and are scored.

---

## Tech Stack

| | |
|---|---|
| **Engine** | Unity (URP) |
| **Multiplayer** | Alteruna (`com.unity.alteruna`) |
| **Input** | Unity New Input System (`com.unity.inputsystem` v1.17.0) |
| **UI** | uGUI Canvas (TextMeshPro) |
| **Language** | C# |

---

## Scripts Structure (`Assets/Scripts/`)

```
Scripts/
├── GameManager.cs                  — Game flow, summary screen
├── UIManager.cs                    — Menu/room panels (Alteruna lobby)
├── InputManager.cs                 — Routes Input System → player components
├── PlayerMovement.cs               — Rigidbody FPS movement (Alteruna-aware)
├── PlayerLook.cs                   — Mouse look, cursor lock/unlock
├── RemoteSmoothing.cs              — Interpolation for remote Alteruna players
├── NetworkedProp.cs                — Physics ownership for networked props
│
├── Interaction/
│   ├── IInteractable.cs            — Interface: Interact(), GetPromptText(), CanInteract()
│   └── InteractionController.cs   — Raycast from camera, detects IInteractable, calls TryInteract()
│
├── Challenges/
│   ├── ChallengeType.cs            — Enum: Phishing, Password, SocialEngineering, USBDrop
│   ├── ChallengeData.cs            — ScriptableObject: id, title, desc, options, emails
│   ├── ChallengeOption.cs          — Serializable: option text, isCorrect, feedbackNarrative
│   ├── ChallengeResult.cs          — Serializable: tracks pass/fail result
│   ├── ChallengeManager.cs         — Singleton: starts challenges, evaluates answers, fires events
│   │                                 Has 'uiPanels' list (drag disabled panels in Inspector)
│   ├── DebriefPanel.cs             — Post-failure educational debrief panel
│   └── Stations/
│       ├── PhishingStation.cs      — IInteractable → opens PhishingUI
│       ├── PasswordStation.cs      — IInteractable → opens PasswordUI
│       ├── USBPickup.cs            — IInteractable → opens ChoiceUI
│       └── SocialEngineeringNPC.cs — IInteractable → opens DialogueUI
│
├── UI/
│   ├── ChallengeUIBase.cs          — Abstract base: Open(), Close(), PopulateUI(), SubmitChoice()
│   ├── PhishingUI.cs               — Email list + arrow navigator (< option >) + submit
│   ├── PasswordUI.cs               — Password challenge panel
│   ├── DialogueUI.cs               — Social engineering dialogue panel
│   └── ChoiceUI.cs                 — USB drop choice panel
│
├── Progress/
│   ├── PlayerProgress.cs           — Alteruna RPC sync for challenge results across clients
│   └── ScoreboardDisplay.cs        — Whiteboard display of player progress
│
└── Consequences/
    └── ConsequenceManager.cs       — Environmental effects on failure (alerts, ransomware, etc.)
```

---

## Input Bindings (`Assets/Input/PlayerInput.inputactions`)

| Action | Key |
|--------|-----|
| Movement | WASD / Left Stick |
| Jump | Space / Gamepad South |
| Look | Mouse Delta / Right Stick |
| ShiftLock | Left Shift |
| **Interact** | **E / Gamepad West** |

---

## Prefabs (`Assets/Prefabs/`)

| Prefab | Purpose |
|--------|---------|
| `Player.prefab` | Main player — has `InputManager`, `PlayerMovement`, `PlayerLook`, `InteractionController`, `Alteruna.Avatar` |
| `Player Variant.prefab` | Variant of Player prefab |
| `EmailEntryRow.prefab` | Email row used by PhishingUI — has Button, TextMeshProUGUI (EmailRow child), Toggle (FlagToggle child) |

---

## Unity Scene Setup (manually configured in Editor)

| GameObject | Components | Notes |
|------------|-----------|-------|
| `HUDCanvas` | Canvas (Screen Space Overlay), `HUDController` | Holds all in-game UI |
| `HUDCanvas/InteractPromptPanel` | Panel, **disabled by default** | Shows "Press E to …" text |
| `HUDCanvas/InteractPromptPanel/PromptText` | TextMeshProUGUI | Wired to HUDController |
| `HUDCanvas/PhishingUIPanel` | Panel, `PhishingUI`, **disabled by default** | Opens when player presses E on laptop |
| `HUDCanvas/PhishingUIPanel/OptionNavigator` | Panel at bottom | Holds `<` `>` buttons + option text + counter |
| `ChallengeManager` | Empty GameObject, `ChallengeManager` script | `uiPanels` list has PhishingUIPanel; `allChallenges` list has Phishing_01 asset |
| Laptop object | Mesh, Collider, `PhishingStation`, Layer: `Interactable` | Interactable object in scene |

---

## Layers

| Layer | Index | Purpose |
|-------|-------|---------|
| `Interactable` | User Layer 3 | Assigned to all interactable objects; used by `InteractionController`'s LayerMask |

---

## "E to Interact" Flow

```
Player looks at object (Layer: Interactable, has IInteractable + Collider)
    ↓
InteractionController raycasts each frame → finds IInteractable
    ↓
HUDController.ShowInteractPrompt("Press E to Inspect Computer")
    ↓
Player presses E → InputManager fires → InteractionController.TryInteract()
    ↓
PhishingStation.Interact(player) → ChallengeManager.StartChallenge(data, controller)
    ↓
InteractionController.EnterChallengeUI() → freezes player, unlocks cursor
    ↓
PhishingUI.Open(data) → panel activates, emails populate, option navigator resets
    ↓
Player cycles options with < > arrows → clicks Submit
    ↓
ChallengeManager.SubmitAnswer() → evaluates, fires events, triggers consequences
    ↓
PhishingUI.Close() → ChallengeManager.OnChallengeUIClosed() → InteractionController.ExitChallengeUI()
    ↓
Player movement restored, cursor locked — can press E again (no replay restriction)
```

---

## Key Design Decisions

- **Stations are always replayable** — `CanInteract()` only blocks if `challengeData` is null
- **PhishingUI uses arrow navigator** (`< option >`) instead of toggles for picking answers
- **Disabled UI panels** are found via the `uiPanels` list on `ChallengeManager` (bypasses Unity's `FindObjectsByType` limitation with disabled objects)
- **EmailEntryRow prefab** structure: Button on root → `EmailRow` (TMP text child) + `FlagToggle` (Toggle on separate child)
- **Player is spawned at runtime** by Alteruna — all scripts must be on the `Player.prefab`, not placed directly in the scene

---

---

# V1.1 — Unity Editor Configuration Reference

> **Note for agents:** Unity Hub and the Unity Editor cannot be accessed programmatically. Everything in this section was manually configured inside the Unity Editor. This documents exactly what was set up so agents have full context.

---

## 1. Layer Setup
**Location:** Edit → Project Settings → Tags and Layers

| Layer Name | Index |
|------------|-------|
| `Interactable` | User Layer 3 |

All interactable objects in the scene must have their **Layer** set to `Interactable`. The `InteractionController` raycast uses this layer mask to detect them.

---

## 2. Player Prefab (`Assets/Prefabs/Player.prefab`)

Opened via double-click in Project panel (Prefab editing mode). The player is **spawned at runtime by Alteruna** — not placed in the scene directly.

### Components on Player root GameObject:
| Component | Key Settings |
|-----------|-------------|
| `Alteruna.Avatar` | (Alteruna built-in, already present) |
| `TransformSynchronizable` | (Alteruna built-in, already present) |
| `PlayerMovement` | Added manually |
| `PlayerLook` | Added manually; field `cam` → assigned to the Camera child |
| `InteractionController` | Added manually; **Interact Range: `3`**, **Interact Layer Mask: `Interactable` only** |
| `InputManager` | Added manually (auto-requires the above 3) |

### Camera child:
- A **Camera** GameObject is a child of the Player root
- Tag: **MainCamera**
- Assigned to `PlayerLook.cam` field in Inspector

---

## 3. Laptop Object (Scene)

Any mesh/model in the scene that the player should be able to interact with.

| Setting | Value |
|---------|-------|
| **Layer** | `Interactable` |
| **Collider** | Box Collider or Mesh Collider — **Is Trigger: OFF** |
| **Script** | `PhishingStation` component attached |
| **PhishingStation → Challenge Data** | `Phishing_01` asset dragged in |

---

## 4. ChallengeData Asset (`Phishing_01`)

**Created via:** Right-click in Project panel → **Create → Cybersecurity → Challenge Data**
**Saved at:** `Assets/` (or a subfolder of your choice)

### Inspector values:

| Field | Value |
|-------|-------|
| Challenge Id | `phishing_01` |
| Title | `Suspicious Email` |
| Description | `You stumble across a suspicious email` |
| Challenge Type | `Phishing` |
| Difficulty Tier | `1` |

### Options (3 entries):

| # | Text | Is Correct | Feedback Narrative |
|---|------|-----------|-------------------|
| 0 | `Click the link and reset your password` | ❌ | `You clicked a phishing link! Your credentials have been stolen.` |
| 1 | `Report the email to IT and delete it` | ✅ | `Correct! The email had a suspicious sender address and urgency tactics — classic phishing.` |
| 2 | `Forward it to a colleague to verify` | ❌ | `By forwarding it, you've now exposed a colleague to the same phishing attack.` |

### Emails (2 entries):

**Email 1 — Phishing:**
| Field | Value |
|-------|-------|
| Sender Name | `IT Security Team` |
| Sender Address | `security@c0mpany-support.xyz` |
| Subject | `URGENT: Reset Your Password Now` |
| Body | `Dear Employee, Your account has been flagged for unusual activity. Click the link below immediately to reset your password or your account will be locked within 24 hours. [Reset Password]` |
| Is Phishing | ✅ |
| Phishing Clues | `Misspelled domain (c0mpany with a zero), urgency/threats, generic greeting, suspicious link` |

**Email 2 — Legitimate:**
| Field | Value |
|-------|-------|
| Sender Name | `HR Department` |
| Sender Address | `hr@yourcompany.com` |
| Subject | `Team lunch this Friday` |
| Body | `Hi everyone! Just a reminder about the team lunch this Friday at 12pm in the break room.` |
| Is Phishing | ❌ |

### Other fields:
| Field | Value |
|-------|-------|
| Consequence Narrative | `Hackers gained access to your credentials. Company data is being exfiltrated.` |
| Consequence Type | `DataBreach` |
| Debrief Text | `Phishing emails often create urgency to trick you into acting without thinking. Always check the sender address, hover over links before clicking, and report suspicious emails to IT.` |
| Real World Stat | `91% of cyberattacks begin with a phishing email (Deloitte, 2023)` |

---

## 5. HUD Canvas (`HUDCanvas`)

**Created via:** Right-click in Hierarchy → UI → Canvas

### HUDCanvas root:
| Component | Settings |
|-----------|---------|
| `Canvas` | Render Mode: **Screen Space - Overlay** |
| `CanvasScaler` | UI Scale Mode: **Scale With Screen Size** |
| `HUDController` | Fields assigned — see below |

### HUDController Inspector assignments:
| Field | Assigned To |
|-------|------------|
| Interact Prompt Panel | `InteractPromptPanel` |
| Interact Prompt Text | `PromptText` (TMP inside InteractPromptPanel) |
| Objective Panel | *(leave empty for now)* |
| Notification Bar | *(leave empty for now)* |

---

## 6. HUD Canvas Hierarchy

```
▼ HUDCanvas
    ├── InteractPromptPanel          (Panel — DISABLED by default)
    │     └── PromptText             (TextMeshProUGUI — "Press E to Interact", size 24, white, centered)
    │
    └── PhishingUIPanel              (Panel — DISABLED by default, has PhishingUI script)
          ├── MainCard               (Panel — dark navy, centered ~950x620)
          │     ├── Header           (Panel — blue #3264C8, height 70, anchor: top-stretch)
          │     │     ├── TitleText  (TMP — size 28, bold, white, left-padded)
          │     │     └── CloseButton (Button — top-right, icon only, calls PhishingUI.Close())
          │     ├── EmailScrollView  (ScrollView — left 45% of card, vertical scroll only)
          │     │     └── Content    (= EmailListContainer, has VerticalLayoutGroup + ContentSizeFitter)
          │     ├── RightPane        (Panel — right 55%, darker background)
          │     │     └── EmailDetailPanel (Panel — DISABLED by default)
          │     │           ├── DetailSenderText   (TMP — size 14, grey)
          │     │           ├── DetailSubjectText  (TMP — size 18, bold, white)
          │     │           └── DetailBodyText     (TMP — size 14, white, scrollable)
          │     ├── OptionNavigator  (Panel — bottom strip, height 80)
          │     │     ├── PrevButton   (Button — "<", left side, ~80px wide)
          │     │     ├── OptionText   (TMP — center, filled by script)
          │     │     ├── CounterText  (TMP — small, "1 / 3", above OptionText)
          │     │     └── NextButton   (Button — ">", right side, ~80px wide)
          │     └── Footer           (Panel — bottom strip, height 60)
          │           ├── FeedbackText  (TMP — left side, yellow #FFDC32)
          │           └── SubmitButton  (Button — right side, green #329650, text "Submit")
```

### PhishingUI Inspector assignments:
| Field | Assigned To |
|-------|------------|
| Email List Container | `Content` (inside EmailScrollView) |
| Email Entry Prefab | `EmailEntryRow.prefab` (from Assets/Prefabs/) |
| Email Detail Panel | `EmailDetailPanel` |
| Detail Sender Text | `DetailSenderText` |
| Detail Subject Text | `DetailSubjectText` |
| Detail Body Text | `DetailBodyText` |
| Submit Button | `SubmitButton` |
| Feedback Text | `FeedbackText` |
| Title Text | `TitleText` |
| Description Text | *(optional TMP for description)* |
| Prev Option Button | `PrevButton` |
| Next Option Button | `NextButton` |
| Option Display Text | `OptionText` |
| Option Counter Text | `CounterText` |

### CloseButton onClick event:
- Object: `PhishingUIPanel`
- Function: `PhishingUI → Close()`

---

## 7. ChallengeManager GameObject

**Created via:** Right-click in Hierarchy → Create Empty → renamed `ChallengeManager`

| Component | Field | Value |
|-----------|-------|-------|
| `ChallengeManager` | All Challenges (list, size 1) | Element 0 → `Phishing_01` asset |
| `ChallengeManager` | UI Panels (list, size 1) | Element 0 → `PhishingUIPanel` (dragged from Hierarchy) |

> **Critical:** The `uiPanels` list must have `PhishingUIPanel` dragged in **even though it is disabled**. This is how `ChallengeManager` finds it at runtime without Unity's `FindObjectsByType` (which skips disabled objects).

---

## 8. EmailEntryRow Prefab (`Assets/Prefabs/EmailEntryRow.prefab`)

**Created via:** Right-click in Hierarchy → UI → Button - TextMeshPro, then customised and dragged into Assets/Prefabs/

### Structure:
```
▼ EmailEntryRow              (Button + Image + HorizontalLayoutGroup)
    ├── EmailRow             (TextMeshProUGUI — size 14, white, flexible width)
    └── FlagToggle           (Empty child GameObject with Toggle component)
```

### Settings:
| Setting | Value |
|---------|-------|
| Root height | `60` |
| Root Image color | `RGB(45, 45, 60)` dark |
| HorizontalLayoutGroup padding | Left: 15, Right: 10 |
| HorizontalLayoutGroup child alignment | Middle Left |
| EmailRow TMP | Size 14, white, flexible width |
| FlagToggle fixed width | ~120px |

> `PhishingUI` uses `GetComponentInChildren<TextMeshProUGUI>()` to find `EmailRow` and `GetComponentInChildren<Toggle>()` to find `FlagToggle`. The Toggle cannot be on the same GameObject as the Button (Unity only allows one Selectable per GameObject), which is why FlagToggle is a child.

---

## 9. Color Reference (UI Theme)

| Element | RGBA |
|---------|------|
| Full-screen overlay | `(0, 0, 0, 180)` |
| Card background | `(30, 30, 40, 255)` |
| Header accent | `(50, 100, 200, 255)` blue |
| Email row background | `(45, 45, 60, 255)` |
| Right pane background | `(20, 20, 30, 255)` |
| Submit button | `(50, 150, 80, 255)` green |
| Feedback text | `(255, 220, 50, 255)` yellow |
| Body text | `(255, 255, 255, 255)` white |
| Subtext / labels | `(170, 170, 190, 255)` grey |

