export type FieldType = 'date' | 'number' | 'instrument' | 'portfolio'

export interface OperationField {
  name: string
  label: string
  type: FieldType
  required: boolean
  min?: number
  max?: number
}

export interface OperationDefinition {
  key: string
  label: string
  section: string
  endpoint: string
  fields: OperationField[]
}
