<template>
  <div
    class="neoTreeSelect"
    v-show="!isHidden"
    :id="myCurrentComponent"
    :dir="isRTL ? 'rtl' : 'ltr'"
  >
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
      <div
        v-if="
          optionsType == 'Organigramme' ||
          optionsType == ' ' ||
          optionsType == null ||
          optionsType == undefined
        "
      >
        <Field
          v-model="internalValue"
          :name="options.label"
          :rules="computedRules"
          v-slot="{ field, errorMessage }"
        >
          <TreeSelect
            v-if="!forService"
            v-model="internalValue"
            :disabled="isDisabled"
            :options="filterText == '' ? TreeItems : filteredItems"
            :loading="loading"
            class="w-full h-35"
            :empty-message="loading ? ' ' : 'Aucune donnée trouvée'"
            selectionMode="single"
            :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
            @click="attachDropdownToParent()"
            @click.stop
            @focus="$emit('focus', $event)"
            @blur="$emit('blur', $event)"
            @mouseenter="$emit('mouseenter', $event)"
            @mouseleave="$emit('mouseleave', $event)"
          >
            <template #header v-if="!loading">
              <div class="p-2">
                <input
                  type="text"
                  v-model="filterText"
                  placeholder="Recherche ..."
                  class="p-inputtext p-component"
                  :class="{
                    'p-invalid': errorMessage || errorState.errorMessage,
                  }"
                />
              </div>
            </template>
          </TreeSelect>

          <TreeSelect
            v-else
            v-model="internalValue"
            :filter="loading ? false : true"
            filterMode="strict"
            filterPlaceholder="Recherche ..."
            :disabled="isDisabled"
            :options="serviceItems"
            :loading="loading"
            class="w-full h-35"
            :empty-message="loading ? ' ' : 'Aucune donnée trouvée'"
            selectionMode="single"
            :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
            @click.stop
            @focus="$emit('focus', $event)"
            @blur="$emit('blur', $event)"
            @mouseenter="$emit('mouseenter', $event)"
            @mouseleave="$emit('mouseleave', $event)"
            :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
          >
          </TreeSelect>

          <small
            class="p-error"
            id="text-error"
            v-if="errorMessage || errorState.errorMessage"
          >
            {{ errorMessage || errorState.errorMessage || "&nbsp;" }}
          </small>
        </Field>
      </div>
      <div v-else>
        <Field
          v-model="internalValue"
          :name="options.label"
          :rules="computedRules"
          v-slot="{ field, errorMessage }"
        >
          <TreeSelect
            v-model="internalValue"
            filterMode="strict"
            :disabled="isDisabled"
            :options="filterText == '' ? TreeItems : filteredItems"
            :loading="loading"
            class="w-full h-35"
            display="comma"
            :empty-message="loading ? ' ' : 'Aucune donnée trouvée'"
            selectionMode="multiple"
            :maxSelectedLabels="termLimit"
            selectedItemsLabel="Vous avez sélectionné le nombre maximum d'éléments"
            :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
            @click="attachDropdownToParent()"
            @focus="$emit('focus', $event)"
            @blur="$emit('blur', $event)"
            @mouseenter="$emit('mouseenter', $event)"
            @mouseleave="$emit('mouseleave', $event)"
            :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
            ><template #header v-if="!loading">
              <div class="p-2">
                <input
                  type="text"
                  v-model="filterText"
                  placeholder="Recherche ..."
                  class="p-inputtext p-component"
                />
              </div>
            </template>
          </TreeSelect>
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
  </div>
