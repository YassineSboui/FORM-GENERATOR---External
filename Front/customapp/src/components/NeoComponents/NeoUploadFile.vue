<template>
  <div
    class="neoUploadFile"
    :class="{ 'mb-3': !isParentNeoTable }"
    v-show="!isHidden"
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
      <!-- rest of the template -->
      <div class="con" v-if="options.isLinear">
        <div class="basic flex flex-row flex-wrap justify-content-between">
          <label
            style="width: 100%"
            class="custom-file-input flex flex-row flex-wrap justify-content-between neo-upload-field"
          >
            <input
              type="file"
              @change="onFileChange"
              :multiple="options.multiple ? true : false"
              ref="fileInput"
              :accept="options.accept"
              style="display: none"
              :disabled="isDisabled || loading"
            />
            <div
              class="names flex flex-row neo-upload-files-container"
              v-if="files.length > 0"
            >
              <div
                class="file-name mr-2 flex flex-row flex-nowrap"
                v-for="file in files"
                :key="file.fileName"
              >
                <span style="display: flex; align-items: center">{{
                  file.fileName ||
                  file.name ||
                  file.elise?.fileName ||
                  file.ecsAi?.fileName
                }}</span>
                <Button
                  class="ml-1"
                  severity="danger"
                  text
                  style="height: 20; width: 20"
                  icon="pi pi-times-circle"
                  @click.stop.prevent="files.splice(files.indexOf(file), 1)"
                ></Button>
              </div>
            </div>

            <div
              v-else
              style="
                max-width: 50%;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
                max-height: 1.2rem;
              "
            >
              {{ options.placeholder || "Sélectionnez un fichier" }}
            </div>
            <div
              v-if="loading"
              class="loader-overlay flex align-items-center justify-content-between"
            >
              <i
                class="pi pi-spin pi-spinner text-xl"
                style="color: #0a6e89"
              ></i>
            </div>
            <span
              v-else
              class="flex justify-content-between align-items-center"
            >
              <i class="pi pi-upload mr-3"></i>
            </span>
          </label>
        </div>
      </div>
      <FileUpload
        v-else
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
  returnBase64: boolean | null;
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
        returnBase64: false,
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
      for (let file of event.files) {
        const reader = new FileReader();

        reader.onload = () => {
          const fileData: any = {
            fileName: file.name,
            size: file.size,
            type: file.type,
            data: reader.result,
          };

          // Add base64 data if returnBase64 option is true
          if (props.options.returnBase64 && reader.result) {
            const result = reader.result as string;
            fileData.base64 = result.split(",")[1];
          }

          files.value.push(fileData);
        };

        reader.onerror = (error) => {
          console.error("Error reading file:", error);
        };

        reader.readAsDataURL(file);
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

    // Helper function to convert file to base64
    const convertToBase64 = (file: File): Promise<string> => {
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
          const result = reader.result as string;
          const base64 = result.split(",")[1];
          resolve(base64);
        };
        reader.onerror = reject;
        reader.readAsDataURL(file);
      });
    };

    // Helper function to process a single file based on options
    const processFile = async (file: File) => {
      // If returnBase64 is true, only store base64 data without uploading
      if (props.options.returnBase64) {
        const base64Data = await convertToBase64(file);
        return {
          fileName: file.name,
          base64: base64Data,
        };
      }
      // Case where the file is related to Elise (handle both API calls)
      else if (props.options.relatedToElise && props.options.useAILise) {
        const [ecsAiResult, eliseResult] = await Promise.all([
          AifileUpload(file),
          fileUpload(file),
        ]);

        return {
          ecsAi: ecsAiResult,
          elise: {
            guid: eliseResult,
            isLinked: false,
            fileName: file.name,
          },
        };
      }
      // Case where only AI upload is needed
      else if (props.options.useAILise && !props.options.relatedToElise) {
        return await AifileUpload(file);
      }
      // Default case: standard file upload
      else {
        return {
          guid: await fileUpload(file),
          isLinked: false,
          fileName: file.name,
        };
      }
    };

    async function onFileChange(event: any) {
      loading.value = true;

      // If multiple files are not allowed, clear the files array
      if (!props.options.multiple) {
        files.value = [];
      }

      // Iterate through the selected files
      for (let file of event.target.files) {
        // Check if the file already exists and remove it
        const existingFileIndex = files.value.findIndex(
          (existingFile: any) => existingFile.fileName === file.name
        );

        if (existingFileIndex !== -1) {
          files.value.splice(existingFileIndex, 1);
        }

        // Process the file and add to array
        const fileData = await processFile(file);
        files.value.push(fileData);
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
.neoUploadFile {
  margin-bottom: 5px;
}
.table-outlined-button {
  background: transparent;
  border-color: var(--p-surface-color);
  color: var(--p-surface-color);
}

.neo-upload-field {
  border: 1px solid var(--p-inputtext-border-color);
  border-radius: 5px;
  padding: 0.75rem;
  background: var(--p-inputtext-background);
  transition: border-color 0.2s, box-shadow 0.2s;
  cursor: pointer;
  height: 100%; // Take full height of parent container
  max-height: 100%; // Don't exceed parent height
  min-height: auto; // Remove minimum height constraint
  overflow: hidden; // Hide overflow to maintain consistent height

  &:hover {
    border-color: var(--p-primary-color);
  }

  &:focus-within {
    border-color: var(--p-primary-color);
    box-shadow: 0 0 0 0.2rem var(--p-primary-color-20);
  }

  &:disabled {
    background: var(--p-inputtext-disabled-background);
    color: var(--p-inputtext-disabled-color);
    cursor: not-allowed;
  }
}

.neo-upload-files-container {
  overflow-x: auto;
  overflow-y: hidden;
  max-width: calc(100% - 40px); // Leave space for the upload icon
  scrollbar-width: thin;
  scrollbar-color: var(--p-primary-color) transparent;
  height: auto; // Auto height but constrained by parent
  max-height: 100%; // Don't exceed parent container
  align-items: center; // Center align items vertically

  // Custom scrollbar for WebKit browsers
  &::-webkit-scrollbar {
    height: 4px;
  }

  &::-webkit-scrollbar-track {
    background: transparent;
  }

  &::-webkit-scrollbar-thumb {
    background: var(--p-primary-color);
    border-radius: 2px;
  }

  &::-webkit-scrollbar-thumb:hover {
    background: var(--p-primary-color-dark);
  }

  .file-name {
    flex-shrink: 0; // Prevent files from shrinking
    white-space: nowrap; // Prevent text wrapping
    background: var(--p-surface-100);
    border: 1px solid var(--p-surface-border);
    border-radius: 15px; // 15px border radius as requested
    padding: 0.2rem 0.4rem; // Reduced padding to make it more compact
    margin-right: 0.5rem;
    display: flex;
    align-items: center;
    max-width: 200px; // Limit individual file name width
    height: 24px; // Fixed smaller height
    font-size: 0.875rem; // Slightly smaller font size

    span {
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: calc(100% - 25px); // Leave space for the delete button
      line-height: 1.2; // Compact line height
    }

    .p-button {
      height: 18px !important;
      min-width: 18px !important;
      font-size: 0.75rem !important;
    }
  }
}
</style>
