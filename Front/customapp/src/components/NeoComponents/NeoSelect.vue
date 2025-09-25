<template>
  <div class="neoselect" v-show="!isHidden">
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        <span>{{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}</span>
        <span
          v-show="options.required"
          style="color: red; margin-left: 5px; margin-right: 5px"
        >
          *
        </span>
        <i
          v-if="options.tooltip"
          class="pi pi-info-circle"
          v-tooltip.top="options.tooltip"
          style="
            cursor: pointer;
            font-size: 12px;
            margin-left: 5px;
            margin-right: 5px;
          "
        ></i>
      </label>
    </div>

    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <Field
        v-model="itemValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <Select
          v-if="returnObject || options.returnObject"
          v-model="itemValue"
          :disabled="isDisabled"
          :readonly="options.readonly"
          :options="internalItems"
          :optionLabel="options.key ?? 'name'"
          :filter="options.searchable"
          :filterBy="options.searchable ? options.value ?? 'name' : undefined"
          class="w-full neoSelectDropdown"
          :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
          :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
          @click.stop
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
          :loading="isLoading"
          :showClear="options.showClear"
        />
        <Select
          v-else
          v-model="itemValue"
          :disabled="isDisabled"
          :readonly="options.readonly"
          :options="internalItems"
          :optionLabel="options.key ?? 'name'"
          :optionValue="options.value ?? 'code'"
          :filter="options.searchable"
          :filterBy="options.searchable ? options.value ?? 'name' : undefined"
          class="w-full neoSelectDropdown"
          :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
          @click.stop
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
          :loading="isLoading"
          :showClear="options.showClear"
        ></Select>
        <small
          class="p-error"
          id="text-error"
          v-if="errorMessage || errorState.errorMessage"
        >
          {{ errorMessage || errorState.errorMessage || "&nbsp;" }}
        </small>
      </Field>
    </div>
  </div>
</template>

<script lang="ts">
import { eliseEnumeration, fetchDataByTableGuid } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import { ref, computed, onMounted, watch, reactive, nextTick } from "vue";
import { logger } from "@/api/api";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  rows: number;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
  type: string;
  key: string;
  value: string;
  selectedSource: string;
  selectedTable: string;
  selectedColumn: string;
  showClear: boolean;
  search: boolean;
  eliseEnumerate: string;
  selectedVariable: string;
  elements: any[];
}

