# ClaimMVP — data model hiện có (rev 138)

Xuất tự động từ MCP `context_entities` (app `2a401c4d-d87e-4c44-8692-9ad661840125`) ngày 2026-09-21, sau đợt B (rev 133/134), B2 (rev 136) và B3 (rev 137/138 — không đổi attribute; chỉ `CapMode` thành Public để MyInsurance tham chiếu `Entities.CapMode.Own`). Bản rev 128 giữ ở `current-model.rev128.bak.md`.
Mỗi dòng = một attribute. **Không tóm tắt.** FK suy từ kiểu `<Entity> Identifier`; `(?)` = entity đích không có trong payload (thường là `User` của System).
`Mandatory` = `isMandatory` của attribute (ODC: *Is Mandatory*). Length/Decimals rỗng = mặc định của kiểu. Thứ tự attribute: `Id` trước, còn lại theo bảng chữ cái (API không trả thứ tự Studio).

**Tổng:** 26 entity do ClaimMVP sở hữu (15 entity thường · 11 static) · entity kế thừa từ thư viện liệt kê tên ở cuối (không đổi so với rev 128).

**Đợt B (21/09) thêm:** static `RenewalStatus` (3 record) · `Member` +6 · `PolicyBenefitSnapshot` +10 · `Claim` +4 · entity mới `BenefitWaitingPeriod` (unique index `UQ_BenefitWaitingPeriod`) · Server Function `COUNT_UNLIMITED()` = 0. **Đợt B2 (21/09) thêm:** `PolicyBenefitSnapshot` +2 (`PerDayCap`, `PerEventCap`) → 22 attribute. **C-PREP-1 (rev 136):** static `CapMode` (Own / Included) · `PolicyBenefitSnapshot.CapModeId` → 23 attribute.

## 1. Entity thường (ClaimMVP)

| Entity | Attributes | Public | FK ra ngoài |
|---|---|---|---|
| AgentRun | 15 | True | Claim |
| Assessment | 9 | True | Claim |
| BenefitWaitingPeriod | 5 | True | ConditionClass |
| CalcSnapshot | 10 | True | ChainRun, Claim |
| ChainRun | 13 | True | Claim |
| Claim | 21 | True | ClaimStatus, LossType, Member, Policy, TreatmentType |
| ClaimDocument | 9 | True | Claim |
| ClaimLine | 16 | True | Claim, ClaimDocument, DecisionCode, DeclineReason, LineDecision |
| LimitLedger | 9 | True | Claim, Member, Policy |
| Member | 13 | True | Policy, RenewalStatus |
| Policy | 7 | True | PolicyStatus |
| PolicyBenefitSnapshot | 23 | True | CapMode, Policy |
| RecEvidence | 6 | True | Recommendation |
| Recommendation | 12 | True | Assessment, ClaimLine |
| TermRegister | 8 | True |  |

### AgentRun
> key `29561723-bf85-4f3c-a214-885963b12b24` · public: True · attributes: 15 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AgentName | Text | 100 |  | yes |  |  |
| 3 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 4 | CorrelationId | Text | 64 |  | no |  |  |
| 5 | DurationMs | Integer |  |  | no |  |  |
| 6 | FinishedOn | Date Time |  |  | no |  |  |
| 7 | HandoffTo | Text | 100 |  | no |  |  |
| 8 | InputJson | Text | 2147483647 |  | no |  |  |
| 9 | IsError | Boolean |  |  | yes |  |  |
| 10 | KnowledgeVersion | Text | 100 |  | no |  |  |
| 11 | ModelId | Text | 100 |  | no |  |  |
| 12 | OutputJson | Text | 2147483647 |  | no |  |  |
| 13 | StartedOn | Date Time |  |  | yes |  |  |
| 14 | Step | Text | 100 |  | no |  |  |
| 15 | ToolName | Text | 100 |  | no |  |  |

### Assessment
> key `e0c02a38-d7b9-4e90-9375-ea01ab6c3ad4` · public: True · attributes: 9 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AssessedOn | Date Time |  |  | yes |  |  |
| 3 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 4 | KnowledgeVersion | Text | 50 |  | no |  |  |
| 5 | LinesAssessed | Integer |  |  | no |  |  |
| 6 | LinesTotal | Integer |  |  | no |  |  |
| 7 | Note | Text | 500 |  | no |  |  |
| 8 | RecommendationCount | Integer |  |  | no |  |  |
| 9 | State | Text | 20 |  | yes |  |  |

