import React from 'react';
import { View } from 'react-native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export function AppCard({ children, accent = false }: { children: React.ReactNode; accent?: boolean }) {
  return <View style={{ backgroundColor: accent ? colors.dark : colors.card, borderRadius: radius.lg, padding: spacing.lg, shadowColor: colors.dark, shadowOpacity: accent ? 0.16 : 0.08, shadowRadius: accent ? 24 : 18, elevation: accent ? 5 : 3, borderWidth: 1, borderColor: accent ? colors.dark3 : colors.border }}>{children}</View>;
}