export default {
  props: {
    items: {
      type: Array,
      // required: true,
    },
    label: {
      type: String,
      required: false,
    },
    label_AR: {
      type: String,
      required: false,
    },
    label_ENG: {
      type: String,
      required: false,
    },
    // type: Object,
    modelValue: {
      // type: Object,
      required: true,
    },
    returnObject: {
      type: Boolean,
      default: false,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        defaultValue: "",
        relatedToElise: false,
        type: "Text",
        name: "CF_Select",
        label: "Select",
        key: "name",
        value: "code",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        selectedSource: "manual",
        selectedTable: "",
        selectedColumn: "",
        eliseEnumerate: "",
        selectedVariable: "",
        showClear: false,
        search: false,
        elements: [{ code: "", name: "" }],
        rules: [],
        events: [],
        tooltip: "",
        keys: [
          { key: "name", required: true },
          { key: "code", required: true },
        ],
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: [
    "update:options",
    "update:modelValue",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
  ],
  setup(props, { emit }) {
    const store = useAppStore();
    const itemValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });
    const isLoading = ref(false);
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const setValue = (value: string | object) => {
      // Temporary variable depends on the props.options.key and props.options.value
      const temporary = ref({} as any);
      let parsedValue = value;

      // Check if the value is a string but represents an object, attempt to parse it
      if (typeof value === "string") {
        try {
          parsedValue = JSON.parse(value);
        } catch (e) {
          parsedValue = value; // If it's not parsable, keep it as a string
        }
      }

      // Handle if returnObject is true
      if (props.options.returnObject) {
        if ((parsedValue as any)[props.options.key]) {
          temporary.value = parsedValue;
        } else {
          // Special case: if only the value property is given, find the full object from elements
          if (
            typeof parsedValue === "string" ||
            typeof parsedValue === "number"
          ) {
            const foundElement = props.options.elements.find(
              (item: any) => item[props.options.value] === parsedValue
            );
            if (foundElement) {
              temporary.value = foundElement;
            } else {
              console.error("Data does not contain the correct key");
            }
          } else {
            console.error("Data does not contain the correct key");
          }
        }
      } else {
        // Handle normal string case
        temporary.value = {
          [props.options.key]: parsedValue,
          [props.options.value]: parsedValue,
        };
      }

      // Check if the element already exists in the list, if not, push the new one
      if (
        !props.options.elements.find(
          (item: any) =>
            item[props.options.value] === temporary.value[props.options.value]
        )
      ) {
        props.options.elements.push(temporary.value);
      }

      // Set the final value for itemValue and emit the event
      itemValue.value = temporary.value[props.options.key];

      emit(
        "update:modelValue",
        props.options.returnObject ? temporary.value : value
      );
    };

    const updateField = (value: string | object) => {
      // Temporary variable depends on the props.options.key and props.options.value
      const temporary = ref({} as any);
      let parsedValue = value;

      // Check if the value is a string but represents an object, attempt to parse it
      if (typeof value === "string") {
        try {
          parsedValue = JSON.parse(value);
        } catch (e) {
          parsedValue = value; // If it's not parsable, keep it as a string
        }
      }
      // Handle if returnObject is true
      if (props.options.returnObject) {
        if ((parsedValue as any)[props.options.key]) {
          temporary.value = {
            [props.options.key]: (parsedValue as any)[props.options.key],
            [props.options.value]: (parsedValue as any)[props.options.value],
          };
        } else {
          // Special case: if only the value property is given, find the full object from elements
          if (
            typeof parsedValue === "string" ||
            typeof parsedValue === "number"
          ) {
            const foundElement = props.options.elements.find(
              (item: any) => item[props.options.value] === parsedValue
            );
            if (foundElement) {
              temporary.value = {
                [props.options.key]: foundElement[props.options.key],
                [props.options.value]: foundElement[props.options.value],
              };
            } else {
              temporary.value = {
                [props.options.key]: parsedValue,
                [props.options.value]: parsedValue,
              };
            }
          } else {
            temporary.value = {
              [props.options.key]: parsedValue,
              [props.options.value]: parsedValue,
            };
          }
        }
      } else {
        // Handle normal string case
        temporary.value = {
          [props.options.key]: parsedValue,
          [props.options.value]: parsedValue,
        };
      }

      // Check if the element already exists in the list, if not, push the new one
      if (
        !props.options.elements.find(
          (item: any) =>
            item[props.options.value] === temporary.value[props.options.value]
        )
      ) {
        props.options.elements.push(temporary.value);
      }

      // Set the final value for itemValue and emit the event
      itemValue.value = temporary.value[props.options.key];
      emit(
        "update:modelValue",
        props.options.returnObject ? temporary.value : value
      );
    };
    // Function to get current value
    const getValue = () => {
      const value = itemValue.value as any;
      if (typeof value != "string" && "id" in value) {
        delete (value as Record<string, any>).id;
      }
      return value;
    };

    // Function to update options
    const updateOptions = async (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
      await nextTick();
    };

    // Function to disable field
    const disableField = () => updateOptions({ disabled: true });
    // Function to enable field
    const enableField = () => updateOptions({ disabled: false });
    // Function to hide field
    const hideField = () => updateOptions({ hidden: true });
    // Function to show field
    const showField = () => updateOptions({ hidden: false });

    const setElements = (newElements: string | any[]) => {
      let parsedElements: any[] = [];
      // Check if newElements is a string, and try to parse it
      if (typeof newElements === "string") {
        try {
          parsedElements = JSON.parse(newElements);
        } catch (error) {
          console.error(
            "Failed to parse elements. Invalid JSON string:",
            error
          );
          logger.error(error);
          return; // Exit if parsing fails
        }
      } else {
        // If already an array, assign it directly
        parsedElements = newElements;
      }
      let formattedElements: any[];

      if (props.options.returnObject) {
        // If returnObject is true, format as an array of objects
        formattedElements = parsedElements.map((item) => {
          // Ensure each item is an object
          if (typeof item === "object" && item !== null) {
            // Create a new object to hold the formatted keys
            const formattedItem: Record<string, any> = {};

            // Map each key to the corresponding property of the item
            for (const key of props.options.keys) {
              // Directly assign the value from the item to the formatted item
              formattedItem[key.key] = item[key.key];
            }
            return formattedItem; // Return the formatted item
          }
          return {}; // Return an empty object if the item is not valid
        });
      } else {
        // If returnObject is false, format as an array of strings
        formattedElements = parsedElements.map((item) => {
          const formattedItem: Record<string, any> = {};

          // Assign each attribute in props.options.key to the same string value
          for (const key of props.options.keys) {
            formattedItem[key.key] = item; // Set each key to the string value
          }
          return formattedItem; // Return the formatted item
        });
      }

      internalItems.value = formattedElements;

      // Update the options with the formatted elements
      updateOptions({ elements: formattedElements });
    };

    const itemsFromStore = computed(() => {
      return store.tableVariables.find((item) => item.key == store.currentTable)
        ?.value;
    });
    watch(
      () => itemsFromStore.value,
      (newValue: any, oldValue) => {
        if (props.options.selectedVariable) {
          if (newValue) {
            const tempArray = ref([] as any);
            const array = ref([] as any);
            newValue.forEach((element: any) => {
              if (element.key == props.options.selectedVariable) {
                element.value.forEach((value: any) => array.value.push(value));
                array.value.forEach((item: any) => {
                  if (
                    tempArray.value.findIndex(
                      (element: any) => element.code == item
                    ) == -1
                  ) {
                    tempArray.value.push({
                      code: item,
                      name: item,
                    });
                  }
                });
              }
            });
            updateItems(array.value);
          }
        }
      },
      { deep: true }
    );

    const internalItems = computed({
      get() {
        const items = props.items ?? props.options.elements;

        // Filtrer les éléments avec des noms vides
        if (Array.isArray(items)) {
          const nameKey = props.options.key ?? "name";
          return items.filter((item) => {
            if (typeof item === "object" && item !== null) {
              const name = item[nameKey];
              return name && name.toString().trim() !== "";
            }
            return true; // Garder les éléments non-objets
          });
        }

        return items;
      },
      set(newValue): void {
        props.options.elements = newValue;
        emit("update:options", props.options);
      },
    });
    const myCurrentTable = store.currentTable as string;
    const vars = store.tableVariables.find(
      (item) => item.key == myCurrentTable
    )?.value;

    const selectionItems = computed(() => {
      if (props.options.selectedSource == "manual") {
        return props.options.elements;
      } else {
        return props.items;
      }
    });

    const updateItems = async (newElements: string | any[]) => {
      if (newElements === undefined || newElements === null) {
        console.warn("New elements are undefined or null, skipping update.");
        return;
      }
      let parsedElements: any[] = [];
      if (typeof newElements === "string") {
        try {
          parsedElements = JSON.parse(newElements);
        } catch (error) {
          console.warn("Failed to parse elements. Invalid JSON string:", error);
          return; // Exit if parsing fails
        }
      } else {
        parsedElements = newElements;
      }

      let formattedElements: any[];

      if (props.options.returnObject) {
        formattedElements = parsedElements
          .map((item) => {
            if (typeof item === "object" && item !== null) {
              const formattedItem: Record<string, any> = {};
              for (const key of props.options.keys) {
                if (key.required && !item[key.key]) {
                  return null; // Skip this item if the required key is not present
                }
                formattedItem[key.key] = item[key.key] || item;
              }

              // Vérifier si le nom est vide
              const nameKey = props.options.key ?? "name";
              if (
                !formattedItem[nameKey] ||
                formattedItem[nameKey].trim() === ""
              ) {
                // Option 1: Filtrer (supprimer) les éléments sans nom
                return null;

                // Option 2: Donner un nom par défaut (décommentez la ligne ci-dessous et commentez la ligne au-dessus)
                // formattedItem[nameKey] = `[Élément vide - ${formattedItem[props.options.value ?? 'code'] || 'Sans code'}]`;
              }

              return formattedItem;
            }
            return null;
          })
          .filter((item) => item !== null); // Filter out null items
      } else {
        formattedElements = parsedElements
          .map((item) => {
            const formattedItem: Record<string, any> = {};
            for (const key of props.options.keys) {
              formattedItem[key.key] = item[key.key] || item;
            }

            // Si le nom est vide, soit filtrer l'élément soit lui donner un nom par défaut
            const nameKey = props.options.key ?? "name";
            if (
              !formattedItem[nameKey] ||
              formattedItem[nameKey].trim() === ""
            ) {
              // Option 1: Filtrer (supprimer) les éléments sans nom
              return null;

              // Option 2: Donner un nom par défaut (décommentez la ligne ci-dessous et commentez la ligne au-dessus)
              // formattedItem[nameKey] = `[Élément vide - ${formattedItem[props.options.value ?? 'code'] || 'Sans code'}]`;
            }

            return formattedItem;
          })
          .filter((item) => item !== null); // Filter out null items (éléments vides)
      }
      console.log("formattedElements", formattedElements);
      // Log the formatted elements for debugging
      internalItems.value = formattedElements;
      // await nextTick();

      // Update the options with the formatted elements
      // updateOptions({ elements: formattedElements });
    };

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );
    watch(
      () => props.options.eliseEnumerate,
      async (newValue, oldValue) => {
        if (newValue && newValue !== oldValue && oldValue !== undefined) {
          fetchEnumerations();
        }
      },
      { immediate: true }
    );
    // Validation rules computation
    const computedRules = computed(() => {
      if (Array.isArray(localOptions.rules)) {
        let expression = localOptions.rules
          .map((item) => item.expression)
          .join("|");

        if (localOptions.required) {
          expression += expression ? "|required" : "required";
        }

        if (localOptions.hidden || localOptions.disabled) {
          expression = "";
        }

        return expression;
      }
      return "";
    });
    // Error state
    const errorState = reactive({
      errorMessage: "",
    });
    // Function to set error
    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };
    // Function to remove error
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    // Watch for field validity and clear error if valid
    watch(
      () => itemValue.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          let isNotEmpty = false;
          if (typeof newValue === "string") {
            isNotEmpty = newValue.trim() !== "";
          } else if (Array.isArray(newValue)) {
            isNotEmpty = newValue.length > 0;
          } else if (typeof newValue === "object" && newValue !== null) {
            isNotEmpty = Object.keys(newValue).length > 0;
          }
          if (isNotEmpty) {
            clearFieldError();
          }
        }
      }
    );
    const fetchEnumerations = async () => {
      let res: any = await eliseEnumeration(props.options.eliseEnumerate);
      const tempArray = ref([] as any);
      res.forEach((element: any) => {
        tempArray.value.push({
          code: element.key,
          name: element.value,
        });
      });
      internalItems.value = tempArray.value;
    };
    onMounted(async () => {
      isLoading.value = true;
      if (props.options.selectedSource == "elise") {
        await fetchEnumerations();
      }

      if (props.options.selectedTable) {
        await handleSelectedTableChange(props.options.selectedTable);
      }
      console.log("Mounted with modelValue:", props.modelValue);
      // Handle initial modelValue when component is mounted
      if (props.modelValue && props.options.returnObject) {
        // Check if modelValue is just a value that needs to be converted to object
        if (
          typeof props.modelValue === "string" ||
          typeof props.modelValue === "number"
        ) {
          // Find the full object from elements or internalItems based on the value
          let foundElement = props.options.elements.find(
            (item: any) => item[props.options.value] === props.modelValue
          );

          // If not found in elements, search in internalItems
          if (!foundElement && internalItems.value) {
            foundElement = internalItems.value.find(
              (item: any) => item[props.options.value] === props.modelValue
            );
          }

          if (foundElement) {
            // Set the itemValue to the key for display
            itemValue.value = foundElement;
            console.log(
              "Setting itemValue to:",
              foundElement[props.options.key]
            );
            // Emit the full object as modelValue
            emit("update:modelValue", foundElement);
          }
        } else if (
          typeof props.modelValue === "object" &&
          props.modelValue !== null &&
          (props.modelValue as any)[props.options.key]
        ) {
          itemValue.value = props.modelValue as any;
        }
      }

      isLoading.value = false;
    });

    async function handleSelectedTableChange(newTable: string) {
      if (newTable) {
        let res: any = await fetchDataByTableGuid(newTable as any);
        const tempArray = [] as any;
        res.forEach((element: any) => {
          var elemJSON = JSON.parse(element.dataJson);
          if (elemJSON.datas[props.options.selectedColumn]) {
            if (elemJSON.datas["code"]) {
              if (
                tempArray.findIndex(
                  (item: any) =>
                    item.name == elemJSON.datas[props.options.selectedColumn]
                ) == -1
              ) {
                tempArray.push({
                  code: elemJSON.datas["code"],
                  name: elemJSON.datas[props.options.selectedColumn],
                });
              } else {
                return;
              }
            } else {
              if (
                tempArray.findIndex(
                  (item: any) =>
                    item.code == elemJSON.datas[props.options.selectedColumn]
                ) == -1
              ) {
                if (!props.options.returnObject) {
                  tempArray.push({
                    code: elemJSON.datas[props.options.selectedColumn],
                    name: elemJSON.datas[props.options.selectedColumn],
                  });
                } else {
                  tempArray.push({
                    code: elemJSON.datas[props.options.selectedColumn],
                    name: elemJSON.datas[props.options.selectedColumn],
                    ...elemJSON.datas,
                  });
                }
              } else {
                return;
              }
            }
          }
        });
        // Only update internalItems if the new data is different
        if (JSON.stringify(internalItems.value) !== JSON.stringify(tempArray)) {
          internalItems.value = tempArray;
        }
      }
    }
    watch(
      () => props.options.selectedTable,
      async (newTable) => {
        await handleSelectedTableChange(newTable);
      }
    );

    watch(
      () => props.options.eliseEnumerate,
      async (newValue, oldValue) => {
        if (newValue && newValue !== oldValue && oldValue !== undefined) {
          fetchEnumerations();
        }
      },
      { immediate: true }
    );

    // Watch for modelValue changes after mount to handle delayed assignment
    watch(
      () => props.modelValue,
      async (newModelValue, oldModelValue) => {
        // Only process if the value actually changed and we have returnObject enabled
        if (
          newModelValue !== oldModelValue &&
          props.options.returnObject &&
          newModelValue
        ) {
          console.log("ModelValue changed:", newModelValue);
          if (
            typeof newModelValue === "string" ||
            typeof newModelValue === "number"
          ) {
            let foundElement = props.options.elements.find(
              (item: any) => item[props.options.value] === newModelValue
            );

            // If not found in elements, search in internalItems
            if (!foundElement && internalItems.value) {
              foundElement = internalItems.value.find(
                (item: any) => item[props.options.value] === newModelValue
              );
            }

            if (foundElement) {
              // Set the itemValue to the full object for display
              itemValue.value = foundElement;
              // Emit the full object as modelValue
              emit("update:modelValue", foundElement);
            }
          } else if (
            typeof newModelValue === "object" &&
            newModelValue !== null &&
            (newModelValue as any)[props.options.key]
          ) {
            itemValue.value = newModelValue as any;
          }
        }
      },
      { deep: true }
    );

    return {
      // dir,
      isDisabled,
      isHidden,
      isLoading,
      itemValue,
      selectionItems,
      internalItems,
      computedRules,
      errorState,
      setValue,
      updateField,
      getValue,
      updateOptions,
      disableField,
      enableField,
      hideField,
      showField,
      updateItems,
      setFieldError,
      clearFieldError,
      setElements,

      // setDirAttributeC,
    };
  },
};
</script>
<style lang="scss">
.input-select .p-select {
  padding: 0.1rem !important;
}
.p-select {
  background-color: unset !important;
}
</style>
