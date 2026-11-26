# Offline US coin catalog: sources and coverage

Research date: November 25, 2026. Context: [Establish offline coin catalog sources and coverage](https://github.com/AnthonyPitman/coin-collection/issues/9). This note recommends a reference-data approach; it does not implement a catalog. The subsequent owner decisions below supersede the original coverage-floor recommendations and open questions later in this research note.

## Subsequent owner decisions

Recorded in [Set initial catalog coverage and date conventions](https://github.com/AnthonyPitman/coin-collection/issues/10):

- Target catalog coverage from 1900 onward for the selected coin types, including earlier design families within that period. The narrower modern-family floors in the research table were not adopted. Pre-1900 catalog coverage is deferred; the existing confirmed-catalog-miss path still permits recording older coins.
- Keep Native American annual designs as a design family within the Sacagawea coin type.
- Use numeric issue year 1976 or 2026 for the corresponding dual-date issues; accept either that year or the recognized full inscription during entry. Preserve the inscription with design information for display, not as a production-year assertion or range.
- Attribute mints only when supported. Auto-select a design only when catalog coverage establishes a unique match; otherwise offer a suggestion for confirmation.

## Recommendation

Maintain a small, reviewed, text-only catalog in the repository and bundle a versioned snapshot with each application release. Use US Mint denomination histories and individual program/design pages as the primary references. Treat the catalog as an aid to selection and warnings, not as proof that an unlisted coin cannot exist. This matches the agreed ability to record unknown years/designs and confirmed catalog misses.

Use design-family rules for stable periods and explicit subject/year records for rotating programs. Do not generate every year/mint combination from a broad date range: a design period does not establish that every mint produced it every year. Likewise, a catalog with only one recorded candidate does not justify automatic selection if coverage for that lookup is known to be incomplete. Record coverage confidence as well as the candidates.

## Sources and useful coverage

The following is a proposed practical starting boundary, not a promise of complete historical coverage. The owner should approve the historical floor before these ranges become first-release requirements. Face values below are the selected denominations, independent of collectible value.

| Requested coin type | Face value | Proposed initial reference coverage and source |
| --- | --- | --- |
| Penny | $0.01 | Lincoln families: wheat reverse 1909–1958, Memorial 1959–2008, the four 2009 subjects, and Union Shield from 2010 through the reviewed cutoff. Include the 2026 date inscription explicitly rather than extending an old inscription assumption. [Penny history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/penny) |
| Nickel | $0.05 | Jefferson/Monticello from 1938, the Westward Journey designs in 2004–2005, and Return to Monticello from 2006. Review the 2026 inscription change separately. Buffalo and earlier families can be later coverage; the source also documents Buffalo 1913–1938, which overlaps Jefferson's first year. [Nickel history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/nickel) |
| Dime | $0.10 | Roosevelt 1946–2025 and the 2026 Emerging Liberty design. The Mint states Roosevelt is to return in 2027; keep that as future program information until release verification. Mercury 1916–1945 is a straightforward optional historical extension. [Dime history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/dime) |
| Quarter | $0.25 | Washington family from 1932, Bicentennial, all 50 State subjects (1999–2008), DC/territories (2009), America the Beautiful (2010–2021), 2021 Crossing the Delaware, American Women (2022–2025), and the five 2026 subjects. The 2021 programs overlap; do not assume a single design from that year. [Quarter history and program links](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/quarter) |
| Half dollar | $0.50 | Kennedy from 1964 through 2025, including the Bicentennial family, and 2026 Enduring Liberty. Franklin 1948–1963 and Walking Liberty 1916–1947 are possible later coverage, not implicitly included by selecting the denomination. [Half-dollar history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/half-dollar) |
| Eisenhower dollar | $1.00 | 1971–1978, with the ordinary Apollo-themed and Bicentennial reverse families distinguished. [Dollar history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/dollar-coins) |
| Susan B. Anthony dollar | $1.00 | 1979–1981 and 1999, not every year in the enclosing interval. [Susan B. Anthony reference](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/susan-b-anthony-dollar) |
| Sacagawea dollar | $1.00 | Original eagle reverse, 2000–2008. Owner decision needed on whether this type also contains the Native American program, which begins in 2009 and changes reverse annually while retaining Sacagawea. The official index lists subjects through 2026. [Native American program and Sacagawea history](https://www.usmint.gov/learn/coins-and-medals/collectible-coins/native-american-dollar-coins) |
| Presidential dollar | $1.00 | The 2007–2016 program plus the separately authorized 2020 George H. W. Bush issue. Enumerate actual issue subjects rather than extrapolating a perpetual schedule. Two Grover Cleveland subjects correspond to separate terms; the years printed beside presidents in the index describe their service, not their coin issue years. Follow individual issue pages for issue-year data. [Presidential program](https://www.usmint.gov/learn/coins-and-medals/collectible-coins/presidential-dollar-coins) |
| American Innovation dollar | $1.00 | The 2018 introductory design and the reviewed annual state subjects from 2019 through 2026. The current official index lists 2026 Iowa, Wisconsin, California, and Minnesota subjects. Future announced subjects should not silently become confirmed issued entries. [American Innovation program](https://www.usmint.gov/learn/coins-and-medals/collectible-coins/american-innovation-dollar-coins) |

The 2026 date matters: the Mint documents special designs, dual dates, and other changes this year. Its [Semiquincentennial media kit](https://www.usmint.gov/news/media-kit/semiq-resources) is a first-party starting point for release review. Therefore, do not label a pre-2026 catalog “current” or assume Roosevelt/Kennedy designs continue unchanged indefinitely. This investigation verifies relevant source availability, not every individual issue and mint combination through November 2026.

## Mint marks and uncertain attribution

The [Mint's mint-mark history](https://www.usmint.gov/learn/collecting-basics/mint-marks) establishes several exceptions that affect this application: circulating coins lacked mint marks in 1965–1967; Philadelphia marks have changed over time; San Francisco also made unmarked circulating pennies in the 1970s/1980s. Consequently, no mark must not universally imply Philadelphia. The same source identifies modern P, D, S, and W marks; do not restrict every supported coin to only P/D/S.

Keep the observed mark exactly distinct from any inferred mint attribution. A mint filter can include an entry only when attribution is established; ambiguous entries need a visible unknown/undetermined grouping rather than being counted as Philadelphia. If candidate mints are retained, do not count one coin in multiple mint totals. This is a recommendation to refine attribution behavior, not a new assumption about the user's unmarked coins.

The [penny reference](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/penny) specifically documents the 2017 P cent and 2019 W collectible cents. General summaries about unmarked cents need these issue-specific exceptions. Finish, condition, metallic varieties, privy marks, and errors remain outside the initial entry identity; source data must not accidentally expand that identity.

## Structured data and acquisition

The official [Machine Readable Data page](https://www.usmint.gov/data) offers CSV downloads and explicitly permits republication of those data with US Mint attribution. Its listed circulating datasets have bounded spans: general circulating coins 2014–2026, America the Beautiful 2014–2021, American Women 2022–2025, and a 2026 Semiquincentennial dataset. Thus this index alone cannot supply the complete earlier coverage proposed above.

The download links for general circulating coins, American Women, and Semiquincentennial data were checked; the research browser identified them as CSV but could not parse that content type. Their column schemas and row completeness were not inspected. No supported all-history design-catalog API was verified. Do not claim that no such API exists; do not plan around an undocumented endpoint.

Recommended acquisition order: use the program pages to define families and subjects, inspect the relevant CSV schemas during catalog preparation for corroborating issue data, then manually review the compact result before bundling it. Automated scraping is not required for the first release. This internal use of source CSV files does not reintroduce the deferred user-facing CSV import/export feature.

## Provenance, reuse, and maintenance

Keep source URL, retrieval/review date, evidence note, and coverage limitations alongside each family/rule or subject. Use stable application identifiers independent of a website's URL or changing display text. Label the catalog snapshot with its own revision and reviewed-through date; that revision is different from the collection save-format version. These are maintenance recommendations.

The Mint's [Terms of Use](https://www.usmint.gov/resources/terms-of-use) warn that not everything on its website is public domain and identify third-party material and design rights. The machine-readable data page supplies a narrower explicit attribution statement for its datasets. Start with independently written factual labels and source citations, without images, copied descriptive prose, logos, or bulk republishing of website pages. Image reuse would need a separate review of the applicable design-use terms. This note does not establish blanket redistribution permission for all Mint content.

For each app release, review changed/new programs, reconcile explicit issue records with source pages, inspect boundaries and overlaps, record additions/corrections in the catalog changelog, and run the examples below. A missing source or unresolved conflict should reduce lookup confidence, not silently erase user records. Catalog updates must preserve saved observations and existing stable design identifiers.

## Validation and ambiguity examples

These are proposed acceptance scenarios, not implemented tests.

| Scenario | Expected catalog behavior and evidence |
| --- | --- |
| Quarter, 2001 | Offer New York, North Carolina, Rhode Island, Vermont, and Kentucky; do not choose North Carolina from year alone. [50 State Quarters](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/quarter/50-state-quarters) |
| Penny, 2009 | Offer four Lincoln Bicentennial subjects rather than one generic reverse. [Penny history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/penny) |
| Nickel, 1938 with older coverage enabled | Preserve Buffalo/Jefferson ambiguity instead of assigning solely by year. [Nickel history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/nickel) |
| Quarter inscribed 1776–1976 | Ask/normalize according to an owner-approved dual-date policy. The Mint says 1975 and 1976 strikes share that inscription and cannot be distinguished by it; never present a guessed production year as observed fact. [Quarter history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/quarter) |
| Unmarked penny from the 1970s | Retain no mark and undetermined mint unless independent evidence establishes attribution; do not automatically count it as Philadelphia. [Mint-mark history](https://www.usmint.gov/learn/collecting-basics/mint-marks) |
| Sacagawea portrait, 2009 | Look for the annual Native American reverse; year and mint mark can be on the edge. [Native American program](https://www.usmint.gov/learn/coins-and-medals/collectible-coins/native-american-dollar-coins) |
| Dime, 2026 | Offer Emerging Liberty rather than automatically extending Roosevelt through the current year. [Dime history](https://www.usmint.gov/learn/coins-and-medals/circulating-coins/dime) |
| Unknown year, or a confirmed catalog miss | Permit known design selection or explicit unknown design, retain the recorded entry, and display coverage warnings. This follows the already resolved validation decision rather than an external historical claim. |

## Decisions still requiring the owner

1. Approve a minimum historical floor for each denomination, or specify older families that matter to the actual collection. The proposed modern-family floor above is a scope recommendation, not an agreed exclusion of older coins.
2. Decide whether Native American annual designs belong under the current Sacagawea type or a newly named coin type. Recommended: retain the familiar Sacagawea entry choice and distinguish the Native American design family, provided the owner agrees to that terminology.
3. Choose accepted input/display for dual-date coins. Recommended: accept the full inscription, store a nominal issue year (1976 or 2026) for sorting, retain the inscription in reference data, and never call it an inferred production year. That refines the current glossary's single observed-year wording and must be confirmed.
These follow-ups can be separate decision tickets. No exhaustive catalog download, image asset set, or network dependency at application runtime is needed to answer them.
