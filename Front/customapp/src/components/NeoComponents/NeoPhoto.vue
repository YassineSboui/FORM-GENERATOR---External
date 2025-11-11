<template>
  <div class="neoPhoto pt-5" v-show="!isHidden">
    <!-- Label Section -->
    <div class="label" v-if="label && showLabel">
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
          >*</span
        >
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

    <!-- Photo Preview Section -->
    <div
      class="input-container"
      :class="{ 'disabled-wrapper': isDisabled }"
      :style="{
        maxWidth: 'fit-content',
        margin:
          options.position === 'center'
            ? '0 auto'
            : options.position === 'start'
            ? isRTL
              ? '0 0 0 auto' // RTL: align start to the right
              : '0 0 0 0' // LTR: no margin for start
            : options.position === 'end'
            ? isRTL
              ? '0 auto 0 0' // RTL: align end to the left
              : '0 0 0 auto' // LTR: align end to the right
            : '0',
      }"
    >
      <Button
        v-if="showType == 'Gallery' && !readOnly"
        :label="$t('NeoPhotoProperties.gallery')"
        icon="pi pi-images"
        class="single-button"
        @click="openFileDialog"
        style="max-width: 100%"
      />
      <Button
        v-if="showType == 'Camera' && !readOnly"
        :label="$t('NeoPhotoProperties.camera')"
        icon="pi pi-camera"
        class="single-button"
        @click="openCameraDialog"
        style="max-width: 100%"
      />
      <ButtonGroup
        v-if="showType == 'Both' && !readOnly"
        style="max-width: 100%"
      >
        <Button
          :label="$t('NeoPhotoProperties.gallery')"
          icon="pi pi-images"
          class="gallery-button"
          @click="openFileDialog"
          style="max-width: 100%"
        />
        <Button
          :label="$t('NeoPhotoProperties.camera')"
          icon="pi pi-camera"
          @click="openCameraDialog"
          class="gallery-button"
          style="max-width: 100%"
        />
      </ButtonGroup>

      <input
        type="file"
        ref="fileInput"
        @change="handleFileChange"
        style="display: none"
        accept="image/*"
        capture="environment"
      />
      <!-- Photo Previews -->
      <div
        class="grid photo-preview-container pl-2 pr-2"
        v-if="internalValue.length || isUploading"
      >
        <div class="photo-preview-box">
          <!-- Loading indicator for photo upload -->
          <div v-if="isUploading" class="photo-preview loading-preview">
            <div class="loading-spinner">
              <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
            </div>
            <small class="loading-text">{{
              $t("NeoPhotoProperties.uploading") || "Uploading..."
            }}</small>
          </div>

          <!-- Existing photos -->
          <div
            class="photo-preview"
            v-for="(photo, index) in internalValue"
            :key="index"
            @click="openGallery(index)"
          >
            <img
              :src="
                'data:image/' +
                getFileExtension(photo.fileName) +
                ';base64,' +
                photo.fileB64
              "
              :alt="`Photo ${index + 1}`"
            />
            <Button
              v-if="!readOnly"
              class="ml-1"
              severity="danger"
              text
              style="height: 20px; width: 20px"
              icon="pi pi-times-circle"
              @click.stop="removePhoto(index)"
            ></Button>
          </div>
        </div>
      </div>
    </div>

    <!-- Error Message -->
    <small class="p-error" id="text-error" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "&nbsp;" }}
    </small>
    <!-- Dialog for Camera Capture -->
    <Dialog
      v-model:visible="visible"
      modal
      header="Capture Photo"
      :style="{ width: '30rem', zIndex: 9999 }"
      id="camera-dialog"
      :appendTo="'body'"
    >
      <div class="flex flex-col items-center">
        <div class="camera-container">
          <camera
            :resolution="{ width: options.width, height: options.height }"
            ref="camera"
            v-if="showCamera"
            autoplay
          >
            <div class="button-container-cancel">
              <Button
                icon="pi pi-times"
                type="button"
                class="camera-button"
                @click="() => (visible = false)"
              ></Button>
            </div>
            <div class="button-container">
              <Button
                icon="pi pi-camera"
                type="button"
                class="camera-button"
                @click="takeSnapshot"
              ></Button>
            </div>
            <div class="button-container-swap">
              <Button
                icon="pi pi-replay"
                type="button"
                class="camera-button"
                @click="swapCamera"
              ></Button>
            </div>
          </camera>
        </div>
      </div>
      <!-- Photo Preview Container -->
      <div
        class="photo-preview-container"
        v-if="internalValue.length || isUploading"
      >
        <div class="photo-preview-box">
          <!-- Loading indicator for photo upload -->
          <div v-if="isUploading" class="photo-preview loading-preview">
            <div class="loading-spinner">
              <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
            </div>
            <small class="loading-text">{{
              $t("NeoPhotoProperties.uploading") || "Uploading..."
            }}</small>
          </div>

          <!-- Existing photos -->
          <div
            class="photo-preview"
            v-for="(photo, index) in internalValue"
            :key="index"
          >
            <img
              :src="
                'data:image/' +
                getFileExtension(photo.fileName) +
                ';base64,' +
                photo.fileB64
              "
              :alt="`Photo ${index + 1}`"
            />
            <Button
              class="ml-1"
              severity="danger"
              text
              style="height: 20px; width: 20px"
              icon="pi pi-times-circle"
              @click.stop="removePhoto(index)"
            ></Button>
          </div>
        </div>
      </div>
    </Dialog>

    <!-- Dialog for Image Gallery -->
    <Dialog
      v-model:visible="galleryVisible"
      modal
      header="Image Gallery"
      :style="{ width: '40rem' }"
      id="galleria-dialog"
    >
      <Galleria
        :value="galleryImages"
        :responsiveOptions="responsiveOptions"
        :numVisible="5"
        containerStyle="max-width: 640px"
      >
        <template #item="slotProps">
          <img
            :src="
              'data:image/' +
              getFileExtension(slotProps.item.fileName) +
              ';base64,' +
              slotProps.item.fileB64
            "
            :alt="`Gallery Image`"
            style="width: 100%"
          />
        </template>
        <template #thumbnail="slotProps">
          <img
            :src="
              'data:image/' +
              getFileExtension(slotProps.item.fileName) +
              ';base64,' +
              slotProps.item.fileB64
            "
            :alt="`Gallery Thumbnail`"
            id="thumbnail"
            style="width: 75%"
          />
        </template>
      </Galleria>
    </Dialog>
  </div>
