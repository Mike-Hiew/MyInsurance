# Claim Console - Requirements Document

Version: 1.0

Date: September 15, 2026

Author: BnK Solution - Claim Platform team

For: Mentor App Generator (ODC)

Research basis: OutSystems Mentor documentation, synced 2026-09-15

### 1. Application Overview

This document describes the **Claim Console**, the internal web application used by a health insurer to adjudicate reimbursement claims. A claim arrives with photographed medical documents already read by an external document-extraction service. The console presents the extracted cost lines, the recommendations produced by the adjudication agents, and the computed settlement, so a claim officer can confirm or reject each recommendation and submit the claim for approval.

The application owns the shared data model for the claim domain. A separate customer-facing application references these entities. All monetary values are produced by an external deterministic Server Action and stored by it; screens in this application display those values and never compute them.

### 2. General Application Settings

The application must use the **"Mentor"** theme available in the ODC tenant.

The interface language is English. Data values, item descriptions and facility names are stored and displayed in Vietnamese as captured.

### 3. Data Model

#### Entities & Attributes

* **Entity: Policy**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - PolicyNo: Text, the unique policy number.
    - PolicyCode: Text, the product code of the health cover.
    - PolicyHolderName: Text, the name of the policy holder.
    - EffectiveDate: Date, the date cover begins.
    - ExpiryDate: Date, the date cover ends.
    - StatusId: An Identifier that is a Foreign Key to the PolicyStatus static entity.

* **Entity: Member**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - MemberNo: Text, the unique member number.
    - FullName: Text, the covered person's name.
    - DateOfBirth: Date, used to derive age at treatment.
    - Gender: Text, the recorded gender.
    - RelationshipToHolder: Text, the relationship to the policy holder.
    - PolicyId: An Identifier that is a Foreign Key to the Policy entity.

* **Entity: PolicyBenefitSnapshot**
  + This entity is stored **locally**.
  + It records benefit values as they stood when the claim was created, so later benefit changes never alter a settled claim.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - PolicyId: An Identifier that is a Foreign Key to the Policy entity.
    - BenefitCode: Text, the code of the benefit item.
    - BenefitName: Text, the display name of the benefit item.
    - SumInsured: Currency, the total insured amount for this benefit.
    - AnnualLimit: Currency, the maximum payable per policy year.
    - PerVisitCap: Currency, the maximum payable for a single visit.
    - CoinsurancePct: Integer, the percentage of each eligible amount borne by the member, from 0 to 100.
    - WaitingPeriodDays: Integer, the number of days after the effective date before this benefit pays.
    - SnapshotDate: Date, the date these values were captured.

* **Entity: Claim**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimNo: Text, an auto-generated unique claim number used as the correlation key across every system.
    - PolicyId: An Identifier that is a Foreign Key to the Policy entity.
    - MemberId: An Identifier that is a Foreign Key to the Member entity.
    - StatusId: An Identifier that is a Foreign Key to the ClaimStatus static entity.
    - TreatmentTypeId: An Identifier that is a Foreign Key to the TreatmentType static entity.
    - EventDate: Date, the date the illness or injury occurred.
    - AdmissionDate: Date, the date treatment began.
    - DischargeDate: Date, the date treatment ended.
    - FacilityName: Text, the treating hospital or clinic as printed on the documents.
    - TotalClaimed: Currency, the sum of all amounts requested.
    - TotalPayable: Currency, the sum the insurer will pay, written only by the adjudication chain.
    - AssignedTo: User Identifier, the officer currently working the claim.
    - CreatedOn: DateTime, the timestamp the claim was created.

* **Entity: ClaimDocument**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - FileName: Text, the uploaded file name.
    - DocumentTypeId: An Identifier that is a Foreign Key to the DocumentType static entity.
    - TemplateId: Text, the extraction template that matched this document.
    - IdpRequestId: Text, the identifier returned by the extraction service.
    - RawExtractionJson: Text, the extraction result exactly as first received. This value is never updated after it is written.
    - Confidence: Integer, the extraction confidence for this document, from 0 to 100.
    - ErrorCode: Text, the error code returned by the extraction service when reading failed.
    - UploadedOn: DateTime, the timestamp the document was uploaded.

