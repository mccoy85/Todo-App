import { useState, useMemo, useCallback } from 'react';
import type { Priority, TodoQueryParams, StatusFilter } from '../types/todo';

/** Available page size options for pagination */
export const PAGE_SIZE_OPTIONS = [5, 10, 20] as const;

/** LocalStorage key for persisting user's page size preference */
export const STORAGE_KEY = 'todo-page-size';

/** Default page size when no preference is stored */
export const DEFAULT_PAGE_SIZE = 10;

/**
 * Retrieves the user's stored page size preference from localStorage.
 * Returns the default if no valid value is stored or localStorage is unavailable.
 */
export const getStoredPageSize = (): number => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = parseInt(stored, 10);
      // Only accept values that are in our allowed options
      if (PAGE_SIZE_OPTIONS.includes(parsed as typeof PAGE_SIZE_OPTIONS[number])) {
        return parsed;
      }
    }
  } catch {
    // localStorage not available (private browsing, SSR, etc.)
  }
  return DEFAULT_PAGE_SIZE;
};

/**
 * Persists the user's page size preference to localStorage.
 * Silently fails if localStorage is unavailable.
 */
export const storePageSize = (size: number): void => {
  try {
    localStorage.setItem(STORAGE_KEY, String(size));
  } catch {
    // localStorage not available (private browsing, SSR, etc.)
  }
};

/** Return type for the useTodoFilters hook */
interface UseTodoFiltersReturn {
  /** Combined query parameters ready to pass to the API/filter function */
  queryParams: TodoQueryParams;
  /** Current status filter selection */
  statusFilter: StatusFilter;
  /** Current priority filter (undefined = all priorities) */
  priorityFilter: Priority | undefined;
  /** Current sort field (undefined = default sorting) */
  sortBy: string | undefined;
  /** Whether to sort in descending order */
  sortDescending: boolean;
  /** Current page number (1-indexed) */
  page: number;
  /** Current page size */
  pageSize: number;
  /** Available page size options */
  pageSizeOptions: readonly number[];
  /** Whether any filters are active (for UI display) */
  hasFilters: boolean;
  /** Update the status filter (resets to page 1) */
  setStatusFilter: (status: StatusFilter) => void;
  /** Update the priority filter (resets to page 1) */
  setPriorityFilter: (priority: Priority | undefined) => void;
  /** Update the sort field */
  setSortBy: (sortBy: string | undefined) => void;
  /** Toggle between ascending and descending sort */
  toggleSortDirection: () => void;
  /** Navigate to a specific page */
  setPage: (page: number) => void;
  /** Update page size (persists to localStorage, resets to page 1) */
  setPageSize: (size: number) => void;
}

/**
 * Hook for managing todo list filter, sort, and pagination state.
 *
 * Features:
 * - Persists page size preference to localStorage
 * - Automatically resets to page 1 when filters change
 * - Computes query params for API calls
 * - Tracks whether any filters are active
 *
 * @example
 * const filters = useTodoFilters();
 * const { data } = useFilteredTodos(filters.queryParams);
 */
export const useTodoFilters = (): UseTodoFiltersReturn => {
  // Pagination state
  const [page, setPage] = useState(1);
  const [pageSize, setPageSizeState] = useState(getStoredPageSize);

  // Filter state
  const [statusFilter, setStatusFilterState] = useState<StatusFilter>('all');
  const [priorityFilter, setPriorityFilterState] = useState<Priority | undefined>();

  // Sort state
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [sortDescending, setSortDescending] = useState(true);

  /**
   * Convert status filter to isCompleted boolean for API.
   * 'all' and 'deleted' don't filter by completion status.
   */
  const filterCompleted = useMemo(() => {
    if (statusFilter === 'all') return undefined;
    if (statusFilter === 'deleted') return undefined;
    return statusFilter === 'completed';
  }, [statusFilter]);

  /** Combined query parameters for API/filter calls */
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

  /** True if any filter (status or priority) is active */
  const hasFilters = filterCompleted !== undefined || priorityFilter !== undefined;

  // Filter setters reset to page 1 to avoid showing empty pages
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

  // Page size changes are persisted and reset to page 1
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
