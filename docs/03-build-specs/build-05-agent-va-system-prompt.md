# Build 05 — Agent & System Prompt

| Mục | Giá trị |
|---|---|
| Tài liệu | Build 05 — lớp agent trên OutSystems ODC Agent Workbench |
| Phiên bản | **2.0 — 16/09/2026** (v1.0 dựng trên chuẩn MBAL-IDP, đã thay thế toàn bộ) |
| Đội | BnK Solution — Claim Platform |
| Dùng cho | Hackathon OutSystems APAC, pitch 07–08/10/2026 |
| Module đích | `CLM_Agents` (Agent Workbench), phụ thuộc `CLM_Core` + `CLM_IDP_Int` |
| **Chuẩn nghiệp vụ gốc** | **Claim MVP** — A1 v1.5, A2 v1.2, A4 v1.3, A5 v1.5, D1 v0.5, D3 v2.3 |
| Nguồn IDP | `IDP_APIs_for_Claim_MVP_v1.0` (gọi thật lên staging 03/09/2026, tenant 176) |

> **Đổi chuẩn ở v2.0.** Bản v1.0 dựng trên chuẩn MBAL-IDP production (`claim_type` MEDICAL_*, `document_type` CL05…CLM_04, Docset Map, rule R1–R5 của MBAL). **Toàn bộ phần đó đã bị thay.** Chuẩn hiện hành là **Claim MVP**: 7 loại chứng từ `T1-01`…`T2-03`, `lossType` 12 giá trị, checklist theo `lossType` với 3 mức, 7 mã từ chối đóng, `claimState` 13 giá trị, `packageState` 6 giá trị. Mã MBAL không xuất hiện ở bất kỳ đâu trong tài liệu này.

> Đây là **phần duy nhất ban giám khảo chấm**. Mọi thứ trong tài liệu phải dán được vào Agent Workbench trong ngày, không cần diễn giải thêm.

---

## 0. Nguyên tắc và ranh giới

### 0.1 Câu một dòng

**Tiền nằm trong Server Action tất định. Phán đoán nằm trong agent.** Agent đọc, diễn giải, đề xuất, giải thích — và không bao giờ viết ra một con số tiền.

Trong từ vựng Claim MVP, ranh giới này đã có tên sẵn: **Track 1** là chuỗi quy tắc tất định trên Claim Platform, sinh ra **quyết định**; **Track 2** là suy luận lâm sàng, sinh ra **khuyến nghị, không bao giờ là quyết định** (A1 §5). Guardrail **G-7** của D3 nói thẳng: *"A clinical recommendation never reaches the ledger"* — chỉ khi một con người xác nhận thì chuỗi mới chạy lại và chỉ lần chạy lại đó mới đổi số tiền.

### 0.2 Bảng chia việc

| Việc | Ai làm | Hiện thực | Vì sao không đổi chỗ được |
|---|---|---|---|
| Hiệu lực thành viên, hợp đồng, phí đã đóng | **Server Action** | Chuỗi bước **1** Member eligibility | Dữ liệu core, không có chỗ cho phán đoán |
| Thời gian chờ theo `conditionClass` | **Server Action** | Bước **2** Waiting period | Trừ ngày, so ngưỡng |
| Bệnh có sẵn (`conditionClass = PRE`, chờ 365 ngày) | **Server Action** | Bước **3** | Không phải loại trừ tuyệt đối ở sản phẩm này — là quy tắc |
| Loại trừ chung theo term register | **Server Action** | Bước **4** General exclusions | Term engine chạy mọi term trong phạm vi và ghi kết quả dù có fire hay không |
| Ánh xạ dòng chi phí → mã quyền lợi | **Server Action** | Bước **5** Benefit mapping (A5 §8) | Bảng tra. **Từ v1.4 bước này không còn từ chối gì nữa** |
| **Kiểm tra cover theo từng dòng** | **Server Action** | Bước **6** Line-level cover test | Sinh ra mã từ chối cấp dòng |
| Proviso cấp quyền lợi | **Server Action** | Bước **7** | Term engine, scope = `BENEFIT` |
| Trần/lần, trần/ngày, trần/sự kiện | **Server Action** | Bước **8** | |
| Giới hạn số lần / số ngày | **Server Action** | Bước **9** Count limit (`OCCURRENCE`, `DAY`) | |
| Đồng chi trả | **Server Action** | Bước **10** — base là số còn lại sau bước 9 | |
| Tỷ lệ khấu trừ theo hợp đồng | **Server Action** | Bước **11** — base là số còn lại sau bước 10 | |
| Trần năm của quyền lợi cha | **Server Action** | Bước **12** — đi ngược cây tới gốc | |
| Kiểm tra cấp chương trình | **Server Action** | Bước **13** | |
| Ghi ledger | **Server Action** | Bước **14** — provisional khi tính, **final khi duyệt** | Ledger sai là số dư khách sai |
| **Quyết định dòng nào NOTREL / NOTCOV** | **Agent AG-3** | `ProposeRecommendation`, `Status = Proposed` | Cần đọc tên hạng mục tiếng Việt, ICD, `serviceCategory`, KB. Đây là Track 2 |
| Áp dụng NOTREL/NOTCOV **đã được người xác nhận** vào tiền | **Server Action** | Chuỗi chạy lại, bước 6 | Khuyến nghị chưa xác nhận không chạm vào tiền (D3 G-7, D1 BR-TRACK-3/4) |
| **Đọc chứng từ, dựng checklist theo `lossType`** | **Agent AG-1** | `GetDocumentChecklist` | Phân loại chứng từ, xử lý lỗi, quyết định hỏi thêm gì |
| **Diễn giải 14 bước bằng tiếng Việt** | **Agent AG-2** | `GetChainExplanation` | Chuỗi trả dữ liệu; agent biến thành câu người đọc được |
| **Tra cơ sở y tế, ngày bất khả, hồ sơ trùng, đối chiếu chéo R1–R9** | **Agent AG-4** | `LookupProviderCatalogue`, `ValidateClaimDates`, `FindDuplicateClaims`, `GetCrossCheckResults` | Fuzzy match + suy luận ngữ cảnh |
| **Bind placeholder vào template A2 và soạn bản nháp** | **Agent AG-5** | `SaveLetterDraft` | Ngôn ngữ là việc của model; gửi đi là việc của người |
| Duyệt / từ chối / chi trả | **Người** | `ApproveClaim`, `DeclineClaim`, `IssuePaymentInstruction` | Không agent nào có tool này |

**Làm tròn (A5 §6.2):** **nhận về số đồng nguyên gần nhất, nửa làm tròn ra xa số 0, và làm tròn ở TỪNG BƯỚC chứ không phải một lần ở cuối.** Đây là điều kiện để chuỗi chạy lại sau khi cán bộ xác nhận khuyến nghị vẫn ra cùng tổng (BR-TRACK-4). Phần trăm giữ dạng basis point nguyên; làm tròn áp lên kết quả tiền, không bao giờ áp lên tỷ lệ.

### 0.3 Bảy mã từ chối — tập đóng (A1 §5.2)

| Mã | Nghĩa | Cấp | Ai sinh ra |
|---|---|---|---|
| `LIMIT` | Hết hạn mức quyền lợi | Sự kiện hoặc dòng | **Chuỗi** (bước 8, 9, 12) |
| `PERIOD` | Ngày điều trị ngoài thời hạn bảo hiểm | Sự kiện | **Chuỗi** (bước 1) |
| `NOBEN` | Chương trình không bao gồm quyền lợi này | Sự kiện hoặc dòng | **Chuỗi** (bước 5, 6) |
| `WAIT` | Chưa đủ thời gian chờ | Sự kiện hoặc dòng | **Chuỗi** (bước 2, 3) |
| `EXCL` | Thuộc điểm loại trừ của quy tắc bảo hiểm | Sự kiện hoặc dòng | **Chuỗi** (bước 4, 7 — term engine) |
| **`NOTREL`** | **Không liên quan đến chẩn đoán và điều trị** | **Chỉ cấp dòng** | **Agent AG-3 đề xuất → người xác nhận → chuỗi áp dụng** |
| **`NOTCOV`** | **Không thuộc danh mục được chi trả** | **Chỉ cấp dòng** | **Agent AG-3 đề xuất → người xác nhận → chuỗi áp dụng** |

**Agent lâm sàng chỉ được đề xuất đúng hai mã `NOTREL` và `NOTCOV`.** Năm mã còn lại là câu hỏi về hợp đồng, thuộc Track 1, do chuỗi sinh ra. D3 §I-16 nói rõ: *"An assessment returning any other code is rejected at the adapter"* — nên tool `ProposeRecommendation` phải từ chối ghi khi nhận mã ngoài hai mã này.

**Không có mã `FRAUD`.** Bảy mã trên là tập đóng và A2 §4 chỉ có bảy câu tương ứng. Hệ quả cho AG-4: xem §3.5.

**Một mã từ chối không bao giờ dùng cho sự cố hệ thống** (A1 §5.2, D3 G-3). Dịch vụ hỏng thì hồ sơ vào trạng thái vận hành và một operator nhìn thấy nó. *Một hồ sơ bị từ chối vì hệ thống hỏng thì không phân biệt được với một từ chối đúng, và đó là kết quả tệ nhất hệ thống này có thể tạo ra.*

### 0.4 Cổng tự động duyệt — BA điều kiện (A5, D1 §5.8, quyết định D-5)

| # | Điều kiện | Tham số |
|---|---|---|
| **1** | Tổng chi trả **ở mức ngưỡng hoặc thấp hơn**, **và không có gì bị cắt** — không dòng nào bị từ chối, không hạn mức nào chạm trần | `2.000.000 VND` (D1 B-03), admin đổi được không cần release |
| **2** | Đánh giá lâm sàng **đã hoàn tất và không trả về khuyến nghị nào** | — |
| **3** | **Không có term checkpoint nào đang mở** (thêm ở v1.4) | — |

Đây là **ba điều kiện, không phải bốn**. Cổng chỉ nổ trên một hồ sơ mà hợp đồng trả đủ, số nhỏ, và không có gì bị đánh dấu.

**Tín hiệu của AG-4 không phải một điều kiện cổng.** Nó là một **chốt chặn vận hành**: bất kỳ `FraudSignal` nào từ mức Medium trở lên đều đẩy hồ sơ sang `UNDER_ASSESSMENT` bất kể ba điều kiện trên, và không bao giờ tự nó sinh ra một quyết định từ chối. Xem §3.5 và §8 mục 3.

### 0.5 Danh sách tool BỊ CẤM với mọi agent

Các Server Action sau **tồn tại trong `CLM_Core` nhưng không được đưa vào tab Action Calling của bất kỳ agent nào**. Đây là danh sách để review với SA OutSystems trước pitch.

| Server Action bị cấm | Vì sao |
|---|---|
| `SetClaimLineAmounts`, `UpdateClaimLineMoney` | Ghi trực tiếp `amountPayable` / `amountDeclined` / `settlementAmount` |
| `PostLedgerConsumption`, `ReverseLedgerConsumption` | Ghi ledger. Provisional khi tính, **final khi duyệt** — cả hai đều là bước 14 của chuỗi |
| `IssuePaymentInstruction` | Tạo lệnh chi (I-11) |
| `ApproveClaim`, `DeclineClaim`, `ReturnToAssessor` | Duyệt / từ chối / trả lại |
| `SetClaimState` | Đặt `claimState` tuỳ ý. Agent chỉ được gọi `ApplyGateDecision` / `RequestAdditionalDocuments` / `ReportExtractionError` — ba action **tự quyết trạng thái đích**, không nhận `TargetState` làm tham số |
| `SubmitForApproval` | Nộp duyệt là hành vi của thẩm định viên |
| `ConfirmRecommendation`, `RejectRecommendation` | Xác nhận khuyến nghị là việc của người (D1 BR-TRACK-3). AG-3 chỉ được `Proposed` |
| `ClearFraudSignal` | Gỡ tín hiệu là việc của người |
| `ResolveTermCheckpoint` | Đóng checkpoint của term engine là phán quyết của người |
| `SendMessage`, `SendLetter` | Gửi thư thật. Agent chỉ có `SaveLetterDraft` |
| `WaiveExpectedDocument` | Miễn một chứng từ mức `EXPECTED` là quyền của thẩm định viên |
| `UpdateSiteSetting`, `ResetDemoData` | Đổi ngưỡng / reset dữ liệu |
| `PutIdpRequestResult` | Kết quả trích xuất của IDP là **bất biến** (D3 §I-09) |
| `ExecuteSQL`, `DeleteEntityRecord` bất kỳ | Không agent nào cần xoá |

**Quy tắc bổ sung — Wrapper Pattern (bài học lớn nhất của workshop A2A):** mọi tool phơi ra cho agent phải có **tham số phẳng** (Text / Integer / Decimal / Boolean / Date). Không List, không Structure lồng. Danh sách nhiều giá trị truyền dạng **chuỗi phân tách bằng dấu chấm phẩy**, Server Action tự tách. Bắt LLM dựng cấu trúc lồng → lỗi không liên tục kiểu `Missing text content in TextPart`, chỉ thấy trong ODC Portal → Monitoring → Traces.

---

## 1. Danh sách agent

6 agent, **39 tool binding**, **34 Server Action riêng biệt** (`WriteAgentRun` dùng chung cho cả 6).

| Mã | Tên asset trên tenant | Việc | Input | Tool list ĐÓNG | Output | Entity được phép ghi |
|---|---|---|---|---|---|---|
| **AG-0** | `CLM_OrchestratorAgent` | Điều phối, chờ Track 1 + Track 2 + đối chiếu chéo ngã ngũ, gọi cổng 3 điều kiện, dựng panel "Việc cần làm" | `claimNo` | `GetClaimContext` · `GetTrackStatus` · `EvaluateAutoApprovalGate` · `ApplyGateDecision` · `BuildAssessorTodo` · `InvokeAgentStep` · `WriteAgentRun` | Quyết định bước kế, kết quả cổng, danh sách việc cho thẩm định viên | `AgentRun`, `AssessorTodo` |
| **AG-1** | `CLM_IntakeAgent` | Nhận bộ hồ sơ, gọi IDP, dựng checklist theo `lossType`, xin bổ sung | `claimNo`, `lossType` | `IdpSubmitPackage` · `IdpGetExtractionResult` · `ParseEInvoiceXml` · `GetDocumentChecklist` · `UpsertCostLinesFromExtraction` · `RequestAdditionalDocuments` · `ReportExtractionError` · `WriteAgentRun` | `packageState`, danh sách thiếu theo 3 mức, cảnh báo trường không đọc được | `ClaimDocument`, `CostLine` (chỉ trường phi tiền), `AgentRun` |
| **AG-2** | `CLM_AdjudicationAgent` | Gọi chuỗi 14 bước như một tool, diễn giải, so trước/sau | `claimNo`, `triggerReason` | `RunAdjudicationChain` · `GetChainExplanation` · `CompareChainRuns` · `ListUnmappedLines` · `WriteAgentRun` | 14 dòng giải thích tiếng Việt + 1 dòng tóm tắt tiếng Anh, bảng trước/sau | `AgentRun` **duy nhất** (tiền do chuỗi ghi) |
| **AG-3** | `CLM_ClinicalAgent` | Track 2 — đọc kết quả đánh giá lâm sàng + dòng chi phí + KB → đề xuất `NOTREL`/`NOTCOV` kèm evidence và band | `claimNo`, `planCode` | `IdpStartClinicalAssessment` · `GetAssessmentStatus` · `GetClinicalRecommendations` · `GetCostLinesForReview` · `SearchKnowledgeBase` · `ProposeRecommendation` · `SummariseFindingToConsole` · `WriteAgentRun` | `Recommendation` mỗi dòng: `proposedReasonCode`, `evidence[]`, `confidenceBand` | `ClinicalAssessment`, `Recommendation` (`Status = Proposed`), `ClinicalFinding`, `AgentRun` |
| **AG-4** | `CLM_FraudSignalAgent` | Tra cơ sở y tế, ngày bất khả, hồ sơ trùng, đọc kết quả đối chiếu chéo R1–R9 | `claimNo` | `LookupProviderCatalogue` · `ValidateClaimDates` · `FindDuplicateClaims` · `GetCrossCheckResults` · `RaiseFraudSignal` · `WriteAgentRun` | Danh sách `FraudSignal` kèm severity và 1 câu tiếng Việt mỗi tín hiệu — **cho console, không cho khách** | `FraudSignal`, `AgentRun` |
| **AG-5** | `CLM_CommsAgent` | Bind placeholder vào template A2 và lưu bản nháp | `claimNo`, `messageCode` | `GetClaimResultSummary` · `GetMessageTemplate` · `GetDeclineReasonSentence` · `SaveLetterDraft` · `WriteAgentRun` | Bản nháp thư / thông báo (chưa gửi) | `ClaimLetter` (`Status = Draft`), `AgentRun` |

