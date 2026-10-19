# Prompt lượt N2 — panel read-only "Chain data" trên Administration (không publish)

Turn N2 — a read-only diagnostic panel on the Administration screen. No new entity, no server action that writes, no change to any chain step.

Add a block "Chain data" at the bottom of the Administration screen (same card style as the other blocks) with two tables:

A. "ChainRun theo hồ sơ" — aggregate ChainRun joined to Claim (ChainRun.ClaimId = Claim.Id, left join so runs whose claim no longer exists still appear), Max Records 5000, grouped by ChainRun.ClaimId (+ Claim.ClaimNo): columns ClaimId · ClaimNo (empty when no claim) · Count(ChainRun.Id) as Runs · Max(ChainRun.RunNo) as MaxRunNo. Distinct TriggeredBy is not expressible as a group aggregate; instead add a second aggregate on ChainRun grouped by ClaimId + TriggeredBy (count), and render it as a nested/adjacent table "TriggeredBy theo hồ sơ": ClaimId · TriggeredBy (show «rỗng» when empty) · Count. Sort both by ClaimId.

B. "CalcSnapshot theo ChainRunId" — aggregate CalcSnapshot grouped by ChainRunId, Max Records 5000: ChainRunId · Count(Id) as Rows. Sort by ChainRunId.

C. "CalcSnapshot mồ côi" — aggregate CalcSnapshot LEFT JOIN ChainRun on CalcSnapshot.ChainRunId = ChainRun.Id, filter  ChainRun.Id = NullIdentifier() or CalcSnapshot.ChainRunId = NullIdentifier()  (i.e. ChainRunId is 0/null or points to no ChainRun), Max Records 5000, grouped by CalcSnapshot.ChainRunId (+ CalcSnapshot.ClaimId if the entity has it): ChainRunId · ClaimId · Count. Header line above the table: "Mồ côi: {total count}" (sum of counts; 0 when none — render "Mồ côi: 0").

Above the block put one line "Tổng: {ChainRun rows} ChainRun · {CalcSnapshot rows} CalcSnapshot" from two plain count aggregates.

Everything is read-only (aggregates only, no client/server action that creates, updates or deletes). Report: the aggregates (entities, joins, filters, group-by, Max Records), the widget tree summary of the new block, validation error count. Do NOT publish.
