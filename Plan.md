Dentaire App - Full Project Structure Plan (Revised)

Goals





Build a clean Avalonia desktop app with strict 3-tier architecture.



Keep shared app models in Business so both UI and DataAccess use the same model types.



Avoid UnitOfWork and avoid DTO indirection in core app flows.



Support DB provider switching (SQLite/SQL Server) through configuration and DI only.

Solution Layout





DentaireApp.sln



src/DentaireApp.UI.Avalonia (Avalonia UI, MVVM Toolkit)



src/DentaireApp.Business (shared models, repository interfaces, business services/rules)



src/DentaireApp.DataAccess.EFCore (EF Core context + repository implementations + provider setup)



src/DentaireApp.DataAccess.Migrations (migration runner/design-time factory)



src/DentaireApp.Infrastructure (cross-cutting concerns)



src/DentaireApp.Bootstrap (DI/composition root and options binding)



tests/DentaireApp.Business.Tests



tests/DentaireApp.DataAccess.Tests



tests/DentaireApp.UI.Tests (optional)

Detailed Project Structure





src/DentaireApp.UI.Avalonia





App.axaml, Program.cs



Views/ (PatientQueueView, PatientRecordView, OdontogramView, BillingView)



ViewModels/ (ShellViewModel, feature view models)



Navigation/



Controls/ (reusable odontogram and list controls)



Converters/



Resources/Styles/, Resources/Themes/



Assets/



src/DentaireApp.Business





Models/





Patients/ (Patient, MedicalNote)



Appointments/ (Appointment, AppointmentStatus)



Odontogram/ (ToothState, ToothSurfaceOperation, TreatmentRecord)



Billing/ (Invoice, InvoiceLine, Payment)



Contracts/Repositories/





IPatientRepository



IAppointmentRepository



ITreatmentRepository



IInvoiceRepository



Contracts/Services/



Services/ (business workflows)



Validation/ (business rules only)



Common/ (enums, constants, result types)



src/DentaireApp.DataAccess.EFCore





Persistence/AppDbContext



Persistence/Configurations/ (entity mappings for Business models)



Repositories/ (implements Business repository interfaces)



Providers/Sqlite/ and Providers/SqlServer/ (provider-specific setup)



Seed/ (optional)



src/DentaireApp.DataAccess.Migrations





DesignTime/ (IDesignTimeDbContextFactory)



migrations and snapshot



src/DentaireApp.Bootstrap





DependencyInjection/ (AddBusinessServices, AddDataAccess, provider registrations)



Options/DatabaseOptions (Provider, ConnectionString)



src/DentaireApp.Infrastructure





Configuration/, Logging/, Clock/ (optional), shared technical helpers

Layer Responsibilities





UI (DentaireApp.UI.Avalonia)





Handles views/viewmodels, user interactions, and navigation.



Uses only Business services/models; no EF Core or SQL references.



Business (DentaireApp.Business)





Owns shared models used by the whole app.



Owns repository contracts and business services.



Contains rules/validation for dental workflows.



DataAccess (DentaireApp.DataAccess.EFCore)





Implements Business repository contracts.



Maps Business models to persistence schema.



Contains provider-specific EF registration only.

UI Design System (Dedicated Section)





This section is the baseline for all future UI/UX discussions and decisions.



Objective: keep screens visually consistent, calm, and clinic-friendly across all modules.

Core Color Palette (Confirmed)





PrimaryBlue: #2E4682



PrimaryGreen: #7A9E7E



SurfaceFaintGreen: #E7EDE3



NeutralGray: #D9D9D9

Planned UI Tokenization





Add semantic tokens (mapped to the core palette) in UI resources:





Color.Background.App



Color.Background.Card



Color.Border.Default



Color.Text.Primary



Color.Text.Secondary



Color.Action.Primary



Color.Action.PrimaryHover



Color.Accent.Success



Keep raw hex values centralized in one Avalonia theme resource file, then consume semantic tokens in views/styles.

UI Scope To Define Before Implementation





Typography system (font family, sizes, heading/body scales).



Spacing scale (4/8/12/16/24...) and layout grid rules.



Component states (default/hover/pressed/disabled/focus).



Reusable components:





queue row cards



odontogram tooth states



form fields



primary/secondary buttons



tables and payment rows



Accessibility checks (contrast, readable text size, keyboard focus visibility).

Provider Switching (No Code Changes in Features)





appsettings stores:





