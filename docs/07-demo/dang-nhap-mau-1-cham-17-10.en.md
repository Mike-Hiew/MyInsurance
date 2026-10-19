# One-tap sample login — ClaimMVP rev 810 · MyInsurance rev 254 (17/10)

> English translation for reviewers. Vietnamese original: [dang-nhap-mau-1-cham-17-10.md](dang-nhap-mau-1-cham-17-10.md).

**Mike's request:** drop the role cards that only pre-fill the username and switch to one tap to sign in, for both apps and all roles. The goal is for Claude to log in and test on its own, without needing Mike.

## What was done
| App | Card (RoleKey) | Username / password settings | Screen after login |
|---|---|---|---|
| ClaimMVP | officer | SampleUser_Officer / SamplePassword_Officer | WorkList |
| ClaimMVP | approver | SampleUser_Approver / SamplePassword_Approver | ApprovalQueue |
| ClaimMVP | ops | SampleUser_Ops / SamplePassword_Ops | OpsQueue |
| ClaimMVP | presenter | SampleUser_Presenter / SamplePassword_Presenter | AgentTrace |
| MyInsurance | Customer | SampleUser_Customer / SamplePassword_Customer | PortalHome |

- `SamplePassword_*` is a secret setting (Secret). `EnableSampleLogin` defaults to False; when it is off, the cards are hidden and the server action returns nothing.
- Each sample login writes one `SampleLogin` log line containing the RoleKey, the username and the result. The log does not contain the password.
- On a successful login: `IsSampleAccount` = True, an orange label "Sample account · demo" appears in the bottom-left corner (fixed in rev 809), then the user is taken to the role's screen.
- Rev 809–810: each card shows the role name in the currently selected language, with a hint line "One tap · demo / 1 chạm · demo" under the name. The note line under the cards was changed to "One tap signs you in with a demo account". The literally displayed "\n" string and the run-together two-language text were removed. Layout of each card: icon on the left, role name and hint stacked vertically on the right.

## Test results (17/10, 09:05–09:25, Mike had filled in the settings and turned on the flag)
| Card | Account | Screen after login |
|---|---|---|
| ClaimMVP · Claim officer | mike | WorkList ✓ |
| ClaimMVP · Approver | Mai Trung Hieu | ApprovalQueue ✓ |
| ClaimMVP · Operations | Mai Trung Hieu | OpsQueue ✓ |
| ClaimMVP · Presenter | Mai Trung Hieu | AgentTrace ✓ |
| MyInsurance · Customer ("Khách hàng") | Mai Trung Hieu | PortalHome ✓ ("Chào An" (EN: "Hello An")) |

- ChainTests rev 808: 46/46, gate 5/5 (run at 09:06).
- **Note:** Approver, Ops and Presenter currently point to the same account (Mike's account). To test real authorization, including the four-eyes check (the assessor and the approver must be different people), each role needs its own account.
- **Bug found:** the Sign out button (`LogoutBtn`) does sign out, but does not automatically redirect to the Login page; the user has to reopen the page manually.
- Chrome shares the login session across all tabs. When Claude logs in with a sample card, the tabs Mike has open also switch to that account. After testing, Claude logged back in using the Approver card (Mike's account).

## Security risk
- On ODC, the system Login action runs in the browser (client action). Therefore the server action (ClaimMVP: `GetSamplePassword`, MyInsurance: `GetSampleCredentials`) has to return the sample password to the browser, and the Login screen allows anonymous access.
- Consequence: while the flag is on, anyone who can open the Login page can obtain the passwords of the sample accounts.
- Mitigation:
  - Use dedicated sample accounts only, never personal accounts.
  - Use only on the Development environment.
  - Turn the flag on only while testing or demoing.

## Notes
- MyInsurance: the Member table has no UserId column, so every customer account sees the same demo member (the member with the smallest Id).
