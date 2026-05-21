import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ActivityIndicator } from 'react-native';
import { mobileApi } from '../services/api';

export default function GamificationScreen() {
  const [snapshot, setSnapshot] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    mobileApi
      .getOperationsSnapshot()
      .then(setSnapshot)
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <ActivityIndicator style={styles.loader} size="large" color="#0ea5e9" />;
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>BarberSync Futurista</Text>
      <Text style={styles.subtitle}>Gamificação e IA em tempo real</Text>
      <View style={styles.card}>
        <Text style={styles.cardTitle}>KPIs inteligentes</Text>
        {Object.entries(snapshot?.kpis ?? {}).map(([key, value]) => (
          <Text key={key} style={styles.item}>{key}: {value}%</Text>
        ))}
      </View>
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Previsão de demanda</Text>
        {(snapshot?.demandForecast ?? []).slice(0, 3).map((item) => (
          <Text key={`${item.hourUtc}-${item.serviceName}`} style={styles.item}>
            {item.serviceName} • {item.expectedAppointments} agendamentos
          </Text>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#020617', padding: 20 },
  loader: { flex: 1, justifyContent: 'center' },
  title: { fontSize: 24, fontWeight: '700', color: '#e2e8f0', marginBottom: 6 },
  subtitle: { fontSize: 14, color: '#94a3b8', marginBottom: 18 },
  card: { backgroundColor: '#0f172a', borderRadius: 12, padding: 14, marginBottom: 12 },
  cardTitle: { color: '#38bdf8', fontWeight: '700', marginBottom: 8 },
  item: { color: '#cbd5e1', marginBottom: 4 }
});
