# ClaimMVP

> English translation for reviewers. Vietnamese original: [ClaimMVP.md](ClaimMVP.md).

> WebApplication · Revision 169 · Updated 2026-09-22

## Overview

| Metric | Count |
|---|---|
| UI flows | 2 |
| Screens | 17 |
| Server actions | 140 |
| Functions | 8 |
| Business entities | 21 |
| Static enums | 11 |
| Structures | 21 |
| Roles | 5 |
| Inherited library items | 0 |

## UI

### UI Flow: Common  _(6 screens)_

- **ChangePassword** — Screen where users can change their current password.
- **InvalidPermissions** — Screen where users will be redirected when they have invalid permissions.
- **Login** — Screen where users login.
- **RecoverPasswordRequest** — Screen where users can request a new password.
- **RecoverPasswordReset** — Screen where users can set a new password for this app. Users should be redirected here from a password recovery request.
- **UserProfile** — Screen where the users can see and edit the user profile.

### UI Flow: MainFlow  _(11 screens)_

- **Administration** — _(no description)_
- **AgentTrace** — _(no description)_
- **ChainTests** — _(no description)_
- **Checkpoints** — _(no description)_
- **ClaimDetail** — _(no description)_
- **Dashboard** — _(no description)_
- **LimitLedgerView** — _(no description)_
- **OpsQueue** — _(no description)_
- **Policies** — _(no description)_
- **TermRegisterList** — _(no description)_
- **WorkList** — _(no description)_

## Logic

### Server actions (140)