</template>
<script lang="ts">
import {
  defineComponent,
  ref,
  computed,
  reactive,
  watch,
  nextTick,
  onMounted,
} from "vue";
import { logger } from "@/api/api";
import { fileUpload, getFileByGuid } from "@/api/api";
import { useI18n } from "vue-i18n";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  disabled: boolean;
  required: boolean;
  readonly: boolean;
  hidden: boolean;
  showType: string;
  position: string;
  width: number;
  height: number;
  altText: string;
  returnBase64: boolean;
  rules: { expression: string }[];
  events: any[];
}

export default defineComponent({
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      default: [],
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "Image",
        tooltip: "",
        disabled: false,
        required: false,
        hidden: false,
        readonly: false,
        showType: "Les deux",
        rules: [],
        events: [],
        position: "center",
        width: 375, // Default width
        height: 375, // Default height
        altText: "", // Default alt text
        returnBase64: false, // Default returnBase64
      }),
    },
    showLabel: {
      type: Boolean,
      default: true,
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
  emits: ["update:options", "update:modelValue"],

  // Constant Variables
  setup(props, { emit }) {
    const { t } = useI18n();
    const camera = ref<any>();
    const galleryImages = ref<string[]>([]);
    const responsiveOptions = [
      {
        breakpoint: "400px",
        numVisible: 5,
      },
      {
        breakpoint: "400px",
        numVisible: 3,
      },
      {
        breakpoint: "400px",
        numVisible: 1,
      },
    ];
    const localOptions = reactive({ ...props.options });
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const readOnly = computed(() => localOptions.readonly);
    const showType = computed(() => props.options.showType) as any;
    const showCamera = ref(true);
    const visible = ref(false); // Track the dialog visibility
    const galleryVisible = ref(false);
    const fileInput = ref<HTMLInputElement | null>(null);
    const devices = ref<MediaDeviceInfo[]>([]);
    const currentDeviceIndex = ref(0);
    const isUploading = ref(false); // Add loading state for photo upload

    // Computed Variables
    const internalValue = computed({
      get(): any[] {
        return Array.isArray(props.modelValue) ? props.modelValue : [];
      },
      set(value: any) {
        emit("update:modelValue", value);
      },
    }) as any;

    // Function to get current value
    const getValue = () => {
      console.log("[NeoPhoto] getValue called, returning:", props.modelValue);
      return props.modelValue;
      //return internalValue.value;
    };

    function setValue(value: any) {
      // Check if the value is a string formatted as an array
      if (
        typeof value === "string" &&
        value.startsWith("[") &&
        value.endsWith("]")
      ) {
        try {
          // Parse the string into an array
          value = JSON.parse(value);
          internalValue.value = value;
        } catch (error) {
          console.error("Invalid array format:", error);
          logger.error(error);
          return;
        }
      }

      // Ensure it's an array after parsing
      if (Array.isArray(value)) {
        internalValue.value = value;
        galleryImages.value = value;
        // loadImages(); // Load images when the value is set
      } else {
        console.error("Value is not an array");
      }
    }

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

    // Function to take a snapshot
    const takeSnapshot = async () => {
      try {
        if (camera.value) {
          console.log(
            "[NeoPhoto] takeSnapshot - Initiating camera snapshot..."
          );
          isUploading.value = true; // Start loading

          const blob = await camera.value.snapshot({
            width: props.options.width,
            height: props.options.height,
          });

          const reader = new FileReader();
          reader.readAsDataURL(blob);
          reader.onload = async () => {
            if (reader.result) {
              const base64String = reader.result as string;

              // Convert blob to File object for fileUpload API
              const file = new File(
                [blob],
                `NeoForm_Photo_${new Date().getTime()}.png`,
                {
                  type: "image/png",
                }
              );

              try {
                const response = await fileUpload(file);
                if (response) {
                  const fileData: any = {
                    fileName: file.name,
                    fileB64: base64String.split(",")[1],
                    guid: response,
                  };

                  // Add base64 data if returnBase64 option is true
                  if (props.options.returnBase64) {
                    fileData.base64 = base64String.split(",")[1];
                  }

                  const newValue = [...internalValue.value, fileData];
                  internalValue.value = newValue; // This will trigger the setter and emit the event
                } else {
                  console.error(
                    "Failed to upload photo: API response was not successful."
                  );
                }
              } catch (uploadError) {
                console.error("Failed to upload photo:", uploadError);
                logger.error(uploadError);
              } finally {
                isUploading.value = false; // Stop loading
              }
            } else {
              console.error("Failed to read file as FileB64 string.");
              isUploading.value = false; // Stop loading
            }
          };

          setTimeout(() => URL.revokeObjectURL(blob), 1000); // Revoke URL after use
        }
      } catch (error) {
        setFieldError("Failed to capture photo. Please try again.");
        console.error("Snapshot error:", error);
        logger.error(error);
        isUploading.value = false; // Stop loading on error
      }
    };

    const getFileExtension = (fileName: any) => {
      return fileName.split(".").pop();
    };

    const handleFileChange = async (event: Event) => {
      const target = event.target as HTMLInputElement;
      const file = target.files?.[0];
      if (file) {
        isUploading.value = true; // Start loading

        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = async () => {
          if (reader.result) {
            const base64String = reader.result as string;

            // Call the fileUpload API
            try {
              const response = await fileUpload(file);
              if (response) {
                const fileData: any = {
                  fileName: file.name,
                  fileB64: base64String.split(",")[1],
                  guid: response,
                };

                // Add base64 data if returnBase64 option is true
                if (props.options.returnBase64) {
                  fileData.base64 = base64String.split(",")[1];
                }

                const newValue = [...internalValue.value, fileData];
                internalValue.value = newValue; // This will trigger the setter and emit the event
              } else {
                console.error(
                  "Failed to upload photo: API response was not successful."
                );
              }
            } catch (uploadError) {
              console.error("Failed to upload photo:", uploadError);
              logger.error(uploadError);
            } finally {
              isUploading.value = false; // Stop loading
            }
          } else {
            console.error("Failed to read file as FileB64 string.");
            isUploading.value = false; // Stop loading
          }
        };
      }
    };

    const swapCamera = async () => {
      // Ensure there are devices available
      if (devices.value.length > 0) {
        // Switch to the next camera in the list
        currentDeviceIndex.value =
          (currentDeviceIndex.value + 1) % devices.value.length;
        const device = devices.value[currentDeviceIndex.value];
        await camera.value?.changeCamera(device.deviceId);
      }
    };

    const removePhoto = (index: number) => {
      internalValue.value.splice(index, 1);
    };

    const openGallery = (index: number) => {
      galleryImages.value = internalValue.value;
      galleryVisible.value = true;
      nextTick(() => {
        (document.querySelector(".p-galleria") as any).activeIndex = index;
      });
    };

    // Watch for changes in localOptions.width and localOptions.height
    watch(
      () => [localOptions.width, localOptions.height],
      async () => {
        showCamera.value = false;
        await nextTick();
        showCamera.value = true;
      }
    );

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );

    // Watch for camera dialog visibility to load devices
    watch(visible, async (newVisible) => {
      if (newVisible) {
        await loadDevices();
      }
    });

    const errorState = reactive({
      errorMessage: "",
    });

    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    const openFileDialog = () => {
      fileInput.value?.click();
    };

    const openCameraDialog = async () => {
      console.log("Opening camera dialog, visible:", visible.value);

      // Check if we're on iOS and use fallback for better compatibility
      const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);

      if (isIOS) {
        // For iOS, try to use the native camera through file input with capture attribute
        const input = document.createElement("input");
        input.type = "file";
        input.accept = "image/*";
        input.capture = "environment"; // Use rear camera by default

        input.onchange = (event: any) => {
          const file = event.target.files[0];
          if (file) {
            handleFileChange({ target: { files: [file] } } as any);
          }
        };

        input.click();
        return;
      }

      // For Android and other devices, use the camera component
      try {
        // Request camera permissions first
        const stream = await navigator.mediaDevices.getUserMedia({
          video: {
            facingMode: "environment", // Start with rear camera
          },
        });

        // If we got permissions, stop the stream and show the camera dialog
        stream.getTracks().forEach((track) => track.stop());

        visible.value = true;
        console.log("After setting visible:", visible.value);
      } catch (error) {
        console.error("Camera permission denied or not available:", error);

        // Fallback to file input if camera access fails
        const input = document.createElement("input");
        input.type = "file";
        input.accept = "image/*";
        input.capture = "environment";

        input.onchange = (event: any) => {
          const file = event.target.files[0];
          if (file) {
            handleFileChange({ target: { files: [file] } } as any);
          }
        };

        input.click();
      }
    };

    const loadDevices = async () => {
      try {
        devices.value = (
          await navigator.mediaDevices.enumerateDevices()
        ).filter((device) => device.kind === "videoinput");
      } catch (error) {
        console.error("Error loading devices:", error);
        logger.error(error);
      }
    };

    // const loadImages = async () => {
    //   try {
    //     const images = await Promise.all(
    //       internalValue.value.map(async (photo: any) => {
    //         const response = await getFileByGuid(photo.Guid);
    //         return {
    //           FileB64: response.fileB64,
    //           Guid: photo.Guid,
    //           FileName: response.fileName,
    //         };
    //       })
    //     );
    //     internalValue.value = images;
    //     galleryImages.value = images;
    //   } catch (error) {
    //     console.error("Error loading images:", error);
    //     logger.error(error);
    //   }
    // };

    onMounted(() => {
      loadDevices(); // Load devices when the component is mounted
      // loadImages(); // Load images when the component is mounted
    });

    return {
      isDisabled,
      isHidden,
      internalValue,
      errorState,
      showCamera,
      camera,
      visible,
      galleryImages,
      galleryVisible,
      responsiveOptions,
      fileInput,
      showType,
      readOnly,
      isUploading,
      t,
      setFieldError,
      clearFieldError,
      takeSnapshot,
      getValue,
      setValue,
      updateOptions,
      disableField,
      enableField,
      hideField,
      showField,
      removePhoto,
      openGallery,
      swapCamera,
      loadDevices,
      getFileExtension,
      // loadImages,
      openFileDialog,
      openCameraDialog,
      handleFileChange,
    };
  },
});
</script>