* **Entity: ClaimLine**
  + This entity is stored **locally**.
  + Each row is one cost line read from an itemised statement.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - ClaimDocumentId: An Identifier that is a Foreign Key to the ClaimDocument entity.
    - Description: Text, the service or item name as printed.
    - ServiceDate: Date, the date the service was delivered.
    - Quantity: Integer, the number of units.
    - AmountClaimed: Currency, the amount requested for this line.
    - AmountPayable: Currency, the amount the insurer will pay, written only by the adjudication chain.
    - BenefitCode: Text, the benefit item this line maps to.
    - DecisionCodeId: An Identifier that is a Foreign Key to the DecisionCode static entity.
    - DecisionReason: Text, the reason recorded when a line is not paid in full.
    - Confidence: Integer, the extraction confidence for this line, from 0 to 100.
    - IsVerified: Boolean, whether an officer has checked this line against the original image.

* **Entity: ClaimLineBreakdown**
  + This entity is stored **locally**.
  + It explains, per cause, why a line is not paid in full.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimLineId: An Identifier that is a Foreign Key to the ClaimLine entity.
    - CauseCodeId: An Identifier that is a Foreign Key to the ExclusionCause static entity.
    - Amount: Currency, the amount attributed to this cause.

* **Entity: ChainRun**
  + This entity is stored **locally**.
  + Each row is one execution of the deterministic adjudication chain. Rows are kept so two runs can be compared.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - RunNo: Integer, the sequence number of this run for the claim.
    - Version: Text, the version of the calculation rules applied.
    - InputsHash: Text, a hash of the inputs, used to detect whether displayed amounts are stale.
    - StepOutputsJson: Text, the output of each calculation step.
    - GateResultJson: Text, the outcome of each auto-approval condition.
    - TotalRequested: Currency, the total amount requested.
    - TotalCovered: Currency, the total amount within cover.
    - TotalPayable: Currency, the total the insurer will pay.
    - TotalCustomerBorne: Currency, the total the member pays themselves.
    - TriggeredBy: Text, what caused this run.
    - RunAt: DateTime, the timestamp of the run.

* **Entity: LimitLedger**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - PolicyId: An Identifier that is a Foreign Key to the Policy entity.
    - MemberId: An Identifier that is a Foreign Key to the Member entity.
    - BenefitCode: Text, the benefit item whose limit this row tracks.
    - PeriodYear: Integer, the policy year.
    - UsedAmount: Currency, the amount consumed so far in the period.
    - RemainingAmount: Currency, the amount still available in the period.
    - LastClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - UpdatedOn: DateTime, the timestamp of the last posting.

* **Entity: AgentRun**
  + This entity is stored **locally**.
  + Each row is one agent step. Rows are added and never changed or removed.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - AgentName: Text, the agent that ran.
    - Step: Text, the step within the agent's work.
    - ToolName: Text, the tool the agent called.
    - InputJson: Text, a record of what the agent received.
    - OutputJson: Text, a record of what the agent produced.
    - HandoffTo: Text, the agent that received work next.
    - KnowledgeVersion: Text, the version of the knowledge base consulted.
    - ModelId: Text, the model that produced the output.
    - DurationMs: Integer, the elapsed milliseconds.
    - IsError: Boolean, whether the step failed.
    - CorrelationId: Text, the key that links this row to the claim across systems.
    - StartedOn: DateTime, the start timestamp.
    - FinishedOn: DateTime, the end timestamp.

* **Entity: ClinicalAssessment**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - WorkflowRequestId: Text, the identifier of the assessment request.
    - KnowledgeBaseVersion: Text, the version of the clinical rules applied.
    - StateId: An Identifier that is a Foreign Key to the AssessmentState static entity.
    - ItemsJson: Text, the assessment findings as received.
    - StartedOn: DateTime, the start timestamp.
    - FinishedOn: DateTime, the end timestamp.

