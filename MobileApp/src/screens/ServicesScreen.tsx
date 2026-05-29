import React from 'react';
import { Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

export function ServicesScreen() {
  return <View style={{ flex: 1, backgroundColor: colors.bg, padding: spacing.lg, gap: spacing.md }}>
    <Text style={{ color: colors.dark, fontSize: 26, fontWeight: '900' }}>BarberSync</Text>
    <AppCard><Text style={{ fontSize: 20, fontWeight: '800', color: colors.text }}>ServicesScreen</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Serviços, agendamentos, cashback e perfil com identidade visual BarberSync 2.0.</Text></AppCard>
    <AppButton title="Ver próximos horários" />
  </View>;
}
