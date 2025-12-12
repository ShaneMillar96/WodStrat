# WodStrat – User Flow

This document describes the end-to-end user flows for WodStrat: how an athlete discovers, sets up, and uses the app to generate personalized workout strategies and review their training over time.

The flows are written to cover the intended MVP+ experience (profile, benchmarks, WOD strategy, basic history), even if not all features are implemented yet. They're meant to guide backend, frontend, and UX decisions.

---

## 1. User Personas

### 1.1 Primary Persona – The Hybrid Functional Fitness Athlete

- Attends a CrossFit/functional fitness gym regularly
- Occasionally competes (CrossFit Open, HYROX, local comps)
- Wants to improve pacing, consistency, and workout strategy
- Already uses some tracking tools (SugarWOD, BTWB, spreadsheets, Garmin, etc.)

### 1.2 Secondary Persona – The Coach / Programming Lead (Future)

- Programs daily WODs for a gym or remote clients
- Wants to give athletes structured pacing and strategy notes
- Might manage multiple athletes inside WodStrat

> Initial flows focus mainly on the primary athlete persona.

---

## 2. High-Level App Flow

At a high level, the WodStrat experience looks like this:

1. Visit App → Sign Up / Log In
2. Onboarding → Create Athlete Profile
3. Enter Benchmarks → Build Performance Snapshot
4. Daily Use → Paste WOD → Generate Strategy
5. Perform Workout → Log Outcome (future) → Compare
6. View History & Trends (future)

**In simplified form:**

```
Landing → Onboarding → Profile → Benchmarks → WOD Strategy → (Workout) → History & Insights
```

---

## 3. Global Navigation Model

The app will eventually have a simple primary navigation:

| Section | Description |
|---------|-------------|
| **Dashboard** | Overview of current day + quick actions |
| **Profile** | Athlete info & settings |
| **Benchmarks** | Performance data management |
| **Workouts / Strategy** | WOD input & strategy generation |
| **History / Analytics** (future) | Performance over time |

**MVP can reduce this to:**

- Profile
- Benchmarks
- Strategy

---

## 4. New User Flow – First-Time Experience

### 4.1 Entry Points

- Direct link (shared by coach/friend)
- Search / typed URL

### 4.2 Steps

#### Step 1 – Landing Page

User sees marketing copy explaining:
- "Paste your WOD, get a personal strategy"
- Simple "how it works" steps
- **Primary call-to-action:** "Get Started"

#### Step 2 – Sign Up (Future Auth)

User can:
- Sign up with email/password (or social login in future)
- Or "Continue as Guest" (optional for early MVP)

#### Step 3 – Onboarding Intro

Very short explanation:
> "To give you good strategies, we need a tiny bit of info about you."

**Button:** "Set Up My Profile"

#### Step 4 – Athlete Profile Setup

User fills a basic form:
- Name / nickname
- Age (or date of birth)
- Height / weight (optional but encouraged)
- Training experience (Beginner / Intermediate / Advanced)
- Primary goals (e.g., "Improve pacing", "Prepare for Open", etc.)

**Actions:**
- Continue → Benchmarks
- Option to Skip (with a warning like "Strategies may be less accurate")

#### Step 5 – Benchmark Input Prompt

User is invited to add key benchmarks. Example suggestions:
- Max unbroken pull-ups
- 500m row time
- 2k row time
- "Fran" time
- 1RM clean & jerk / front squat

They can:
- **Add benchmarks now** → goes to Benchmarks flow
- **Skip for now** → goes to Strategy screen with "low confidence" warnings later

#### Step 6 – First Strategy Prompt (Optional Instant Hook)

After onboarding, user is shown:
> "Paste today's WOD and we'll show you an example strategy."

This gives them immediate value and demonstrates the product.

---

## 5. Returning User Flow – Daily Usage

### 5.1 Primary Daily Flow

For a returning athlete, the typical daily flow:

```
Dashboard / Home → Paste WOD → Generate Strategy → Read & Decide → Go Train
```

### 5.2 Steps

#### Step 1 – Access App

User opens WodStrat (already authenticated or using a persistent session).

#### Step 2 – Navigate to Strategy Screen

From Dashboard or nav item: "Strategy" / "Today's WOD"

#### Step 3 – Paste Workout

User copies WOD from:
- Affiliate's whiteboard / SugarWOD / CF.com / comp announcement

Pastes into a multi-line text field, e.g.:

```
For time:
21-15-9
Thrusters 42.5/30kg
Pull-ups
```

**Optional fields:**
- Intended time cap
- Notes (e.g., "RX weight, but PU scaling allowed")

#### Step 4 – Generate Strategy

User clicks "Generate Strategy".

**Backend:**
1. Creates workout entry
2. Parses workout into structured representation
3. Calls strategy engine (rule-based at first, AI later)

#### Step 5 – View Strategy

**UI shows:**
- Summary (1–2 sentence overview)
- Recommended rep schemes
- Target pace / time window
- Breakdown by round/movement
- Warnings (e.g. grip, breathing, legs)
- Scaled options if relevant

**Controls:**
- "Show more details" (expandable sections)
- "Regenerate strategy" (when AI is in place)
- "Save strategy" (future feature)

#### Step 6 – Pre-Workout Decisions

User reads and decides:
- Which option to follow (e.g., "Aggressive", "Safe", "Scaled")
- Might tweak plan mentally based on how they feel that day