### BenefitWaitingPeriod
> key `ccf099e8-6c28-4c0f-8dd0-aa8570fc70da` · public: True · attributes: 5 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | BenefitGroupCode | Text | 10 |  | yes |  |  |
| 3 | ConditionClassId | ConditionClass Identifier |  |  | yes |  | ConditionClass |
| 4 | Days | Integer |  |  | yes |  |  |
| 5 | PlanCode | Text | 20 |  | yes |  |  |

### CalcSnapshot
> key `74888a36-1a74-40b9-b951-be0b10722980` · public: True · attributes: 10 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | ChainRunId | ChainRun Identifier |  |  | yes |  | ChainRun |
| 3 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 4 | Detail | Text | 500 |  | no |  |  |
| 5 | Outcome | Text | 20 |  | no |  |  |
| 6 | ReasonCode | Text | 20 |  | no |  |  |
| 7 | ResultAmount | Decimal | 18 | 0 | no |  |  |
| 8 | StepLabel | Text | 10 |  | no |  |  |
| 9 | StepName | Text | 100 |  | yes |  |  |
| 10 | StepNo | Integer |  |  | yes |  |  |

### ChainRun
> key `c0504122-bc6a-400f-afbb-e2a3f8591a81` · public: True · attributes: 13 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 3 | GateResultJson | Text | 2147483647 |  | no |  |  |
| 4 | InputsHash | Text | 64 |  | no |  |  |
| 5 | RunAt | Date Time |  |  | yes |  |  |
| 6 | RunNo | Integer |  |  | yes |  |  |
| 7 | StepOutputsJson | Text | 2147483647 |  | no |  |  |
| 8 | TotalCovered | Decimal | 18 | 0 | no |  |  |
| 9 | TotalCustomerBorne | Decimal | 18 | 0 | no |  |  |
| 10 | TotalPayable | Decimal | 18 | 0 | no |  |  |
| 11 | TotalRequested | Decimal | 18 | 0 | no |  |  |
| 12 | TriggeredBy | Text | 100 |  | no |  |  |
| 13 | Version | Text | 20 |  | no |  |  |

### Claim
> key `c6aa05c3-cfae-4a79-a85b-90d9b7f3c449` · public: True · attributes: 21 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AdmissionDate | Date |  |  | no |  |  |
| 3 | AssignedTo | Text | 100 |  | no |  |  |
| 4 | ClaimNo | Text | 30 |  | yes |  |  |
| 5 | CreatedOn | Date Time |  |  | yes |  |  |
| 6 | Diagnosis | Text | 200 |  | no |  |  |
| 7 | DischargeDate | Date |  |  | no |  |  |
| 8 | EventDate | Date |  |  | yes |  |  |
| 9 | FacilityName | Text | 200 |  | no |  |  |
| 10 | IcdCode | Text | 10 |  | no |  |  |
| 11 | IsInNetwork | Boolean |  |  | no |  |  |
| 12 | LossTypeId | LossType Identifier |  |  | no |  | LossType |
| 13 | MemberId | Member Identifier |  |  | yes |  | Member |
| 14 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 15 | RuleSetRef | Text | 50 |  | no |  |  |
| 16 | StatusId | ClaimStatus Identifier |  |  | yes |  | ClaimStatus |
| 17 | TotalClaimed | Decimal | 18 | 0 | no |  |  |
| 18 | TotalPayable | Decimal | 18 | 0 | no |  |  |
| 19 | TreatmentFrom | Date |  |  | no |  |  |
| 20 | TreatmentTo | Date |  |  | no |  |  |
| 21 | TreatmentTypeId | TreatmentType Identifier |  |  | yes |  | TreatmentType |

