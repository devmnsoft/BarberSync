import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

export function ProfileScreen() {
  return <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}><Text style={{ color: colors.dark, fontSize: 26, fontWeight: '900' }}>Perfil</Text><AppCard><View style={{ flexDirection:'row', alignItems:'center', gap: spacing.md }}><View style={{ width: 64, height: 64, borderRadius: 22, backgroundColor: colors.dark, alignItems:'center', justifyContent:'center' }}><Text style={{ color: colors.gold, fontSize: 24, fontWeight:'900' }}>M</Text></View><View><Text style={{ color: colors.text, fontSize: 20, fontWeight:'900' }}>Marcos Cliente</Text><Text style={{ color: colors.muted }}>VIP • aceita promoções • WhatsApp verificado</Text></View></View></AppCard><AppCard><Text style={{ color: colors.text, fontWeight:'900' }}>Preferências</Text><Text style={{ color: colors.muted, marginTop: 8 }}>Profissional: Rafael Barber • Serviço: Corte + Barba • Horário: final do dia.</Text></AppCard><AppButton title="Atualizar meus dados" /></ScrollView>;
}
