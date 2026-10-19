# Spec — Agent & system prompt

> **Văn bản đầy đủ:** Project → `claude/build-05-agent-va-system-prompt`.

## Quy mô

6 agent · **39 tool binding / 34 Server Action** · system prompt dán thẳng.

## Ranh giới không được vượt

> **Agent không có tool ghi Entity tiền.** Không tool nào ghi trường tiền của dòng chi phí, sổ hạn mức, lệnh chi trả hay trạng thái hồ sơ.
> Mũi tên giữa tầng Agentic và tầng Deterministic mang nhãn: **"calls as a tool · never writes"**.

Agent **đề xuất và giải thích**; **con người quyết định**; **chuỗi tính tiền**.

## System prompt phải có 4 phần

1. Schema Entity liệt kê rõ
2. Luật ánh xạ ý định → tool
3. Thiếu thông tin thì **hỏi lại, không bịa**
4. Mở đầu câu trả lời bằng `Action taken: …` khi đã ghi dữ liệu

## Khi agent không trả lời

`CLM-CONSOLE-015`: *"The clinical assessment did not complete for this claim. It has **not** been assessed as clean."*
Đây là câu chống lại cám dỗ "agent timeout thì cho qua". `AGENT_TIMEOUT` quy về `EXTRACTION_ERROR`, và với khách hiện là *Đang xử lý chứng từ*.

## Tám giả định Agent Workbench cần SA xác nhận

`build-05` §9. Ba cái quan trọng nhất:
- Agent gọi được agent khác không, hay Orchestrator phải là Server Action / BPT?
- Tool input có bắt buộc phẳng không?
- Trần 120 s có đủ cho IDP không (IDP ~16 s/trang, flow ~30 s)?

## Sàn không được thủng

**Một agent trên Agent Workbench, gọi một Server Action, ra một con số đúng, có bản ghi `AgentRun`.**

Trạng thái 20/09: màn `AgentTrace` đã dựng và đọc từ Entity; **chưa có agent thật nào ghi vào**. Chatbox trợ lý ở console và portal hiện là stub.
