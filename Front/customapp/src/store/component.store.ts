import { defineStore } from "pinia";
import { ref, type Ref } from "vue";

export const useComponentStore = (options: any) =>
  defineStore(options.name, () => {
    const modelValue = ref("");
    const isDisabled = ref(options.disabled);
    const isHidden = ref(options.hidden);
    const items = ref([]);

    const events: Ref<any> = ref(options.events);

    const deactivateField = () => {
      isDisabled.value = true;
    };
    const activateField = () => {
      isDisabled.value = false;
    };
    const hideField = () => {
      isHidden.value = true;
    };
    const showField = () => {
      isHidden.value = false;
    };
    const updateItems = (value: any) => {
      items.value = value;
    };

    return {
      modelValue,
      isDisabled,
      isHidden,
      items,
      events,
      deactivateField,
      activateField,
      hideField,
      showField,
      updateItems,
    };
  });

export const getComponentStore = (name: string) =>
  defineStore<string, any>(name, () => {});