</template>
<script lang="ts">
import {
  fetchFlowChart,
  fetchFlowChartUsers,
  eliseGetFullThesaurus,
  fetchFlowChartWithUsers,
} from "@/api/api";
import { useToast } from "primevue/usetoast";
import {
  computed,
  ref,
  onMounted,
  onBeforeUnmount,
  watch,
  nextTick,
  reactive,
} from "vue";
import { logger } from "@/api/api";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  type: string;
  tooltip: string;
  selectedType: string;
  selectedSource: string;
  selectedService: string;
  thesaurusId: string;
  termLimit: number;
  returnLabel: boolean;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    items: {
      type: Array,
      required: false,
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
    modelValue: {
      required: true,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "TreeView",
        type: "TREEVIEW",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        selectedType: "",
        selectedSource: "",
        selectedService: "",
        thesaurusId: "",
        termLimit: 3,
        returnLabel: false,
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
    "update:modelValue",
    "focus",
    "blur",
    "mouseleave",
    "mouseenter",
    "update:options",
  ],
  setup(props, { emit }) {
    const toast = useToast();
    const TreeItems = ref<any[]>([]); // Reactive items for the TreeSelect
    const loading = ref(false);
    // Manage item value
    const internalValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });
    const termLimit = props.options.termLimit;
    const filterText = ref("");
    const filterTreeItems = (items: any[], filterText: string) => {
      const result: any[] = [];
      const searchTree = (nodes: any[]) => {
        for (const node of nodes) {
          if (node.label.toLowerCase().includes(filterText.toLowerCase())) {
            result.push({ ...node, children: [] }); // Add node without children
          }
          if (node.children && node.children.length > 0) {
            searchTree(node.children);
          }
        }
      };
      searchTree(items);
      return result;
    };
    const filteredItems = computed(() => {
      return filterTreeItems(TreeItems.value, filterText.value);
    });
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });
    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    // Function to get current value
    const getValue = () => {
      if (optionsType.value == "Organigramme") {
        console.log("internalValue.value", internalValue.value);
        return Object.keys(internalValue.value as any)[0] ?? ""; // Return the first key
      } else {
        return Object.keys(internalValue.value as any) ?? [];
      }
    };
    // Add a temporary variable to store the initial model value
    const initialModelValue = ref(props.modelValue);
    // Function to update field
    const setValue = (value: any) => {
      if (optionsType.value == "Organigramme") {
        if (typeof value === "string") {
          internalValue.value = { [value]: true };
        } else {
          internalValue.value = value;
        }
        console.log("internalValue.value", internalValue.value);
      } else {
        if (Array.isArray(value) && typeof value[0] === "string") {
          internalValue.value = value.map((item: any) => ({ [item]: true }));
        } else {
          internalValue.value = value;
        }
      }
    };
    const updateField = (value: any) => {
      if (optionsType.value == "Organigramme") {
        internalValue.value = { [value]: true };
      } else {
        internalValue.value = value.map((item: any) => ({
          [item]: true,
        }));
      }
    };
    // Function to update options
    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };
    // Function to disable field
    const disableField = () => updateOptions({ disabled: true });
    // Function to enable field
    const enableField = () => updateOptions({ disabled: false });
    // Function to hide field
    const hideField = () => updateOptions({ hidden: true });
    // Function to show field
    const showField = () => updateOptions({ hidden: false });
    const optionsType = computed(() => {
      return props.options.selectedType as any;
    }) as any;
    const selectionItems = computed(() => {
      return props.items;
    });
    const serviceItems = computed(() => {
      return TreeItems.value;
    });
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
    // Handle the dropdown positioning manually
    const handleSelection = () => {
      const dropdown = document.querySelector(
        ".p-treeselect-overlay"
      ) as HTMLElement;
      if (
        dropdown &&
        dropdown.classList.contains("p-connected-overlay-visible")
      ) {
        dropdown.classList.remove("p-connected-overlay-visible");
      }
    };
    const showIcon = () => {
      try {
        const container = document.getElementById(myCurrentComponent.value);
        nextTick(() => {
          const TreeSelectLabel = container?.querySelector(
            ".p-treeselect-label-container"
          ) as HTMLInputElement;
          if (TreeSelectLabel) {
            // Create new structure
            const grid = document.createElement("div");
            grid.classList.add("grid");
            grid.classList.add("gap-2");
            grid.style.width = "100%";
            // Apply flexbox to ensure items stay on the same line
            grid.style.display = "flex";
            grid.style.alignItems = "flex-start"; // Align items to the top (start) of the flex container
            grid.style.paddingTop = "0.25rem";
            const col1 = document.createElement("div");
            col1.style.width = "15px";
            col1.style.height = "30px";
            col1.classList.add("col-2");

            const col2 = document.createElement("div");
            col2.style.height = "30px";
            col2.classList.add("col-9");
            // Create and add icon to col-1
            const icon = document.createElement("i");
            if (
              props.options.selectedSource == "service" ||
              props.options.selectedSource == "user/service" ||
              props.forService
            ) {
              props.options.selectedSource == "user/service"
                ? icon.classList.add("pi", "pi-users")
                : icon.classList.add("pi", "pi-share-alt");
            } else {
              icon.classList.add("pi", "pi-user");
            }
            // Adjust padding based on RTL
            icon.style.paddingLeft = props.isRTL ? "" : "10px";
            icon.style.paddingRight = props.isRTL ? "10px" : "0";
            icon.style.paddingTop = "2px";
            col1.appendChild(icon);
            // Move old child to col-2
            if (TreeSelectLabel.firstChild) {
              col2.appendChild(TreeSelectLabel.firstChild);
            }
            // Add col-1 and col-2 to grid
            grid.appendChild(col1);
            grid.appendChild(col2);
            // Replace TreeSelectLabel with new structure
            TreeSelectLabel.parentNode?.replaceChild(grid, TreeSelectLabel);
          }
        });
      } catch (error) {
        console.error(error);
        logger.error(error);
      }
    };
    const hideIcon = () => {
      try {
        const container = document.getElementById(myCurrentComponent.value);
        nextTick(() => {
          const TreeSelectLabel = container?.querySelector(
            ".p-treeselect-label-container"
          ) as HTMLDivElement;
          const grid = TreeSelectLabel?.parentNode as HTMLDivElement;
          if (TreeSelectLabel && grid && grid.classList.contains("grid")) {
            // Restore original TreeSelectLabel
            const originalLabel = document.createElement("div");
            originalLabel.classList.add("p-treeselect-label-container");
            // Move the existing children back to TreeSelectLabel
            while (grid.firstChild) {
              originalLabel.appendChild(grid.firstChild);
            }
            // Replace the grid with the original TreeSelectLabel
            grid.parentNode?.replaceChild(originalLabel, grid);
          }
        });
      } catch (error) {
        console.error(error);
        logger.error(error);
      }
    };
    const attachDropdownToParent = async () => {
      await nextTick();
      // Find the dropdown and parent within the specific instance
      const dropdown = document.querySelector(
        ".p-treeselect-overlay"
      ) as HTMLElement;

      if (dropdown && parent) {
        dropdown.style.direction = props.isRTL ? "rtl" : "ltr";
      }
    };
    const fetchServicesAndUsers = async () => {
      try {
        loading.value = true;
        TreeItems.value = [];
        // Fetch services and users combined using the new API
        const servicesAndUsersList = await fetchFlowChartWithUsers(); // This is your updated API function
        console.log("mergedArray", servicesAndUsersList);
        console.log("typeof mergedArray", typeof servicesAndUsersList);
        // Use your function to create the tree items based on the merged data
        TreeItems.value = createServiceUsersTree(servicesAndUsersList);
      } catch (error) {
        console.error("Error fetching services and users:", error);
      } finally {
        loading.value = false;
      }
    };
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
            data: item.identifier,
            label: item.name,
            key: props.forService
              ? item.identifier
              : props.options.returnLabel
              ? item.name
              : item.identifier,
            children: createSubTree(item.identifier),
          }));

        return children;
      };
      // Create tree for each top-level parent
      const tree = topLevelParents.map((parent: any) => ({
        data: parent.identifier,
        label: parent.name,
        key: props.forService
          ? parent.identifier
          : props.options.returnLabel
          ? parent.name
          : parent.identifier,
        children: createSubTree(parent.identifier),
      }));

      return tree;
    };
    const createServiceUsersTree = (data: any) => {
      // Find top-level parents
      const topLevelParents = data.filter(
        (item: any) => !item.parentIdentifier && !item.serviceIdentifier
      );
      // Recursively create tree for each top-level parent
      const createSubTree = (parentIdentifier: any) => {
        const children = data
          .filter(
            (item: any) =>
              item?.parentIdentifier === parentIdentifier ||
              item?.serviceIdentifier === parentIdentifier
          )
          .map((item: any) => ({
            data: item?.identifier || item?.userIdentifier,
            label: item?.name || item?.userName,
            key: props.forService
              ? item?.identifier || item?.userIdentifier
              : props.options.returnLabel
              ? item?.name || item?.userName
              : item?.identifier || item?.userIdentifier,
            icon: !item?.serviceIdentifier ? "pi pi-share-alt" : "pi pi-user",
            children: createSubTree(item?.identifier || item?.userIdentifier),
          }));

        return children;
      };

      // Create tree for each top-level parent
      const tree = topLevelParents.map((parent: any) => ({
        data: parent?.identifier || parent?.userIdentifier,
        label: parent?.name || parent?.userName,
        key: props.forService
          ? parent?.identifier || parent?.userIdentifier
          : props.options.returnLabel
          ? parent?.name || parent?.userName
          : parent?.identifier || parent?.userIdentifier,
        icon: !parent?.serviceIdentifier ? "pi pi-share-alt" : "pi pi-user",
        children: createSubTree(parent?.identifier || parent?.userIdentifier),
      }));

      return tree;
    };
    const buildTree = (data: any) => {
      const tree: any = [];
      data.forEach((item: any, index: any) => {
        tree.push({
          data: item.userName,
          label: item.userName,
          key: props.forService
            ? item.userIdentifier
            : props.options.returnLabel
            ? item.name
            : item.userIdentifier,
        });
      });

      return tree;
    };
    const buildTreeThesaurus = (array: any, parentKey = "") => {
      return array.map((item: any, index: any) => {
        const key = parentKey ? `${parentKey}-${index}` : `${index}`;
        const newItem = {
          key: item.EntryId,
          label: item.Label,
          data: item.EntryId,
          children:
            item.Children.length > 0
              ? buildTreeThesaurus(item.Children, key)
              : [],
        };
        return newItem;
      });
    };
    watch(
      () => props.options.selectedSource,
      async (newValue, oldValue) => {
        if (newValue !== oldValue && optionsType.value == "Organigramme") {
          const container = document.getElementById(myCurrentComponent.value);

          if (newValue == "service") {
            nextTick(async () => {
              const iconElement = container?.querySelector(
                "i"
              ) as HTMLInputElement;

              if (iconElement) {
                iconElement.className = "pi pi-share-alt";
              }
              try {
                loading.value = true;
                TreeItems.value = [];

                const response = await fetchFlowChart();
                TreeItems.value = createTree(response);
                loading.value = false;
              } catch (error) {
                console.error("Error in createTree function:", error);
                logger.error(error);
              }
            });
          } else if (newValue == "user") {
            nextTick(() => {
              const iconElement = container?.querySelector(
                "i"
              ) as HTMLInputElement;

              if (iconElement) {
                iconElement.className = "pi pi-user";
              }
            });
            try {
              loading.value = true;
              TreeItems.value = [];
              if (props.options.selectedService != "") {
                const response = await fetchFlowChartUsers({
                  serviceId: props.options.selectedService,
                });
                TreeItems.value = buildTree(response);
              }
              loading.value = false;
            } catch (error) {
              console.error("Error in createTree function:", error);
              logger.error(error);
            }
          } else {
            nextTick(async () => {
              const iconElement = container?.querySelector(
                "i"
              ) as HTMLInputElement;

              if (iconElement) {
                iconElement.className = "pi pi-users";
              }
              try {
                fetchServicesAndUsers();
              } catch (error) {
                console.error("Error in createTree function:", error);
                logger.error(error);
              }
            });
          }
        }
      },
      { deep: true }
    );
    watch(
      () => props.options.thesaurusId,
      async (newValue, oldValue) => {
        if (newValue !== oldValue && optionsType.value == "Thesaurus") {
          loading.value = true;
          const response: any = await eliseGetFullThesaurus(
            props.options.thesaurusId,
            ""
          );
          TreeItems.value = buildTreeThesaurus(response.Children);
          loading.value = false;
        }
      },
      { deep: true }
    );
    watch(
      () => props.options.selectedType,
      async (newValue, oldValue) => {
        if (newValue == "Organigramme") {
          showIcon();
        } else {
          hideIcon();
        }
        if (newValue != oldValue && newValue != "") {
          internalValue.value = {} as any;
          TreeItems.value = [];
        }
      },
      { deep: true }
    );
    watch(
      () => props.options.selectedService,
      async (newValue, oldValue) => {
        if (newValue !== oldValue && optionsType.value == "Organigramme") {
          try {
            loading.value = true;
            TreeItems.value = [];
            if (props.options.selectedService != "") {
              const response = await fetchFlowChartUsers({
                serviceId: newValue,
              });
              TreeItems.value = buildTree(response);
            }
            loading.value = false;
          } catch (error) {
            console.error("Error in createTree function:", error);
            logger.error(error);
          }
        }
      },
      { deep: true }
    );
    watch(
      () =>
        internalValue.value
          ? Object.keys(internalValue.value as any).length
          : 0,
      (newValue) => {
        if (
          newValue > props.options.termLimit &&
          props.options.termLimit != 0 &&
          optionsType.value != "Organigramme"
        ) {
          internalValue.value = Object.fromEntries(
            Object.entries(internalValue.value as any).slice(
              0,
              props.options.termLimit
            )
          );
          setFieldError("Vous avez atteint le nombre maximum d'éléments");
          toast.add({
            severity: "error",
            summary: "Erreur",
            detail: "Vous avez atteint le nombre maximum d'éléments",
            life: 5000,
          });
          setTimeout(() => {
            clearFieldError();
          }, 5000);
        }
      },
      { deep: true }
    );
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );
    watch(
      () => props.options.returnLabel,
      async (newValue, oldValue) => {
        if (newValue != oldValue) {
          internalValue.value = {} as any;
          TreeItems.value = [];
          if (props.options.selectedSource == "service" || props.forService) {
            try {
              loading.value = true;
              const response = await fetchFlowChart();
              TreeItems.value = createTree(response);
              loading.value = false;
            } catch (error) {
              console.error("Error in createTree function:", error);
              logger.error(error);
            }
          } else if (props.options.selectedSource == "user") {
            try {
              loading.value = true;
              if (props.options.selectedService != "") {
                const response = await fetchFlowChartUsers({
                  serviceId: props.options.selectedService,
                });
                TreeItems.value = buildTree(response);
              }
              loading.value = false;
            } catch (error) {
              console.error("Error in createTree function:", error);
              logger.error(error);
            }
          } else {
            try {
              fetchServicesAndUsers();
            } catch (error) {
              console.error("Error in createTree function:", error);
              logger.error(error);
            }
          }
        }
      },
      { deep: true }
    );
    // Watch for changes in TreeItems and set the value when it's fully filled
    watch(
      () => TreeItems.value,
      (newValue) => {
        if (
          newValue.length > 0 &&
          initialModelValue.value != "" &&
          initialModelValue.value != null &&
          initialModelValue.value != undefined
        ) {
          setValue(initialModelValue.value);
        }
      },
      { deep: true }
    );
    watch(
      () => props.modelValue,
      (newValue: any) => {
        if (
          newValue.length > 0 &&
          props.modelValue != "" &&
          props.modelValue != null &&
          props.modelValue != undefined
        ) {
          setValue(props.modelValue);
        }
      },
      { deep: true }
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

    // Clean up before unmounting
    onBeforeUnmount(() => {
      window.removeEventListener("resize", () => {});
    });
    // On mounted lifecycle hook for managing dropdown
    onMounted(async () => {
      if (optionsType.value == "Organigramme") {
        showIcon();
        if (props.options.selectedSource == "service" || props.forService) {
          try {
            loading.value = true;
            // Empty the model value until the tree is fully loaded
            initialModelValue.value = props.modelValue;
            internalValue.value = {};

            const response = await fetchFlowChart();
            TreeItems.value = createTree(response);
            loading.value = false;
          } catch (error) {
            console.error("Error in createTree function:", error);
            logger.error(error);
          }
        } else if (props.options.selectedSource == "user") {
          try {
            loading.value = true;
            // Empty the model value until the tree is fully loaded
            initialModelValue.value = props.modelValue;
            internalValue.value = {};

            if (props.options.selectedService != "") {
              const response = await fetchFlowChartUsers({
                serviceId: props.options.selectedService,
              });
              TreeItems.value = buildTree(response);
            }
            loading.value = false;
          } catch (error) {
            console.error("Error in createTree function:", error);
            logger.error(error);
          }
        } else {
          try {
            fetchServicesAndUsers();
          } catch (error) {
            console.error("Error in createTree function:", error);
            logger.error(error);
          }
        }
      } else if (
        optionsType.value == "Thesaurus" &&
        props.options.thesaurusId
      ) {
        loading.value = true;
        // Empty the model value until the tree is fully loaded
        initialModelValue.value = props.modelValue;
        internalValue.value = {};

        const response: any = await eliseGetFullThesaurus(
          props.options.thesaurusId,
          ""
        );
        console.log("response", response);
        TreeItems.value = buildTreeThesaurus(response.Children);
        loading.value = false;
      }
    });

    return {
      loading,
      isHidden,
      termLimit,
      TreeItems,
      isDisabled,
      errorState,
      filterText,
      optionsType,
      serviceItems,
      filteredItems,
      computedRules,
      internalValue,
      selectionItems,
      myCurrentComponent,
      getValue,
      setValue,
      hideField,
      showField,
      buildTree,
      createTree,
      updateField,
      enableField,
      disableField,
      setFieldError,
      updateOptions,
      fetchFlowChart,
      clearFieldError,
      handleSelection,
      fetchFlowChartUsers,
      generateRandomString,
      attachDropdownToParent,
    };
  },
};
</script>
<style>
.h-35 {
  height: 35px;
}
</style>
