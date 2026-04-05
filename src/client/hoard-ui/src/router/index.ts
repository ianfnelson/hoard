import { createRouter, createWebHistory } from 'vue-router'
import { useNavigationStore } from '@/stores/navigationStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('@/layouts/LayoutDefault.vue'),
      children: [
        {
          path: '',
          redirect: '/portfolios',
        },
        {
          path: 'portfolios',
          redirect: async () => {
            const navStore = useNavigationStore()
            await navStore.loadPortfolios()
            return navStore.portfolios.length
              ? `/portfolios/${navStore.portfolios[0].id}`
              : '/instruments'
          },
        },
        {
          path: 'portfolios/:id',
          component: () => import('@/layouts/LayoutPortfolio.vue'),
          props: true,
          children: [
            {
              path: '',
              redirect: (to) => ({ path: `/portfolios/${to.params.id}/overview` }),
            },
            {
              path: 'overview',
              name: 'portfolio-overview',
              component: () => import('@/pages/PortfolioOverviewPage.vue'),
              props: true,
            },
            {
              path: 'exposure',
              name: 'portfolio-exposure',
              component: () => import('@/pages/PortfolioExposurePage.vue'),
              props: true,
            },
            {
              path: 'positions',
              name: 'portfolio-positions',
              component: () => import('@/pages/PortfolioPositionsPage.vue'),
              props: true,
            },
            {
              path: 'snapshots',
              name: 'portfolio-snapshots',
              component: () => import('@/pages/PortfolioSnapshotsPage.vue'),
              props: true,
            },
            {
              path: 'valuations',
              name: 'portfolio-valuations',
              component: () => import('@/pages/PortfolioValuationsPage.vue'),
              props: true,
            },
            {
              path: 'history',
              name: 'portfolio-history',
              component: () => import('@/pages/PortfolioHistoryPage.vue'),
              props: true,
            },
          ],
        },
        {
          path: 'instruments',
          name: 'instruments',
          component: () => import('@/pages/InstrumentsPage.vue'),
        },
        {
          path: 'instruments/:id',
          name: 'instrument-detail',
          component: () => import('@/pages/InstrumentDetailPage.vue'),
          props: true,
        },
        {
          path: 'prices/:id',
          name: 'price-detail',
          component: () => import('@/pages/PriceDetailPage.vue'),
          props: true,
        },
        {
          path: 'transactions',
          name: 'transactions',
          component: () => import('@/pages/TransactionsPage.vue'),
        },
        {
          path: 'transactions/:id',
          name: 'transaction-detail',
          component: () => import('@/pages/TransactionDetailPage.vue'),
          props: true,
        },
        {
          path: 'news',
          name: 'news',
          component: () => import('@/pages/NewsArticlesPage.vue'),
        },
        {
          path: 'news/:id',
          name: 'news-article-detail',
          component: () => import('@/pages/NewsArticleDetailPage.vue'),
          props: true,
        },
        {
          path: 'alerts',
          name: 'alerts',
          component: () => import('@/pages/AlertsPage.vue'),
        },
        {
          path: 'alerts/:id',
          name: 'alert-detail',
          component: () => import('@/pages/AlertDetailPage.vue'),
          props: true,
        },
        {
          path: 'system',
          component: () => import('@/layouts/LayoutSystem.vue'),
          children: [
            {
              path: '',
              redirect: '/system/run-logs',
            },
            {
              path: 'run-logs',
              name: 'system-run-logs',
              component: () => import('@/pages/SystemRunLogsPage.vue'),
            },
            {
              path: 'operations',
              name: 'system-operations',
              component: () => import('@/pages/SystemOperationsPage.vue'),
            },
          ],
        },
      ],
    },
  ],
})

export default router
