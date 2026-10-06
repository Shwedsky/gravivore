# GRAVIVORE Visual Production V2 — Source Intake Checkpoint

Branch: `art/visual-production-v2-intake`
Base main at intake start: `40b682d3570d08c427999d42514501bb6c1c2dfd`

## Scope

This checkpoint starts source intake for the first production slice only:

- G-0 donor mechanics pool
- Scout donor review
- Cutter donor review
- Magnetar donor-mechanics review
- industrial environment backbone
- material source pack
- optional secondary props only when acquisition is straightforward

## Production contract

Source-of-truth documents are the merged visual-preproduction and sourcing packages from PR #53 and PR #54. No art-direction reinterpretation is allowed in this intake pass.

## Intake gate

An asset may only advance from visual candidate to production approval when its actual source archive has been acquired from an official/publicly authorized source, inspected, and matched to an explicit license/provenance record. Web-page inspection alone does not qualify as source inspection.

Allowed maturity/status terms for this pass:

- BLOCKED BY LOGIN
- PAYMENT REQUIRED
- SOURCE UNAVAILABLE
- DOWNLOADED
- SOURCE INSPECTED
- LICENSE VERIFIED
- BLENDER INSPECTED
- APPROVED FOR PRODUCTION
- REJECTED

## Delivery rules

- No Unity runtime integration in this branch.
- No large third-party source archives committed unless repository policy explicitly permits it.
- Downloaded archives are tracked by source URL, filename, license/provenance, and SHA256.
- Major intake blocks are committed and pushed independently.
- This branch remains unmerged until the final intake gate is reviewed.
