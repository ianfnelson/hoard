import { ref, type Ref } from 'vue'
import * as signalR from '@microsoft/signalr'

export interface SignalRSubscriptionConfig {
  hubUrl: string
  eventName: string
  subscribeMethod: string
  unsubscribeMethod: string
}

export function useSignalRSubscription(
  config: SignalRSubscriptionConfig,
  currentId: Ref<number | null>,
  onEvent: () => Promise<void>
) {
  const hubConnection = ref<signalR.HubConnection | null>(null)
  const hubStarted = ref(false)

  async function ensureConnection() {
    if (hubConnection.value && hubStarted.value) {
      return
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(config.hubUrl)
      .withAutomaticReconnect()
      .build()

    connection.on(config.eventName, async (payload: { [key: string]: number }) => {
      const payloadId = Object.values(payload)[0]
      if (payloadId === currentId.value) {
        await onEvent()
      }
    })

    connection.onreconnected(async () => {
      if (currentId.value !== null) {
        try {
          await connection.invoke(config.subscribeMethod, currentId.value)
        } catch {
          // swallow reconnect edge cases
        }
      }
    })

    await connection.start()

    hubConnection.value = connection
    hubStarted.value = true
  }

  async function subscribe(id: number) {
    await ensureConnection()

    try {
      await hubConnection.value?.invoke(config.subscribeMethod, id)
    } catch {
      // connection race / reconnect – ignore
    }
  }

  async function unsubscribe() {
    if (hubConnection.value && hubStarted.value && currentId.value !== null) {
      try {
        await hubConnection.value.invoke(config.unsubscribeMethod, currentId.value)
      } catch {
        // ignore
      }
    }
  }

  return {
    subscribe,
    unsubscribe,
  }
}
