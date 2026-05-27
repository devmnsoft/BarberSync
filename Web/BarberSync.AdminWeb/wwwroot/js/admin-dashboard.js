document.addEventListener("DOMContentLoaded", async function () {
    const fallbackKpis = [
        { label: "Receita hoje", value: "R$ 4.850,00", icon: "💰", variation: "+12%" },
        { label: "Receita mês", value: "R$ 92.340,00", icon: "📈", variation: "+18%" },
        { label: "Agendamentos hoje", value: "26", icon: "📅", variation: "+7%" },
        { label: "Clientes ativos", value: "412", icon: "👥", variation: "+22" },
        { label: "Atendimentos em andamento", value: "4", icon: "✂️", variation: "Agora" },
        { label: "Comandas abertas", value: "7", icon: "🧾", variation: "Agora" },
        { label: "Ticket médio", value: "R$ 126,00", icon: "🎯", variation: "+9%" },
        { label: "Estoque crítico", value: "8", icon: "📦", variation: "Atenção" },
        { label: "Avaliação média", value: "4.8", icon: "⭐", variation: "Excelente" },
        { label: "Totem online", value: "2", icon: "🖥️", variation: "Online" },
        { label: "Campanhas ativas", value: "5", icon: "🎯", variation: "Ativas" },
        { label: "Profissionais disponíveis", value: "6", icon: "💈", variation: "Hoje" }
    ];

    const container = document.querySelector("[data-dashboard-kpis]");
    if (!container) {
        console.warn("Container [data-dashboard-kpis] não encontrado.");
        return;
    }

    let kpis = Array.isArray(window.dashboardKpis) ? window.dashboardKpis : null;

    if (!kpis) {
        try {
            if (window.adminApiClient?.get) {
                const demoSummary = {
                    revenueToday: 4850,
                    revenueMonth: 92340,
                    appointmentsToday: 26,
                    activeClients: 412,
                    ongoing: 4,
                    openOrders: 7,
                    avgTicket: 126,
                    stockCritical: 8,
                    rating: 4.8,
                    kioskOnline: 2,
                    activeCampaigns: 5,
                    availableProfessionals: 6
                };

                const { data } = await window.adminApiClient.get("/AdminApi/dashboard", demoSummary);
                kpis = [
                    { label: "Receita hoje", value: `R$ ${Number(data?.revenueToday ?? 4850).toLocaleString("pt-BR", { minimumFractionDigits: 2 })}`, icon: "💰", variation: "+12%" },
                    { label: "Receita mês", value: `R$ ${Number(data?.revenueMonth ?? 92340).toLocaleString("pt-BR", { minimumFractionDigits: 2 })}`, icon: "📈", variation: "+18%" },
                    { label: "Agendamentos hoje", value: String(data?.appointmentsToday ?? 26), icon: "📅", variation: "+7%" },
                    { label: "Clientes ativos", value: String(data?.activeClients ?? 412), icon: "👥", variation: "+22" },
                    { label: "Atendimentos em andamento", value: String(data?.ongoing ?? 4), icon: "✂️", variation: "Agora" },
                    { label: "Comandas abertas", value: String(data?.openOrders ?? 7), icon: "🧾", variation: "Agora" },
                    { label: "Ticket médio", value: `R$ ${Number(data?.avgTicket ?? 126).toLocaleString("pt-BR", { minimumFractionDigits: 2 })}`, icon: "🎯", variation: "+9%" },
                    { label: "Estoque crítico", value: String(data?.stockCritical ?? 8), icon: "📦", variation: "Atenção" },
                    { label: "Avaliação média", value: String(data?.rating ?? 4.8), icon: "⭐", variation: "Excelente" },
                    { label: "Totem online", value: String(data?.kioskOnline ?? 2), icon: "🖥️", variation: "Online" },
                    { label: "Campanhas ativas", value: String(data?.activeCampaigns ?? 5), icon: "🎯", variation: "Ativas" },
                    { label: "Profissionais disponíveis", value: String(data?.availableProfessionals ?? 6), icon: "💈", variation: "Hoje" }
                ];
            }
        } catch (error) {
            console.warn("Falha ao carregar KPIs da API. Usando fallback.", error);
        }
    }

    kpis = Array.isArray(kpis) && kpis.length ? kpis : fallbackKpis;

    container.innerHTML = kpis.map(kpi => `
        <article class="kpi-card">
            <div class="kpi-icon">${kpi.icon ?? "📊"}</div>
            <div>
                <p class="kpi-label">${kpi.label ?? "Indicador"}</p>
                <strong class="kpi-value">${kpi.value ?? "-"}</strong>
                <span class="kpi-variation">${kpi.variation ?? ""}</span>
            </div>
        </article>
    `).join("");
});
