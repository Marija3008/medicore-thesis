import DateTimePicker, {
  DateTimePickerEvent,
} from "@react-native-community/datetimepicker";
import { router } from "expo-router";
import { useState } from "react";
import {
  Pressable,
  ScrollView,
  StyleSheet,
  Switch,
  Text,
  TextInput,
  View,
} from "react-native";

import { useCreateMedication } from "@/features/medications/hooks/useCreateMedication";
import { AppButton } from "@/shared/components/AppButton";
import { Card } from "@/shared/components/Card";
import { Screen } from "@/shared/components/Screen";
import { colors } from "@/shared/theme/colors";
import { radius } from "@/shared/theme/radius";
import { spacing } from "@/shared/theme/spacing";
import { typography } from "@/shared/theme/typography";

export default function AddMedicationScreen() {
  const [name, setName] = useState("");
  const [dose, setDose] = useState("");
  const [unit, setUnit] = useState("");
  const [instructions, setInstructions] = useState("");
  const [startDate, setStartDate] = useState<Date>(new Date());
  const [endDate, setEndDate] = useState<Date | null>(null);

  type ScheduleInput = {
    id: number;
    time: Date;
    reminderEnabled: boolean;
  };

  const [schedules, setSchedules] = useState<ScheduleInput[]>([]);
  const [editingScheduleId, setEditingScheduleId] = useState<number | null>(
    null,
  );

  const [showStartDatePicker, setShowStartDatePicker] = useState(false);
  const [showEndDatePicker, setShowEndDatePicker] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const createMutation = useCreateMedication();

  const formatDate = (date: Date) => {
    return date.toLocaleDateString();
  };

  const handleStartDateChange = (
    event: DateTimePickerEvent,
    selectedDate?: Date,
  ) => {
    setShowStartDatePicker(false);

    if (event.type === "set" && selectedDate) {
      setStartDate(selectedDate);

      if (endDate && selectedDate > endDate) {
        setEndDate(null);
      }
    }
  };

  const handleEndDateChange = (
    event: DateTimePickerEvent,
    selectedDate?: Date,
  ) => {
    setShowEndDatePicker(false);

    if (event.type === "set" && selectedDate) {
      setEndDate(selectedDate);
    }
  };

  const addSchedule = () => {
    const time = new Date();
    time.setHours(8, 0, 0, 0);

    const id = Date.now();

    setSchedules((current) => [
      ...current,
      {
        id,
        time,
        reminderEnabled: true,
      },
    ]);

    setEditingScheduleId(id);
  };

  const removeSchedule = (id: number) => {
    setSchedules((current) => current.filter((schedule) => schedule.id !== id));

    if (editingScheduleId === id) {
      setEditingScheduleId(null);
    }
  };

  const handleScheduleTimeChange = (
    event: DateTimePickerEvent,
    selectedTime?: Date,
  ) => {
    const scheduleId = editingScheduleId;

    setEditingScheduleId(null);

    if (event.type === "set" && selectedTime && scheduleId !== null) {
      setSchedules((current) =>
        current.map((schedule) =>
          schedule.id === scheduleId
            ? {
                ...schedule,
                time: selectedTime,
              }
            : schedule,
        ),
      );
    }
  };

  const toggleScheduleReminder = (id: number) => {
    setSchedules((current) =>
      current.map((schedule) =>
        schedule.id === id
          ? {
              ...schedule,
              reminderEnabled: !schedule.reminderEnabled,
            }
          : schedule,
      ),
    );
  };
  const formatTime = (date: Date) => {
    return date.toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const handleCreate = async () => {
    setError(null);

    if (!name.trim()) {
      setError("Please enter the medication name.");
      return;
    }

    if (!dose.trim()) {
      setError("Please enter the medication dose.");
      return;
    }

    if (endDate && endDate < startDate) {
      setError("End date cannot be before the start date.");
      return;
    }

    try {
      await createMutation.mutateAsync({
        name: name.trim(),
        dose: dose.trim(),
        unit: unit.trim() || null,
        instructions: instructions.trim() || null,
        startDate: startDate.toISOString(),
        endDate: endDate ? endDate.toISOString() : null,
        schedules: schedules.map((schedule) => ({
          timeOfDay: `${schedule.time
            .getHours()
            .toString()
            .padStart(2, "0")}:${schedule.time
            .getMinutes()
            .toString()
            .padStart(2, "0")}:00`,
          reminderEnabled: schedule.reminderEnabled,
        })),
      });

      router.replace("/patient/medications");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Medication could not be created.",
      );
    }
  };

  return (
    <Screen scroll={false}>
      <View style={styles.container}>
        <View style={styles.topBar}>
          <Text style={styles.backText} onPress={() => router.back()}>
            ← Back
          </Text>
        </View>

        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.scrollContent}
        >
          <View style={styles.header}>
            <Text style={styles.eyebrow}>Medications</Text>
            <Text style={styles.title}>Add Medication</Text>
            <Text style={styles.subtitle}>
              Add a medication to your personal medication list.
            </Text>
          </View>

          <Card style={styles.card}>
            <Text style={styles.label}>Medication name</Text>

            <TextInput
              value={name}
              onChangeText={setName}
              placeholder="Example: Vitamin B12"
              placeholderTextColor={colors.textSoft}
              style={styles.input}
            />
          </Card>

          <Card style={styles.card}>
            <Text style={styles.label}>Dose</Text>

            <TextInput
              value={dose}
              onChangeText={setDose}
              placeholder="Example: 1000"
              placeholderTextColor={colors.textSoft}
              style={styles.input}
            />

            <Text style={styles.fieldLabel}>Unit</Text>

            <TextInput
              value={unit}
              onChangeText={setUnit}
              placeholder="Example: mcg"
              placeholderTextColor={colors.textSoft}
              style={styles.input}
            />
          </Card>

          <Card style={styles.card}>
            <Text style={styles.label}>Instructions</Text>

            <TextInput
              value={instructions}
              onChangeText={setInstructions}
              placeholder="Example: Take once daily after breakfast"
              placeholderTextColor={colors.textSoft}
              style={[styles.input, styles.multilineInput]}
              multiline
              textAlignVertical="top"
            />
          </Card>

          <Card style={styles.card}>
            <Text style={styles.label}>Treatment period</Text>

            <Text style={styles.fieldLabel}>Start date</Text>

            <Pressable
              style={styles.dateInput}
              onPress={() => setShowStartDatePicker(true)}
            >
              <Text style={styles.dateText}>📅 {formatDate(startDate)}</Text>
            </Pressable>

            {showStartDatePicker ? (
              <DateTimePicker
                value={startDate}
                mode="date"
                onChange={handleStartDateChange}
              />
            ) : null}

            <Text style={styles.fieldLabel}>End date (optional)</Text>

            <Pressable
              style={styles.dateInput}
              onPress={() => setShowEndDatePicker(true)}
            >
              <Text
                style={[styles.dateText, !endDate && styles.datePlaceholder]}
              >
                {endDate ? `📅 ${formatDate(endDate)}` : "📅 Select end date"}
              </Text>
            </Pressable>

            {showEndDatePicker ? (
              <DateTimePicker
                value={endDate ?? startDate}
                mode="date"
                minimumDate={startDate}
                onChange={handleEndDateChange}
              />
            ) : null}
          </Card>

          <Card style={styles.card}>
            <Text style={styles.label}>Medication schedule</Text>

            <Text style={styles.scheduleDescription}>
              Add the times when you normally take this medication.
            </Text>

            {schedules.map((schedule) => (
              <View key={schedule.id} style={styles.scheduleItem}>
                <Text style={styles.fieldLabel}>Time</Text>

                <Pressable
                  style={styles.dateInput}
                  onPress={() => setEditingScheduleId(schedule.id)}
                >
                  <Text style={styles.dateText}>
                    🕐 {formatTime(schedule.time)}
                  </Text>
                </Pressable>

                <View style={styles.reminderRow}>
                  <View style={styles.switchText}>
                    <Text style={styles.scheduleTitle}>Reminder</Text>
                    <Text style={styles.scheduleDescription}>
                      Remind me at this medication time.
                    </Text>
                  </View>

                  <Switch
                    value={schedule.reminderEnabled}
                    onValueChange={() => toggleScheduleReminder(schedule.id)}
                  />
                </View>

                <Pressable
                  onPress={() => removeSchedule(schedule.id)}
                  style={styles.removeButton}
                >
                  <Text style={styles.removeText}>Remove time</Text>
                </Pressable>
              </View>
            ))}

            {editingScheduleId !== null ? (
              <DateTimePicker
                value={
                  schedules.find(
                    (schedule) => schedule.id === editingScheduleId,
                  )?.time ?? new Date()
                }
                mode="time"
                is24Hour
                onChange={handleScheduleTimeChange}
              />
            ) : null}

            <View style={styles.addScheduleButton}>
              <AppButton
                title="+ Add medication time"
                variant="secondary"
                onPress={addSchedule}
              />
            </View>

            {schedules.length === 0 ? (
              <Text style={styles.noScheduleText}>
                No medication times added.
              </Text>
            ) : null}
          </Card>

          {error ? <Text style={styles.errorText}>{error}</Text> : null}

          <View style={styles.createButton}>
            <AppButton
              title={
                createMutation.isPending
                  ? "Adding medication..."
                  : "Add Medication"
              }
              onPress={handleCreate}
            />
          </View>

          <Text style={styles.helperText}>
            Medication information can be updated later from the medication
            details.
          </Text>
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
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
    marginTop: spacing.xs,
  },

  card: {
    marginBottom: spacing.lg,
  },

  label: {
    ...typography.subtitle,
    color: colors.text,
    marginBottom: spacing.md,
  },

  fieldLabel: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: spacing.md,
    marginBottom: spacing.xs,
  },

  input: {
    minHeight: 50,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    paddingHorizontal: spacing.md,
    color: colors.text,
    backgroundColor: colors.background,
  },

  multilineInput: {
    minHeight: 100,
    paddingTop: spacing.md,
  },

  errorText: {
    ...typography.bodyMedium,
    color: colors.danger,
    marginBottom: spacing.md,
  },

  createButton: {
    marginTop: spacing.md,
  },

  helperText: {
    ...typography.caption,
    color: colors.textSoft,
    textAlign: "center",
    marginTop: spacing.md,
  },
  dateInput: {
    minHeight: 50,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    paddingHorizontal: spacing.md,
    justifyContent: "center",
    backgroundColor: colors.background,
  },

  dateText: {
    ...typography.body,
    color: colors.text,
  },

  datePlaceholder: {
    color: colors.textSoft,
  },
  switchRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    gap: spacing.md,
  },

  switchText: {
    flex: 1,
  },

  scheduleTitle: {
    ...typography.bodyMedium,
    color: colors.text,
  },

  scheduleDescription: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: spacing.xs,
  },

  reminderRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    gap: spacing.md,
    marginTop: spacing.lg,
  },
  scheduleItem: {
    borderTopWidth: 1,
    borderTopColor: colors.border,
    marginTop: spacing.lg,
    paddingTop: spacing.md,
  },

  addScheduleButton: {
    marginTop: spacing.lg,
  },

  removeButton: {
    alignSelf: "flex-start",
    marginTop: spacing.md,
  },

  removeText: {
    ...typography.bodyMedium,
    color: colors.danger,
  },

  noScheduleText: {
    ...typography.caption,
    color: colors.textSoft,
    marginTop: spacing.md,
  },
});
