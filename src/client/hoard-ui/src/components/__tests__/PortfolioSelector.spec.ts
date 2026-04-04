import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import PortfolioSelector from '../navigation/PortfolioSelector.vue'
import { useNavigationStore } from '@/stores/navigationStore'

const vuetify = createVuetify()

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      {
        path: '/portfolios/:id',
        name: 'portfolio-overview',
        component: { template: '<div />' },
      },
      { path: '/', name: 'home', component: { template: '<div />' } },
    ],
  })
}

let pinia: Pinia

async function mountSelector(portfolios: Array<{ id: number; name: string }>, routeId?: string) {
  pinia = createPinia()
  setActivePinia(pinia)

  const router = createTestRouter()
  await router.push(routeId ? `/portfolios/${routeId}` : '/')
  await router.isReady()

  const store = useNavigationStore()
  store.portfolios = portfolios as never

  return mount(PortfolioSelector, {
    global: {
      plugins: [vuetify, pinia, router],
    },
  })
}

describe('PortfolioSelector', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should show "Portfolios" when no portfolio is selected', async () => {
    const wrapper = await mountSelector([{ id: 1, name: 'ISA' }])
    expect(wrapper.text()).toContain('Portfolios')
  })

  it('should show current portfolio name when on a portfolio route', async () => {
    const wrapper = await mountSelector(
      [
        { id: 1, name: 'ISA' },
        { id: 2, name: 'SIPP' },
      ],
      '1'
    )
    expect(wrapper.text()).toContain('ISA')
  })

  it('should fall back to "Portfolios" when portfolio id not found in store', async () => {
    const wrapper = await mountSelector([{ id: 1, name: 'ISA' }], '999')
    expect(wrapper.text()).toContain('Portfolios')
  })
})
