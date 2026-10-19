# Mentor prompts — A5Step01…05 (soạn trước, gửi từng lượt)

> Mỗi bước một Server Action, chữ ký chung: in `Ctx` (ChainCtx) → out `Ctx`, `StepLog` Text, `Outcome` Text (PASS/FAIL/CUT/HOLD/SKIP), `ResultAmount` Decimal. Không bước nào ghi Entity. Log tiếng Việt, ngắn, đủ số (BR-WAIT-5).

## Turn B — A5Step01, 02, 03

**A5Step01_Eligibility** (A5 §6.1 #1, D1 §5.3 #1 — cả hồ sơ):
- Ba điều kiện, đánh giá tại `Ctx.TreatmentFrom` (ngày dịch vụ):
  1. member active: `CoverageFrom <= TreatmentFrom` and (`CoverageTo = NullDate()` or `TreatmentFrom <= CoverageTo`)
  2. policy in force: `PolicyActive` and `PolicyEffective <= TreatmentFrom <= PolicyExpiry`
  3. premium paid: `PremiumPaid = True`; nếu `PremiumPaidIsNull` ⇒ HOLD (BR-CHAIN-1), không PASS, không FAIL.
- Fail 1 hoặc 2 ⇒ `Ctx.ClaimDeclined = True`, `ClaimDeclineCode = "PERIOD"`, mọi dòng `Declined = True`, `DeclineCode = "PERIOD"`, `Running = 0`; Outcome FAIL. Fail 3 ⇒ decline `PERIOD`? — **không**: A1 không có mã riêng cho chưa đóng phí; dùng `PERIOD` với log ghi rõ "phí chưa ghi nhận" (D1 §5.3: computed decline). 
- PASS ⇒ log "Thành viên hiệu lực từ {CoverageFrom}, hợp đồng {PlanCode} hiệu lực {Effective}–{Expiry}, phí đã ghi nhận; ngày điều trị {TreatmentFrom}".
- ResultAmount = Σ Running.

**A5Step02_WaitingPeriod** (A5 §5, #2 — mỗi event):
- Nếu `Ctx.ConditionClassIsNull` ⇒ HOLD, `Ctx.Held = True`, Checkpoints += "Chưa xác định nhóm bệnh (condition class) — bước 2 không đánh giá được" (BR-CHAIN-1). Outcome HOLD.
- Tra `BenefitWaitingPeriod` where `PlanCode = Ctx.PlanCode` and `BenefitGroupCode = Ctx.BenefitGroupCode` and `ConditionClassId` ↔ `Ctx.ConditionClassValue` (join static). Không có dòng ⇒ `n/a` ⇒ HOLD + checkpoint "Tổ hợp {group}/{class} không có kỳ chờ trong bảng — lỗi dữ liệu (BR-WAIT-4)".
- `Start = If(ContinuousCoverFrom <> NullDate(), ContinuousCoverFrom, CoverageFrom)`; `Eligible = AddDays(Start, Days)`. Nếu `TreatmentFrom < Eligible` ⇒ FAIL: `ClaimDeclined`, `ClaimDeclineCode = "WAIT"`, mọi dòng Declined WAIT, Running 0. Log (BR-WAIT-5): "Kỳ chờ {Days} ngày cho {group}/{class}, tính từ {Start} ⇒ đủ điều kiện từ {Eligible}; ngày điều trị {TreatmentFrom} — {đủ|chưa đủ}".
- `Days = 0` là 0 ngày thật (tai nạn), không phải "không có".

**A5Step03_PreExisting** (#3 — no-op có log trên sản phẩm này):
- Nếu `ConditionClassValue = "PRE"` ⇒ log "Bệnh có sẵn: sản phẩm không loại trừ, đã áp kỳ chờ 365 ngày ở bước 2" ; else log "Không phải bệnh có sẵn". Outcome PASS (hoặc SKIP nếu không PRE). Không đổi tiền.

## Turn C — A5Step04, 05

**A5Step04_GeneralExclusions** (#4 — term engine scope PRODUCT; D1 §5.3.1):
- Đọc `TermRegister` where `IsActive` and `Scope = "PRODUCT"`. Với mỗi term: xác định dòng khớp — quy tắc khớp lấy từ register, không hard-code: term có `Code` chứa mã ServiceCategory (vd `TERM-HC-003` ↔ SUPPLEMENT) — **hỏi Mentor liệt kê 10 term và cột nào nói term áp cho category nào**; nếu register không có cột đó ⇒ đợt này bước 4 chỉ **ghi log mọi term đã kiểm ("không bắn")** và mở checkpoint cho term class J/E có dòng khớp theo Category (SUPPLEMENT ↔ TERM-HC-003).
- `ClauseRef` bắt đầu `TBC-` ⇒ không bao giờ decline (BR-TERM-10) ⇒ checkpoint thay vì `EXCL`. Chỉ khi clause thật + class D + effect DECLINE_LINE ⇒ dòng Declined `EXCL`, Running 0, Cut(StepNo 4, CauseCode "EXCL", Amount).
- Log liệt kê từng term: "{Code} {Name} — {không khớp|checkpoint|EXCL dòng n}". Outcome PASS nếu không cắt, CUT nếu có EXCL, HOLD nếu mở checkpoint (Ctx.Held).

**A5Step05_BenefitMapping** (#5 — không từ chối gì):
- Với mỗi dòng: tra `ServiceCategory` where `Value = Category` → `BenefitCode = If(TreatmentType = Inpatient, InpatientBenefit, OutpatientBenefit)`. TreatmentType: đọc từ `LossTypeValue`/`Claim.TreatmentTypeId` — Ctx cần thêm `TreatmentTypeValue` (thêm vào LoadContext nếu thiếu).
- Nếu mã tra được ≠ `BenefitCode` đã seed ⇒ ghi log lệch, **dùng mã tra được** (bảng là nguồn), Ctx.Lines.BenefitCode = mã tra; nếu tra ra rỗng ⇒ `Held` dòng, HoldReason "UNMAPPED — không có quyền lợi cho {Category}/{treatmentType}", Ctx.Held = True, checkpoint. Cập nhật Parent/Root từ snapshot.
- Log: "6 dòng → 200.1 ×3, 200.2 ×3" kiểu đếm theo mã. Outcome PASS / HOLD.
