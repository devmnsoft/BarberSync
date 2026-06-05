import React, { useState } from 'react';
import { Pressable, ScrollView, Text, View } from 'react-native';

const badge = {
  paddingHorizontal: 12,
  paddingVertical: 6,
  borderRadius: 999,
  fontWeight: '700',
  marginTop: 8,
  alignSelf: 'flex-start'
};

const slots = ['Hoje 18:30 • Rafael Barber', 'Amanhã 10:00 • Camila Beauty', 'Sábado 09:40 • Lucas Navalha'];

export default function ScheduleScreen() {
  const [selected, setSelected] = useState(slots[0]);
  const [status, setStatus] = useState('Escolha um horário para criar o agendamento demo.');
  return (
    <ScrollView style={{ flex: 1, backgroundColor: '#0f172a' }} contentContainerStyle={{ padding: 22 }}>
      <Text style={{ color: '#fff', fontSize: 28, fontWeight: '900' }}>Agenda Inteligente</Text>
      <Text style={{ color: '#cbd5e1', marginTop: 8, lineHeight: 22 }}>
        Horários sugeridos por IA com serviço, profissional, duração prevista, check-in no Totem e cashback planejado.
      </Text>

      <View style={{ marginTop: 20, padding: 16, borderRadius: 18, backgroundColor: '#1e293b' }}>
        <Text style={{ color: '#fff', fontSize: 16, fontWeight: '800' }}>Serviço selecionado</Text>
        <Text style={{ color: '#94a3b8', marginTop: 6 }}>Combo Corte + Barba • 70 min • R$ 75,00</Text>
        <Text style={{ ...badge, backgroundColor: '#14532d', color: '#bbf7d0' }}>Cashback previsto: R$ 3,75</Text>
      </View>

      {slots.map((slot) => (
        <Pressable key={slot} accessibilityRole="button" onPress={() => { setSelected(slot); setStatus(`Horário selecionado: ${slot}.`); }} style={{ marginTop: 14, padding: 16, borderRadius: 16, backgroundColor: selected === slot ? '#d4af37' : '#1e293b' }}>
          <Text style={{ color: selected === slot ? '#052e2b' : '#fff', fontWeight: '900' }}>{slot}</Text>
          <Text style={{ color: selected === slot ? '#052e2b' : '#94a3b8', marginTop: 4 }}>Check-in liberado 10 min antes no Totem.</Text>
        </Pressable>
      ))}

      <Pressable accessibilityRole="button" onPress={() => setStatus(`Agendamento demo confirmado para ${selected}. Dashboard, Cliente 360 e Totem serão atualizados.`)} style={{ marginTop: 18, backgroundColor: '#16a34a', borderRadius: 16, padding: 16, alignItems: 'center' }}>
        <Text style={{ color: '#ffffff', fontWeight: '900' }}>Confirmar agendamento</Text>
      </Pressable>
      <Text style={{ color: '#bbf7d0', marginTop: 14, fontWeight: '800' }}>{status}</Text>
    </ScrollView>
  );
}
