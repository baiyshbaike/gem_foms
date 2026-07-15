import type { PatientGender } from '@/services/types/dialysis'

export interface DerivedPatientIdentity {
  birthDate: string
  gender: PatientGender
}

export function derivePatientIdentityFromInn(inn: string): DerivedPatientIdentity | null {
  if (!/^\d{14}$/.test(inn)) {
    return null
  }

  const genderCode = Number(inn[0])
  const day = Number(inn.slice(1, 3))
  const month = Number(inn.slice(3, 5))
  const year = Number(inn.slice(5, 9))

  const gender = genderCode === 2
    ? 1
    : genderCode === 1
      ? 2
      : null

  if (gender === null || year < 1) {
    return null
  }

  const date = new Date(0)
  date.setUTCHours(0, 0, 0, 0)
  date.setUTCFullYear(year, month - 1, day)

  if (
    date.getUTCFullYear() !== year
    || date.getUTCMonth() !== month - 1
    || date.getUTCDate() !== day
    || date.getTime() > Date.now()
  ) {
    return null
  }

  return {
    birthDate: [
      String(year).padStart(4, '0'),
      String(month).padStart(2, '0'),
      String(day).padStart(2, '0'),
    ].join('-'),
    gender: gender as PatientGender,
  }
}
