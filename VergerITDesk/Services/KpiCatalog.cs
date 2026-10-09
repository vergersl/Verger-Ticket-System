using VergerITDesk.Models;

namespace VergerITDesk.Services;

/// <summary>The 20 Digital &amp; IT KPIs from the master sheet. Seeded into the database on startup;
/// this list is the source of truth if you need to re-seed or add a KPI later.</summary>
public static class KpiCatalog
{
    public static readonly List<KpiDefinition> Definitions = new()
    {
        new() { Ref = "IT.9.1", Name = "ERP transformation milestones delivered on time", Csf = "Projects", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 20, Target = 0.900m, Formula = "Milestones completed by due date / milestones due × 100" },
        new() { Ref = "IT.9.2", Name = "ERP master and transactional data quality", Csf = "Projects", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 20, Target = 0.950m, Formula = "Valid records passing completeness, accuracy and duplicate checks / records tested × 100" },
        new() { Ref = "IT.9.3", Name = "ERP active-user adoption", Csf = "Projects", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 15, Target = 0.850m, Formula = "Licensed in-scope users completing defined core transactions / in-scope users × 100" },
        new() { Ref = "IT.3.1", Name = "Digital automation benefit realization", Csf = "Net Profit", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 15, Target = 0.900m, Formula = "Verified annualized benefit realized / approved benefit target × 100" },
        new() { Ref = "IT.3.2", Name = "External ERP and IT support spend adherence", Csf = "Net Profit", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 15, Target = 0.900m, Formula = "IF actual spend ≤ budget, 100%; otherwise budget / actual spend × 100" },
        new() { Ref = "IT.9.11", Name = "ERP report customizations delivered on time and accepted", Csf = "Projects", Owner = "Radika", Role = "ERP & Digital Transformation Lead", Weight = 15, Target = 0.900m, Formula = "ERP report customization requests deployed by agreed due date and accepted in UAT / customization requests due × 100" },

        new() { Ref = "IT.9.4", Name = "BI dashboards delivered on time", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 20, Target = 0.900m, Formula = "Dashboards accepted by agreed due date / dashboards due × 100" },
        new() { Ref = "IT.9.5", Name = "Dashboard first-pass UAT acceptance", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 15, Target = 0.950m, Formula = "Dashboards accepted in first UAT cycle / dashboards submitted to UAT × 100" },
        new() { Ref = "IT.9.6", Name = "Dashboard active-user adoption", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 15, Target = 0.800m, Formula = "Monthly active intended users / intended users with access × 100" },
        new() { Ref = "IT.9.7", Name = "Dashboard enhancements completed within SLA", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 15, Target = 0.900m, Formula = "Enhancement requests completed within agreed SLA / requests due × 100" },
        new() { Ref = "IT.9.8", Name = "Published report data accuracy", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 15, Target = 0.980m, Formula = "Validated report checks passed / total validation checks performed × 100" },
        new() { Ref = "IT.9.9", Name = "Analytics stakeholder satisfaction", Csf = "Projects", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 10, Target = 0.850m, Formula = "Positive survey responses (4 or 5) / total valid responses × 100" },
        new() { Ref = "IT.3.3", Name = "Cost savings from BI/AI enhancements", Csf = "Net Profit", Owner = "Amila", Role = "BI & Automation Analyst", Weight = 10, Target = 0.900m, Formula = "Finance-validated annualized cost savings realized from deployed BI/AI enhancements / approved annualized savings target × 100" },

        new() { Ref = "IT.3.4", Name = "Critical IT service availability", Csf = "Net Profit", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 20, Target = 0.995m, Formula = "(Agreed service minutes − unplanned outage minutes) / agreed service minutes × 100" },
        new() { Ref = "IT.3.5", Name = "Service desk tickets resolved within SLA", Csf = "Net Profit", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 15, Target = 0.900m, Formula = "Tickets resolved within priority SLA / tickets resolved × 100", ComputedType = "sla" },
        new() { Ref = "IT.3.6", Name = "First-contact resolution rate", Csf = "Net Profit", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 10, Target = 0.750m, Formula = "Tickets resolved at first contact without reassignment or reopen / eligible tickets × 100", ComputedType = "fcr" },
        new() { Ref = "IT.8.1", Name = "Critical security patch compliance", Csf = "ESG", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 15, Target = 0.950m, Formula = "In-scope assets patched within approved window / in-scope assets × 100" },
        new() { Ref = "IT.8.2", Name = "Backup restore-test success", Csf = "ESG", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 15, Target = 1.000m, Formula = "Successful restore tests meeting RTO and data-integrity checks / restore tests scheduled × 100" },
        new() { Ref = "IT.9.10", Name = "IT infrastructure and CCTV milestones delivered on time", Csf = "Projects", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 15, Target = 0.900m, Formula = "Accepted milestones completed by due date / milestones due × 100" },
        new() { Ref = "IT.3.7", Name = "IT operating and procurement budget adherence", Csf = "Net Profit", Owner = "Dilan", Role = "Senior Executive - IT", Weight = 10, Target = 0.950m, Formula = "IF actual YTD spend ≤ budget YTD, 100%; otherwise budget YTD / actual YTD spend × 100" },
    };
}

