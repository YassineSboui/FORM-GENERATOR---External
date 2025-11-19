<template>
  <div v-if="!readonly">
    <DataTable
      v-if="config.objectConfig.formConfig.expandable"
      :frozenValue="lockedRows"
      tableClass="editable-cells-table"
      :tableStyle="{ minWidth: '30rem', maxHeight: '70vh' }"
      scrollHeight="60vh"
      v-model:filters="filters"
      class="custom-datatable"
      filterDisplay="menu"
      editMode="row"
      removableSort
      scrollable
      :globalFilterFields="columns"
      :id="myCurrentComponent"
      :loading="loading"
      :value="objects"
      dataKey="id"
      :ref="isTableCreation ? 'dt' : ''"
      v-model:expandedRows="expandedRows"
      @row-edit-cancel="onRowEditCancel"
      @row-edit-save="onRowEditSave"
      :reorderableColumns="
        props.reOrder || config.objectConfig.formConfig.enableReorder
      "
      @rowReorder="onRowReorder"
      v-model:selection="selectedObjects"
    >
      <template #header v-if="!store.local">
        <div class="flex justify-content-end">
          <Button
            v-if="
              config.objectConfig.formConfig.selectable &&
              selectedObjects &&
              selectedObjects.length >= 2
            "
            size="small"
            class="iconStyle"
            style="margin: 0.25%"
            @click="deleteSelectedRows"
          >
            <span class="pi pi-trash"></span>
          </Button>
          <Button
            v-if="
              type === 'DIALOG' &&
              (!config.objectConfig.formConfig.allowedActions ||
                config.objectConfig.formConfig.allowedActions.includes(
                  'create'
                ))
            "
            size="small"
            @click="openNew"
            class="iconStyle"
            style="margin: 0.25%"
          >
            <span class="p-button-text" v-if="addBtn">{{ addBtn }}</span>
            <span class="pi pi-plus" v-else></span>
          </Button>
        </div>
      </template>

      <template #empty>{{ t("NeoTable.empty") }}</template>
      <template #loading>{{ t("NeoTable.loading") }}</template>

      <template #expansion="{ data }">
        <div class="p-grid p-fluid">
          <div class="p-col-12 p-md-6">
            <p v-html="data[showOnDetailColumns.options.name]"></p>
          </div>
        </div>
      </template>
      <Column
        v-if="config.objectConfig.formConfig.selectable"
        :selectionMode="config.objectConfig.formConfig.selectionMode"
        headerStyle="width: 3rem"
      ></Column>
      <!-- <Column expander style="width: 5rem" /> -->
      <Column
        v-if="props.reOrder || config.objectConfig.formConfig.enableReorder"
        rowReorder
        headerStyle="width: 3rem"
        :reorderableColumn="false"
      />
      <Column
        v-for="column in filterColumnsByShow"
        :key="column.column_name"
        :field="column.column_name"
        :filterField="column.column_name"
        :sortable="column.sortable"
        :headerStyle="{
          'min-width': column.taille ? `${column.taille}rem` : '150px',
        }"
        :dir="isRTL ? 'rtl' : 'ltr'"
      >
        <template #header>
          <span
            v-tooltip.top="column.columnConfig.options.tooltip"
            style="font-weight: bold"
          >
            <span :class="{ underline: column.unique }">
              {{
                typeof column.columnConfig.options.label === "string" &&
                column.columnConfig.options.label.includes(".")
                  ? t(column.columnConfig.options.label)
                  : column.columnConfig.options.label
              }}
            </span>
          </span>
          <span
            v-show="column.columnConfig.options.required"
            style="color: red; margin-left: 5px; margin-right: 5px"
          >
            *
          </span>
        </template>
        <template #body="{ data, field }">
          <component
            v-if="
              ['NeoRating', 'NeoCheckbox'].includes(
                column.columnConfig.component
              )
            "
            :options="computedOptions(column.columnConfig.options)"
            :ref="column.columnConfig.options.name"
            :is="column.columnConfig.component"
            :isParentNeoTable="true"
            v-model="data[field as any]"
            :isRTL="isRTL"
          />
          <div v-else-if="column.columnConfig.component === 'NeoSelect'">
            {{
              neoSelectValue(
                data[field as any],
                column.columnConfig.options.returnObject,
                column.columnConfig.options.value,
                column.columnConfig.options.elements,
                column.columnConfig.options.key
              )
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoDatepicker'">
            {{ formatDate(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoTimePicker'">
            {{ formatTime(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoUploadFile'">
            {{
              data[field as any] && data[field as any].length > 0
                ? data[field as any]
                    .map((file: any) => file.fileName)
                    .join(", ")
                : ""
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoCheckboxGroup'">
            {{ data[field as any].toString() }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoFlowchart_V2'">
            {{ data[field as any].name }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoContact'">
            {{
              contactDisplay(
                data[field as any],
                column.columnConfig.options.optionLabel
              )
            }}
          </div>
          <div v-else>{{ data[field as any] }}</div>
        </template>

        <template #editor="{ data, field }">
          <div class="inline-component">
            <component
              :options="NotRequiredOptions(column.columnConfig.options)"
              :ref="column.columnConfig.options.name"
              :is="column.columnConfig.component"
              :isParentNeoTable="true"
              v-model="data[field]"
              :isRTL="isRTL"
              @update:options="(e : any) => {
              column.columnConfig.options = e;
              }"
            />
          </div>
        </template>

        <template #filter="{ filterModel }" v-if="column.filters">
          <InputText
            v-model="filterModel.value"
            type="text"
            class="p-column-filter"
            placeholder="recherche ..."
          />
        </template>
      </Column>

      <Column
        v-if="type !== 'DIALOG'"
        :rowEditor="true"
        style="width: 2%; min-width: 2rem"
        header-class="headerClass"
        :bodyStyle="{
          'text-align': 'center',
        }"
        alignFrozen="right"
        :frozen="EditFrozen"
        :dir="isRTL ? 'rtl' : 'ltr'"
      />

      <Column
        style="width: 10%; max-width: 100px; min-width: 100px"
        :rowEditor="true"
        header-class="headerClass"
        :bodyStyle="{
          'text-align': 'center',
        }"
        alignFrozen="right"
        :frozen="EditFrozen"
        v-if="
          (type === 'INLINE' && lockedRows.length > 0) ||
          (type === 'DIALOG' &&
            objects.length > 0 &&
            editingRows.length === 0 &&
            config.objectConfig.formConfig.withActions !== false)
        "
        :dir="isRTL ? 'rtl' : 'ltr'"
      >
        <template #body="slotProps">
          <div class="flex justify-content-end">
            <Button
              v-if="
                slotProps.data.id !== undefined &&
                deletable &&
                !slotProps.data.blockedRow &&
                (!config.objectConfig.formConfig.allowedActions ||
                  config.objectConfig.formConfig.allowedActions.includes(
                    'delete'
                  ))
              "
              style="margin-right: 10px"
              icon="pi pi-trash"
              class="grid-icon-delete"
              text
              rounded
              @click="() => Delete(slotProps.data)"
            />
            <Button
              v-if="
                slotProps.data.id !== undefined &&
                type === 'DIALOG' &&
                (!config.objectConfig.formConfig.allowedActions ||
                  config.objectConfig.formConfig.allowedActions.includes(
                    'update'
                  ))
              "
              icon="pi pi-pencil"
              class="grid-icon-edit"
              text
              rounded
              @click="() => editCol(slotProps)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <!-- v-model:selection="selectedRows" -->
    <DataTable
      v-else
      v-model:editingRows="editingRows"
      :frozenValue="lockedRows"
      tableClass="editable-cells-table"
      :tableStyle="{ minWidth: '30rem', maxHeight: '70vh' }"
      scrollHeight="60vh"
      v-model:filters="filters"
      class="custom-datatable"
      filterDisplay="menu"
      editMode="row"
      removableSort
      scrollable
      @row-click="handleRowClick"
      @row-edit-cancel="onRowEditCancel"
      @row-edit-save="onRowEditSave"
      :globalFilterFields="columns"
      :id="myCurrentComponent"
      :loading="loading"
      :value="objects"
      dataKey="id"
      :ref="isTableCreation ? 'dt' : ''"
      :reorderableColumns="
        props.reOrder || config.objectConfig.formConfig.enableReorder
      "
      @rowReorder="onRowReorder"
      v-model:selection="selectedObjects"
    >
      <template #header v-if="!store.local">
        <div class="flex justify-content-end">
          <Button
            v-if="
              config.objectConfig.formConfig.selectable &&
              selectedObjects &&
              selectedObjects.length >= 2
            "
            size="small"
            class="iconStyle"
            style="margin: 0.25%"
            @click="deleteSelectedRows"
          >
            <span class="pi pi-trash"></span>
          </Button>
          <Button
            v-if="
              type === 'DIALOG' &&
              (!config.objectConfig.formConfig.allowedActions ||
                config.objectConfig.formConfig.allowedActions.includes(
                  'create'
                ))
            "
            size="small"
            @click="openNew"
            class="iconStyle"
            style="margin: 0.25%"
          >
            <span class="p-button-text" v-if="addBtn">{{ addBtn }}</span>
            <span class="pi pi-plus" v-else></span>
          </Button>
        </div>
      </template>
      <template #empty>{{ t("NeoTable.empty") }}</template>
      <template #loading>{{ t("NeoTable.loading") }}</template>

      <!-- :header="column.columnConfig.options.label" -->
      <!-- <Column
      selectionMode="multiple"
      headerStyle="width: 3rem"
      v-if="config.objectConfig.formConfig.selectionMode"
    ></Column> -->
      <Column
        v-if="config.objectConfig.formConfig.selectable"
        :selectionMode="config.objectConfig.formConfig.selectionMode"
        headerStyle="width: 3rem"
      ></Column>
      <Column
        v-if="props.reOrder || config.objectConfig.formConfig.enableReorder"
        rowReorder
        headerStyle="width: 3rem"
        :reorderableColumn="false"
      />
      <Column
        v-for="column in filterColumnsByShow"
        :key="column.column_name"
        :field="column.column_name"
        :filterField="column.column_name"
        :sortable="column.sortable"
        :headerStyle="{
          'min-width': column.taille ? `${column.taille}rem` : '150px',
        }"
        :dir="isRTL ? 'rtl' : 'ltr'"
      >
        <template #header>
          <span
            v-tooltip.top="column.columnConfig.options.tooltip"
            style="font-weight: bold"
          >
            <span :class="{ underline: column.unique }">
              {{
                typeof column.columnConfig.options.label === "string" &&
                column.columnConfig.options.label.includes(".")
                  ? t(column.columnConfig.options.label)
                  : column.columnConfig.options.label
              }}
            </span>
          </span>
          <span
            v-show="column.columnConfig.options.required"
            style="color: red; margin-left: 5px; margin-right: 5px"
          >
            *
          </span>
        </template>
        <template #body="{ data, field }">
          <component
            v-if="
              ['NeoRating', 'NeoCheckbox'].includes(
                column.columnConfig.component
              )
            "
            :options="computedOptions(column.columnConfig.options)"
            :ref="column.columnConfig.options.name"
            :is="column.columnConfig.component"
            :isParentNeoTable="true"
            v-model="data[field as any]"
            :isRTL="isRTL"
          />
          <div v-else-if="column.columnConfig.component === 'NeoSelect'">
            {{
              neoSelectValue(
                data[field as any],
                column.columnConfig.options.returnObject,
                column.columnConfig.options.value,
                column.columnConfig.options.elements,
                column.columnConfig.options.key
              )
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoDatepicker'">
            {{ formatDate(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoTimePicker'">
            {{ formatTime(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoUploadFile'">
            {{
              data[field as any] && data[field as any].length > 0
                ? data[field as any]
                    .map((file: any) => file.fileName)
                    .join(", ")
                : ""
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoCheckboxGroup'">
            {{ data[field as any].toString() }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoFlowchart_V2'">
            {{ data[field as any].name }}
          </div>

          <div v-else-if="column.columnConfig.component === 'NeoContact'">
            {{
              contactDisplay(
                data[field as any],
                column.columnConfig.options.optionLabel
              )
            }}
            {{ data[field as any][column.columnConfig.options.optionLabel] }}
          </div>
          <div v-else>
            {{ data[field as any] }}
          </div>
        </template>

        <template #editor="{ data, field }">
          <div class="inline-component">
            <component
              v-if="column.columnConfig.component === 'NeoDatepicker'"
              :options="NotRequiredOptions(column.columnConfig.options)"
              :ref="column.columnConfig.options.name"
              :is="column.columnConfig.component"
              :isParentNeoTable="true"
              v-model="data[field]"
              :isRTL="isRTL"
              @update:modelValue="
                handleInputChange(column.columnConfig.options.even)
              "
            />
            <component
              v-else
              :options="NotRequiredOptions(column.columnConfig.options)"
              :ref="column.columnConfig.options.name"
              :is="column.columnConfig.component"
              :isParentNeoTable="true"
              v-model="data[field]"
              :isRTL="isRTL"
              @update:options="(e : any) => {
              column.columnConfig.options = e;
              }"
              @update:modelValue="
                handleInputChange(column.columnConfig.options.events)
              "
            />
          </div>
        </template>

        <template #filter="{ filterModel }" v-if="column.filters">
          <InputText
            v-model="filterModel.value"
            type="text"
            class="p-column-filter"
            placeholder="recherche ..."
          />
        </template>
      </Column>

      <Column
        v-if="type !== 'DIALOG'"
        :rowEditor="true"
        style="width: 2%; min-width: 2rem"
        header-class="headerClass"
        :bodyStyle="{
          'text-align': 'center',
        }"
        alignFrozen="right"
        :dir="isRTL ? 'rtl' : 'ltr'"
        class="p-cell-editing"
      />

      <Column
        v-if="config.objectConfig.formConfig.withActions !== false"
        style="width: 10%; max-width: 100px; min-width: 100px"
        :rowEditor="true"
        header-class="headerClass"
        :bodyStyle="{
          'text-align': 'center',
        }"
        alignFrozen="right"
        :frozen="EditFrozen"
        :dir="isRTL ? 'rtl' : 'ltr'"
      >
        <template #body="slotProps">
          <div class="flex justify-content-end">
            <Button
              v-if="
                slotProps.data.id !== undefined &&
                type === 'DIALOG' &&
                (!config.objectConfig.formConfig.allowedActions ||
                  config.objectConfig.formConfig.allowedActions.includes(
                    'update'
                  ))
              "
              icon="pi pi-pencil"
              class="grid-icon-edit"
              text
              rounded
              @click="() => editCol(slotProps)"
            />
            <Button
              v-if="
                slotProps.data.id !== undefined &&
                deletable &&
                !slotProps.data.blockedRow &&
                slotProps.data.id != editingRows[0]?.id &&
                (!config.objectConfig.formConfig.allowedActions ||
                  config.objectConfig.formConfig.allowedActions.includes(
                    'delete'
                  ))
              "
              style="margin-right: 10px"
              icon="pi pi-trash"
              class="grid-icon-delete"
              text
              rounded
              @click="() => Delete(slotProps.data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="formDialog"
      :header="isEditTableCol ? t('Dialog.edit') : t('Dialog.add')"
      :modal="true"
      :class="dialogClass"
      :style="dialogStyle"
      :closable="false"
      :dir="isRTL ? 'rtl' : 'ltr'"
    >
      <!-- {{ configForm.Resolution }} -->

      <component-form-table
        v-if="formDialog"
        v-model="form"
        :configForm="configForm"
        :myWatchedVariable="myWatchedVariable"
        @fieldsValueChanged="handleFieldsValue"
        :tableFields="tableFields"
        :isRTL="isRTL"
      ></component-form-table>

      <template #footer>
        <Button
          :label="t('Dialog.cancel')"
          icon="pi pi-times"
          @click="hideDialog"
          text
        />
        <Button
          :label="t('Dialog.validate')"
          icon="pi pi-check"
          type="submit"
          @click="setMyWatchedVariable"
        />
      </template>
    </Dialog>
  </div>
  <div v-else>
    <DataTable
      :tableStyle="{ minWidth: '30rem', maxHeight: '70vh' }"
      tableClass="editable-cells-table"
      scrollHeight="60vh"
      v-model:filters="filters"
      class="custom-datatable"
      filterDisplay="menu"
      removableSort
      scrollable
      :globalFilterFields="columns"
      :id="myCurrentComponent"
      :loading="loading"
      :value="objects"
      dataKey="id"
      :ref="isTableCreation ? 'dt' : ''"
      v-model:expandedRows="expandedRows"
      v-model:selection="selectedObjects"
    >
      <template #empty>{{ t("NeoTable.empty") }}</template>
      <template #loading>{{ t("NeoTable.loading") }}</template>
      <Column
        v-if="config.objectConfig.formConfig.selectable"
        :selectionMode="config.objectConfig.formConfig.selectionMode"
        headerStyle="width: 3rem"
      ></Column>
      <Column
        v-for="column in filterColumnsByShow"
        :key="column.column_name"
        :field="column.column_name"
        :filterField="column.column_name"
        :sortable="column.sortable"
        :headerStyle="{
          'min-width': column.taille ? `${column.taille}rem` : '150px',
        }"
        :dir="isRTL ? 'rtl' : 'ltr'"
      >
        <template #header>
          <span
            v-tooltip.top="column.columnConfig.options.tooltip"
            style="font-weight: bold"
          >
            <span :class="{ underline: column.unique }">
              {{
                typeof column.columnConfig.options.label === "string" &&
                column.columnConfig.options.label.includes(".")
                  ? t(column.columnConfig.options.label)
                  : column.columnConfig.options.label
              }}
            </span>
          </span>
          <span
            v-show="column.columnConfig.options.required"
            style="color: red; margin-left: 5px; margin-right: 5px"
          >
            *
          </span>
        </template>
        <template #body="{ data, field }">
          <component
            v-if="
              ['NeoRating', 'NeoCheckbox'].includes(
                column.columnConfig.component
              )
            "
            :options="computedOptions(column.columnConfig.options)"
            :ref="column.columnConfig.options.name"
            :is="column.columnConfig.component"
            :isParentNeoTable="true"
            v-model="data[field as any]"
            :isRTL="isRTL"
          />
          <div v-else-if="column.columnConfig.component === 'NeoSelect'">
            {{
              neoSelectValue(
                data[field as any],
                column.columnConfig.options.returnObject,
                column.columnConfig.options.value,
                column.columnConfig.options.elements,
                column.columnConfig.options.key
              )
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoDatepicker'">
            {{ formatDate(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoTimePicker'">
            {{ formatTime(data[field as any]) }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoUploadFile'">
            {{
              data[field as any] && data[field as any].length > 0
                ? data[field as any]
                    .map((file: any) => file.fileName)
                    .join(", ")
                : ""
            }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoCheckboxGroup'">
            {{ data[field as any].toString() }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoFlowchart_V2'">
            {{ data[field as any].name }}
          </div>
          <div v-else-if="column.columnConfig.component === 'NeoContact'">
            {{
              contactDisplay(
                data[field as any],
                column.columnConfig.options.optionLabel
              )
            }}
          </div>
          <div v-else>
            {{ data[field as any] }}
          </div>
        </template>

        <template #editor="{ data, field }">
          <div class="inline-component">
            <component
              :options="NotRequiredOptions(column.columnConfig.options)"
              :ref="column.columnConfig.options.name"
              :is="column.columnConfig.component"
              :isParentNeoTable="true"
              v-model="data[field]"
              :isRTL="isRTL"
              @update:options="(e : any) => {
              column.columnConfig.options = e;
              }"
            />
          </div>
        </template>

        <template #filter="{ filterModel }" v-if="column.filters">
          <InputText
            v-model="filterModel.value"
            type="text"
            class="p-column-filter"
            placeholder="recherche ..."
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import {
  nextTick,
  onBeforeMount,
  onMounted,
  ref,
  computed,
  watch,
  getCurrentInstance,
} from "vue";
import { FilterMatchMode, FilterOperator } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useAppStore } from "@/store/app.store";
import {
  createData,
  fetchDataByTableGuid,
  updateData,
  deleteData,
  logger,
  callEliseWebService,
} from "@/api/api";
import { useI18n } from "vue-i18n";
import { executeCodeAsync } from "@/utils/codeExecutor";
import {
  storeUtility,
  fieldUtility,
  formUtility,
  stringUtility,
  mathUtility,
  arrayUtility,
  eliseUtility,
  initializeBlocklyUtilities,
} from "@/utils/blocklyUtilities";

const props = defineProps({
  label: String,
  modelValue: {
    type: Array,
    default: () => [],
  },
  config: {
    type: Object,
    default: () => ({}),
  },
  params: {
    type: Array,
    default: () => [],
  },
  isTableCreation: {
    type: Boolean,
    default: false,
  },
  checkUnique: {
    type: Boolean,
    default: false,
  },
  addBtn: {
    type: String,
    default: "",
  },
  isRTL: {
    type: Boolean,
    default: false,
  },
  deletable: {
    type: Boolean,
    default: true,
  },
  readonly: {
    type: Boolean,
    default: false,
  },
  reOrder: {
    type: Boolean,
    default: false,
  },
  canAdd: {
    type: Boolean,
    default: true,
  },
});

const generateRandomString = (length: number) => {
  const characters = "0123456789";
  const randomArray = Array.from(
    { length },
    () => characters[Math.floor(Math.random() * characters.length)]
  );

  return randomArray.join("");
};

// Reactive references
const { t } = useI18n();
const formData = ref();
const tableFields = ref({} as any);
const isEditTableCol = ref(null);
const expandedRows = ref({}) as any;
const myCurrentComponent = ref(generateRandomString(10));
const loading = ref(false);
const editingRows = ref([] as any);
const filters = ref({} as any);
const dataForTable = ref([] as any);
const fieldsValue = ref({} as any);
const lockedRows = ref([] as any);
const EditFrozen = ref(true);
const showOnDetailColumns = ref({} as any);
const formDialog = ref(false);
const clearFields = ref(false);
const submitted = ref(false);
const myWatchedVariable = ref(false);

const confirm = useConfirm();
const toast = useToast();
const store = useAppStore();
const index = ref(null as any);
let form = ref<any>(null);
// Event emitters
const emit = defineEmits(["update:modelValue", "column", "selectedObjects"]);
const app = getCurrentInstance() as any;
const selectedObjects = ref([] as any);
// Computed properties

// Dynamic class for dialog based on resolution
const dialogClass = computed(() => {
  const resolution = configForm.value.Resolution;
  return !resolution || resolution.trim() === "" || resolution === "Plein écran"
    ? "p-dialog-maximized"
    : "";
});

// Dynamic inline styles for dialog dimensions
const dialogStyle = computed(() => {
  const resolution = configForm.value.Resolution;
  if (!resolution || resolution.trim() === "" || resolution === "Plein écran") {
    // If no resolution is provided or it's "Plein écran", no specific width/height is set
    return {};
  }

  // Handle valid resolution case
  const [width, height] = resolution
    .split("x")
    .map((value: any) => value.trim());
  return {
    width: `${width}px`,
    height: `${height}px`,
  };
});

const configForm = computed(() => props.config.objectConfig.formConfig);
const type = computed(
  () => props.config.objectConfig.formConfig.Type_Insertion
);
const columns = computed(() => {
  const tableConfig = ref(props.config.objectConfig);
  const column1Objects = ref([] as any);

  // Collect column data from zones
  tableConfig.value.formTemplate.forEach((zone: any) => {
    const columnNames = ["column1", "column2", "column3", "column4"];
    columnNames.forEach((columnName) => {
      if (zone.rows[columnName] !== undefined) {
        column1Objects.value.push(...zone.rows[columnName]);
      }
    });
  });

  // Build the returned columns based on configuration
  const returnedColumns = tableConfig.value.formConfig.TableColumns.reduce(
    (accumulator: any[], zone: any) => {
      let matchingColumn = null;

      column1Objects.value.forEach((column: any) => {
        if (zone.column_name === column.options.name) {
          if (column.options.showOnDetail) {
            showOnDetailColumns.value = column;
          } else {
            matchingColumn = column; // Keep the last matching column if not shown on detail
          }
        }
      });

      // Push the modified zone with its corresponding column configuration
      if (matchingColumn) {
        accumulator.push({
          ...zone,
          columnConfig: matchingColumn,
        });
      }

      return accumulator;
    },
    []
  );

  return returnedColumns as any; // Return the constructed array
});

const getColumnNames = (data: any) => {
  const columnNames = [] as any;

  // Loop through each row
  data.forEach((zone: any) => {
    Object.keys(zone.rows).forEach((column) => {
      zone.rows[column].forEach((item: any) => {
        // Check if item has options and extract name
        if (item.options && item.options.name) {
          columnNames.push(item.options.name);
        }
      });
    });
  });

  return columnNames;
};

const reorderColumns = (columns: any, order: any) => {
  // Create a map for quick lookup of the order based on 'column_name'
  const orderMap = order.reduce((map: any, item: any, index: any) => {
    map[item] = index;
    return map;
  }, {});

  // Sort the columns based on their 'column_name' value matching the 'column_name' in the order array
  return columns.sort((a: any, b: any) => {
    // Get the order index from the orderMap using 'column_name' from columns
    const orderA = orderMap[a.column_name] ?? Infinity;
    const orderB = orderMap[b.column_name] ?? Infinity;

    // Return the sorted columns based on their position in the order array
    return orderA - orderB;
  });
};

const data = computed(() => props.config.objectConfig.formTemplate);
const filterColumnsByShow = computed(() => {
  // First, filter the columns based on 'show'
  const filteredColumns = columns.value.filter(
    (column: any) => column.show === true
  );

  // Then, reorder the filtered columns based on the provided order
  return reorderColumns(filteredColumns, getColumnNames(data.value));
});

const objects = computed({
  get() {
    return typeof props.modelValue === "string"
      ? ([] as any)
      : (props.modelValue as any);
  },
  set(value) {
    emit("update:modelValue", value);
  },
});
const tabVarsComputed = computed(() => {
  const { TableID } = props.config.objectConfig.formConfig;
  return store.tableVariables.find((item) => item.key === TableID)?.value;
});
const uniqueColumns = computed(() => {
  const uniqueColumns = columns.value
    .filter((column: any) => column.unique)
    .map((column: any) => column.column_name);
  // i want to return only the columns name that have the unique property set to true
  return uniqueColumns;
});
const fetchTableData = async (code: string) => {
  return await fetchDataByTableGuid(code);
};
const handleInputChange = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find((event: any) => event.rule.code === "change");
  if (selectedEvent) {
    console.log("[NeoTable] Change event triggered");
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("[NeoTable] Error executing event code:", error);
      logger.error(error);
    }
  }
};
// Watchers
watch(
  () => data.value,
  (value: any) => {
    form.value = value; // Direct assignment instead of data.value
  }
);

watch(
  () => tabVarsComputed.value,
  async (value) => {
    if (!value) return; // Early exit if value is falsy
    console.log("[NeoTable] Table variables changed");
    // Helper function to find variable by key
    const findVariableByKey = (key: string) =>
      value.find((item: any) => item.key === key);
    try {
      // eval the code of the Last event
      console.log("[NeoTable] App refs available:", Object.keys(app.refs));
      await nextTick();
      // app.refs.TEST[0].disableField();
      const events = props.config.objectConfig.formConfig.events;
      if (events && events.length > 0) {
        const lastEventCode = events[events.length - 1].code;
        if (lastEventCode) {
          await executeCodeAsync(lastEventCode, createExecutionContext());
        }
      }
      setTimeout(async () => {
        await nextTick();
      }, 500);

      // rerender dom to apply the changes
    } catch (error) {
      console.error("Error parsing JSON:", error);
      logger.error(error);
    }
    // columns.value.forEach((column: any) => {
    //   const config = column.columnConfig;

    //   if (config.component === "NeoSelect") {
    //     const variable = findVariableByKey("select_options");
    //     if (variable) {
    //       // Use a simple map instead of pushing items into an array
    //       config.options.elements = (variable.value as unknown as string[]).map(
    //         (element: string) => ({
    //           code: element,
    //           name: element,
    //         })
    //       );
    //     }
    //   }

    //   if (config.component === "NeoTextField") {
    //     const variable = findVariableByKey("text_value");
    //     if (variable) {
    //       config.options.label = variable.value;
    //     }
    //   }
    // });
  },
  { deep: true }
);
watch(
  () => props.params,
  async (value) => {
    try {
      console.log("[NeoTable] Params changed:", value);
      await executeCodeAsync(
        configForm.value.events[3].code,
        createExecutionContext()
      );
    } catch (error) {
      console.error("Error parsing Excel data:", error);
      logger.error(error);
    }
  },
  { deep: true }
);
// Methods
const changeIcon = () => {
  if (
    props.config.objectConfig.formConfig.allowedActions &&
    !props.config.objectConfig.formConfig.allowedActions.includes("create")
  )
    return;
  try {
    const container = document.getElementById(myCurrentComponent.value);
    nextTick(() => {
      const firstRowSaveButton = container?.querySelector(
        ".p-datatable-row-editor-save"
      ) as HTMLInputElement;
      // Check if the button exists before changing its innerHTML
      if (firstRowSaveButton) {
        firstRowSaveButton.innerHTML = '<i class="pi pi-plus" />';
      }
    });
  } catch (error) {
    console.error("Error changing icon:", error);
    logger.error(error);
  }
};

const forceChangeIcon = () => {
  if (
    props.config.objectConfig.formConfig.allowedActions &&
    !props.config.objectConfig.formConfig.allowedActions.includes("create")
  )
    return;
  try {
    const container = document.getElementById(myCurrentComponent.value);

    if (container) {
      const observer = new MutationObserver(() => {
        const firstRowSaveButton = container.querySelector(
          ".p-datatable-row-editor-save"
        ) as HTMLInputElement;

        // Check if the button exists before changing its innerHTML
        if (firstRowSaveButton) {
          firstRowSaveButton.innerHTML = '<i class="pi pi-plus" />';
          observer.disconnect(); // Stop observing once the element is found and updated
        }
      });

      // Start observing the container for changes
      observer.observe(container, { childList: true, subtree: true });

      // Initial check in case the element is already present
      const firstRowSaveButton = container.querySelector(
        ".p-datatable-row-editor-save"
      ) as HTMLInputElement;

      if (firstRowSaveButton) {
        firstRowSaveButton.innerHTML = '<i class="pi pi-plus" />';
        observer.disconnect(); // Stop observing if the element is already present
      }
    }
  } catch (error) {
    console.error("Error changing icon:", error);
    logger.error(error);
  }
};

const uuidv4 = () => {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const randomValue = (Math.random() * 16) | 0; // Generate a random value between 0 and 15
    const value = c === "x" ? randomValue : (randomValue & 0x3) | 0x8; // Adjust for 'y' to ensure correct version
    return value.toString(16); // Convert to hex string
  });
};

// Create execution context with all necessary variables and functions
const createExecutionContext = () => ({
  // Helper functions
  uuidv4,

  // Data objects
  Fields: store.Fields,
  objects,
  fieldsValue,
  tableFields,
  selectedObjects,
  formData,
  editingRows,
  lockedRows,

  // System objects
  app,
  store,
  toast,
  t,

  // Blockly utilities - Secure abstraction layer
  fieldUtility,
  stringUtility,
  mathUtility,
  arrayUtility,
  eliseUtility,
  storeUtility,
  formUtility,
});

const computedOptions = (originalOptions: Record<string, any>) => {
  // Create a shallow copy and override properties in one step
  return {
    ...originalOptions,
    readonly: true,
    disabled: true,
  };
};

const NotRequiredOptions = (originalOptions: Record<string, any>) => {
  // Create a shallow copy and override the 'required' property
  return {
    ...originalOptions,
    required: false,
  };
};

const createEmptyObject = (data: Array<any>): Record<string, any> => {
  const result: Record<string, any> = {};

  data.forEach((column: any) => {
    // Initialize values based on the component type
    result[column.column_name] =
      column.columnConfig.component === "NeoSwitch" ||
      column.columnConfig.component === "NeoCheckbox"
        ? true
        : "";
  });

  return result;
};

const areObjectAttributesNotEmpty = (obj: Record<string, any>): boolean => {
  const requiredColumnNames = columns.value
    .filter((column: any) => column.columnConfig.options.required)
    .map((column: any) => column.columnConfig.options.name);

  // Check if all required attributes are non-empty
  return requiredColumnNames.every((key: any) => {
    if (obj.hasOwnProperty(key)) {
      const value = obj[key];
      return !(typeof value === "string" && value.trim() === "");
    }
    return true; // If the key doesn't exist, treat it as valid
  });
};
const checkDuplicateUniqueAttribute = (
  obj: Record<string, any>,
  uniqueKeys: any,
  index?: number
): boolean => {
  for (let i = 0; i < objects.value.length; i++) {
    if (index !== undefined && i === index) continue; // Skip the index to be excluded if provided
    for (let j = 0; j < uniqueKeys.length; j++) {
      if (obj[uniqueKeys[j]] === objects.value[i][uniqueKeys[j]]) {
        return true; // Return true if any key is duplicate
      }
    }
  }
  return false;
};
// Detect Changes and Save
const setMyWatchedVariable = () => {
  myWatchedVariable.value = true;
};

// Functions to handle row actions
const handleRowClick = (event: any) => {
  if (
    type.value === "DIALOG" ||
    (props.config.objectConfig.formConfig.allowedActions &&
      !props.config.objectConfig.formConfig.allowedActions.includes("update"))
  )
    return; // Early return for 'DIALOG' type or if edit is not allowed
  onRowEditInit(event); // Proceed if type is not 'DIALOG'
};

const editCol = (slotProps: any) => {
  index.value = slotProps.index;
  tableFields.value = slotProps.data;
  isEditTableCol.value = slotProps.data.id || null; // Ensuring a valid value for id
  formDialog.value = true;
};

const Delete = async (obj: any) => {
  confirm.require({
    message: t("Setup.confirmImport"),
    header: t("ActionButtons.delete"),
    icon: "pi pi-info-circle",

    rejectLabel: t("ConfirmDialog.reject"),
    rejectClass: "p-button-danger",
    acceptLabel: t("ConfirmDialog.accept"),
    accept: async () => {
      const formConfig = props.config.objectConfig.formConfig;
      const onRowDeleteEvent = formConfig?.events?.find(
        (event: any) => event.rule.code === "onRowDelete"
      )?.code;

      // If not JSON, call deleteData
      if (formConfig?.sortie !== "JSON") {
        try {
          deleteData(obj.id)
            .then(() => {
              objects.value = objects.value.filter((o: any) => o.id !== obj.id);
            })
            .catch((error) => {
              console.error("Error deleting object:", error);
              logger.error(error);
            });
        } catch (error) {
          console.error("Error deleting object:", error);
          logger.error(error);
        }
      } else {
        objects.value = objects.value.filter((o: any) => o.id !== obj.id);
      }

      await nextTick();

      // If onRowDeleteFunction is defined, evaluate it
      if (onRowDeleteEvent) {
        try {
          await executeCodeAsync(onRowDeleteEvent, createExecutionContext());
        } catch (error) {
          console.error("Error in onRowDelete function:", error);
          logger.error(error);
        }
      }

      // Handle table creation case
      if (props.isTableCreation) {
        store.deleteID(obj.column_name);
      }

      onRowEditCancel();
      hideDialog();
    },
    reject: () => {
      // Optional: Handle rejection case if needed
    },
  });
};

const deleteSelectedRows = () => {
  if (!selectedObjects.value || selectedObjects.value.length < 2) return;
  confirm.require({
    message: t("NeoTable.confirmDeleteSelected"),
    header: t("ActionButtons.delete"),
    icon: "pi pi-info-circle",
    rejectLabel: t("ConfirmDialog.reject"),
    rejectClass: "p-button-danger",
    acceptLabel: t("ConfirmDialog.accept"),
    accept: async () => {
      const formConfig = props.config.objectConfig.formConfig;
      const onRowDeleteEvent = formConfig?.events?.find(
        (event: any) => event.rule.code === "onRowDelete"
      )?.code;
      let idsToDelete: any[] = [];
      for (const row of selectedObjects.value) {
        idsToDelete.push(row.id);
        // If not JSON, call deleteData
        if (formConfig?.sortie !== "JSON") {
          try {
            await deleteData(row.id);
          } catch (error) {
            console.error("Error deleting object:", error);
            logger.error(error);
          }
        }
        // Handle table creation case
        if (props.isTableCreation) {
          store.deleteID(row.column_name);
        }
      }
      // Remove from local objects
      objects.value = objects.value.filter(
        (row: any) => !idsToDelete.includes(row.id)
      );
      selectedObjects.value = [];
      await nextTick();
      // If onRowDeleteFunction is defined, evaluate it (once after all deletions)
      if (onRowDeleteEvent) {
        try {
          await executeCodeAsync(onRowDeleteEvent, createExecutionContext());
        } catch (error) {
          console.error("Error in onRowDelete function:", error);
          logger.error(error);
        }
      }
      toast.add({
        severity: "success",
        summary: t("Toast.success"),
        detail: t("NeoTable.massDeleteSuccess"),
        life: 3000,
      });
      onRowEditCancel();
      hideDialog();
    },
    reject: () => {},
  });
};
const onRowEditInit = (event: { data: Record<string, any>; index: number }) => {
  const { data } = event;
  if (data?.id !== undefined) {
    lockedRows.value = [];
    editingRows.value = [{ ...data }];
  }
};

const onRowEditSave = async (event: any) => {
  var fillRow = false; // Flag to determine if we need to fill the row
  var to_create = false; // Flag to determine if we're creating a new row
  var isJsonOutput = props.config.objectConfig.formConfig.sortie == "JSON";
  let { newData, index, data } = event; // Destructure the event object to get newData, index, and data

  // Find the 'beforeRowSave' event function, if any
  const beforeRowSaveFunction =
    props.config.objectConfig.formConfig?.events?.find(
      (event: any) => event.rule.code == "beforeRowSave"
    )?.code;
  // Execute the 'beforeRowSave' function BEFORE any validation or saving logic
  if (
    beforeRowSaveFunction != undefined &&
    beforeRowSaveFunction != null &&
    beforeRowSaveFunction != ""
  ) {
    try {
      // Store original data for comparison
      const originalData = JSON.stringify(fieldsValue.value);
      // Execute the function and capture the potentially modified newData
      const context = {
        ...createExecutionContext(),
        newData: newData,
      };

      const resp = await executeCodeAsync(beforeRowSaveFunction, context);
      // Check if newData was modified in the context
      if (context.newData && JSON.stringify(context.newData) !== originalData) {
        fieldsValue.value = context.newData;
        newData = context.newData; // Also update the local value variable
      }
    } catch (error) {
      console.error("[NeoTable] Error executing beforeRowSave event:", error);
      logger.error(error);
    }
  }

  // Find the 'afterRowSave' event function, if any
  const afterRowSaveFunction =
    props.config.objectConfig.formConfig?.events?.find(
      (event: any) => event.rule.code == "afterRowSave"
    )?.code;
  // Execute the 'afterRowSave' function, if it exists
  console.log(
    "[NeoTable] AfterRowSave function available:",
    !!afterRowSaveFunction
  );
  if (
    afterRowSaveFunction != undefined &&
    afterRowSaveFunction != null &&
    afterRowSaveFunction != ""
  ) {
    try {
      await executeCodeAsync(afterRowSaveFunction, createExecutionContext());
    } catch (error) {
      console.error("[NeoTable] Error executing afterRowSave event:", error);
      logger.error(error);
    }
  }
  // Initialize the new object structure for dataJson
  const newObject = {
    dataJson: {
      guid: uuidv4(), // Generate a unique identifier
      application: "NEOFORM",
      dataType: "TAB",
      objectId: "",
      objectGuid: "",
      isEncrypted: false,
      datas: {},
    },
  };
  // Function to check if column_name is unique
  const isColumnNameUnique = (
    columnName: string,
    indexToExclude: number = -1
  ) => {
    return !objects.value.some((obj: any, idx: number) => {
      return obj.column_name === columnName && idx !== indexToExclude;
    });
  };

  // Function to check if id is unique
  const isIdUnique = (id: string, indexToExclude: number = -1) => {
    return !objects.value.some((obj: any, idx: number) => {
      return obj.id === id && idx !== indexToExclude;
    });
  };

  // Logic for uniqueness check
  if (props.isTableCreation && props.checkUnique) {
    // Check if column_name is unique
    if (
      !isColumnNameUnique(
        newData.column_name,
        event.data?.id ? index : undefined
      )
    ) {
      // If the type is not 'DIALOG', reset the form and editing states
      if (type.value != "DIALOG" && props.canAdd) {
        if (
          props.config.objectConfig.formConfig.allowedActions &&
          !props.config.objectConfig.formConfig.allowedActions.includes(
            "create"
          )
        )
          return;
        const emptyObj = createEmptyObject(columns.value); // Create an empty object template based on the columns
        lockedRows.value = event.data?.id == undefined ? [{ ...newData }] : []; // Reset locked rows
        editingRows.value = [{ ...newData }]; // Reset editing rows
        event.data?.id == undefined || changeIcon(); // Handle any icon state changes
      }
      toast.add({
        severity: "error",
        summary: t("Toast.error"),
        detail: t("NeoTable.uniqueColumnName"),
        life: 3000,
      });
      return;
    }
  } else {
    // If `props.isTableCreation` is false, check if there's an id in the newData
    // console.log("newData", newData);

    if (event.newData?.id != undefined) {
      // If there's an id, check its uniqueness
      if (
        !isIdUnique(
          event.data?.id,
          event.data?.id != undefined ? index : undefined
        )
      ) {
        // If the type is not 'DIALOG', reset the form and editing states
        if (type.value != "DIALOG" && props.canAdd) {
          if (
            props.config.objectConfig.formConfig.allowedActions &&
            !props.config.objectConfig.formConfig.allowedActions.includes(
              "create"
            )
          )
            return;
          const emptyObj = createEmptyObject(columns.value); // Create an empty object template based on the columns
          lockedRows.value =
            event.data?.id == undefined ? [{ ...newData }] : []; // Reset locked rows
          editingRows.value = [{ ...newData }]; // Reset editing rows
          event.data?.id == undefined; // Handle any icon state changes
        }
        toast.add({
          severity: "error",
          summary: t("Toast.error"),
          detail: "L'ID doit être unique.",
          life: 3000,
        });
        return;
      }
    }
    // If there's no id, skip the uniqueness check
  }

  // Now proceed with the existing logic for updating or creating rows
  if (event.data?.id != undefined) {
    // Validate that all required fields are filled in
    if (
      !areObjectAttributesNotEmpty(newData) ||
      checkDuplicateUniqueAttribute(newData, uniqueColumns.value, index)
    ) {
      if (!areObjectAttributesNotEmpty(newData)) {
        toast.add({
          severity: "error",
          summary: t("Toast.error"),
          detail: t("NeoTable.requiredFields"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("Toast.error"),
          detail: t("NeoTable.uniqueFields"),
          life: 3000,
        });
      }

      // Show error if fields are missing
    } else {
      // If the output format is JSON or local storage is used
      if (isJsonOutput || store.local) {
        objects.value[index] = newData; // Update the local object directly
      } else {
        // Prepare the object to send to the backend for updating
        newObject.dataJson = {
          guid: uuidv4(),
          application: "NEOFORM",
          isEncrypted: false,
          dataType: "TAB",
          objectId: props.config.id,
          objectGuid: props.config.guid,
          datas: newData,
        };
        var id = event.data.id;
        var dataJson = newObject.dataJson;
        // Call the API to update the object with the new data
        const obj = await updateData({ id, dataJson });
        // Update the local state with the modified data
        objects.value[index] = newData;
      }
      // Emit the updated model to the parent component
      emit("update:modelValue", objects.value);
    }
  } else {
    // For new rows (without `id`), handle creation logic
    if (
      !areObjectAttributesNotEmpty(newData) ||
      checkDuplicateUniqueAttribute(newData, uniqueColumns.value)
    ) {
      // Show error if required fields are missing
      if (!areObjectAttributesNotEmpty(newData)) {
        toast.add({
          severity: "error",
          summary: t("Toast.error"),
          detail: t("NeoTable.requiredFields"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("Toast.error"),
          detail: t("NeoTable.uniqueFields"),
          life: 3000,
        });
      }
      fillRow = true;
    } else {
      // If not using JSON output or local storage, flag this as a new creation
      if (!(isJsonOutput || store.local)) {
        to_create = true;
      }
      // Add the new row to the objects list
      objects.value.push({
        id: generateRandomString(15), // Generate a temporary unique ID
        ...newData,
      });
      // Emit the new data as a column
      emit("column", { ...newData });
    }
  }

  // If the type is not 'DIALOG', reset the form and editing states
  if (type.value != "DIALOG" && props.canAdd) {
    if (
      props.config.objectConfig.formConfig.allowedActions &&
      !props.config.objectConfig.formConfig.allowedActions.includes("create")
    )
      return;
    const emptyObj = createEmptyObject(columns.value); // Create an empty object template based on the columns
    lockedRows.value = fillRow ? [{ ...newData }] : [{ ...emptyObj }]; // Reset locked rows
    editingRows.value = fillRow ? [{ ...newData }] : [{ ...emptyObj }]; // Reset editing rows
    changeIcon(); // Handle any icon state changes
  }

  // If creating a new row, send the new object data to the backend for saving
  if (to_create && !isJsonOutput) {
    newObject.dataJson = {
      guid: uuidv4(),
      application: "NEOFORM",
      dataType: "TAB",
      isEncrypted: false,
      objectId: props.config.id,
      objectGuid: props.config.guid,
      datas: newData,
    };
    const obj = await createData(newObject); // API call to create new data
    objects.value[objects.value.length - 1].id = obj.id; // Update the last object's ID with the server response
  }
};

const onRowReorder = (event: any) => {
  objects.value = event.value;
};

const onRowEditCancel = (event?: unknown) => {
  if (type.value === "DIALOG") {
    editingRows.value = [];
    return; // Early return for clarity
  }

  if (lockedRows.value.length === 0 && props.canAdd) {
    if (
      props.config.objectConfig.formConfig.allowedActions &&
      !props.config.objectConfig.formConfig.allowedActions.includes("create")
    )
      return;
    const emptyObj = createEmptyObject(columns.value);
    lockedRows.value = [{ ...emptyObj }];
    editingRows.value = [{ ...emptyObj }];
    changeIcon();
  }
};

const handleFieldsValue = async (value: any) => {
  var isJsonOutput = props.config.objectConfig.formConfig.sortie == "JSON";

  // Initialize a new object structure
  const newObject = {
    dataJson: {
      guid: uuidv4(),
      application: "NEOFORM",
      dataType: "TAB",
      objectId: props.config.id || "",
      objectGuid: props.config.guid || "",
      datas: {},
    },
  };

  // Check if the required fields are populated
  if (
    !areObjectAttributesNotEmpty(value) ||
    checkDuplicateUniqueAttribute(value, uniqueColumns.value, index.value)
  ) {
    if (!areObjectAttributesNotEmpty(value)) {
      toast.add({
        severity: "error",
        summary: t("Toast.error"),
        detail: t("NeoTable.requiredFields"),
        life: 3000,
      });
    } else {
      toast.add({
        severity: "error",
        summary: t("Toast.error"),
        detail: t("NeoTable.uniqueFields"),
        life: 3000,
      });
    }
    myWatchedVariable.value = false;
    return; // Early return if attributes are not valid
  }

  // Handle uniqueness check when props.isTableCreation is true
  if (props.isTableCreation) {
    const isColumnNameUnique = !objects.value.some((obj: any, idx: number) => {
      return obj.column_name === value.column_name && idx !== index.value;
    });

    const isIdUnique = !objects.value.some((obj: any, idx: number) => {
      return obj.id == value.id && idx !== index.value;
    });

    if (!isColumnNameUnique) {
      toast.add({
        severity: "error",
        summary: t("Toast.error"),
        detail: t("NeoTable.uniqueColumnName"),
        life: 3000,
      });
      myWatchedVariable.value = false; // Reset watched variable
      return;
    }

    if (!isIdUnique) {
      toast.add({
        severity: "error",
        summary: t("Toast.error"),
        detail: "L'ID doit être unique.",
        life: 3000,
      });
      myWatchedVariable.value = false; // Reset watched variable
      return;
    }
  }

  fieldsValue.value = { ...value }; // Store the field values

  // Find the 'beforeRowSave' event function, if any
  const beforeRowSaveFunction =
    props.config.objectConfig.formConfig?.events?.find(
      (event: any) => event.rule.code == "beforeRowSave"
    )?.code;
  // Execute the 'beforeRowSave' function BEFORE any validation or saving logic
  console.log(
    "[NeoTable] BeforeRowSave function available:",
    !!beforeRowSaveFunction
  );
  if (beforeRowSaveFunction != undefined) {
    try {
      // Store original data for comparison
      const originalData = JSON.stringify(fieldsValue.value);

      // Execute the function and capture the potentially modified newData
      const context = {
        ...createExecutionContext(),
        newData: fieldsValue.value,
      };

      await executeCodeAsync(beforeRowSaveFunction, context);

      // Check if newData was modified in the context
      if (context.newData && JSON.stringify(context.newData) !== originalData) {
        console.log(
          "newData was modified by beforeRowSave, updating fieldsValue"
        );
        fieldsValue.value = context.newData;
        value = context.newData; // Also update the local value variable
      }
    } catch (error) {
      console.error(
        "[NeoTable] Error executing beforeColumnSave event:",
        error
      );
      logger.error(error);
    }
  }

  // Handle editing of an existing table column
  if (isEditTableCol.value !== null) {
    const id = isEditTableCol.value;
    // Prepare the data JSON structure
    const dataJson = {
      guid: uuidv4(),
      application: "NEOFORM",
      dataType: "TAB",
      objectId: props.config.id,
      objectGuid: props.config.guid,
      datas: fieldsValue.value,
    };

    // Update the data based on the output format
    if (!isJsonOutput) {
      const obj = await updateData({ id, dataJson });
      const updatedObjects = objects.value.map((obj: any) =>
        obj.id === id ? { ...fieldsValue.value, id: obj.id } : obj
      );
      emit("update:modelValue", updatedObjects);
    } else {
      if (props.isTableCreation) {
        store.updateID(
          objects.value[index.value].column_name,
          value.column_name
        ); // Add the column name to the store
      }

      const updatedObjects = objects.value.map((obj: any) =>
        obj.id === id
          ? fieldsValue.value.id
            ? { ...fieldsValue.value } // If `id` exists in `fieldsValue.value`, just spread `fieldsValue.value`
            : { ...fieldsValue.value, id: obj.id } // If `id` does not exist, include `id` from `obj`
          : obj
      );
      emit("update:modelValue", updatedObjects);
    }

    isEditTableCol.value = null; // Reset edit state
  } else {
    // Handle creation of a new entry
    if (!isJsonOutput) {
      newObject.dataJson.datas = fieldsValue.value; // Fill in the data
      const createdObj = await createData(newObject);
      dataForTable.value.push(createdObj); // Update the table data
      const updatedObjects = [
        ...objects.value,
        { ...fieldsValue.value, id: createdObj.id },
      ];
      emit("update:modelValue", updatedObjects);
    } else {
      const newEntry = { id: generateRandomString(15), ...fieldsValue.value };
      const updatedObjects = [...objects.value, newEntry];

      emit("update:modelValue", updatedObjects);
    }
  }

  if (props.isTableCreation) {
    if (index.value == null) {
      store.addID(value.column_name);
    }
  }

  console.log("[NeoTable] Saving column fields");
  console.log("[NeoTable] Config available:", !!props.config.objectConfig);

  // Find the 'afterRowSave' event function, if any
  const afterRowSaveFunction =
    props.config.objectConfig.formConfig?.events?.find(
      (event: any) => event.rule.code == "afterRowSave"
    )?.code;

  // Execute the 'afterRowSave' function, if it exists
  console.log(
    "[NeoTable] AfterRowSave function for column available:",
    !!afterRowSaveFunction
  );
  if (
    afterRowSaveFunction != undefined &&
    afterRowSaveFunction != null &&
    afterRowSaveFunction != ""
  ) {
    try {
      const context = {
        ...createExecutionContext(),
        newData: fieldsValue.value,
      };
      await executeCodeAsync(afterRowSaveFunction, context);
    } catch (error) {
      console.error("[NeoTable] Error executing afterColumnSave event:", error);
      logger.error(error);
    }
  }

  myWatchedVariable.value = false;
  hideDialog();
};

// Helper functions
const neoSelectValue = (
  value: any,
  returnObject: boolean,
  valueKey: string,
  elements: Array<any>,
  key: string
) => {
  if (
    returnObject == false ||
    returnObject == undefined ||
    returnObject == null
  ) {
    const elem = elements.find((element: any) => element.code === value);
    if (elem == undefined) {
      return value;
    }
    return elem.name || elem[key];
  }
  if (returnObject) {
    return value[key];
  } else {
    const element = elements.find((element: any) => element.code === value);
    return element ? element[key] : "";
  }
};

const formatDate = (date: Date | string) => {
  const parsedDate = new Date(date);

  // Check if date is valid
  if (isNaN(parsedDate.getTime())) {
    return "";
  }

  return parsedDate.toLocaleDateString(undefined, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
};

const formatTime = (date: Date | string) => {
  const parsedDate = new Date(date);

  // Check if date is valid
  if (isNaN(parsedDate.getTime())) {
    return "";
  }

  return parsedDate.toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
};

const contactDisplay = (value: any, labelOption: string) => {
  return value[labelOption] ?? value.mission.person.name ?? value;
};

const initFilters = (columns: Array<any>) => {
  const filterConfig: any = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  };

  columns.forEach((column) => {
    filterConfig[column.column_name] = {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    };
  });

  filters.value = filterConfig;
  return filters;
};

const openNew = () => {
  if (type.value !== "INLINE") {
    index.value = null;
    clearFields.value = true; // Clear any existing fields
    formDialog.value = true; // Open the form dialog
  } else {
    // Prepare form data for inline type
    formData.value = {
      rows: {},
    };
    submitted.value = false; // Reset submission state
    clearFields.value = true; // Clear any existing fields
  }
};

const hideDialog = () => {
  // Reset form and dialog states
  clearFields.value = true; // Clear form fields
  formDialog.value = false; // Close the dialog
  submitted.value = false; // Reset submission status
  tableFields.value = {}; // Clear table field values
  isEditTableCol.value = null; // Reset edit state
};

// Lifecycle hooks
onBeforeMount(async () => {
  const isJsonOutput = props.config.objectConfig.formConfig.sortie === "JSON";
  // Initialize objects if empty
  if (objects.value.length === 0) {
    objects.value = [];
  }

  // Map through objects to assign unique IDs
  objects.value = objects.value.map((item: any, index: any) => ({
    ...item,
    id: item.id != null && item.id != undefined ? item.id : index,
  }));

  // Fetch data if not in referential creation mode
  if (!props.isTableCreation && !store.local && !isJsonOutput) {
    dataForTable.value = await fetchDataByTableGuid(props.config.guid);
  }
  // Process dataForTable if sortie is not JSON
  if (props.config.objectConfig.formConfig.sortie !== "JSON") {
    dataForTable.value.forEach((item: any) => {
      const datas = JSON.parse(item.dataJson).datas;
      if (datas) {
        objects.value.push({ ...datas, id: item.id });
      }
    });
  }
});

onMounted(async () => {
  // Initialize Blockly utilities with the component context
  try {
    initializeBlocklyUtilities({
      app: app,
      store: store,
    });
  } catch (error) {
    console.error("Failed to initialize Blockly utilities:", error);
    logger.error(error);
  }

  // Set the form value from data
  form.value = data.value;
  try {
    initializeBlocklyUtilities({
      app: app,
      store: store,
    });
  } catch (error) {
    console.error("Failed to initialize Blockly utilities:", error);
    logger.error(error);
  }

  // Set the form value from data
  form.value = data.value;
  console.log("[NeoTable] isTableCreation:", props.isTableCreation);
  // Set the current table ID if not in table creation mode
  if (!props.isTableCreation) {
    console.log(
      "[NeoTable] Setting current table ID:",
      props.config.objectConfig.formConfig.TableID
    );
    store.setCurrentTable(props.config.objectConfig.formConfig.TableID);
  }

  // Set table variables if they exist
  const { variables, TableID } = props.config.objectConfig.formConfig || {};
  if (variables && !props.config.objectConfig.formConfig?.isConfig) {
    const tableVariables: TableVariables[] = [
      {
        key: TableID,
        value: variables,
      },
    ];
    store.setTableVariables(tableVariables);
  }
  if (type.value !== "DIALOG" && props.canAdd) {
    forceChangeIcon();
  }
  initFilters(columns.value);

  // Set up empty objects for locked and editing rows if not in dialog mode
  if (type.value !== "DIALOG" && props.canAdd) {
    if (
      props.config.objectConfig.formConfig.allowedActions &&
      !props.config.objectConfig.formConfig.allowedActions.includes("create")
    )
      return;
    const emptyObj = createEmptyObject(columns.value);
    lockedRows.value = [{ ...emptyObj }];
    editingRows.value = [{ ...emptyObj }];
  }
  // try {
  //     // eval the code of the Last event
  //     console.log("app refs", app.refs);
  //     await nextTick();
  //     await eval(
  //       "(async () => { const store = useAppStore(); " +
  //         props.config.objectConfig.formConfig.events[0].code +
  //         "})()"
  //     );
  //     setTimeout(async () => {
  //       await nextTick();
  //     }, 500);

  //     // rerender dom to apply the changes
  //   } catch (error) {
  //     console.error("Error parsing JSON:", error);
  //     logger.error(error);
  //   }
});
const refs = computed(() => app.refs);
console.log(
  "[NeoTable] Component refs available:",
  refs.value ? Object.keys(refs.value).length : 0
);
// Expose the references to the parent component

// Expose the references to the parent component
const downloadObjectAsJson = (data: any, fileName: any) => {
  const jsonData = JSON.stringify(data);
  const blob = new Blob([jsonData], { type: "application/json" });
  const url = URL.createObjectURL(blob);

  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;

  document.body.appendChild(a);
  a.click();
  URL.revokeObjectURL(url);
};
const selectedRows = ref();
const selectionModeButtons = computed(() => {
  return [
    {
      type: "Button",
      label: t("ActionButtons.export"),
      command: () => {
        downloadObjectAsJson(
          selectedRows.value,
          "TAB_" + // name of the table
            props.config.objectConfig.formConfig.TableID +
            "_" +
            new Date().toISOString().slice(0, 10) +
            "_" +
            new Date().toLocaleTimeString().slice(0, 8)
        );
      },
    },
    {
      type: "Button",
      label: t("ActionButtons.delete"),
      command: () => {
        confirm.require({
          message: "Êtes-vous sur de vouloir continuer?",
          header: t("ActionButtons.delete"),
          icon: "pi pi-info-circle",

          acceptLabel: t("ConfirmDialog.accept"),
          rejectLabel: t("ConfirmDialog.reject"),
          rejectClass: "p-button-danger",
          accept: async () => {
            // isLoading.value = true;
            selectedRows.value.forEach(async (element: any) => {
              await deleteObject(element.id);
            });
            // isLoading.value = false;
            // hideDialog();
            // toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 });
          },
          reject: () => {
            // toast.add({ severity: 'error', summary: 'Rejected', detail: 'You have rejected', life: 3000 });
          },
        });
      },
    },
    {
      type: "Button",
      label: "Executer l'event",
      command: () => {},
    },
  ];
});
async function deleteObject(id: any) {
  if (props.config.objectConfig.formConfig.sortie !== "JSON") {
    await deleteData(id);
    objects.value = objects.value.filter((item: any) => item.id != id);
  } else {
    objects.value = objects.value.filter((item: any) => item.id != id);
  }
}

// watch selectedRows to get the selected objects
watch(selectedObjects, (newValue) => {
  if (!newValue || newValue.length === 0) {
    console.log("No rows selected");
    emit("selectedObjects", []); // Emit an empty array if no rows are selected
    return;
  }
  if (newValue) {
    console.log("Selected rows:", newValue);
    emit("selectedObjects", selectedObjects.value);
  }
});

async function executeWebService(webServiceName: String, parameters: any) {
  const obj = {
    eliseWsInputType: webServiceName,
    objet: parameters,
  };
  console.log("[NeoTable] Calling Elise web service:", webServiceName);
  try {
    const result = await callEliseWebService(obj);
    return result;
  } catch (error) {
    console.error("[NeoTable] Error calling Elise web service:", error);
    logger.error(error);
    return error;
  }
}
defineExpose({
  refs,
  formDialog,
  form,
  tableFields,
  myWatchedVariable,
  fetchTableData,
  executeWebService,
});
</script>

<style lang="scss">
.custom-disabled {
  color: #00000082 !important; /* Keep the text black even when disabled */
  background-color: transparent !important; /* Optional: Keep background white */
  border-color: transparent !important; /* Optional: Remove the border */
  box-shadow: none !important; /* Optional: Remove the shadow */
  opacity: 1 !important; /* Remove the greyed-out effect */
  /* Optional: Show a "not allowed" cursor */
}
.p-cell-editing {
  // background-color: #fdfcfc; /* Slightly different background color */
  backdrop-filter: blur(2px); /* Apply a slight blur effect */
}
.iconStyle {
  height: 25px;
  width: 25px;
  margin: 0.1%;
}

.p-datatable .p-inputnumber {
  width: 100%;
  padding-top: 5px !important;
}

.p-datatable .p-inputtext {
  width: 100%;
  padding-top: 5px !important;
}
</style>
