import React from 'react';
import { View } from 'react-native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export function AppCard({ children }: { children: React.ReactNode }) {
  return <View style={{ backgroundColor: colors.card, borderRadius: radius.lg, padding: spacing.lg, shadowColor: colors.dark, shadowOpacity: 0.08, shadowRadius: 18, elevation: 3 }}>{children}</View>;
}
