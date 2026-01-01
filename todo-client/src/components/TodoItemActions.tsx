import { Button, Popconfirm, Tooltip } from 'antd';
import { DeleteOutlined, EditOutlined, CopyOutlined, RollbackOutlined } from '@ant-design/icons';

interface TodoItemActionsProps {
  isDeletedView: boolean;
  isToggling: boolean;
  isDeleting: boolean;
  isRestoring: boolean;
  onEdit: () => void;
  onDuplicate: () => void;
  onDelete: () => void;
  onRestore: () => void;
}

export const TodoItemActions = ({
  isDeletedView,
  isToggling,
  isDeleting,
  isRestoring,
  onEdit,
  onDuplicate,
  onDelete,
  onRestore,
}: TodoItemActionsProps) => {
  if (isDeletedView) {
    return (
      <Tooltip title="Restore">
        <Button
          type="text"
          size="small"
          icon={<RollbackOutlined />}
          loading={isRestoring}
          onClick={onRestore}
          aria-label="Restore task"
        />
      </Tooltip>
    );
  }

  return (
    <>
      <Tooltip title="Edit">
        <Button type="text" size="small" icon={<EditOutlined />} onClick={onEdit} disabled={isToggling || isDeleting} aria-label="Edit task" />
      </Tooltip>
      <Tooltip title="Duplicate">
        <Button type="text" size="small" icon={<CopyOutlined />} onClick={onDuplicate} disabled={isToggling || isDeleting} aria-label="Duplicate task" />
      </Tooltip>
      <Popconfirm title="Delete task?" onConfirm={onDelete} okText="Delete" okButtonProps={{ danger: true }}>
        <Tooltip title="Delete">
          <Button type="text" size="small" icon={<DeleteOutlined />} loading={isDeleting} disabled={isToggling} danger aria-label="Delete task" />
        </Tooltip>
      </Popconfirm>
    </>
  );
};
