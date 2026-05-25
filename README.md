# Modernization Assessor

> A partner-reusable, multi-agent AI system that turns a customer's raw IT estate inventory into a draft Azure modernization assessment — workload classification, target services, rationale, prerequisites, risks — in **hours instead of weeks**.

Built with **Microsoft Agent Framework (MAF)** + **Azure AI Foundry**. C# / .NET 9. Designed from day one to be cloned and resold by Microsoft partners.

[![Build](https://github.com/fabio-padua/modernization-assessor/actions/workflows/build.yml/badge.svg)](https://github.com/fabio-padua/modernization-assessor/actions/workflows/build.yml)
![Status: v0.1 thin slice](https://img.shields.io/badge/status-v0.1%20thin%20slice-blue)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

---

## Table of contents

1. [Why this exists](#why-this-exists)
2. [What it does](#what-it-does)
3. [Architecture](#architecture)
4. [The 7 R's classification model](#the-7-rs-classification-model)
5. [Repository layout](#repository-layout)
6. [Quickstart](#quickstart)
7. [Partner-reusable design principles](#partner-reusable-design-principles)
8. [Roadmap](#roadmap)
9. [Comparison to existing tooling](#comparison-to-existing-tooling)
10. [Responsible AI & human-in-the-loop](#responsible-ai--human-in-the-loop)
11. [License](#license)

---

## Why this exists

A typical Azure modernization assessment today takes **2–3 architects 2 weeks** of interviews, Azure Migrate exports, spreadsheet wrangling, and PowerPoint crafting. Output is inconsistent across engagements and rarely reusable. Partners that want to sell *fixed-price* modernization assessments can't, because their cost basis is unpredictable.

The opportunity is not "an AI that replaces the architect." It's **an AI that gives the architect a strong, well-reasoned first draft** — so the architect spends their time validating, customizing, and selling, not parsing CSVs.

This repository is the reference implementation of that idea, packaged so any Microsoft partner can clone it, plug in their own templates and pricing models, and ship.

## What it does

**Input:** a customer IT estate inventory — Azure Migrate CSV, ServiceNow CMDB export, Excel workbook, or partner-specific format.

**Output:** a draft assessment deliverable containing:

1. A canonical, normalized inventory (deduped, fields completed where possible)
2. Per-workload modernization strategy (one of the **7 R's**) with target Azure service, rationale, prerequisites, and risks
3. *(v0.2)* A 3-year TCO model based on live Azure Retail Pricing
4. *(v0.2)* A wave-based migration roadmap with dependency awareness
5. *(v0.2)* A risk register anchored to the Azure Well-Architected Framework

**Format:** structured JSON for downstream tooling + a human-readable Markdown summary. Word and PowerPoint generation is on the roadmap once the underlying analysis is solid.

## Architecture

```
                    ┌────────────────────────────────────────────────┐
                    │   Partner / customer-facing UI (Teams, web)    │
                    └─────────────────────┬──────────────────────────┘
                                          │
                    ┌─────────────────────▼──────────────────────────┐
                    │   Azure API Management — AI Gateway            │
                    │   per-partner token quotas · content safety    │
                    │   jailbreak detection · cost attribution       │
                    └─────────────────────┬──────────────────────────┘
                                          │
                    ┌─────────────────────▼──────────────────────────┐
                    │   MAF Orchestrator (Azure Container Apps)      │
                    │   C# .NET 9 · graph-based multi-agent workflow │
                    │   durable checkpoint/resume · OTel tracing     │
                    └──┬──────────────────────────────────────────┬──┘
                       │                                          │
       ┌───────────────▼──────────────┐         ┌─────────────────▼─────────────┐
       │  Foundry Agent Service       │         │  MCP tool servers (ACA jobs)  │
       │  ──  5 specialist agents:    │         │  ──  Azure Pricing            │
       │    · Discovery               │         │  ──  Inventory parser         │
       │    · Workload Classifier     │         │  ──  Partner extension point  │
       │    · Cost Estimator          │         └───────────────────────────────┘
       │    · Migration Planner       │
       │    · Risk Assessment         │
       └────────┬─────────────────────┘
                │
   ┌────────────▼─────────┐   ┌─────────────────────┐   ┌────────────────────────┐
   │  Azure AI Search     │   │  Azure Cosmos DB    │   │  App Insights (OTel)   │
   │  RAG over WAF +      │   │  thread + agent     │   │  spans tagged          │
   │  migration KB        │   │  state              │   │  partnerId, agentId,   │
   │                      │   │                     │   │  toolName, tokenCost   │
   └──────────────────────┘   └─────────────────────┘   └────────────────────────┘
```

### Why each piece

- **Foundry Agent Service** holds the *declarative* specialists — system prompt, knowledge sources, tools, and model are all config. A partner can clone an agent, edit its prompt or swap the model, and redeploy without touching orchestrator code.
- **Microsoft Agent Framework (MAF)** holds the *code-first* orchestration — intent routing, fan-out/fan-in across specialists, durable workflow with checkpointing, deterministic post-processing. MAF unifies Semantic Kernel + AutoGen; we use it directly.
- **MCP tool servers** are the **partner extension point**. To integrate a partner's pricing engine, ServiceNow tenant, GitHub Enterprise org, or proprietary migration factory templates, the partner writes (or forks) an MCP server. They never touch the orchestrator codebase.
- **APIM as AI Gateway** gives per-partner token caps, content safety enforcement, jailbreak detection, and *per-customer* cost attribution. This is what makes the product resellable rather than a demo.
- **AI Search** provides retrieval over the Well-Architected Framework, Azure Migrate guidance, and a partner's own past assessments / playbooks.
- **Cosmos DB** stores thread state, intermediate agent outputs, and human-review annotations. Per-partner containers isolate data.
- **App Insights** captures distributed traces with partner / agent / tool / cost dimensions — feeds the observability workbook shipped with the template.

## The 7 R's classification model

Every workload is classified into exactly one of seven strategies:

| Strategy | When to use | Typical Azure target |
|---|---|---|
| **Rehost** | Healthy workload, no immediate refactor need | Azure VM |
| **Replatform** | Container-friendly, gain managed runtime with minimal change | Azure Container Apps, App Service |
| **Refactor** | DB engine swap, PaaS adoption with code changes | Azure SQL MI, Azure DB for PostgreSQL |
| **Rebuild** | Cloud-native rewrite worth the investment | Functions, Logic Apps, ACA |
| **Replace** | SaaS equivalent already exists | Microsoft 365, Dynamics 365 |
| **Retire** | Decommission — no business value | (none) |
| **Retain** | Keep on-prem (regulatory, mainframe, sovereignty) | (none / Azure Arc for governance) |

The Workload Classifier agent must output a rationale for every choice, anchored to attributes present in the input. Hallucinated rationales are caught by the `groundedness` evaluator in the eval harness.

## Repository layout

```
modernization-assessor/
├─ README.md                    # this file
├─ LICENSE                      # MIT
├─ ModernizationAssessor.sln
│
├─ src/
│  ├─ Orchestrator/             # MAF host (CLI in v0.1, HTTP in v0.2)
│  │  ├─ Program.cs
│  │  ├─ Pipeline/              # Discovery + Classifier stages
│  │  └─ appsettings.json
│  ├─ Agents/                   # Foundry agent definitions (one folder each)
│  │  ├─ DiscoveryAgent/                  agent.md
│  │  ├─ WorkloadClassifierAgent/         agent.md
│  │  ├─ CostEstimatorAgent/              (v0.2)
│  │  ├─ MigrationPlannerAgent/           (v0.2)
│  │  └─ RiskAssessmentAgent/             (v0.2)
│  ├─ McpTools/                 # Partner-pluggable MCP servers
│  │  ├─ AzurePricingTool/                (v0.2)
│  │  └─ InventoryParserTool/             (v0.2)
│  └─ Shared/Models/            # Workload, Classification, AssessmentReport
│
├─ tests/Orchestrator.Tests/    # xUnit
├─ infra/                       # Bicep — subscription-scope deployment
│  └─ main.bicep
├─ .foundry/                    # Foundry skill workspace
│  ├─ agent-metadata.yaml       # canonical env + agent config
│  ├─ datasets/                 # golden eval datasets (gitignored content)
│  ├─ evaluators/               # evaluator definitions
│  └─ results/                  # eval results (gitignored content)
├─ samples/inventory/           # synthetic customer inventories
├─ docs/architecture/           # v0.1-scope.md, design notes
└─ .github/workflows/           # CI: build + test + smoke run
```

## Quickstart

### Prerequisites

- .NET 9 SDK ([download](https://dotnet.microsoft.com/download))
- Azure CLI (optional for v0.1; required for v0.2 `azd up`)
- Azure Developer CLI ≥ 1.25 (required for v0.2)
- Docker Desktop (required for v0.2 MCP servers)

### v0.1 — run the thin slice locally

```pwsh
git clone https://github.com/fabio-padua/modernization-assessor.git
cd modernization-assessor

dotnet build
dotnet test

dotnet run --project src/Orchestrator -- `
  --inventory samples/inventory/contoso.csv `
  --customer Contoso
```

Output:

```
src/Orchestrator/out/
  assessment-YYYYMMDD-HHMMSS.json
  assessment-YYYYMMDD-HHMMSS.md
```

The Markdown file is a complete (if rough) draft assessment ready for human review.

### v0.2 — deploy to Azure (roadmap)

```pwsh
azd auth login
azd init
azd env set ENABLE_HOSTED_AGENTS true
azd env set PARTNER_CODE acme
azd up
```

This provisions Foundry + ACA + APIM + AI Search + Cosmos + App Insights into the active subscription, deploys the orchestrator container, creates the five Foundry agents, and uploads sample datasets to AI Search.

## Partner-reusable design principles

These are not aspirational. Every PR should be checked against this list.

1. **Zero-touch onboarding.** One `azd up` deploys the full stack into a partner subscription. No portal clicks.
2. **Externalize everything that varies** — model, prompts, tools, RAG sources, branding — into `agent.yaml` / env / Key Vault. *Code never needs to change to onboard a new partner.*
3. **Multi-tenant by default.** Partner brings own Entra tenant + App Registration. Managed identity end-to-end. No keys.
4. **Partner-pluggable tools via MCP, not by forking the orchestrator.** New data sources = new MCP server.
5. **Built-in observability.** Every span tagged with `partnerId`, `agentId`, `toolName`, `tokenCost`. App Insights workbook ships with the template.
6. **Eval harness in-repo.** Golden dataset + Foundry evaluators so partners can retune safely. Continuous evaluation against the golden set on every PR.
7. **Per-partner cost attribution at the gateway.** APIM policies meter and tag every token by partner — invoicing & quota live there.

## Roadmap

| Version | Scope | Status |
|---|---|---|
| v0.0 | Repo scaffolding, sample inventory, .NET solution shell | ✅ Done |
| **v0.1** | **CLI thin slice: Discovery + Classifier deterministic stages, JSON + Markdown output, xUnit tests, CI workflow** | ✅ **Current** |
| v0.2 | Foundry project (`azd up`), Discovery + Classifier as Foundry prompt agents, MAF orchestrator HTTP host, Azure Pricing MCP tool, golden eval dataset, App Insights tracing | 🚧 Next |
| v0.3 | Cost Estimator + Migration Planner agents, AI Search RAG over WAF + migration KB, APIM AI Gateway, multi-tenant Cosmos DB | ⏳ |
| v0.4 | Risk Assessment agent (Well-Architected + LGPD), Word/PowerPoint output, eval regression detection in CI | ⏳ |
| v1.0 | Partner branding kit, onboarding documentation, reference customer story | 🎯 |

See [`docs/architecture/v0.1-scope.md`](docs/architecture/v0.1-scope.md) for the current sprint scope.

## Comparison to existing tooling

| | Azure Migrate | Manual assessment | **Modernization Assessor** |
|---|---|---|---|
| Inventory normalization | ✅ Azure-side only | Manual | ✅ Multi-source via Discovery agent |
| Workload classification | ❌ (rehost-centric) | ✅ | ✅ Full 7 R's |
| Rationale per recommendation | ❌ | ✅ (verbose) | ✅ (structured, auditable) |
| Cost model | ✅ (Azure list price) | ✅ (custom) | ✅ Live Retail Pricing API (v0.2) |
| WAF + risk anchoring | ❌ | Depends on architect | ✅ (v0.4) |
| Customer deliverable output | ❌ | Architect-built | ✅ Draft Markdown / Word / PPT |
| Time per assessment | Hours (inventory only) | 2–3 weeks | **Hours, then human review** |
| Partner-resellable | n/a | n/a | ✅ By design |

This is not a replacement for Azure Migrate — it consumes Azure Migrate output as one of its inputs.

## Responsible AI & human-in-the-loop

This system makes high-stakes recommendations about a customer's business systems. It is built with these guardrails:

- **No output is customer-ready without architect review.** The Markdown summary header always carries a "DRAFT — pending architect review" watermark.
- **Every recommendation includes a confidence score and a rationale anchored to input attributes.** Low-confidence items are surfaced for review first.
- **Hallucination evaluators run on every PR.** A recommendation that cites attributes not present in the input fails the gate.
- **No PII in agent memory.** Cosmos DB stores agent reasoning, not customer PII; PII fields are tokenized at the gateway.
- **Customer data never leaves the partner tenant.** Each deployment is single-tenant. The model endpoint is the partner's own Foundry project.
- **Auditability.** Every span is logged with partner / agent / tool / cost. Any output can be traced back to the exact prompt and tool calls.

## License

MIT — see [LICENSE](LICENSE). You are free to fork, customize, and resell. Attribution appreciated, not required.

---

**Author:** Fabio Padua — Partner Solution Architect, Microsoft.
This is part of my Track A signature work on Microsoft AI Solutions + App Innovation.
Feedback, issues, and PRs welcome.
