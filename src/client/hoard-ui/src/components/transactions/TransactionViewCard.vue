<script setup lang="ts">
import type { TransactionDetailDto } from '@/api/dtos/Transactions/TransactionDetailDto'
import { formatCurrency, formatDate, formatUnits, getTrendClass } from '@/utils/formatters'

defineProps<{
  transaction: TransactionDetailDto
}>()

defineEmits<{
  edit: []
  delete: []
}>()
</script>

<template>
  <v-card>
    <v-card-title>Transaction Details</v-card-title>
    <v-card-text>
      <!-- Section 1: Core (Date, Account, Type) -->
      <v-row dense>
        <v-col cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Date</div>
          <div class="text-body-1">{{ formatDate(transaction.date) }}</div>
        </v-col>
        <v-col cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Account</div>
          <div class="text-body-1">{{ transaction.accountName }}</div>
        </v-col>
        <v-col cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Transaction Type</div>
          <div class="text-body-1">{{ transaction.transactionTypeName }}</div>
        </v-col>
      </v-row>

      <v-divider class="my-3" />

      <!-- Section 2: Value & Trade (Value, Units, Price) -->
      <v-row dense>
        <v-col cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Value</div>
          <div class="text-body-1" :class="getTrendClass(transaction.value)">
            {{ formatCurrency(transaction.value) }}
          </div>
        </v-col>
        <v-col v-if="transaction.units !== undefined" cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Units</div>
          <div class="text-body-1" :class="getTrendClass(transaction.units)">
            {{ formatUnits(transaction.units) }}
          </div>
        </v-col>
        <v-col v-if="transaction.price !== undefined" cols="12" sm="4">
          <div class="text-caption text-medium-emphasis">Price</div>
          <div class="text-body-1">{{ transaction.price }}</div>
        </v-col>
      </v-row>

      <!-- Section 3: Costs (conditional) -->
      <template
        v-if="
          transaction.dealingCharge ||
          transaction.stampDuty ||
          transaction.ptmLevy ||
          transaction.fxCharge
        "
      >
        <v-divider class="my-3" />
        <v-row dense>
          <v-col v-if="transaction.dealingCharge" cols="12" sm="6" md="3">
            <div class="text-caption text-medium-emphasis">Dealing Charge</div>
            <div class="text-body-1">{{ formatCurrency(transaction.dealingCharge) }}</div>
          </v-col>
          <v-col v-if="transaction.stampDuty" cols="12" sm="6" md="3">
            <div class="text-caption text-medium-emphasis">Stamp Duty</div>
            <div class="text-body-1">{{ formatCurrency(transaction.stampDuty) }}</div>
          </v-col>
          <v-col v-if="transaction.ptmLevy" cols="12" sm="6" md="3">
            <div class="text-caption text-medium-emphasis">PTM Levy</div>
            <div class="text-body-1">{{ formatCurrency(transaction.ptmLevy) }}</div>
          </v-col>
          <v-col v-if="transaction.fxCharge" cols="12" sm="6" md="3">
            <div class="text-caption text-medium-emphasis">FX Charge</div>
            <div class="text-body-1">{{ formatCurrency(transaction.fxCharge) }}</div>
          </v-col>
        </v-row>
      </template>

      <!-- Section 4: Instrument (conditional) -->
      <template v-if="transaction.instrumentName">
        <v-divider class="my-3" />
        <v-row dense>
          <v-col>
            <div class="text-caption text-medium-emphasis">Instrument</div>
            <div class="text-body-1">
              <router-link
                v-if="transaction.instrumentId"
                :to="{ name: 'instrument-detail', params: { id: transaction.instrumentId } }"
                class="text-decoration-none"
              >
                {{ transaction.instrumentName }}
              </router-link>
              <span v-else>{{ transaction.instrumentName }}</span>
              <span v-if="transaction.instrumentTicker" class="text-medium-emphasis">
                ({{ transaction.instrumentTicker }})
              </span>
            </div>
          </v-col>
        </v-row>
      </template>

      <!-- Section 5: Notes (conditional) -->
      <template v-if="transaction.notes">
        <v-divider class="my-3" />
        <v-row dense>
          <v-col>
            <div class="text-caption text-medium-emphasis">Notes</div>
            <div class="text-body-1">{{ transaction.notes }}</div>
          </v-col>
        </v-row>
      </template>
    </v-card-text>
    <v-card-actions>
      <v-btn color="primary" variant="flat" prepend-icon="mdi-pencil" @click="$emit('edit')">
        Edit
      </v-btn>
      <v-btn color="error" variant="flat" prepend-icon="mdi-delete" @click="$emit('delete')">
        Delete
      </v-btn>
    </v-card-actions>
  </v-card>
</template>