### ClaimDocument
> key `3ba88938-54d4-485c-a59c-f7e65f3c5a8d` · public: True · attributes: 9 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 3 | Confidence | Decimal | 37 | 8 | no |  |  |
| 4 | ErrorCode | Text | 50 |  | no |  |  |
| 5 | FileName | Text | 260 |  | no |  |  |
| 6 | IdpRequestId | Text | 100 |  | no |  |  |
| 7 | RawExtractionJson | Text | 2147483647 |  | no |  |  |
| 8 | TemplateId | Text | 50 |  | no |  |  |
| 9 | UploadedOn | Date Time |  |  | yes |  |  |

### ClaimLine
> key `f35e70e5-5e25-4427-874b-13f104b82dc5` · public: True · attributes: 16 · last change: 2026-09-20

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AmountClaimed | Decimal | 18 | 0 | no |  |  |
| 3 | AmountPayable | Decimal | 18 | 0 | no |  |  |
| 4 | BenefitCode | Text | 50 |  | no |  |  |
| 5 | Category | Text | 20 |  | no |  |  |
| 6 | ClaimDocumentId | ClaimDocument Identifier |  |  | no |  | ClaimDocument |
| 7 | ClaimId | Claim Identifier |  |  | yes |  | Claim |
| 8 | Confidence | Decimal | 37 | 8 | no |  |  |
| 9 | DecisionCodeId | DecisionCode Identifier |  |  | no |  | DecisionCode |
| 10 | DecisionReason | Text | 500 |  | no |  |  |
| 11 | DeclineReasonId | DeclineReason Identifier |  |  | no |  | DeclineReason |
| 12 | Description | Text | 500 |  | no |  |  |
| 13 | LineDecisionId | LineDecision Identifier |  |  | no |  | LineDecision |
| 14 | Quantity | Integer |  |  | no |  |  |
| 15 | ServiceDate | Date |  |  | no |  |  |
| 16 | SourceDoc | Text | 100 |  | no |  |  |

### LimitLedger
> key `30eeea9a-e354-4227-9e40-69f1f39a47a9` · public: True · attributes: 9 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | BenefitCode | Text | 50 |  | yes |  |  |
| 3 | LastClaimId | Claim Identifier |  |  | no |  | Claim |
| 4 | MemberId | Member Identifier |  |  | yes |  | Member |
| 5 | PeriodYear | Integer |  |  | yes |  |  |
| 6 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 7 | RemainingAmount | Decimal | 18 | 0 | no |  |  |
| 8 | UpdatedOn | Date Time |  |  | no |  |  |
| 9 | UsedAmount | Decimal | 18 | 0 | no |  |  |

### Member
> key `887acb3e-3fef-480f-9813-6987f67b0e88` · public: True · attributes: 13 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | BenefitLevelNo | Integer |  |  | no |  |  |
| 3 | CardNo | Text | 20 |  | no |  |  |
| 4 | ContinuousCoverFrom | Date |  |  | no |  |  |
| 5 | CoverageFrom | Date |  |  | no |  |  |
| 6 | CoverageTo | Date |  |  | no |  |  |
| 7 | DateOfBirth | Date |  |  | no |  |  |
| 8 | FullName | Text | 200 |  | yes |  |  |
| 9 | Gender | Text | 10 |  | no |  |  |
| 10 | MemberNo | Text | 50 |  | yes |  |  |
| 11 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 12 | RelationshipToHolder | Text | 50 |  | no |  |  |
| 13 | RenewalStatusId | RenewalStatus Identifier |  |  | no |  | RenewalStatus |

### Policy
> key `0b3f261b-082f-4a9c-8265-49ed0b230638` · public: True · attributes: 7 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | EffectiveDate | Date |  |  | yes |  |  |
| 3 | ExpiryDate | Date |  |  | yes |  |  |
| 4 | PolicyCode | Text | 20 |  | yes |  |  |
| 5 | PolicyHolderName | Text | 200 |  | yes |  |  |
| 6 | PolicyNo | Text | 50 |  | yes |  |  |
| 7 | StatusId | PolicyStatus Identifier |  |  | yes |  | PolicyStatus |