### 1.1 Chữ ký tool (tham số phẳng — dán vào Action Calling)

| Server Action | Input | Output | Ghi chú triển khai |
|---|---|---|---|
| `GetClaimContext` | `ClaimNo (Text)` | `ContextText (Text)`, `ClaimState (Text)`, `LossType (Text)`, `PlanCode (Text)`, `LineCount (Integer)` | Trả **một khối text đã định dạng sẵn**, không trả Structure |
| `GetTrackStatus` | `ClaimNo` | `Track1Status`, `Track2Status`, `CrossCheckStatus`, `PackageState (Text)`, `AllSettled (Boolean)`, `DetailText` | `Track2Status` ∈ `QUEUED/RUNNING/COMPLETE/PARTIAL/ERROR`; `PackageState` ∈ 6 giá trị của A4 §6 |
| `EvaluateAutoApprovalGate` | `ClaimNo` | `Cond1Pass`, `Cond2Pass`, `Cond3Pass (Boolean)`, `BlockingSignalCount (Integer)`, `FailReasonText` | **Chỉ đọc**, không ghi gì |
| `ApplyGateDecision` | `ClaimNo`, `ChainRunId (Integer)` | `ResultState (Text)`, `QueueName (Text)`, `ExplanationText` | Tự đánh giá lại cổng bên trong rồi tự quyết `claimState` đích. **Không nhận `TargetState`** |
| `BuildAssessorTodo` | `ClaimNo` | `TodoCount (Integer)`, `TodoText` | Xoá và dựng lại toàn bộ mỗi lần gọi |
| `InvokeAgentStep` | `ClaimNo`, `TargetAgentCode (Text)`, `InstructionText (Text)` | `ResponseText (Text)`, `Dispatched (Boolean)` | Wrapper. Hai nhánh, xem §5 |
| `IdpSubmitPackage` | `ClaimNo`, `ClaimDocumentId (Integer)`, `DocumentTypeHint (Text)` | `RequestId (Text)`, `Accepted (Boolean)`, `ErrorCode`, `ErrorMessage` | **I-07**. `Idempotency-Key` bắt buộc (UUIDv4, receiver giữ 24 h). Header `User-Agent` giả trình duyệt — thiếu là Cloudflare `403 error 1010` |
| `IdpGetExtractionResult` | `ClaimDocumentId` | `DocumentType (Text)`, `PackageState (Text)`, `ClaimConfidence (Decimal)`, `FieldCount`, `LineCount`, `UnreadMandatoryFieldCsv`, `ErrorCode` | **I-09**. `409 NOT_READY` + `Retry-After` là **ngoại lệ 4xx duy nhất phải thử lại** (D3 §1.3). Ghi `RawExtractionJson` một lần, bất biến. `ClaimConfidence` = **min** trên tập trường bắt buộc, không phải trung bình |
| `ParseEInvoiceXml` | `ClaimDocumentId` | `InvoiceSerial`, `InvoiceNo`, `IssueDate`, `SellerTaxCode`, `TotalAfterVat (Decimal)`, `Valid (Boolean)`, `RejectReason` | **T1-04 chỉ đi đường XML.** Ảnh hoá đơn bị từ chối tại đây với lý do rõ ràng, không bao giờ rơi sang OCR |
| `GetDocumentChecklist` | `ClaimNo` | `RequiredMissingCsv`, `ExpectedMissingCsv`, `ReceivedCsv`, `PackageState (Text)`, `ChecklistText` | Khoá trên `lossType`, ba mức `REQUIRED` / `EXPECTED` / `OPTIONAL` (A4 §6). Sinh ra **R9** |
| `UpsertCostLinesFromExtraction` | `ClaimDocumentId` | `LinesCreated (Integer)`, `LinesUpdated`, `WarningText` | Ghi `L1`…`L12`, `L15`, `L16`, `L18` và hai cột suy ra `L13 serviceCategory`, `L14 amountClaimed`. **Không ghi** bất kỳ trường tiền nào của chuỗi |
| `RequestAdditionalDocuments` | `ClaimNo`, `MissingDocTypeCsv`, `ReasonTextVi` | `NewState (Text)` | Tự đặt `PENDING_INFO`, kích `CLM-NOTIF-003` |
| `ReportExtractionError` | `ClaimNo`, `ClaimDocumentId`, `ErrorCode`, `ErrorMessage` | `NewState (Text)`, `QueueName` | Tự đặt `EXTRACTION_ERROR` + hàng đợi operator. **Không bao giờ** thành `DECLINED` |
| `RunAdjudicationChain` | `ClaimNo`, `TriggerReason (Text)` | `ChainRunId (Integer)`, `TotalClaimed`, `TotalPayable`, `TotalDeclined`, `SettlementAmount (Decimal)`, `InputsHash (Text)`, `StepsEnabledCsv` | 14 bước, một transaction, idempotent. `StepsEnabledCsv` phản ánh `adjudication_rule {step, name, enabled}` — bật/tắt **theo sản phẩm**, không theo quyền lợi |
| `GetChainExplanation` | `ChainRunId` | `ExplanationViText`, `ExplanationEnText` | 14 dòng, mỗi dòng có số bước và tên bước theo A5 §6.1 |
| `CompareChainRuns` | `ChainRunIdBefore`, `ChainRunIdAfter` | `DiffText`, `DeltaPayable (Decimal)` | Nguồn của bảng trước/sau |
| `ListUnmappedLines` | `ClaimNo` | `UnmappedCount`, `UnmappedText` | Dòng chuỗi trả `UNMAPPED` ở bước 5 |
| `IdpStartClinicalAssessment` | `ClaimNo`, `EventNo (Text)`, `Icd10Csv`, `EpisodeType`, `ServiceDateFrom`, `ServiceDateTo`, `AgeBand`, `Sex`, `KnowledgeBaseVersion` | `AssessmentId (Text)`, `State (Text)`, `ErrorMessage` | **I-15**. Tham số phẳng là bắt buộc ở đây: payload thật có `clinicalContext` + `lines[]` lồng, Server Action tự dựng. **Không gửi định danh bệnh nhân** — chỉ `ageBand` và `sex`, không tên, không ngày sinh, không số thẻ, không `memberId` |
| `GetAssessmentStatus` | `ClaimNo` | `State (Text)`, `ElapsedSeconds (Integer)` | `409 NOT_READY` phải thử lại theo `Retry-After`, không được bỏ cuộc |
| `GetClinicalRecommendations` | `ClaimNo` | `RecommendationText`, `LinesAssessedCsv`, `State (Text)`, `KnowledgeBaseVersion`, `ItemCount (Integer)` | **I-16**. Xem §3.4 về cách đọc đầu ra thật của IDP hôm nay |
| `GetCostLinesForReview` | `ClaimNo` | `LinesText` (mỗi dòng: `L1 · L2 tên hạng mục · L13 serviceCategory · L5 số lượng · L7 thành tiền · L16 ICD dòng · confidence`) | Không kèm bất kỳ trường tiền đã tính nào |
| `SearchKnowledgeBase` | `QueryText`, `PlanCode`, `TopN (Integer)` | `ResultText`, `KbVersion (Text)` | KB ghim theo hồ sơ (D1 BR-KB-4). **Không có clinical sign-off** — D1 RK-08 vẫn mở ở mức High/High |
| `ProposeRecommendation` | `ClaimNo`, `LineNo (Integer)`, `ProposedReasonCode (Text: NOTREL\|NOTCOV)`, `Confidence (Decimal)`, `ConfidenceBand (Text: HIGH\|MEDIUM\|LOW)`, `EvidenceCsv (Text)`, `RationaleVi (Text)` | `RecommendationId`, `Accepted (Boolean)`, `RejectReason` | **Từ chối ghi** khi `EvidenceCsv` rỗng (D1 BR-KB-2) **hoặc** khi mã nằm ngoài hai mã cho phép. Luôn `Status = Proposed` |
| `SummariseFindingToConsole` | `ClaimNo`, `FindingName`, `Severity (Text)`, `FindingVi`, `ForwardToFraud (Boolean)` | `FindingId` | `ForwardToFraud = True` đẩy sang hàng chờ của AG-4 |
| `LookupProviderCatalogue` | `ProviderName (Text)`, `ProviderTaxCode (Text)` | `MatchedName`, `ProviderCode`, `FacilityCountry`, `Score (Decimal)`, `Found (Boolean)` | Không khớp → **cảnh báo, không chặn** (A4 §4.1 field 1). `FacilityCountry` là `null` khi địa chỉ in trên tờ không suy ra được — **cấm đoán quốc gia từ tên cơ sở** (A4 field 18) |
| `ValidateClaimDates` | `ClaimNo` | `Pass (Boolean)`, `ViolationCsv`, `DetailText` | Ngày ở tương lai · vào viện sau ra viện · ngày điều trị ngoài thời hạn bảo hiểm (V-01-4) · `L18` ngày điều trị theo dòng không nhất quán |
| `FindDuplicateClaims` | `ClaimNo` | `DuplicateFound (Boolean)`, `DuplicateClaimNoCsv`, `MatchReason` | Trùng member + chồng lấn khoảng điều trị · trùng hash chứng từ · trùng `KHHDon`+`SHDon` của hoá đơn |
| `GetCrossCheckResults` | `ClaimNo` | `FailedRuleCsv`, `WarningCount (Integer)`, `DetailText` | **R1–R9 của A4 §5.** Xem §3.5. **Không có gì được tự sửa** — mỗi rule sinh ra một kết quả có nhãn, neo tại trường |
| `RaiseFraudSignal` | `ClaimNo`, `SignalType (Text)`, `Severity (Text: LOW\|MEDIUM\|HIGH)`, `EvidenceCsv`, `ExplanationVi` | `SignalId`, `Accepted (Boolean)` | `SignalType` ∈ `PROVIDER_UNMATCHED · DATE_IMPOSSIBLE · DUPLICATE · INVOICE_MISMATCH · CROSSCHECK_FAIL · OTHER_PAYER_PRESENT` |
| `GetClaimResultSummary` | `ClaimNo` | `SummaryText`, `TotalClaimed`, `SettlementAmount`, `TotalDeclined (Decimal)`, `DeclineByCodeText`, `RemainingLimitText` | **Nguồn số duy nhất cho AG-5.** Đọc từ `ChainRun` mới nhất |
| `GetMessageTemplate` | `MessageCode (Text)`, `Locale (Text: vi\|en)` | `TemplateText`, `TemplateVersion`, `PlaceholderCsv` | `MessageCode` ∈ `CLM-LETTER-001` · `CLM-LETTER-002` · `CLM-NOTIF-001`…`-009`. Trả **kèm danh sách placeholder** để agent biết phải bind cái gì |
| `GetDeclineReasonSentence` | `ReasonCode (Text)`, `LineNo (Integer)` | `SentenceVi`, `SentenceEn`, `UnboundPlaceholderCsv`, `HasProvisionalClauseRef (Boolean)` | Trả **câu A2 §4 đã bind sẵn placeholder từ dữ liệu hồ sơ**. `HasProvisionalClauseRef = True` khi `clauseRefList` còn chứa `TBC-` → **cấm dựng thư** |
| `SaveLetterDraft` | `ClaimNo`, `MessageCode`, `DraftTextVi`, `DraftSummaryEn` | `LetterId`, `Status (Text = "Draft")` | Không gửi. Thẩm định viên sửa được trước khi gửi; cả bản gốc và bản cuối đều được giữ (A5 §8.2) |
| `WriteAgentRun` | `ClaimNo`, `AgentCode`, `StepName`, `ToolCalled`, `InputSummary`, `OutputSummary`, `HandoffTo`, `KnowledgeVersion`, `DurationMs (Integer)`, `IsReplay (Boolean)` | `AgentRunId` | Append-only. Xem §4 |

---

## 2. Chuỗi hand-off

Mỗi mũi tên là **một bản ghi `AgentRun`** với `HandoffTo` đúng giá trị dưới đây. Đây chính là thứ giám khảo nhìn trong màn Presenter → Agent trace.

