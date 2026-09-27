import { useMutation, useQueryClient } from "@tanstack/react-query";

import { medicationsService } from "@/features/medications/services/medicationsService";

export function useCreateMedication() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: medicationsService.createMedication,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["medications"],
      });
    },
  });
}