* **Entity: ClinicalRecommendation**
  + This entity is stored **locally**.
  + Each row is one proposal about one cost line, awaiting an officer decision.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimLineId: An Identifier that is a Foreign Key to the ClaimLine entity.
    - ReasonCodeId: An Identifier that is a Foreign Key to the DecisionCode static entity.
    - EvidenceJson: Text, the supporting quotations and clause references.
    - ConfidenceBandId: An Identifier that is a Foreign Key to the ConfidenceBand static entity.
    - StatusId: An Identifier that is a Foreign Key to the RecommendationStatus static entity.
    - DecidedBy: User Identifier, the officer who confirmed or rejected the proposal.
    - DecisionReason: Text, the reason the officer recorded. Required when a proposal is rejected.
    - DecidedOn: DateTime, the timestamp of the officer decision.

* **Entity: FraudSignal**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - SignalTypeId: An Identifier that is a Foreign Key to the FraudSignalType static entity.
    - SeverityId: An Identifier that is a Foreign Key to the SignalSeverity static entity.
    - EvidenceJson: Text, the supporting detail.
    - StatusId: An Identifier that is a Foreign Key to the SignalStatus static entity.
    - ClearedBy: User Identifier, the officer who cleared the signal.
    - ClearReason: Text, the reason recorded when clearing. Required to clear a signal.
    - ClearedOn: DateTime, the timestamp the signal was cleared.

* **Entity: CrossCheckResult**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - RuleCodeId: An Identifier that is a Foreign Key to the CrossCheckRule static entity.
    - ResultId: An Identifier that is a Foreign Key to the CheckResult static entity.
    - Detail: Text, what the check compared and found.
    - CheckedOn: DateTime, the timestamp of the check.

* **Entity: ClaimEvent**
  + This entity is stored **locally**.
  + It is the claim timeline. Rows are added and never changed or removed.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - EventTypeId: An Identifier that is a Foreign Key to the ClaimEventType static entity.
    - Actor: Text, the person or agent responsible.
    - Detail: Text, the description shown on the timeline.
    - IsCustomerVisible: Boolean, whether this event appears to the customer.
    - OccurredOn: DateTime, the timestamp of the event.

* **Entity: ClaimLetter**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - LetterTypeId: An Identifier that is a Foreign Key to the LetterType static entity.
    - DraftText: Text, the drafted letter awaiting officer approval.
    - SentText: Text, the letter as sent.
    - DraftedBy: Text, the agent that drafted the letter.
    - SentBy: User Identifier, the officer who sent the letter.
    - SentOn: DateTime, the timestamp the letter was sent.

* **Entity: PaymentOrder**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ClaimId: An Identifier that is a Foreign Key to the Claim entity.
    - Amount: Currency, the amount to be paid.
    - AccountMasked: Text, the payout account with all but the last four digits hidden.
    - StatusId: An Identifier that is a Foreign Key to the PaymentStatus static entity.
    - IssuedOn: DateTime, the timestamp the order was issued.

* **Entity: ExclusionList**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - BenefitCode: Text, the benefit item the exclusion applies to.
    - ItemPattern: Text, the item description pattern that triggers the exclusion.
    - ReasonCodeId: An Identifier that is a Foreign Key to the DecisionCode static entity.
    - IsActive: Boolean, whether the exclusion is currently applied.
    - Note: Text, the clause reference supporting the exclusion.

* **Entity: BenefitItemMap**
  + This entity is stored **locally**.
  + It maps an item description on a document to a benefit code.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - TemplateId: Text, the extraction template this mapping applies to.
    - SourceItemName: Text, the item description as printed.
    - BenefitCode: Text, the benefit item it maps to.
    - ServiceGroup: Text, the grouping used for reporting.
    - IsActive: Boolean, whether the mapping is currently applied.

* **Entity: BenefitCategoryMap**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - BenefitCode: Text, the benefit item.
    - CategoryName: Text, the category the benefit belongs to.
    - DisplayOrder: Integer, the order the category appears in.

* **Entity: ReasonCodeDictionary**
  + This entity is stored **locally**.
  + It holds the approved wording for each decision code.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - DecisionCodeId: An Identifier that is a Foreign Key to the DecisionCode static entity.
    - CustomerText: Text, the wording shown to the customer.
    - OfficerText: Text, the wording shown to the officer.
    - IsActive: Boolean, whether this wording is currently used.

* **Entity: IdpSession**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - TokenReference: Text, a reference to the stored credential. The credential itself is held outside this entity.
    - AcquiredOn: DateTime, the timestamp the session was opened.
    - ExpiresOn: DateTime, the timestamp the session expires.
    - IsActive: Boolean, whether this session is currently used.

