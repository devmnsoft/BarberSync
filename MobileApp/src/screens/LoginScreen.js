import React, { useState } from 'react';
import { Pressable, Text, View } from 'react-native';

export default function LoginScreen() {
  const [status, setStatus] = useState('Token demo pronto para autenticação segura.');
  return (
    <View style={{ flex: 1, padding: 24, backgroundColor: '#f8fafc', justifyContent: 'center' }}>
      <Text style={{ color: '#052e2b', fontSize: 30, fontWeight: '900' }}>Entrar no BarberSync</Text>
      <Text style={{ color: '#64748b', marginTop: 10, lineHeight: 22 }}>
        Login demo com telefone, biometria visual e sessão offline para demonstrar agenda, cashback, recibos e notificações sem tela vazia.
      </Text>
      <View style={{ marginTop: 22, padding: 18, borderRadius: 20, backgroundColor: '#ffffff', borderWidth: 1, borderColor: '#e2e8f0' }}>
        <Text style={{ color: '#0f172a', fontWeight: '900' }}>Cliente Demo</Text>
        <Text style={{ color: '#64748b', marginTop: 6 }}>Telefone: (11) 99999-0000 • Perfil VIP • Cashback ativo</Text>
      </View>
      <Pressable accessibilityRole="button" onPress={() => setStatus('Login demo realizado. Próximo passo: escolher serviço e agendar.')} style={{ marginTop: 18, backgroundColor: '#052e2b', borderRadius: 16, padding: 16, alignItems: 'center' }}>
        <Text style={{ color: '#ffffff', fontWeight: '900' }}>Acessar demonstração</Text>
      </Pressable>
      <Text style={{ color: '#16a34a', marginTop: 14, fontWeight: '800' }}>{status}</Text>
    </View>
  );
}
