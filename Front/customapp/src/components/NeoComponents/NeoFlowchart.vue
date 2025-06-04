<template>
  <div class="neoTreeSelect" v-show="!isHidden" :id="myCurrentComponent">
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        {{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}
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
        ></i
      ></label>
    </div>
    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <!-- append-to="self" -->
      <TreeSelect
        v-if="!forService"
        v-model="itemValue"
        :disabled="isDisabled"
        :options="items ?? options.elements"
        class="w-full neoTreeSelectDropdown flex align-items-center"
        :pt="{
          root: { class: 'w-full ' },
          labelContainer: {
            class: '',
          },
        }"
        :empty-message="
          loading ? 'Recherche en cours ...' : 'Aucune donnée trouvée'
        "
        selectionMode="single"
        @update:modelValue="search = ''"
        @click.stop
        @focus="$emit('focus', $event)"
        @blur="$emit('blur', $event)"
        @mouseenter="$emit('mouseenter', $event)"
        @mouseleave="$emit('mouseleave', $event)"
      >
        <template #value="{ value }">
          <i class="pi pi-share-alt pl-2"></i>
          {{ value[0]?.label }}
        </template>

        <template #header="{ value, options }">
          <IconField iconPosition="left" class="m-3" v-if="!loading">
            <InputIcon class="pi pi-search"> </InputIcon>
            <InputText v-model="search" placeholder="Recherche" />
          </IconField>
        </template>
      </TreeSelect>
      <TreeSelect
        v-else
        v-model="itemValue"
        :disabled="isDisabled"
        :options="serviceItems"
        class="w-full neoTreeSelectDropdown flex align-items-center"
        :pt="{
          root: { class: 'w-full ' },
          labelContainer: {
            class: 'labelcontainerMultiSelect ',
          },
        }"
        :empty-message="
          loading ? 'Recherche en cours ...' : 'Aucune donnée trouvée'
        "
        selectionMode="single"
        @click.stop
        @focus="$emit('focus', $event)"
        @blur="$emit('blur', $event)"
        @mouseenter="$emit('mouseenter', $event)"
        @mouseleave="$emit('mouseleave', $event)"
      >
        <template #value="{ value }">
          <i class="pi pi-share-alt p-2"></i>
          {{ value[0]?.label }}
        </template>
        <template #header="{ value, options }">
          <IconField iconPosition="left" class="m-3" v-if="!loading">
            <InputIcon class="pi pi-search"> </InputIcon>
            <InputText v-model="search" placeholder="Recherche" />
          </IconField>
        </template>
      </TreeSelect>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, nextTick, onMounted, watch } from "vue";
