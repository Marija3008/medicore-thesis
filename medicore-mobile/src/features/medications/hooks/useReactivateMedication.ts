import { useMutation, useQueryClient } from "@tanstack/react-query";

import { medicationsService } from "../services/medicationsService";

export function useReactivateMedication() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (medicationId: number) =>
      medicationsService.reactivateMedication(medicationId),

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