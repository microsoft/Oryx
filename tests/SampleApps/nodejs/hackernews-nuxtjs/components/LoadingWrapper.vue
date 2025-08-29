<template>
  <div v-if="loading" style="text-align: center">
    <Spinner :show="true" />
  </div>
  <div v-else>
    <slot />
  </div>
</template>

<script>
import Spinner from './Spinner'

export default {
  components: {
    Spinner
  },
  props: {
    loading: {
      type: Boolean,
      default: false
    }
  }
}
</script>