- **A5Chain_DryRun** — Test harness: loads context then runs Steps 01–13 in order. No entity writes.
- **A5Chain_InputsHash** — Builds a plain-text fingerprint of the claim inputs for idempotency detection. No hashing library — plain concatenation.
- **A5Chain_LoadContext** — Loads all data needed for one adjudication chain run into a ChainCtx record. Reads only — writes nothing.
- **A5Ledger_ApplyPosting** — Applies a posting to the LimitLedger (creates the row if it does not exist). Sign +1 = FINAL, -1 = REVERSED.
- **A5Ledger_Finalize** — Applies all PROVISIONAL postings of a claim to the LimitLedger (Sign +1) and sets Status to FINAL.
- **A5Ledger_Reverse** — Reverses all FINAL postings of a claim in the LimitLedger (Sign -1) and sets Status to REVERSED.
- **A5Step01_Eligibility** — A5 §6.1 #1 / D1 §5.3 #1 — Checks membership status, policy and premium payment as of the treatment date.
- **A5Step02_WaitingPeriod** — A5 §5 #2 — Checks the waiting period by disease group and benefit group.
- **A5Step03_PreExisting** — D1 §5.3 #3 — Pre-existing conditions: this product has no exclusion; the 365-day waiting period was already applied in step 2.
- **A5Step04_EvalTerm** — Evaluates one term against all lines. ONE ForEach over OutCtx.Lines; all branches converge on JoinLine before cycling back.
- **A5Step04_FindCheckpoint** — Searches a pre-fetched TermCheckpoint list for a row matching ClaimLineId + TermCode. Returns Status (UPHELD/CLEARED/OPEN/""). One ForEach, no nesting.
- **A5Step04_GeneralExclusions** — A5 §6.1 #4 / D1 §5.3 #4 §5.3.1 — Checks the general exclusion terms. Fetches TermRegister Status=ACTIVE, Scope=PRODUCT, RuleSetRef=Ctx.RuleSetRef, sorted by SortNo. Evaluates each term with a Switch on Code (named predicates). For each matching line:
- **A5Step05_BenefitMapping** — A5 §6.1 #5 — Maps a benefit to each expense line. Uses ServiceCategory.InpatientBenefit when TreatmentTypeValue = 'INPATIENT', otherwise OutpatientBenefit. Declines no line. UNMAPPED → Held + Checkpoint. Writes no entity.
- **A5Step06_FindRec** — Searches a pre-fetched Recommendation+Assessment list for a row matching ClaimLineId. Returns Found, Code, Decision, Title. One flat ForEach, no nesting.
- **A5Step06_HasBenefit** — Searches a pre-fetched PolicyBenefitSnapshot list for a row matching BenefitCode. Returns Found Boolean. One flat ForEach, no nesting.
- **A5Step06_LineCoverTest** — D1 §5.5 / A5 §8.2 — Checks coverage for each line. NOTCOV (NONE), ASSESSOR→Held, NOBEN, Recommendation Confirmed. Writes no entity.
- **A5Step07_BenefitProvisos** — D1 B-23 — Checks the benefit-granting provisos (Scope=BENEFIT/BENEFIT_SUBTREE). Count=0 → SKIP. Count>0 → HOLD each proviso. Writes no entity.
- **A5Step08_ApplyGroupCap** — Applies a monetary cap to all lines in a group (same GroupKey), in Ordinal order. ONE ForEach. Returns OutCtx, CutAmount, GroupSum, LogLine.
- **A5Step08_GetBenefitLimit** — Searches a pre-fetched PolicyBenefitSnapshot list for a row matching BenefitCode. Returns Found, LimitType, cap columns, CountLimit, CountUnit. One flat ForEach.
- **A5Step08_SubLimit** — A5 §6.1 #8 — Applies per-visit / per-day / per-event caps. Pass 1: assigns GroupKey. Pass 2: calls ApplyGroupCap for each unprocessed group. Writes no entity.
- **A5Step09_CheckDayNode** — Checks DAY count limit for one benefit node. One loop over snapshot list to find the row; one aggregate for LimitLedger. No ForEach over Ctx.Lines.
- **A5Step09_CountLimit** — A5 §6.1 #9 — Checks the visit/day count limit. VISIT: counts visits, compares with LimitLedger.UsedCount, declines the excess visits. DAY: Held (Q-IDP-06). Writes no entity.
- **A5Step09_CountVisits** — Counts distinct VisitKey values among non-declined lines for a given BenefitCode. ONE ForEach with a seen-keys accumulator.
- **A5Step09_DeclineExcessVisits** — Declines lines whose visit index exceeds Remaining for a given BenefitCode. ONE ForEach with seen-keys accumulator and visit index.
- **A5Step09_HoldBenefitLines** — Sets Held=True and HoldReason on every non-declined line matching BenefitCode. ONE ForEach. Returns OutCtx and HeldCount.
- **A5Step10_CoPayment** — A5 §6.1 #10 — Co-payment per benefit. Resolves CopayPct through the own→parent→root tree. Writes no entity.
- **A5Step10_ResolveCopay** — Resolves CopayPct for a line: own → parent → root → 0. One loop over snapshot list. Returns Bp (basis points as Integer) and SourceCode.
- **A5Step11_DeductionRatio** — A5 §6.1 #11 — Out-of-network deduction (PROVIDER_OFF_NETWORK). Applies only when IsInNetwork = False. B-19 interim: reads only the line's own benefit row. Writes no entity.
- **A5Step11_GetDeduction** — Reads DeductionPct and DeductionCondition from the line's OWN benefit row only (no ancestor walk). One flat loop.
- **A5Step12_ApplyAnnualCap** — Applies annual cap to all lines whose GroupKey = NodeCode. B-20 interim: limit consumption order = ServiceDate ascending, then Ordinal. ONE ForEach.
- **A5Step12_FindAnnualNode** — For a single line's benefit codes (BenefitCode, ParentBenefitCode, RootBenefitCode), finds the first code that carries its own annual cap (CapMode=OWN, LimitType=ANNUAL, AnnualLimit>0) by scanning the pre-fetched snapshot list. Returns Node
- **A5Step12_ParentAnnualCap** — A5 §6.1 #12 — Annual cap by benefit tree. Pass 1: assigns GroupKey = RootBenefitCode, calls FindAnnualNode per line to collect NodeCodes. Pass 2: ForEach NodeCodes → GetLedger → ApplyAnnualCap. Writes no entity.
- **A5Step13_PlanLevel** — A5 §6.1 #13 — Plan-wide cap. Products HC-STD/PLS/PRM have no plan-wide cap; the step exists for future products. Outcome = SKIP.
- **A5Step14_PostDay** — Creates DAY LedgerPosting(s) PROVISIONAL for BenefitCode and ancestors that have CountUnit=DAY. At most 3 creates; one loop over snapshot list to read CountUnit.
- **A5Step14_PostLedger** — A5 §6.1 #14 — Writes the provisional ledger. Reverses old PROVISIONAL postings, updates AmountPayable/TotalPayable, creates AMOUNT + OCCURRENCE postings, writes CalcSnapshot.
- **A5Step14_PostLine** — BR-TREE-2: creates a PROVISIONAL AMOUNT posting for BenefitCode, ParentBenefitCode (if different) and RootBenefitCode (if different from both). No loop. Returns PostCount.
- **A5Term_InScope** — Pure predicate: given a TermCode and line facts, returns InScope (line matches the term) and Known (the engine has a named predicate for this code). No loops, no aggregates.
- **AcknowledgeLowConfidenceLines** — Marks the latest Assessment for a claim as COMPLETE after the assessor has checked low-confidence lines against the original.
- **BatchB_DataCheck** — Read-only data quality check for batch B / B2 migration. Returns counts for each expected state.
- **BootstrapDemoData** — Clears and re-seeds all demo data in one call.
- **ChainTest_Assert** — Pure predicate: evaluates one ChainTest assertion. No loops, no aggregates.
- **ChainTest_CleanupFixtures** — Deletes all TST-* claims and FX-* members/policies. Returns DeletedClaims and RemainingClaims counts.
- **ChainTest_Concurrent** — Helper for T-CHAIN-06: reset, run claim1 then claim2 twice, finalize claim1, measure ledger shift and posting states.
- **ChainTest_ConfirmRerun** — BR-TRACK-4: run chain once, confirm all NOTREL recommendations, run chain again. Returns TotalA/TotalB, RunNoA/RunNoB, Log.
- **ChainTest_DeleteClaimTree** — Deletes all rows referencing a claim (in FK-safe order), then the claim itself. Used by Seed_ChainTestFixtures cleanup.
- **ChainTest_DeleteMemberLedger** — Deletes all LimitLedger rows for a given MemberId. Helper for ChainTest_CleanupFixtures.
- **ChainTest_DeletePolicySnapshot** — Deletes all PolicyBenefitSnapshot rows for a given PolicyId. Helper for ChainTest_CleanupFixtures.
- **ChainTest_FindStep** — Searches a pre-fetched StepResults list for a row matching StepNo. Returns Found, Outcome, ResultAmount, StepLog. One flat ForEach.
- **ChainTest_RunTwice** — SC-H4: run chain twice on the same claim, compare InputsHash and StepOutputsJson. Returns TotalA/TotalB, HashSame, StepsSame, Log.
- **COUNT_UNLIMITED** — Named sentinel for PolicyBenefitSnapshot.CountLimit: 0 means no count limit. Every comparison must use COUNT_UNLIMITED(), never a bare 0.
- **DecideRecommendation** — Records the assessor decision on a clinical recommendation. CONFIRMED also sets the linked ClaimLine to NOTREL. Does NOT touch any amount.
- **FormatVND** — Formats a Decimal amount as Vietnamese Dong (vi-VN style). Returns '—' for negative amounts. — *public*
- **GetCalcSnapshotData** — Returns the latest ChainRun for a claim, its CalcSnapshot steps, outpatient limit usage, and whether the claim is final. (Spec name: GetCalcSnapshot)
- **GetCalcSnapshotData2** — Returns the latest ChainRun for a claim, its CalcSnapshot steps, outpatient limit usage, and whether the claim is final. — *public*
- **GetClaimStatusInfo** — Returns Label and Value for a given ClaimStatus identifier.
- **GetIncidents** — Returns all open technical incidents for the OpsQueue screen. Read-only.
- **GetLedgerCounters** — Returns limit counters for a member/policy. Read-only. UsedPct computed here only.
- **GetLedgerCounters2** — Returns limit counters for a member/policy. Read-only. — *public*
- **IdpAssessmentPoller_Run** — Called by the IdpAssessmentPoller timer. Polls all IdpAssessmentRun rows with State = '' (not yet terminal). Backoff: 3s→5s→10s→30s per AttemptCount. Ceiling: Settings.IdpPollMaxMinutes. Status 3 (Finished) → call IdpGetAssessment, State=CO
- **IdpCreateGroup** — Creates a new IDP request group. One claim = one group. Calls POST /request/group with empty body.
- **IdpGetAssessment** — Calls GET /request/{id}/result/json. Hard rule 5: HTTP 200 {data:[...]} = COMPLETE; HTTP 200 {} = ERROR (BROKEN — flow emitted markdown, NEVER 'no findings'); HTTP 400 statusCode 4007 = not ready. Normalises 'None', 'null', '00000' to empty
- **IdpGetAssessmentStatus** — Calls GET /workflow/request/{id}. Returns Status integer. Reads 'id' field — NOT 'workflowRequestId' (comes back null on GET). result.logs is stderr from the flow engine (3000+ chars of UserWarning) — never surface as failure.
- **IdpGetGroupStatus** — Returns one IdpGroupStatusItem per request in the group. RequestStatus: 0=Sending, 1=Processing, 2=Completed, 3=Failed, 4=Cancelling, 5=Cancelled.
- **IdpGetRawFile** — Calls GET /request/{id}/raw-file. Returns ZIP bytes to caller. Do not render. Hard rule 10: never call PUT /request/{id}/result.
- **IdpGetResult** — Calls GET /request/{id}/result and parses with IdpParseExtraction(IsSyncShape=False). Hard rule 10: never call PUT /request/{id}/result.
- **IdpGetTemplateCsv** — Reads the document set whose id = Settings.IdpTemplateSetId from GET /template/document-set, joins its templates with commas. Hard rule 2: exposes RequestedCount (from set metadata) and ReturnedCount (actual templates returned) separately —
- **IdpGetToken** — Returns a valid BnK IDP access token. Uses the cached token if still valid; re-logs in otherwise. ForceRefresh=True always re-logs in.
- **IdpParseExtraction** — Single shared parser for all IDP extraction response shapes. IsSyncShape=True: result.result (two levels, POST /request/sync). IsSyncShape=False: result (one level, GET /request/{id}/result). Never throws — parse failure sets ErrorCode=PARS
- **IdpPing** — Diagnostic action: forces a fresh login, calls /user/me, and returns a multi-line report. Use to verify the authentication layer end-to-end.
- **IdpRefreshIdentity** — Calls /user/me with the cached token and stores TenantId, UserId, RoleId on the IdpSession row. Validates RoleId against IdpExpectedRoleId.
- **IdpRunAssessment** — Triggers execution of a workflow request. HARD RULE 3: GET /workflow/run-workflow-request/{id} is unstable — returns HTTP 400 on first call and 200 on retry with identical parameters. This is a deliberate exception to the project-wide no-re
- **IdpSearchHospital** — Calls GET /metadata/opensearch/table/322/search. Hard rule 3: duplicates exist — orders by score, returns best hit first. Hard rule 4: empty hospital_code and '00000' normalised to null. Hard rule 5: server error = table not indexed (config
- **IdpStartAssessment** — Creates and starts a clinical assessment. Step 1: verify all OCR requests are Completed (hard rule 2 — IDP accepts Failed requests without validation). Step 2: call PUT /workflow/request with workflowId=Settings.IdpWorkflowId, name='ASSESS-
- **IdpSubmitDocument** — Async multipart submit. Returns as soon as IDP accepts the file. name=ClaimNo (hard rule 2), templates=Settings.IdpTemplateCsv (hard rule 1).
- **IdpSubmitSync** — Synchronous submit. Guard: if PageCount > IdpSyncMaxPages, returns failed Outcome without calling API. Timeout 60 s (hard rule 8). Parses response with IdpParseExtraction(IsSyncShape=True).
- **MigrateA6_DecisionCodes** — Backfills LineDecisionId and DeclineReasonId on ClaimLine rows that have DecisionCodeId set but LineDecisionId not yet set.
- **Portal_GetCurrentMember** — Returns the Member with the lowest Id (demo: "Nguyễn Văn An"), their Policy, and their Claims sorted by CreatedOn desc.
- **Portal_GetCurrentMember2** — Returns the Member with the lowest Id, their Policy, and their Claims sorted by CreatedOn desc. — *public*
- **Portal_SendSupplement** — Creates an AgentRun row recording that the customer sent supplementary information for a claim.
- **Portal_SendSupplement2** — Creates an AgentRun row recording that the customer sent supplementary information for a claim. — *public*
- **Portal_SubmitClaim** — Creates a new Claim with 5 ClaimDocument rows and 1 AgentRun row. Money fields are always 0 — the adjudication chain fills them.
- **Portal_SubmitClaim2** — Creates a new Claim with 5 ClaimDocument rows and 1 AgentRun row. Money fields are always 0. — *public*
- **RoundVnd** — A5 §6.2: nearest whole dong, half away from zero, applied at every money step.
- **RunAdjudicationChain** — Runs the 14-step A5 chain on a claim and writes the results (TotalPayable, AmountPayable, PROVISIONAL LedgerPosting, CalcSnapshot, ChainRun). The only place that writes money. Input: ClaimId, TriggeredBy. Output: ChainRunId, TotalPayable, DurationMs, Outcomes
- **RunChainTests** — Runs all RUNNABLE ChainTest rows. Calls A5Chain_DryRun per test, evaluates assertions, updates ChainTest. Writes only ChainTest.
- **RunSeedBatch0Now** — Calls Seed_AssessmentAndTerms so the seed can be re-triggered without republishing. Expose to Administrators via a screen.
- **Seed_AgentRuns** — Seeds AgentRun rows for the 4 demo claims. Idempotent — guarded by count check.
- **Seed_AssessmentAndTerms** — Seeds all demo data. No guards except TermRegister. Always creates fresh after reset.
- **Seed_BatchB2_Remap** — Batch B2: fills ClaimLine.Category on existing seed lines, derives ClaimLine.BenefitCode from ServiceCategory (A4 §4.5) — never guesses — then retires the 5 prototype benefit codes. Idempotent.
- **Seed_BatchB3_Claim0001** — Migrates the existing CLM-2026-0001 to the 6-line outpatient hero claim when its lines are still the old 4 inpatient ones. Idempotent: does nothing when the claim already has 6 lines whose first Description is 'Tiền khám' (EN: "Consultation fee").
- **Seed_BatchB_Backfill** — Batch B backfill for rows that already exist and are NOT recreated by Re-run demo seed. Idempotent: only fills NULL/empty values, never overwrites, never creates or deletes rows, never touches money.
- **Seed_BenefitSchedule** — Seeds the A5 §4.1/§4.2 benefit schedule: 17 benefits × 3 plans = 51 PolicyBenefitSnapshot rows. Idempotent upsert keyed on (PolicyId, BenefitCode): update all columns when the row exists, create otherwise. Never deletes.
- **Seed_BenefitSchedule_Upsert** — Private helper: UPSERT one PolicyBenefitSnapshot row keyed on (PolicyId, BenefitCode). Creates if absent, updates all columns if present.
- **Seed_BenefitSnapshots** — Creates PolicyBenefitSnapshot seed rows for the 3 demo policies.
- **Seed_ChainTestFixtures** — Idempotent: deletes all TST-* claims and FX-* policies/members, then recreates fixture data for chain tests.
- **Seed_ChainTestFixtures_Part2** — Claims part 2 for chain test fixtures (T-CH05-b through T-CH09-c).
- **Seed_ChainTestFixtures_Part3** — Claims part 3 for chain test fixtures (T-CH09-b, T-CH09-c).
- **Seed_ChainTestFixtures_Part4** — Claims part 4 for chain test fixtures (T-CH10-a through T-CH12-c).
- **Seed_ChainTestFixtures_Part5** — Claims part 5 for chain test fixtures (T-CHAIN-01 through T-CHAIN-05).
- **Seed_ChainTestFixtures_Part6** — Claims part 6 for chain test fixtures (T-CHAIN-06a and T-CHAIN-06b).
- **Seed_ChainTestFixtures_Part7** — Claims part 7 for chain test fixtures (T-CHAIN-07: 1 day ICU inpatient on FX-M1).
- **Seed_ChainTests** — Deletes all ChainTest rows then calls Seed_ChainTests_A/B/C in order.
- **Seed_ChainTests_A** — Seeds ChainTest rows 1–23 (section 9.1, steps 1–7).
- **Seed_ChainTests_B** — Seeds ChainTest rows batch B. Empty shell — to be implemented in a later turn.
- **Seed_ChainTests_C** — Seeds ChainTest rows batch C. Empty shell — to be implemented in a later turn.
- **Seed_ClaimDocuments** — Seeds ClaimDocument rows for the 4 demo claims. Idempotent — guarded by count check.
- **Seed_ClaimLines** — Creates 16 demo ClaimLine records for the 4 demo claims.
- **Seed_Claims** — Creates 4 demo Claim records and returns their Ids.
- **Seed_ClearDemoData** — Deletes all demo data rows from all entities in dependency-safe order.
- **Seed_CopyPBS** — Copies all PolicyBenefitSnapshot rows from FromPolicyId to ToPolicyId. Used by Seed_ChainTestFixtures.
- **Seed_FixtureClaim** — Creates one fixture Claim and returns its Id. Used by Seed_ChainTestFixtures.
- **Seed_FixtureLine** — Creates one fixture ClaimLine and adds Amount to Claim.TotalClaimed. Used by Seed_ChainTestFixtures.
- **Seed_LimitLedger** — Creates LimitLedger seed rows for the 3 demo members.
- **Seed_PoliciesAndMembers** — Creates 3 Policy and 3 Member seed records and returns their Ids.
- **Seed_ResetDemoClaimData** — Deletes all seed-owned rows for the 4 demo claims in dependency-safe order, then logs done.
- **Seed_TermRegister_v2** — Seeds the 10 A5 §7.2 product terms (TERM-HC-001..010). Idempotent: deletes all TermRegister rows first, then creates. Called by RunSeedBatch0Now.
- **Seed_WaitingPeriod_Upsert** — Private helper: UPSERT one BenefitWaitingPeriod row keyed on (PlanCode, BenefitGroupCode, ConditionClassId). Creates if absent, updates Days if present and different.
- **Seed_WaitingPeriods** — Seeds the 51 BenefitWaitingPeriod rows of A5 §5 (17 combinations × 3 plans). Idempotent UPSERT keyed on (PlanCode, BenefitGroupCode, ConditionClassId): update Days when the row exists, create it otherwise. Never deletes.
- **SendChangeEmail** — Triggers the update email operation for users who don't  have access to the ODC Portal, returning a verification code that  can be sent by email to the user.
- **SendResetPasswordEmail** — Triggers the reset password operation for users that don’t have access to the ODC Portal, returning a verification code that can be sent by email to the user.
- **Step01_PolicyEligibility** — CH-01. Policy active and premium paid on EventDate; member covered under the rider. Reads Policy, PolicyBenefitSnapshot, Member. Output: eligible / not eligible at claim level. NOT IMPLEMENTED.
- **Step02_WaitingPeriod** — CH-02. EventDate minus rider effective date must be at least the waiting period for the loss type (illness 30 days, specified illness 90, injury 0) per snapshot. Output per line: excluded with reason WAIT, or continue. NOT IMPLEMENTED.
- **Step03_EventClassification** — CH-03. Map loss type + treatment type + ICD to a benefit category (Outpatient / Inpatient / Surgery / Maternity). Output: benefit category for the claim. NOT IMPLEMENTED.
- **Step04_LineBenefitMapping** — CH-04. Map each ClaimLine service group to a benefit item in the snapshot. Output: benefit item per line, or UNMAPPED which routes to an officer. NOT IMPLEMENTED.
- **Step05_CoverageScopeExclusions** — CH-05. Apply policy exclusions and CONFIRMED clinical recommendations: NOTCOV (not a covered benefit) and NOTREL (not related to diagnosis). Output: covered or excluded with a reason code per line. NOT IMPLEMENTED.
- **Step06_SocialInsuranceDeduction** — CH-06. Subtract the amount already paid by social insurance (BHYT) where the document shows it. Output: net requested per line. NOT IMPLEMENTED.
- **Step07_PerVisitCap** — CH-07. Cap per visit or per day per benefit item from the snapshot. Output: capped amount plus the CAP_VISIT excess. NOT IMPLEMENTED.
- **Step08_PerYearLimitCheck** — CH-08. Remaining annual limit per benefit item, read from LimitLedger. Output: amount within the remaining limit plus the LIMIT_YEAR excess. NOT IMPLEMENTED.
- **Step09_CoPayment** — CH-09. Apply the co-payment percentage (dong chi tra) per product from the snapshot, e.g. PLS 20 percent. Output: insurer share and customer share with reason COPAY. NOT IMPLEMENTED.
- **Step10_Deductible** — CH-10. Apply the annual deductible if the product has one. Output: DEDUCT amount. NOT IMPLEMENTED.
- **Step11_ClaimLevelCap** — CH-11. Total payable must not exceed the overall annual limit of the rider. Output: LIMIT_TOTAL excess. NOT IMPLEMENTED.
- **Step12_Rounding** — CH-12. Round payable to 1,000 VND per the product rule. Interim decision B-20: round down. This is the only step allowed to round. NOT IMPLEMENTED.
- **Step13_CustomerBorneBreakdown** — CH-13. For each line, split the customer-borne amount by cause: WAIT, NOTCOV, NOTREL, CAP_VISIT, LIMIT_YEAR, COPAY, DEDUCT, LIMIT_TOTAL. Feeds the customer-facing explanation. NOT IMPLEMENTED.
- **Step14_LedgerPreview** — CH-14. Compute limit consumption per benefit item without posting it, and the claim totals: Requested, Covered, Payable, CustomerBorne. NOT IMPLEMENTED.
- **TakeUnassigned** — Assigns all claims with empty AssignedTo to the current user. Returns the count assigned and their IDs.
- **TransitionClaimStatus** — Transitions a claim to a new status. Enforces allowed-transition table and four-eyes rule. Writes an AgentRun audit row.
- **UndoTakeUnassigned** — Clears AssignedTo on the given claim IDs (reverting a TakeUnassigned operation).
- **UpdateUser** — Allows a logged-in user to edit profile information. The user email can't be changed using this action.  Throws an exception if the built-in identity provider is disabled for the current app.
- **WorkList_StatusCounts** — Returns per-status claim counts for the current user (AssignedTo = GetUserId()), plus TotalAll. No status/date filters.
- **WriteAgentRun** — FR-CORE-24. Appends one immutable audit row per agent turn or tool call. Append-only by design: there is no update and no delete counterpart. HandoffTo names the agent this turn hands control to.

### Functions (8)

_Inline-callable from expressions._

- **DoLogin** — Performs a login using the system action for the given username and password. Returns if the login was succesful, and if it wasn't, the detailed error message.
- **DoLogout** — Action that logout the client, clear user information and redirect to the home page.
- **FormatVND** — Formats a Decimal amount as Vietnamese Dong (vi-VN style). Returns '—' for zero or negative amounts.
- **IsRecPending** — Returns True when a Recommendation is still pending human decision (Decision is empty or 'Pending').
- **SendChangeEmail** — Triggers the update email operation for users who don't  have access to the ODC Portal, returning a verification code that  can be sent by email to the user.
- **SendResetPasswordEmail** — Starts the reset password process for users that don’t have access to the ODC Portal, returning a verification code that can be sent by email to the user.
- **StatusUi** — Maps ClaimStatus identifier and Value to UiLabel + CssClass for the solid pill.
- **UpdateUser** — Allows a logged-in user to edit profile information. The user email can't be changed using this action.  Throws an exception if the built-in identity provider is disabled for the current app.

## Data

### Business entities (21, owned)

- **AgentRun** — _(no description)_
- **Assessment** — One row per clinical-assessment run of a claim
- **BenefitWaitingPeriod** — (PlanCode, BenefitGroupCode, ConditionClass) → Days. A5 §5. A missing row means the combination cannot arise (n/a) — a lookup miss is a DATA ERROR, never a 0-day wait.
- **CalcSnapshot** — One row per step of the 14-step chain for one ChainRun (display-only)
- **ChainRun** — One execution of RunAdjudicationChain for a claim. InputsHash lets the console detect stale amounts after an edit. Append-only: compare two runs, never update one. BRD 10.1.
- **ChainTest** — Chain test case register.
- **Claim** — _(no description)_
- **ClaimDocument** — _(no description)_
- **ClaimLine** — _(no description)_
- **IdpAssessmentRun** — Tracks one in-flight IDP clinical assessment. Created by IdpStartAssessment; polled by IdpAssessmentPoller. Append-only: never update Status/State directly — use the poller.
- **IdpFieldMap** — Maps an IDP field label (K) to a stable internal field code, per template. Never map by template name.
- **IdpSession** — Single-row token cache for the BnK IDP session. Never holds more than one row.
- **LedgerPosting** — BR-LEDGER-3: postings are never deleted — reversed.
- **LimitLedger** — _(no description)_
- **Member** — _(no description)_
- **Policy** — _(no description)_
- **PolicyBenefitSnapshot** — _(no description)_
- **RecEvidence** — Evidence items cited by a recommendation
- **Recommendation** — AI proposal on ONE claim line
- **TermCheckpoint** — One checkpoint raised by the adjudication chain for a term that matched a claim or line.
- **TermRegister** — Product terms register

### Static enums (11)

`CapMode`, `ClaimStatus`, `ConditionClass`, `DecisionCode`, `DeclineReason`, `LineDecision`, `LossType`, `PolicyStatus`, `RenewalStatus`, `ServiceCategory`, `TreatmentType`

### Structures (21)

- **BenefitRow** — One row in the settlement benefit table, built client-side from GetLines.
- **ChainCtx** — Full working context for one adjudication chain run.
- **ChainCutCtx** — One reduction applied to a line during adjudication.
- **ChainLineCtx** — Per-line working context for the adjudication chain.
- **ChainStepResult** — One step result from the adjudication chain dry-run.
- **ChatMsg** — _(no description)_
- **IdpAssessmentItem** — One item from the IDP clinical assessment flow output. InstructionType is exactly 'Cảnh báo' (EN: "Warning") or 'Trích xuất field' (EN: "Extract field").
- **IdpAssessmentOutcome** — Full result of one IDP clinical assessment. State: COMPLETE | PARTIAL | ERROR. Hard rule 10: advisory only — no proposedReasonCode, no evidence[], no per-line assessment, no confidence band.
- **IdpCallOutcome** — Outcome of a BnK IDP API call. Supports the pattern where HTTP 200 can still carry an errorCode in the body.
- **IdpDocumentSet2** — One document set from GET /template/document-set. Hard rule 1: templates may arrive as strings or numbers — parse tolerantly. Hard rule 2: silently dropped templates leave orphan ids — always compare 
- **IdpExtractionResult** — Full parsed extraction result for one IDP request. ClaimConfidence and Completeness are computed from mandatory fields — never from the IDP confidence figure.
- **IdpField** — One extracted field from an IDP template.
- **IdpGroupStatusItem** — One item in a group status response. RequestStatus: 0=Sending, 1=Processing, 2=Completed, 3=Failed, 4=Cancelling, 5=Cancelled.
- **IdpHospitalHit2** — One hospital search hit. Hard rule 3: duplicates exist — caller takes best score. Hard rule 4: empty hospital_code normalised to null. Hard rule 6: authoritative source — ignore HospitalCode from clin
- **IdpHospitalRawHit** — Raw JSON shape from GET /metadata/opensearch/table/322/search. Internal use only.
- **IdpTable** — One extracted table from an IDP template.
- **IdpTableRow** — One row in an IDP extracted table.
- **IdpTemplateData** — Extraction result for one template within a request.
- **IncidentRow** — One row in the operations queue incident list.
- **LedgerCounterRow** — One row in the limit ledger counter list for a member/policy.
- **StatusCount** — One row in the WorkList status count result.

## Security

- **ClaimApprover** — *application role* — Approves or declines claims above the auto-approval gate or flagged claims; four-eyes rule — must differ from the submitting officer.
- **ClaimOfficer** — *application role* — Works the claim queue, reviews extraction and cost lines, confirms or rejects agent recommendations with a mandatory reason, submits claims for approval.
- **ClaimOps** — *application role* — Handles the ExtractionError queue, extraction-service outages, replay mode and retries.
- **Presenter** — *application role* — Read-only access to the agent trace and architecture views for the demo.
- **Template_WebApp** — *application role* — _(no description)_

## Dependencies

_No tracked library inheritance._

---

_Asset key:_ `2a401c4d-d87e-4c44-8692-9ad661840125`  
_Generated:_ 2026-09-23  
_Tool:_ `outsystems-app-documentation` v1.0.0  
