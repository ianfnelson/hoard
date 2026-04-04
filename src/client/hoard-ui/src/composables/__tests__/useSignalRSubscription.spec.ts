import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref } from 'vue'
import { useSignalRSubscription } from '../useSignalRSubscription'

const mockInvoke = vi.fn()
const mockStart = vi.fn()
const mockOn = vi.fn()
const mockOnReconnected = vi.fn()

vi.mock('@microsoft/signalr', () => {
  class MockHubConnectionBuilder {
    withUrl() {
      return this
    }
    withAutomaticReconnect() {
      return this
    }
    build() {
      return {
        invoke: mockInvoke,
        start: mockStart,
        on: mockOn,
        onreconnected: mockOnReconnected,
      }
    }
  }

  return { HubConnectionBuilder: MockHubConnectionBuilder }
})

const config = {
  hubUrl: '/hubs/test',
  eventName: 'TestUpdated',
  subscribeMethod: 'SubscribeToTest',
  unsubscribeMethod: 'UnsubscribeFromTest',
}

describe('useSignalRSubscription', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockStart.mockResolvedValue(undefined)
    mockInvoke.mockResolvedValue(undefined)
  })

  it('should start connection and invoke subscribe method', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)
    await subscribe(42)

    expect(mockStart).toHaveBeenCalledOnce()
    expect(mockInvoke).toHaveBeenCalledWith('SubscribeToTest', 42)
  })

  it('should not start a second connection on subsequent subscribes', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)
    await subscribe(1)
    await subscribe(2)

    expect(mockStart).toHaveBeenCalledOnce()
    expect(mockInvoke).toHaveBeenCalledTimes(2)
  })

  it('should register event handler that filters by currentId', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn().mockResolvedValue(undefined)

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)
    currentId.value = 5
    await subscribe(5)

    const eventHandler = mockOn.mock.calls.find((c) => c[0] === 'TestUpdated')?.[1]
    expect(eventHandler).toBeDefined()

    // Matching ID should trigger onEvent
    await eventHandler({ testId: 5 })
    expect(onEvent).toHaveBeenCalledOnce()

    // Non-matching ID should not trigger onEvent
    onEvent.mockClear()
    await eventHandler({ testId: 99 })
    expect(onEvent).not.toHaveBeenCalled()
  })

  it('should register reconnect handler that resubscribes', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)
    currentId.value = 7
    await subscribe(7)

    const reconnectHandler = mockOnReconnected.mock.calls[0]?.[0]
    expect(reconnectHandler).toBeDefined()

    mockInvoke.mockClear()
    await reconnectHandler()

    expect(mockInvoke).toHaveBeenCalledWith('SubscribeToTest', 7)
  })

  it('should not resubscribe on reconnect when currentId is null', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)
    currentId.value = 1
    await subscribe(1)

    const reconnectHandler = mockOnReconnected.mock.calls[0]?.[0]

    currentId.value = null
    mockInvoke.mockClear()
    await reconnectHandler()

    expect(mockInvoke).not.toHaveBeenCalled()
  })

  it('should invoke unsubscribe method when unsubscribing', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe, unsubscribe } = useSignalRSubscription(config, currentId, onEvent)
    currentId.value = 3
    await subscribe(3)

    mockInvoke.mockClear()
    await unsubscribe()

    expect(mockInvoke).toHaveBeenCalledWith('UnsubscribeFromTest', 3)
  })

  it('should not invoke unsubscribe when currentId is null', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe, unsubscribe } = useSignalRSubscription(config, currentId, onEvent)
    await subscribe(1)

    currentId.value = null
    mockInvoke.mockClear()
    await unsubscribe()

    expect(mockInvoke).not.toHaveBeenCalled()
  })

  it('should swallow subscribe invoke errors gracefully', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    mockInvoke.mockRejectedValueOnce(new Error('connection race'))

    const { subscribe } = useSignalRSubscription(config, currentId, onEvent)

    await expect(subscribe(1)).resolves.toBeUndefined()
  })

  it('should swallow unsubscribe invoke errors gracefully', async () => {
    const currentId = ref<number | null>(null)
    const onEvent = vi.fn()

    const { subscribe, unsubscribe } = useSignalRSubscription(config, currentId, onEvent)
    currentId.value = 1
    await subscribe(1)

    mockInvoke.mockRejectedValueOnce(new Error('disconnected'))

    await expect(unsubscribe()).resolves.toBeUndefined()
  })
})
