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
      <Card accent={colors.primary}><Text style={{ color: colors.primary, fontSize: 14, fontWeight: '900' }}>LOGIN DEMO VISUAL</Text><Text style={{ color: colors.ink, fontSize: 22, fontWeight: '900', marginTop: 4 }}>Cliente Demo autenticado</Text><Text style={{ color: colors.muted, marginTop: 6 }}>Acesso rápido por telefone, biometria mock e token de fidelidade local.</Text></Card>
      <Text style={{ color: colors.primary, fontSize: 32, fontWeight: '900' }}>BarberSync</Text>
      <Card accent={colors.gold}><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Canal comercial SaaS</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Home • Serviços • Agendar • Meus agendamentos • Cashback • Cupons • Promoções • Avaliações • Perfil • Notificações.</Text><View style={{ flexDirection: 'row', flexWrap: 'wrap' }}><Pill>Home</Pill><Pill>Serviços</Pill><Pill>Agendar</Pill><Pill>Cashback</Pill><Pill>Cupons</Pill><Pill>Perfil</Pill></View></Card>
      <Text style={{ color: colors.muted, marginTop: 6, marginBottom: 18 }}>Mobile premium coerente com canais: serviços publicados, promoções, cashback, agendamentos, perfil e notificações demo.</Text>
      <Card accent={colors.gold}><Text style={{ fontSize: 20, fontWeight: '900', color: colors.ink }}>Olá, Cliente Demo 👋</Text><Text style={{ color: colors.muted, marginTop: 6 }}>Seu próximo atendimento é hoje às 18:30 com Rafael Barber.</Text><Text style={{ color: colors.gold, marginTop: 8, fontWeight: '900' }}>Chegue 5 minutos antes e ganhe cashback extra.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Serviços publicados no Mobile</Text><Text style={{ color: colors.muted, marginTop: 6 }}>Mesma curadoria visual do Admin 10.0, com toggles de canal simulados.</Text><View style={{ flexDirection: 'row', flexWrap: 'wrap' }}><Pill>✂️ Corte R$ 45</Pill><Pill>💈 Combo R$ 78</Pill><Pill>🧴 Hidratação R$ 89</Pill></View></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Agendamentos</Text><Text style={{ marginTop: 10, color: colors.slate }}>Hoje 18:30 • Corte + Barba • Rafael Barber</Text><Text style={{ color: colors.muted }}>Status: confirmado • Check-in disponível no Totem.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Cashback</Text><Text style={{ color: colors.green, fontSize: 28, fontWeight: '900', marginTop: 8 }}>R$ 38,50</Text><Text style={{ color: colors.muted }}>Disponível para o próximo agendamento.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Promoções publicadas</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Cupom RETORNO20 e BEMVINDO15 com validade e segmentação demo configuráveis no Admin.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Notificações demo</Text><Text style={{ color: colors.muted, marginTop: 8 }}>🔔 Agendamento confirmado • 💸 Cashback disponível • 🎁 Promoção ativa • 🖥️ Check-in no Totem liberado.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Histórico de serviços</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Últimos atendimentos: Corte + Barba, Hidratação e Barboterapia com recibos demo locais.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Avaliação</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Avalie seu último atendimento com NPS, comentário e estrelas em modo demo.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Perfil</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Preferência: Corte + Barba • Profissional favorito: Rafael Barber • NPS: promotor.</Text></Card>
      <Card><Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>Dados demo locais</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Serviços, agendamento, meus agendamentos, cashback, promoções e perfil rodam sem dependência externa para apresentação.</Text></Card>
      <View style={{ backgroundColor: colors.primary, borderRadius: radius.lg, padding: spacing.lg, alignItems: 'center' }}><Text style={{ color: colors.white, fontWeight: '900' }}>Agendar agora</Text></View>
    </ScrollView>
  );
}