<style lang="scss">
.neoPhoto {
  width: 100%;
  .label {
    display: flex;
    flex: 1;
    flex-direction: row;
    height: 20px;
    max-height: 20px;
    .label-container {
      color: #165c77;
      min-width: 150px;
      align-items: center;
      display: flex;
      padding-bottom: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
    }
  }
  .input-container {
    padding-bottom: 5px;
    display: flex;
    flex-direction: column; // Stack button and preview vertically
    align-items: center; // Center-align items
    width: 100%;
    height: 100%;
  }
}

.disabled-wrapper {
  pointer-events: none;
  opacity: 0.5;
}
.camera-container {
  overflow: hidden; /* Prevent overflow of the camera component */
  width: 100%; /* Ensure the container takes the full width */
  max-width: 100%; /* Avoid exceeding the dialog width */
}
.photo-preview-container {
  display: flex;
  gap: 10px;
  margin-top: 10px;
  flex-wrap: wrap;
}
.photo-preview {
  position: relative;
  width: 100px;
  height: 100px;
}

.loading-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background-color: #f5f5f5;
  border: 2px dashed #ccc;
  border-radius: 8px;
  color: #666;
}

.loading-spinner {
  margin-bottom: 8px;
  color: var(--p-primary-color);
}

.loading-text {
  font-size: 10px;
  text-align: center;
  color: #888;
}

