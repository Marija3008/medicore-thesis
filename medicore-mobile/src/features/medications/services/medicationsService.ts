import {
  CreateMedicationRequest,
  Medication,
  UpdateMedicationRequest,
} from "@/features/medications/types";
import { ApiError, apiFetch } from "@/services/api/httpClient";

export const medicationsService = {
  getMedications: async (): Promise<Medication[]> => {
    return await apiFetch<Medication[]>("/medications");
  },

  getMedicationById: async (
    medicationId: number,
  ): Promise<Medication | undefined> => {
    try {
      return await apiFetch<Medication>(`/medications/${medicationId}`);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return undefined;
      }

      throw error;
    }
  },

  createMedication: async (
    request: CreateMedicationRequest,
  ): Promise<Medication> => {
    return await apiFetch<Medication>("/medications", {
      method: "POST",
      body: JSON.stringify(request),
    });
  },

  updateMedication: async (
  medicationId: number,
  request: UpdateMedicationRequest
): Promise<Medication> => {
  return await apiFetch<Medication>(
    `/medications/${medicationId}`,
    {
      method: "PUT",
      body: JSON.stringify(request),
    }
  );
},

deactivateMedication: async (
  medicationId: number
): Promise<Medication> => {
  return await apiFetch<Medication>(
    `/medications/${medicationId}/deactivate`,
    {
      method: "PATCH",
    }
  );
},

reactivateMedication: async (
  medicationId: number
): Promise<Medication> => {
  return await apiFetch<Medication>(
    `/medications/${medicationId}/reactivate`,
    {
      method: "PATCH",
    }
  );
},
};