### PolicyBenefitSnapshot
> key `669c5cce-1910-4f71-9490-015b69cc326d` · public: True · attributes: 23 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AnnualLimit | Decimal | 18 | 0 | no |  |  |
| 3 | BenefitCode | Text | 50 |  | yes |  |  |
| 4 | BenefitName | Text | 200 |  | yes |  |  |
| 5 | CapModeId | CapMode Identifier |  |  | no |  | CapMode |
| 6 | CoinsurancePct | Decimal | 37 | 8 | no |  |  |
| 7 | CopayPct | Text | 8 |  | no |  |  |
| 8 | CountLimit | Integer |  |  | no |  |  |
| 9 | CountUnit | Text | 10 |  | no |  |  |
| 10 | DeductionCondition | Text | 50 |  | no |  |  |
| 11 | DeductionPct | Integer |  |  | no |  |  |
| 12 | Depth | Integer |  |  | no |  |  |
| 13 | EffectiveFrom | Date |  |  | no |  |  |
| 14 | EffectiveTo | Date |  |  | no |  |  |
| 15 | LimitType | Text | 20 |  | no |  |  |
| 16 | ParentBenefitCode | Text | 50 |  | no |  |  |
| 17 | PerDayCap | Decimal | 18 | 0 | no |  |  |
| 18 | PerEventCap | Decimal | 18 | 0 | no |  |  |
| 19 | PerVisitCap | Decimal | 18 | 0 | no |  |  |
| 20 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 21 | SnapshotDate | Date |  |  | yes |  |  |
| 22 | SumInsured | Decimal | 18 | 0 | no |  |  |
| 23 | WaitingPeriodDays | Integer |  |  | no |  |  |

### RecEvidence
> key `5a0be826-b0f6-4c3c-ab8b-c59dd06469f8` · public: True · attributes: 6 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | Kind | Text | 20 |  | yes |  |  |
| 3 | RecommendationId | Recommendation Identifier |  |  | yes |  | Recommendation |
| 4 | Reference | Text | 200 |  | yes |  |  |
| 5 | SortOrder | Integer |  |  | no |  |  |
| 6 | Summary | Text | 500 |  | no |  |  |

### Recommendation
> key `f65ba38d-e2a2-4f2a-8967-4c1d218f95a4` · public: True · attributes: 12 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AssessmentId | Assessment Identifier |  |  | yes |  | Assessment |
| 3 | Band | Text | 10 |  | no |  |  |
| 4 | ClaimLineId | ClaimLine Identifier |  |  | yes |  | ClaimLine |
| 5 | Code | Text | 20 |  | yes |  |  |
| 6 | Confidence | Decimal | 5 | 2 | no |  |  |
| 7 | DecidedBy | Text | 100 |  | no |  |  |
| 8 | DecidedOn | Date Time |  |  | no |  |  |
| 9 | Decision | Text | 20 |  | no |  |  |
| 10 | DecisionReason | Text | 500 |  | no |  |  |
| 11 | Rationale | Text | 2000 |  | no |  |  |
| 12 | Title | Text | 200 |  | no |  |  |

### TermRegister
> key `37109329-82b3-443e-adc9-e946cc6c3b9b` · public: True · attributes: 8 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AppliedCount30d | Integer |  |  | no |  |  |
| 3 | ClauseRef | Text | 100 |  | no |  |  |
| 4 | Code | Text | 20 |  | yes |  |  |
| 5 | IsActive | Boolean |  |  | yes |  |  |
| 6 | Name | Text | 200 |  | yes |  |  |
| 7 | Scope | Text | 20 |  | yes |  |  |
| 8 | TermClass | Text | 1 |  | yes |  |  |

## 2. Static entity (ClaimMVP) — attribute (records ở `current-enums.md`)

### CapMode  *(static)*
> key `0c77722b-a1f7-43ab-a973-dcac1b58760c` · public: True (rev 137) · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | no |  |  |
| 3 | Label | Text | 50 |  | no |  |  |
| 4 | LabelVi | Text | 100 |  | no |  |  |
| 5 | Order | Integer |  |  | no |  |  |
| 6 | Value | Text | 50 |  | no |  |  |

### ClaimStatus  *(static)*
> key `137904b2-1943-4915-a4a9-065cd60e4f02` · public: True · attributes: 8 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | CustomerLabelEn | Text | 100 |  | yes |  |  |
| 3 | CustomerLabelVi | Text | 100 |  | yes |  |  |
| 4 | Is_Active | Boolean |  |  | yes |  |  |
| 5 | Label | Text | 50 |  | yes |  |  |
| 6 | LabelVi | Text | 100 |  | yes |  |  |
| 7 | Order | Integer |  |  | yes |  |  |
| 8 | Value | Text | 50 |  | yes |  |  |

