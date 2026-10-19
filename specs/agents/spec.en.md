# Spec — Agents & system prompts

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** Project → `claude/build-05-agent-va-system-prompt`.

## Scale

6 agents · **39 tool bindings / 34 Server Actions** · system prompts pasted directly.

## The boundary that must not be crossed

> **Agents have no tool that writes a money Entity.** No tool writes the money fields of a cost line, the limit ledger, a payout order or the claim status.
> The arrow between the Agentic layer and the Deterministic layer carries the label: **"calls as a tool · never writes"**.

Agents **propose and explain**; **humans decide**; **the adjudication chain** computes.

## The system prompt must have 4 parts

1. The Entity schema, listed explicitly
2. The intent → tool mapping rules
3. When information is missing, **ask back, don't invent**
4. Begin the reply with `Action taken: …` when data has been written

## When the agent does not answer

`CLM-CONSOLE-015`: *"The clinical assessment did not complete for this claim. It has **not** been assessed as clean."*
This sentence guards against the temptation of "the agent timed out, so let it through". `AGENT_TIMEOUT` maps to `EXTRACTION_ERROR`, which the customer currently sees as `"Đang xử lý chứng từ"` (EN: "Processing documents").

## Eight Agent Workbench assumptions the SA needs to confirm

`build-05` §9. The three most important:
- Can an agent call another agent, or must the Orchestrator be a Server Action / BPT?
- Must tool input be flat?
- Is the 120 s ceiling enough for IDP (IDP ~16 s/page, flow ~30 s)?

## The floor that must not be breached

**One agent on Agent Workbench, calling one Server Action, producing one correct number, with an `AgentRun` record.**

Status 20/09: the `AgentTrace` screen is built and reads from the Entity; **no real agent writes to it yet**. The assistant chatbox in the console and the portal is currently a stub.
