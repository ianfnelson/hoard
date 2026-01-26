<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { usePortfolioStatements } from "@/composables/usePortfolioStatements";
import { formatCurrency, formatPercentage, getTrendClass } from "@/utils/formatters";

const props = defineProps<{
  id: string;
}>();

const { items, isLoading, error, fetchStatements } = usePortfolioStatements();

type ViewTab = 'performance' | 'trades' | 'contributions';
const activeTab = ref<ViewTab>('performance');

const performanceHeaders = [
  { title: "Year", key: "year", sortable: true },
  { title: "Start Value", key: "startValue", sortable: true, align: 'end' },
  { title: "End Value", key: "endValue", sortable: true, align: 'end' },
  { title: "Change", key: "valueChange", sortable: true, align: 'end' },
  { title: "Average", key: "averageValue", sortable: true, align: 'end' },
  { title: "Return", key: "return", sortable: true, align: 'end' },

] as const;

const tradesHeaders = [
  { title: "Year", key: "year", sortable: true },
  { title: "Total Buys", key: "totalBuys", sortable: true, align: 'end' },
  { title: "Total Sells", key: "totalSells", sortable: true, align: 'end' },
  { title: "Trade Count", key: "countTrades", sortable: true, align: 'end' },
  { title: "Churn", key: "churn", sortable: true, align: 'end' },
  { title: "Dealing", key: "totalDealingCharge", sortable: true, align: 'end' },
  { title: "Stamp Duty", key: "totalStampDuty", sortable: true, align: 'end' },
  { title: "PTM Levy", key: "totalPtmLevy", sortable: true, align: 'end' },
  { title: "FX Charge", key: "totalFxCharge", sortable: true, align: 'end' }
] as const;

const movementsHeaders = [
  { title: "Year", key: "year", sortable: true },
  { title: "Interest", key: "totalIncomeInterest", sortable: true, align: 'end' },
  { title: "Loyalty", key: "totalIncomeLoyaltyBonus", sortable: true, align: 'end' },
  { title: "Promotion", key: "totalIncomePromotion", sortable: true, align: 'end' },
  { title: "Dividends", key: "totalIncomeDividends", sortable: true, align: 'end' },
  { title: "Personal", key: "totalDepositPersonal", sortable: true, align: 'end' },
  { title: "Employer", key: "totalDepositEmployer", sortable: true, align: 'end' },
  { title: "Tax Reclaim", key: "totalDepositIncomeTaxReclaim", sortable: true, align: 'end' },
  { title: "Transfers", key: "totalDepositTransferIn", sortable: true, align: 'end' },
  { title: "Withdrawals", key: "totalWithdrawals", sortable: true, align: 'end' },
  { title: "Fees", key: "totalFees", sortable: true, align: 'end' },
] as const;

watch(() => props.id, async (newId) => {
  if (newId) {
    await fetchStatements(Number(newId));
  }
}, { immediate: true });

onMounted(async () => {
  if (props.id) {
    await fetchStatements(Number(props.id));
  }
});
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-alert
          v-if="error"
          type="error"
          variant="tonal"
          class="mb-4"
        >
          {{ error }}
        </v-alert>

        <v-tabs
          v-model="activeTab"
          bg-color="transparent"
          density="compact"
          class="mb-4"
        >
          <v-tab value="performance">Performance</v-tab>
          <v-tab value="trades">Trades</v-tab>
          <v-tab value="movements">Movements</v-tab>
        </v-tabs>

        <v-window v-model="activeTab">
          <v-window-item value="performance">
            <v-data-table
              :headers="performanceHeaders"
              :items="items"
              :loading="isLoading"
              density="compact"
              :items-per-page="15"
              :items-per-page-options="[
                { value: 10, title: '10' },
                { value: 15, title: '15' },
                { value: 25, title: '25' },
                { value: 50, title: '50' },
                { value: -1, title: 'All' }
              ]"
              :sort-by="[{ key: 'year', order: 'desc' }]"
            >
              <template #item.startValue="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.endValue="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.valueChange="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.averageValue="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.return="{ value }">
                <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
              </template>



              <template #no-data>
                No statement data available
              </template>
            </v-data-table>
          </v-window-item>

          <v-window-item value="trades">
            <v-data-table
              :headers="tradesHeaders"
              :items="items"
              :loading="isLoading"
              density="compact"
              :items-per-page="15"
              :items-per-page-options="[
                { value: 10, title: '10' },
                { value: 15, title: '15' },
                { value: 25, title: '25' },
                { value: 50, title: '50' },
                { value: -1, title: 'All' }
              ]"
              :sort-by="[{ key: 'year', order: 'desc' }]"
            >
              <template #item.totalBuys="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalSells="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalDealingCharge="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalStampDuty="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalPtmLevy="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalFxCharge="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.churn="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #no-data>
                No statement data available
              </template>
            </v-data-table>
          </v-window-item>

          <v-window-item value="movements">
            <v-data-table
              :headers="movementsHeaders"
              :items="items"
              :loading="isLoading"
              density="compact"
              :items-per-page="15"
              :items-per-page-options="[
                { value: 10, title: '10' },
                { value: 15, title: '15' },
                { value: 25, title: '25' },
                { value: 50, title: '50' },
                { value: -1, title: 'All' }
              ]"
              :sort-by="[{ key: 'year', order: 'desc' }]"
            >
              <template #item.totalIncomeInterest="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalIncomeLoyaltyBonus="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalIncomePromotion="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalIncomeDividends="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalDepositPersonal="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalDepositEmployer="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalDepositIncomeTaxReclaim="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalDepositTransferIn="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalWithdrawals="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.totalFees="{ value }">
                <span :class="getTrendClass(-value)">{{ formatCurrency(value) }}</span>
              </template>



              <template #no-data>
                No statement data available
              </template>
            </v-data-table>
          </v-window-item>
        </v-window>
      </v-col>
    </v-row>
  </v-container>
</template>