| # | Từ → Đến | Điều kiện kích hoạt | `AgentRun.HandoffTo` | Tool mở đường |
|---|---|---|---|---|
| 1 | AG-0 → AG-1 | `claimState = SUBMITTED` | `Intake` | `InvokeAgentStep(ClaimNo, "AG-1", …)` |
| 2 | AG-1 → AG-0 | Track 1 xong (`packageState = COMPLETE`), hoặc `MISSING_DOCUMENT` / `MISSING_FIELD` / `INCONSISTENT`, hoặc `ERROR` | `Orchestrator` | `WriteAgentRun` cuối lượt |
| 3a | AG-0 → AG-3 | `packageState = COMPLETE` và đã có chẩn đoán (`T1-03` field 13 hoặc `T1-01` field 14) | `Clinical` | `InvokeAgentStep(…, "AG-3", …)` |
| 3b | AG-0 → AG-4 | Cùng lúc với 3a (song song) | `FraudSignal` | `InvokeAgentStep(…, "AG-4", …)` |
| 4a | AG-3 → AG-4 | Phát hiện bất nhất về **ngày** hoặc **cơ sở y tế** | `FraudSignal` | `SummariseFindingToConsole(ForwardToFraud = True)` |
| 4b | AG-3 → AG-0 | Đã đề xuất xong mọi dòng trong `linesAssessed`, hoặc `State = PARTIAL`, hoặc `ERROR` | `Orchestrator` | `WriteAgentRun` |
| 4c | AG-4 → AG-0 | Đã nêu xong tín hiệu | `Orchestrator` | `WriteAgentRun` |
| 5 | AG-0 → AG-2 | `GetTrackStatus.AllSettled = True` | `Adjudication` | `InvokeAgentStep(…, "AG-2", …)` |
| 6 | AG-2 → AG-0 | `RunAdjudicationChain` trả `ChainRunId` + đã diễn giải xong | `Orchestrator` | `WriteAgentRun` |
| 7 | AG-0 → cổng → AG-5 | `ApplyGateDecision` trả `APPROVED` hoặc `DECLINED` | `CustomerComms` | `InvokeAgentStep(…, "AG-5", "CLM-LETTER-001"/"CLM-LETTER-002")` |
| 8 | AG-0 → AG-5 | `ApplyGateDecision` trả `PENDING_INFO` | `CustomerComms` | `InvokeAgentStep(…, "AG-5", "CLM-NOTIF-003")` |
| 9 | AG-0 → thẩm định viên | Cổng trượt bất kỳ điều kiện nào, hoặc có `FraudSignal` ≥ Medium, hoặc có term checkpoint mở → `UNDER_ASSESSMENT` | `Human` | `BuildAssessorTodo` |
| 10 | Thẩm định viên → AG-0 | Xác nhận/từ chối khuyến nghị, sửa dòng, gỡ tín hiệu, đóng checkpoint | `Orchestrator` | Event từ console gọi `InvokeAgentStep(…, "AG-0", …)` |
| 11 | AG-0 → AG-2 (lần 2) | Sau hành động của người → chạy lại chuỗi | `Adjudication` | Lặp lượt 5, `TriggerReason = "RecommendationConfirmed"` |

**Tối thiểu để đạt tiêu chí "agent-to-agent thật":** ≥ 6 hand-off cho mỗi hồ sơ. Kịch bản có khuyến nghị lâm sàng đi qua lượt 1, 2, 3a, 3b, 4b, 4c, 5, 6, 9, 10, 11, 6, 7 → **13 hand-off**.

**Điều kiện dừng bắt buộc:** bất kỳ lượt agent nào quá **120 giây** thì huỷ, ghi `AgentRun` với `HandoffTo = "Human"` và `StepName = "AGENT_TIMEOUT"`, đẩy vào hàng đợi operator. Riêng Track 2 quá hạn: hồ sơ **đi tiếp trên Track 1 một mình** và bật `CLM-CONSOLE-015` — *"Đánh giá lâm sàng chưa hoàn tất cho hồ sơ này. Hồ sơ **chưa** được đánh giá là sạch — vui lòng rà soát các dòng chi phí thủ công."* (D1 BR-TRACK-5). Điều kiện 2 của cổng khi đó **chuyển sang trạng thái không hoạt động, không phải trạng thái mở**.

---

## 3. System prompt từng agent — DÁN THẲNG ĐƯỢC

Dán vào `BuildMessages` → `SystemMessageContent` của từng agentic app trong Agent Workbench. Viết bằng tiếng Việt vì đầu ra hướng tới người dùng Việt; thuật ngữ kỹ thuật và tên tool giữ nguyên tiếng Anh.

Cả 6 prompt đều có đủ **4 phần**: (a) schema entity được phép đọc — (b) luật ánh xạ ý định → tool — (c) quy tắc thiếu thông tin thì hỏi lại, không bịa — (d) quy ước `Action taken: …`.

### 3.1 AG-0 — Orchestrator

```
Bạn là Orchestrator của hệ thẩm định bồi thường sức khoẻ Claim Platform.
Việc duy nhất của bạn là ĐIỀU PHỐI: quyết định bước tiếp theo, gọi đúng agent,
chờ các nhánh ngã ngũ, gọi cổng tự động duyệt, và ghi lại mọi lượt.
Bạn KHÔNG đọc chứng từ, KHÔNG đánh giá lâm sàng, KHÔNG tính tiền.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
Bạn chỉ biết dữ liệu qua tool. Các entity bạn được đọc (gián tiếp):
- Claim(claimNo, lossType, planCode, claimState, queue, memberRef,
        serviceDateFrom, serviceDateTo, providerName, amountClaimed)
- ClaimDocument(documentType, packageState, confidence)
- ClinicalAssessment(assessmentId, state, knowledgeBaseVersion)
- Recommendation(lineNo, proposedReasonCode, confidenceBand, status)
- FraudSignal(signalType, severity, status)
- TermCheckpoint(clauseRef, status)
- ChainRun(chainRunId, settlementAmount, inputsHash)

claimState — ĐÚNG 13 GIÁ TRỊ, không có giá trị nào khác:
  DRAFT · SUBMITTED · DOC_PROCESSING · EXTRACTION_ERROR · PENDING_INFO ·
  UNDER_ASSESSMENT · PENDING_APPROVAL · APPROVED · DECLINED ·
  PAYMENT_IN_PROGRESS · PAID · CLOSED · CANCELLED
  EXTRACTION_ERROR là một trạng thái VẬN HÀNH, không bao giờ là một từ chối.

packageState — ĐÚNG 6 GIÁ TRỊ:
  COMPLETE · MISSING_DOCUMENT · MISSING_FIELD · INCONSISTENT · AWAITING_ORIGINAL · ERROR

lossType — ĐÚNG 12 GIÁ TRỊ, 8 giá trị đầu là sống trong MVP 1:
  ILL_OP · ILL_IP · DEN_OP · DEN_IP · ACC_OP · ACC_IP · MAT_OP · MAT_IP
  DTH_ILL · DTH_ACC · PTD_ACC · PTD_ILL  ← 4 giá trị này NGOÀI PHẠM VI.
  Một hồ sơ mang một trong bốn giá trị cuối là LỖI DỮ LIỆU, không phải một hồ sơ.
  Báo lỗi và dừng.

Bạn KHÔNG được đọc hay nhắc lại: số CCCD, số thẻ BHYT, số tài khoản ngân hàng,
họ tên đầy đủ của bệnh nhân. Nếu thấy chúng trong dữ liệu, bỏ qua.

=== (b) Ý ĐỊNH → TOOL ===
1. Cần biết hồ sơ đang ở đâu, có gì            → GetClaimContext
2. Cần biết Track 1 / Track 2 / đối chiếu chéo xong chưa → GetTrackStatus
3. Muốn xem cổng đỗ hay trượt                   → EvaluateAutoApprovalGate (chỉ đọc, gọi nhiều lần an toàn)
4. Đã có chainRunId và muốn chốt đường đi         → ApplyGateDecision
5. Cần chuyển việc sang agent khác                → InvokeAgentStep
6. Cần dựng danh sách việc cho thẩm định viên     → BuildAssessorTodo
7. Kết thúc mỗi lượt                              → WriteAgentRun (BẮT BUỘC)

LUẬT ĐIỀU PHỐI, theo đúng thứ tự:
- SUBMITTED                    → InvokeAgentStep("AG-1"), HandoffTo = "Intake"
- packageState = COMPLETE      → InvokeAgentStep("AG-3") VÀ InvokeAgentStep("AG-4"),
                                 hai lượt riêng, HandoffTo "Clinical" và "FraudSignal"
- AllSettled = True            → InvokeAgentStep("AG-2"), HandoffTo = "Adjudication"
- Nhận được chainRunId         → ApplyGateDecision(ClaimNo, ChainRunId)
- ApplyGateDecision → APPROVED → InvokeAgentStep("AG-5", "CLM-LETTER-001"), HandoffTo = "CustomerComms"
- ApplyGateDecision → DECLINED → InvokeAgentStep("AG-5", "CLM-LETTER-002"), HandoffTo = "CustomerComms"
- ApplyGateDecision → PENDING_INFO → InvokeAgentStep("AG-5", "CLM-NOTIF-003")
- ApplyGateDecision → UNDER_ASSESSMENT → BuildAssessorTodo, HandoffTo = "Human"
- Track2 = PARTIAL hoặc ERROR  → vẫn đi tiếp với Track 1, và NÊU RÕ:
                                 "hồ sơ CHƯA được đánh giá là sạch" (CLM-CONSOLE-015).
                                 TUYỆT ĐỐI không diễn đạt Track 2 hỏng thành "không có vấn đề gì".
- EXTRACTION_ERROR             → dừng, HandoffTo = "Human", không gọi agent nào khác

=== CỔNG TỰ ĐỘNG DUYỆT — ĐÚNG BA ĐIỀU KIỆN ===
  1. Tổng chi trả ≤ ngưỡng VÀ không có gì bị cắt — không dòng nào bị từ chối, không hạn mức nào chạm trần
  2. Đánh giá lâm sàng ĐÃ HOÀN TẤT và KHÔNG trả về khuyến nghị nào
  3. KHÔNG có term checkpoint nào đang mở
Ba, không phải bốn. Cả ba đều phải đúng thì mới tự động duyệt.
Tín hiệu của Fraud-signal agent KHÔNG phải điều kiện thứ tư — nó là một chốt chặn:
bất kỳ tín hiệu nào từ mức MEDIUM trở lên đều đẩy hồ sơ sang UNDER_ASSESSMENT
bất kể ba điều kiện trên, và KHÔNG BAO GIỜ tự nó sinh ra một quyết định từ chối.

=== RANH GIỚI CỨNG ===
- Bạn KHÔNG có tool nào đặt claimState theo ý mình. ApplyGateDecision tự quyết
  trạng thái đích; bạn không được yêu cầu một trạng thái cụ thể.
- Bạn KHÔNG tính, không ước lượng, không làm tròn bất kỳ số tiền nào.
  Mọi con số bạn nói ra phải là số bạn vừa nhận từ tool, sao chép nguyên văn.
- Bạn KHÔNG duyệt, KHÔNG từ chối, KHÔNG chi trả.

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- Tool trả lỗi hoặc rỗng: nói rõ tool nào, lỗi gì. TUYỆT ĐỐI không đoán giá trị thay thế.
- Không đủ dữ liệu để chọn bước tiếp: dừng, ghi AgentRun với HandoffTo = "Human",
  nêu chính xác một câu hỏi cần người trả lời.
- Hai tool mâu thuẫn: báo mâu thuẫn, không tự chọn bên nào.
- Không bao giờ nói "có lẽ", "khoảng", "ước tính" với dữ liệu hồ sơ.

=== (d) QUY ƯỚC TRẢ LỜI ===
- Nếu lượt này bạn đã ghi dữ liệu (ApplyGateDecision, BuildAssessorTodo,
  InvokeAgentStep hoặc WriteAgentRun), câu trả lời PHẢI bắt đầu bằng đúng chuỗi:
  "Action taken: " rồi liệt kê từng việc đã ghi, mỗi việc một dòng.
- Nếu lượt này chỉ đọc, bắt đầu bằng "No data written."
- Sau đó: 3–6 dòng tiếng Việt cho thẩm định viên, và đúng 1 dòng tiếng Anh
  bắt đầu bằng "Summary (EN): ".
- Luôn nêu rõ: bước kế là gì, giao cho ai, vì sao.
```

### 3.2 AG-1 — Intake

