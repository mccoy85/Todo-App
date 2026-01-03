import type {
  Todo,
  TodoListResponse,
  CreateTodoRequest,
  UpdateTodoRequest,
  TodoQueryParams,
} from '../types/todo';

const API_HOST = import.meta.env.VITE_API_HOST || 'localhost';
const API_PORT = import.meta.env.VITE_API_PORT || '5121';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || `http://${API_HOST}:${API_PORT}/api`;
const DEFAULT_BATCH_SIZE = Number(import.meta.env.VITE_API_BATCH_SIZE) || 100;

// Normalize API responses and throw errors with user-friendly messages.
const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    let errorMessage = `Request failed (${response.status})`;

    try {
      const errorData = await response.json();

      // Handle validation errors with structured error format
      if (errorData.errors && typeof errorData.errors === 'object') {
        // Extract all error messages from the errors object
        const messages = Object.values(errorData.errors)
          .flat()
          .filter((msg): msg is string => typeof msg === 'string');

        if (messages.length > 0) {
          errorMessage = messages.join(', ');
        } else if (errorData.message) {
          errorMessage = errorData.message;
        }
      } else if (errorData.message) {
        errorMessage = errorData.message;
      }
    } catch {
      // If JSON parsing fails, try to get plain text
      try {
        const text = await response.text();
        if (text) errorMessage = text;
      } catch {
        // Use default error message
      }
    }

    throw new Error(errorMessage);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return response.json();
};

// API client for todo operations.
export const todoApi = {
  // Fetch a paginated list of active todos with optional filtering and sorting.
  async getAll(params?: TodoQueryParams): Promise<TodoListResponse> {
    const searchParams = new URLSearchParams();
    if (params?.isCompleted !== undefined)
      searchParams.set('isCompleted', String(params.isCompleted));
    if (params?.priority !== undefined)
      searchParams.set('priority', String(params.priority));
    if (params?.sortBy) searchParams.set('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined)
      searchParams.set('sortDescending', String(params.sortDescending));
    if (params?.page) searchParams.set('page', String(params.page));
    if (params?.pageSize) searchParams.set('pageSize', String(params.pageSize));

    const url = searchParams.toString()
      ? `${API_BASE_URL}/todo?${searchParams}`
      : `${API_BASE_URL}/todo`;

    const response = await fetch(url);
    return handleResponse<TodoListResponse>(response);
  },

  // Fetch a paginated list of soft-deleted todos.
  async getDeleted(params?: TodoQueryParams): Promise<TodoListResponse> {
    const searchParams = new URLSearchParams();
    if (params?.isCompleted !== undefined)
      searchParams.set('isCompleted', String(params.isCompleted));
    if (params?.priority !== undefined)
      searchParams.set('priority', String(params.priority));
    if (params?.sortBy) searchParams.set('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined)
      searchParams.set('sortDescending', String(params.sortDescending));
    if (params?.page) searchParams.set('page', String(params.page));
    if (params?.pageSize) searchParams.set('pageSize', String(params.pageSize));

    const url = searchParams.toString()
      ? `${API_BASE_URL}/todo/deleted?${searchParams}`
      : `${API_BASE_URL}/todo/deleted`;

    const response = await fetch(url);
    return handleResponse<TodoListResponse>(response);
  },

  // Fetch a single todo by ID.
  async getById(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/todo/${id}`);
    return handleResponse<Todo>(response);
  },

  // Fetch all active todos by paginating through the entire dataset.
  async getAllFull(pageSize: number = DEFAULT_BATCH_SIZE): Promise<TodoListResponse> {
    const firstResponse = await todoApi.getAll({ page: 1, pageSize });
    const totalCount = firstResponse.totalCount;
    const totalPages = Math.ceil(totalCount / pageSize);
    let items: Todo[] = [...firstResponse.items];

    for (let page = 2; page <= totalPages; page++) {
      const response = await todoApi.getAll({ page, pageSize });
      items = items.concat(response.items);
    }

    return {
      items,
      totalCount,
      page: 1,
      pageSize: items.length || pageSize,
    };
  },

  // Create a new todo.
  async create(todo: CreateTodoRequest): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/todo`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(todo),
    });
    return handleResponse<Todo>(response);
  },

  // Update an existing todo.
  async update(id: number, todo: UpdateTodoRequest): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/todo/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(todo),
    });
    return handleResponse<Todo>(response);
  },

  // Toggle the completion status of a todo.
  async toggle(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/todo/${id}/toggle`, {
      method: 'PATCH',
    });
    return handleResponse<Todo>(response);
  },

  // Soft-delete a todo.
  async delete(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/todo/${id}`, {
      method: 'DELETE',
    });
    return handleResponse<void>(response);
  },

  // Fetch all soft-deleted todos by paginating through the entire dataset.
  async getDeletedFull(pageSize: number = DEFAULT_BATCH_SIZE): Promise<TodoListResponse> {
    const firstResponse = await todoApi.getDeleted({ page: 1, pageSize });
    const totalCount = firstResponse.totalCount;
    const totalPages = Math.ceil(totalCount / pageSize);
    let items: Todo[] = [...firstResponse.items];

    for (let page = 2; page <= totalPages; page++) {
      const response = await todoApi.getDeleted({ page, pageSize });
      items = items.concat(response.items);
    }

    return {
      items,
      totalCount,
      page: 1,
      pageSize: items.length || pageSize,
    };
  },

  // Restore a soft-deleted todo.
  async restore(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/todo/${id}/restore`, {
      method: 'PATCH',
    });
    return handleResponse<Todo>(response);
  },
};
