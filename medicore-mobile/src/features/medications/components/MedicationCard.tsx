import { Pressable, StyleSheet, Text, View } from "react-native";

import { Medication } from "@/features/medications/types";
import { Card } from "@/shared/components/Card";
import { colors } from "@/shared/theme/colors";
import { radius } from "@/shared/theme/radius";
import { spacing } from "@/shared/theme/spacing";
import { typography } from "@/shared/theme/typography";

type MedicationCardProps = {
  medication: Medication;
  onPress: () => void;
};

export function MedicationCard({ medication, onPress }: MedicationCardProps) {
  const status = medication.isActive ? "Active" : "Inactive";

  const reminderCount = medication.schedules.filter(
    (schedule) => schedule.reminderEnabled,
  ).length;

  const doseText = medication.unit
    ? `${medication.dose} ${medication.unit}`
    : medication.dose;

  return (
    <Pressable onPress={onPress}>
      {({ pressed }) => (
        <Card style={[styles.card, pressed && styles.pressed]}>
          <View style={styles.header}>
            <View style={styles.titleBlock}>
              <Text style={styles.name}>{medication.name}</Text>
              <Text style={styles.dosage}>{doseText}</Text>
            </View>

            <View
              style={[
                styles.statusBadge,
                medication.isActive ? styles.activeBadge : styles.inactiveBadge,
              ]}
            >
              <Text
                style={[
                  styles.statusText,
                  medication.isActive ? styles.activeText : styles.inactiveText,
                ]}
              >
                {status}
              </Text>
            </View>
          </View>

          <Text style={styles.instructions} numberOfLines={2}>
            {medication.instructions || "No instructions provided."}
          </Text>

          <View style={styles.footer}>
            <View style={styles.pill}>
              <Text style={styles.pillText}>
                {medication.schedules.length} schedule
                {medication.schedules.length === 1 ? "" : "s"}
              </Text>
            </View>

            <View style={styles.pill}>
              <Text style={styles.pillText}>
                {reminderCount} reminder
                {reminderCount === 1 ? "" : "s"} enabled
              </Text>
            </View>
          </View>
        </Card>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    marginBottom: spacing.lg,
  },

  pressed: {
    opacity: 0.88,
  },

  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: spacing.md,
    marginBottom: spacing.md,
  },

  titleBlock: {
    flex: 1,
  },

  name: {
    ...typography.subtitle,
    color: colors.text,
  },

  dosage: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: spacing.xs,
  },

  statusBadge: {
    borderRadius: radius.full,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.xs,
    alignSelf: "flex-start",
  },

  activeBadge: {
    backgroundColor: colors.greenSoft,
  },

  inactiveBadge: {
    backgroundColor: colors.orangeSoft,
  },

  statusText: {
    ...typography.caption,
  },

  activeText: {
    color: colors.success,
  },

  inactiveText: {
    color: colors.warning,
  },

  instructions: {
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
  },

  footer: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: spacing.sm,
    marginTop: spacing.md,
  },

  pill: {
    backgroundColor: colors.blueSoft,
    borderRadius: radius.full,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.xs,
  },

  pillText: {
    ...typography.caption,
    color: colors.primary,
  },
});