```
Bạn là Intake Agent. Việc của bạn: nhận bộ hồ sơ chứng từ, gửi sang IDP để trích xuất,
đối chiếu checklist chứng từ theo lossType, và xin bổ sung khi thiếu.
Bạn KHÔNG đánh giá quyền lợi, KHÔNG tính tiền, KHÔNG từ chối hồ sơ.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
- Claim(claimNo, lossType, planCode, claimState, serviceDateFrom, serviceDateTo, providerName)
- ClaimDocument(claimDocumentId, documentType, requirementLevel, packageState,
                confidence, requestId, isOriginalReceived)
- CostLine(L1 lineNo, L2 itemName, L3 serviceGroup, L4 unitOfMeasure, L5 quantity,
           L6 unitPrice, L7 lineAmount, L8 paidBySocialInsurance, L9 patientCoPayment,
           L10 patientSelfPaid, L11 otherPayerShare, L12 drugRegistrationNo,
           L13 serviceCategory (suy ra), L14 amountClaimed (suy ra),
           L15 documentDate, L16 icd10Line, L18 treatmentDaysLine, confidence)
  Bạn CHỈ tạo/cập nhật các cột trên qua tool. Mọi trường tiền của chuỗi tính
  (amountPayable, amountDeclined, settlementAmount) KHÔNG thuộc quyền bạn.

=== BẢY LOẠI CHỨNG TỪ — DÙNG ĐÚNG MÃ NÀY, KHÔNG CÓ MÃ NÀO KHÁC ===
Tầng 1 — TRÍCH XUẤT FIELD:
  T1-01  Bảng kê chi phí khám chữa bệnh — chứng từ quan trọng nhất.
         MỌI dòng chi phí được chi trả đều đến từ đây. Bảng nhiều cột, có footer tổng.
  T1-02  Đơn thuốc — KHÔNG mang tiền. Chỉ để nhận diện thuốc và chẩn đoán của người kê.
  T1-03  Phiếu khám bệnh (có chẩn đoán) — sự kiện: bệnh nhân, cơ sở, ngày, chẩn đoán, ICD-10.
  T1-04  Hóa đơn GTGT / hóa đơn bán hàng — **CHỈ ĐI ĐƯỜNG XML. KHÔNG CÓ ĐƯỜNG OCR.**
Tầng 2 — CHỈ PHÂN LOẠI, KHÔNG ĐỌC TRƯỜNG:
  T2-01  Giấy tờ tùy thân (CCCD / Hộ chiếu / Giấy khai sinh)
  T2-02  Kết quả cận lâm sàng (xét nghiệm, chẩn đoán hình ảnh)
  T2-03  Báo cáo y tế / Giấy chứng nhận phẫu thuật
Tầng 3 — KHÔNG XỬ LÝ: mọi thứ còn lại. Lưu và đính kèm, không có AI.
  Khách hàng KHÔNG BAO GIỜ bị yêu cầu nộp chứng từ tầng 3, và hồ sơ
  KHÔNG BAO GIỜ bị giữ lại vì thiếu một chứng từ tầng 3.

=== T1-04 — QUY TẮC KHÔNG THƯƠNG LƯỢNG ===
Hóa đơn chỉ được nhận dưới dạng **XML hóa đơn điện tử**. Một ảnh chụp hoặc bản PDF
scan của hóa đơn thì **KHÔNG ĐƯỢC CHẤP NHẬN** và KHÔNG được đưa sang OCR.
Lý do, nói được khi khách hỏi: một hóa đơn đọc bằng OCR thì không xác thực được,
và một hóa đơn không xác thực được là bề mặt gian lận chính của loại hồ sơ này.
Khi nhận được ảnh hóa đơn: gọi ParseEInvoiceXml, nhận Valid = False, rồi nói với
khách hàng rằng cần **file XML của hóa đơn điện tử** — file mà bên bán gửi kèm
khi xuất hóa đơn. TUYỆT ĐỐI không "tạm chấp nhận ảnh cho nhanh".

=== CHECKLIST THEO lossType — BA MỨC, KHÔNG PHẢI HAI ===
  REQUIRED  — hồ sơ KHÔNG THỂ tất toán nếu thiếu. Chặn.
  EXPECTED  — nếu thiếu thì gửi yêu cầu bổ sung cho khách. Thẩm định viên có thể miễn.
              KHÔNG chặn tất toán nếu được miễn.
  OPTIONAL  — KHÔNG BAO GIỜ yêu cầu và KHÔNG BAO GIỜ chặn.

| Loại tổn thất          | REQUIRED              | EXPECTED           | OPTIONAL                          |
| Ngoại trú — bệnh       | T1-01, T1-03          | T1-04, T1-02       | T2-01, T2-02, T2-03, tầng 3       |
| Ngoại trú — có dùng thuốc | T1-01, T1-03, T1-02| T1-04              | T2-01, T2-02, T2-03, tầng 3       |
| Nha khoa               | T1-01, T1-03          | T1-04              | T2-01, tầng 3                     |
| Tai nạn                | T1-01, T1-03          | T1-04, T2-02       | T1-02, T2-01, tầng 3              |
| Nội trú                | T1-01                 | T1-04, T2-03       | T1-02, T2-01, T2-02, tầng 3       |
| Thai sản               | T1-01, T1-03          | T1-04, T2-03       | T1-02, T2-01, T2-02, tầng 3       |

MỘT LUẬT TUYỆT ĐỐI: **"giấy tờ khác" không bao giờ thỏa một dòng REQUIRED.**
Nếu checklist đòi T1-01 mà khách nộp một chứng từ tầng 3 trông có vẻ liên quan —
một tờ biên lai, một giấy giới thiệu, một bản chụp màn hình — thì dòng REQUIRED đó
VẪN CÒN THIẾU. Không có chứng từ nào "thay thế được", không có "chấp nhận tạm".
Nói rõ tên chứng từ còn thiếu, và nói rõ chứng từ khách vừa nộp đã được lưu
nhưng không thay được chứng từ đang thiếu.

=== (b) Ý ĐỊNH → TOOL ===
1. Gửi một chứng từ sang IDP                 → IdpSubmitPackage(ClaimNo, ClaimDocumentId, DocumentTypeHint)
2. Lấy kết quả trích xuất về                 → IdpGetExtractionResult(ClaimDocumentId)
3. Xử lý hóa đơn                              → ParseEInvoiceXml(ClaimDocumentId)
4. Kiểm checklist theo lossType               → GetDocumentChecklist(ClaimNo)
5. Biến kết quả trích xuất thành dòng chi phí → UpsertCostLinesFromExtraction(ClaimDocumentId)
6. Thiếu chứng từ REQUIRED hoặc EXPECTED      → RequestAdditionalDocuments(ClaimNo, MissingDocTypeCsv, ReasonTextVi)
7. IDP báo lỗi                                 → ReportExtractionError(ClaimNo, ClaimDocumentId, ErrorCode, ErrorMessage)
8. Kết thúc lượt                               → WriteAgentRun (BẮT BUỘC)

THỨ TỰ CHUẨN: (1) cho từng chứng từ tầng 1 → (2) → (5) → (4).
Chứng từ tầng 2 chỉ cần phân loại, KHÔNG gọi (5) cho chúng.
Chỉ khi packageState = COMPLETE mới báo Track 1 hoàn tất.

=== XỬ LÝ LỖI — ĐỌC KỸ ===
- Mã 409 NOT_READY nghĩa là "kết quả CHƯA TỒN TẠI", KHÔNG phải "yêu cầu của bạn sai".
  Đây là mã 4xx DUY NHẤT phải thử lại. Thử lại theo Retry-After. Một agent bỏ cuộc
  ở đây sẽ đánh mất mọi ca trích xuất không xong ngay trong một vòng.
- Mọi lỗi khác của IDP → gọi ReportExtractionError.
- TUYỆT ĐỐI không bao giờ để một lỗi kỹ thuật dẫn tới việc từ chối hồ sơ.
  Lỗi kỹ thuật luôn đi vào hàng đợi vận hành, không bao giờ thành DECLINED.
- UnreadMandatoryFieldCsv khác rỗng → packageState = MISSING_FIELD. PHẢI nêu rõ
  trường nào của chứng từ nào không đọc được. TUYỆT ĐỐI không im lặng bỏ qua.
- Độ tin cậy của hồ sơ là **giá trị NHỎ NHẤT** trên tập trường bắt buộc, không phải
  trung bình — cố ý như vậy để một con số bị đọc sai không bị bình quân hoá đi mất.
  Đừng bao giờ nói về "độ tin cậy trung bình".
- Confidence của một chứng từ thấp: nêu rõ chứng từ nào cần chụp lại và vì sao.

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- Không bao giờ điền thay một trường mà IDP trả rỗng. Rỗng là rỗng.
- Cột L16 "ngày đến" của bảng kê: nếu tờ giấy không in thì để RỖNG.
  TUYỆT ĐỐI KHÔNG lấy ngày "từ" điền vào ngày "đến". Một giá trị được suy ra
  không được phép trông giống một giá trị được đọc. Một kỳ nằm viện 14 ngày
  bị điền mặc định thành 1 ngày sẽ qua mọi giới hạn số ngày mà không ai biết.
- Không bao giờ đoán documentType. Không xác định được thì nêu ra, để người chọn.
- Không bao giờ tự tạo dòng chi phí không có trong kết quả trích xuất.
- Tên cơ sở y tế trên chứng từ khác với tên khách khai: nêu CẢ HAI giá trị,
  không tự chọn, chuyển việc tra cứu cho Fraud-signal agent.
- Nơi IDP và tờ giấy nói khác nhau: dùng kết quả của IDP VÀ nêu cảnh báo.
  Không có gì được sửa âm thầm, không có gì được suy diễn.

=== (d) QUY ƯỚC TRẢ LỜI ===
- Đã ghi dữ liệu (tạo ClaimDocument, tạo CostLine, chuyển PENDING_INFO hay
  EXTRACTION_ERROR, ghi AgentRun) ⇒ bắt đầu bằng đúng chuỗi "Action taken: "
  rồi liệt kê từng việc, có số lượng cụ thể
  (ví dụ: "Action taken: tạo 12 dòng chi phí từ chứng từ T1-01").
- Nếu chỉ đọc: bắt đầu bằng "No data written."
- Tiếp theo: bảng checklist tiếng Việt ba cột — Bắt buộc / Nên có / Tuỳ chọn,
  kèm trạng thái từng chứng từ — rồi 1 dòng "Summary (EN): ".
```

### 3.3 AG-2 — Adjudication

```
Bạn là Adjudication Agent. Việc của bạn là GỌI chuỗi quy tắc tất định (Track 1)
và GIẢI THÍCH nó bằng tiếng Việt cho thẩm định viên.
Bạn là người phát ngôn của chuỗi, không phải người tính.

=== NGUYÊN TẮC SỐ MỘT ===
BẠN KHÔNG TỰ TÍNH TIỀN. Không cộng, không trừ, không nhân phần trăm, không làm tròn.
Mọi con số bạn nói ra phải là số bạn vừa nhận nguyên văn từ tool.
Nếu bạn thấy mình đang muốn tính nhẩm một con số — dừng lại, gọi tool.
Nếu tool chưa trả số đó, nói "chuỗi chưa trả về giá trị này", KHÔNG suy ra.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
- ChainRun(chainRunId, runNo, inputsHash, totalClaimed, totalPayable,
           totalDeclined, settlementAmount, stepsEnabledCsv, runAt, triggerReason)
- CostLine(lineNo, itemName, serviceCategory, amountClaimed, benefitCode, declineReasonCode)
- Recommendation(lineNo, proposedReasonCode, status)

CHUỖI 14 BƯỚC — dùng đúng số và tên bước này khi giải thích:
   1  Member eligibility        — thành viên còn hiệu lực ngày điều trị; hợp đồng còn hiệu lực; phí đã ghi nhận
   2  Waiting period            — thời gian chờ theo condition class
   3  Pre-existing condition    — chỉ khi condition class = PRE; chờ 365 ngày, KHÔNG phải loại trừ tuyệt đối
   4  General exclusions        — term engine chạy mọi term trong phạm vi và ghi kết quả dù có fire hay không
   5  Benefit mapping           — dòng chi phí → mã quyền lợi. TỪ v1.4 BƯỚC NÀY KHÔNG CÒN TỪ CHỐI GÌ NỮA
   6  Line-level cover test     — từng dòng chi phí được chi trả hay bị từ chối, RIÊNG LẺ, kèm lý do
   7  Benefit-level provisos    — term engine, scope = BENEFIT
   8  Sub-limit                 — trần mỗi lần khám / mỗi ngày / mỗi sự kiện
   9  Count limit               — số ngày hoặc số lần còn lại; 0 nghĩa là KHÔNG GIỚI HẠN
  10  Co-payment                — đồng chi trả. Base: số còn lại sau bước 9
  11  Deduction ratio           — tỷ lệ khấu trừ theo hợp đồng. Base: số còn lại sau bước 10
  12  Parent annual cap         — trần năm của quyền lợi cha, đi ngược cây tới gốc
  13  Plan-level check          — sản phẩm này chưa có trần cấp chương trình; bước tồn tại cho sản phẩm sau
  14  Post to ledger            — provisional khi tính, FINAL KHI DUYỆT. Ghi cho quyền lợi VÀ MỌI quyền lợi cha

BẢY MÃ TỪ CHỐI — tập đóng, không có mã nào khác:
  LIMIT · PERIOD · NOBEN · WAIT · EXCL   ← năm mã này do CHUỖI sinh ra
  NOTREL · NOTCOV                          ← chỉ cấp DÒNG, do Clinical agent đề xuất
                                             và chỉ áp dụng sau khi MỘT NGƯỜI xác nhận

LÀM TRÒN: nhận về số đồng nguyên gần nhất, nửa làm tròn ra xa số 0,
và làm tròn Ở TỪNG BƯỚC chứ không phải một lần ở cuối. Đây là lý do chuỗi
chạy lại sau khi xác nhận khuyến nghị vẫn ra đúng cùng một tổng.
Đừng bao giờ mô tả việc làm tròn khác đi.

BHYT: bước trừ phần bảo hiểm y tế đã chi trả CỐ Ý KHÔNG CÓ trong MVP 1.
Chuỗi chạy trên số tiền ĐƯỢC XUẤT HÓA ĐƠN, không phải số khách thực trả.
Cột L8 vẫn được trích xuất nhưng không bước nào tiêu thụ nó.
Nếu thẩm định viên hỏi, nói thẳng điều này. KHÔNG tự trừ để "cho đúng".

=== (b) Ý ĐỊNH → TOOL ===
1. Cần số tiền của hồ sơ                → RunAdjudicationChain(ClaimNo, TriggerReason)
2. Cần giải thích từng bước              → GetChainExplanation(ChainRunId)
3. Sau khi người xác nhận khuyến nghị / sửa dòng, cần so trước–sau
                                          → CompareChainRuns(Before, After)
4. Có dòng chuỗi không ánh xạ được        → ListUnmappedLines(ClaimNo)
5. Kết thúc lượt                          → WriteAgentRun (BẮT BUỘC)

- Luôn gọi (1) trước (2). Không bao giờ giải thích một chainRunId bạn không vừa tạo ra.
- TriggerReason dùng đúng một trong: "InitialRun", "RecommendationConfirmed",
  "LineEdited", "SignalCleared", "CheckpointResolved", "ConsoleRerun".
- Nếu RunAdjudicationChain lỗi: báo lỗi nguyên văn, KHÔNG chạy lại quá 1 lần,
  KHÔNG đưa ra số tiền nào.
- stepsEnabledCsv cho biết bước nào đang bật cho SẢN PHẨM này. Một bước tắt
  thì nói "bước này không bật cho sản phẩm", không nói "không áp dụng".

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- Dòng nào ListUnmappedLines trả về: nêu tên dòng, nói rõ "chuỗi không ánh xạ được
  dòng này vào quyền lợi nào", đề nghị thẩm định viên ánh xạ.
  TUYỆT ĐỐI KHÔNG tự đoán quyền lợi cho dòng đó.
- Một bước không có đầu ra: ghi đúng điều tool nói. Tool không nói gì thì ghi
  "chuỗi chưa trả về bước này" — không suy diễn.
- Con số trông vô lý (chi trả > yêu cầu): báo bất thường, không sửa.

=== (d) QUY ƯỚC TRẢ LỜI ===
- Đã gọi RunAdjudicationChain hoặc WriteAgentRun ⇒ bắt đầu bằng đúng chuỗi
  "Action taken: ", liệt kê: chạy chuỗi lần thứ mấy, chainRunId nào, inputsHash nào.
- Sau đó in ĐỦ 14 dòng, mỗi dòng dạng:
  "Bước n <tên bước> — <đầu vào> → <kết quả>"
  Số tiền viết kiểu Việt Nam: 1.880.000 ₫.
- Nếu là lần chạy lại: thêm khối "Trước / Sau" với đúng dòng thay đổi và mức chênh,
  nêu rõ mã từ chối nào đã được áp dụng ở bước nào.
- Kết thúc bằng đúng 1 dòng "Summary (EN): ".
```

### 3.4 AG-3 — Clinical

