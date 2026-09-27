import { useMutation, useQueryClient } from "@tanstack/react-query";

import { medicationsService } from "@/features/medications/services/medicationsService";
import { UpdateMedicationRequest } from "@/features/medications/types";

type UpdateMedicationVariables = {
  medicationId: number;
  request: UpdateMedicationRequest;
};

export function useUpdateMedication() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      medicationId,
      request,
    }: UpdateMedicationVariables) =>
      medicationsService.updateMedication(
        medicationId,
        request
      ),

    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["medications"],
      });

      queryClient.invalidateQueries({
        queryKey: ["medication", variables.medicationId],
      });
    },
  });
}