body.dark .loading-preview {
  background-color: #333 !important;
  border-color: #555 !important;
  color: #ccc !important;
}

body.dark .loading-text {
  color: #aaa !important;
}
.photo-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}
.photo-preview .ml-1 {
  position: absolute;
  top: 5px;
  right: 5px;
}
.camera {
  position: relative;
}
.button-container-cancel {
  position: absolute;
  bottom: 10px;
  left: 15%;
  transform: translateX(-50%);
  zoom: 80%;
}
.button-container {
  position: absolute;
  bottom: 10px;
  left: 50%;
  transform: translateX(-50%);
}
.button-container-swap {
  position: absolute;
  bottom: 10px;
  left: 85%;
  transform: translateX(-50%);
  zoom: 80%;
}
.camera-button {
  margin-bottom: 10px !important;
  width: 50px !important ;
  height: 50px !important ;
  display: flex !important ;
  justify-content: center !important ;
  align-items: center !important ;
  background: rgba(255, 255, 255, 0.2) !important ;
  border-radius: 50% !important ;
  border: 2px solid #ffffff77 !important ; /* White border */
  backdrop-filter: blur(10px) !important ;
  cursor: pointer !important ;
  transition: background 0.3s !important ;
}
.camera-button i {
  color: #fff;
  font-size: 1.5em;
}
.camera-button:hover {
  background: rgba(255, 255, 255, 0.3); /* Slightly brighter on hover */
}
.photo-preview-box {
  display: flex; /* Align images in a row */
  overflow-x: auto; /* Enable horizontal scrolling */
  background-color: #f8f8f8; /* Light background color */
  border: 2px solid var(--p-primary-color); /* Border color */
  border-radius: 10px; /* Rounded corners */
  padding: 10px; /* Padding inside the box */
  margin-top: 20px; /* Space above the preview box */
  width: 100%; /* Full width of the dialog */
  max-height: 150px; /* Set a max height for the preview box */
  max-width: 80vw;
}
body.dark .photo-preview-box {
  background-color: #222 !important;
}
/* Media query for screens wider than 450px */
@media (min-width: 500px) {
  .photo-preview-box {
    max-width: 450px; /* Set max width if screen is wider than 450px */
  }
}

.photo-preview {
  position: relative; /* For positioning the remove button */
  margin-right: 10px; /* Space between Photos */
}

.photo-preview img {
  width: 100px; /* Set a fixed width for preview images */
  height: 100px; /* Set a fixed height for preview images */
  object-fit: cover; /* Ensure images cover the area without distortion */
  border-radius: 8px; /* Rounded corners for images */
}
@media (max-width: 768px) {
  #camera-dialog {
    width: 80% !important;
  }
  #galleria-dialog {
    width: 80% !important;
  }
}
.gallery-button {
  border-radius: 15px !important;
  margin: 2px;
}
.single-button {
  height: 40px;
}
.camera-button2 {
  border-radius: 0px 8px 8px 0px !important;
  height: 40px;
}
#video[data-v-74104ed5] {
  width: 100%;
  height: unset !important;
  border: 5px solid var(--p-primary-color);
  border-radius: 20px;
}

/* Ensure camera dialog appears on top */
#camera-dialog {
  z-index: 9999 !important;
}

#camera-dialog .p-dialog {
  z-index: 9999 !important;
}

#camera-dialog .p-dialog-mask {
  z-index: 9998 !important;
}
</style>
