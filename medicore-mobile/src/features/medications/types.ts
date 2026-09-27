export interface MedicationSchedule {
  id: number;
  timeOfDay: string;
  reminderEnabled: boolean;
}

export interface Medication {
  id: number;
  name: string;
  dose: string;
  unit?: string | null;
  instructions?: string | null;

  startDate: string;
  endDate?: string | null;

  isActive: boolean;
  createdAt: string;

  schedules: MedicationSchedule[];
}

export interface CreateMedicationRequest {
  name: string;
  dose: string;
  unit?: string | null;
  instructions?: string | null;

  startDate: string;
  endDate?: string | null;

  schedules: {
    timeOfDay: string;
    reminderEnabled: boolean;
  }[];
}

export interface UpdateMedicationRequest {
  name: string;
  dose: string;
  unit?: string | null;
  instructions?: string | null;

  startDate: string;
  endDate?: string | null;

  isActive: boolean;

  schedules: {
    timeOfDay: string;
    reminderEnabled: boolean;
  }[];
}
