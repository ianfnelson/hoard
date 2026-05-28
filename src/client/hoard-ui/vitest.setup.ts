process.env.TZ = 'UTC'

// jsdom lacks ResizeObserver, which some Vuetify components (e.g. VProgressCircular) require.
if (!globalThis.ResizeObserver) {
  globalThis.ResizeObserver = class {
    observe() {}
    unobserve() {}
    disconnect() {}
  }
}
