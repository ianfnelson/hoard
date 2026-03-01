export interface RunLogDto {
  runLogId: number
  runTypeId: number
  runTypeName: string
  runId: string
  startedAtUtc: string
  completedAtUtc: string | null
}
