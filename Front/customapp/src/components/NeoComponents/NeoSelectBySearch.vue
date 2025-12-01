<template>
  <div class="neoselect">
    <div class="label" v-if="label">
      <label class="label-container">
        <span>{{ label }}</span>
      </label>
    </div>

    <div
      class="input-container"
      :style="{
        height: '30px',
        'max-height': '30px',
      }"
    >
      <Select
        v-model="itemValue"
        :options="items"
        filter
        optionLabel="name"
        optionValue="code"
        class="w-full neoSelectDropdown"
        @click.stop
        :loading="isLoading"
      >
        <template #option="slotProps">
          <div class="flex align-items-center">
            <div>{{ slotProps.option.name }}</div>
          </div>
        </template>
      </Select>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{
  modelValue: any;
  label: string;
  items: any[];
  loading: boolean;
}>();
const emit = defineEmits(["update:modelValue"]);
const itemValue = computed({
  get() {
    return props.modelValue;
  },
  set(newValue): void {
    emit("update:modelValue", newValue);
  },
});
const isLoading = computed(() => props.loading);
</script>

<style lang="scss">
@import "@/scss/variables";
.neoselect {
  width: 100%;
  .label {
    display: flex;
    flex: 1;
    flex-direction: row;
    height: 20px;
    max-height: 20px;
    .label-container {
      min-width: 150px;
      align-items: center;
      display: flex;
      padding-bottom: 5px;
      padding-top: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
      .label .label-container-modified {
        text-align: right;
      }
    }
  }
  .input-container {
    width: 100%;
    .neoSelectDropdown {
      background-color: #f3f8f9;
      span {
        font-family: Trebuchet MS, sans-serif;
        font-size: 12px;
        display: flex;
        align-items: center;
      }
    }
  }
}
</style>
