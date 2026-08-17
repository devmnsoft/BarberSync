import React, { useCallback, useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { colors } from './src/theme/colors';
import { radius, spacing } from './src/theme/spacing';
import { mobileApi } from './src/services/api';

const Card = ({ children, accent }) => (
  <View style={{ backgroundColor: colors.card, borderRadius: radius.lg, padding: spacing.lg, marginBottom: spacing.md, borderLeftWidth: accent ? 5 : 0, borderLeftColor: accent || colors.gold, shadowColor: colors.ink, shadowOpacity: 0.08, shadowRadius: 18, elevation: 3 }}>{children}</View>
);
const Title = ({ children }) => <Text style={{ fontSize: 18, fontWeight: '900', color: colors.ink }}>{children}</Text>;
const Empty = ({ children }) => <Text style={{ color: colors.muted, marginTop: spacing.sm }}>{children}</Text>;

export default function App() {
  const [snapshot, setSnapshot] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async (signal) => {
    setLoading(true);
    setError(null);
    try {
      setSnapshot(await mobileApi.getMobileSummary(signal));
    } catch (requestError) {
      if (requestError?.name !== 'AbortError') setError(requestError);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    load(controller.signal);
    return () => controller.abort();
  }, [load]);

  if (loading && !snapshot) return (
    <View accessibilityRole="progressbar" accessibilityLabel="Carregando dados" style={{ flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: colors.bg }}>
      <ActivityIndicator size="large" color={colors.primary} /><Text style={{ color: colors.muted, marginTop: spacing.md }}>Carregando sua experiência BarberSync…</Text>
    </View>
  );

  if (error && !snapshot) return (
    <View style={{ flex: 1, justifyContent: 'center', padding: spacing.xl, backgroundColor: colors.bg }}>
      <Card accent={colors.primary}><Title>Não foi possível carregar seus dados</Title><Empty>{error.message}</Empty>{error.traceId ? <Text selectable style={{ color: colors.muted, marginTop: spacing.sm }}>Código de suporte: {error.traceId}</Text> : null}</Card>
      <Pressable accessibilityRole="button" onPress={() => load()} style={{ backgroundColor: colors.primary, borderRadius: radius.lg, padding: spacing.lg, alignItems: 'center' }}><Text style={{ color: colors.white, fontWeight: '900' }}>Tentar novamente</Text></Pressable>
    </View>
  );

  const appointments = snapshot?.appointments ?? [];
  const loyalty = snapshot?.loyalty?.[0];
  const coupons = snapshot?.coupons ?? [];
  const notifications = snapshot?.notifications ?? [];
  return (
    <ScrollView refreshControl={<RefreshControl refreshing={loading} onRefresh={() => load()} tintColor={colors.primary} />} style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.xl, paddingTop: 58 }}>
      <Text style={{ color: colors.primary, fontSize: 32, fontWeight: '900', marginBottom: spacing.lg }}>BarberSync</Text>
      {error ? <Card accent={colors.gold}><Title>Atualização indisponível</Title><Empty>{error.message} Os últimos dados carregados continuam visíveis.</Empty></Card> : null}
      <Card accent={colors.gold}><Title>Próximo atendimento</Title>{appointments[0] ? <><Text style={{ marginTop: 10, color: colors.slate }}>{appointments[0].time} • {appointments[0].serviceName}</Text><Empty>{appointments[0].professionalName} • {appointments[0].status}</Empty></> : <Empty>Você não possui atendimentos agendados.</Empty>}</Card>
      <Card><Title>Meus agendamentos</Title>{appointments.length ? appointments.map(item => <View key={item.id} style={{ marginTop: spacing.md }}><Text style={{ color: colors.slate, fontWeight: '800' }}>{item.serviceName}</Text><Empty>{item.time} • {item.professionalName} • {item.status}</Empty></View>) : <Empty>Nenhum agendamento encontrado.</Empty>}</Card>
      <Card><Title>Cashback e pontos</Title>{loyalty ? <><Text style={{ color: colors.green, fontSize: 28, fontWeight: '900', marginTop: 8 }}>R$ {Number(loyalty.cashbackBalance ?? 0).toFixed(2).replace('.', ',')}</Text><Empty>{Number(loyalty.pointsBalance ?? 0)} pontos disponíveis.</Empty></> : <Empty>Nenhum saldo de fidelidade disponível.</Empty>}</Card>
      <Card><Title>Cupons disponíveis</Title>{coupons.length ? coupons.map(coupon => <Text key={coupon.id ?? coupon.code} style={{ color: colors.slate, marginTop: spacing.sm }}><Text style={{ fontWeight: '900' }}>{coupon.code}</Text>{coupon.discount ? ` • ${coupon.discount}` : ''}</Text>) : <Empty>Nenhum cupom disponível no momento.</Empty>}</Card>
      <Card><Title>Notificações</Title>{notifications.length ? notifications.map((item, index) => <Text key={item.id ?? index} style={{ color: colors.slate, marginTop: spacing.sm }}>• {typeof item === 'string' ? item : item.message ?? item.title}</Text>) : <Empty>Você está em dia. Nenhuma notificação nova.</Empty>}</Card>
    </ScrollView>
  );
}