#### Step 7 – Go Perform the Workout

This happens offline, in real life!

Post-workout, user may optionally log outcome.

---

## 6. Benchmarks Flow

Benchmarks are key to personalization. This flow can be used during onboarding or later via navigation.

### 6.1 Access

- From nav: **Benchmarks**
- Or from profile onboarding with "Add Benchmarks Now"

### 6.2 List View

User sees a table or card list of:

| Field | Description |
|-------|-------------|
| Benchmark name | e.g., "Fran", "500m Row" |
| Metric | Time, reps, load, pace |
| Last recorded value | The athlete's result |
| Date | When it was recorded |

**Actions:**
- Add Benchmark
- Edit
- Delete (optional)

### 6.3 Add / Edit Benchmark

#### Step 1 – Select Benchmark Type

Predefined options (e.g.):
- Fran
- 500m row
- 2k row
- Max unbroken pull-ups
- 1RM deadlift / clean / squat

Or **Custom Benchmark**.

#### Step 2 – Enter Value

Depending on type:
- Time input (mm:ss)
- Numeric reps
- Weight (kg)

#### Step 3 – Save

- Data sent to backend
- UI updates list
- Strategy engine will now use this data in future WODs

---

## 7. Workout Strategy Flow (Detailed)

This is the core WodStrat flow and the heart of the product.

### 7.1 Entry Points

- "Generate Strategy" button from Dashboard
- Direct navigation to Strategy page
- (Later) from a History workout, "Re-generate strategy" for review

### 7.2 Steps

#### 1. Paste WOD Text

User pastes in any free-form description.

#### 2. (Backend) Parse Workout

Identify:
- Workout type (For Time / AMRAP / EMOM / etc.)
- Expected time domain (short / mid / long)
- Movements, reps, loads
- Total volume per movement

#### 3. (Backend) Retrieve Athlete Data

- Athlete profile
- Benchmarks
- (Future) Historical workouts, HR, etc.

#### 4. (Backend) Generate Strategy

- **v1:** Rule-based logic
- **v2:** AI model with structured prompt

#### 5. Strategy Response Delivered

- **Summary** (short explanation)
- **Detailed plan:**
  - Suggested sets, break patterns
  - Pace guidance per movement/round
  - Expected finish time window
- Option for alternative (scaled) strategies

#### 6. Display Strategy to User

- **Main area:** Summary + key metrics
- **Collapsible sections with details:**
  - Round-by-round pacing
  - Movement-specific notes
  - Scaling suggestions

#### 7. Optional – User Feedback (Future)

User may rate:
- "How accurate was this strategy?"
- "How hard did it feel?" (RPE)

This closes the loop for future AI improvements.

---

## 8. Post-Workout Logging Flow (Future)

Once logging is implemented, the flow extends:

### 8.1 Entry

**After viewing strategy:**
- Option: "I finished the workout, log my result"

**Or from History tab:**
- Select workout → "Log Result"

### 8.2 Steps

#### Step 1 – Input Actual Result

- Finish time
- RX / scaled
- Notes (e.g., "Too hot, fell apart on round 3")

#### Step 2 – Optional Advanced Metrics

- HR average/max (if available)
- RPE
- Movement-specific fail points (checkboxes)

#### Step 3 – Save

- Backend stores actual vs predicted
- Analytics engine can later:
  - Compare predicted vs actual
  - Refine strategy biases

---

## 9. History & Insights Flow (Future)

Once enough data is collected, users can see their evolution.

### 9.1 Entry

Nav item: **History / Analytics**

### 9.2 Views

- **Workout history** – table/list of past strategies + results
- **Benchmark trends** – graph per movement/benchmark
- **Strengths & weaknesses** – e.g., barbell vs gymnastics vs engine

### 9.3 Drill-down

Click any workout to see:
- Original WOD
- Strategy (original AI output)
- Logged result
- Compare predicted vs actual (future AI tuning)

---

## 10. Error / Edge Case Flows

### 10.1 Missing Profile Data

User tries to generate strategy without profile/benchmarks:

**Show warning:**
> "We can generate a basic strategy, but it'll be less accurate. Add your benchmarks to improve precision."

**Offer:**
- Proceed anyway
- Go to Profile/Benchmarks

### 10.2 Unparsable Workout Text

If WOD parsing fails:

**Show error:**
> "We couldn't understand this workout format."

**Suggest:**
- Provide example formats
- Let user tweak text and retry

### 10.3 AI Service Unavailable (Future)

**Fallback:**
- Use rule-based strategy

**Show notice:**
> "AI is unavailable, using backup strategy logic."

---

## 11. Summary

WodStrat's core user flow is:

1. **Set up who you are** → Profile + Benchmarks
2. **Tell us what you're doing today** → Paste WOD
3. **Get a smart, personal plan** → Strategy
4. **(Later) Track reality vs plan** → Results + Insights

This document should guide:

| Area | Focus |
|------|-------|
| **Backend** | API design around athletes, benchmarks, workouts, strategies, history |
| **Frontend** | Page structure, navigation, and UI priorities |
| **Product** | MVP scope and future phases |

---

## Next Steps

Potential follow-up artifacts:
- A UI wireframe spec that maps these flows to pages & components
- A Mermaid diagram version of the key flows for inclusion in your docs
- API contract drafts that match these flows exactly