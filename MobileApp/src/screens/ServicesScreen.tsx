import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

const services = [{ name: 'Corte Masculino', price: 'R$ 45', time: '40 min' }, { name: 'Barba Tradicional', price: 'R$ 35', time: '30 min' }, { name: 'Combo Corte + Barba', price: 'R$ 70', time: '60 min' }];
export function ServicesScreen() {
  return <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}><Text style={{ color: colors.dark, fontSize: 26, fontWeight: '900' }}>Serviços</Text>{services.map(service => <AppCard key={service.name}><View style={{ flexDirection:'row', justifyContent:'space-between', alignItems:'center' }}><View><Text style={{ fontSize: 18, fontWeight: '900', color: colors.text }}>{service.name}</Text><Text style={{ color: colors.muted, marginTop: 4 }}>{service.time} • disponível no app, site e totem</Text></View><Text style={{ color: colors.gold, fontWeight: '900', fontSize: 18 }}>{service.price}</Text></View><View style={{ marginTop: spacing.md }}><AppButton title="Agendar este serviço" /></View></AppCard>)}</ScrollView>;
}
