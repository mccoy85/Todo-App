/* @vitest-environment jsdom */
import '@testing-library/jest-dom/vitest';
import { describe, it, expect, vi, beforeAll } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { TodoPage } from '../components/TodoPage';
import { Priority } from '../types/todo';

const mockUseTodoFilters = vi.fn();
const mockUseFilteredTodos = vi.fn();
const mockUseDeletedTodos = vi.fn();

vi.mock('../hooks/useTodoFilters', () => ({
  useTodoFilters: () => mockUseTodoFilters(),
}));

vi.mock('../hooks/useTodos', () => ({
  useFilteredTodos: () => mockUseFilteredTodos(),
  useDeletedTodos: () => mockUseDeletedTodos(),
  useCreateTodo: () => ({ isPending: false, mutate: vi.fn() }),
  useUpdateTodo: () => ({ isPending: false, mutate: vi.fn() }),
  useToggleTodo: () => ({ isPending: false, mutate: vi.fn(), variables: undefined }),
  useDeleteTodo: () => ({ isPending: false, mutate: vi.fn(), variables: undefined }),
  useRestoreTodo: () => ({ isPending: false, mutate: vi.fn(), variables: undefined }),
}));

beforeAll(() => {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation(() => ({
      matches: false,
      media: '',
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  });
});

const renderPage = () =>
  render(
    <MemoryRouter>
      <TodoPage />
    </MemoryRouter>
  );

describe('TodoPage', () => {
  it('renders the active view header and add button', () => {
    mockUseTodoFilters.mockReturnValue({
      queryParams: { page: 1, pageSize: 10 },
      statusFilter: 'all',
      priorityFilter: undefined,
      sortBy: undefined,
      sortDescending: true,
      page: 1,
      pageSize: 10,
      pageSizeOptions: [5, 10, 20],
      hasFilters: false,
      setStatusFilter: vi.fn(),
      setPriorityFilter: vi.fn(),
      setSortBy: vi.fn(),
      toggleSortDirection: vi.fn(),
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    mockUseFilteredTodos.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      counts: { total: 0, active: 0, completed: 0 },
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    mockUseDeletedTodos.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      totalCount: 0,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText('My Tasks')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add task/i })).toBeInTheDocument();
  });

  it('renders the deleted view without the add button', () => {
    mockUseTodoFilters.mockReturnValue({
      queryParams: { page: 1, pageSize: 10 },
      statusFilter: 'deleted',
      priorityFilter: undefined,
      sortBy: undefined,
      sortDescending: true,
      page: 1,
      pageSize: 10,
      pageSizeOptions: [5, 10, 20],
      hasFilters: false,
      setStatusFilter: vi.fn(),
      setPriorityFilter: vi.fn(),
      setSortBy: vi.fn(),
      toggleSortDirection: vi.fn(),
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    mockUseFilteredTodos.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      counts: { total: 0, active: 0, completed: 0 },
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    mockUseDeletedTodos.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      totalCount: 0,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText('Deleted Tasks')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /add task/i })).toBeNull();
  });

  it('shows a todo item when data exists', () => {
    mockUseTodoFilters.mockReturnValue({
      queryParams: { page: 1, pageSize: 10 },
      statusFilter: 'all',
      priorityFilter: undefined,
      sortBy: undefined,
      sortDescending: true,
      page: 1,
      pageSize: 10,
      pageSizeOptions: [5, 10, 20],
      hasFilters: false,
      setStatusFilter: vi.fn(),
      setPriorityFilter: vi.fn(),
      setSortBy: vi.fn(),
      toggleSortDirection: vi.fn(),
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    mockUseFilteredTodos.mockReturnValue({
      data: {
        items: [
          {
            id: 1,
            title: 'Write tests',
            description: 'Add coverage',
            isCompleted: false,
            createdAt: new Date().toISOString(),
            priority: Priority.Medium,
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 10,
      },
      counts: { total: 1, active: 1, completed: 0 },
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    mockUseDeletedTodos.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10 },
      totalCount: 0,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });

    renderPage();

    expect(screen.getByText('Write tests')).toBeInTheDocument();
    expect(screen.getByText('Add coverage')).toBeInTheDocument();
  });
});
