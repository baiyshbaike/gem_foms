import { describe, expect, it } from 'vitest'

import { derivePatientIdentityFromInn } from './patient-identity'

describe('patient identity derivation', () => {
  it('derives a male patient and birth date from an INN', () => {
    expect(derivePatientIdentityFromInn('20101199012345')).toEqual({
      birthDate: '1990-01-01',
      gender: 1,
    })
  })

  it('derives a female patient and birth date from an INN', () => {
    expect(derivePatientIdentityFromInn('13112200012345')).toEqual({
      birthDate: '2000-12-31',
      gender: 2,
    })
  })

  it.each([
    '2010119901234',
    '30101199012345',
    '23102200112345',
    '20113200112345',
  ])('rejects an invalid INN: %s', (inn) => {
    expect(derivePatientIdentityFromInn(inn)).toBeNull()
  })
})
