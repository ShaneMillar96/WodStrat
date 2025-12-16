import { z } from 'zod';

/**
 * Password validation requirements:
 * - Minimum 8 characters
 * - At least 1 capital letter
 * - At least 1 special character
 */
const passwordSchema = z
  .string()
  .min(8, 'Password must be at least 8 characters')
  .regex(/[A-Z]/, 'Password must contain at least 1 capital letter')
  .regex(
    /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/,
    'Password must contain at least 1 special character'
  );

/**
 * Login form schema
 */
export const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Please enter a valid email address'),
  password: z
    .string()
    .min(1, 'Password is required'),
});

/**
 * Register form schema with password confirmation
 */
export const registerSchema = z
  .object({
    email: z
      .string()
      .min(1, 'Email is required')
      .email('Please enter a valid email address'),
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'Please confirm your password'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

/**
 * Type inferred from login schema
 */
export type LoginSchemaType = z.infer<typeof loginSchema>;

/**
 * Type inferred from register schema
 */
export type RegisterSchemaType = z.infer<typeof registerSchema>;
