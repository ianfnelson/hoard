import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import { configs as tseslintConfigs } from 'typescript-eslint'
import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'
import globals from 'globals'

export default [
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,mts,tsx,vue}'],
  },

  {
    name: 'app/files-to-ignore',
    ignores: ['**/dist/**', '**/dist-ssr/**', '**/coverage/**'],
  },

  {
    name: 'app/globals',
    languageOptions: {
      globals: {
        ...globals.browser,
      },
    },
  },

  js.configs.recommended,
  ...tseslintConfigs.recommended,
  ...pluginVue.configs['flat/recommended'],

  {
    files: ['**/*.vue'],
    languageOptions: {
      parserOptions: {
        parser: '@typescript-eslint/parser',
      },
    },
  },

  {
    name: 'app/rules',
    rules: {
      // Vuetify uses v-slot:item.columnName syntax for data tables
      'vue/valid-v-slot': ['error', { allowModifiers: true }],
    },
  },

  skipFormatting,
]
