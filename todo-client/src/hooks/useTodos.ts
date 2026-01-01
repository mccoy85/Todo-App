import useSWR, { useSWRConfig } from 'swr';
import { useMemo, useState, useCallback } from 'react';
import { todoApi } from '../services/todoApi';
import type { Todo, TodoQueryParams, CreateTodoRequest, UpdateTodoRequest, TodoListResponse } from '../types/todo';

const TODOS_ALL_KEY = 'todos/all';
const DELETED_ALL_KEY = 'deleted-todos/all';

const applyFilters = (
  data: TodoListResponse | undefined,
  params: TodoQueryParams
): TodoListResponse | undefined => {
  if (!data) return undefined;

  let items = [...data.items];

  if (params.isCompleted !== undefined) {
    items = items.filter((todo) => todo.isCompleted === params.isCompleted);
  }

  if (params.priority !== undefined) {
    items = items.filter((todo) => todo.priority === params.priority);
  }

  if (params.sortBy) {
    items.sort((a, b) => {
      let comparison = 0;
      switch (params.sortBy) {
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'priority':
          comparison = a.priority - b.priority;
          break;
        case 'duedate':
          if (!a.dueDate && !b.dueDate) comparison = 0;
          else if (!a.dueDate) comparison = 1;
          else if (!b.dueDate) comparison = -1;
          else comparison = new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
          break;
        case 'createdat':
        default:
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
      }
      return params.sortDescending ? -comparison : comparison;
    });
  }

  const totalCount = items.length;

  const page = params.page ?? 1;
  const pageSize = params.pageSize ?? 10;
  const startIndex = (page - 1) * pageSize;
  const paginatedItems = items.slice(startIndex, startIndex + pageSize);

  return {
    items: paginatedItems,
    totalCount,
    page,
    pageSize,
  };
};

const updateList = (items: Todo[], updated: Todo) =>
  items.map((item) => (item.id === updated.id ? updated : item));

export const useFilteredTodos = (params: TodoQueryParams) => {
  const { data, error, isLoading, isValidating, mutate } = useSWR(
    TODOS_ALL_KEY,
    () => todoApi.getAllFull(),
    { revalidateOnFocus: false, refreshInterval: 60000 }
  );

  const filteredData = useMemo(
    () => applyFilters(data, params),
    [data, params]
  );

  const counts = useMemo(() => {
    if (!data) return { total: 0, active: 0, completed: 0 };
    return {
      total: data.items.length,
      active: data.items.filter((t) => !t.isCompleted).length,
      completed: data.items.filter((t) => t.isCompleted).length,
    };
  }, [data]);

  return {
    data: filteredData,
    counts,
    error,
    isLoading,
    isValidating,
    isError: !!error,
    refetch: mutate,
  };
};

export const useDeletedTodos = (params: TodoQueryParams) => {
  const { data, error, isLoading, isValidating, mutate } = useSWR(
    DELETED_ALL_KEY,
    () => todoApi.getDeletedFull(),
    { revalidateOnFocus: false, refreshInterval: 60000 }
  );

  const filteredData = useMemo(
    () => applyFilters(data, params),
    [data, params]
  );

  return {
    data: filteredData,
    totalCount: data?.items.length ?? 0,
    error,
    isLoading,
    isValidating,
    isError: !!error,
    refetch: mutate,
  };
};

export const useTodo = (id: number) => {
  const { data, error, isLoading } = useSWR(
    id > 0 ? `todo/${id}` : null,
    () => todoApi.getById(id)
  );

  return { data, error, isLoading, isError: !!error };
};

export const useCreateTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);

  const mutateAsync = useCallback(
    async (todo: CreateTodoRequest, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      try {
        const created = await todoApi.create(todo);
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: [created, ...current.items],
            totalCount: current.totalCount + 1,
          };
        }, false);
        options?.onSuccess?.();
      } catch (error) {
        options?.onError?.(error instanceof Error ? error : new Error('Create failed'));
      } finally {
        setIsPending(false);
      }
    },
    [mutate]
  );

  return { mutate: mutateAsync, isPending };
};

export const useUpdateTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);

  const mutateAsync = useCallback(
    async (
      { id, todo }: { id: number; todo: UpdateTodoRequest },
      options?: { onSuccess?: () => void; onError?: (error: Error) => void }
    ) => {
      setIsPending(true);
      try {
        const updated = await todoApi.update(id, todo);
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: updateList(current.items, updated),
          };
        }, false);
        options?.onSuccess?.();
      } catch (error) {
        options?.onError?.(error instanceof Error ? error : new Error('Update failed'));
      } finally {
        setIsPending(false);
      }
    },
    [mutate]
  );

  return { mutate: mutateAsync, isPending };
};

export const useToggleTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);
  const [variables, setVariables] = useState<number | undefined>();

  const mutateAsync = useCallback(
    async (id: number, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      setVariables(id);
      try {
        const updated = await todoApi.toggle(id);
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: updateList(current.items, updated),
          };
        }, false);
        options?.onSuccess?.();
      } catch (error) {
        options?.onError?.(error instanceof Error ? error : new Error('Toggle failed'));
      } finally {
        setIsPending(false);
        setVariables(undefined);
      }
    },
    [mutate]
  );

  return { mutate: mutateAsync, isPending, variables };
};

export const useDeleteTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);
  const [variables, setVariables] = useState<number | undefined>();

  const mutateAsync = useCallback(
    async (id: number, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      setVariables(id);
      try {
        await todoApi.delete(id);
        // Find the todo before updating caches
        const currentData = await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => current, false);
        const deletedTodo = currentData?.items.find((item) => item.id === id);

        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: current.items.filter((item) => item.id !== id),
            totalCount: Math.max(0, current.totalCount - 1),
          };
        }, false);

        if (deletedTodo) {
          await mutate(DELETED_ALL_KEY, (current?: TodoListResponse) => {
            if (!current) {
              return { items: [deletedTodo], totalCount: 1, page: 1, pageSize: 1 };
            }
            return {
              ...current,
              items: [deletedTodo, ...current.items],
              totalCount: current.totalCount + 1,
            };
          }, false);
        }
        options?.onSuccess?.();
      } catch (error) {
        options?.onError?.(error instanceof Error ? error : new Error('Delete failed'));
      } finally {
        setIsPending(false);
        setVariables(undefined);
      }
    },
    [mutate]
  );

  return { mutate: mutateAsync, isPending, variables };
};

export const useRestoreTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);
  const [variables, setVariables] = useState<number | undefined>();

  const mutateAsync = useCallback(
    async (id: number, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      setVariables(id);
      try {
        const restored = await todoApi.restore(id);
        await mutate(DELETED_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          const remaining = current.items.filter((item) => item.id !== id);
          return {
            ...current,
            items: remaining,
            totalCount: Math.max(0, current.totalCount - 1),
          };
        }, false);
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: [restored, ...current.items],
            totalCount: current.totalCount + 1,
          };
        }, false);
        options?.onSuccess?.();
      } catch (error) {
        options?.onError?.(error instanceof Error ? error : new Error('Restore failed'));
      } finally {
        setIsPending(false);
        setVariables(undefined);
      }
    },
    [mutate]
  );

  return { mutate: mutateAsync, isPending, variables };
};
