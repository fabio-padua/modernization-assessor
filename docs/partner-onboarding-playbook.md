# Modernization Assessor Partner Onboarding Playbook

## 1) Purpose

This playbook helps a Microsoft partner adopt, customize, and operationalize the Modernization Assessor solution as a repeatable commercial assessment offer.

Primary outcomes:
- Deliver modernization draft assessments faster with consistent quality.
- Package the delivery into a fixed-scope, fixed-price service.
- Preserve architect governance with human review before customer delivery.

## 2) Target Audience

- Partner Practice Lead
- Cloud Solution Architect
- AI Engineer / App Engineer
- Delivery Manager
- Sales / Pre-Sales

## 3) Engagement Model

### Offer Definition

Recommended offer name:
- Azure Modernization Rapid Assessment

Suggested scope:
- Input: customer inventory export (CSV for phase 1; additional formats later)
- Processing: normalization + 7R workload recommendation draft
- Output: JSON + Markdown assessment draft
- Review: architect validation workshop

Suggested non-goals for initial offer:
- Fully automated customer-ready output with no architect review
- Full migration execution planning in first rollout

## 4) Team Roles and Responsibilities

### Practice Lead
- Owns service packaging, pricing, and quality standards.
- Approves final offer scope and go-to-market messaging.

### Lead Architect
- Defines modernization decision standards and review criteria.
- Owns recommendation validity and exception handling.

### AI/App Engineer
- Runs and customizes orchestrator and stage logic.
- Maintains report templates, mappings, and integration code.

### Delivery Manager
- Owns delivery timeline, milestones, and risk tracking.
- Ensures each assessment follows the standard playbook.

### Pre-Sales / Account Team
- Qualifies customer data readiness.
- Sets expectations on deliverables, timeline, and assumptions.

## 5) 6-Week Onboarding Timeline

## Week 1: Foundation and Alignment
- Confirm business goals and target industries.
- Define success metrics:
  - assessment cycle time target
  - recommendation acceptance rate target
  - rework rate target
- Confirm governance:
  - mandatory architect approval gate
  - quality rubric for rationale and confidence

Exit criteria:
- Team roles assigned
- Pilot customer profile agreed
- Service scope baseline approved

## Week 2: Environment and Baseline Execution
- Build and run baseline flow with sample inventory.
- Produce output artifacts and run internal walkthrough.
- Capture baseline measurements:
  - runtime
  - output completeness
  - reviewer feedback

Exit criteria:
- End-to-end baseline run completed
- Baseline report archived for comparison

## Week 3: Classification and Quality Calibration
- Tune rules and rationale standards for partner methodology.
- Add explicit confidence guidance for reviewer prioritization.
- Build a review rubric per workload:
  - strategy correctness
  - rationale groundedness
  - prerequisite quality
  - risk completeness

Exit criteria:
- Calibrated rule set approved by Lead Architect
- Initial quality rubric ratified

## Week 4: Data Onboarding and Parser Strategy
- Identify top 2 customer inventory formats beyond CSV.
- Define mapping contracts to canonical workload schema.
- Implement first parser adapter (or connector path) for most common non-CSV source.

Exit criteria:
- One non-CSV source mapped and tested
- Mapping documentation completed

## Week 5: Delivery Packaging and Sales Enablement
- Convert technical output into customer-facing assessment pack.
- Define fixed-scope assumptions and exclusions.
- Create pre-sales checklist:
  - input readiness
  - stakeholder availability
  - timeline constraints

Exit criteria:
- Offer one-pager and SOW skeleton ready
- Internal demo script ready for account teams

## Week 6: Pilot Delivery and Go/No-Go
- Execute one pilot assessment with real customer data.
- Run retrospective on quality, effort, and cycle time.
- Finalize go/no-go decision for scaled launch.

Exit criteria:
- Pilot signed off by Practice Lead and Lead Architect
- Launch checklist approved

## 6) Delivery Workflow (Per Customer)

1. Intake and qualification
- Verify data source, coverage, and freshness.
- Confirm customer objectives and constraints.

