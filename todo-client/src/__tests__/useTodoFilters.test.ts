import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import {
  useTodoFilters,
  getStoredPageSize,
  storePageSize,
  STORAGE_KEY,
  DEFAULT_PAGE_SIZE,
  PAGE_SIZE_OPTIONS,
} from '../hooks/useTodoFilters';
import { Priority } from '../types/todo';

describe('getStoredPageSize', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('returns default when localStorage is empty', () => {
    expect(getStoredPageSize()).toBe(DEFAULT_PAGE_SIZE);
  });

  it('returns stored value when valid', () => {
    localStorage.setItem(STORAGE_KEY, '5');
    expect(getStoredPageSize()).toBe(5);
  });

  it('returns default when stored value is invalid', () => {
    localStorage.setItem(STORAGE_KEY, '15');
    expect(getStoredPageSize()).toBe(DEFAULT_PAGE_SIZE);
  });

  it('returns default when stored value is not a number', () => {
    localStorage.setItem(STORAGE_KEY, 'invalid');
    expect(getStoredPageSize()).toBe(DEFAULT_PAGE_SIZE);
  });
});

describe('storePageSize', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('stores the page size in localStorage', () => {
    storePageSize(20);
    expect(localStorage.getItem(STORAGE_KEY)).toBe('20');
  });
});

describe('useTodoFilters', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  describe('initial state', () => {
    it('returns default values', () => {
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.page).toBe(1);
      expect(result.current.pageSize).toBe(DEFAULT_PAGE_SIZE);
      expect(result.current.statusFilter).toBe('all');
      expect(result.current.priorityFilter).toBeUndefined();
      expect(result.current.sortBy).toBeUndefined();
      expect(result.current.sortDescending).toBe(true);
      expect(result.current.hasFilters).toBe(false);
    });

    it('uses stored pageSize from localStorage', () => {
      localStorage.setItem(STORAGE_KEY, '5');
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.pageSize).toBe(5);
    });

    it('exposes pageSizeOptions', () => {
      const { result } = renderHook(() => useTodoFilters());
      expect(result.current.pageSizeOptions).toEqual(PAGE_SIZE_OPTIONS);
    });
  });

  describe('setStatusFilter', () => {
    it('updates statusFilter and resets page to 1', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setPage(3);
      });
      expect(result.current.page).toBe(3);

      act(() => {
        result.current.setStatusFilter('completed');
      });
      expect(result.current.statusFilter).toBe('completed');
      expect(result.current.page).toBe(1);
    });

    it('sets isCompleted in queryParams based on status', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setStatusFilter('active');
      });
      expect(result.current.queryParams.isCompleted).toBe(false);

      act(() => {
        result.current.setStatusFilter('completed');
      });
      expect(result.current.queryParams.isCompleted).toBe(true);

      act(() => {
        result.current.setStatusFilter('all');
      });
      expect(result.current.queryParams.isCompleted).toBeUndefined();
    });
  });

  describe('setPriorityFilter', () => {
    it('updates priorityFilter and resets page', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setPage(2);
        result.current.setPriorityFilter(Priority.High);
      });

      expect(result.current.priorityFilter).toBe(Priority.High);
      expect(result.current.queryParams.priority).toBe(Priority.High);
      expect(result.current.page).toBe(1);
    });

    it('sets hasFilters to true when priority is set', () => {
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.hasFilters).toBe(false);

      act(() => {
        result.current.setPriorityFilter(Priority.Low);
      });

      expect(result.current.hasFilters).toBe(true);
    });
  });

  describe('setSortBy', () => {
    it('updates sortBy', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setSortBy('title');
      });

      expect(result.current.sortBy).toBe('title');
      expect(result.current.queryParams.sortBy).toBe('title');
    });
  });

  describe('toggleSortDirection', () => {
    it('toggles sortDescending', () => {
      const { result } = renderHook(() => useTodoFilters());

      expect(result.current.sortDescending).toBe(true);

      act(() => {
        result.current.toggleSortDirection();
      });
      expect(result.current.sortDescending).toBe(false);

      act(() => {
        result.current.toggleSortDirection();
      });
      expect(result.current.sortDescending).toBe(true);
    });
  });

  describe('setPageSize', () => {
    it('updates pageSize, stores in localStorage, and resets page', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setPage(3);
        result.current.setPageSize(20);
      });

      expect(result.current.pageSize).toBe(20);
      expect(result.current.page).toBe(1);
      expect(localStorage.getItem(STORAGE_KEY)).toBe('20');
    });
  });

  describe('queryParams', () => {
    it('includes all filter values', () => {
      const { result } = renderHook(() => useTodoFilters());

      act(() => {
        result.current.setPage(2);
        result.current.setPageSize(5);
        result.current.setStatusFilter('active');
        result.current.setPriorityFilter(Priority.Medium);
        result.current.setSortBy('duedate');
      });

      // Page was reset by priority filter, set it again
      act(() => {
        result.current.setPage(2);
      });

      expect(result.current.queryParams).toEqual({
        page: 2,
        pageSize: 5,
        isCompleted: false,
        priority: Priority.Medium,
        sortBy: 'duedate',
        sortDescending: true,
      });
    });
  });
});
