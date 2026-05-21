import React from 'react';
import { View, Text } from 'react-native';

const badge = {
  paddingHorizontal: 12,
  paddingVertical: 6,
  borderRadius: 999,
  fontWeight: '600',
  marginTop: 8
};

export default function ScheduleScreen() {
  return (
    <View style={{ flex: 1, padding: 20, backgroundColor: '#0f172a' }}>
      <Text style={{ color: '#fff', fontSize: 24, fontWeight: '700' }}>Agenda Inteligente</Text>
      <Text style={{ color: '#cbd5e1', marginTop: 8 }}>
        Horário sugerido por IA: 14:00 • Profissional ideal: Camila • Duração prevista: 50 min
      </Text>

      <View style={{ marginTop: 20, padding: 14, borderRadius: 10, backgroundColor: '#1e293b' }}>
        <Text style={{ color: '#fff', fontSize: 16, fontWeight: '600' }}>Gamificação & Fidelidade</Text>
        <Text style={{ color: '#94a3b8', marginTop: 6 }}>Você está no nível Gold com 1.240 pontos.</Text>
        <Text style={{ ...badge, backgroundColor: '#14532d', color: '#bbf7d0' }}>Cashback disponível: R$ 18,40</Text>
      </View>

      <View style={{ marginTop: 20, padding: 14, borderRadius: 10, backgroundColor: '#1e293b' }}>
        <Text style={{ color: '#fff', fontSize: 16, fontWeight: '600' }}>Upsell recomendado</Text>
        <Text style={{ color: '#94a3b8', marginTop: 6 }}>+ Hidratação Premium (74% de aderência)</Text>
      </View>
    </View>
  );
}
