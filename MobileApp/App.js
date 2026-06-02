import React from 'react';
import { ScrollView, Text, View } from 'react-native';

const colors = { dark:'#0B1220', gold:'#D4A72C', bg:'#F3F4F6', card:'#FFFFFF', text:'#111827', muted:'#6B7280', blue:'#2563EB', green:'#16A34A' };
const Card = ({ children }) => <View style={{ backgroundColor: colors.card, borderRadius: 22, padding: 18, marginBottom: 14, shadowColor: colors.dark, shadowOpacity: 0.08, shadowRadius: 18, elevation: 3 }}>{children}</View>;

export default function App() {
  return (
    <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: 22, paddingTop: 58 }}>
      <Text style={{ color: colors.dark, fontSize: 30, fontWeight: '900' }}>BarberSync</Text>
      <Text style={{ color: colors.muted, marginTop: 6, marginBottom: 18 }}>Mobile premium para clientes: agenda, cashback e histórico.</Text>
      <Card><Text style={{ fontSize: 20, fontWeight: '800' }}>Olá, Cliente Demo 👋</Text><Text style={{ color: colors.muted, marginTop: 6 }}>Seu próximo atendimento é hoje às 18:30 com Rafael Barber.</Text><Text style={{ color: colors.gold, marginTop: 8, fontWeight: '800' }}>Chegue 5 minutos antes e ganhe cashback extra.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '800' }}>Serviços em destaque</Text><Text style={{ marginTop: 10 }}>✂️ Corte Masculino • R$ 45</Text><Text>💈 Corte + Barba • R$ 70</Text><Text>🧴 Hidratação Premium • R$ 55</Text></Card><Card><Text style={{ fontSize: 18, fontWeight: '800' }}>Promoções</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Cupom RETORNO20 para clientes com mais de 30 dias sem visita.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '800' }}>Cashback</Text><Text style={{ color: colors.green, fontSize: 28, fontWeight: '900', marginTop: 8 }}>R$ 38,50</Text><Text style={{ color: colors.muted }}>Disponível para o próximo agendamento.</Text></Card>
      <View style={{ backgroundColor: colors.blue, borderRadius: 18, padding: 16, alignItems: 'center' }}><Text style={{ color: 'white', fontWeight: '900' }}>Agendar agora</Text></View>
    </ScrollView>
  );
}