### ConditionClass  *(static)*
> key `6562b8f9-799d-4b21-881f-032c2658ff66` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

### DecisionCode  *(static)*
> key `87180646-df8a-4981-887d-b5496773f1cf` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

### DeclineReason  *(static)*
> key `35ac5fd2-583e-4df2-a719-d690fd8c1015` · public: True · attributes: 7 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Level | Text | 20 |  | no |  |  |
| 6 | Order | Integer |  |  | yes |  |  |
| 7 | Value | Text | 50 |  | yes |  |  |

### LineDecision  *(static)*
> key `fbd73083-f7f4-4189-9a21-1b7f81db051d` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

### LossType  *(static)*
> key `88810d05-e3ec-4f87-b82f-a770431d3e3b` · public: True · attributes: 8 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | BenefitGroup | Text | 10 |  | no |  |  |
| 3 | Is_Active | Boolean |  |  | yes |  |  |
| 4 | Label | Text | 50 |  | yes |  |  |
| 5 | LabelVi | Text | 100 |  | yes |  |  |
| 6 | Order | Integer |  |  | yes |  |  |
| 7 | TreatmentTypeId | TreatmentType Identifier |  |  | no |  | TreatmentType |
| 8 | Value | Text | 50 |  | yes |  |  |

### PolicyStatus  *(static)*
> key `bed05caa-c5e9-437f-8c39-c747a395cbe2` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

### RenewalStatus  *(static)*
> key `88f95f6f-b1d5-4769-879d-e877e6fc7b07` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

### ServiceCategory  *(static)*
> key `d436ed36-908b-4781-994d-719d448f5a64` · public: True · attributes: 8 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | InpatientBenefit | Text | 10 |  | no |  |  |
| 3 | Is_Active | Boolean |  |  | yes |  |  |
| 4 | Label | Text | 50 |  | yes |  |  |
| 5 | LabelVi | Text | 100 |  | yes |  |  |
| 6 | Order | Integer |  |  | yes |  |  |
| 7 | OutpatientBenefit | Text | 10 |  | no |  |  |
| 8 | Value | Text | 50 |  | yes |  |  |

### TreatmentType  *(static)*
> key `c79b470b-9cee-4835-8609-2a6a14277158` · public: True · attributes: 6 · last change: 2026-09-21

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | LabelVi | Text | 100 |  | yes |  |  |
| 5 | Order | Integer |  |  | yes |  |  |
| 6 | Value | Text | 50 |  | yes |  |  |

## 3. Entity kế thừa từ thư viện (chỉ tên — ngoài phạm vi so sánh)

- **(System)** (8): ActivityInstance, ActivityKind*, ActivityStatus*, HumanActivityRole, ProcessInstance, ProcessStatus*, Role, User
- **OutSystemsCharts** (7): AxisValuesType*, HighchartModules*, LegendLayout*, LegendPosition*, SeriesType*, StackingType*, ValuesType*
- **OutSystemsMaps** (12): DirectionsProvider*, DrawingToolsPosition*, MapErrors*, MapEventTriggered*, MarkerEventTriggered*, SearchPlacesErrors*, SearchTypes*, ShapeEventTriggered*, Style*, TravelMode*, Type*, Zoom*
- **OutSystemsUI** (35): AccordionIconPosition*, AccordionIconType*, Alert*, AnimationType*, Autoplay*, BooleanTypes*, BreakColumns*, CarouselDirection*, CarouselNavigation*, Color*, ColumnBreak*, DatePickerLanguage*, DatePickerTimeFormat*, DatePickerWeekDay*, DeviceResponsive*, Direction*, FloatingPosition*, Gradient*, GutterSize*, IdentityProvider*, MenuAction*, MessageStatus*, Month*, Orientation*, Position*, ScrollbarStyle*, Shape*, SideMenuBehavior*, Size*, Space*, Speed*, StackedCardsPosition*, Steps*, Trigger*, VideoState*

`*` = static entity.
