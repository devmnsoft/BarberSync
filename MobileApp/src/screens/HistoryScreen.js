import React, { useState } from 'react';
import { Pressable, ScrollView, Text, View } from 'react-native';

const events = [
  'PublicWeb criou solicitação PUB-2026-0001',
  'Check-in realizado no Totem KIOSK-DEMO-001',
  'Comanda CMD-1024 paga via PIX demo',
  'Recibo REC-20260605 emitido',
  'Cashback de R$ 5,50 gerado',
  'Avaliação NPS 10 publicada'
];

export default function HistoryScreen() {
  const [message, setMessage] = useState('Histórico sincronizado com o FullServiceFlow demo.');
  return (
    <ScrollView style={{ flex: 1, backgroundColor: '#f8fafc' }} contentContainerStyle={{ padding: 24 }}>
      <Text style={{ color: '#052e2b', fontSize: 28, fontWeight: '900' }}>Histórico 360º</Text>
      <Text style={{ color: '#64748b', marginTop: 8 }}>Linha do tempo mobile com agendamento, atendimento, pagamento, cashback e avaliação.</Text>
      {events.map((event, index) => (
        <View key={event} style={{ marginTop: 14, padding: 16, borderRadius: 18, backgroundColor: '#ffffff', borderLeftWidth: 5, borderLeftColor: index === events.length - 1 ? '#d4af37' : '#052e2b' }}>
          <Text style={{ color: '#0f172a', fontWeight: '900' }}>{index + 1}. {event}</Text>
        </View>
      ))}
      <Pressable accessibilityRole="button" onPress={() => setMessage('Recibo demo compartilhado por WhatsApp e e-mail.')} style={{ marginTop: 18, backgroundColor: '#d4af37', borderRadius: 16, padding: 16, alignItems: 'center' }}>
        <Text style={{ color: '#052e2b', fontWeight: '900' }}>Compartilhar recibo</Text>
      </Pressable>
      <Text style={{ color: '#16a34a', marginTop: 14, fontWeight: '800' }}>{message}</Text>
    </ScrollView>
  );
}
