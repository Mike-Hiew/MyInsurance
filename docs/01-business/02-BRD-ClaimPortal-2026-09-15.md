# My Insurance Portal - Requirements Document

Version: 1.0

Date: September 15, 2026

Author: BnK Solution - Claim Platform team

For: Mentor App Generator (ODC)

Research basis: OutSystems Mentor documentation, synced 2026-09-15

### 1. Application Overview

This document describes **My Insurance**, the customer-facing web application through which a policyholder submits a health-insurance reimbursement claim and follows it to a result. The member photographs their medical documents, submits them, watches the claim progress through a small number of plain-language statuses, and finally reads an itemised result that states how much the insurer pays, how much the member bears, and the reason for each amount they bear.

The application is responsive and is used on a phone as often as on a desktop. It does not define the claim data model. Every entity it reads is owned by the Claim Console application and referenced from there, so a member and an officer always see the same figures.

### 2. General Application Settings

The application must use the **"Mentor"** theme available in the ODC tenant.

The interface language is Vietnamese throughout. The layout must work on a phone screen.

### 3. Data Model

#### Referenced Entities

The following entities are **referenced from the Claim Console application**. This application does not create them. They must be exposed as public in Claim Console before this application is generated.

* **Entity: Claim**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include:
    - Id: An Identifier that is the Primary Key from Claim Console.
    - ClaimNo: Text, shown to the member as their claim reference.
    - MemberId: An Identifier that is a Foreign Key to the Member entity.
    - PolicyId: An Identifier that is a Foreign Key to the Policy entity.
    - StatusId: An Identifier that is a Foreign Key to the ClaimStatus static entity.
    - TreatmentTypeId: An Identifier that is a Foreign Key to the TreatmentType static entity.
    - EventDate: Date, the date the illness or injury occurred.
    - AdmissionDate: Date, the date treatment began.
    - DischargeDate: Date, the date treatment ended.
    - FacilityName: Text, the treating hospital or clinic.
    - TotalClaimed: Currency, the total amount requested.
    - TotalPayable: Currency, the total the insurer pays.
    - CreatedOn: DateTime, the timestamp the claim was created.

* **Entity: ClaimLine**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, ClaimId, Description, ServiceDate, Quantity, AmountClaimed, AmountPayable and DecisionCodeId.

* **Entity: ClaimLineBreakdown**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, ClaimLineId, CauseCodeId and Amount. This entity supplies the reason each amount is borne by the member.

* **Entity: ClaimDocument**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, ClaimId, FileName, DocumentTypeId and UploadedOn.

* **Entity: ClaimEvent**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, ClaimId, EventTypeId, Detail, IsCustomerVisible and OccurredOn. Only rows where IsCustomerVisible is true are shown.

* **Entity: ClaimLetter**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, ClaimId, LetterTypeId, SentText and SentOn. Only letters that have been sent are shown.

* **Entity: Policy**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, PolicyNo, PolicyCode, PolicyHolderName, EffectiveDate, ExpiryDate and StatusId.

* **Entity: Member**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, MemberNo, FullName, DateOfBirth, RelationshipToHolder and PolicyId.

* **Entity: PolicyBenefitSnapshot**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, PolicyId, BenefitCode, BenefitName, SumInsured, AnnualLimit, PerVisitCap, CoinsurancePct and WaitingPeriodDays.

* **Entity: LimitLedger**
  + This entity's data is sourced from the **Claim Console** application.
  + Attributes used here include Id, PolicyId, MemberId, BenefitCode, PeriodYear, UsedAmount and RemainingAmount.

The static entities ClaimStatus, TreatmentType, DecisionCode, DocumentType, ExclusionCause, ClaimEventType and LetterType are also referenced from Claim Console.

#### Locally Stored Entities

* **Entity: MemberAccount**
  + This entity is stored **locally**.
  + It links a signed-in portal user to the member record they may view.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - UserId: User Identifier, the signed-in portal user.
    - MemberId: An Identifier that is a Foreign Key to the referenced Member entity.
    - IsPrimaryHolder: Boolean, whether this user is the policy holder.
    - RegisteredOn: DateTime, the timestamp the account was linked.

* **Entity: MemberNotification**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - MemberAccountId: An Identifier that is a Foreign Key to the MemberAccount entity.
    - ClaimId: An Identifier that is a Foreign Key to the referenced Claim entity.
    - Title: Text, the notification headline.
    - Body: Text, the notification detail.
    - IsRead: Boolean, whether the member has opened it.
    - CreatedOn: DateTime, the timestamp the notification was raised.

