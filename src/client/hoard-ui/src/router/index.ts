import { createRouter, createWebHistory } from 'vue-router'

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
          redirect: () => {
            // TODO: Redirect to first portfolio from store
            return '/portfolios/1'
          },
        },
        {
          path: 'portfolios/:id',
          component: () => import('@/layouts/LayoutPortfolio.vue'),
          props: true,
          children: [
            {
              path: '',
              redirect: 'overview',
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
              path: 'statements',
              name: 'portfolio-statements',
              component: () => import('@/pages/PortfolioStatementsPage.vue'),
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
          path: 'transactions',
          name: 'transactions',
          component: () => import('@/pages/TransactionsPage.vue'),
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
          path: 'system',
          name: 'system',
          component: () => import('@/pages/SystemPage.vue'),
        },
      ],
    },
  ],
})

export default router
