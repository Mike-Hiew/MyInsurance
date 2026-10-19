# ClaimMVP — data model hiện có (rev 128)

Xuất tự động từ MCP `context_entities` (app `2a401c4d-d87e-4c44-8692-9ad661840125`, `owned_only: false`) ngày 2026-09-20.
Mỗi dòng = một attribute. **Không tóm tắt.** FK suy từ kiểu `<Entity> Identifier`; `(?)` = entity đích không có trong payload (thường là `User` của System).
`Mandatory` = `isMandatory` của attribute (ODC: *Is Mandatory*). Length/Decimals rỗng = mặc định của kiểu. Thứ tự attribute: `Id` trước, còn lại theo bảng chữ cái (API không trả thứ tự Studio).

**Tổng:** 18 entity do ClaimMVP sở hữu (14 entity thường · 4 static) · 62 entity kế thừa từ thư viện (System / OutSystemsUI / OutSystemsCharts / OutSystemsMaps — liệt kê tên ở cuối, không phải bài so).

## 1. Entity thường (ClaimMVP)

| Entity | Attributes | Public | FK ra ngoài |
|---|---|---|---|
| AgentRun | 15 | True | Claim |
| Assessment | 9 | True | Claim |
| CalcSnapshot | 10 | True | ChainRun, Claim |
| ChainRun | 13 | True | Claim |
| Claim | 17 | True | ClaimStatus, Member, Policy, TreatmentType |
| ClaimDocument | 9 | True | Claim |
| ClaimLine | 14 | True | Claim, ClaimDocument, DecisionCode |
| LimitLedger | 9 | True | Claim, Member, Policy |
| Member | 7 | True | Policy |
| Policy | 7 | True | PolicyStatus |
| PolicyBenefitSnapshot | 10 | True | Policy |
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
> One row per clinical-assessment run of a claim

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

### CalcSnapshot
> key `74888a36-1a74-40b9-b951-be0b10722980` · public: True · attributes: 10 · last change: 2026-09-19
> One row per step of the 14-step chain for one ChainRun (display-only)

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
> One execution of RunAdjudicationChain for a claim. InputsHash lets the console detect stale amounts after an edit. Append-only: compare two runs, never update one. BRD 10.1.

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
> key `c6aa05c3-cfae-4a79-a85b-90d9b7f3c449` · public: True · attributes: 17 · last change: 2026-09-19

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
| 12 | MemberId | Member Identifier |  |  | yes |  | Member |
| 13 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 14 | StatusId | ClaimStatus Identifier |  |  | yes |  | ClaimStatus (static) |
| 15 | TotalClaimed | Decimal | 18 | 0 | no |  |  |
| 16 | TotalPayable | Decimal | 18 | 0 | no |  |  |
| 17 | TreatmentTypeId | TreatmentType Identifier |  |  | yes |  | TreatmentType (static) |

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
> key `f35e70e5-5e25-4427-874b-13f104b82dc5` · public: True · attributes: 14 · last change: 2026-09-19

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
| 9 | DecisionCodeId | DecisionCode Identifier |  |  | no |  | DecisionCode (static) |
| 10 | DecisionReason | Text | 500 |  | no |  |  |
| 11 | Description | Text | 500 |  | no |  |  |
| 12 | Quantity | Integer |  |  | no |  |  |
| 13 | ServiceDate | Date |  |  | no |  |  |
| 14 | SourceDoc | Text | 100 |  | no |  |  |

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
> key `887acb3e-3fef-480f-9813-6987f67b0e88` · public: True · attributes: 7 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | DateOfBirth | Date |  |  | no |  |  |
| 3 | FullName | Text | 200 |  | yes |  |  |
| 4 | Gender | Text | 10 |  | no |  |  |
| 5 | MemberNo | Text | 50 |  | yes |  |  |
| 6 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 7 | RelationshipToHolder | Text | 50 |  | no |  |  |

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
| 7 | StatusId | PolicyStatus Identifier |  |  | yes |  | PolicyStatus (static) |