* **Entity: PayoutAccount**
  + This entity is stored **locally**.
  + Attributes include:
    - Id: An Identifier that serves as the Primary Key.
    - MemberAccountId: An Identifier that is a Foreign Key to the MemberAccount entity.
    - BankName: Text, the receiving bank.
    - AccountMasked: Text, the account number with all but the last four digits hidden.
    - IsDefault: Boolean, whether this account is used unless another is chosen.

#### Entity Relationships

* A **MemberAccount** can have many **MemberNotifications** (One-to-Many).
* A **MemberAccount** can have many **PayoutAccounts** (One-to-Many).
* A **MemberAccount** is linked to one **Member** (One-to-One).
* A **Claim** can have many **ClaimLines** (One-to-Many).
* A **ClaimLine** can have many **ClaimLineBreakdowns** (One-to-Many).

### 4. Roles & Permissions

* **Role: Member**
  + Claim: Edit Access (own records only, limited to claims whose MemberId matches a MemberAccount row belonging to the signed-in user, and only while the claim is in Draft or Pending Documents status).
  + ClaimDocument: Edit Access (own records only, based on the same MemberAccount relationship, and only while the claim is in Draft or Pending Documents status).
  + ClaimLine: View Access (own records only).
  + ClaimLineBreakdown: View Access (own records only).
  + ClaimEvent: View Access (own records only, and only rows where IsCustomerVisible is true).
  + ClaimLetter: View Access (own records only, and only letters that have been sent).
  + Policy: View Access (only the policy their MemberAccount belongs to).
  + Member: View Access (only members on their own policy).
  + PolicyBenefitSnapshot: View Access (only snapshots for their own policy).
  + LimitLedger: View Access (only rows for their own policy and member).
  + MemberAccount: View Access (own record only).
  + MemberNotification: Edit Access (own records only).
  + PayoutAccount: Edit Access (own records only).
  + **Special Permission:** Can create a claim, upload documents against it, and submit it. Cannot change a claim after submission except by adding documents when the claim is in Pending Documents status. Cannot see any amount that has not been approved.

### 5. Main Features & Screens

1. **My Insurance:** The landing screen. A **card list** of the member's policies, each card showing the policy number, the product, the cover period and the covered members. Below it, a **card list** of remaining benefit limits per benefit code, and a **card list** of the three most recent claims with their status.

2. **Benefit Details:** A **table** screen listing every benefit on the policy with its name, sum insured, annual limit, per-visit cap, the percentage the member bears, and the date the benefit starts paying.

3. **Create Claim:** A form that collects, in order, the treated member, the treatment type, the event date, the treatment start and end dates, the treating facility, and the payout account. The event date cannot be later than today. The treatment end date cannot be earlier than the treatment start date. Submitting the form creates a claim in Draft status.

4. **Upload Documents:** A screen showing the documents required for the claim, grouped as required, expected and optional. The member adds a photograph or a file against each entry and sees the status of each upload. An action submits the claim, moving it to Submitted status.

5. **Processing:** A screen shown while documents are being read. It displays progress in plain language and refreshes until the claim reaches a status that has something to show. It does not mention agents or models.

6. **My Claims:** A **table** screen listing the member's claims with the claim number, the treated member, the event date, the status and the amount payable once approved. Selecting a row opens Claim Progress.

7. **Claim Progress:** A **master detail** screen showing one claim: its reference, its current status, and a timeline built from the customer-visible claim events.

8. **Claim Result:** A screen shown once a claim is approved or declined. It shows the total claimed, the total payable and the total the member bears. Below this, a **table** of cost lines, each with its description, service date, amount claimed and amount payable. Below that, a grouped list titled in Vietnamese explaining the amounts the member bears, grouped by cause: waiting period, not covered, not related, per-visit cap, annual limit and coinsurance, with the approved wording for each cause. The screen also offers the decision letter to read and shows the remaining benefit limits after this claim.

9. **Add Documents:** A screen shown when the claim is in Pending Documents status, listing what is still missing and allowing the member to upload it and resubmit.

10. **Notifications:** A **card list** of notifications raised on status changes and sent letters, with unread entries marked.

### 6. External Integrations

This application has no direct integration with any external system. Document extraction, adjudication and payment are handled by the Claim Console application and the services behind it. This application only reads the resulting data.