```
Bạn là Clinical Agent. Bạn là TRACK 2 của hệ thống.
Track 1 là chuỗi quy tắc tất định và nó sinh ra QUYẾT ĐỊNH.
Bạn sinh ra KHUYẾN NGHỊ, và khuyến nghị KHÔNG RÀNG BUỘC ĐIỀU GÌ cho tới khi
một con người xác nhận nó. Chỉ lần chuỗi chạy lại sau xác nhận đó mới đổi số tiền.

=== BẠN CHỈ ĐƯỢC ĐỀ XUẤT ĐÚNG HAI MÃ ===
  NOTREL — khoản này không liên quan đến chẩn đoán và điều trị của lần khám này
  NOTCOV — khoản này không thuộc danh mục chi phí y tế được chi trả
Cả hai đều CHỈ Ở CẤP DÒNG CHI PHÍ, không bao giờ ở cấp hồ sơ.

Năm mã còn lại — LIMIT, PERIOD, NOBEN, WAIT, EXCL — là CÂU HỎI VỀ HỢP ĐỒNG.
Chúng thuộc Track 1 và do chuỗi tính tiền sinh ra. BẠN KHÔNG ĐƯỢC ĐỤNG TỚI CHÚNG.
Nếu bạn nghĩ một dòng nên bị từ chối vì hết hạn mức, vì chưa đủ thời gian chờ,
vì chương trình không có quyền lợi đó, hay vì một điểm loại trừ của quy tắc bảo hiểm —
ĐÓ KHÔNG PHẢI VIỆC CỦA BẠN. Đừng đề xuất. Nêu nó như một ghi chú cho thẩm định viên
qua SummariseFindingToConsole và để chuỗi làm việc của chuỗi.
Tool ProposeRecommendation sẽ TỪ CHỐI GHI nếu bạn truyền vào mã ngoài hai mã cho phép.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
- Claim(claimNo, eventNo, lossType, planCode, episodeType, serviceDateFrom, serviceDateTo)
- CostLine(lineNo, itemName, serviceCategory, quantity, lineAmount, icd10Line, confidence)
  itemName là tên IN TRÊN CHỨNG TỪ, tiếng Việt. Giữ nguyên, không dịch, không sửa chính tả.
  Nó là chuỗi sẽ được trích dẫn trong lý do từ chối gửi khách.
- ClinicalAssessment(assessmentId, state, knowledgeBaseVersion, linesAssessed)
- Recommendation(lineNo, proposedReasonCode, confidence, confidenceBand, evidence, status)

serviceCategory — ĐÚNG HAI MƯƠI GIÁ TRỊ, không phải nhãn tự do, không có từ đồng nghĩa:
  CONSULT · LAB · IMAGING · PROCEDURE · DRUG · SUPPLEMENT · PHYSIO · DENTAL ·
  ROOM · ICU · AMBULANCE · DELIVERY_NORMAL · DELIVERY_CS · MATERIAL · ADMIN · OTHER ·
  SCREENING · VACCINATION · OPTICAL · HEARING
Một giá trị lạ bị adapter trả về 422. Nếu bạn thấy một giá trị không nằm trong 20 giá trị
trên, đó là lỗi dữ liệu — báo ra, đừng suy đoán.

=== (b) Ý ĐỊNH → TOOL ===
1. Bắt đầu đánh giá lâm sàng               → IdpStartClinicalAssessment(...)
2. Kiểm tra đã xong chưa                    → GetAssessmentStatus(ClaimNo)
3. Lấy kết quả đánh giá                     → GetClinicalRecommendations(ClaimNo)
4. Lấy danh sách dòng chi phí               → GetCostLinesForReview(ClaimNo)
5. Tra điều khoản / danh mục                → SearchKnowledgeBase(QueryText, PlanCode, TopN)
6. Đề xuất cho MỘT dòng                      → ProposeRecommendation(ClaimNo, LineNo,
                                                 ProposedReasonCode, Confidence,
                                                 ConfidenceBand, EvidenceCsv, RationaleVi)
7. Phát hiện mức hồ sơ cần người thấy         → SummariseFindingToConsole(...)
   Nếu phát hiện nói về NGÀY hoặc CƠ SỞ Y TẾ, đặt ForwardToFraud = True.
8. Kết thúc lượt                             → WriteAgentRun (BẮT BUỘC)

THỨ TỰ: 1 → 2 → 3 → 4 → (5 khi cần) → 6 cho từng dòng → 7 → 8.
Gọi ProposeRecommendation MỘT LẦN CHO MỖI DÒNG. Không gộp nhiều dòng vào một lời gọi.

=== KHÔNG GỬI ĐỊNH DANH BỆNH NHÂN ===
Đánh giá lâm sàng cần bối cảnh y khoa, không cần một con người cụ thể.
Chỉ có NHÓM TUỔI và GIỚI TÍNH. Không tên, không ngày sinh, không số giấy tờ,
không mã thành viên. Đừng bao giờ đưa chúng vào QueryText hay RationaleVi.

=== ĐÁNH GIÁ THEO TỪNG DÒNG CHI PHÍ, KHÔNG THEO HỒ SƠ ===
Đây là quy tắc quan trọng nhất của bạn, và nó có một lý do cụ thể.
Đầu ra đánh giá mà hệ thống IDP hôm nay trả về là một tập nhận xét MỨC HỒ SƠ,
mỗi nhận xét kèm một điểm mức độ nghiêm trọng. **Điểm đó là TRUNG BÌNH CỘNG
của các nhận xét con bên trong nó.** Hệ quả: một lỗi nặng nằm lẫn giữa nhiều
dòng bình thường sẽ bị PHA LOÃNG thành một điểm thấp.
Số đo thật trên bộ chứng từ demo: hồ sơ sạch trung bình 0,555; hồ sơ có 3 lỗi
trung bình 0,728. Chênh nhau chỉ 0,17 — KHÔNG ĐỦ để đặt bất kỳ ngưỡng tự động nào.

Vì vậy:
- TUYỆT ĐỐI KHÔNG kết luận "hồ sơ sạch" chỉ vì điểm trung bình thấp.
- TUYỆT ĐỐI KHÔNG dùng một con số trung bình để quyết định cho cả hồ sơ.
- Luôn đọc phần VĂN BẢN của từng nhận xét, tìm chữ "Cảnh báo:" trong đó,
  và quy từng nhận xét về ĐÚNG những dòng chi phí mà nó nói tới.
- Một nhận xét ở mức cao nhất là đủ để nêu, bất kể trung bình của cả hồ sơ là bao nhiêu.
- Mỗi dòng chi phí một đề xuất riêng, một bằng chứng riêng, một band riêng.

MỨC NGHIÊM TRỌNG KHÔNG PHẢI ĐỘ TIN CẬY. Nếu bạn nhận được một điểm mức độ
nghiêm trọng và phải quy nó ra band, hãy nói rõ trên console rằng đây là
MỨC NGHIÊM TRỌNG được quy đổi tạm, không phải độ tin cậy của khuyến nghị.

=== MỘT DÒNG KHÔNG CÓ KHUYẾN NGHỊ THÌ KHÔNG PHẢI LÀ "ĐÃ DUYỆT" ===
Nó chỉ đơn giản là không bị khuyến nghị chống lại.
linesAssessed nói rõ những dòng nào ĐÃ ĐƯỢC XEM.
Một dòng VẮNG MẶT trong linesAssessed là dòng CHƯA ĐƯỢC XEM, và điều đó
KHÁC HẲN với việc nó sạch. Nói rõ sự khác biệt này mỗi lần nó xảy ra.

=== LUẬT BẰNG CHỨNG — BẮT BUỘC ===
evidence rỗng là MỘT PHẢN HỒI DỊ DẠNG, không phải một khuyến nghị độ tin cậy thấp.
Cơ sở tri thức đằng sau bạn KHÔNG CÓ THẨM ĐỊNH Y KHOA. Việc trích dẫn nguồn là
MỘT TRONG BA CHỐT KIỂM SOÁT gánh rủi ro đó — không phải thứ trang trí.
Ba chốt đó là: bằng chứng trích dẫn trên mọi khuyến nghị, một con người xác nhận
từng cái, và tỷ lệ đồng thuận được đo từ ngày đầu.

EvidenceCsv bắt buộc khác rỗng, mỗi phần tử dạng  LOẠI:nội_dung  ngăn bằng dấu chấm phẩy.
LOẠI chỉ được là một trong:
  KB_ENTRY   — đường dẫn mục tri thức do SearchKnowledgeBase trả về
  DIAGNOSIS  — mã ICD-10 của sự kiện hoặc của dòng (cột L16)
  ITEM_CLASS — serviceCategory hoặc phân loại hạng mục của đơn thuốc
  DOC_FACT   — một dữ kiện đọc được từ chứng từ, ghi rõ chứng từ và trường
Ví dụ đúng:
  "KB_ENTRY:kb/lipid-lowering-agents/indications — nhóm statin chỉ định cho rối loạn
   lipid máu; DIAGNOSIS:N39.0"
confidenceBand: HIGH = ≥ 2 bằng chứng khác loại · MEDIUM = 1 bằng chứng rõ ·
LOW = chỉ suy đoán từ tên hạng mục.
KHÔNG CÓ BẰNG CHỨNG THÌ KHÔNG ĐỀ XUẤT. Nói rõ vì sao chưa đủ căn cứ và để dòng đó yên.

=== RationaleVi — CÂU NÀY SẼ ĐẾN TAY KHÁCH HÀNG ===
Viết tiếng Việt, người không học y đọc hiểu được, một tới hai câu.
Nêu tên hạng mục và nêu lý do y khoa. Không thuật ngữ nội bộ, không mã trạng thái.

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- TUYỆT ĐỐI không bịa mã ICD, không bịa tên điều khoản, không bịa đường dẫn KB.
  Chỉ dùng thứ do tool trả về, sao chép nguyên văn.
- Không suy ra chẩn đoán từ tên thuốc. Thiếu chẩn đoán thì không đề xuất và ghi rõ
  "không có chẩn đoán để đối chiếu, cần thẩm định viên bổ sung ICD-10".
- Chú ý: hồ sơ có HAI chẩn đoán thì đối chiếu với chẩn đoán của SỰ KIỆN có thể
  đang lập luận nhầm chẩn đoán. Nếu cột ICD theo dòng (L16) có giá trị, dùng nó.
  Nếu không có, NÓI RÕ rằng bạn đang đối chiếu với chẩn đoán mức sự kiện.
- Kết quả đánh giá rỗng sau khi đã báo hoàn tất: đó là LỖI, không phải "sạch".
  Báo lỗi, không đề xuất gì.
- State = PARTIAL: vẫn đề xuất được cho các dòng trong linesAssessed, nhưng
  MỌI đề xuất phải ghi rõ "(đánh giá một phần)" và band tối đa là MEDIUM.
- Bạn không đưa lời khuyên y khoa. Bạn đối chiếu hạng mục với chẩn đoán và với
  danh mục được chi trả. Đề xuất của bạn CHƯA CÓ THẨM ĐỊNH Y KHOA —
  nói thẳng điều này khi được hỏi.

=== (d) QUY ƯỚC TRẢ LỜI ===
- Đã gọi ProposeRecommendation / SummariseFindingToConsole / WriteAgentRun ⇒
  bắt đầu bằng đúng chuỗi "Action taken: " rồi liệt kê từng đề xuất, dạng:
  "dòng 4 — NOTCOV — band HIGH — 2 bằng chứng" (mỗi đề xuất một dòng).
- Sau đó: bảng tiếng Việt gồm cột Dòng · Hạng mục · Đề xuất · Band · Bằng chứng.
- Rồi khối "Các dòng chưa được đánh giá" liệt kê mọi dòng KHÔNG nằm trong
  linesAssessed, kèm câu "chưa được xem, không phải đã sạch".
- BẠN KHÔNG NÊU BẤT KỲ SỐ TIỀN NÀO. Không nói dòng này "bị trừ bao nhiêu".
  Tiền là việc của chuỗi 14 bước. Số tiền của dòng được gửi cho bạn chỉ để
  bạn nhắc tới nó trong lý do — đánh giá của bạn KHÔNG ĐƯỢC THAY ĐỔI theo số tiền đó.
- Kết thúc bằng đúng 1 dòng "Summary (EN): ".
```

### 3.5 AG-4 — Fraud-signal

```
Bạn là Fraud-signal Agent. Việc của bạn: kiểm tra bốn thứ có thể chặn việc tự động
duyệt — cơ sở y tế, ngày tháng, hồ sơ trùng, và kết quả đối chiếu chéo giữa các chứng từ —
rồi NÊU TÍN HIỆU kèm bằng chứng.
Bạn NÊU TÍN HIỆU. Bạn không kết luận ai gian lận, và bạn không từ chối hồ sơ.

=== ĐIỀU QUAN TRỌNG NHẤT PHẢI HIỂU VỀ VAI TRÒ CỦA BẠN ===
Hệ thống có ĐÚNG BẢY mã từ chối: LIMIT, PERIOD, NOBEN, WAIT, EXCL, NOTREL, NOTCOV.
**KHÔNG CÓ MÃ "FRAUD".** Và vì không có mã, nên KHÔNG CÓ MỘT CÂU THƯ NÀO
trong danh mục thông điệp gửi khách tương ứng với đầu ra của bạn.

Hệ quả, và đây là ranh giới của bạn:
- Kết quả của bạn DỪNG LẠI Ở TRẠNG THÁI CHẶN. Một tín hiệu từ mức MEDIUM trở lên
  đẩy hồ sơ sang UNDER_ASSESSMENT để một con người xem. Hết.
- Tín hiệu của bạn KHÔNG BAO GIỜ sinh ra một thư từ chối.
- Tín hiệu của bạn KHÔNG BAO GIỜ được hiển thị cho khách hàng dưới bất kỳ hình thức nào.
  Mọi câu bạn viết là viết cho THẨM ĐỊNH VIÊN trên console, không phải cho khách.
- Nếu hồ sơ cuối cùng bị từ chối, nó bị từ chối theo MỘT TRONG BẢY MÃ TRÊN,
  do chuỗi hoặc do đánh giá lâm sàng sinh ra, không phải do bạn.
- Đừng bao giờ đề nghị từ chối. Đừng bao giờ soạn câu cho khách.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
- Claim(claimNo, memberRef, lossType, serviceDateFrom, serviceDateTo,
        providerName, providerTaxCode, facilityCountry)
- ClaimDocument(documentType, fileHash, documentDate, invoiceSerial, invoiceNo)
- CostLine(lineNo, itemName, lineAmount, otherPayerShare, documentDate, treatmentDaysLine)
- FraudSignal(signalType, severity, evidence, status)
Loại tín hiệu hợp lệ, chỉ dùng đúng 6 mã này:
  PROVIDER_UNMATCHED · DATE_IMPOSSIBLE · DUPLICATE · INVOICE_MISMATCH ·
  CROSSCHECK_FAIL · OTHER_PAYER_PRESENT
Mức độ: LOW · MEDIUM · HIGH.

=== CHÍN QUY TẮC ĐỐI CHIẾU CHÉO — ĐỌC QUA GetCrossCheckResults ===
  R1  Tên bệnh nhân nhất quán giữa T1-01, T1-02, T1-03 (bỏ dấu, không phân biệt hoa thường)
  R2  Ngày sinh nhất quán giữa T1-01 và T1-03 (khớp chính xác)
  R3  Cơ sở y tế nhất quán — mã số thuế khớp chính xác khi có, không thì khớp tên
  R4  Ngày dịch vụ nằm trong kỳ điều trị
  R5  Số học của bảng kê — tổng các dòng khớp footer, sai số ±1 đồng
  R6  Hóa đơn khớp bảng kê — **BỎ QUA KÈM GHI CHÚ khi không có XML**, không phải FAIL
  R7  Thuốc trong đơn xuất hiện trên bảng kê (khớp tên mờ)
  R8  Chẩn đoán nhất quán — ICD khớp chính xác khi cả hai đều có, không thì so văn bản
  R9  Bộ hồ sơ đầy đủ — sinh ra packageState
Mọi rule đều sinh ra CẢNH BÁO có nhãn, neo tại trường. **KHÔNG CÓ GÌ ĐƯỢC TỰ SỬA.**

=== (b) Ý ĐỊNH → TOOL ===
1. Tra cơ sở y tế trong danh mục           → LookupProviderCatalogue(ProviderName, ProviderTaxCode)
2. Kiểm tra logic ngày                      → ValidateClaimDates(ClaimNo)
3. Tìm hồ sơ trùng                          → FindDuplicateClaims(ClaimNo)
4. Đọc kết quả R1–R9                        → GetCrossCheckResults(ClaimNo)
5. Nêu một tín hiệu                        → RaiseFraudSignal(ClaimNo, SignalType, Severity,
                                               EvidenceCsv, ExplanationVi)
6. Kết thúc lượt                            → WriteAgentRun (BẮT BUỘC)

LUẬT XẾP MỨC ĐỘ — dùng đúng bảng này, không tự nới:
- Ngày điều trị ở tương lai, hoặc vào viện sau ra viện          → DATE_IMPOSSIBLE, HIGH
- Ngày điều trị ngoài thời hạn bảo hiểm                          → DATE_IMPOSSIBLE, HIGH
- Ngày chứng từ theo dòng mâu thuẫn với kỳ điều trị               → DATE_IMPOSSIBLE, MEDIUM
- Trùng thành viên + chồng lấn kỳ điều trị, hoặc trùng hash tệp   → DUPLICATE, HIGH
- Trùng ký hiệu + số hóa đơn giữa hai hồ sơ                       → DUPLICATE, HIGH
- Hóa đơn XML không khớp bảng kê (R6 FAIL)                        → INVOICE_MISMATCH, HIGH
- **R6 bị BỎ QUA vì không có XML → KHÔNG PHẢI tín hiệu.**
  Đó là một chứng từ mức EXPECTED còn thiếu, việc của Intake agent, không phải của bạn.
- R1, R2, R3 hoặc R8 thất bại                                     → CROSSCHECK_FAIL, MEDIUM
- R5 sai số học vượt ±1 đồng                                       → CROSSCHECK_FAIL, MEDIUM
- R7 có thuốc trong đơn không thấy trên bảng kê                    → CROSSCHECK_FAIL, LOW
- Không tra được cơ sở y tế trong danh mục                        → PROVIDER_UNMATCHED, MEDIUM
- facilityCountry khác VN                                          → PROVIDER_UNMATCHED, HIGH
- Cột "nguồn khác" (L11) có giá trị trên một hoặc nhiều dòng       → OTHER_PAYER_PRESENT, LOW
  (một bên thứ ba đã trả một phần — hệ thống chưa có quy tắc đóng góp,
   nên đây thuần túy là thứ thẩm định viên cần NHÌN THẤY)

Mọi tín hiệu từ MEDIUM trở lên đều chặn tự động duyệt và đẩy hồ sơ sang UNDER_ASSESSMENT.
Bạn không quyết định điều đó — Server Action quyết định. Bạn chỉ nêu đúng mức.

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- Found = False KHÔNG có nghĩa cơ sở y tế không tồn tại. Có thể do viết tắt hoặc
  sai chính tả. Nêu PROVIDER_UNMATCHED mức MEDIUM, ghi rõ tên đã tra và điểm khớp
  cao nhất, và đề nghị thẩm định viên đối chiếu tờ gốc. Không khớp danh mục là
  CẢNH BÁO, không phải chặn đứng.
- **TUYỆT ĐỐI KHÔNG ĐOÁN QUỐC GIA TỪ TÊN CƠ SỞ Y TẾ.** Một cơ sở tên nghe như
  tiếng nước ngoài vẫn có thể ở Việt Nam. facilityCountry rỗng thì là RỖNG,
  không phải VN và cũng không phải nước ngoài. Đây là một cấm lệnh, không phải gợi ý.
- Không bịa mã cơ sở y tế. Tool trả rỗng thì ghi "không tra được".
- Chứng từ thiếu ngày: ghi "không có ngày trên chứng từ",
  KHÔNG suy ngày từ chứng từ khác.
- Không đủ dữ liệu để kết luận: nêu tín hiệu mức LOW kèm câu hỏi cho thẩm định viên,
  thay vì nâng mức để "cho chắc".

=== NGÔN NGỮ BẮT BUỘC ===
Trong ExplanationVi, TUYỆT ĐỐI KHÔNG dùng các từ: "gian lận", "giả mạo", "khai man",
"kê sai", "trục lợi", "sai phạm". Chỉ mô tả sự kiện quan sát được.
Đúng:  "Ngày điều trị ghi 20/03/2027 nằm sau ngày hôm nay; cần đối chiếu lại chứng từ gốc."
Sai:   "Khách hàng khai man ngày điều trị."

=== (d) QUY ƯỚC TRẢ LỜI ===
- Đã gọi RaiseFraudSignal hoặc WriteAgentRun ⇒ bắt đầu bằng đúng chuỗi "Action taken: "
  rồi liệt kê từng tín hiệu: "PROVIDER_UNMATCHED — MEDIUM".
- Nếu không có tín hiệu nào: bắt đầu bằng "Action taken: không nêu tín hiệu nào."
  rồi liệt kê ĐỦ 4 kiểm tra đã chạy và kết quả từng cái, kèm 9 rule đối chiếu chéo —
  thẩm định viên cần thấy bạn ĐÃ KIỂM, không phải bỏ qua.
- Mỗi tín hiệu kèm đúng 1 câu tiếng Việt cho thẩm định viên.
- BẠN KHÔNG NÊU SỐ TIỀN, KHÔNG ĐỀ XUẤT TỪ CHỐI, KHÔNG SOẠN CÂU CHO KHÁCH.
- Kết thúc bằng đúng 1 dòng "Summary (EN): ".
```

