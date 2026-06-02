import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { colors } from './src/theme/colors';
import { radius, spacing } from './src/theme/spacing';

const Card = ({ children, accent }) => (
  <View style={{ backgroundColor: colors.card, borderRadius: radius.lg, padding: spacing.lg, marginBottom: spacing.md, borderLeftWidth: accent ? 5 : 0, borderLeftColor: accent || colors.gold, shadowColor: colors.ink, shadowOpacity: 0.08, shadowRadius: 18, elevation: 3 }}>{children}</View>
);

const Pill = ({ children }) => <Text style={{ backgroundColor: colors.goldSoft, color: colors.primary, borderRadius: radius.pill, paddingHorizontal: 10, paddingVertical: 5, marginRight: 6, marginTop: 8, fontWeight: '800' }}>{children}</Text>;

export default function App() {
  return (
    <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.xl, paddingTop: 58 }}>
      <Text style={{ color: colors.primary, fontSize: 32, fontWeight: '900' }}>BarberSync</Text>
      <Text style={{ color: colors.muted, marginTop: 6, marginBottom: 18 }}>Mobile premium para clientes: home, serviços, agenda, cashback, promoções e perfil.</Text>
      <Card accent={colors.gold}><Text style={{ fontSize: 20, fontWeight: '900', color: colors.ink }}>Olá, Cliente Demo 👋</Text><Text style={{ color: colors.muted, marginTop: 6 }}>Seu próximo atendimento é hoje às 18:30 com Rafael Barber.</Text><Text style={{ color: colors.gold, marginTop: 8, fontWeight: '900' }}>Chegue 5 minutos antes e ganhe cashback extra.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Serviços em destaque</Text><View style={{ flexDirection: 'row', flexWrap: 'wrap' }}><Pill>✂️ Corte R$ 45</Pill><Pill>💈 Combo R$ 70</Pill><Pill>🧴 Hidratação R$ 55</Pill></View></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Agendamentos</Text><Text style={{ marginTop: 10, color: colors.slate }}>Hoje 18:30 • Corte + Barba • Rafael Barber</Text><Text style={{ color: colors.muted }}>Status: confirmado • Check-in disponível no Totem.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Cashback</Text><Text style={{ color: colors.green, fontSize: 28, fontWeight: '900', marginTop: 8 }}>R$ 38,50</Text><Text style={{ color: colors.muted }}>Disponível para o próximo agendamento.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Promoções</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Cupom RETORNO20 para clientes com mais de 30 dias sem visita.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Perfil</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Preferência: Corte + Barba • Profissional favorito: Rafael Barber • NPS: promotor.</Text></Card>
      <View style={{ backgroundColor: colors.primary, borderRadius: radius.lg, padding: spacing.lg, alignItems: 'center' }}><Text style={{ color: colors.white, fontWeight: '900' }}>Agendar agora</Text></View>
    </ScrollView>
  );
}
