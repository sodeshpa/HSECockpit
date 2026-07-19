import { useExecutiveKPIs, useCriticalRisks, useComplianceSummary } from "../../hooks/useExecutive";
import { Shield, AlertTriangle, Activity, CheckCircle, XCircle, HelpCircle } from "lucide-react";
import { RadialBarChart, RadialBar, ResponsiveContainer, PolarAngleAxis } from "recharts";

function KPITile({ title, value, icon: Icon, color, suffix = "", qualityLabel }) {
  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-center gap-3">
        <div className={`rounded-full p-2 ${color}`}>
          <Icon className="h-5 w-5 text-white" aria-hidden="true" />
        </div>
        <h3 className="text-sm font-medium text-gray-600">{title}</h3>
      </div>
      <p className="mt-3 text-3xl font-bold text-gray-900" aria-label={`${title}: ${value}${suffix}`}>
        {value}{suffix}
      </p>
      {qualityLabel && qualityLabel !== "VALID" && (
        <span className="mt-1 inline-block rounded bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800">
          Data: {qualityLabel}
        </span>
      )}
    </article>
  );
}

function BarrierHealthGauge({ score }) {
  const getColor = (s) => s >= 70 ? "#22c55e" : s >= 40 ? "#f59e0b" : "#ef4444";
  const getBand = (s) => s >= 70 ? "Healthy" : s >= 40 ? "At Risk" : "Critical";
  const data = [{ name: "Health", value: score, fill: getColor(score) }];

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h3 className="text-sm font-medium text-gray-600">Barrier Health Score</h3>
      <div className="mt-2 flex items-center justify-center" aria-label={`Barrier health: ${score.toFixed(1)}% - ${getBand(score)}`}>
        <ResponsiveContainer width={200} height={200}>
          <RadialBarChart cx="50%" cy="50%" innerRadius="60%" outerRadius="90%" barSize={20} data={data} startAngle={180} endAngle={0}>
            <PolarAngleAxis type="number" domain={[0, 100]} angleAxisId={0} tick={false} />
            <RadialBar background clockWise dataKey="value" angleAxisId={0} />
          </RadialBarChart>
        </ResponsiveContainer>
      </div>
      <div className="text-center">
        <p className="text-2xl font-bold text-gray-900">{score.toFixed(1)}%</p>
        <p className={`text-sm font-medium ${score >= 70 ? "text-green-600" : score >= 40 ? "text-amber-600" : "text-red-600"}`}>
          {getBand(score)}
        </p>
      </div>
    </article>
  );
}

function CriticalRisksList({ risks }) {
  if (!risks || risks.length === 0) {
    return (
      <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h3 className="text-sm font-medium text-gray-600">Open Critical Risks</h3>
        <p className="mt-4 text-sm text-gray-500">No open critical risks</p>
      </article>
    );
  }

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h3 className="text-sm font-medium text-gray-600 mb-4">Open Critical Risks</h3>
      <ul className="space-y-3" aria-label="Critical risks list">
        {risks.map((risk, i) => (
          <li key={risk.riskId || i} className="flex items-start gap-3 rounded-md bg-red-50 p-3">
            <AlertTriangle className="h-4 w-4 mt-0.5 text-red-600 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-gray-900">{risk.description}</p>
              <p className="text-xs text-gray-500 mt-1">{risk.siteName} &bull; {risk.daysOpen} days open</p>
            </div>
          </li>
        ))}
      </ul>
    </article>
  );
}

function ComplianceSummary({ data }) {
  if (!data || data.length === 0) {
    return (
      <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h3 className="text-sm font-medium text-gray-600">Compliance Status</h3>
        <p className="mt-4 text-sm text-gray-500">No compliance data available</p>
      </article>
    );
  }

  const compliant = data.filter(s => s.status === "COMPLIANT").length;
  const nonCompliant = data.filter(s => s.status === "NON_COMPLIANT").length;
  const unknown = data.filter(s => s.status === "UNKNOWN").length;

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h3 className="text-sm font-medium text-gray-600 mb-4">Compliance Status</h3>
      <div className="grid grid-cols-3 gap-4 mb-4">
        <div className="text-center">
          <CheckCircle className="h-6 w-6 text-green-600 mx-auto" aria-hidden="true" />
          <p className="text-xl font-bold text-gray-900 mt-1">{compliant}</p>
          <p className="text-xs text-gray-500">Compliant</p>
        </div>
        <div className="text-center">
          <XCircle className="h-6 w-6 text-red-600 mx-auto" aria-hidden="true" />
          <p className="text-xl font-bold text-gray-900 mt-1">{nonCompliant}</p>
          <p className="text-xs text-gray-500">Non-Compliant</p>
        </div>
        <div className="text-center">
          <HelpCircle className="h-6 w-6 text-gray-400 mx-auto" aria-hidden="true" />
          <p className="text-xl font-bold text-gray-900 mt-1">{unknown}</p>
          <p className="text-xs text-gray-500">Unknown</p>
        </div>
      </div>
      <ul className="space-y-2">
        {data.map((site) => (
          <li key={site.siteId} className="flex items-center justify-between text-sm">
            <span className="text-gray-700">{site.siteName}</span>
            <span className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium ${
              site.status === "COMPLIANT" ? "bg-green-100 text-green-800" :
              site.status === "NON_COMPLIANT" ? "bg-red-100 text-red-800" :
              "bg-gray-100 text-gray-600"
            }`}>
              {site.status === "COMPLIANT" ? "Compliant" : site.status === "NON_COMPLIANT" ? "Non-Compliant" : "Unknown"}
            </span>
          </li>
        ))}
      </ul>
    </article>
  );
}

export function ExecutiveCockpit() {
  const { data: kpis, isLoading: kpisLoading } = useExecutiveKPIs();
  const { data: risks, isLoading: risksLoading } = useCriticalRisks();
  const { data: compliance, isLoading: complianceLoading } = useComplianceSummary();

  if (kpisLoading || risksLoading || complianceLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <span className="text-gray-500">Loading executive dashboard...</span>
      </div>
    );
  }

  return (
    <section className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8" aria-label="Executive Cockpit">
      <header className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Executive HSE Cockpit</h1>
        <p className="mt-1 text-sm text-gray-600">Portfolio-level HSE posture at a glance.</p>
      </header>

      {/* KPI Tiles */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4 mb-6">
        <KPITile title="Barrier Health" value={kpis?.barrierHealthScore?.toFixed(1) || "N/A"} suffix="%" icon={Shield} color="bg-green-600" qualityLabel={kpis?.dataQualityStatus} />
        <KPITile title="Open Critical Risks" value={kpis?.openCriticalRisks || 0} icon={AlertTriangle} color="bg-red-600" />
        <KPITile title="Incidents MTD" value={kpis?.incidentCountMTD || 0} icon={Activity} color="bg-orange-500" />
        <KPITile title="Compliance Rate" value={kpis?.complianceRate?.toFixed(1) || "N/A"} suffix="%" icon={CheckCircle} color="bg-blue-600" />
      </div>

      {/* Detail Grid */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <BarrierHealthGauge score={kpis?.barrierHealthScore || 0} />
        <CriticalRisksList risks={risks} />
        <ComplianceSummary data={compliance} />
      </div>
    </section>
  );
}
