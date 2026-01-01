import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest';
import { todoApi } from '../services/todoApi';
import type { TodoListResponse } from '../types/todo';

vi.mock('antd', () => ({
  message: {
    error: vi.fn(),
  },
}));

const baseUrl = import.meta.env.VITE_API_BASE_URL;

const createResponse = (data: unknown, status = 200) => ({
  ok: status >= 200 && status < 300,
  status,
  json: async () => data,
  text: async () => (typeof data === 'string' ? data : ''),
});

describe('todoApi', () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
    fetchMock.mockReset();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('builds a filtered list request', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ items: [], totalCount: 0, page: 1, pageSize: 10 }));

    await todoApi.getAll({ isCompleted: false, page: 2, pageSize: 5, sortBy: 'title', sortDescending: true });

    expect(fetchMock).toHaveBeenCalledWith(
      `${baseUrl}/todo?isCompleted=false&sortBy=title&sortDescending=true&page=2&pageSize=5`
    );
  });

  it('requests deleted todos with filters', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ items: [], totalCount: 0, page: 1, pageSize: 10 }));

    await todoApi.getDeleted({ priority: 2, page: 1, pageSize: 10 });

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/deleted?priority=2&page=1&pageSize=10`);
  });

  it('fetches a single todo by id', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ id: 1, title: 'Test' }));

    await todoApi.getById(1);

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/1`);
  });

  it('creates a todo', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ id: 1, title: 'Created' }));

    await todoApi.create({ title: 'Created', priority: 1 });

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title: 'Created', priority: 1 }),
    });
  });

  it('updates a todo', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ id: 1, title: 'Updated' }));

    await todoApi.update(1, { title: 'Updated', priority: 0, isCompleted: false });

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/1`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title: 'Updated', priority: 0, isCompleted: false }),
    });
  });

  it('toggles a todo', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ id: 1, isCompleted: true }));

    await todoApi.toggle(1);

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/1/toggle`, { method: 'PATCH' });
  });

  it('deletes a todo', async () => {
    fetchMock.mockResolvedValueOnce(createResponse('', 204));

    await todoApi.delete(1);

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/1`, { method: 'DELETE' });
  });

  it('restores a todo', async () => {
    fetchMock.mockResolvedValueOnce(createResponse({ id: 1, isDeleted: false }));

    await todoApi.restore(1);

    expect(fetchMock).toHaveBeenCalledWith(`${baseUrl}/todo/1/restore`, { method: 'PATCH' });
  });

  it('batches full list retrieval', async () => {
    const getAllSpy = vi.spyOn(todoApi, 'getAll');
    getAllSpy.mockImplementation(async (params) => {
      if (params?.page === 1) {
        return { items: [{ id: 1 }], totalCount: 2, page: 1, pageSize: 1 } as TodoListResponse;
      }
      return { items: [{ id: 2 }], totalCount: 2, page: 2, pageSize: 1 } as TodoListResponse;
    });

    const result = await todoApi.getAllFull(1);

    expect(result.items).toHaveLength(2);
    expect(getAllSpy).toHaveBeenCalledTimes(2);

    getAllSpy.mockRestore();
  });

  it('batches deleted list retrieval', async () => {
    const getDeletedSpy = vi.spyOn(todoApi, 'getDeleted');
    getDeletedSpy.mockImplementation(async (params) => {
      if (params?.page === 1) {
        return { items: [{ id: 1 }], totalCount: 2, page: 1, pageSize: 1 } as TodoListResponse;
      }
      return { items: [{ id: 2 }], totalCount: 2, page: 2, pageSize: 1 } as TodoListResponse;
    });

    const result = await todoApi.getDeletedFull(1);

    expect(result.items).toHaveLength(2);
    expect(getDeletedSpy).toHaveBeenCalledTimes(2);

    getDeletedSpy.mockRestore();
  });
});
