import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'

import { apiClient, unwrapData } from '@/services/http'

import type { IResponse } from '../types/response.type'

export interface ITask {
  title: string
  description: string
  status: 'pending' | 'in-progress' | 'completed'
}

export function useGetTasksQuery() {
  return useQuery<IResponse<ITask[]>, Error>({
    queryKey: ['useGetTasksQuery'],
    queryFn: () => unwrapData(apiClient.get<IResponse<ITask[]>>('/tasks')),
  })
}

export function useGetTaskByIdQuery(id: number) {
  return useQuery<IResponse<ITask>, Error>({
    queryKey: ['useGetTaskQuery', id],
    queryFn: () => unwrapData(apiClient.get<IResponse<ITask>>(`/tasks/${id}`)),
  })
}

export function useUpdateTaskMutation(id: number) {
  const queryClient = useQueryClient()

  return useMutation<IResponse<boolean>, Error, Partial<ITask>>({
    mutationKey: ['useUpdateTaskMutation', id],
    mutationFn: data => unwrapData(apiClient.put<IResponse<boolean>>(`/tasks/${id}`, data)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['useGetTaskQuery', id] })
      queryClient.invalidateQueries({ queryKey: ['useGetTasksQuery'] })
    },
  })
}

export function useCreateTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation<IResponse<ITask>, Error, ITask>({
    mutationKey: ['useCreateTaskMutation'],
    mutationFn: data => unwrapData(apiClient.post<IResponse<ITask>>('/tasks', data)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['useGetTasksQuery'] })
    },
  })
}

export function useDeleteTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation<IResponse<boolean>, Error, number>({
    mutationKey: ['useDeleteTaskMutation'],
    mutationFn: id => unwrapData(apiClient.delete<IResponse<boolean>>(`/tasks/${id}`)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['useGetTasksQuery'] })
    },
  })
}