DatabaseOptions.Provider (Sqlite initially, SqlServer later)



DatabaseOptions.ConnectionString



Bootstrap reads options and registers one provider branch.



Repositories and business services remain unchanged when provider changes.



Migration flow is centralized in DataAccess.Migrations.



Current decision: start with SQLite as the active provider.

Dependency Rules





UI -> Business



DataAccess.EFCore -> Business



Bootstrap -> UI + Business + DataAccess.EFCore + Infrastructure (composition only)



Business -> Infrastructure (optional lightweight abstractions only)



No reverse references.

Dependency Diagram

flowchart LR
  UI[DentaireApp.UI.Avalonia] --> Business[DentaireApp.Business]
  DataEF[DentaireApp.DataAccess.EFCore] --> Business
  Bootstrap[DentaireApp.Bootstrap] --> UI
  Bootstrap --> Business
  Bootstrap --> DataEF
  Business --> Infra[DentaireApp.Infrastructure]
  DataEF --> Infra
  Migrations[DentaireApp.DataAccess.Migrations] --> DataEF

Initial Milestones (Before Feature Details)





Create solution + all projects with references following dependency rules.



Add Business shared models and repository interfaces.



Add EF Core DbContext + repository skeletons implementing Business contracts.



Add provider selection through DatabaseOptions (default Sqlite).



Wire DI in Bootstrap.



Confirm app startup with SQLite provider.



Deliver first vertical slice: create/list patient through UI -> Business -> Data.

Discussion Checklist Before Implementation





First provider confirmed: SQLite.



Confirm naming conventions for models and repository methods.



Confirm module delivery order: Queue -> Patient Record -> Odontogram -> Billing.



Confirm migration/seed strategy.



Confirm UI design decisions:





typography scale



spacing/layout rules



component library priorities



color usage per screen type

Queue Prediction Design (Main Page)

Goal





Predict, for each queued patient, a time interval when they should be present at clinic (example: 16:00 - 16:30).

Confirmed Prediction Decisions





Prediction output: interval window, not only a single timestamp.



Average basis: global average from all historical completed visits.



Interval method: configurable fixed window (maintainable setting).



Doctor arrival strategy: planned start first, then auto-recalculate when doctor checks in.



Before doctor check-in, patient-facing status: pending doctor arrival (no exact interval).

Configurable Variables (Business Options)





AverageConsultationMinutes (derived from historical data, periodically recomputed).



IntervalPredictionMinutes (manual config, default 30).



PlannedSessionStart (configured daily schedule baseline).



DoctorCheckInTime (actual arrival trigger from reception/doctor action).



PredictionAnchorMode





PlannedBeforeCheckIn then ActualAfterCheckIn.

Core Algorithm (v1)





Before doctor checks in:





Do not publish exact patient intervals.



UI shows status: Pending doctor arrival.



After doctor checks in:





QueueStartReference = DoctorCheckInTime.



For queue item at index i (0-based among waiting patients):





ExpectedStart = QueueStartReference + (i * AverageConsultationMinutes)



IntervalStart = ExpectedStart



IntervalEnd = ExpectedStart + IntervalPredictionMinutes



Output shown to UI as formatted interval (example: 16:00 - 16:30).



If planned schedule is shown internally before check-in, mark it as non-patient-facing draft only.

Business Service Contract (Planned)





Add service in Business.Contracts.Services:





IQueuePredictionService



Responsibilities:





Compute global average duration from historical visits.



Produce interval predictions for current queue.



Apply config options (IntervalPredictionMinutes).



Recompute all predictions when DoctorCheckInTime is set/updated.



Return prediction state (PendingDoctorArrival or Ready).

Data Requirements





Historical completed visit records with:





StartedAt



CompletedAt



Queue data with deterministic ordering (position).

Initial Guardrails





If insufficient history:





fallback to configurable default average (example: AverageConsultationMinutesFallback = 20).



Clamp invalid values:





IntervalPredictionMinutes must be > 0 and within a safe upper limit.



Timezone/clock source should come from app clock abstraction (from Infrastructure).

UI Behavior on Main Queue Page





Each queue row displays:





patient name



predicted interval (HH:mm - HH:mm) when prediction state is Ready



If doctor has not checked in:





show queue rows with Pending doctor arrival badge/message



hide or disable exact interval labels



Optional future enhancement:





confidence indicator or color cue when average is based on low history volume.