* **Entity: IdpFieldMap**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - TemplateId: Text, the extraction template this mapping applies to.
    - FieldKey: Text, the field name returned by the extraction service.
    - FieldCode: Text, the attribute it maps to.
    - Version: Text, the version of the mapping.
    - IsActive: Boolean, whether the mapping is currently applied.

* **Entity: IdpReplay**
  + This entity is stored **locally**.
  + It holds previously recorded extraction responses so a demonstration can run when the extraction service is unavailable.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - ReplayKey: Text, the key that identifies which recorded response to return.
    - Endpoint: Text, the operation the recording belongs to.
    - ResponseJson: Text, the recorded response.
    - RecordedOn: DateTime, the timestamp of the recording.

* **Entity: AccuracyRun**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - RunLabel: Text, the name of the measurement run.
    - TemplateId: Text, the extraction template measured.
    - FieldsCompared: Integer, the number of fields checked.
    - FieldsMatched: Integer, the number of fields that matched the expected value.
    - AccuracyPct: Integer, the percentage of fields that matched, from 0 to 100.
    - RunOn: DateTime, the timestamp of the measurement.

#### Entity Relationships

* A **Policy** can have many **Members** (One-to-Many).
* A **Policy** can have many **PolicyBenefitSnapshots** (One-to-Many).
* A **Policy** can have many **Claims** (One-to-Many).
* A **Member** can have many **Claims** (One-to-Many).
* A **Claim** can have many **ClaimDocuments** (One-to-Many).
* A **Claim** can have many **ClaimLines** (One-to-Many).
* A **ClaimDocument** can have many **ClaimLines** (One-to-Many).
* A **ClaimLine** can have many **ClaimLineBreakdowns** (One-to-Many).
* A **ClaimLine** can have many **ClinicalRecommendations** (One-to-Many).
* A **Claim** can have many **ChainRuns** (One-to-Many).
* A **Claim** can have many **AgentRuns** (One-to-Many).
* A **Claim** can have many **ClaimEvents** (One-to-Many).
* A **Claim** can have many **FraudSignals** (One-to-Many).
* A **Claim** can have many **CrossCheckResults** (One-to-Many).
* A **Claim** can have many **ClaimLetters** (One-to-Many).
* A **Claim** can have many **ClinicalAssessments** (One-to-Many).
* A **Claim** can have many **PaymentOrders** (One-to-Many).
* A **Policy** can have many **LimitLedger** rows (One-to-Many).

### 4. Static Entities

* **Entity Name:** ClaimStatus
  + **Purpose:** Defines the states a claim moves through from creation to closure. A claim moves forward through this order and does not skip from an early state directly to Approved, Declined or Paid.
  + **Records:** Draft, Submitted, Extracting, Pending Documents, Extraction Error, Assessing, Adjudicated, Pending Review, Blocked, Pending Approval, Approved, Declined, Paid, Closed.

* **Entity Name:** PolicyStatus
  + **Purpose:** Defines whether a policy is currently in force.
  + **Records:** Active, Lapsed, Expired, Cancelled.

* **Entity Name:** TreatmentType
  + **Purpose:** Distinguishes the setting in which treatment was delivered.
  + **Records:** Inpatient, Outpatient.

* **Entity Name:** DecisionCode
  + **Purpose:** Defines the outcome recorded against a cost line.
  + **Records:** Covered, Not Covered, Not Related, Pending Review.

* **Entity Name:** DocumentType
  + **Purpose:** Categorises the medical documents supporting a claim.
  + **Records:** Discharge Paper, Itemised Cost Statement, Prescription, Lab Result, Hospital Receipt.

* **Entity Name:** ExclusionCause
  + **Purpose:** Explains why part of a cost line is borne by the member.
  + **Records:** Waiting Period, Not Covered, Not Related, Per Visit Cap, Annual Limit, Coinsurance.

* **Entity Name:** RecommendationStatus
  + **Purpose:** Tracks whether an officer has acted on an agent proposal.
  + **Records:** Proposed, Confirmed, Rejected.

* **Entity Name:** ConfidenceBand
  + **Purpose:** Groups agent confidence into bands shown on the recommendation card.
  + **Records:** High, Medium, Low.

