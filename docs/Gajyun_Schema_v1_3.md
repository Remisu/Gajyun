# Gajyun implemented schema v1.3

This document describes the current application and supersedes the implementation notes for v1.1/v1.2 in `Gajyun_Dicionario_Dados_v1_1.md`. The original SQL and Markdown remain historical references, not scripts to run against the application database. The embedded, searchable `Dictionary.v1.3.json` describes all 27 implemented business tables and `vwReservationFinancialSummary` without requiring database connectivity.

## Migration baseline and local reset

- Active baseline: `20261106083537_InitialSchemaV13`.
- SQL Server: `(localdb)\MSSQLLocalDB`; database: `GuesthouseGajyunDb`.
- Startup checks connectivity, applies pending EF migrations and seeds rooms, beds, booking statuses and aliases. It never calls `EnsureCreated`, deletes an existing database or recreates one automatically on failure.
- On 2026-11-06, staff explicitly approved deleting the old guesthouse database and replacing its migration baseline. A COPY_ONLY/CHECKSUM backup was verified with RESTORE VERIFYONLY before dropping only that database.
- Backup: `%LOCALAPPDATA%\Gajyun\Backups\GuesthouseGajyunDb_before_v13_20261106_2a982e0028cf4f4f955e3331da658fb2.bak`. This contains guest data: keep it private, outside source control, with an appropriate retention policy.
- The rebuilt database starts with no reservations, customers or import batches. Historical reservations were not automatically reimported. Staff must choose the CSV and resolve room questions in the application; the updated name rule resolves customers automatically.
- The superseded migrations were archived outside the compiled Data project in `session-state\files\MigrationArchive20261106`. This baseline is not an in-place upgrade for another populated v1.2 database; do not apply it there.

## Model changes

- `StayMemo` replaces `ReservationMemo`; its owner is `StayID`. Staff notes follow an operational stay without merging the original bookings.
- `Reservation.SourcePaymentMethodID` references `PaymentMethod`. It describes the source-designated method and does not establish that money was received.
- `Reservation.BookedAt` has an index. Review and insertion follow booking time across all OTAs, with source row number breaking ties. SQL retrieval still requires explicit `ORDER BY BookedAt`; table storage has no guaranteed order.
- `ReservationGroup.LogoUrl` stores the supplied combined logo. No artificial multi-OTA booking platform is created.
- `OTARoomMapping` uses OTA-scoped normalized names and generic aliases; `ExternalRoomCode` was removed. Original source room codes remain in audited CSV content only.
- Actual money, billable charges, OTA statements and tax snapshots remain separate. The financial view excludes unclassified OTA statement figures.

## Seeded catalogs and bundled images

The migration seeds lookup catalogs; runtime room/bed/alias seeding remains idempotent.

- Five physical rooms with capacities 4, 2, 4, 2 and 6: total 18 people.
- Four Jyun beds, four Tomo beds and six Miyabi beds: total 14. Original Japanese labels are preserved. Show and Anne have no individually managed beds and still support whole-room assignments.
- 250 country/territory entries use the supplied two-letter flag PNGs. XK is a user-assigned Kosovo code, not an official ISO assignment. The four UK subdivision flags are bundled assets, not separate country rows.
- Booking.com, Agoda, Hostelworld and direct sale have stable local WPF resource logo URLs. The combined logo belongs to booking groups.
- Document types: Passport, Driver's license, Foreign residence card, Other.
- Payment methods: Cash, Credit Card, Bank Transfer, Other, Internet payment.
- Images are packaged with the desktop application and resolve without Internet access. Source PNGs remain in `docs`; no binaries or guest CSV data are stored as SQL image blobs.

## Approved Kyoto accommodation-tax policies

Official source, checked and staff-approved on 2026-11-06: <https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html>.

Rates apply to accommodation-only price per person/night. Meals and consumption tax are excluded. Lower bounds are inclusive; upper bounds are exclusive.

| Policy | Taxable price, JPY/person/night | Tax, JPY/person/night |
|---|---:|---:|
| 2025/pre-revision | 0 to below 20,000 | 200 |
| 2025/pre-revision | 20,000 to below 50,000 | 500 |
| 2025/pre-revision | 50,000 and above | 1,000 |
| 2026 revision | 0 to below 6,000 | 200 |
| 2026 revision | 6,000 to below 20,000 | 400 |
| 2026 revision | 20,000 to below 50,000 | 1,000 |
| 2026 revision | 50,000 to below 100,000 | 4,000 |
| 2026 revision | 100,000 and above | 10,000 |

