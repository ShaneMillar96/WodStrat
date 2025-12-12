# WodStrat – UI Wireframes (Low-Fidelity / Text-Based)

Below are text-based UI wireframes for WodStrat. These are low-fidelity wireframes, similar to early design sketches you'd hand to a designer or frontend dev. They map exactly to the USERFLOW.md and provide a clear starting point for layout + component decisions.

> These wireframes represent **structure**, not visual styling. They illustrate page layout, main elements, buttons, fields, and navigation.

---

## 1. Global Navigation (Top-Level)

```
┌─────────────────────────────────────────────────────────────┐
│ WodStrat │ Dashboard │ Profile │ Benchmarks │ Strategy │ ▼ │
└─────────────────────────────────────────────────────────────┘
```

> Future dropdown ("▼") can hold Settings, Logout, Integrations.

---

## 2. Dashboard (Future, placeholder for now)

```
┌─────────────────────────────────────────────────────────────┐
│                    DASHBOARD (MVP Optional)                 │
├─────────────────────────────────────────────────────────────┤
│ Welcome back, <AthleteName>                                 │
│                                                             │
│ [ Paste Today's WOD ]  → takes user directly to Strategy    │
│                                                             │
│ Quick Links:                                                │
│  - Update Benchmarks                                        │
│  - View Recent Workouts (Future)                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Athlete Profile Page

```
┌─────────────────────────────────────────────────────────────┐
│                     ATHLETE PROFILE                         │
├─────────────────────────────────────────────────────────────┤
│ Personal Info                                               │
│ ─────────────────────────────────────────────────────────── │
│ Name:        [____________________]                         │
│ Age:         [____]                                         │
│ Height (cm): [____]                                         │
│ Weight (kg): [____]                                         │
│ Experience:  ( ) Beginner  ( ) Intermediate  ( ) Advanced   │
│                                                             │
│ Goals (optional):                                           │
│ [ Improve pacing ] [ Prepare for Open ] [ HYROX training ]  │
│ [ General fitness ]                                         │
│                                                             │
│   [ Save Profile ]     [ Reset / Clear ]                    │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Benchmarks List Page

```
┌─────────────────────────────────────────────────────────────┐
│                         BENCHMARKS                          │
├─────────────────────────────────────────────────────────────┤
│  [ + Add Benchmark ]                                        │
├─────────────────────────────────────────────────────────────┤
│ Benchmark Name       │ Type     │ Value          │ Date     │
│ ─────────────────────┼──────────┼────────────────┼───────── │
│ Max Pull-Ups         │ Reps     │ 12             │ 2024-01  │
│ 500m Row             │ Time     │ 1:42           │ 2024-01  │
│ Clean & Jerk         │ Load     │ 85kg           │ 2024-01  │
│ Fran                 │ Time     │ 6:45           │ 2023-12  │
├─────────────────────────────────────────────────────────────┤
│ (click row to edit/remove)                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Add / Edit Benchmark Modal

```
┌─────────────────────────────────────────────────────────────┐
│                    ADD BENCHMARK                            │
├─────────────────────────────────────────────────────────────┤
│ Benchmark Type:  [ Dropdown ▼ ]                             │
│ (Preset examples: Pull-ups, 500m row, 1RM deadlift, Fran…)  │
│                                                             │
│ Value Input:                                                │
│ - If Time: [ mm : ss ]                                      │
│ - If Weight: [ ___ kg ]                                     │
│ - If Reps: [ ___ ]                                          │
│                                                             │
│ Notes (optional): [__________________________]              │
│                                                             │
│    [ Save ]       [ Cancel ]                                │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. Strategy Page – Paste WOD Screen

```
┌─────────────────────────────────────────────────────────────┐
│                         STRATEGY                            │
├─────────────────────────────────────────────────────────────┤
│ Step 1 — Paste your WOD                                     │
│ ─────────────────────────────────────────────────────────── │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │  For time:                                              │ │
│ │  21-15-9                                                │ │
│ │  Thrusters (42.5/30kg)                                  │ │
│ │  Pull-Ups                                               │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ Time Cap (optional): [ ____ minutes ]                       │
│                                                             │
│ [ Generate Strategy ]                                       │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. Strategy Page – After Parsing (Preview of Structured Workout)

```
┌─────────────────────────────────────────────────────────────┐
│                  WORKOUT PARSED (PREVIEW)                   │
├─────────────────────────────────────────────────────────────┤
│ Workout Type:       FOR TIME                                │
│ Rounds:             3 (21-15-9)                             │
│ Movements:                                                  │
│   - Thrusters @ 42.5/30kg                                   │
│   - Pull-Ups                                                │
│ Estimated Domain:   Short (6–10 min)                        │
│                                                             │
│ [ Generate Strategy ]                                       │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Strategy Page – AI / Rule-Based Strategy Output

