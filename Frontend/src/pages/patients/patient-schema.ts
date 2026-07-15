import { z } from 'zod'

function requiredText(label: string, maxLength: number) {
  return z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .max(maxLength, `${label} must be at most ${maxLength} characters`)
}

const birthDateSchema = z
  .string()
  .regex(/^\d{4}-\d{2}-\d{2}$/, 'Birth date is required')
  .refine((value) => {
    const date = new Date(`${value}T00:00:00Z`)
    return !Number.isNaN(date.getTime()) && date.toISOString().startsWith(value)
  }, 'Birth date is invalid')
  .refine(
    value => value <= currentLocalDate(),
    'Birth date cannot be in the future',
  )

function currentLocalDate(): string {
  const today = new Date()
  const year = today.getFullYear()
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day = String(today.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const patientBaseSchema = z.object({
  inn: z
    .string()
    .trim()
    .regex(/^\d{14}$/, 'INN must contain exactly 14 digits'),
  firstName: requiredText('First name', 100),
  lastName: requiredText('Last name', 100),
  middleName: requiredText('Middle name', 100),
  birthDate: birthDateSchema,
  gender: z.union([z.literal(1), z.literal(2)]),
  address: requiredText('Address', 500),
  address2: requiredText('Address 2', 500),
  phone: requiredText('Phone', 50),
  regionId: z.number().int().positive('Region is required'),
  districtId: z.number().int().positive('District is required'),
})

export const createPatientSchema = patientBaseSchema

export const updatePatientSchema = patientBaseSchema.extend({
  groupId: z.number().int().positive('Group is required'),
  specialStatus: z.boolean(),
  isActive: z.boolean(),
})
