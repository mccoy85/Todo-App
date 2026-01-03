import { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, Flex } from 'antd';
import dayjs from 'dayjs';
import type { Todo, CreateTodoRequest } from '../types/todo';
import { Priority, PRIORITY_OPTIONS } from '../types/todo';

interface TodoFormModalProps {
  open: boolean;
  editingTodo: Todo | null;
  initialValues?: Partial<CreateTodoRequest>;
  isSubmitting: boolean;
  onSubmit: (data: CreateTodoRequest) => void;
  onCancel: () => void;
}

// Modal form for creating or editing a todo.
export const TodoFormModal = ({ open, editingTodo, initialValues, isSubmitting, onSubmit, onCancel }: TodoFormModalProps) => {
  const [form] = Form.useForm();

  // Watch the title field to derive form validity reactively.
  const titleValue = Form.useWatch('title', form);
  const isFormValid = Boolean(titleValue?.trim());

  // Populate form fields when opening for edit or duplicate.
  useEffect(() => {
    if (open) {
      if (editingTodo) {
        form.setFieldsValue({
          title: editingTodo.title,
          description: editingTodo.description ?? '',
          priority: editingTodo.priority,
          dueDate: editingTodo.dueDate ? dayjs(editingTodo.dueDate) : null,
        });
      } else if (initialValues) {
        form.setFieldsValue({
          title: initialValues.title ?? '',
          description: initialValues.description ?? '',
          priority: initialValues.priority ?? Priority.Low,
          dueDate: initialValues.dueDate ? dayjs(initialValues.dueDate) : null,
        });
      }
    } else {
      form.resetFields();
    }
  }, [open, editingTodo, initialValues, form]);

  // Validate and submit the form data.
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();

      const payload = {
        title: values.title.trim(),
        description: values.description?.trim() || undefined,
        priority: values.priority,
        dueDate: values.dueDate
          ? values.dueDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss')
          : undefined,
      };
      onSubmit(payload);
    } catch { /* validation failed */ }
  };

  return (
    <Modal
      title={editingTodo ? 'Edit Task' : 'New Task'}
      open={open}
      onOk={handleSubmit}
      onCancel={onCancel}
      okText={editingTodo ? 'Save' : 'Create'}
      confirmLoading={isSubmitting}
      okButtonProps={{ disabled: !isFormValid }}
      destroyOnHidden
      centered
      width={480}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item name="title" label="Title" rules={[{ required: true, message: 'Required' }, { max: 200 }]}>
          <Input placeholder="What needs to be done?" size="large" />
        </Form.Item>
        <Form.Item name="description" label="Description" rules={[{ max: 1000 }]}>
          <Input.TextArea rows={3} placeholder="Details..." showCount maxLength={1000} />
        </Form.Item>
        <Flex gap={16}>
          <Form.Item name="priority" label="Priority" style={{ flex: 1 }} initialValue={Priority.Low} rules={[{ required: true, message: 'Required' }]}>
            <Select size="large" options={PRIORITY_OPTIONS} />
          </Form.Item>
          <Form.Item
            name="dueDate"
            label="Due Date"
            style={{ flex: 1 }}
            rules={[
              {
                validator: (_, value) =>
                  !value || value >= dayjs().startOf('day')
                    ? Promise.resolve()
                    : Promise.reject('Cannot be in the past'),
              },
            ]}
          >
            <DatePicker
              style={{ width: '100%' }}
              size="large"
              format="MMM D, YYYY"
              disabledDate={(d) => d < dayjs().startOf('day')}
            />
          </Form.Item>
        </Flex>
      </Form>
    </Modal>
  );
};
