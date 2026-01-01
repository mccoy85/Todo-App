/* @vitest-environment jsdom */
import '@testing-library/jest-dom/vitest';
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { TodoItemActions } from '../components/TodoItemActions';

describe('TodoItemActions', () => {
  it('shows only restore action for deleted view', () => {
    render(
      <TodoItemActions
        isDeletedView
        isToggling={false}
        isDeleting={false}
        isRestoring={false}
        onEdit={vi.fn()}
        onDuplicate={vi.fn()}
        onDelete={vi.fn()}
        onRestore={vi.fn()}
      />
    );

    expect(screen.getAllByRole('button')).toHaveLength(1);
  });

  it('shows edit, duplicate, and delete actions for active view', () => {
    render(
      <TodoItemActions
        isDeletedView={false}
        isToggling={false}
        isDeleting={false}
        isRestoring={false}
        onEdit={vi.fn()}
        onDuplicate={vi.fn()}
        onDelete={vi.fn()}
        onRestore={vi.fn()}
      />
    );

    expect(screen.getAllByRole('button')).toHaveLength(3);
  });
});
