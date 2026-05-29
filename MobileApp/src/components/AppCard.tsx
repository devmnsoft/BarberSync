import React from 'react';
import { View } from 'react-native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export function AppCard({ children, accent = false }: { children: React.ReactNode; accent?: boolean }) {
  return <View style={{ backgroundColor: accent ? colors.dark : colors.card, borderRadius: radius.lg, padding: spacing.lg, shadowColor: colors.dark, shadowOpacity: 0.08, shadowRadius: 18, elevation: 3, borderWidth: 1, borderColor: accent ? colors.dark3 : colors.border }}>{children}</View>;
}
