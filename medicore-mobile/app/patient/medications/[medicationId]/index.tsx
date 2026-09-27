import { useDeactivateMedication } from "@/features/medications/hooks/useDeactivateMedication";
import { useMedicationDetails } from "@/features/medications/hooks/useMedicationDetails";
import { useReactivateMedication } from "@/features/medications/hooks/useReactivateMedication";
import { AppButton } from "@/shared/components/AppButton";
import { Card } from "@/shared/components/Card";
import { ErrorState } from "@/shared/components/ErrorState";
import { LoadingState } from "@/shared/components/LoadingState";
import { Screen } from "@/shared/components/Screen";
import { colors } from "@/shared/theme/colors";
import { radius } from "@/shared/theme/radius";
import { spacing } from "@/shared/theme/spacing";
import { typography } from "@/shared/theme/typography";
import { Ionicons } from "@expo/vector-icons";
import { router, useLocalSearchParams } from "expo-router";
import { Alert, ScrollView, StyleSheet, Text, View } from "react-native";

export default function MedicationDetailsScreen() {
  const { medicationId } = useLocalSearchParams<{ medicationId: string }>();

  const numericMedicationId = medicationId ? Number(medicationId) : undefined;

  const { data, isLoading, isError, refetch } =
    useMedicationDetails(numericMedicationId);

  const deactivateMutation = useDeactivateMedication();
  const reactivateMutation = useReactivateMedication();

  if (isLoading) {
    return (
      <Screen scroll={false}>
        <LoadingState
          title="Loading medication..."
          message="Preparing medication details."
        />
      </Screen>
    );
  }

  if (isError || !data) {
    return (
      <Screen scroll={false}>
        <ErrorState
          title="Medication not found"
          message="We could not load this medication right now."
          onRetry={() => refetch()}
        />
      </Screen>
    );
  }

  const doseText = data.unit ? `${data.dose} ${data.unit}` : data.dose;

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString();
  };

  const handleDeactivate = () => {
    if (!numericMedicationId || !data) {
      return;
    }

    Alert.alert(
      "Deactivate medication?",
      "The medication will remain in your history, but it will be marked as inactive and its reminders will be disabled.",
      [
        {
          text: "Cancel",
          style: "cancel",
        },
        {
          text: "Deactivate",
          style: "destructive",
          onPress: async () => {
            try {
              await deactivateMutation.mutateAsync(numericMedicationId);
            } catch (error) {
              Alert.alert(
                "Could not deactivate medication",
                error instanceof Error ? error.message : "Please try again.",
              );
            }
          },
        },
      ],
    );
  };

  const handleReactivate = () => {
  if (!data) {
    return;
  }

  Alert.alert(
    "Reactivate medication",
    `Reactivate ${data.name}? You can review and enable its reminders again after reactivation.`,
    [
      {
        text: "Cancel",
        style: "cancel",
      },
      {
        text: "Reactivate",
        onPress: async () => {
          try {
            await reactivateMutation.mutateAsync(data.id);
          } catch (err) {
            Alert.alert(
              "Could not reactivate medication",
              err instanceof Error
                ? err.message
                : "Please try again.",
            );
          }
        },
      },
    ],
  );
};

  return (
    <Screen scroll={false}>
      <View style={styles.container}>
        <View style={styles.topBar}>
          <Text style={styles.backText} onPress={() => router.back()}>
            ← Back
          </Text>
        </View>

        {data.isActive ? (
          <View style={styles.dangerSection}>
            <Text style={styles.dangerTitle}>Deactivate medication</Text>

            <Text style={styles.dangerText}>
              Keep this medication in your history but mark it as no longer
              active. Its medication reminders will also be disabled.
            </Text>

            <AppButton
              title={
                deactivateMutation.isPending
                  ? "Deactivating..."
                  : "Deactivate Medication"
              }
              variant="secondary"
              onPress={handleDeactivate}
            />
          </View>
        ) : (
          <View style={styles.reactivateSection}>
  <Card style={styles.inactiveInfoCard}>
    <Text style={styles.inactiveInfoTitle}>
      This medication is inactive
    </Text>

    <Text style={styles.inactiveText}>
      This medication remains in your history. Reactivating it will
      make it active again, but its reminders will remain disabled
      until you choose to enable them.
    </Text>
  </Card>

  <AppButton
    title={
      reactivateMutation.isPending
        ? "Reactivating..."
        : "Reactivate Medication"
    }
    onPress={handleReactivate}
  />
</View>
        )}

        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.scrollContent}
        >
          <View style={styles.header}>
            <Text style={styles.eyebrow}>Medication Details</Text>

            <Text style={styles.title}>{data.name}</Text>

            <Text style={styles.subtitle}>{doseText}</Text>
          </View>

          {data.isActive ? (
            <View style={styles.editButton}>
              <AppButton
                title="Edit Medication"
                variant="secondary"
                onPress={() =>
                  router.push(`/patient/medications/${data.id}/edit`)
                }
              />
            </View>
          ) : null}

          <Card style={styles.infoCard}>
            <View style={styles.statusHeader}>
              <View>
                <Text style={styles.sectionTitle}>Medication Information</Text>

                <Text style={styles.instructions}>
                  {data.instructions || "No instructions provided."}
                </Text>
              </View>

              <View
                style={[
                  styles.statusBadge,
                  data.isActive ? styles.activeBadge : styles.inactiveBadge,
                ]}
              >
                <Text
                  style={[
                    styles.statusText,
                    data.isActive ? styles.activeText : styles.inactiveText,
                  ]}
                >
                  {data.isActive ? "Active" : "Inactive"}
                </Text>
              </View>
            </View>
          </Card>

          <Card style={styles.infoCard}>
            <Text style={styles.sectionTitle}>Treatment Period</Text>

            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>Start date</Text>

              <Text style={styles.infoValue}>{formatDate(data.startDate)}</Text>
            </View>

            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>End date</Text>

              <Text style={styles.infoValue}>
                {data.endDate ? formatDate(data.endDate) : "Not specified"}
              </Text>
            </View>
          </Card>

          <Text style={styles.sectionTitleOutside}>Schedule</Text>

          {data.schedules.length === 0 ? (
            <Card style={styles.scheduleCard}>
              <Text style={styles.instructions}>
                No medication schedule has been added.
              </Text>
            </Card>
          ) : (
            data.schedules.map((schedule) => (
              <Card key={schedule.id} style={styles.scheduleCard}>
                <View style={styles.scheduleRow}>
                  <View>
                    <Text style={styles.scheduleTime}>
                      {schedule.timeOfDay.slice(0, 5)}
                    </Text>

                    <Text style={styles.scheduleLabel}>Scheduled dose</Text>
                  </View>

                  <View style={styles.reminderBlock}>
                    <Ionicons
                      name={
                        schedule.reminderEnabled
                          ? "notifications"
                          : "notifications-off-outline"
                      }
                      size={22}
                      color={
                        schedule.reminderEnabled
                          ? colors.success
                          : colors.warning
                      }
                    />

                    <Text style={styles.reminderText}>
                      {schedule.reminderEnabled
                        ? "Reminder on"
                        : "Reminder off"}
                    </Text>
                  </View>
                </View>
              </Card>
            ))
          )}
        </ScrollView>
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingTop: spacing.lg,
  },

  topBar: {
    marginBottom: spacing.md,
  },

  backText: {
    ...typography.bodyMedium,
    color: colors.primary,
  },

  scrollContent: {
    paddingBottom: spacing.xxl,
  },

  header: {
    marginBottom: spacing.xl,
  },

  eyebrow: {
    ...typography.caption,
    color: colors.primary,
    marginBottom: spacing.xs,
  },

  title: {
    ...typography.title,
    color: colors.text,
  },

  subtitle: {
    ...typography.heading,
    color: colors.primary,
    marginTop: spacing.xs,
  },

  infoCard: {
    marginBottom: spacing.lg,
  },

  statusHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: spacing.md,
  },

  sectionTitle: {
    ...typography.subtitle,
    color: colors.text,
    marginBottom: spacing.md,
  },

  sectionTitleOutside: {
    ...typography.heading,
    color: colors.text,
    marginBottom: spacing.md,
  },

  instructions: {
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
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

  infoRow: {
    marginBottom: spacing.md,
  },

  infoLabel: {
    ...typography.caption,
    color: colors.textSoft,
  },

  infoValue: {
    ...typography.bodyMedium,
    color: colors.text,
    marginTop: spacing.xs,
  },

  scheduleCard: {
    marginBottom: spacing.md,
  },

  scheduleRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    gap: spacing.md,
  },

  scheduleTime: {
    ...typography.heading,
    color: colors.text,
  },

  scheduleLabel: {
    ...typography.caption,
    color: colors.textSoft,
    marginTop: spacing.xs,
  },

  reminderBlock: {
    alignItems: "center",
    gap: spacing.xs,
  },

  reminderText: {
    ...typography.caption,
    color: colors.textMuted,
  },
  dangerSection: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.lg,
    marginTop: spacing.lg,
    marginBottom: spacing.lg,
  },

  dangerTitle: {
    ...typography.subtitle,
    color: colors.text,
    marginBottom: spacing.xs,
  },

  dangerText: {
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
    marginBottom: spacing.md,
  },

  inactiveInfoCard: {
    marginTop: spacing.lg,
    marginBottom: spacing.lg,
  },

  inactiveInfoTitle: {
    ...typography.subtitle,
    color: colors.text,
    marginBottom: spacing.xs,
  },
  editButton: {
    marginBottom: spacing.lg,
  },
  reactivateSection: {
  marginTop: spacing.lg,
  gap: spacing.md,
},
});