### 3.6 AG-5 — Customer-communication

```
Bạn là Customer-communication Agent. Việc của bạn: lấy MỘT TEMPLATE có sẵn trong
danh mục thông điệp, BIND ĐẦY ĐỦ PLACEHOLDER của nó bằng dữ liệu hồ sơ, và lưu bản nháp.
Thư của bạn là bản nháp. Một con người sẽ đọc và bấm gửi.

=== QUY TẮC TUYỆT ĐỐI: BẠN KHÔNG VIẾT VĂN, BẠN BIND TEMPLATE ===
Mọi câu gửi tới khách hàng PHẢI là một template hoàn chỉnh của danh mục thông điệp
với mọi placeholder đã được bind. **CẤM GHÉP CÂU TIẾNG VIỆT TỪ CÁC MẢNH.**
Cấm diễn đạt lại một template "cho tự nhiên hơn".
Cấm thêm một câu của riêng bạn vào phần Lý do.
Cấm dịch, cấm rút gọn, cấm mở rộng.
Nếu một placeholder KHÔNG BIND ĐƯỢC vì thiếu dữ liệu, bạn DỪNG LẠI và báo
placeholder nào thiếu. Bạn KHÔNG điền tạm, KHÔNG bỏ trống, KHÔNG viết vòng qua nó.
Một template có placeholder chưa bind là một văn bản có chỗ trống,
trên giấy tiêu đề của doanh nghiệp bảo hiểm, nói về một khoản tiền.

=== (a) SCHEMA ĐƯỢC PHÉP ĐỌC ===
- Claim(claimNo, claimState, serviceDateFrom, serviceDateTo, providerName)
- ChainRun(totalClaimed, settlementAmount, totalDeclined)   — qua GetClaimResultSummary
- DeclineByCode(reasonCode, lineNo, itemName, itemAmount)    — qua GetClaimResultSummary
- MessageTemplate(messageCode, templateText, placeholderCsv) — qua GetMessageTemplate
Bạn KHÔNG được đọc hay nhắc: số CCCD, số thẻ BHYT, số tài khoản đầy đủ, ngày sinh.
Số tài khoản chỉ hiển thị dạng che, đúng như tool trả về.

MÃ THÔNG ĐIỆP được phép dùng:
  CLM-LETTER-001  Thông báo chi trả bồi thường
  CLM-LETTER-002  Thông báo từ chối bồi thường
  CLM-NOTIF-001   Đã nhận hồ sơ
  CLM-NOTIF-003   Cần bổ sung chứng từ
  CLM-NOTIF-005   Đã được duyệt
  CLM-NOTIF-006   Không thuộc phạm vi chi trả
  CLM-NOTIF-008   Chi trả một phần
  CLM-NOTIF-009   Đã hủy theo yêu cầu

BẢY MÃ TỪ CHỐI và câu tương ứng — lấy qua GetDeclineReasonSentence, DÙNG NGUYÊN VĂN:
  LIMIT   placeholder: {benefitName} {periodKey} {remainingLimit}
  PERIOD  placeholder: {serviceDate} {coverageFrom} {coverageTo}
  NOBEN   placeholder: {planName} {benefitName}
  WAIT    placeholder: {benefitName} {waitingDays} {coverageFrom} {serviceDate}
  EXCL    placeholder: {itemName} {itemAmount} {clauseRefList}
  NOTREL  placeholder: {itemName} {itemAmount} {diagnosis}
  NOTCOV  placeholder: {itemName} {itemAmount}
Một từ chối LUÔN nêu tên hạng mục và số tiền của nó. Không bao giờ chỉ một chữ "từ chối".

=== (b) Ý ĐỊNH → TOOL ===
1. Lấy số liệu của hồ sơ                  → GetClaimResultSummary(ClaimNo)
2. Lấy template                            → GetMessageTemplate(MessageCode, Locale)
3. Lấy câu từ chối đã bind cho một dòng     → GetDeclineReasonSentence(ReasonCode, LineNo)
4. Lưu bản nháp                            → SaveLetterDraft(ClaimNo, MessageCode,
                                               DraftTextVi, DraftSummaryEn)
5. Kết thúc lượt                           → WriteAgentRun (BẮT BUỘC)

THỨ TỰ BẮT BUỘC: 1 → 2 → 3 cho từng mã từ chối → 4.
Không bao giờ dựng thư trước khi gọi (1) và (2).

=== BA ĐIỀU CẤM — ÁP DỤNG CHO MỌI THÔNG ĐIỆP KHÔNG TRỪ CÁI NÀO ===
CẤM 1 — KHÔNG BUỘC TỘI.
  TUYỆT ĐỐI KHÔNG dùng: "kê sai", "gian lận", "khai man", "trục lợi", "giả mạo",
  "sai phạm", "vi phạm", "không trung thực". Kể cả khi hồ sơ có tín hiệu bất thường.
  Tín hiệu của Fraud-signal agent KHÔNG BAO GIỜ xuất hiện trong bất kỳ thông điệp nào
  gửi khách, dưới bất kỳ cách diễn đạt nào.

CẤM 2 — KHÔNG QUY QUYẾT ĐỊNH CHO AI HAY CHO MÔ HÌNH.
  TUYỆT ĐỐI KHÔNG viết "hệ thống AI xác định", "mô hình đánh giá", "thuật toán",
  "tự động phân tích", "agent", hay bất kỳ cách nói nào gán quyết định cho máy.
  Quyết định là của doanh nghiệp bảo hiểm, căn cứ trên quy tắc bảo hiểm và hồ sơ.
  Viết "chúng tôi", không viết "hệ thống".
  Cũng không được lộ tên trạng thái hay mã nội bộ: PENDING_INFO, NOTCOV,
  EXTRACTION_ERROR là ngôn ngữ của console, không phải của khách hàng.

CẤM 3 — LUÔN CÓ ĐƯỜNG KHIẾU NẠI.
  Mọi thông báo từ chối PHẢI có mục quyền yêu cầu xem xét lại: xin xem xét lại
  bằng cách nào, gửi cho ai, trong thời hạn bao lâu. Thiếu mục này thì KHÔNG LƯU
  BẢN NHÁP — báo thiếu và dừng. Một quyết định từ chối không kèm đường phản hồi
  là cách chắc chắn nhất để biến một quyết định thành một khiếu nại.

=== ĐIỀU CẤM THỨ TƯ, CHỈ ÁP DỤNG CHO THƯ TỪ CHỐI ===
KHÔNG BAO GIỜ in ra một tham chiếu điều khoản CHƯA ĐƯỢC BAN HÀNH.
Nếu GetDeclineReasonSentence trả HasProvisionalClauseRef = True — nghĩa là
{clauseRefList} còn chứa một tham chiếu tạm — thì **DỪNG LẠI, KHÔNG LƯU BẢN NHÁP**,
và báo rằng điều khoản chưa có số hiệu chính thức nên chưa thể ra thư từ chối.
Một quyết định từ chối sai có thể được đảo ngược. Một văn bản có chỗ trống
đã đến tay khách hàng thì không.

=== LUẬT SỐ TIỀN ===
- MỌI con số phải sao chép nguyên văn từ GetClaimResultSummary.
- BẠN KHÔNG CỘNG, KHÔNG TRỪ, KHÔNG KIỂM TRA LẠI PHÉP TÍNH, KHÔNG LÀM TRÒN.
- Tổng các phần bị từ chối trông không khớp tổng chung: KHÔNG tự sửa cho khớp.
  Dừng lại, báo bất thường, không lưu bản nháp.
- Định dạng tiền: 1.880.000 ₫. Định dạng ngày: dd/MM/yyyy.

=== CẤU TRÚC THƯ ===
CLM-LETTER-001 (chi trả): tiêu đề và khối tham chiếu · kết quả · bảng chi tiết
  từng hạng mục · phần khách tự chi trả, NHÓM THEO MÃ TỪ CHỐI với câu nguyên văn
  của từng mã · hạn mức còn lại · kênh liên hệ.
CLM-LETTER-002 (từ chối): cùng tiêu đề và khối tham chiếu, rồi:
  · Kết luận — một câu rõ ràng rằng hồ sơ không thuộc phạm vi chi trả
  · Lý do — câu của danh mục cho mã áp dụng, MỌI placeholder đã bind
  · Căn cứ — điều khoản hoặc CÁC điều khoản được viện dẫn, mỗi điều khoản kèm
    số điều số khoản và NỘI DUNG TRÍCH NGUYÊN VĂN, ở đúng phiên bản hồ sơ được
    đánh giá. Nhiều điều khoản cùng fire trên một dòng thì in ĐỦ, theo đúng thứ tự
    engine trả về. **Không bao giờ tóm tắt, không bao giờ diễn giải lại,
    không bao giờ chỉ in điều khoản đầu tiên.**
  · Điều không bị ảnh hưởng — nói rõ hợp đồng vẫn tiếp tục, quyền lợi khác không đổi
  · Quyền yêu cầu xem xét lại

=== (c) THIẾU THÔNG TIN THÌ HỎI LẠI, KHÔNG BỊA ===
- Thiếu bất kỳ số nào, hoặc bất kỳ placeholder nào: KHÔNG dựng thư. Nói rõ thiếu gì và dừng.
- Không bịa điều khoản, không bịa số điều, không bịa nội dung điều khoản.
- Không hứa thời hạn chi trả nếu tool không cung cấp.
- Không trả lời câu hỏi y khoa. Chuyển sang thẩm định viên.
- Chỉ nói về hồ sơ đang xử lý. Từ chối mọi câu hỏi về hồ sơ của người khác.

=== (d) QUY ƯỚC TRẢ LỜI ===
- Đã gọi SaveLetterDraft hoặc WriteAgentRun ⇒ bắt đầu bằng đúng chuỗi "Action taken: "
  rồi ghi: mã thông điệp, số placeholder đã bind trên tổng số, trạng thái
  "Draft — chờ người duyệt gửi".
- Sau đó in toàn văn bản nháp.
- Kết thúc bằng đúng 1 dòng "Summary (EN): ".
- Luôn nhắc một câu ở cuối phần báo cáo (KHÔNG nằm trong thư):
  "Bản nháp, chưa gửi cho khách hàng."
```

---

## 4. Schema `AgentRun`

Entity append-only trong `CLM_Core`. Không có Update action, không có Delete action — đây là điều kiện để giám khảo tin vào trace.

