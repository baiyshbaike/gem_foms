<script lang="ts" setup>
import { useAuth } from '@/composables/use-auth'

const { login, loading } = useAuth()

const username = ref('')
const password = ref('')

async function submit() {
  await login(username.value, password.value)
}
</script>

<template>
  <UiCard class="w-full max-w-sm">
    <UiCardHeader>
      <UiCardTitle class="text-2xl">
        Login
      </UiCardTitle>
      <UiCardDescription>
        Enter your username and password to continue.
      </UiCardDescription>
    </UiCardHeader>
    <UiCardContent>
      <form class="grid gap-4" @submit.prevent="submit">
        <div class="grid gap-2">
          <UiLabel for="username">
            Username
          </UiLabel>
          <UiInput
            id="username"
            v-model="username"
            autocomplete="username"
            placeholder="admin"
            required
          />
        </div>
        <div class="grid gap-2">
          <UiLabel for="password">
            {{ $t('password') }}
          </UiLabel>
          <UiInput
            id="password"
            v-model="password"
            type="password"
            autocomplete="current-password"
            required
            placeholder="********"
          />
        </div>

        <UiButton
          type="submit"
          class="w-full"
          :disabled="loading || !username || !password"
        >
          <UiSpinner v-if="loading" class="mr-2" />
          {{ $t('login') }}
        </UiButton>
      </form>
    </UiCardContent>
  </UiCard>
</template>
