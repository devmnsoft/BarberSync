import React from 'react';
import { Text, View } from 'react-native';

/**
 * Safe authentication boundary used while the native identity flow is mounted.
 * It deliberately exposes no demo identity and never fabricates a local session.
 */
export default function LoginScreen() {
  return (
    <View style={{ flex: 1, padding: 24, backgroundColor: '#f8fafc', justifyContent: 'center' }}>
      <Text style={{ color: '#052e2b', fontSize: 30, fontWeight: '900' }}>Entrar no BarberSync</Text>
      <Text style={{ color: '#64748b', marginTop: 10, lineHeight: 22 }}>
        Autentique-se pelo provedor seguro da sua organização para acessar seus dados.
      </Text>
      <View style={{ marginTop: 22, padding: 18, borderRadius: 20, backgroundColor: '#ffffff', borderWidth: 1, borderColor: '#e2e8f0' }}>
        <Text style={{ color: '#0f172a', fontWeight: '900' }}>Sessão necessária</Text>
        <Text style={{ color: '#64748b', marginTop: 6 }}>
          Nenhum dado é mantido neste dispositivo antes da autenticação.
        </Text>
      </View>
    </View>
  );
}
