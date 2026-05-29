import React from 'react';
import { Text, View } from 'react-native';
import { colors } from '../theme/colors';

export function EmptyState({ title, message }: { title: string; message: string }) {
  return <View style={{ alignItems: 'center', padding: 28 }}><Text style={{ fontSize: 34 }}>💈</Text><Text style={{ color: colors.text, fontWeight: '800', fontSize: 18 }}>{title}</Text><Text style={{ color: colors.muted, textAlign: 'center', marginTop: 6 }}>{message}</Text></View>;
}
