// Generates docs/Modernization-Assessor-Defense.pptx
// Run from repo root:  node docs/build-deck.js

const PptxGenJS = require("C:\\Users\\fabiopadua\\AppData\\Roaming\\npm\\node_modules\\pptxgenjs");

const pres = new PptxGenJS();
pres.layout = "LAYOUT_WIDE";   // 13.333 × 7.5 in
pres.author = "Fabio Padua";
pres.company = "Microsoft";
pres.title = "Modernization Assessor — Defense of Architecture";
pres.subject = "Partner-reusable multi-agent AI for Azure modernization";

// Palette: Midnight Executive
const NAVY = "1E2761";
const ICE = "CADCFC";
const WHITE = "FFFFFF";
const MUTED = "6B7393";
const ACCENT = "F96167";     // pop accent for callouts
const SLATE_BG = "F4F6FB";   // light content background
const TEXT_DARK = "1A1F36";
const TEXT_MUTED = "4A516B";

const W = 13.333;
const H = 7.5;
const MARGIN = 0.55;

// helpers
const makeShadow = () => ({
  type: "outer", color: "000000", opacity: 0.10, blur: 12, offset: 2, angle: 135,
});

function addFooter(slide, pageNum) {
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0, y: H - 0.32, w: W, h: 0.04, fill: { color: NAVY }, line: { type: "none" },
  });
  slide.addText("Modernization Assessor  ·  Track A — AI Solutions + App Innovation", {
    x: MARGIN, y: H - 0.28, w: 8, h: 0.25,
    fontFace: "Calibri", fontSize: 9, color: MUTED, valign: "middle", margin: 0,
  });
  slide.addText(`${pageNum} / 12`, {
    x: W - 1.2, y: H - 0.28, w: 0.6, h: 0.25,
    fontFace: "Calibri", fontSize: 9, color: MUTED, align: "right", valign: "middle", margin: 0,
  });
}

