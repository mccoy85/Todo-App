import { describe, it, expect } from 'vitest';
import { applyFilters } from '../hooks/useTodos';
import type { TodoListResponse } from '../types/todo';
import { Priority } from '../types/todo';

const createTodo = (overrides: Partial<TodoListResponse['items'][0]> = {}) => ({
  id: 1,
  title: 'Test Todo',
  isCompleted: false,
  createdAt: '2024-01-15T10:00:00Z',
  priority: Priority.Medium,
  ...overrides,
});

const createResponse = (items: TodoListResponse['items']): TodoListResponse => ({
  items,
  totalCount: items.length,
  page: 1,
  pageSize: 10,
});

describe('applyFilters', () => {
  describe('when data is undefined', () => {
    it('returns undefined', () => {
      const result = applyFilters(undefined, {});
      expect(result).toBeUndefined();
    });
  });

  describe('filtering by completion status', () => {
    const data = createResponse([
      createTodo({ id: 1, isCompleted: false }),
      createTodo({ id: 2, isCompleted: true }),
      createTodo({ id: 3, isCompleted: false }),
    ]);

    it('filters active todos when isCompleted is false', () => {
      const result = applyFilters(data, { isCompleted: false });
      expect(result?.items).toHaveLength(2);
      expect(result?.items.every((t) => !t.isCompleted)).toBe(true);
      expect(result?.totalCount).toBe(2);
    });

    it('filters completed todos when isCompleted is true', () => {
      const result = applyFilters(data, { isCompleted: true });
      expect(result?.items).toHaveLength(1);
      expect(result?.items[0].isCompleted).toBe(true);
      expect(result?.totalCount).toBe(1);
    });

    it('returns all todos when isCompleted is undefined', () => {
      const result = applyFilters(data, {});
      expect(result?.items).toHaveLength(3);
    });
  });

  describe('filtering by priority', () => {
    const data = createResponse([
      createTodo({ id: 1, priority: Priority.Low }),
      createTodo({ id: 2, priority: Priority.Medium }),
      createTodo({ id: 3, priority: Priority.High }),
      createTodo({ id: 4, priority: Priority.Low }),
    ]);

    it('filters by Low priority', () => {
      const result = applyFilters(data, { priority: Priority.Low });
      expect(result?.items).toHaveLength(2);
      expect(result?.items.every((t) => t.priority === Priority.Low)).toBe(true);
    });

    it('filters by High priority', () => {
      const result = applyFilters(data, { priority: Priority.High });
      expect(result?.items).toHaveLength(1);
      expect(result?.items[0].priority).toBe(Priority.High);
    });
  });

  describe('sorting', () => {
    it('sorts by title ascending', () => {
      const data = createResponse([
        createTodo({ id: 1, title: 'Charlie' }),
        createTodo({ id: 2, title: 'Alpha' }),
        createTodo({ id: 3, title: 'Bravo' }),
      ]);

      const result = applyFilters(data, { sortBy: 'title' });
      expect(result?.items.map((t) => t.title)).toEqual(['Alpha', 'Bravo', 'Charlie']);
    });

    it('sorts by title descending', () => {
      const data = createResponse([
        createTodo({ id: 1, title: 'Charlie' }),
        createTodo({ id: 2, title: 'Alpha' }),
        createTodo({ id: 3, title: 'Bravo' }),
      ]);

      const result = applyFilters(data, { sortBy: 'title', sortDescending: true });
      expect(result?.items.map((t) => t.title)).toEqual(['Charlie', 'Bravo', 'Alpha']);
    });

    it('sorts by priority ascending (Low to High)', () => {
      const data = createResponse([
        createTodo({ id: 1, priority: Priority.High }),
        createTodo({ id: 2, priority: Priority.Low }),
        createTodo({ id: 3, priority: Priority.Medium }),
      ]);

      const result = applyFilters(data, { sortBy: 'priority' });
      expect(result?.items.map((t) => t.priority)).toEqual([Priority.Low, Priority.Medium, Priority.High]);
    });

    it('sorts by priority descending (High to Low)', () => {
      const data = createResponse([
        createTodo({ id: 1, priority: Priority.Low }),
        createTodo({ id: 2, priority: Priority.High }),
        createTodo({ id: 3, priority: Priority.Medium }),
      ]);

      const result = applyFilters(data, { sortBy: 'priority', sortDescending: true });
      expect(result?.items.map((t) => t.priority)).toEqual([Priority.High, Priority.Medium, Priority.Low]);
    });

    it('sorts by dueDate ascending', () => {
      const data = createResponse([
        createTodo({ id: 1, dueDate: '2024-03-01' }),
        createTodo({ id: 2, dueDate: '2024-01-01' }),
        createTodo({ id: 3, dueDate: '2024-02-01' }),
      ]);

      const result = applyFilters(data, { sortBy: 'duedate' });
      expect(result?.items.map((t) => t.id)).toEqual([2, 3, 1]);
    });

    it('handles missing dueDates by putting them last', () => {
      const data = createResponse([
        createTodo({ id: 1, dueDate: undefined }),
        createTodo({ id: 2, dueDate: '2024-01-01' }),
        createTodo({ id: 3, dueDate: undefined }),
      ]);

      const result = applyFilters(data, { sortBy: 'duedate' });
      expect(result?.items.map((t) => t.id)).toEqual([2, 1, 3]);
    });

    it('sorts by createdAt by default', () => {
      const data = createResponse([
        createTodo({ id: 1, createdAt: '2024-03-01T10:00:00Z' }),
        createTodo({ id: 2, createdAt: '2024-01-01T10:00:00Z' }),
        createTodo({ id: 3, createdAt: '2024-02-01T10:00:00Z' }),
      ]);

      const result = applyFilters(data, { sortBy: 'createdat' });
      expect(result?.items.map((t) => t.id)).toEqual([2, 3, 1]);
    });
  });

  describe('pagination', () => {
    const data = createResponse(
      Array.from({ length: 25 }, (_, i) => createTodo({ id: i + 1, title: `Todo ${i + 1}` }))
    );

    it('returns first page with default pageSize of 10', () => {
      const result = applyFilters(data, { page: 1 });
      expect(result?.items).toHaveLength(10);
      expect(result?.items[0].id).toBe(1);
      expect(result?.page).toBe(1);
      expect(result?.pageSize).toBe(10);
      expect(result?.totalCount).toBe(25);
    });

    it('returns second page', () => {
      const result = applyFilters(data, { page: 2, pageSize: 10 });
      expect(result?.items).toHaveLength(10);
      expect(result?.items[0].id).toBe(11);
    });

    it('returns partial last page', () => {
      const result = applyFilters(data, { page: 3, pageSize: 10 });
      expect(result?.items).toHaveLength(5);
      expect(result?.items[0].id).toBe(21);
    });

    it('respects custom pageSize', () => {
      const result = applyFilters(data, { page: 1, pageSize: 5 });
      expect(result?.items).toHaveLength(5);
      expect(result?.pageSize).toBe(5);
    });

    it('returns empty array for page beyond data', () => {
      const result = applyFilters(data, { page: 10, pageSize: 10 });
      expect(result?.items).toHaveLength(0);
    });
  });

  describe('combined filters', () => {
    const data = createResponse([
      createTodo({ id: 1, title: 'A', isCompleted: false, priority: Priority.High }),
      createTodo({ id: 2, title: 'B', isCompleted: true, priority: Priority.High }),
      createTodo({ id: 3, title: 'C', isCompleted: false, priority: Priority.Low }),
      createTodo({ id: 4, title: 'D', isCompleted: false, priority: Priority.High }),
    ]);

    it('applies multiple filters together', () => {
      const result = applyFilters(data, {
        isCompleted: false,
        priority: Priority.High,
        sortBy: 'title',
        sortDescending: true,
      });

      expect(result?.items).toHaveLength(2);
      expect(result?.items.map((t) => t.title)).toEqual(['D', 'A']);
      expect(result?.totalCount).toBe(2);
    });
  });
});
