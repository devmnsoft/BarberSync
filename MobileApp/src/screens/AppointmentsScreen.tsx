import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

export function AppointmentsScreen() {
  return <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}><Text style={{ color: colors.dark, fontSize: 26, fontWeight: '900' }}>Agendamentos</Text><AppCard accent><Text style={{ color: colors.gold, fontWeight: '900' }}>Hoje • 18:30</Text><Text style={{ color: colors.white, fontSize: 22, fontWeight: '900', marginTop: 8 }}>Corte + Barba</Text><Text style={{ color: colors.blueSoft, marginTop: 6 }}>Rafael Barber • Confirmado</Text></AppCard><View style={{ flexDirection:'row', gap: spacing.sm }}><AppButton title="Check-in" variant="gold"/><AppButton title="Remarcar" variant="ghost"/></View><AppCard><Text style={{ color: colors.text, fontWeight: '900' }}>Histórico recente</Text><Text style={{ color: colors.muted, marginTop: 8 }}>3 atendimentos concluídos, ticket médio R$ 72 e avaliação enviada.</Text></AppCard></ScrollView>;
}