Main Page UI/Business Logic Alignment (Based on Latest UI)





Search box behavior





Live filter queue by patient name and/or queue number.



Search filtering does not change true queue order; it only changes visible subset.



When a patient is found from search:





apply a visible stroke highlight around the row (as in the latest mockup),



set it as active selection,



auto-scroll to place it near the middle of the list viewport when possible.



If multiple matches exist:





focus first match by default,



allow moving through matches with navigation actions.



**Nouveau Numero action**





Opens a modal dialog to collect required patient info before creating ticket.



Dialog fields:





Nom (required)



Age (required, numeric range validation)



Adresse (required)



Telephone (required, format validation)



On confirm:





create patient if new (or reuse existing patient by matching policy to be finalized),



create queue ticket with next sequence number,



set status Waiting,



trigger prediction refresh cycle.



On cancel:





close dialog with no queue/patient changes.



Selected queue row





One active row at a time (highlighted in blue).



Selected row drives detail panel/navigation target for next screens.



Search-focused row can keep a secondary stroke state while the active row style remains primary.



**Precedent / Suivant navigation**





Suivant: moves workflow to next waiting patient (or next page slice if pagination mode is active).



Precedent: returns to previous item/page context.



Buttons enabled/disabled based on available neighbors.



In search mode, navigation first moves across matches, then falls back to normal queue order.



Business contracts to support this UI





IQueueService.GetQueue(date, doctorId, page, pageSize, searchTerm)



IQueueService.CreateTicketForPatient(patientId)



IPatientRegistrationService.RegisterOrResolvePatient(nom, age, adresse, telephone)



IQueueService.SetActiveTicket(ticketId)



IQueueService.MoveNext() / IQueueService.MovePrevious()



IQueuePredictionService.GetPredictions(queueSnapshot)



Row-centering/scroll logic stays in UI (ViewModel + view interaction), not in Business.



Queue row DTO alternative avoided





Keep using Business shared models; UI projection is done in ViewModel only (no extra DTO layer).

Patient Record Page (Second Page) - Interaction and Data Model

Navigation Trigger (From Queue Page)





Right-click on a patient row in queue opens a context menu with:





Traiter patient



Dossier patient



Selecting Dossier patient navigates to the patient second page (the form/table page from your mockup).



Selecting Traiter patient starts treatment workflow for that queue ticket (status transition to InProgress, then route to treatment workspace/page to be finalized).



The clicked patient becomes the active context for data loading/saving.

Page Purpose





Display and edit patient identity header:





Nom



Age



Telephone



Adresse



Display a treatment/billing table section with columns:





Dates



Natures des Operations



Prix Conven



Recu



A Recevoir



Provide actions:





Enregistrer (save current edits)



Annuler (revert unsaved edits)



+ (add another table instance/sheet for the same patient)

Financial Consistency Rule (Confirmed)





For each treatment line, enforce:





Prix Conven = Recu + A Recevoir



Validation is defined in Business layer (not only UI) to guarantee integrity regardless of entry point.



Save is blocked when equality is not satisfied.



UI behavior on invalid line:





highlight invalid cells/row,



show clear message indicating expected equation.

Data Relationship (Confirmed Requirement)





One patient can have many treatment sheets/tables over time.



Proposed model relation in Business layer:





Patient (1) -> (N) TreatmentSheet



TreatmentSheet (1) -> (N) TreatmentLine



This keeps your requirement explicit: each click on + creates a new TreatmentSheet under the same patient.

Business Contracts (Planned)





IPatientRecordService.GetPatientRecord(patientId)



IPatientRecordService.GetTreatmentSheets(patientId)



IPatientRecordService.CreateTreatmentSheet(patientId)



IPatientRecordService.SaveTreatmentSheet(sheet)



IPatientRecordService.CancelChanges(sheetId) (or ViewModel-level rollback for draft state)

UI/State Behavior





On open:





load patient header + latest sheet by default.



On + click:





create a new empty sheet,



append to patient sheet list,



switch UI focus to the new sheet.



Save flow:





validate required fields and numeric money fields before commit.



validate financial rule Prix Conven = Recu + A Recevoir on each row.



Cancel flow:





restore last persisted state for current sheet.

Persistence Notes (SQLite First)





Suggested tables:





Patients



TreatmentSheets (FK: PatientId)



TreatmentLines (FK: TreatmentSheetId)



This schema supports unlimited historical sheets per patient without duplicating patient identity data.

