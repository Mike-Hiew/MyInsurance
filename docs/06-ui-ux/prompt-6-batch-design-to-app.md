# 6 prompt paste thẳng cho `outsystems-design-to-app`

Nguồn: `Prototype_Insurance_BnK_v0.5.html` đã cắt bằng `cut_prototype.py`.
Mỗi batch là **một file HTML chạy được độc lập** — mở bằng trình duyệt thấy đúng màn hình cần build,
mọi màn ngoài phạm vi đã thay bằng stub ghi rõ *"not in this batch. Do NOT build this screen."*

Thay `<app>` bằng tên app thật trên tenant. Thay `E:\Hackaton\batch\` bằng đường dẫn thật.

---

## Khối luật chung — dán kèm MỌI batch

```
Rules — do not deviate:
- Bind to EXISTING entities (Claim, ClaimLine, ClaimStatus, Assessment, Recommendation,
  RecEvidence, CalcSnapshot, TermRegister). Do NOT create new entities.
- Any screen rendered by a stub that says "not in this batch" must NOT be built.
- Block mapping (do not fall back to Container + custom CSS):
    sidebar + mini/expand  -> LayoutSideMenu + Menu + ILink      (NOT LayoutBlank + flex)
    .sortable table        -> TableRecords + Aggregate sort/paging (NOT JS sort)
    .badge-status / .conf  -> Tag
    .card.kpi              -> Card + Counter
    .tab strip             -> Tabs + TabsHeaderItem + TabsContentItem
    .note                  -> Alert (inline)    #toast -> Notification
    .avatar initials       -> UserAvatar        ic() SVG -> IIcon (Phosphor)
    .nav-badge             -> IconBadge         charts -> DonutChart/BarChart/LineChart
    #modal                 -> Sidebar panel or OS UI modal
- Theme: override OS UI variables at :root on the THEME stylesheet, never declare custom
  CSS vars on a screen stylesheet:
    --primary->--color-primary   --ok->--color-success   --warn->--color-warning
    --danger->--color-error      --bg->--color-background-body
    --surface->--color-neutral-0 --surface-2->--color-neutral-1
    --text->--color-text-primary --border->--color-neutral-3
- Currency: call Server Action FormatVND. Never toLocaleString, never raw Format.
- Declare the role for every screen explicitly; do not leave "Accessible by: Everyone".
```

---

## Batch 1 — App shell + Work list 🔴

```
Use outsystems-design-to-app to build the app shell and the Work list screen in <app>.
Design source: E:\Hackaton\batch\batch1_shell_worklist.html
Screens to build: login screen, app shell (sidebar + topbar), Work list (F42).
<khối luật chung>
```

## Batch 2 — Claim detail, tab 1–4 🔴 *(màn được chấm)*

```
Use outsystems-design-to-app to build the Claim detail screen in <app>, tabs 1-4 only.
Design source: E:\Hackaton\batch\batch2_detail_tab1_4.html
Screens to build: Claim detail header + progress line + todo bar, with tabs
  General (F45), Documents (F44), Extraction & costs (F46), Events & cover (F47).
The costs tab carries the AI recommendation block with evidence chips — build it as
  a repeating Card list inside the tab, each evidence item a Tag. Confidence pill -> Tag.
Tabs 5-8 are stubs in this file: render the TabsContentItem empty, do not invent content.
<khối luật chung>
```

## Batch 3 — Claim detail, tab 5–8 + panel trợ lý 🔴

```
Use outsystems-design-to-app to extend the Claim detail screen in <app> with tabs 5-8
and the right-hand assistant panel.
Design source: E:\Hackaton\batch\batch3_detail_tab5_8_panel.html
Screens to build: tabs Calculation & ledger (F48), Settlement plan (F49),
  Approval & payment (F50), History & log (F53), plus the assistant side panel (F56).
The 14-step calculation table is DISPLAY ONLY — bind every number to Server Action
  GetCalcSnapshot. Do not compute anything in the screen, no expressions doing math.
Approval/payment buttons call existing Server Actions; do not generate business logic.
<khối luật chung>
```

## Batch 4 — Dashboard + Operations queue 🟡

```
Use outsystems-design-to-app to build the Dashboard and Operations queue screens in <app>.
Design source: E:\Hackaton\batch\batch4_dashboard_opqueue.html
Screens to build: Dashboard (F54), Operations queue (F53).
KPI cards -> Card + Counter. Charts -> OutSystems UI charts bound to aggregates.
<khối luật chung>
```

## Batch 5 — Policy / Terms / Checkpoints / Ledger / Admin 🟡

```
Use outsystems-design-to-app to build the reference screens in <app>.
Design source: E:\Hackaton\batch\batch5_policy_terms_ledger.html
Screens to build: Policies & benefits (F60), Term register (F65),
  Checkpoint queue (F67), Limit ledger (F48), Administration (F55).
Term register binds to TermRegister (10 rows already seeded). All five are TableRecords.
<khối luật chung>
```

## Batch 6 — Portal khách hàng 🟡

```
Use outsystems-design-to-app to build the customer portal screens in <app>_Portal.
Design source: E:\Hackaton\batch\batch6_portal_customer.html
Screens to build: My dashboard (S07), Benefits (S08), New claim (S09), My claims (S11),
  Claim detail, Supplement request (S12), Notifications (S14).
Different theme from the console: light hero, no dark sidebar. Customer role only.
<khối luật chung>
```

---

## Không build

`agents · trace · kpi · pitch · osmap` — đây là **slide trình bày**, không phải app.
Ngoại lệ duy nhất: `trace` (nhật ký AgentRun) là dữ liệu thật → build sau, đọc từ Entity `AgentRun`.
Khung điện thoại `mobileApp()` cũng bỏ — Reactive Web responsive lo, mobile native để sau.

## Dùng lại cho v6.1

```
python3 cut_prototype.py E:\Hackaton\Prototype_MVP1_Console_v6.1.html E:\Hackaton\batch
```

Script tự tách top-level symbol nên không phụ thuộc số dòng. v6.1 có thêm presenter mode
(`setDemo`, `TECHV`) và đổi mặc định ngôn ngữ — nếu có symbol mới, thêm tên vào `BATCHES`
hoặc `COMMON` trong đầu file. Symbol bị thiếu sẽ tự thành stub, không vỡ trang.