function addContentHeader(slide, eyebrow, title) {
  slide.background = { color: SLATE_BG };
  // Eyebrow strip
  slide.addShape(pres.shapes.RECTANGLE, {
    x: MARGIN, y: 0.45, w: 0.2, h: 0.18, fill: { color: NAVY }, line: { type: "none" },
  });
  slide.addText(eyebrow, {
    x: MARGIN + 0.32, y: 0.40, w: 10, h: 0.3,
    fontFace: "Calibri", fontSize: 11, color: NAVY, bold: true, charSpacing: 4, margin: 0,
  });
  slide.addText(title, {
    x: MARGIN, y: 0.75, w: W - 2 * MARGIN, h: 0.85,
    fontFace: "Georgia", fontSize: 32, color: TEXT_DARK, bold: true, margin: 0,
  });
  // Divider
  slide.addShape(pres.shapes.RECTANGLE, {
    x: MARGIN, y: 1.65, w: 1.3, h: 0.04, fill: { color: NAVY }, line: { type: "none" },
  });
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 1 — Title
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  s.background = { color: NAVY };

  // Decorative ice-blue accent block (motif: small navy/ice rectangle pair, repeats across deck)
  s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 0.45, h: H, fill: { color: ICE }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.45, y: 0, w: 0.10, h: H, fill: { color: ACCENT }, line: { type: "none" } });

  s.addText("TRACK A — AI SOLUTIONS + APP INNOVATION", {
    x: 1.1, y: 1.6, w: 11, h: 0.4,
    fontFace: "Calibri", fontSize: 12, color: ICE, bold: true, charSpacing: 8, margin: 0,
  });

  s.addText("Modernization Assessor", {
    x: 1.1, y: 2.1, w: 11.5, h: 1.4,
    fontFace: "Georgia", fontSize: 52, color: WHITE, bold: true, margin: 0,
  });

  s.addText("A partner-reusable multi-agent AI system for Azure modernization assessments — built on Microsoft Agent Framework + Azure AI Foundry.", {
    x: 1.1, y: 3.6, w: 11.0, h: 1.3,
    fontFace: "Calibri", fontSize: 20, color: ICE, italic: true, margin: 0,
  });

  // Tag pills
  const pills = ["Microsoft Agent Framework", "Azure AI Foundry", "C# / .NET 9", "MCP-extensible"];
  let x = 1.1;
  pills.forEach((p) => {
    const w = 0.25 + p.length * 0.105;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, {
      x, y: 5.2, w, h: 0.42,
      fill: { color: NAVY }, line: { color: ICE, width: 1 }, rectRadius: 0.1,
    });
    s.addText(p, {
      x, y: 5.2, w, h: 0.42,
      fontFace: "Calibri", fontSize: 11, color: ICE, bold: true, align: "center", valign: "middle", margin: 0,
    });
    x += w + 0.18;
  });

  // Author block
  s.addText("Fabio Padua", {
    x: 1.1, y: 6.4, w: 11, h: 0.35,
    fontFace: "Calibri", fontSize: 16, color: WHITE, bold: true, margin: 0,
  });
  s.addText("Partner Solution Architect  ·  CS Tech Americas  ·  Microsoft", {
    x: 1.1, y: 6.72, w: 11, h: 0.3,
    fontFace: "Calibri", fontSize: 12, color: ICE, margin: 0,
  });
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 2 — The pain today
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "THE PROBLEM", "Modernization assessments don't scale.");

  // Three big stat cards
  const stats = [
    { num: "2–3", label: "Senior architects per engagement" },
    { num: "2 weeks", label: "Minimum elapsed time per assessment" },
    { num: "0", label: "Reusable artifacts across engagements" },
  ];
  const cardW = 3.7, cardH = 2.3, gap = 0.4;
  const totalW = stats.length * cardW + (stats.length - 1) * gap;
  let cx = (W - totalW) / 2;
  stats.forEach((st) => {
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: 2.1, w: cardW, h: cardH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: 2.1, w: 0.08, h: cardH, fill: { color: ACCENT }, line: { type: "none" },
    });
    s.addText(st.num, {
      x: cx + 0.3, y: 2.3, w: cardW - 0.4, h: 1.2,
      fontFace: "Georgia", fontSize: 56, color: NAVY, bold: true, margin: 0,
    });
    s.addText(st.label, {
      x: cx + 0.3, y: 3.5, w: cardW - 0.4, h: 0.7,
      fontFace: "Calibri", fontSize: 14, color: TEXT_MUTED, margin: 0,
    });
    cx += cardW + gap;
  });

  s.addText([
    { text: "Customers want fixed-price modernization. ", options: { bold: true } },
    { text: "Partners can't sell what they can't price. Cost basis is unpredictable when every engagement re-does the same analysis from scratch — and the deliverable quality depends on which architect happens to be available." },
  ], {
    x: MARGIN + 0.2, y: 5.0, w: W - 2 * MARGIN - 0.4, h: 1.8,
    fontFace: "Calibri", fontSize: 16, color: TEXT_DARK, valign: "top", paraSpaceAfter: 6, margin: 0,
  });

  addFooter(s, 2);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 3 — The opportunity
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "THE OPPORTUNITY", "A strong draft in hours, not weeks.");

  // Before / After comparison
  const colW = 5.6, colH = 4.6, colY = 2.0;
  const leftX = 0.7, rightX = leftX + colW + 0.9;

  // BEFORE
  s.addShape(pres.shapes.RECTANGLE, {
    x: leftX, y: colY, w: colW, h: colH,
    fill: { color: WHITE }, line: { color: ICE, width: 1 }, shadow: makeShadow(),
  });
  s.addText("BEFORE", {
    x: leftX + 0.3, y: colY + 0.25, w: colW - 0.6, h: 0.4,
    fontFace: "Calibri", fontSize: 11, color: MUTED, bold: true, charSpacing: 6, margin: 0,
  });
  s.addText("Manual assessment", {
    x: leftX + 0.3, y: colY + 0.6, w: colW - 0.6, h: 0.55,
    fontFace: "Georgia", fontSize: 22, color: TEXT_DARK, bold: true, margin: 0,
  });
  const beforeItems = [
    { text: "Architects spend 80% of their time parsing spreadsheets", options: { bullet: true, breakLine: true } },
    { text: "Output quality varies engagement-to-engagement", options: { bullet: true, breakLine: true } },
    { text: "No reusable templates, no learning loop", options: { bullet: true, breakLine: true } },
    { text: "Premium-priced, slow, hard to differentiate", options: { bullet: true } },
  ];
  s.addText(beforeItems, {
    x: leftX + 0.3, y: colY + 1.5, w: colW - 0.6, h: colH - 1.7,
    fontFace: "Calibri", fontSize: 14, color: TEXT_DARK, paraSpaceAfter: 10, valign: "top",
  });

  // AFTER
  s.addShape(pres.shapes.RECTANGLE, {
    x: rightX, y: colY, w: colW, h: colH,
    fill: { color: NAVY }, line: { type: "none" }, shadow: makeShadow(),
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: rightX, y: colY, w: 0.08, h: colH, fill: { color: ACCENT }, line: { type: "none" },
  });
  s.addText("AFTER", {
    x: rightX + 0.3, y: colY + 0.25, w: colW - 0.6, h: 0.4,
    fontFace: "Calibri", fontSize: 11, color: ICE, bold: true, charSpacing: 6, margin: 0,
  });
  s.addText("With Modernization Assessor", {
    x: rightX + 0.3, y: colY + 0.6, w: colW - 0.6, h: 0.55,
    fontFace: "Georgia", fontSize: 22, color: WHITE, bold: true, margin: 0,
  });
  const afterItems = [
    { text: "AI produces a structured first draft in hours", options: { bullet: true, breakLine: true } },
    { text: "Architect validates and customizes — does not start from zero", options: { bullet: true, breakLine: true } },
    { text: "Same artifact every time — partner ships a productized service", options: { bullet: true, breakLine: true } },
    { text: "Fixed-price modernization assessments become viable", options: { bullet: true } },
  ];
  s.addText(afterItems, {
    x: rightX + 0.3, y: colY + 1.5, w: colW - 0.6, h: colH - 1.7,
    fontFace: "Calibri", fontSize: 14, color: ICE, paraSpaceAfter: 10, valign: "top",
  });

  s.addText("This is not 'AI replacing architects.' It's 'AI giving architects superpowers.'", {
    x: MARGIN, y: 6.75, w: W - 2 * MARGIN, h: 0.4,
    fontFace: "Calibri", fontSize: 14, italic: true, color: NAVY, align: "center", margin: 0,
  });

  addFooter(s, 3);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 4 — What it does (input → output)
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "WHAT IT DOES", "Inventory in. Assessment out.");

  // Input box
  const inX = 0.7, outX = 8.2;
  const boxW = 4.4, boxH = 4.1, boxY = 2.2;

  s.addShape(pres.shapes.RECTANGLE, {
    x: inX, y: boxY, w: boxW, h: boxH,
    fill: { color: WHITE }, line: { color: ICE, width: 1 }, shadow: makeShadow(),
  });
  s.addText("INPUT", {
    x: inX + 0.3, y: boxY + 0.25, w: boxW - 0.6, h: 0.35,
    fontFace: "Calibri", fontSize: 11, color: MUTED, bold: true, charSpacing: 6, margin: 0,
  });
  s.addText("Customer IT estate", {
    x: inX + 0.3, y: boxY + 0.6, w: boxW - 0.6, h: 0.5,
    fontFace: "Georgia", fontSize: 20, color: NAVY, bold: true, margin: 0,
  });
  s.addText([
    { text: "Azure Migrate CSV export", options: { bullet: true, breakLine: true } },
    { text: "ServiceNow CMDB extract", options: { bullet: true, breakLine: true } },
    { text: "Excel application inventory", options: { bullet: true, breakLine: true } },
    { text: "Partner-specific tooling output", options: { bullet: true } },
  ], {
    x: inX + 0.3, y: boxY + 1.35, w: boxW - 0.6, h: boxH - 1.5,
    fontFace: "Calibri", fontSize: 14, color: TEXT_DARK, paraSpaceAfter: 12, valign: "top",
  });

  // Arrow
  s.addShape(pres.shapes.LINE, {
    x: inX + boxW + 0.2, y: boxY + boxH / 2, w: outX - (inX + boxW) - 0.4, h: 0,
    line: { color: NAVY, width: 3, endArrowType: "triangle" },
  });
  s.addText("MAF Orchestrator\n+ Foundry Agents", {
    x: inX + boxW + 0.15, y: boxY + boxH / 2 - 1.0, w: outX - (inX + boxW) - 0.3, h: 0.8,
    fontFace: "Calibri", fontSize: 12, italic: true, color: NAVY, align: "center", valign: "bottom", margin: 0,
  });

  // Output box
  s.addShape(pres.shapes.RECTANGLE, {
    x: outX, y: boxY, w: boxW, h: boxH,
    fill: { color: NAVY }, line: { type: "none" }, shadow: makeShadow(),
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: outX, y: boxY, w: 0.08, h: boxH, fill: { color: ACCENT }, line: { type: "none" },
  });
  s.addText("OUTPUT", {
    x: outX + 0.3, y: boxY + 0.25, w: boxW - 0.6, h: 0.35,
    fontFace: "Calibri", fontSize: 11, color: ICE, bold: true, charSpacing: 6, margin: 0,
  });
  s.addText("Draft assessment", {
    x: outX + 0.3, y: boxY + 0.6, w: boxW - 0.6, h: 0.5,
    fontFace: "Georgia", fontSize: 20, color: WHITE, bold: true, margin: 0,
  });
  s.addText([
    { text: "Normalized inventory (canonical schema)", options: { bullet: true, breakLine: true } },
    { text: "Per-workload 7 R's strategy + rationale", options: { bullet: true, breakLine: true } },
    { text: "3-year TCO (Azure Retail Pricing) — v0.2", options: { bullet: true, breakLine: true } },
    { text: "Migration roadmap + risk register — v0.2", options: { bullet: true } },
  ], {
    x: outX + 0.3, y: boxY + 1.35, w: boxW - 0.6, h: boxH - 1.5,
    fontFace: "Calibri", fontSize: 14, color: ICE, paraSpaceAfter: 12, valign: "top",
  });

  s.addText("Structured JSON for downstream tooling + human-readable Markdown summary. Word & PowerPoint output on the roadmap.", {
    x: MARGIN, y: 6.7, w: W - 2 * MARGIN, h: 0.45,
    fontFace: "Calibri", fontSize: 13, italic: true, color: TEXT_MUTED, align: "center", margin: 0,
  });

  addFooter(s, 4);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 5 — Architecture
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "ARCHITECTURE", "Five layers. Each one earns its place.");

  // Stack of bands
  const layers = [
    { title: "Partner / customer UI", desc: "Teams, web app, or partner's own portal", c: ICE, tc: TEXT_DARK },
    { title: "Azure API Management — AI Gateway", desc: "Per-partner token quotas · content safety · jailbreak detection · cost attribution", c: NAVY, tc: WHITE },
    { title: "MAF Orchestrator on Azure Container Apps", desc: "C# .NET 9 · multi-agent workflow · durable checkpoint/resume · OTel", c: NAVY, tc: WHITE },
    { title: "Foundry Agent Service + MCP tool servers", desc: "Five specialist agents (Discovery, Classifier, Cost, Planner, Risk) · partner-pluggable", c: NAVY, tc: WHITE },
    { title: "Data plane: AI Search · Cosmos DB · App Insights", desc: "RAG over WAF + KB · agent state · OTel tagged partner / agent / tool / cost", c: ICE, tc: TEXT_DARK },
  ];
  const startY = 2.0;
  const bandH = 0.85;
  const gap = 0.12;
  layers.forEach((L, i) => {
    const y = startY + i * (bandH + gap);
    s.addShape(pres.shapes.RECTANGLE, {
      x: MARGIN, y, w: W - 2 * MARGIN, h: bandH,
      fill: { color: L.c }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: MARGIN, y, w: 0.10, h: bandH, fill: { color: ACCENT }, line: { type: "none" },
    });
    s.addText(L.title, {
      x: MARGIN + 0.3, y: y + 0.05, w: 5.5, h: bandH - 0.05,
      fontFace: "Calibri", fontSize: 16, bold: true, color: L.tc, valign: "middle", margin: 0,
    });
    s.addText(L.desc, {
      x: MARGIN + 6.0, y: y + 0.05, w: W - MARGIN - 6.5, h: bandH - 0.05,
      fontFace: "Calibri", fontSize: 12, color: L.tc === WHITE ? ICE : TEXT_MUTED, valign: "middle", margin: 0,
    });
  });

  addFooter(s, 5);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 6 — Why this architecture (defense)
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "DEFENSE", "Why each piece earns its place.");

  const items = [
    { k: "Foundry Agent Service", v: "Declarative specialists. Partners clone agents, tweak prompts, swap models — without touching orchestrator code." },
    { k: "Microsoft Agent Framework", v: "Code-first orchestration with durable workflow + checkpointing. Unifies SK + AutoGen; we adopt it directly." },
    { k: "MCP tool servers", v: "The partner extension point. Plug ServiceNow, GitHub Enterprise, proprietary pricing engines without forking." },
    { k: "APIM as AI Gateway", v: "Per-partner quotas, content safety, jailbreak detection, cost attribution. What makes the product resellable." },
    { k: "Azure AI Search", v: "RAG over Well-Architected Framework and partner's past assessments — grounds recommendations in real guidance." },
    { k: "App Insights + OTel", v: "Every span tagged with partner / agent / tool / tokenCost. Workbook ships with the template." },
  ];

  // 2-column grid
  const colW = 6.05, rowH = 1.45;
  items.forEach((it, i) => {
    const col = i % 2, row = Math.floor(i / 2);
    const x = MARGIN + col * (colW + 0.3);
    const y = 2.0 + row * (rowH + 0.2);
    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: colW, h: rowH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: 0.08, h: rowH, fill: { color: NAVY }, line: { type: "none" },
    });
    s.addText(it.k, {
      x: x + 0.25, y: y + 0.15, w: colW - 0.4, h: 0.4,
      fontFace: "Calibri", fontSize: 14, bold: true, color: NAVY, margin: 0,
    });
    s.addText(it.v, {
      x: x + 0.25, y: y + 0.55, w: colW - 0.4, h: rowH - 0.6,
      fontFace: "Calibri", fontSize: 12, color: TEXT_DARK, valign: "top", margin: 0,
    });
  });

  addFooter(s, 6);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 7 — The 7 R's
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "CLASSIFICATION MODEL", "The 7 R's — one strategy per workload.");

  const rs = [
    { r: "Rehost", t: "Azure VM", d: "Healthy workload, no refactor needed" },
    { r: "Replatform", t: "Container Apps · App Service", d: "Containerize, gain managed runtime" },
    { r: "Refactor", t: "Azure SQL MI · Postgres", d: "DB swap or PaaS code changes" },
    { r: "Rebuild", t: "Functions · Logic Apps", d: "Cloud-native rewrite worth investing" },
    { r: "Replace", t: "M365 · Dynamics 365", d: "SaaS equivalent already exists" },
    { r: "Retire", t: "—", d: "Decommission, no business value" },
    { r: "Retain", t: "Azure Arc (governance)", d: "Mainframe, sovereignty, regulatory" },
  ];

  // 7-card grid: 4 + 3 layout
  const cardW = 3.0, cardH = 1.95;
  const rowGap = 0.25, colGap = 0.2;
  const row1Count = 4, row2Count = 3;
  const row1TotalW = row1Count * cardW + (row1Count - 1) * colGap;
  const row2TotalW = row2Count * cardW + (row2Count - 1) * colGap;
  const row1StartX = (W - row1TotalW) / 2;
  const row2StartX = (W - row2TotalW) / 2;

  rs.forEach((item, i) => {
    const inRow1 = i < row1Count;
    const idx = inRow1 ? i : i - row1Count;
    const x = (inRow1 ? row1StartX : row2StartX) + idx * (cardW + colGap);
    const y = 2.05 + (inRow1 ? 0 : 1) * (cardH + rowGap);

    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: cardW, h: cardH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: cardW, h: 0.45, fill: { color: NAVY }, line: { type: "none" },
    });
    s.addText(item.r, {
      x, y, w: cardW, h: 0.45,
      fontFace: "Georgia", fontSize: 16, bold: true, color: WHITE, align: "center", valign: "middle", margin: 0,
    });
    s.addText(item.t, {
      x: x + 0.15, y: y + 0.6, w: cardW - 0.3, h: 0.45,
      fontFace: "Calibri", fontSize: 12, bold: true, color: ACCENT, align: "center", margin: 0,
    });
    s.addText(item.d, {
      x: x + 0.2, y: y + 1.1, w: cardW - 0.4, h: cardH - 1.2,
      fontFace: "Calibri", fontSize: 11, color: TEXT_DARK, align: "center", valign: "top", margin: 0,
    });
  });

  s.addText("Every classification must include a rationale anchored to attributes in the input. Hallucinated rationales fail the eval gate.", {
    x: MARGIN, y: 6.7, w: W - 2 * MARGIN, h: 0.4,
    fontFace: "Calibri", fontSize: 13, italic: true, color: TEXT_MUTED, align: "center", margin: 0,
  });

  addFooter(s, 7);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 8 — Partner-reusable design principles
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "DESIGN PRINCIPLES", "What makes it actually resellable.");

  const items = [
    { n: "1", t: "Zero-touch onboarding", d: "One `azd up` deploys the full stack into a partner subscription." },
    { n: "2", t: "Externalize everything that varies", d: "Model, prompts, tools, RAG sources, branding — in config, not code." },
    { n: "3", t: "Multi-tenant by default", d: "Partner brings own Entra tenant. Managed identity end-to-end. No keys." },
    { n: "4", t: "Plug-in via MCP, not by forking", d: "New data sources = new MCP server, never a fork of the orchestrator." },
    { n: "5", t: "Observability built-in", d: "Every span tagged partner / agent / tool / cost. Workbook ships with template." },
    { n: "6", t: "Eval harness in-repo", d: "Golden dataset + Foundry evaluators so partners can retune safely." },
    { n: "7", t: "Cost attribution at the gateway", d: "APIM meters and tags every token by partner. Invoicing lives there." },
  ];

  // Two columns for compact layout
  const colCount = 2;
  const cardW = 6.05, cardH = 1.15;
  const rowGap = 0.13, colGap = 0.2;
  items.forEach((it, i) => {
    const col = i % colCount, row = Math.floor(i / colCount);
    const x = MARGIN + col * (cardW + colGap);
    const y = 1.92 + row * (cardH + rowGap);

    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: cardW, h: cardH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });

    // Number badge
    s.addShape(pres.shapes.OVAL, {
      x: x + 0.18, y: y + 0.22, w: 0.7, h: 0.7,
      fill: { color: NAVY }, line: { type: "none" },
    });
    s.addText(it.n, {
      x: x + 0.18, y: y + 0.22, w: 0.7, h: 0.7,
      fontFace: "Georgia", fontSize: 20, bold: true, color: WHITE, align: "center", valign: "middle", margin: 0,
    });

    s.addText(it.t, {
      x: x + 1.0, y: y + 0.15, w: cardW - 1.1, h: 0.36,
      fontFace: "Calibri", fontSize: 14, bold: true, color: NAVY, margin: 0,
    });
    s.addText(it.d, {
      x: x + 1.0, y: y + 0.50, w: cardW - 1.1, h: cardH - 0.55,
      fontFace: "Calibri", fontSize: 11, color: TEXT_DARK, valign: "top", margin: 0,
    });
  });

  addFooter(s, 8);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 9 — Roadmap
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "ROADMAP", "Five versions to a referenceable customer win.");

  const milestones = [
    { v: "v0.0", t: "Scaffold", d: "Repo, sample data, .NET shell", done: true },
    { v: "v0.1", t: "Thin slice", d: "Discovery + Classifier (deterministic), JSON+MD output, tests, CI", done: true, current: true },
    { v: "v0.2", t: "Foundry-wired", d: "azd up · prompt agents · MAF HTTP · Pricing MCP · eval harness", done: false },
    { v: "v0.3", t: "Full pipeline", d: "Cost Estimator · Planner · AI Search RAG · APIM gateway", done: false },
    { v: "v0.4", t: "Production polish", d: "Risk · Word/PPT · CI regression guards", done: false },
    { v: "v1.0", t: "Pilot ship", d: "Branding · onboarding · reference story", done: false },
  ];

  const trackY = 4.0;
  const lineX = 1.3, lineW = W - 2.6;
  // Track line
  s.addShape(pres.shapes.LINE, {
    x: lineX, y: trackY, w: lineW, h: 0,
    line: { color: ICE, width: 4 },
  });

  const stepW = lineW / (milestones.length - 1);
  milestones.forEach((m, i) => {
    const cx = lineX + i * stepW;
    const dotColor = m.done ? NAVY : ICE;
    const ringColor = m.current ? ACCENT : NAVY;

    // outer ring for current
    if (m.current) {
      s.addShape(pres.shapes.OVAL, {
        x: cx - 0.28, y: trackY - 0.28, w: 0.56, h: 0.56,
        fill: { color: ACCENT }, line: { type: "none" },
      });
    }
    s.addShape(pres.shapes.OVAL, {
      x: cx - 0.20, y: trackY - 0.20, w: 0.40, h: 0.40,
      fill: { color: dotColor }, line: { color: ringColor, width: 2 },
    });

    // Alternating above/below
    const above = i % 2 === 0;
    const ty = above ? trackY - 1.55 : trackY + 0.45;
    const labelW = 1.9;

    s.addText(m.v, {
      x: cx - labelW / 2, y: ty, w: labelW, h: 0.35,
      fontFace: "Calibri", fontSize: 14, bold: true, color: NAVY, align: "center", margin: 0,
    });
    s.addText(m.t, {
      x: cx - labelW / 2, y: ty + 0.34, w: labelW, h: 0.35,
      fontFace: "Georgia", fontSize: 13, bold: true, color: TEXT_DARK, align: "center", margin: 0,
    });
    s.addText(m.d, {
      x: cx - labelW / 2, y: ty + 0.7, w: labelW, h: 0.7,
      fontFace: "Calibri", fontSize: 10, color: TEXT_MUTED, align: "center", valign: "top", margin: 0,
    });
  });

  s.addText("YOU ARE HERE  →  v0.1", {
    x: MARGIN, y: 6.6, w: W - 2 * MARGIN, h: 0.45,
    fontFace: "Calibri", fontSize: 14, bold: true, color: ACCENT, align: "center", charSpacing: 6, margin: 0,
  });

  addFooter(s, 9);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 10 — Competitive position
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "COMPETITIVE POSITION", "How it relates to existing tooling.");

  const headers = [
    { text: "Capability", options: { bold: true, color: WHITE, fill: { color: NAVY }, align: "left", valign: "middle" } },
    { text: "Azure Migrate", options: { bold: true, color: WHITE, fill: { color: NAVY }, align: "center", valign: "middle" } },
    { text: "Manual assessment", options: { bold: true, color: WHITE, fill: { color: NAVY }, align: "center", valign: "middle" } },
    { text: "Modernization Assessor", options: { bold: true, color: WHITE, fill: { color: ACCENT }, align: "center", valign: "middle" } },
  ];
  const rowsData = [
    ["Inventory normalization", "Azure-side only", "Manual", "Multi-source via Discovery agent"],
    ["7 R's classification", "—", "Yes", "Yes (full model)"],
    ["Structured rationale per recommendation", "—", "Yes (verbose)", "Yes (auditable)"],
    ["Cost model", "Azure list price", "Custom", "Live Retail Pricing API (v0.2)"],
    ["WAF risk anchoring", "—", "Architect-dependent", "Yes (v0.4)"],
    ["Customer deliverable", "—", "Architect-built", "Draft Markdown / Word / PPT"],
    ["Time per assessment", "Hours (inv. only)", "2–3 weeks", "Hours, then human review"],
    ["Partner-resellable", "—", "—", "By design"],
  ];

  const tableData = [headers, ...rowsData.map((r) =>
    r.map((c, idx) => ({
      text: c,
      options: {
        fontSize: 11,
        color: TEXT_DARK,
        bold: idx === 3,
        align: idx === 0 ? "left" : "center",
        valign: "middle",
      },
    }))
  )];

  s.addTable(tableData, {
    x: MARGIN, y: 1.95, w: W - 2 * MARGIN,
    colW: [3.2, 2.7, 2.9, 3.43],
    rowH: 0.45,
    border: { type: "solid", color: "E5E8F0", pt: 1 },
    fontFace: "Calibri",
  });

  s.addText("Not a replacement for Azure Migrate — it consumes Azure Migrate output as one of its inputs.", {
    x: MARGIN, y: 6.7, w: W - 2 * MARGIN, h: 0.4,
    fontFace: "Calibri", fontSize: 13, italic: true, color: TEXT_MUTED, align: "center", margin: 0,
  });

  addFooter(s, 10);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 11 — Responsible AI
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  addContentHeader(s, "RESPONSIBLE AI", "Guardrails for high-stakes recommendations.");

  const items = [
    { t: "Human-in-the-loop, always", d: "No output is customer-ready without architect review. The summary header always carries a 'DRAFT — pending architect review' watermark." },
    { t: "Grounded rationale or rejection", d: "Every recommendation cites attributes from the input. Hallucination evaluators fail PRs where the rationale references data that isn't there." },
    { t: "Confidence-first triage", d: "Low-confidence items are surfaced to the architect first. Confidence calibration is itself an evaluator." },
    { t: "No PII in agent memory", d: "Cosmos DB holds reasoning, not PII. Sensitive fields tokenized at the gateway before reaching the model." },
    { t: "Customer data stays in partner tenant", d: "Single-tenant deployments. Model endpoint is the partner's own Foundry project. No data crosses tenant boundaries." },
    { t: "End-to-end auditability", d: "Every output traceable to the exact prompt + tool calls + token costs. Required for regulated industries." },
  ];

  const cardW = 6.05, cardH = 1.45;
  items.forEach((it, i) => {
    const col = i % 2, row = Math.floor(i / 2);
    const x = MARGIN + col * (cardW + 0.3);
    const y = 1.95 + row * (cardH + 0.15);

    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: cardW, h: cardH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x, y, w: 0.08, h: cardH, fill: { color: ACCENT }, line: { type: "none" },
    });
    s.addText(it.t, {
      x: x + 0.3, y: y + 0.15, w: cardW - 0.4, h: 0.4,
      fontFace: "Calibri", fontSize: 14, bold: true, color: NAVY, margin: 0,
    });
    s.addText(it.d, {
      x: x + 0.3, y: y + 0.55, w: cardW - 0.4, h: cardH - 0.6,
      fontFace: "Calibri", fontSize: 11, color: TEXT_DARK, valign: "top", margin: 0,
    });
  });

  addFooter(s, 11);
}