* **Entity Name:** AssessmentState
  + **Purpose:** Records how far a clinical assessment progressed.
  + **Records:** Complete, Partial, Error.

* **Entity Name:** FraudSignalType
  + **Purpose:** Categorises the irregularities detected on a claim.
  + **Records:** Facility Mismatch, Duplicate Claim, Impossible Date, Amount Outlier.

* **Entity Name:** SignalSeverity
  + **Purpose:** Ranks how strongly a signal should block automatic approval.
  + **Records:** Low, Medium, High.

* **Entity Name:** SignalStatus
  + **Purpose:** Tracks whether a signal still blocks the claim.
  + **Records:** Open, Cleared.

* **Entity Name:** CrossCheckRule
  + **Purpose:** Identifies the consistency checks run across documents on one claim.
  + **Records:** Member Name Match, Policy Number Match, Treatment Dates Consistent, Facility Match, Total Amount Match, Document Set Complete, Diagnosis Present, Service Dates In Window, Duplicate Line Check.

* **Entity Name:** CheckResult
  + **Purpose:** Records the outcome of a consistency check.
  + **Records:** Pass, Fail, Not Applicable.

* **Entity Name:** ClaimEventType
  + **Purpose:** Categorises entries on the claim timeline.
  + **Records:** Claim Created, Documents Submitted, Extraction Completed, Extraction Failed, Assessment Completed, Adjudicated, Recommendation Confirmed, Recommendation Rejected, Signal Raised, Signal Cleared, Submitted For Approval, Approved, Declined, Payment Issued, Letter Sent, Claim Closed.

* **Entity Name:** LetterType
  + **Purpose:** Defines the letters sent to a member about a claim.
  + **Records:** Approval Letter, Decline Letter, Additional Documents Request.

* **Entity Name:** PaymentStatus
  + **Purpose:** Tracks the progress of a payment order.
  + **Records:** Pending, Issued, Paid, Failed.

### 5. Roles & Permissions

* **Role: Claim Officer**
  + Claim: Edit Access (only claims where the AssignedTo attribute matches the logged-in user, or where AssignedTo is empty).
  + ClaimLine: Edit Access (only lines belonging to claims assigned to them, based on the ClaimId relationship).
  + ClinicalRecommendation: Edit Access (only recommendations on their assigned claims).
  + FraudSignal: Edit Access (only signals on their assigned claims).
  + ClaimDocument: View Access.
  + ClaimLineBreakdown: View Access.
  + ChainRun: View Access.
  + LimitLedger: View Access.
  + AgentRun: View Access.
  + ClaimEvent: View Access.
  + ClaimLetter: Edit Access (only letters on their assigned claims).
  + Policy, Member, PolicyBenefitSnapshot: View Access.
  + PaymentOrder: View Access.
  + ExclusionList, BenefitItemMap, BenefitCategoryMap, ReasonCodeDictionary: View Access.
  + IdpSession, IdpFieldMap, IdpReplay, AccuracyRun: No Access.
  + **Special Permission:** Can assign an unassigned claim to themselves. Can confirm or reject a clinical recommendation, recording a reason. Can clear a fraud signal, recording a reason. Can submit a claim for approval.

* **Role: Claim Approver**
  + Claim: Edit Access (all records).
  + ClaimLine: View Access (all records).
  + ClinicalRecommendation: View Access (all records).
  + FraudSignal: View Access (all records).
  + ClaimLetter: Edit Access (all records).
  + PaymentOrder: Edit Access (all records).
  + ClaimDocument, ClaimLineBreakdown, ChainRun, LimitLedger, AgentRun, ClaimEvent: View Access.
  + Policy, Member, PolicyBenefitSnapshot: View Access.
  + ExclusionList, BenefitItemMap, BenefitCategoryMap, ReasonCodeDictionary: View Access.
  + IdpSession, IdpFieldMap, IdpReplay, AccuracyRun: No Access.
  + **Special Permission:** Can approve a claim, decline a claim recording a decision code, or return it to the officer. Can send a letter. Approvers cannot edit cost lines.

