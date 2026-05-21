import React from 'react';

const card = { padding: 16, borderRadius: 12, background: '#1f2937', minWidth: 260 };

export default function App() {
  return (
    <main style={{ minHeight: '100vh', padding: 30, background: '#111827', color: '#fff' }}>
      <h1>Totem Inteligente BarberSync</h1>
      <p>Check-in gamificado, reconhecimento por vídeo e recomendações de upsell em tempo real.</p>

      <section style={{ display: 'flex', gap: 14, flexWrap: 'wrap', marginTop: 20 }}>
        <article style={card}>
          <h3>Fila preditiva</h3>
          <p>Tempo estimado: <strong style={{ color: '#34d399' }}>12 min</strong></p>
        </article>
        <article style={card}>
          <h3>Recompensa do dia</h3>
          <p>+80 pontos ao confirmar presença no totem.</p>
        </article>
        <article style={card}>
          <h3>Notificações ao vivo</h3>
          <p>Estoque crítico: 3 • Novos clientes hoje: 14</p>
        </article>
      </section>
    </main>
  );
}
