import React from 'react';
import { Text, TouchableOpacity } from 'react-native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export function AppButton({ title, onPress, variant = 'primary' }: { title: string; onPress?: () => void; variant?: 'primary' | 'gold' | 'ghost' }) {
  const backgroundColor = variant === 'gold' ? colors.gold : variant === 'ghost' ? colors.white : colors.blue;
  const color = variant === 'gold' ? colors.dark : variant === 'ghost' ? colors.text : colors.white;
  return <TouchableOpacity onPress={onPress} style={{ backgroundColor, borderRadius: radius.md, padding: spacing.md, alignItems: 'center' }}><Text style={{ color, fontWeight: '800' }}>{title}</Text></TouchableOpacity>;
}
