import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

export function HomeScreen() {
  return <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}>
    <Text style={{ color: colors.dark, fontSize: 28, fontWeight: '900' }}>BarberSync</Text>
    <AppCard accent><Text style={{ color: colors.gold, fontWeight: '900' }}>Barbearia Elite Demo</Text><Text style={{ color: colors.white, fontSize: 24, fontWeight: '900', marginTop: 8 }}>Seu próximo atendimento premium em poucos toques.</Text><Text style={{ color: colors.blueSoft, marginTop: 8 }}>Corte + barba hoje às 18:30 com Rafael Barber.</Text></AppCard>
    <View style={{ flexDirection: 'row', gap: spacing.sm }}><AppButton title="Agendar" variant="gold"/><AppButton title="Serviços" variant="ghost"/></View>
    <AppCard><Text style={{ color: colors.text, fontSize: 18, fontWeight: '900' }}>Resumo</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Cashback disponível: R$ 32,50 • 4 visitas no mês • avaliação média 4,9.</Text></AppCard>
  </ScrollView>;
}