2. Data normalization
- Ingest inventory and map to canonical schema.
- Flag missing and low-confidence fields for review.

3. Recommendation generation
- Produce 7R strategy and target Azure service per workload.
- Attach rationale, prerequisites, and risks.

4. Architect review gate
- Triage low-confidence or high-impact workloads first.
- Resolve contradictions and edge-case misclassifications.

5. Output packaging
- Produce executive summary and workload detail sections.
- Convert into customer-facing narrative and action list.

6. Readout and next-step plan
- Walk customer through findings and assumptions.
- Confirm migration assessment backlog and wave candidates.

## 7) Acceptance Criteria

### Technical Acceptance
- End-to-end run succeeds with target customer inventory.
- Output contains workload and classification sections with no schema breaks.
- No critical runtime errors in orchestration path.

### Quality Acceptance
- Recommendation rationale references real workload attributes.
- Confidence values are meaningfully distributed (not uniform).
- Architect review confirms acceptable recommendation quality threshold.

### Delivery Acceptance
- Assessment delivered within agreed SLA window.
- Customer readout completed with documented assumptions.
- Follow-up modernization backlog produced.

## 8) KPI and Governance Dashboard

Track weekly:
- Median assessment turnaround time
- Workloads reviewed per architect-hour
- Recommendation acceptance rate at first review
- Rework rate after architect review
- Percentage of low-confidence workloads

Governance controls:
- Mandatory architect sign-off
- Versioned rule/prompt change log
- Pilot-to-production gate reviews

## 9) Risks and Mitigations

Risk: inconsistent customer source quality
- Mitigation: enforce intake readiness checklist and minimum data contract.

Risk: over-trust in automated recommendations
- Mitigation: keep architect review mandatory and visible in process.

Risk: misclassification in edge cases
- Mitigation: maintain exception catalog and update rule set every sprint.

Risk: difficult commercialization
- Mitigation: standardize offer packaging and pre-sales qualification script.

## 10) Minimum Resellable Version (MRV) Checklist

A partner is ready to sell when all items below are complete:

Commercial readiness:
- Offer name, scope, assumptions, and exclusions approved.
- Fixed-price or fixed-bucket pricing model defined.
- SOW template and delivery timeline available.

Technical readiness:
- Baseline end-to-end execution validated on real customer-like data.
- At least one partner-specific classification calibration completed.
- Output format aligned to partner delivery standards.

Quality readiness:
- Review rubric documented and in use.
- Architect sign-off gate enforced.
- Rework loop and defect logging defined.

Operational readiness:
- Named owner for platform maintenance.
- Named owner for methodology updates.
- Runbook for common failures and data issues.

Go-to-market readiness:
- Internal demo storyline validated.
- Sales FAQ and qualification checklist prepared.
- First pilot customer identified and scheduled.

## 11) First 90 Days After Launch

Days 1-30:
- Run 2 to 3 guided assessments.
- Capture quality and timing metrics.
- Stabilize data onboarding and review workflow.

Days 31-60:
- Add one additional source connector.
- Improve rationale quality based on architect feedback.
- Refine offer packaging and proposal language.

Days 61-90:
- Industrialize internal training for additional consultants.
- Define vertical-specific variants (for example, regulated industries).
- Publish partner case story and internal benchmark metrics.

## 12) Implementation Pointers in This Repository

Core execution:
- src/Orchestrator/Program.cs
- src/Orchestrator/Pipeline/AssessmentPipeline.cs

Data normalization:
- src/Orchestrator/Pipeline/DiscoveryStage.cs
- src/Shared/Models/Workload.cs

Recommendation logic and output:
- src/Orchestrator/Pipeline/ClassifierStage.cs
- src/Orchestrator/Pipeline/MarkdownRenderer.cs
- src/Shared/Models/Classification.cs
- src/Shared/Models/AssessmentReport.cs

Roadmap and architecture context:
- README.md
- docs/architecture/v0.1-scope.md
- infra/main.bicep
- .foundry/agent-metadata.yaml