The pre-revision policy is valid from 2018-10-01 until 2026-03-01 exclusive. The revised policy begins 2026-03-01. Settings shows each room alongside a policy ComboBox and Save action. Changes go through the settings service and never rewrite historical snapshots. The legal policy for each service date remains distinct from the room's selected internal policy.

Breakfast-inclusive CSV room totals do not establish the accommodation-only taxable base. Imported tax snapshots therefore remain `PendingReview`, with legal/applied policy IDs retained separately and no invented calculated tax amounts or additional tax charges.

## Friendly reviewed CSV workflow

1. Choose CSV: the application reads and prepares a review automatically.
2. Review New bookings, Changed bookings, Previously active cancellations and Questions/held records. In Questions/held, select a booking to see the existing database reservation and incoming CSV values side by side, with differences highlighted, before reading the hold reason or answering the room question. A new booking explicitly shows that no existing reservation exists. Held/unverified sources retain their raw dates, room names and stated totals; unverified prices are not presented as validated charges.
3. Resolve room questions, acknowledge the batch and import. Customer identity questions are no longer required. Changing the file, encoding or any room decision clears approval. Advanced encoding/raw preview is collapsed by default.

Only the supplied combined Japanese export is mapped.

New active bookings create nightly room-total charges, unclassified OTA statements, customer links, room assignments, pending tax snapshots and an operational Stay/StayReservation. New cancelled bookings create the original reservation and its audit/customer associations, but no invented financial lines or stay. Nightly room prices are never multiplied by guest count. Deposit text remains an unclassified statement, not commission or receipt.

The staff-approved rule designates `Internet payment` when the details contain a supported monetary price. This populates `SourcePaymentMethodID`, not `ReservationPayment`. Actual payer, receipt amount and payment time require evidence of a real money movement; a quoted booking price or deposit does not supply that evidence.

Changed bookings display source differences and require batch approval. Safe source-owned updates preserve payments, manual extras, actual check-in, GroupID and staff notes. Previously-active cancellations preserve existing financial and stay history while changing booking status. Staff-edited source fields, payment designation, imported finances, classified statements, confirmed taxes and conflicting accommodation/stay changes are held for reconciliation. Concurrent changes after review reject the transaction.

On 2026-11-07, staff replaced the confirmation rule: matching normalized full names mean the same customer. Imports automatically reuse an existing customer or one created earlier in the batch, across all OTAs, without prompting. Matching uses Unicode Form KC, collapsed Unicode whitespace and invariant uppercase; it is not fuzzy or partial-name matching. Original customer text, contact details and staff data are preserved. If historical duplicate customer records already exist, new bookings use the lowest matching CustomerID; existing records and reservation links are not retroactively merged. Correcting different people with identical names is deferred to a future interface.

The shared customer and adjacent dates can join operational stays across OTAs. Nonconsecutive visits stay separate; bridge bookings preserve stay notes. Original reservation identifiers, prices, payments and import histories remain independent. Automatic adjacency excludes stays containing cancelled booking links. A genuinely different guest name on an existing booking still requires reconciliation rather than replacing its customer association.

Booking.com and Hostelworld bookings can share an OTA-scoped group when they have the same prefix before the final hyphen and a numeric-text suffix.

Approved imports learn OTA-specific normalized room-name aliases. Conflicting external/master names require selection of one of the five physical rooms. Contradictory choices for one OTA/name reject the batch. No physical rooms or automatic individual bed assignments are created.

All persistence uses a serialized transaction and preserves complete source bytes plus row audit history. Repeated imports do not duplicate business records. Only the latest accepted `Processed` source version determines whether a row is unchanged; approved updates and cancellations also produce `Processed` audit rows. Damaged audit data surfaces an explicit error or reconciliation hold, never an automatic overwrite.

## Verification scope

All 69 revised-schema/import/UI checks passed on 2026-11-07, including the automatic-name rule, exact existing/incoming comparison values, highlighted changes and comparison-before-question rendering. Validation uses temporary, uniquely named SQL Server LocalDB databases, never production guest reservations. It also covers the real sample, repeated imports, approvals, stays/groups, rollback, staff-data protection, damaged audit handling, room conflicts, lookup seeds, local asset URLs and migration rollback/recreation. Temporary verification databases are removed afterward. Local database creation and seeding succeeded through the application initialization service after the authorized 2026-11-06 reset. The 2026-11-07 changes require no schema migration, reset or retroactive customer merge.

A persistent xUnit project remains blocked by unavailable native Visual Studio project-inventory tooling; the session verification executable is not presented as an xUnit suite. The final completion report records the actual executed check count and build result.