// ─────────────────────────────────────────────────────────────────────────────
// SLIDE 12 — Call to action
// ─────────────────────────────────────────────────────────────────────────────
{
  const s = pres.addSlide();
  s.background = { color: NAVY };
  s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 0.45, h: H, fill: { color: ICE }, line: { type: "none" } });
  s.addShape(pres.shapes.RECTANGLE, { x: 0.45, y: 0, w: 0.10, h: H, fill: { color: ACCENT }, line: { type: "none" } });

  s.addText("NEXT STEPS", {
    x: 1.1, y: 0.9, w: 11, h: 0.4,
    fontFace: "Calibri", fontSize: 12, color: ICE, bold: true, charSpacing: 8, margin: 0,
  });

  s.addText("Pick one partner. Ship v0.2. Win a reference.", {
    x: 1.1, y: 1.4, w: 11.5, h: 1.6,
    fontFace: "Georgia", fontSize: 44, color: WHITE, bold: true, margin: 0,
  });

  const cards = [
    { n: "01", t: "Validate with a partner", d: "Pick one of SoftwareOne, BraSoftware, FCAMARA, 7IT, Bioma. Run their last assessment through v0.2 and compare outputs." },
    { n: "02", t: "Ship v0.2 in 2 sprints", d: "Foundry agents wired, MAF HTTP host, Pricing MCP, golden eval set. Branded for the chosen partner." },
    { n: "03", t: "Land a reference customer", d: "Drive one partner-led pilot to a referenceable PoC. That's the proof point that turns this into a productized service." },
  ];
  const cardW = 3.8, cardH = 2.9, gap = 0.35;
  const totalW = cards.length * cardW + (cards.length - 1) * gap;
  let cx = (W - totalW) / 2;
  cards.forEach((c) => {
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: 3.4, w: cardW, h: cardH,
      fill: { color: WHITE }, line: { type: "none" }, shadow: makeShadow(),
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: 3.4, w: cardW, h: 0.08, fill: { color: ACCENT }, line: { type: "none" },
    });
    s.addText(c.n, {
      x: cx + 0.3, y: 3.6, w: cardW - 0.6, h: 0.7,
      fontFace: "Georgia", fontSize: 36, color: ICE, bold: true, margin: 0,
    });
    s.addText(c.t, {
      x: cx + 0.3, y: 4.35, w: cardW - 0.6, h: 0.5,
      fontFace: "Georgia", fontSize: 18, color: NAVY, bold: true, margin: 0,
    });
    s.addText(c.d, {
      x: cx + 0.3, y: 4.9, w: cardW - 0.6, h: cardH - 1.5,
      fontFace: "Calibri", fontSize: 13, color: TEXT_DARK, valign: "top", margin: 0,
    });
    cx += cardW + gap;
  });

  s.addText("github.com/fabio-padua/modernization-assessor", {
    x: 1.1, y: 6.7, w: 11, h: 0.4,
    fontFace: "Consolas", fontSize: 14, color: ICE, italic: true, margin: 0,
  });
}

pres.writeFile({ fileName: "docs/Modernization-Assessor-Defense.pptx" })
  .then((f) => console.log("Wrote", f))
  .catch((e) => { console.error(e); process.exit(1); });
