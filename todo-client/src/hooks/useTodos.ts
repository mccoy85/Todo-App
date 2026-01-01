import useSWR, { useSWRConfig } from 'swr';
import { useMemo, useState, useCallback } from 'react';
import { todoApi } from '../services/todoApi';
import type { Todo, TodoQueryParams, CreateTodoRequest, UpdateTodoRequest, TodoListResponse } from '../types/todo';

/** SWR cache key for the main todos list */
const TODOS_ALL_KEY = 'todos/all';

/** SWR cache key for soft-deleted todos */
const DELETED_ALL_KEY = 'deleted-todos/all';

/**
 * Client-side filtering, sorting, and pagination of todos.
 *
 * This function operates on the full dataset cached by SWR, allowing
 * instant filter/sort changes without additional API calls.
 *
 * @param data - The full todo list response from the API
 * @param params - Filter, sort, and pagination parameters
 * @returns Filtered and paginated response, or undefined if no data
 *
 * @example
 * const filtered = applyFilters(allTodos, {
 *   isCompleted: false,
 *   priority: Priority.High,
 *   sortBy: 'duedate',
 *   page: 1,
 *   pageSize: 10
 * });
 */
export const applyFilters = (
  data: TodoListResponse | undefined,
  params: TodoQueryParams
): TodoListResponse | undefined => {
  if (!data) return undefined;

  let items = [...data.items];

  // Filter by completion status
  if (params.isCompleted !== undefined) {
    items = items.filter((todo) => todo.isCompleted === params.isCompleted);
  }

  // Filter by priority level
  if (params.priority !== undefined) {
    items = items.filter((todo) => todo.priority === params.priority);
  }

  // Apply sorting
  if (params.sortBy) {
    items.sort((a, b) => {
      let comparison = 0;
      switch (params.sortBy) {
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'priority':
          // Lower number = lower priority (Low=0, Medium=1, High=2)
          comparison = a.priority - b.priority;
          break;
        case 'duedate':
          // Items without due dates sort to the end
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
      // Reverse for descending order
      return params.sortDescending ? -comparison : comparison;
    });
  }

  // Calculate total after filtering (before pagination)
  const totalCount = items.length;

  // Apply pagination
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

/**
 * Helper to update a single todo in an array by ID.
 * Returns a new array with the updated item.
 */
const updateList = (items: Todo[], updated: Todo) =>
  items.map((item) => (item.id === updated.id ? updated : item));

/**
 * Hook for fetching and filtering the main todo list.
 *
 * Uses SWR to cache the full dataset and applies client-side filtering
 * for instant UI updates. Auto-refreshes every 60 seconds.
 *
 * @param params - Filter, sort, and pagination parameters
 * @returns Filtered data, counts, loading states, and refetch function
 *
 * @example
 * const { data, counts, isLoading } = useFilteredTodos({
 *   isCompleted: false,
 *   sortBy: 'duedate'
 * });
 */
export const useFilteredTodos = (params: TodoQueryParams) => {
  const { data, error, isLoading, isValidating, mutate } = useSWR(
    TODOS_ALL_KEY,
    () => todoApi.getAllFull(),
    { revalidateOnFocus: false, refreshInterval: 60000 }
  );

  // Apply filters to cached data
  const filteredData = useMemo(
    () => applyFilters(data, params),
    [data, params]
  );

  // Calculate counts from unfiltered data for status tabs
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

/**
 * Hook for fetching and filtering soft-deleted todos.
 *
 * Similar to useFilteredTodos but for the deleted items view.
 *
 * @param params - Filter, sort, and pagination parameters
 */
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

/**
 * Hook for fetching a single todo by ID.
 *
 * @param id - Todo ID (must be > 0 to trigger fetch)
 */
export const useTodo = (id: number) => {
  const { data, error, isLoading } = useSWR(
    // Only fetch if ID is valid
    id > 0 ? `todo/${id}` : null,
    () => todoApi.getById(id)
  );

  return { data, error, isLoading, isError: !!error };
};

/**
 * Hook for creating a new todo.
 *
 * Optimistically updates the SWR cache by prepending the new item
 * to the list without triggering a refetch.
 *
 * @returns mutate function and pending state
 *
 * @example
 * const { mutate: createTodo, isPending } = useCreateTodo();
 * createTodo({ title: 'New task', priority: Priority.High }, {
 *   onSuccess: () => message.success('Created!')
 * });
 */
export const useCreateTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);

  const mutateAsync = useCallback(
    async (todo: CreateTodoRequest, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      try {
        const created = await todoApi.create(todo);
        // Optimistically update cache - prepend new item
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: [created, ...current.items],
            totalCount: current.totalCount + 1,
          };
        }, false); // false = don't revalidate
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

/**
 * Hook for updating an existing todo.
 *
 * Optimistically updates the item in the SWR cache.
 *
 * @returns mutate function and pending state
 */
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
        // Update the item in place
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

/**
 * Hook for toggling a todo's completion status.
 *
 * Exposes `variables` to track which item is being toggled (for UI feedback).
 *
 * @returns mutate function, pending state, and variables (the ID being toggled)
 */
export const useToggleTodo = () => {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);
  const [variables, setVariables] = useState<number | undefined>();

  const mutateAsync = useCallback(
    async (id: number, options?: { onSuccess?: () => void; onError?: (error: Error) => void }) => {
      setIsPending(true);
      setVariables(id); // Track which item is being toggled
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

/**
 * Hook for soft-deleting a todo.
 *
 * Removes the item from the main list and adds it to the deleted list.
 * Both caches are updated optimistically.
 *
 * @returns mutate function, pending state, and variables (the ID being deleted)
 */
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

        // Get the todo data before removing it (for adding to deleted list)
        const currentData = await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => current, false);
        const deletedTodo = currentData?.items.find((item) => item.id === id);

        // Remove from main list
        await mutate(TODOS_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          return {
            ...current,
            items: current.items.filter((item) => item.id !== id),
            totalCount: Math.max(0, current.totalCount - 1),
          };
        }, false);

        // Add to deleted list (if we have the data)
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

/**
 * Hook for restoring a soft-deleted todo.
 *
 * Removes the item from the deleted list and adds it back to the main list.
 * Both caches are updated optimistically.
 *
 * @returns mutate function, pending state, and variables (the ID being restored)
 */
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

        // Remove from deleted list
        await mutate(DELETED_ALL_KEY, (current?: TodoListResponse) => {
          if (!current) return current;
          const remaining = current.items.filter((item) => item.id !== id);
          return {
            ...current,
            items: remaining,
            totalCount: Math.max(0, current.totalCount - 1),
          };
        }, false);

        // Add back to main list
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
