import React from 'react';
import { Text, TouchableOpacity } from 'react-native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export function AppButton({ title, onPress, variant = 'primary' }: { title: string; onPress?: () => void; variant?: 'primary' | 'gold' | 'ghost' }) {
  const backgroundColor = variant === 'gold' ? colors.gold : variant === 'ghost' ? colors.white : colors.blue;
  const color = variant === 'gold' ? colors.dark : variant === 'ghost' ? colors.text : colors.white;
  return <TouchableOpacity accessibilityRole="button" onPress={onPress} activeOpacity={0.86} style={{ backgroundColor, borderRadius: radius.md, paddingVertical: spacing.md, paddingHorizontal: spacing.lg, alignItems: 'center', borderWidth: variant === 'ghost' ? 1 : 0, borderColor: colors.border, shadowColor: colors.dark, shadowOpacity: variant === 'ghost' ? 0 : 0.12, shadowRadius: 12, elevation: variant === 'ghost' ? 0 : 2 }}><Text style={{ color, fontWeight: '900', fontSize: 15 }}>{title}</Text></TouchableOpacity>;
}
