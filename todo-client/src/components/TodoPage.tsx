import { useState } from 'react';
import { Layout, Card, Button, Flex, Typography, Select, Segmented, Badge, Pagination, Spin, Empty, Tag, Checkbox, Tooltip, App } from 'antd';
import dayjs from 'dayjs';
import {
  PlusOutlined,
  ReloadOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  UnorderedListOutlined,
  SortAscendingOutlined,
  SortDescendingOutlined,
  DeleteOutlined,
  CalendarOutlined,
  FlagOutlined,
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { useFilteredTodos, useCreateTodo, useUpdateTodo, useToggleTodo, useDeleteTodo, useDeletedTodos, useRestoreTodo } from '../hooks/useTodos';
import { useTodoFilters } from '../hooks/useTodoFilters';
import { TodoItemActions } from './TodoItemActions';
import { TodoFormModal } from './TodoFormModal';
import type { Todo, CreateTodoRequest, StatusFilter } from '../types/todo';
import { Priority, PRIORITY_OPTIONS, SORT_OPTIONS } from '../types/todo';

const { Header, Content } = Layout;
const { Title, Text } = Typography;

// Visual styling for each priority level (color, label, and whether to show a flag icon).
const priorityConfig: Record<Priority, { color: string; label: string; icon: boolean }> = {
  [Priority.Low]: { color: '#52c41a', label: 'Low', icon: false },
  [Priority.Medium]: { color: '#faad14', label: 'Medium', icon: false },
  [Priority.High]: { color: '#ff4d4f', label: 'High', icon: true },
};

// Convert due dates into a user-friendly label and status flags.
const formatDueDate = (dateString: string): { text: string; isOverdue: boolean; isToday: boolean } => {
  // Parse the UTC date from the server and convert to local date
  const dueDate = dayjs.utc(dateString).local().startOf('day');
  const today = dayjs().startOf('day');
  const diffDays = dueDate.diff(today, 'day');

  if (diffDays < 0) return { text: `${Math.abs(diffDays)} day${Math.abs(diffDays) > 1 ? 's' : ''} overdue`, isOverdue: true, isToday: false };
  if (diffDays === 0) return { text: 'Due today', isOverdue: false, isToday: true };
  if (diffDays === 1) return { text: 'Due tomorrow', isOverdue: false, isToday: false };
  if (diffDays <= 7) return { text: `Due in ${diffDays} days`, isOverdue: false, isToday: false };
  return { text: dueDate.format('MMM D'), isOverdue: false, isToday: false };
};

// Main task list view with filters, pagination, and CRUD actions.
export const TodoPage = () => {
  const { message } = App.useApp();
  const [formOpen, setFormOpen] = useState(false);
  const [editingTodo, setEditingTodo] = useState<Todo | null>(null);
  const [duplicateValues, setDuplicateValues] = useState<CreateTodoRequest | null>(null);

  const filters = useTodoFilters();
  const {
    data: activeData,
    counts,
    isLoading: activeLoading,
    isValidating: activeValidating,
    isError: activeError,
    refetch: refetchActive,
  } = useFilteredTodos(filters.queryParams);
  const {
    data: deletedData,
    totalCount: deletedTotalCount,
    isLoading: deletedLoading,
    isValidating: deletedValidating,
    isError: deletedError,
    refetch: refetchDeleted,
  } = useDeletedTodos(filters.queryParams);
  const createMutation = useCreateTodo();
  const updateMutation = useUpdateTodo();
  const toggleMutation = useToggleTodo();
  const deleteMutation = useDeleteTodo();
  const restoreMutation = useRestoreTodo();

  const isSubmitting = createMutation.isPending || updateMutation.isPending;
  const isDeletedView = filters.statusFilter === 'deleted';
  const data = isDeletedView ? deletedData : activeData;
  const isLoading = isDeletedView ? deletedLoading : activeLoading;
  const isValidating = isDeletedView ? deletedValidating : activeValidating;
  const isError = isDeletedView ? deletedError : activeError;
  const refetch = isDeletedView ? refetchDeleted : refetchActive;
  const deletedCount = deletedTotalCount;

  // Handle create or update submission from the form modal.
  const handleFormSubmit = (todoData: CreateTodoRequest) => {
    if (editingTodo) {
      updateMutation.mutate(
        { id: editingTodo.id, todo: { ...todoData, isCompleted: editingTodo.isCompleted } },
        {
          onSuccess: () => { message.success('Task updated'); closeForm(); },
          onError: (error) => message.error(error.message),
        }
      );
    } else {
      createMutation.mutate(todoData, {
        onSuccess: () => { message.success('Task created'); closeForm(); },
        onError: (error) => message.error(error.message),
      });
    }
  };

  // Reset form state and close the modal.
  const closeForm = () => {
    setEditingTodo(null);
    setDuplicateValues(null);
    setFormOpen(false);
  };

  // Open the form modal in edit mode with existing todo data.
  const openEditForm = (todo: Todo) => {
    setEditingTodo(todo);
    setDuplicateValues(null);
    setFormOpen(true);
  };

  // Open the form modal pre-filled with a copy of an existing todo.
  const openDuplicateForm = (todo: Todo) => {
    setEditingTodo(null);
    setDuplicateValues({
      title: `${todo.title} (copy)`,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
    });
    setFormOpen(true);
  };

  return (
    <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Header style={{ background: '#fff', padding: '0 16px', borderBottom: '1px solid #f0f0f0' }}>
        <Flex align="center" justify="space-between" style={{ height: '100%' }}>
          <Flex align="center" gap={12}>
            <img src="/tasky.svg" alt="Tasky logo" style={{ width: 36, height: 36, borderRadius: 12 }} />
            <Text style={{ fontSize: 18, fontWeight: 600 }}>Tasky</Text>
          </Flex>
          <Link to="/">
            <Button type="text">Back to home</Button>
          </Link>
        </Flex>
      </Header>
      <Content style={{ padding: '24px 16px', maxWidth: 720, margin: '0 auto', width: '100%' }}>
        {/* Header */}
        <Card style={{ marginBottom: 24, borderRadius: 12 }} styles={{ body: { padding: '24px 28px' } }}>
          <Flex justify="space-between" align="center">
            <div>
              <Title level={2} style={{ margin: 0, fontWeight: 600 }}>
                {isDeletedView ? 'Deleted Tasks' : 'My Tasks'}
              </Title>
              <Flex align="center" gap={8}>
                <Text type="secondary">
                  {isDeletedView ? `${deletedCount} deleted tasks` : `${counts.total} total tasks`}
                </Text>
                {isValidating && !isLoading && <Spin size="small" />}
              </Flex>
            </div>
            {!isDeletedView && (
              <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => setFormOpen(true)} loading={isSubmitting}>
                Add Task
              </Button>
            )}
          </Flex>
        </Card>

        {/* Filters */}
        <Card style={{ marginBottom: 24, borderRadius: 12 }} styles={{ body: { padding: '16px 20px' } }}>
          <Flex vertical gap={16}>
            <Segmented
              value={filters.statusFilter}
              onChange={(v) => filters.setStatusFilter(v as StatusFilter)}
              block
              options={[
                { value: 'all', label: <Flex align="center" gap={6}><UnorderedListOutlined /><span>All</span><Badge count={counts.total} showZero color="#8c8c8c" /></Flex> },
                { value: 'active', label: <Flex align="center" gap={6}><ClockCircleOutlined /><span>Active</span><Badge count={counts.active} showZero color="#1677ff" /></Flex> },
                { value: 'completed', label: <Flex align="center" gap={6}><CheckCircleOutlined /><span>Completed</span><Badge count={counts.completed} showZero color="#52c41a" /></Flex> },
                { value: 'deleted', label: <Flex align="center" gap={6}><DeleteOutlined /><span>Deleted</span><Badge count={deletedCount} showZero color="#ff4d4f" /></Flex> },
              ]}
            />
            <Flex gap={12} wrap="wrap">
              <Select style={{ minWidth: 140 }} placeholder="Priority" allowClear value={filters.priorityFilter} onChange={filters.setPriorityFilter}
                options={PRIORITY_OPTIONS} />
              <Select style={{ minWidth: 150 }} placeholder="Sort by" allowClear value={filters.sortBy} onChange={filters.setSortBy}
                options={SORT_OPTIONS} />
              {filters.sortBy && (
                <Button icon={filters.sortDescending ? <SortDescendingOutlined /> : <SortAscendingOutlined />} onClick={filters.toggleSortDirection}>
                  {filters.sortDescending ? 'Desc' : 'Asc'}
                </Button>
              )}
            </Flex>
          </Flex>
        </Card>

        {/* List */}
        <Card style={{ borderRadius: 12 }} styles={{ body: { padding: data?.items.length ? '16px 20px' : '0' } }}>
          {isLoading ? (
            <Flex justify="center" align="center" style={{ padding: 80 }}><Spin size="large" /></Flex>
          ) : isError ? (
            <Empty description={<Text type="secondary">Unable to load tasks</Text>} style={{ padding: 60 }}>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>Retry</Button>
            </Empty>
          ) : !data?.items.length ? (
            <Empty
              description={
                <Text type="secondary">
                  {isDeletedView
                    ? 'No deleted tasks yet'
                    : filters.hasFilters
                      ? 'No todos match filters'
                      : 'No todos yet'}
                </Text>
              }
              style={{ padding: 60 }}
            >
              {!filters.hasFilters && !isDeletedView && (
                <Button type="primary" icon={<PlusOutlined />} onClick={() => setFormOpen(true)}>Create First Todo</Button>
              )}
            </Empty>
          ) : (
            <Flex vertical gap={12}>
              {data.items.map((todo) => {
                const priority = priorityConfig[todo.priority];
                const dueInfo = todo.dueDate ? formatDueDate(todo.dueDate) : null;
                const isToggling = toggleMutation.isPending && toggleMutation.variables === todo.id;
                const isDeleting = deleteMutation.isPending && deleteMutation.variables === todo.id;
                const isRestoring = restoreMutation.isPending && restoreMutation.variables === todo.id;

                return (
                  <div key={todo.id} style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '14px 16px', background: todo.isCompleted ? '#fafafa' : '#fff', borderRadius: 10, border: '1px solid #f0f0f0', opacity: isDeleting || isRestoring ? 0.5 : 1 }}>
                    <Checkbox
                      checked={todo.isCompleted}
                      onChange={() => toggleMutation.mutate(todo.id, {
                        onSuccess: () => message.success(todo.isCompleted ? 'Task marked as active' : 'Task marked as completed'),
                      })}
                      disabled={isToggling || isDeleting || isDeletedView}
                      style={{ marginTop: 2 }}
                    />
                    <div style={{ flex: 1, minWidth: 0 }}>
                      <Flex align="center" gap={8} wrap="wrap">
                        <Text strong delete={todo.isCompleted} style={{ fontSize: 15, color: todo.isCompleted ? '#8c8c8c' : '#262626' }}>{todo.title}</Text>
                        {priority.icon && <Tooltip title="High Priority"><FlagOutlined style={{ color: priority.color, fontSize: 12 }} /></Tooltip>}
                        <Tag color={todo.isCompleted ? 'default' : priority.color} style={{ margin: 0, fontSize: 11 }}>{priority.label}</Tag>
                      </Flex>
                      {todo.description && <Text type="secondary" style={{ display: 'block', marginTop: 6, fontSize: 13 }}>{todo.description}</Text>}
                      {dueInfo && !todo.isCompleted && (
                        <Flex align="center" gap={4} style={{ marginTop: 8 }}>
                          <CalendarOutlined style={{ fontSize: 12, color: dueInfo.isOverdue ? '#ff4d4f' : dueInfo.isToday ? '#faad14' : '#8c8c8c' }} />
                          <Text style={{ fontSize: 12, color: dueInfo.isOverdue ? '#ff4d4f' : dueInfo.isToday ? '#faad14' : '#8c8c8c', fontWeight: dueInfo.isOverdue || dueInfo.isToday ? 500 : 400 }}>{dueInfo.text}</Text>
                        </Flex>
                      )}
                    </div>
                    <Flex gap={4}>
                      <TodoItemActions
                        isDeletedView={isDeletedView}
                        isToggling={isToggling}
                        isDeleting={isDeleting}
                        isRestoring={isRestoring}
                        onEdit={() => openEditForm(todo)}
                        onDuplicate={() => openDuplicateForm(todo)}
                        onDelete={() => deleteMutation.mutate(todo.id, { onSuccess: () => message.success('Task deleted') })}
                        onRestore={() => restoreMutation.mutate(todo.id, { onSuccess: () => message.success('Task restored') })}
                      />
                    </Flex>
                  </div>
                );
              })}
              <Flex justify="center" style={{ marginTop: 24 }}>
                <Pagination current={filters.page} pageSize={filters.pageSize} total={data.totalCount} onChange={filters.setPage}
                  showSizeChanger pageSizeOptions={filters.pageSizeOptions.map(String)} onShowSizeChange={(_, size) => filters.setPageSize(size)}
                  showTotal={(total, range) => `${range[0]}-${range[1]} of ${total}`} />
              </Flex>
            </Flex>
          )}
        </Card>
      </Content>

      <TodoFormModal
        open={formOpen}
        editingTodo={editingTodo}
        initialValues={duplicateValues ?? undefined}
        isSubmitting={isSubmitting}
        onSubmit={handleFormSubmit}
        onCancel={closeForm}
      />
    </Layout>
  );
};
