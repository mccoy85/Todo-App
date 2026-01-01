import type {
  Todo,
  TodoListResponse,
  CreateTodoRequest,
  UpdateTodoRequest,
  TodoQueryParams,
} from '../types/todo';
import { message } from 'antd';

const API_HOST = import.meta.env.VITE_API_HOST || 'localhost';
const API_PORT = import.meta.env.VITE_API_PORT || '5121';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || `http://${API_HOST}:${API_PORT}/api/todo`;
const DEFAULT_BATCH_SIZE = Number(import.meta.env.VITE_API_BATCH_SIZE) || 100;

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const error = await response.text();
    message.error(error || `Request failed (${response.status})`);
    throw new Error(error || `HTTP error ${response.status}`);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return response.json();
}

export const todoApi = {
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
      ? `${API_BASE_URL}?${searchParams}`
      : API_BASE_URL;

    const response = await fetch(url);
    return handleResponse<TodoListResponse>(response);
  },

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
      ? `${API_BASE_URL}/deleted?${searchParams}`
      : `${API_BASE_URL}/deleted`;

    const response = await fetch(url);
    return handleResponse<TodoListResponse>(response);
  },

  async getById(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/${id}`);
    return handleResponse<Todo>(response);
  },

  async getAllFull(batchSize: number = DEFAULT_BATCH_SIZE): Promise<TodoListResponse> {
    let page = 1;
    let items: Todo[] = [];
    let totalCount = 0;

    while (true) {
      const response = await todoApi.getAll({ page, pageSize: batchSize });
      totalCount = response.totalCount;
      items = items.concat(response.items);
      if (items.length >= totalCount || response.items.length === 0) {
        break;
      }
      page += 1;
    }

    return {
      items,
      totalCount,
      page: 1,
      pageSize: items.length || batchSize,
    };
  },

  async create(todo: CreateTodoRequest): Promise<Todo> {
    const response = await fetch(API_BASE_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(todo),
    });
    return handleResponse<Todo>(response);
  },

  async update(id: number, todo: UpdateTodoRequest): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(todo),
    });
    return handleResponse<Todo>(response);
  },

  async toggle(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/${id}/toggle`, {
      method: 'PATCH',
    });
    return handleResponse<Todo>(response);
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
    });
    return handleResponse<void>(response);
  },

  async getDeletedFull(batchSize: number = DEFAULT_BATCH_SIZE): Promise<TodoListResponse> {
    let page = 1;
    let items: Todo[] = [];
    let totalCount = 0;

    while (true) {
      const response = await todoApi.getDeleted({ page, pageSize: batchSize });
      totalCount = response.totalCount;
      items = items.concat(response.items);
      if (items.length >= totalCount || response.items.length === 0) {
        break;
      }
      page += 1;
    }

    return {
      items,
      totalCount,
      page: 1,
      pageSize: items.length || batchSize,
    };
  },

  async restore(id: number): Promise<Todo> {
    const response = await fetch(`${API_BASE_URL}/${id}/restore`, {
      method: 'PATCH',
    });
    return handleResponse<Todo>(response);
  },
};