import { logger } from "@/api/api";
import { fetchFlowChart, fetchFlowChartUsers } from "@/api/api";
import { ref } from "vue";

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
        name: "",
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        selectedSource: "",
        selectedService: "",
        elements: [],
        rules: [],
        events: [],
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    forService: {
      type: Boolean,
      default: false,
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: [
    "update:modelValue",
    "update:options",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
  ],
  setup(props, { emit }) {
    const itemValue = computed({
      get() {
        return props.modelValue
          ? { [String(props.modelValue)]: true }
          : [{ label: "", key: "", data: "" }];
      },
      set(newValue): void {
        const key = Object.keys(newValue)[0];
        emit("update:modelValue", key);
      },
    });

    const isDisabled = computed({
      get(): boolean {
        return props.options.disabled;
      },
      set(value: boolean) {
        props.options.disabled = value;
        emit("update:options", props.options);
      },
    });
    const isHidden = computed({
      get(): boolean {
        return props.options.hidden;
      },
      set(value: boolean) {
        props.options.hidden = value;
        emit("update:options", props.options);
      },
    });
    const selectionItems = computed(() => {
      return props.items;
    });
    const serviceItems = computed(() => {
      return flowItems.value;
    });

    const items = computed(() => {
      return flowItems.value;
    });
    var flowItems = ref<any[]>([]);

    const generateRandomString = (length: any) => {
      const characters = "0123456789";
      let randomString = "";

      for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * characters.length);
        randomString += characters[randomIndex];
      }

      return randomString;
    };
    const myCurrentComponent = ref(generateRandomString(10));
    function disableField() {
      isDisabled.value = true;
    }
    function enableField() {
      isDisabled.value = false;
    }
    function hideField() {
      isHidden.value = true;
    }
    function showField() {
      isHidden.value = false;
    }
    function updateField(value: any) {
      // const element = ref({code : value , name : value});
      // props.options.elements.push(element.value);
      props.options.elements = [];
      value.forEach((val: any) => {
        if (
          !props.options.elements.find(
            (element: any) => element.code == val.code
          )
        ) {
          props.options.elements.push(val);
        }
      });

      emit("update:options", props.options);
      itemValue.value = value[0];
      // emit("update:modelValue", itemValue.value);
    }
    const createTree = (data: any) => {
      // Find top-level parents

      const topLevelParents = data.filter(
        (item: any) => !item.parentIdentifier
      );

      // Recursively create tree for each top-level parent
      const createSubTree = (parentIdentifier: any) => {
        const children = data
          .filter((item: any) => item.parentIdentifier === parentIdentifier)
          .map((item: any) => ({
            key: item.identifier,
            label: item.name,
            data: item.identifier,
            children: createSubTree(item.identifier),
          }));

        return children;
      };

      // Create tree for each top-level parent
      const tree = topLevelParents.map((parent: any) => ({
        key: parent.identifier,
        label: parent.name,
        data: parent.identifier,
        children: createSubTree(parent.identifier),
      }));

      return tree;
    };
    const buildTree = (data: any) => {
      const tree: any = [];

      data.forEach((item: any, index: any) => {
        tree.push({
          key: item.userName,
          label: item.userName,
          data: item.userIdentifier,
        });
      });

      return tree;
    };

    const loading = ref(true);
    const search = ref("");
    const allItems = ref<any[]>([]);
    const normalizeText = (text: string) => {
      return text
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase();
    };
    watch(
      () => search.value,
      async (newValue, oldValue) => {
        if (newValue && newValue !== "") {
          const normalizedSearchTerm = normalizeText(newValue);
          flowItems.value = allItems.value
            .filter((item) => {
              const normalizedName = normalizeText(item.name);
              return normalizedName.includes(normalizedSearchTerm);
            })
            .map((item) => ({
              key: item.identifier,
              label: item.name,
              data: item.identifier,
            }));
        } else {
          flowItems.value = createTree(allItems.value);
        }
      }
    );
    // Added `onMounted` event hook
    onMounted(async () => {
      loading.value = true;
      if (props.options.selectedSource == "service" || props.forService) {
        try {
          const response = await fetchFlowChart();
          allItems.value = response;
          flowItems.value = createTree(response);
          if (props.modelValue && props.modelValue !== "") {
            var found = allItems.value.find(
              (d) => d.identifier == props.modelValue
            );
            if (found) {
              emit("update:modelValue", {
                key: found.identifier,
                label: found.name,
                data: found.identifier,
              });
            }
          }
        } catch (error) {
          console.error("Error in createTree function:", error);
          logger.error(error);
        }
      }

      if (props.options.selectedSource == "user") {
        try {
          const response = await fetchFlowChartUsers({
            serviceId: props.options.selectedService,
          });
          allItems.value = response;
          flowItems.value = buildTree(response);

          if (props.modelValue && props.modelValue !== "") {
            var found = allItems.value.find(
              (d) => d.identifier === props.modelValue
            );
            if (found)
              emit("update:modelValue", {
                key: found.identifier,
                label: found.name,
                data: found.identifier,
              });
          }
        } catch (error) {
          console.error("Error in createTree function:", error);
          logger.error(error);
        }
      }
      loading.value = false;
    });
    const isRequired = computed({
      get(): boolean {
        return props.options.required;
      },
      set(value: boolean) {
        props.options.required = value;
        emit("update:options", props.options);
      },
    });
    function manageProperties(opt: any) {
      if (opt) {
        isDisabled.value = opt.disabled;
        isHidden.value = opt.hidden;
        isRequired.value = opt.required;
      }
    }

    return {
      loading,
      search,
      isDisabled,
      isHidden,
      itemValue,
      selectionItems,
      items,
      flowItems,
      serviceItems,
      myCurrentComponent,
      disableField,
      enableField,
      hideField,
      showField,
      updateField,
      fetchFlowChart,
      fetchFlowChartUsers,
      createTree,
      buildTree,
      generateRandomString,
      manageProperties,
    };
  },
};
</script>