### PolicyBenefitSnapshot
> key `669c5cce-1910-4f71-9490-015b69cc326d` · public: True · attributes: 10 · last change: 2026-09-19

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Long Integer |  |  | yes | PK |  |
| 2 | AnnualLimit | Decimal | 18 | 0 | no |  |  |
| 3 | BenefitCode | Text | 50 |  | yes |  |  |
| 4 | BenefitName | Text | 200 |  | yes |  |  |
| 5 | CoinsurancePct | Decimal | 37 | 8 | no |  |  |
| 6 | PerVisitCap | Decimal | 18 | 0 | no |  |  |
| 7 | PolicyId | Policy Identifier |  |  | yes |  | Policy |
| 8 | SnapshotDate | Date |  |  | yes |  |  |
| 9 | SumInsured | Decimal | 18 | 0 | no |  |  |
| 10 | WaitingPeriodDays | Integer |  |  | no |  |  |

### RecEvidence
> key `5a0be826-b0f6-4c3c-ab8b-c59dd06469f8` · public: True · attributes: 6 · last change: 2026-09-19
> Evidence items cited by a recommendation

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
> AI proposal on ONE claim line

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
> Product terms register

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


### ClaimStatus  *(static)*
> key `137904b2-1943-4915-a4a9-065cd60e4f02` · public: True · attributes: 4 · last change: 2026-09-20

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | Order | Integer |  |  | yes |  |  |

### DecisionCode  *(static)*
> key `87180646-df8a-4981-887d-b5496773f1cf` · public: True · attributes: 4 · last change: 2026-09-20

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | Order | Integer |  |  | yes |  |  |

### PolicyStatus  *(static)*
> key `bed05caa-c5e9-437f-8c39-c747a395cbe2` · public: True · attributes: 4 · last change: 2026-09-20

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | Order | Integer |  |  | yes |  |  |

### TreatmentType  *(static)*
> key `c79b470b-9cee-4835-8609-2a6a14277158` · public: True · attributes: 4 · last change: 2026-09-20

| # | Attribute | Data type | Length | Decimals | Mandatory | PK | FK → |
|---|---|---|---|---|---|---|---|
| 1 | Id | Integer |  |  | yes | PK |  |
| 2 | Is_Active | Boolean |  |  | yes |  |  |
| 3 | Label | Text | 50 |  | yes |  |  |
| 4 | Order | Integer |  |  | yes |  |  |

## 3. Entity kế thừa từ thư viện (chỉ tên — ngoài phạm vi so sánh)

- **(System)** (8): ActivityInstance, ActivityKind*, ActivityStatus*, HumanActivityRole, ProcessInstance, ProcessStatus*, Role, User
- **OutSystemsCharts** (7): AxisValuesType*, HighchartModules*, LegendLayout*, LegendPosition*, SeriesType*, StackingType*, ValuesType*
- **OutSystemsMaps** (12): DirectionsProvider*, DrawingToolsPosition*, MapErrors*, MapEventTriggered*, MarkerEventTriggered*, SearchPlacesErrors*, SearchTypes*, ShapeEventTriggered*, Style*, TravelMode*, Type*, Zoom*
- **OutSystemsUI** (35): AccordionIconPosition*, AccordionIconType*, Alert*, AnimationType*, Autoplay*, BooleanTypes*, BreakColumns*, CarouselDirection*, CarouselNavigation*, Color*, ColumnBreak*, DatePickerLanguage*, DatePickerTimeFormat*, DatePickerWeekDay*, DeviceResponsive*, Direction*, FloatingPosition*, Gradient*, GutterSize*, IdentityProvider*, MenuAction*, MessageStatus*, Month*, Orientation*, Position*, ScrollbarStyle*, Shape*, SideMenuBehavior*, Size*, Space*, Speed*, StackedCardsPosition*, Steps*, Trigger*, VideoState*

`*` = static entity.
