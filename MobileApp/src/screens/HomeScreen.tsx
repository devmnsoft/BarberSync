import React from 'react';
import { ScrollView, Text, View } from 'react-native';
import { AppCard } from '../components/AppCard';
import { AppButton } from '../components/AppButton';
import { colors } from '../theme/colors';
import { spacing } from '../theme/spacing';

const services = [
  { name: 'Corte + Barba', price: 'R$ 95', tag: 'Mais pedido' },
  { name: 'Barba Tradicional', price: 'R$ 45', tag: '30 min' },
  { name: 'Hidratação Premium', price: 'R$ 75', tag: 'Novo' },
];

const promos = [
  { title: 'RETORNO20', text: '20% no combo para voltar em até 21 dias.' },
  { title: 'VIP Cashback', text: 'Use R$ 32,50 de saldo no próximo atendimento.' },
];

export function HomeScreen() {
  return (
    <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} contentContainerStyle={{ padding: spacing.lg, gap: spacing.md }}>
      <View style={{ gap: spacing.xs }}>
        <Text style={{ color: colors.muted, fontWeight: '800' }}>Olá, Lucas 👋</Text>
        <Text style={{ color: colors.dark, fontSize: 30, fontWeight: '900' }}>BarberSync</Text>
        <Text style={{ color: colors.muted }}>Experiência premium da Barbearia Elite Demo no seu bolso.</Text>
      </View>

      <AppCard accent>
        <Text style={{ color: colors.gold, fontWeight: '900' }}>Próximo agendamento</Text>
        <Text style={{ color: colors.white, fontSize: 24, fontWeight: '900', marginTop: 8 }}>Corte + Barba</Text>
        <Text style={{ color: colors.blueSoft, marginTop: 8 }}>Hoje às 18:30 com Rafael Barber • Unidade Jardins</Text>
        <View style={{ flexDirection: 'row', gap: spacing.sm, marginTop: spacing.md }}>
          <AppButton title="Confirmar" variant="gold" />
          <AppButton title="Remarcar" variant="ghost" />
        </View>
      </AppCard>

      <View style={{ flexDirection: 'row', gap: spacing.sm }}>
        <View style={{ flex: 1 }}><AppButton title="Agendar" variant="gold" /></View>
        <View style={{ flex: 1 }}><AppButton title="Serviços" variant="ghost" /></View>
      </View>

      <AppCard>
        <Text style={{ color: colors.text, fontSize: 18, fontWeight: '900' }}>Cashback BarberSync</Text>
        <Text style={{ color: colors.green, fontSize: 30, fontWeight: '900', marginTop: spacing.sm }}>R$ 32,50</Text>
        <Text style={{ color: colors.muted }}>Saldo disponível para usar em serviços ou produtos selecionados.</Text>
      </AppCard>

      <Text style={{ color: colors.dark, fontSize: 20, fontWeight: '900' }}>Serviços em destaque</Text>
      <View style={{ gap: spacing.sm }}>
        {services.map(service => (
          <AppCard key={service.name}>
            <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
              <View>
                <Text style={{ color: colors.text, fontWeight: '900', fontSize: 16 }}>{service.name}</Text>
                <Text style={{ color: colors.muted, marginTop: spacing.xs }}>{service.tag}</Text>
              </View>
              <Text style={{ color: colors.gold, fontWeight: '900', fontSize: 18 }}>{service.price}</Text>
            </View>
          </AppCard>
        ))}
      </View>

      <Text style={{ color: colors.dark, fontSize: 20, fontWeight: '900' }}>Promoções para você</Text>
      <View style={{ gap: spacing.sm }}>
        {promos.map(promo => (
          <AppCard key={promo.title}>
            <Text style={{ color: colors.purple, fontWeight: '900' }}>{promo.title}</Text>
            <Text style={{ color: colors.text, marginTop: spacing.xs }}>{promo.text}</Text>
          </AppCard>
        ))}
      </View>


      <AppCard>
        <Text style={{ color: colors.text, fontSize: 18, fontWeight: '900' }}>Histórico integrado</Text>
        <Text style={{ color: colors.muted, marginTop: 8 }}>Último fluxo: PublicWeb → Agenda → Totem → Comanda PIX → Cashback → Avaliação NPS 10.</Text>
      </AppCard>

      <AppCard>
        <Text style={{ color: colors.text, fontSize: 18, fontWeight: '900' }}>Resumo da experiência</Text>
        <Text style={{ color: colors.muted, marginTop: 8 }}>4 visitas no mês • avaliação média 4,9 • 2 cupons ativos • atendimento sem fila pelo totem.</Text>
      </AppCard>
    </ScrollView>
  );
}
