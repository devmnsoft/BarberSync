import React from 'react';
import { View, Text } from 'react-native';

export default function App() {
  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', padding: 20, gap: 12 }}>
      <Text style={{ fontSize: 22, fontWeight: '700' }}>BarberSync Mobile Gamificado</Text>
      <Text>⭐ Programa de pontos e cashback em tempo real</Text>
      <Text>📅 Agenda inteligente + cobrança automática</Text>
      <Text>🔔 Push, WhatsApp e Telegram com gatilhos de eventos</Text>
    </View>
  );
}
