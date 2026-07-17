import { z } from 'zod'

function requiredText(label: string, maxLength: number) {
  return z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .max(maxLength, `${label} must be at most ${maxLength} characters`)
}

function optionalText(label: string, maxLength: number) {
  return z
    .string()
    .trim()
    .max(maxLength, `${label} must be at most ${maxLength} characters`)
    .transform(value => value || null)
}

export const tenantEditorSchema = z.object({
  code: requiredText('Code', 100)
    .transform(value => value.toUpperCase())
    .pipe(z.string().regex(
      /^[A-Z0-9][A-Z0-9_-]*$/,
      'Code may contain only Latin letters, numbers, hyphens and underscores',
    )),
  name: requiredText('Name', 200),
  address: optionalText('Address', 500),
  phone: requiredText('Phone', 50),
  regionId: z.number().int().positive('Region is required'),
  districtId: z.number().int().positive('District is required'),
  isActive: z.boolean(),
})

export const createTenantSchema = tenantEditorSchema.omit({ isActive: true })
export const updateTenantSchema = tenantEditorSchema