| Field | Kiểu | Bắt buộc | Ý nghĩa | Ví dụ |
|---|---|---|---|---|
| `AgentRunId` | Long Integer, auto | ✓ | Khoá | `4417` |
| `ClaimNo` | Text(20) | ✓ | Hồ sơ. Cũng là correlation id gửi sang IDP | `CLM-26-000417` |
| `EventNo` | Text(24) | | Sự kiện bảo hiểm trong hồ sơ | `CLM-26-000417/1` |
| `AgentCode` | Text(8) | ✓ | `AG-0`…`AG-5` | `AG-3` |
| `AgentName` | Text(50) | ✓ | Tên hiển thị | `Clinical` |
| `StepName` | Text(60) | ✓ | Bước trong vòng đời của agent | `AssessLines` |
| `ToolCalled` | Text(80) | | Tên Server Action đã gọi; rỗng nếu lượt chỉ suy luận | `ProposeRecommendation` |
| `InterfaceCode` | Text(8) | | Mã giao diện khi lượt này chạm hệ ngoài | `I-16` |
| `InputSummary` | Text(500) | ✓ | Tóm tắt đầu vào, **đã che PII** | `lossType=ILL_OP; 12 lines; state=COMPLETE; linesAssessed=[1,2]` |
| `OutputSummary` | Text(1000) | ✓ | Tóm tắt đầu ra | `line 1 → NOTREL band HIGH (2 evidence); line 2 → no recommendation` |
| `HandoffTo` | Text(20) | ✓ | `Intake · Clinical · FraudSignal · Adjudication · CustomerComms · Orchestrator · Human · None` | `Orchestrator` |
| `KnowledgeVersion` | Text(60) | | `knowledgeBaseVersion` ghim theo hồ sơ (D1 BR-KB-4) | `kb-2026-08-01` |
| `ModelId` | Text(60) | ✓ | Model connection trên tenant | `TrialClaudeHaiku4_5` |
| `DurationMs` | Integer | ✓ | Thời gian lượt | `8420` |
| `IsReplay` | Boolean | ✓ | `True` khi chạy trên bộ ghi sẵn | `False` |
| `CorrelationId` | Text(40) | ✓ | Gom một lượt xử lý hồ sơ qua nhiều agent | `RUN-000417-03` |
| `SequenceNo` | Integer | ✓ | Thứ tự trong `CorrelationId`, để trace hiển thị đúng khi timestamp trùng | `7` |
| `CreatedAt` | DateTime | ✓ | Mốc thời gian | `16/09/2026 14:22:07` |

### 4.1 Một bản ghi ví dụ

```
AgentRunId        : 4417
ClaimNo           : CLM-26-000417
EventNo           : CLM-26-000417/1
AgentCode         : AG-3
AgentName         : Clinical
StepName          : AssessLines
ToolCalled        : ProposeRecommendation
InterfaceCode     : I-16
InputSummary      : lossType=ILL_OP; planCode=HC-PLS; episodeType=OUTPATIENT;
                    diagnosis=N39.0; 12 cost lines; assessment state=COMPLETE;
                    linesAssessed=[1,2]; KB query "statin indications"
OutputSummary     : line 1 "Atorvastatin 20mg" (DRUG, 168.000 ₫) → NOTREL,
                    confidence 0.91, band HIGH, evidence =
                    KB_ENTRY:kb/lipid-lowering-agents/indications; DIAGNOSIS:N39.0.
                    Status=Proposed. line 2 (IMAGING) → no recommendation.
                    10 dòng còn lại KHÔNG nằm trong linesAssessed — chưa được xem.
HandoffTo         : Orchestrator
KnowledgeVersion  : kb-2026-08-01
ModelId           : TrialClaudeHaiku4_5
DurationMs        : 8420
IsReplay          : False
CorrelationId     : RUN-000417-03
SequenceNo        : 7
CreatedAt         : 16/09/2026 14:22:07
```

### 4.2 Ba quy tắc không được phá

1. **Lượt không ghi được `AgentRun` là lượt hỏng.** `WriteAgentRun` lỗi thì agent phải báo lỗi chứ không được coi như xong việc.
2. **`InputSummary` và `OutputSummary` đã che PII trước khi ghi.** Không có họ tên đầy đủ, ngày sinh, số CCCD, số thẻ BHYT, số tài khoản — vì màn Presenter chiếu bảng này lên cho giám khảo xem.
3. **`OutputSummary` của AG-3 phải phân biệt được "không có khuyến nghị" với "chưa được xem".** Hai thứ đó khác nhau và trace là nơi duy nhất chứng minh hệ thống phân biệt được.

---

## 5. Phương án dự phòng — nếu Agent Workbench không cho agent gọi agent

Đây là giả định rủi ro nhất của cả kiến trúc, và là câu hỏi số 1 cho SA. Phương án dưới đây phải sẵn sàng trước khi hỏi, để câu trả lời "không" không làm mất một ngày.

### 5.1 Thay đổi duy nhất

AG-0 thôi là agent, trở thành **Server Action `OrchestrateClaim`** (bọc trong một BPT process nếu cần chờ lâu). **Năm agent còn lại không đổi một dòng nào** — prompt giữ nguyên, tool list giữ nguyên, `AgentRun` giữ nguyên. Câu chuyện kể cho giám khảo cũng giữ nguyên, vì hand-off vẫn nằm trong `AgentRun` với `HandoffTo`.

### 5.2 Hiện thực cụ thể

**Bước 1 — Entity `AgentQueue`** (mới, trong `CLM_Core`):

| Field | Kiểu | Ý nghĩa |
|---|---|---|
| `AgentQueueId` | Long Integer | Khoá |
| `ClaimNo` | Text(20) | Hồ sơ |
| `TargetAgentCode` | Text(8) | `AG-1`…`AG-5` |
| `InstructionText` | Text(2000) | Câu lệnh gửi agent |
| `Status` | Text(12) | `Pending · Running · Done · Error` |
| `ResponseText` | Text(4000) | Đầu ra agent trả về |
| `CorrelationId` | Text(40) | Gom cùng lượt |
| `SequenceNo` | Integer | Thứ tự |
| `EnqueuedAt` / `StartedAt` / `FinishedAt` | DateTime | Đo thời gian |

**Bước 2 — `InvokeAgentStep` đổi hiện thực, giữ nguyên chữ ký.** Đây là điểm mấu chốt: prompt của AG-0 không cần sửa vì tool vẫn tên đó, vẫn tham số đó. Bên trong:
- Nhánh A (agent→agent được hỗ trợ): gọi thẳng agent đích, trả `ResponseText`, `Dispatched = True`.
- Nhánh B (không được hỗ trợ): ghi một dòng `AgentQueue` với `Status = Pending`, trả `Dispatched = True` và `ResponseText = "queued"`.

Chọn nhánh bằng **Site Setting `AgentToAgentSupported` (Boolean)**. Bật/tắt không cần sửa code, không cần republish agent.

**Bước 3 — Server Action `OrchestrateClaim(ClaimNo)`** thay vai trò suy luận của AG-0 bằng một máy trạng thái tường minh:

```
OrchestrateClaim(ClaimNo):
  ctx = GetClaimContext(ClaimNo)
  Switch ctx.ClaimState:
    SUBMITTED / DOC_PROCESSING
                     → Enqueue("AG-1", "Đọc chứng từ và đối chiếu checklist theo lossType")
                       WriteAgentRun(AG-0, "StartOrchestration", HandoffTo = "Intake")
    packageState = COMPLETE
                     → Enqueue("AG-3", …)  WriteAgentRun(HandoffTo = "Clinical")
                       Enqueue("AG-4", …)  WriteAgentRun(HandoffTo = "FraudSignal")
    GetTrackStatus.AllSettled
                     → Enqueue("AG-2", …)  WriteAgentRun(HandoffTo = "Adjudication")
    ChainRun mới nhất đã có
                     → r = ApplyGateDecision(ClaimNo, latestChainRunId)
                       If r.ResultState in (APPROVED, DECLINED)
                            → Enqueue("AG-5", messageCode)  WriteAgentRun(HandoffTo = "CustomerComms")
                       Else → BuildAssessorTodo(ClaimNo)     WriteAgentRun(HandoffTo = "Human")
    PENDING_INFO     → Enqueue("AG-5", "CLM-NOTIF-003")  WriteAgentRun(HandoffTo = "CustomerComms")
    EXTRACTION_ERROR → RouteToOperator                   WriteAgentRun(HandoffTo = "Human")
```

**Bước 4 — Timer `AgentDispatcher`**, chu kỳ 5 giây: lấy các dòng `AgentQueue.Status = Pending` theo `SequenceNo`, gọi agent tương ứng, ghi `ResponseText`, đặt `Done`, rồi gọi lại `OrchestrateClaim(ClaimNo)` để đi bước kế. Quá 120 giây → `Error`, `AgentRun` với `StepName = "AGENT_TIMEOUT"`, `HandoffTo = "Human"`.

**Bước 5 — Không đổi gì ở màn Presenter.** Trace vẫn đọc `AgentRun`, vẫn hiện 13 hand-off. Khác biệt duy nhất: dòng của AG-0 có `ModelId = "n/a (deterministic orchestrator)"` thay vì tên model.

### 5.3 Đường thứ ba — A2A, nếu còn thời gian

Nếu Agent Workbench cho agent→agent nhưng chỉ **trong cùng một app**, có thể dùng **A2A connection** cho một cặp agent để chứng minh khả năng liên thông: publish AG-3 ra agent card, tạo A2A connection trong ODC Portal → Connections, rồi gọi qua `SendMessage`. **Bắt buộc bọc qua wrapper tham số phẳng** — `SendMessage` nhận list `UserMessage` với các field loại trừ lẫn nhau, để LLM tự dựng sẽ lỗi rời rạc kiểu `Missing text content in TextPart`, chỉ thấy trong Monitoring → Traces. Đây là việc **sau** khi sàn đã vững, không phải trước.

---

## 6. Thứ tự dựng và đường cắt

### 6.1 Thứ tự dựng

Nguyên tắc: **dựng theo chiều con số chạy được**, không theo chiều sơ đồ đẹp.

| Thứ tự | Agent | Vì sao trước | Phụ thuộc |
|---|---|---|---|
| 1 | **AG-2 Adjudication** | Là agent duy nhất chứng minh trọn vẹn nguyên tắc kiến trúc: gọi chuỗi 14 bước như một tool, không tự tính, ra đúng một con số. Một agent này đã đủ sàn | Chuỗi 14 bước xong |
| 2 | **AG-0 Orchestrator** | Có agent thứ hai là có hand-off, là có cái để chấm | AG-2 + `InvokeAgentStep` |
| 3 | **AG-3 Clinical** | Là phần "thông minh" nhất, lấp đúng khoảng trống mà IDP hôm nay chưa có (xem §8 mục 5), và là linh hồn kịch bản demo | I-15/I-16 + KB |
| 4 | **AG-1 Intake** | Mở đầu luồng, nhưng có thể thay tạm bằng seed dữ liệu | I-07/I-09 + adapter |
| 5 | **AG-4 Fraud-signal** | Tách rời, không chặn ai | Danh mục cơ sở y tế + R1–R9 |
| 6 | **AG-5 Comms** | Đầu ra cuối, dựng nhanh nhất, ít rủi ro nhất vì chỉ bind template | A2 templates trong seed |

**Dựng bằng gì:** `app_create` với `kind: "AIAgent"` qua outsystems-mcp dựng được vỏ agent và nối model connection. Ba điều đã kiểm chứng trên tenant ngày 08/09, phải nhớ:
- **Timeout ≠ thất bại.** Asset xuất hiện sau ~21 giây, client có thể timeout ở ~66 giây. Sau mỗi timeout: **không retry**, đọc `context_agents` xác minh trước. Retry mù → asset trùng hoặc lỗi `OS-APPS-40018`.
- **Luôn truyền `model` tường minh.** Tenant có 3 connection (`TrialGPT5`, `TrialGPT4o` là entitlement Trial có hạn mức; `OpenAI_Model` là Customer). Bỏ trống là lỗi ngay.
- **Wiring model không đọc được bằng tool read-only** — `context_agents` trả `providerName: null` cho cả 14 agent. Muốn chắc thì mở ODC Studio nhìn.

Vỏ agent do MCP dựng ra **không phải agent tốt**. Chất lượng nằm ở system prompt §3 và tool list đóng §1 — hai thứ đó làm tay, dán vào.

**Việc phải làm tay, Mentor không làm được:** REST Consume tới IDP (`User-Agent` giả trình duyệt, `Idempotency-Key`, xử lý `409 NOT_READY` với `Retry-After`, ký HMAC-SHA256 cho webhook I-08), chuỗi 14 bước, cổng 3 điều kiện, parser XML hoá đơn, và toàn bộ system prompt.

### 6.2 Đường cắt khi trễ

Cắt theo đúng thứ tự này. Cắt sớm hơn một bước còn hơn cắt muộn nửa ngày.

| # | Cắt cái gì | Mất gì | Còn giữ được gì |
|---|---|---|---|
| 1 | Bỏ kịch bản phụ (thời gian chờ / nha khoa) | Một câu chuyện phụ | Ba kịch bản chính |
| 2 | Bỏ chat hỏi đáp trên app khách | Một tính năng "S" | Toàn bộ luồng chính |
| 3 | Bỏ màn dashboard console | Tiles KPI | Trace, worklist, chi tiết |
| 4 | Bỏ AG-5, thư dựng sẵn dạng tĩnh từ template A2 | Một agent, một hand-off | 5 agent, 11 hand-off |
| 5 | Bỏ AG-4, tín hiệu seed sẵn | Kịch bản chặn sống động | Cổng vẫn chặn đúng, hồ sơ vẫn không tự duyệt |
| 6 | Bỏ AG-1, chứng từ nạp bằng seed + bộ ghi sẵn | Track 1 live | AG-0 + AG-2 + AG-3 — vẫn đủ kể chuyện agentic |
| 7 | Bỏ AG-3, khuyến nghị seed sẵn ở `Status = Proposed` | Track 2 live | Người vẫn xác nhận, chuỗi vẫn chạy lại, số tiền vẫn đổi đúng |
| 8 | Bỏ AG-0, chỉ còn AG-2 | Hand-off | Sàn |
| 9 | Bỏ gọi IDP thật, chạy bộ phản hồi ghi sẵn | IDP live | Mọi thứ khác, chạy offline |

### 6.3 Sàn tuyệt đối không được thủng

> **Một agent trên Agent Workbench, gọi một Server Action tất định làm tool, ra một con số đúng, và có bản ghi `AgentRun` chứng minh nó đã gọi.**

Cụ thể: **AG-2 gọi `RunAdjudicationChain`, hồ sơ demo ra đúng số tiền, chạy lại lần hai ra cùng `inputsHash` và cùng số tiền, và `AgentRun` có một dòng `ToolCalled = RunAdjudicationChain`.**

Nếu chỉ còn 2 giờ trước pitch và mọi thứ khác hỏng, đây là thứ phải chạy được. Mọi quyết định cắt đều phải trả lời được câu: *cắt cái này thì sàn có còn không?*

---

## 7. Checklist chứng minh cho giám khảo

Mọi mục dưới đây phải **chụp hoặc quay được**, và phải có bản dự phòng offline quay sẵn trước 06/10.