```
┌─────────────────────────────────────────────────────────────┐
│                   PERSONALIZED STRATEGY                     │
├─────────────────────────────────────────────────────────────┤
│ Summary                                                     │
│ ─────────────────────────────────────────────────────────── │
│ "Aim for controlled aggression. Break thrusters early to    │
│ avoid grip blow-up before pull-ups. Expect to finish        │
│ between 6:30–7:30."                                         │
│                                                             │
│ Detailed Recommendations                                    │
│ ─────────────────────────────────────────────────────────── │
│ Thrusters:                                                  │
│   21 → 12/9                                                 │
│   15 → 8/7                                                  │
│   9  → unbroken                                             │
│                                                             │
│ Pull-Ups:                                                   │
│   21 → 8/7/6                                                │
│   15 → 6/5/4                                                │
│   9  → 5/4                                                  │
│                                                             │
│ Pacing Guide:                                               │
│   Stay below 90% HR until midway through set of 15.         │
│   Maintain steady breathing cadence during thrusters.       │
│                                                             │
│ Risks & Notes:                                              │
│   - Grip fatigue likely high                                │
│   - Don't push round of 21 too hard                         │
│                                                             │
│ Scaled Options:                                             │
│   - Thrusters @ 30kg / 20kg                                 │
│   - Banded pull-ups                                         │
│                                                             │
│ [ Regenerate ]   [ Save Strategy ] (future)                 │
└─────────────────────────────────────────────────────────────┘
```

> This layout anticipates AI enhancements without forcing UI changes later.

---

## 9. Strategy History Page (Future)

```
┌─────────────────────────────────────────────────────────────┐
│                        WORKOUT HISTORY                      │
├─────────────────────────────────────────────────────────────┤
│ Date       │ Workout Title         │ Result    │ Strategy   │
│ ───────────┼───────────────────────┼───────────┼─────────── │
│ 2024-01-14 │ 21-15-9 Thruster/PUs  │ 7:12 RX   │ View       │
│ 2024-01-12 │ EMOM 10 clean pulls   │ Completed │ View       │
│ 2024-01-10 │ 2k Row Test           │ 7:47      │ View       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ (Pagination)                                                │
└─────────────────────────────────────────────────────────────┘
```

---

## 10. Strategy Details Page (From History) – Future

```
┌─────────────────────────────────────────────────────────────┐
│                  WORKOUT DETAILS (HISTORY)                  │
├─────────────────────────────────────────────────────────────┤
│ Original Workout                                            │
│ ─────────────────────────────────────────────────────────── │
│ For time: 21-15-9 thrusters & pull-ups                      │
│                                                             │
│ Your Result: 7:12 RX                                        │
│ Predicted: 6:30–7:30                                        │
│                                                             │
│ Performance Comparison                                      │
│ ─────────────────────────────────────────────────────────── │
│ Round 21: predicted 2:20 → actual 2:40                      │
│ Round 15: predicted 2:10 → actual 2:15                      │
│ Round 9 : predicted 1:40 → actual 1:30                      │
│                                                             │
│ AI Strategy (historic snapshot)                             │
│ ─────────────────────────────────────────────────────────── │
│ (original strategy content shown here)                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 11. Mobile Wireframe Examples (Simplified)

### Strategy Input (Mobile)

```
┌─────────────────────────┐
│ Paste today's WOD       │
│ ─────────────────────── │
│ [ text box             ]│
│                         │
│ Time cap: [ __ ] mins   │
│                         │
│ [ Generate Strategy ]   │
└─────────────────────────┘
```

### Strategy Output (Mobile)

```
┌───────────────────────────┐
│ Summary                   │
│ "Finish in 6:45–7:30…"    │
├───────────────────────────┤
│ Thrusters 21 → 12/9       │
│ Pull-Ups 21 → 8/7/6       │
├───────────────────────────┤
│ [ Show Details ▼ ]        │
│ [ Regenerate ]            │
└───────────────────────────┘
```

---

## 12. Component Inventory (For Dev Reference)

### Shared Components

- **Navbar** – Top navigation bar
- **Button** – Primary/secondary variants
- **Card container** – Content wrapper
- **Modal** – Dialog overlay
- **Table** – Data display
- **TextArea** – Multi-line input
- **Time/Weight/Reps input widgets** – Specialized inputs
- **Strategy accordion sections** – Expandable content

### Page-Specific Components

- **Profile form inputs** – Athlete profile fields
- **Benchmark add/edit modal** – CRUD for benchmarks
- **WOD input module** – Workout text entry
- **Parsed-wod preview card** – Structured workout display
- **Strategy output blocks** – AI/rule-based recommendations
- **History list rows** – Past workout entries