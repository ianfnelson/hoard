/**
 * Formats a number as currency (GBP)
 * @param value The numeric value to format
 * @returns Formatted currency string (e.g., "£1,234.56")
 */
export function formatCurrency(value: number | null | undefined): string {
  if (value == null || isNaN(value)) return ''

  return value.toLocaleString('en-GB', {
    style: 'currency',
    currency: 'GBP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
}

/**
 * Formats a number as a percentage with 2 decimal places
 * @param value The numeric value to format
 * @returns Formatted percentage string (e.g., "12.34%")
 */
export function formatPercentage(value: number | null | undefined): string {
  if (value == null || isNaN(value)) return ''

  return `${value.toFixed(2)}%`
}

/**
 * Formats an "updated time" for display in the UI
 * - If within the last 24 hours: HH:mm format
 * - Otherwise: DD Mmm YYYY format (e.g., "23 Jan 2006")
 * @param date The date to format (Date object or ISO string)
 * @returns Formatted date string
 */
export function formatUpdatedTime(date: Date | string | null | undefined): string {
  if (!date) return ''

  const parsedDate = typeof date === 'string' ? new Date(date) : date

  if (isNaN(parsedDate.getTime())) return ''

  const now = new Date()
  const diffMs = now.getTime() - parsedDate.getTime()
  const diffHours = diffMs / (1000 * 60 * 60)

  if (diffHours < 24) {
    // Within last 24 hours - show time
    return parsedDate.toLocaleString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    })
  } else {
    // Older than 24 hours - show date
    return parsedDate.toLocaleString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    })
  }
}

/**
 * Formats a date for display in the UI
 * @param date The date to format (Date object or ISO string)
 * @returns Formatted date string (e.g. "15 Jan 2026")
 */
export function formatDate(date: Date | string | null | undefined): string {
  if (!date) return ''

  const parsedDate = typeof date === 'string' ? new Date(date) : date

  if (isNaN(parsedDate.getTime())) return ''

  return parsedDate.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

/**
 * Formats a date and time for display
 * @param date The date to format (Date object or ISO string)
 * @returns Formatted date-time string (e.g., "15 Jan 2026 14:30")
 */
export function formatDateTime(date: Date | string | null | undefined): string {
  if (!date) return ''

  const parsedDate = typeof date === 'string' ? new Date(date) : date

  if (isNaN(parsedDate.getTime())) return ''

  const datePart = parsedDate.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })

  const timePart = parsedDate.toLocaleString('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })

  return `${datePart} ${timePart}`
}

/**
 * Returns CSS class for trend-based coloring
 * @param value The numeric value to determine trend
 * @returns CSS class name ('text-positive', 'text-negative', or undefined)
 */
export function getTrendClass(value: number | null | undefined): string | undefined {
  if (value == null) return undefined
  if (value > 0) return 'text-positive'
  if (value < 0) return 'text-negative'
  return undefined
}

/**
 * Formats a boolean as Yes/No
 * @param value The boolean value to format
 * @returns "Yes" for true, "No" for false, or empty string for null/undefined
 */
export function formatYesNo(value: boolean | null | undefined): string {
  if (value == null) return ''
  return value ? 'Yes' : 'No'
}
