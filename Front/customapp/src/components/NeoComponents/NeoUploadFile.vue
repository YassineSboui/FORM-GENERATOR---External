<template>
  <div class="neoUploadFile" v-show="!isHidden">
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
      <!-- rest of the template -->
      <FileUpload
        v-if="!isParentNeoTable"
        mode="basic"
        name="demo[]"
        @uploader="onAdvancedUpload($event)"
        customUpload
        :multiple="options.multiple ? true : false"
        :maxFileSize="options.size"
        :accept="options.accept"
        :disabled="isDisabled"
      >
        <template #empty>
          <div
            class="flex align-items-center justify-content-center flex-column"
          >
            <i
              class="pi pi-cloud-upload border-2 border-circle p-5 text-8xl text-400 border-400"
            />
            <p class="mt-4 mb-0">Drag and drop files to here to upload.</p>
          </div>
        </template>
        <template
          #header="{ chooseCallback, uploadCallback, clearCallback, files }"
        >
          <div
            class="flex flex-wrap justify-content-between align-items-center flex-1 gap-2"
          >
            <div class="flex gap-2">
              <Button
                @click="chooseCallback()"
                icon="pi pi-images"
                rounded
                outlined
              ></Button>
              <Button
                @click="uploadEvent(files)"
                icon="pi pi-cloud-upload"
                rounded
                outlined
                severity="success"
                :disabled="!files || files.length === 0"
              ></Button>
              <Button
                @click="clear(clearCallback)"
                icon="pi pi-times"
                rounded
                outlined
                severity="danger"
                :disabled="!files || files.length === 0"
              ></Button>
            </div>
          </div>
        </template>
        <template #content="{ removeUploadedFileCallback, removeFileCallback }">
          <div v-if="uploadedFiles.length > 0">
            <div class="">
              <div
                v-for="(file, index) of uploadedFiles"
                :key="file.fileName + file.type + file.size"
                class="card m-1 p-3 flex border-1 surface-border align-items-center justify-content-between flex-wrap"
              >
                <div class="flex align-items-center">
                  <Avatar
                    class="p-overlay-badge mr-3"
                    :image="file.objectURL"
                    size="xlarge"
                  />
                  <div class="flex flex-column">
                    <span>{{ file.fileName }}</span>
                    <Badge
                      value="Completed"
                      class="mt-1"
                      severity="success"
                      style="max-width: 6rem"
                    />
                  </div>
                </div>

                <Button
                  icon="pi pi-times"
                  @click="remove(index)"
                  rounded
                  text
                  severity="danger"
                />
              </div>
            </div>
          </div>
        </template>
      </FileUpload>
      <FileUpload
        v-else
        mode="basic"
        name="demo[]"
        @uploader="onAdvancedUpload($event)"
        customUpload
        :multiple="options.multiple ? true : false"
        :maxFileSize="options.size"
        :accept="options.accept"
        :disabled="isDisabled"
        class="table-outlined-button"
        auto
      >
        <template #empty>
          <div
            class="flex align-items-center justify-content-center flex-column"
          >
            <i
              class="pi pi-cloud-upload border-2 border-circle p-5 text-8xl text-400 border-400"
            />
            <p class="mt-4 mb-0">Drag and drop files to here to upload.</p>
          </div>
        </template>
        <template
          #header="{ chooseCallback, uploadCallback, clearCallback, files }"
        >
          <div
            class="flex flex-wrap justify-content-between align-items-center flex-1 gap-2"
          >
            <div class="flex gap-2">
              <Button
                @click="chooseCallback()"
                icon="pi pi-images"
                rounded
                outlined
              ></Button>
              <Button
                @click="uploadEvent(files)"
                icon="pi pi-cloud-upload"
                rounded
                outlined
                severity="success"
                :disabled="!files || files.length === 0"
              ></Button>
              <Button
                @click="clear(clearCallback)"
                icon="pi pi-times"
                rounded
                outlined
                severity="danger"
                :disabled="!files || files.length === 0"
              ></Button>
            </div>
          </div>
        </template>
        <template #content="{ removeUploadedFileCallback, removeFileCallback }">
          <div v-if="uploadedFiles.length > 0">
            <div class="">
              <div
                v-for="(file, index) of uploadedFiles"
                :key="file.fileName + file.type + file.size"
                class="card m-1 p-3 flex border-1 surface-border align-items-center justify-content-between flex-wrap"
              >
                <div class="flex align-items-center">
                  <Avatar
                    class="p-overlay-badge mr-3"
                    :image="file.objectURL"
                    size="xlarge"
                  />
                  <div class="flex flex-column">
                    <span>{{ file.fileName }}</span>
                    <Badge
                      value="Completed"
                      class="mt-1"
                      severity="success"
                      style="max-width: 6rem"
                    />
                  </div>
                </div>

                <Button
                  icon="pi pi-times"
                  @click="remove(index)"
                  rounded
                  text
                  severity="danger"
                />
              </div>
            </div>
          </div>
        </template>
      </FileUpload>
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { fileUpload, AifileUpload } from "@/api/api";
import { computed, reactive, ref, watch } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  placeholder: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  multiple: boolean | null;
  accept: string;
  size: number;
  isLinear: boolean | null;
  rules: { expression: string }[];
  events: any[];
}
export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      // type: FileList,
      default: [],
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "",
        tooltip: "",
        placeholder: "Sélectionnez un fichier",
        multiple: true,
        required: false,
        hidden: false,
        disabled: false,
        accept: "",
        size: 1000000,
        isLinear: false,
        rules: [],
        events: [],
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: ["update:modelValue", "update:options"],
  setup(props, { emit }) {
    const loading = ref(false);
    const files = computed({
      get() {
        // If modelValue is undefined or not an array, initialize it as an empty array
        return Array.isArray(props.modelValue)
          ? (props.modelValue as any)
          : ([] as any);
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });
    async function onAdvancedUpload(event: any) {
      emit("update:modelValue", event.files);

      for (let file of event.files) {
        const reader = new FileReader();
        reader.onload = async () => {
          return reader.result;
        };

        reader.onerror = (error) => {
          throw error;
        };
        await reader.readAsDataURL(file);
        reader.onload = () => {
          files.value.push({
            fileName: file.name,
            size: file.size,
            type: file.type,
            data: reader.result,
          });
          // emit("update:modelValue", files.value);
        };
      }
    }
    const selectedFile = ref("" as any);
    const uploadedFiles = computed({
      get() {
        // If modelValue is undefined or not an array, initialize it as an empty array
        return Array.isArray(props.modelValue)
          ? (props.modelValue as any)
          : ([] as any);
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });

    const uploadEvent = (event: any) => {
      emit("update:modelValue", event);
      uploadedFiles.value = event;
    };
    async function onFileChange(event: any) {
      loading.value = true;
      // If multiple files are not allowed, clear the files array before adding the new file
      if (!props.options.multiple) {
        files.value = [];
      }

      // Iterate through the selected files
      for (let file of event.target.files) {
        // Check if the file already exists in the array by comparing the name
        const existingFileIndex = files.value.findIndex(
          (existingFile: any) => existingFile.fileName === file.name
        );

        // If the file already exists, replace it (remove the old file)
        if (existingFileIndex !== -1) {
          files.value.splice(existingFileIndex, 1);
        }

        // Case where the file is related to Elise (handle upload via fileUpload API)
        if (props.options.useAILise) {
          const result = await AifileUpload(file);
          files.value.push(result);
        } else {
          const result = await fileUpload(file);
          files.value.push({
            guid: result,
            isLinked: false,
            fileName: file.name,
          });
        }
      }

      // Emit the updated files array
      emit("update:modelValue", files.value);
      // Reset the file input to allow re-selection of the same file
      event.target.value = "";

      loading.value = false;
    }

    // Function to get current value
    const getValue = () => {
      return props.modelValue;
    };

    // Fonction to get Files names
    const getFiles = () => {
      return files.value.map((file: any) => file.fileName);
    };

    const clear = (event: any) => {
      event();
      uploadedFiles.value = [];
    };
    const remove = (index: any) => {
      uploadedFiles.value.splice(index, 1);
    };
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

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

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
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

    // Watch for field validity and clear error if valid
    watch(
      () => files.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          // If required, not empty
          if (newValue && newValue.trim() !== "") {
            clearFieldError();
          }
        }
      }
    );

    return {
      isHidden,
      files,
      loading,
      selectedFile,
      isDisabled,
      uploadedFiles,
      onAdvancedUpload,
      onFileChange,
      disableField,
      enableField,
      hideField,
      showField,
      uploadEvent,
      clear,
      remove,
      computedRules,
      setFieldError,
      clearFieldError,
      errorState,
      getValue,
      getFiles,
      updateOptions,
    };
  },
};
</script>

<style lang="scss">
.table-outlined-button {
  background: transparent;
  border-color: var(--p-surface-color);
  color: var(--p-surface-color);
}
</style>
