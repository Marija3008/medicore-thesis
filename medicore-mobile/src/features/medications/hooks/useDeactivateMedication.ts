import { useMutation, useQueryClient } from "@tanstack/react-query";

import { medicationsService } from "@/features/medications/services/medicationsService";

export function useDeactivateMedication() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (medicationId: number) =>
      medicationsService.deactivateMedication(medicationId),

    onSuccess: (_, medicationId) => {
      queryClient.invalidateQueries({
        queryKey: ["medications"],
      });

      queryClient.invalidateQueries({
        queryKey: ["medication", medicationId],
      });
    },
  });
}
