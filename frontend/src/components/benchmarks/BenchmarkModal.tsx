import React from 'react';
import { Modal } from '../ui';
import { BenchmarkForm } from './BenchmarkForm';
import type { BenchmarkSchemaType } from '../../schemas/benchmarkSchema';
import type { BenchmarkDefinition, AthleteBenchmark } from '../../types';

export interface BenchmarkModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback when the modal should close */
  onClose: () => void;
  /** Callback when form is submitted */
  onSubmit: (data: BenchmarkSchemaType) => void;
  /** The benchmark definition being recorded/edited */
  definition: BenchmarkDefinition | null;
  /** Existing benchmark data for edit mode */
  existingBenchmark?: AthleteBenchmark | null;
  /** Whether the form is submitting */
  isSubmitting?: boolean;
}

/**
 * Modal wrapper for benchmark add/edit form
 */
export const BenchmarkModal: React.FC<BenchmarkModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  definition,
  existingBenchmark,
  isSubmitting = false,
}) => {
  const isEditMode = !!existingBenchmark;
  const title = isEditMode ? `Edit ${definition?.name}` : `Add ${definition?.name}`;

  // Don't render if no definition
  if (!definition) {
    return null;
  }

  const handleClose = () => {
    if (!isSubmitting) {
      onClose();
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={title}
      size="md"
      closeOnBackdropClick={!isSubmitting}
      closeOnEscape={!isSubmitting}
    >
      <BenchmarkForm
        definition={definition}
        existingBenchmark={existingBenchmark}
        onSubmit={onSubmit}
        onCancel={handleClose}
        isSubmitting={isSubmitting}
      />
    </Modal>
  );
};