### 7.1 Bản đồ agent

- [ ] Ảnh chụp **ODC Studio** cho thấy 6 agentic app trong module `CLM_Agents`.
- [ ] Ảnh chụp tab **Action Calling** của AG-2 cho thấy tool list **đóng**, đúng 5 tool, **không có** tool nào ghi tiền.
- [ ] Ảnh chụp tab Action Calling của AG-3 cho thấy có `ProposeRecommendation` và **không có** `ConfirmRecommendation`.
- [ ] Sơ đồ 4 tầng với mũi tên giữa Agentic và Deterministic ghi rõ **"calls as a tool · never writes"**.
- [ ] Bảng §0.5 in ra một trang: danh sách Server Action bị cấm — đưa cho giám khảo cầm tay khi họ hỏi "làm sao các anh chắc agent không tiêu tiền?".

### 7.2 Trace `AgentRun` có `HandoffTo`

- [ ] Quay màn Presenter → Agent trace, cuộn hết, đếm được **≥ 6 hand-off** (thực tế 13), mỗi dòng thấy rõ `AgentCode`, `ToolCalled`, `InterfaceCode`, `HandoffTo`, `DurationMs`, `ModelId`, `KnowledgeVersion`.
- [ ] Quay cảnh **trace cập nhật ngay** sau khi thẩm định viên bấm "Xác nhận" khuyến nghị — hai dòng mới xuất hiện (`AG-0 → Adjudication`, `AG-2 → Orchestrator`). Đây là bằng chứng hand-off là thật, không phải log dựng sẵn.
- [ ] Ảnh chụp một dòng `AgentRun` mở rộng đầy đủ, giống §4.1 — đặc biệt dòng phân biệt "no recommendation" với "chưa được xem".
- [ ] Chứng minh **append-only**: entity `AgentRun` không có Update/Delete action nào được expose.

### 7.3 Số tiền chạy lại ra giống nhau

- [ ] Quay cảnh bấm **"Chạy lại chuỗi"** hai lần liên tiếp, không sửa gì → **`inputsHash` giống hệt, `settlementAmount` giống hệt**. Chiếu cả hai `ChainRun` cạnh nhau.
- [ ] Quay cảnh **trước/sau** sau khi xác nhận một khuyến nghị `NOTREL`: số tiền đổi đúng một lượng, và `CompareChainRuns` chỉ ra đúng dòng nào, bước nào.
- [ ] Nói được câu này khi bị hỏi: *chuỗi làm tròn ở từng bước, không phải một lần ở cuối — đó là lý do lần chạy lại trả về cùng một tổng.*
- [ ] Chạy cùng hồ sơ ở chế độ ghi sẵn (ngắt mạng) → **cùng hash, cùng số tiền**.
- [ ] Ảnh chụp `AgentRun` của AG-2 cho thấy `OutputSummary` chỉ **sao chép** con số từ tool, không có phép tính nào của model.

### 7.4 Bốn bằng chứng phụ nên có

- [ ] **Cổng chặn được thanh toán**: một tín hiệu `DATE_IMPOSSIBLE (HIGH)` → hồ sơ sang `UNDER_ASSESSMENT`, và chứng minh **không tồn tại `PaymentInstruction` nào** cho hồ sơ đó.
- [ ] **Lỗi kỹ thuật không bao giờ thành từ chối**: ép lỗi → `EXTRACTION_ERROR` + hàng đợi operator, không phải `DECLINED`.
- [ ] **Bằng chứng bắt buộc**: gọi `ProposeRecommendation` với `EvidenceCsv` rỗng → tool từ chối ghi. Gọi với `ProposedReasonCode = "EXCL"` → tool cũng từ chối ghi.
- [ ] **Ảnh hóa đơn bị từ chối đúng cách**: nộp một ảnh chụp hóa đơn → `ParseEInvoiceXml` trả `Valid = False`, AG-1 xin file XML, **và ảnh đó không bao giờ chạy qua OCR**.

### 7.5 Bốn câu hỏi khó và câu trả lời một dòng

| Câu hỏi | Trả lời |
|---|---|
| "Làm sao các anh chắc agent không tự bịa số tiền?" | Agent không có tool nào ghi trường tiền — đây là danh sách action bị cấm, và đây là tool list đóng của từng agent trong Studio. |
| "Nếu model đổi phiên bản thì số tiền có đổi không?" | Không. Số tiền do chuỗi 14 bước sinh ra, cùng đầu vào cho cùng `inputsHash`. Model chỉ đổi câu chữ giải thích. |
| "AI của các anh quyết định từ chối khách hàng à?" | Không. AI đề xuất đúng hai mã cấp dòng, kèm bằng chứng trích dẫn, và không ràng buộc gì cho tới khi một con người xác nhận. Năm mã còn lại do chuỗi tất định sinh ra. Và không có mã nào tên là "gian lận". |
| "Agent của các anh có thực sự nói chuyện với nhau không, hay chỉ là một script?" | Đây là trace 13 hand-off, mỗi dòng có agent nguồn, tool đã gọi, agent đích và thời gian thực thi — và đây là lúc thẩm định viên bấm xác nhận, trace sinh thêm hai dòng ngay trước mặt các anh. |

---

## 8. Chênh lệch phát hiện khi đổi chuẩn — cần chốt

Ghi lại vì mỗi mục đều làm đổi prompt hoặc đổi tool.

| # | Phát hiện | Nguồn | Hệ quả |
|---|---|---|---|
| 1 | **Cổng tự động duyệt có BA điều kiện, không phải bốn.** Điều kiện 1 gộp "dưới ngưỡng" với "không có gì bị cắt"; điều kiện 3 "không có term checkpoint mở" là mới ở v1.4 | A5, D1 §5.8 quyết định D-5 | §0.4 và prompt AG-0 đã sửa. Tín hiệu gian lận **không** là điều kiện cổng — nó là chốt chặn riêng |
| 2 | **A4 v1.3 định nghĩa TÁM loại chứng từ, không phải bảy.** `T2-04` (biên bản/xác nhận hoàn cảnh sự cố) thêm ở v1.3 | A4 §1.3 | `T2-04` **chỉ tới hồ sơ qua một term**, không xuất hiện trong bất kỳ dòng nào của bảng checklist theo loại tổn thất, và mức mặc định của nó cố ý còn bỏ ngỏ (`B-24`). Prompt AG-1 giữ đúng bảy loại của checklist và **không nhắc `T2-04`** — đúng với trạng thái hiện hành. **A1 v1.5 §6 vẫn ghi "Seven in MVP 1" — hai tài liệu lệch nhau, cần chốt** |
| 3 | **Không có mã `FRAUD` và không có câu thư nào cho nó.** Bảy mã từ chối là tập đóng, A2 §4 có đúng bảy câu | A1 §5.2, A2 §4 | Prompt AG-4 §3.5 đã nêu thẳng: kết quả dừng ở trạng thái chặn, không sinh thư, không hiển thị cho khách |
| 4 | **Bảng checklist A4 §6 có SÁU dòng loại sự kiện, không phải tám.** Ngoại trú-bệnh · ngoại trú-có thuốc · nha khoa · tai nạn · nội trú · thai sản | A4 §6 | Prompt AG-1 dùng đúng sáu dòng. Bảng khoá trên `episodeType` (phép chiếu của `lossType`), không trực tiếp trên 8 giá trị `lossType` sống — **`lossType` chưa thay được `episodeType` vì A5 §8.1 vẫn khoá trên `episodeType`**; cần chốt khi nào rút gọn |
| 5 | **I-15/I-16 là hợp đồng của Claim MVP, KHÁC hẳn thứ IDP đang chạy.** Hợp đồng nói IDP trả `proposedReasonCode` + `evidence[]` + `confidenceBand` + `linesAssessed`. Đối chiếu thật lên staging: endpoint chạy (H6 ✅) nhưng **`proposedReasonCode` (H7), `evidence[]` (H8), band (H9), `linesAssessed` (H11) đều KHÔNG CÓ**; đầu ra thật là tập nhận xét mức hồ sơ kèm điểm nghiêm trọng, và điểm đó là **trung bình cộng** | D3 §I-15/I-16 (Confirmed 21/08) vs `IDP_APIs_for_Claim_MVP_v1.0` nhóm H | **Đây chính là lý do AG-3 tồn tại.** Prompt §3.4 giữ nguyên cảnh báo "trung bình cộng nên lỗi nặng bị pha loãng" và số đo 0,555 vs 0,728, nhưng **đóng khung lại theo 7 mã từ chối**: AG-3 nhận đầu ra mức hồ sơ làm **đầu vào** và tự sinh ra thứ hợp đồng I-16 yêu cầu. Nguồn đã ghi rõ trong prompt và trong bảng này |
| 6 | **Rule đối chiếu chéo là R1–R9 của A4 §5**, không phải R1–R5 của MBAL. `R6` (hóa đơn khớp bảng kê) **bị bỏ qua kèm ghi chú khi không có XML** — không phải FAIL | A4 §5 | Tool `GetCrossCheckResults` và prompt AG-4 đã thay. Phân biệt "bỏ qua" với "thất bại" được nêu tường minh |
| 7 | **Làm tròn: nhận số đồng nguyên gần nhất, nửa ra xa số 0, ở TỪNG BƯỚC.** Không phải "làm tròn xuống 1.000 ₫" | A5 §6.2 | Prompt AG-2 đã sửa, kèm lý do (chuỗi chạy lại phải ra cùng tổng) |
| 8 | **Bước trừ BHYT cố ý KHÔNG CÓ trong chuỗi.** Cột `L8` vẫn trích xuất nhưng không bước nào tiêu thụ | A5 §6.1, simplification S-17 | Prompt AG-2 phải nói thẳng khi được hỏi, không tự trừ. **Đây là điều kiện cổng trước khi có tiền thật chạy qua I-11** |
| 9 | **Cột `L16` của bảng kê (ngày đến) không được mặc định bằng cột `L15`.** Sửa ở A4 v1.3 vì một kỳ nằm viện 14 ngày bị mặc định thành 1 ngày sẽ qua mọi giới hạn số ngày mà không ai biết | A4 §4.1 | Prompt AG-1 có cấm lệnh riêng cho việc này |
| 10 | **Cấm đoán quốc gia cơ sở y tế từ tên cơ sở.** `facilityCountry` rỗng là rỗng, không phải `VN` | A4 field 18 | Prompt AG-4 có cấm lệnh riêng |
| 11 | **Mọi term của sản phẩm thí điểm đang mang tham chiếu điều khoản tạm (`TBC-nn`)**, và quy tắc cấm từ chối trên một tham chiếu tạm | A2 §7.2, A5 §7.2, D1 `BR-TERM-10` | Tool `GetDeclineReasonSentence` trả cờ `HasProvisionalClauseRef`, prompt AG-5 có điều cấm thứ tư. **Thực tế hôm nay: chưa một thư từ chối theo `EXCL` nào ra khỏi được hệ thống** — cần chốt với nghiệp vụ |
| 12 | **`CLM-NOTIF-005` (đã duyệt) và `-008` (chi trả một phần) chưa có ranh giới.** Bước 10 áp đồng chi trả nên gần như mọi hồ sơ đều tất toán thấp hơn số yêu cầu | A2 §10 mục A2-09 | AG-5 hiện chọn mã theo `messageCode` do AG-0 truyền vào, nên quyết định nằm ở `ApplyGateDecision` chứ không ở prompt. **Cần nghiệp vụ chốt** trước khi demo, vì nó đổi câu chữ khách đọc trên hầu hết hồ sơ |

---

## 9. Giả định còn phải xác thực với SA OutSystems

| # | Câu hỏi | Ảnh hưởng nếu sai | Ai trả lời |
|---|---|---|---|
| 1 | Callable-action list của agent có nhận **AGENT khác** không, hay chỉ Server Action? | Quyết định AG-0 là agent hay Server Action. Đã có phương án §5, không chặn | SA — ưu tiên số 1 |
| 2 | Tool input có bắt buộc phẳng không, hay ODC tự dựng được Structure lồng? | Nếu phẳng bắt buộc thì toàn bộ 34 chữ ký ở §1.1 đúng như đã viết. Đặc biệt `IdpStartClinicalAssessment` — payload I-15 có `clinicalContext` + `lines[]` lồng | SA |
| 3 | `description` của Server Action ảnh hưởng thế nào tới việc model chọn tool? Có trần số tool mỗi agent không? | AG-1 và AG-3 mỗi agent 8 tool | SA |
| 4 | Guardrail và audit trail **có sẵn** tới đâu? Có phải tự ghi `AgentRun` toàn bộ không? | Nếu nền tảng có sẵn trace thì `AgentRun` thành lớp bổ sung nghiệp vụ | SA |
| 5 | Agent xử lý **long-running** thế nào, và có cơ chế **retry theo `Retry-After`** trong một lượt tool không? | `409 NOT_READY` của I-09 và I-16 là ngoại lệ 4xx duy nhất phải thử lại. Nếu agent không làm được trong lượt thì phải đẩy hết sang Timer | SA |
| 6 | Giới hạn EAP ngày pitch: session timeout, TTL token, số lượt gọi model, hạn mức Trial của `TrialGPT5`/`TrialGPT4o`? | Nếu Trial có trần lượt thì phải wire ít nhất AG-2 vào `OpenAI_Model` (Customer) | SA + Partner Manager |
| 7 | Agent có chạy được dưới **service identity với role riêng** không? | Cần để chứng minh agent không có quyền ghi tiền ở tầng nền tảng chứ không chỉ ở tool list | SA |
| 8 | Có cách nào **khoá** tool list của agent để lần publish sau không vô tình thêm tool? | Bảo vệ ranh giới §0.5 khỏi lỗi người | SA |

Ngoài SA, bốn giả định phải xác thực với **đội IDP** (đều là ❌ ở nhóm H của bản đối chiếu): flow đánh giá có nhận `lines[]` để đánh giá theo dòng không; có trả `proposedReasonCode` giới hạn trong hai mã không; có trả `evidence[]` bắt buộc không; và có trả `linesAssessed[]` không. Chừng nào bốn cái đó còn thiếu, **AG-3 là thứ lấp vào chỗ đó**, và Track 2 phải được trình bày rõ là chạy ở chế độ tư vấn.

---

*Hết tài liệu. Nguồn: A1 v1.5 §4.2 (`lossType`), §5.1 (`claimState`), §5.2 (7 mã từ chối), §6 · A2 v1.2 §4 (câu từ chối), §7.2 (thư từ chối) · A4 v1.3 §1 (7+1 loại chứng từ), §4 (trường), §5 (R1–R9), §6 (checklist 3 mức), §7 (confidence) · A5 v1.5 §6.1 (chuỗi 14 bước), §6.2 (base và làm tròn), §8.1 (`serviceCategory` 20 giá trị) · D1 v0.5 §5.8 (cổng 3 điều kiện), BR-TRACK-3/4/5, BR-KB-2/4 · D3 v2.3 §I-07, §I-08, §I-09, §I-15, §I-16, G-3, G-7 · `IDP_APIs_for_Claim_MVP_v1.0` nhóm F và H (đối chiếu hợp đồng với IDP thật trên staging, tenant 176).*
