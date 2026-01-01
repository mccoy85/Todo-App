import { useState, useMemo, useCallback } from 'react';
import type { Priority, TodoQueryParams } from '../types/todo';

type StatusFilter = 'all' | 'active' | 'completed' | 'deleted';

const PAGE_SIZE_OPTIONS = [5, 10, 20] as const;
const STORAGE_KEY = 'todo-page-size';
const DEFAULT_PAGE_SIZE = 10;

const getStoredPageSize = (): number => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = parseInt(stored, 10);
      if (PAGE_SIZE_OPTIONS.includes(parsed as typeof PAGE_SIZE_OPTIONS[number])) {
        return parsed;
      }
    }
  } catch {
    // localStorage not available
  }
  return DEFAULT_PAGE_SIZE;
};

const storePageSize = (size: number): void => {
  try {
    localStorage.setItem(STORAGE_KEY, String(size));
  } catch {
    // localStorage not available
  }
};

interface UseTodoFiltersReturn {
  queryParams: TodoQueryParams;
  statusFilter: StatusFilter;
  priorityFilter: Priority | undefined;
  sortBy: string | undefined;
  sortDescending: boolean;
  page: number;
  pageSize: number;
  pageSizeOptions: readonly number[];
  hasFilters: boolean;
  setStatusFilter: (status: StatusFilter) => void;
  setPriorityFilter: (priority: Priority | undefined) => void;
  setSortBy: (sortBy: string | undefined) => void;
  toggleSortDirection: () => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
}

export const useTodoFilters = (): UseTodoFiltersReturn => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSizeState] = useState(getStoredPageSize);
  const [statusFilter, setStatusFilterState] = useState<StatusFilter>('all');
  const [priorityFilter, setPriorityFilterState] = useState<Priority | undefined>();
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [sortDescending, setSortDescending] = useState(true);

  const filterCompleted = useMemo(() => {
    if (statusFilter === 'all') return undefined;
    if (statusFilter === 'deleted') return undefined;
    return statusFilter === 'completed';
  }, [statusFilter]);

  const queryParams: TodoQueryParams = useMemo(
    () => ({
      page,
      pageSize,
      sortBy,
      sortDescending,
      isCompleted: filterCompleted,
      priority: priorityFilter,
    }),
    [page, pageSize, sortBy, sortDescending, filterCompleted, priorityFilter]
  );

  const hasFilters = filterCompleted !== undefined || priorityFilter !== undefined;

  const setStatusFilter = useCallback((status: StatusFilter) => {
    setStatusFilterState(status);
    setPage(1);
  }, []);

  const setPriorityFilter = useCallback((priority: Priority | undefined) => {
    setPriorityFilterState(priority);
    setPage(1);
  }, []);

  const toggleSortDirection = useCallback(() => {
    setSortDescending((prev) => !prev);
  }, []);

  const setPageSize = useCallback((size: number) => {
    setPageSizeState(size);
    storePageSize(size);
    setPage(1);
  }, []);

  return {
    queryParams,
    statusFilter,
    priorityFilter,
    sortBy,
    sortDescending,
    page,
    pageSize,
    pageSizeOptions: PAGE_SIZE_OPTIONS,
    hasFilters,
    setStatusFilter,
    setPriorityFilter,
    setSortBy,
    toggleSortDirection,
    setPage,
    setPageSize,
  };
};