* **Role: Claim Operations**
  + ClaimDocument: Edit Access (all records).
  + Claim: Edit Access (all records, limited to reassigning and retrying extraction).
  + IdpSession, IdpFieldMap, IdpReplay, AccuracyRun: Full Access.
  + ExclusionList, BenefitItemMap, BenefitCategoryMap, ReasonCodeDictionary: Full Access.
  + AgentRun, ClaimEvent, ChainRun: View Access.
  + ClaimLine, ClinicalRecommendation, FraudSignal: View Access.
  + Policy, Member, PolicyBenefitSnapshot, LimitLedger, PaymentOrder, ClaimLetter: View Access.
  + **Special Permission:** Can retry a failed extraction, switch the application to replay mode, and reassign a claim to another officer.

* **Role: Presenter**
  + All entities: View Access.
  + **Special Permission:** Can open the agent trace and results screens. Cannot change any claim data.

### 6. Main Features & Screens

1. **Dashboard:** A landing page for Claim Approvers and Presenters.
   Dashboard should consist of:
   - Claims In Adjudication as a counter
   - Claims Pending Approval as a counter
   - Claims Blocked as a counter
   - Total Payable This Month as a counter with a sum aggregation
   - Claims by Status as a Donut Chart
   - Claims by Treatment Type as a Donut Chart
   - Payable Amount by Month as a Vertical Bar Chart
   - Average Extraction Confidence by Document Type as a Horizontal Bar Chart

2. **Claim Worklist:** A **table** screen listing every claim the logged-in officer may work. Columns are claim number, member name, treatment type, status, total claimed, total payable, days since creation, and assignee. The screen supports filtering by status and by assignee, searching by claim number or member name, and sorting by days since creation. Selecting a row opens Claim Detail.

3. **Claim Detail:** A **master detail** screen showing one claim. The header shows claim number, status, assignee, member and policy. Related lists on this screen are cost lines, documents, clinical recommendations, fraud signals, cross-check results, chain runs and the claim timeline. On the cost line list the officer can edit a line, and each line shows its clinical recommendation with the evidence, the confidence band, and buttons to confirm or reject. Rejecting requires a reason. The screen has an action to submit the claim for approval.

4. **Document Review:** A **table** screen listing the documents on a claim with their type, template, extraction confidence and error code. The screen has an action to request additional documents from the member and an action to retry a failed extraction.

5. **Settlement Proposal:** A screen showing, per benefit code, the sum insured, remaining limit, per-visit cap, coinsurance percentage, amount claimed, amount payable and amount borne by the member, with totals. The screen shows the outcome of each auto-approval condition and has an action to submit for approval.

6. **Approval Queue:** A **table** screen for Claim Approvers listing claims awaiting approval, with actions to approve, decline recording a decision code, or return the claim to the officer.

7. **Operator Queue:** A **table** screen for Claim Operations listing claims whose extraction failed or whose assessment is incomplete, with actions to retry extraction, switch to replay mode and reassign.

8. **Agent Trace:** A **table** screen listing the agent runs for one claim in time order, showing agent name, step, tool called, hand-off target, duration and whether the step failed. Rows expand to show the recorded input and output.

9. **Policy and Benefits:** A **master detail** screen showing a policy, its covered members, and the benefit snapshot with limits used and remaining.

10. **Verification Queue:** A **table** screen listing cost lines whose confidence is below a threshold and that no officer has yet marked as verified.

11. **Administration:** A screen for Claim Operations to maintain the exclusion list, the benefit item map, the benefit category map and the reason code wording. Each list uses a **list with popup** pattern for quick editing.

### 7. External Integrations

Medical documents are read by an external document-extraction service reached over REST. The extraction result is written to the RawExtractionJson attribute of ClaimDocument on first read and is never updated afterwards. When the extraction service returns an error, the claim moves to the Extraction Error status and appears in the Operator Queue; it is never declined automatically.

All monetary values are produced by a deterministic server-side calculation that is added to this application after generation. That calculation writes Claim.TotalPayable, ClaimLine.AmountPayable, the ClaimLineBreakdown rows and the LimitLedger rows. Do not generate this calculation and do not generate any screen logic that computes a monetary value. Generate the screens so that each monetary attribute is displayed as read-only, and give the Claim Detail and Settlement Proposal screens an action that requests a recalculation.
