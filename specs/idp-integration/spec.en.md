# Spec — BnK IDP integration

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** Project → `claude/build-02-tich-hop-idp`.
> Upstream sources on disk: `../../Preparing/03_BnK-IDP/` and `../../insurance BNK/IDP_API_Spec/`.

## Fixed configuration

| Item | Value |
|---|---|
| Tenant | **176** (`"Phòng 1"`, EN: "Room 1") · user 203 · partner 40 |
| Base URL | `https://idp-api.idpstg.bnksolution.com` |
| Document set | `HEALTH_CLAIM_V1` — **set 194**, do NOT use 193 |
| Flow id | **`5ed8e4fc`** — pinned in a Site Setting |
| Endpoints | 32 total: 14 P0 · 14 P1 · 2 broken · 2 forbidden |

## Six quirks not to forget

1. **A browser-spoofing `User-Agent`** on **every** method — without it Cloudflare returns 403 1010. Set it before testing the second method.
2. **`templates` is passed as a comma-separated string**, not an array.
3. **HTTP 200 can still be an error** — always read `errorCode` in the body. `raw-file` returns a zip.
4. **`autoStart` does NOT work** — you must call `run-workflow-request`, and this endpoint returns 400 sometimes and 200 other times with the same parameters ⇒ a retry exception is needed for `4xx`.
5. **`PUT /request/{id}/result` works** ⇒ IDP results are not immutable ⇒ **this endpoint is forbidden**, and copy `RawExtractionJson` yourself on the very first read.
6. **`extraParams` is a closed type and in practice returns `{}`** ⇒ correlate using the **`name` = `ClaimNo`** field.

## Two blockers, no answer yet from the IDP team

- `Q-IDP-01` — **the document-class crosswalk is empty**; `classification` currently runs on an assumed mapping. **BLOCKS the classification part.**
- **`T1-03` (examination slip) has no template yet on tenant 176**, even though it is `REQUIRED` on 4 of 8 `lossType`s.

## A 200 + `{}` is an error

Flow `8d873709` returns `{}` 0/4 with HTTP 200. Never read it as "no warnings".
