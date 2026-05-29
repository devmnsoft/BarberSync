import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

export function CashbackScreen() {
  return <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}><Text style={{ color: colors.dark, fontSize: 26, fontWeight: '900' }}>Cashback</Text><AppCard accent><Text style={{ color: colors.gold, fontWeight: '900' }}>Saldo disponível</Text><Text style={{ color: colors.white, fontSize: 36, fontWeight: '900', marginTop: 8 }}>R$ 32,50</Text><Text style={{ color: colors.blueSoft, marginTop: 6 }}>Clube Gold • expira em 45 dias</Text></AppCard><AppButton title="Usar no próximo serviço" variant="gold"/><AppCard><Text style={{ color: colors.text, fontWeight: '900' }}>Extrato</Text><Text style={{ color: colors.muted, marginTop: 8 }}>+ R$ 7,00 Combo Corte + Barba • + R$ 4,50 Barba Tradicional.</Text></AppCard></ScrollView>;
}
