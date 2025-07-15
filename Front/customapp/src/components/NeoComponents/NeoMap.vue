<template>
  <div class="neoMapField mb-3" v-show="!isHidden">
    <div class="label" v-if="options.label">
      <label class="label-container">
        {{
          language === "FR"
            ? options.label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : options.label
        }}
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
    <div
      class="input-container"
      :style="{
        maxWidth: 'fit-content',
        margin:
          options.position === 'center'
            ? '0 auto'
            : options.position === 'start'
            ? isRTL
              ? '0 0 0 auto'
              : '0 0 0 0'
            : options.position === 'end'
            ? isRTL
              ? '0 auto 0 0'
              : '0 0 0 auto'
            : '0',
      }"
    >
      <div
        class="map-wrapper"
        :class="{
          'disabled-wrapper': isDisabled,
          'readOnly-wrapper': readOnly,
        }"
        :style="{
          width: options.width + 'px',
          height: options.height + 'px',
        }"
      >
        <div class="map-container">
          <l-map
            ref="leafletMap"
            :key="mapKey"
            :center="position"
            :zoom="15"
            :style="{
              width: options.width + 'px',
              height: options.height + 'px',
            }"
            @ready="onMapReady"
          >
            <l-tile-layer
              url="http://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}"
              :subdomains="['mt0', 'mt1', 'mt2', 'mt3']"
              layer-type="overlay"
              :continuous-world="false"
              :no-wrap="true"
            />
            <!-- button -->
            <l-control position="bottomleft" v-if="!readOnly">
              <div v-if="!readOnly" class="map-controls">
                <Button
                  @click="resetLocation"
                  class="control-btn"
                  :disabled="isDisabled"
                >
                  <i class="pi pi-refresh"></i>
                </Button>
                <Button
                  @click="clearMarkers"
                  class="control-btn"
                  :disabled="isDisabled"
                >
                  <i class="pi pi-times"></i>
                </Button>
                <Button
                  @click="getCurrentLocation"
                  class="control-btn"
                  :disabled="isDisabled"
                >
                  <i class="pi pi-map-marker"></i>
                </Button>
                <Button
                  v-if="options.allowSearch"
                  @click="toggleSearch"
                  class="control-btn"
                  :disabled="isDisabled"
                >
                  <i class="pi pi-search"></i>
                </Button>
              </div>
            </l-control>
          </l-map>

          <div v-if="showSearch && !readOnly" class="search-container">
            <InputText
              v-model="searchQuery"
              :placeholder="$t('NeoMap.SearchPlaceholder')"
              @keyup.enter="searchLocation"
              class="search-input"
            />
            <Button @click="searchLocation" class="search-btn">
              <i class="pi pi-search"></i>
            </Button>
          </div>
        </div>
        <!-- <div v-if="selectedCoordinates" class="coordinates-display">
          <small>
            Coordinates: {{ selectedCoordinates.lat.toFixed(6) }},
            {{ selectedCoordinates.lng.toFixed(6) }}
          </small>
        </div> -->
        <small class="p-error" v-if="errorState.errorMessage">{{
          errorState.errorMessage
        }}</small>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import {
  ref,
  computed,
  reactive,
  watch,
  onMounted,
  onUnmounted,
  nextTick,
} from "vue";
// @ts-ignore
import { LMap, LTileLayer, LControl } from "@vue-leaflet/vue-leaflet";
import * as L from "leaflet";
import "leaflet/dist/leaflet.css";
import { useI18n } from "vue-i18n";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  rules: { expression: string }[];
  events: any[];
  width: number;
  height: number;
  position: string;
  defaultZoom: number;
  allowSearch: boolean;
  allowMultipleMarkers: boolean;
  defaultCenter: { lat: number; lng: number };
}

export default {
  components: {
    LMap,
    LTileLayer,
    LControl,
  },
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      type: [Object, Array, null] as any,
      default: () => null,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        width: 400,
        height: 300,
        position: "center",
        required: false,
        disabled: false,
        hidden: false,
        readonly: false,
        tooltip: "",
        defaultZoom: 10,
        allowSearch: true,
        allowMultipleMarkers: false,
        defaultCenter: { lat: 36.8065, lng: 10.1815 }, // Tunisia coordinates
      }),
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
  emits: ["update:modelValue", "update:options"],
  setup(props, { emit }) {
    // ...existing code...
    const mapKey = ref(Date.now());
    const leafletMap = ref<any>(null);
    const globalMap = ref<any>(null);
    // ...existing code...

    // Watch for map initialization and load markers if needed
    watch(globalMap, (newMap) => {
      if (newMap && internalValue.value) {
        loadMarkers();
      }
    });
    const { t } = useI18n();
    const internalValue = ref(props.modelValue);
    const localOptions = reactive({ ...props.options });
    const isDisabled = computed(() => localOptions.disabled);
    const readOnly = computed(() => localOptions.readonly);
    const isHidden = computed(() => localOptions.hidden);

    const markers = ref<any[]>([]);
    const selectedCoordinates = ref<{ lat: number; lng: number } | null>(null);
    const showSearch = ref(false);
    const searchQuery = ref("");
    const position = ref<[number, number]>([
      localOptions.defaultCenter?.lat || 36.8065,
      localOptions.defaultCenter?.lng || 10.1815,
    ]);
    const location = ref({
      latitude: localOptions.defaultCenter?.lat || 36.8065,
      longitude: localOptions.defaultCenter?.lng || 10.1815,
    });

    const errorState = reactive({
      errorMessage: "",
    });

    const isUpdating = ref(false);

    const onMapReady = (map: any) => {
      globalMap.value = map;

      // Load existing markers if any
      if (internalValue.value) {
        loadMarkers();
      }

      // Add click event listener
      map.on("click", (e: any) => {
        if (isDisabled.value || readOnly.value) return;
        const { lat, lng } = e.latlng;
        updateMapLocation(map, lat, lng);
      });
    };
    const updateMapLocation = (map: any, lat: number, lng: number) => {
      // Prevent recursive updates
      if (isUpdating.value) return;

      // lat and lng must be numbers and not Infinity
      if (lat === Infinity || lng === Infinity || isNaN(lat) || isNaN(lng)) {
        return;
      }

      isUpdating.value = true;

      try {
        const centerPoint = new L.LatLng(lat, lng);

        // Only clear markers if not allowing multiple
        if (!localOptions.allowMultipleMarkers) {
          map.eachLayer((layer: any) => {
            if (layer instanceof L.Marker) {
              map.removeLayer(layer);
            }
          });
        }

        // Add new marker
        map.addLayer(new L.Marker(centerPoint));
        map.setView(centerPoint);
        position.value = [lat, lng];
        location.value = { latitude: lat, longitude: lng };

        // Update component state
        const coordinates = { lat, lng };
        selectedCoordinates.value = coordinates;

        if (localOptions.allowMultipleMarkers) {
          const currentValue = Array.isArray(internalValue.value)
            ? internalValue.value
            : [];
          internalValue.value = [...currentValue, coordinates];
        } else {
          internalValue.value = coordinates;
        }

        emit("update:modelValue", internalValue.value);
      } finally {
        // Use nextTick to ensure the update is complete before allowing new updates
        nextTick(() => {
          isUpdating.value = false;
        });
      }
    };

    const handleMapClick = (event: any) => {
      if (isDisabled.value || readOnly.value) return;

      const { lat, lng } = event.latlng;
      updateMapLocation(globalMap.value, lat, lng);
    };

    const addMarker = (coordinates: { lat: number; lng: number }) => {
      if (
        !globalMap.value ||
        !coordinates ||
        typeof coordinates.lat !== "number" ||
        typeof coordinates.lng !== "number"
      ) {
        return;
      }

      try {
        const marker = new L.Marker([coordinates.lat, coordinates.lng]);
        globalMap.value.addLayer(marker);
        markers.value.push(marker);
        // Do NOT update model or emit events here
        return marker;
      } catch (error) {
        console.error("Error adding marker:", error);
      }
    };

    const loadMarkers = () => {
      if (!internalValue.value) return;

      try {
        // Remove only markers managed by this component
        markers.value.forEach((marker: any) => {
          if (globalMap.value && marker) {
            globalMap.value.removeLayer(marker);
          }
        });
        markers.value = [];

        if (Array.isArray(internalValue.value)) {
          let lastCoord: { lat: number; lng: number } | null = null as any;
          internalValue.value.forEach((coord: any) => {
            if (
              coord &&
              typeof coord.lat === "number" &&
              typeof coord.lng === "number"
            ) {
              addMarker(coord);
              lastCoord = coord;
            }
          });
          if (lastCoord && globalMap.value) {
            globalMap.value.setView(
              [lastCoord.lat, lastCoord.lng],
              18 // Use high zoom for accuracy
            );
            selectedCoordinates.value = lastCoord;
          } else {
            selectedCoordinates.value = null;
          }
        } else if (
          internalValue.value &&
          typeof internalValue.value.lat === "number" &&
          typeof internalValue.value.lng === "number"
        ) {
          addMarker(internalValue.value as any);
          selectedCoordinates.value = internalValue.value as any;
          if (globalMap.value) {
            globalMap.value.setView(
              [internalValue.value.lat, internalValue.value.lng],
              18 // Use high zoom for accuracy
            );
          }
        }
      } catch (error) {
        console.error("Error loading markers:", error);
      }
    };

    const clearMarkers = () => {
      try {
        // Remove only markers managed by this component
        markers.value.forEach((marker: any) => {
          if (globalMap.value && marker) {
            globalMap.value.removeLayer(marker);
          }
        });
        markers.value = [];

        selectedCoordinates.value = null;
        internalValue.value = localOptions.allowMultipleMarkers ? [] : null;

        emit("update:modelValue", internalValue.value);
      } catch (error) {
        console.error("Error clearing markers:", error);
      }
    };

    const getCurrentLocation = () => {
      if (!navigator.geolocation) {
        setFieldError("Geolocation is not supported by this browser.");
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          const coordinates = {
            lat: position.coords.latitude,
            lng: position.coords.longitude,
          };

          if (globalMap.value) {
            globalMap.value.setView([coordinates.lat, coordinates.lng], 18);
          }

          updateMapLocation(globalMap.value, coordinates.lat, coordinates.lng);
          clearFieldError();
        },
        (error) => {
          setFieldError("Unable to retrieve your location: " + error.message);
        },
        { enableHighAccuracy: true }
      );
    };

    const resetLocation = () => {
      if (globalMap.value) {
        // Clear existing markers
        globalMap.value.eachLayer((layer: any) => {
          if (layer instanceof L.Marker) {
            globalMap.value.removeLayer(layer);
          }
        });

        const defaultLat = localOptions.defaultCenter?.lat || 36.8065;
        const defaultLng = localOptions.defaultCenter?.lng || 10.1815;
        const defaultZoom = parseInt(localOptions.defaultZoom) || 10;

        globalMap.value.setView([defaultLat, defaultLng], defaultZoom);
        position.value = [defaultLat, defaultLng];
        location.value = { latitude: defaultLat, longitude: defaultLng };

        // Clear component state
        selectedCoordinates.value = null;
        internalValue.value = localOptions.allowMultipleMarkers ? [] : null;
        emit("update:modelValue", internalValue.value);
      }
    };

    const toggleSearch = () => {
      showSearch.value = !showSearch.value;
    };

    const searchLocation = async () => {
      if (!searchQuery.value.trim()) return;

      try {
        // Using OpenStreetMap Nominatim API for geocoding
        const response = await fetch(
          `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(
            searchQuery.value
          )}&limit=1`
        );
        const data = await response.json();

        if (data.length > 0) {
          const result = data[0];
          const coordinates = {
            lat: parseFloat(result.lat),
            lng: parseFloat(result.lon),
          };

          if (globalMap.value) {
            globalMap.value.setView([coordinates.lat, coordinates.lng], 15);
          }

          updateMapLocation(globalMap.value, coordinates.lat, coordinates.lng);
          clearFieldError();
        } else {
          setFieldError("Location not found");
        }
      } catch (error) {
        setFieldError("Error searching for location");
      }
    };

    // Component methods
    const getValue = () => {
      return internalValue.value;
    };

    const setValue = (value: any) => {
      let parsedValue = value;
      if (typeof value === "string") {
        try {
          parsedValue = JSON.parse(value);
        } catch (e) {
          parsedValue = value;
        }
      }

      internalValue.value = parsedValue;
      clearMarkers();
      if (parsedValue && globalMap.value) {
        if (Array.isArray(parsedValue)) {
          loadMarkers();
          // Select last marker for display
          if (
            parsedValue.length > 0 &&
            typeof parsedValue[parsedValue.length - 1]?.lat === "number" &&
            typeof parsedValue[parsedValue.length - 1]?.lng === "number"
          ) {
            selectedCoordinates.value = parsedValue[parsedValue.length - 1];
          } else {
            selectedCoordinates.value = null;
          }
        } else if (
          typeof parsedValue.lat === "number" &&
          typeof parsedValue.lng === "number"
        ) {
          updateMapLocation(globalMap.value, parsedValue.lat, parsedValue.lng);
          selectedCoordinates.value = parsedValue;
        } else {
          selectedCoordinates.value = null;
        }
      } else {
        selectedCoordinates.value = null;
      }
    };

    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

    const disableField = () => updateOptions({ disabled: true });
    const enableField = () => updateOptions({ disabled: false });
    const hideField = () => updateOptions({ hidden: true });
    const showField = () => updateOptions({ hidden: false });

    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    // Watchers
    watch(
      () => props.modelValue,
      (newValue) => {
        if (JSON.stringify(internalValue.value) !== JSON.stringify(newValue)) {
          internalValue.value = newValue;
          if (globalMap.value) {
            clearMarkers();
            if (newValue) {
              loadMarkers();
            }
          }
        }
      }
    );

    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
        // Force map re-render by updating key
        mapKey.value = Date.now();
        // Update position only if defaultCenter changed
        if (newOptions.defaultCenter) {
          const newLat = newOptions.defaultCenter.lat || 36.8065;
          const newLng = newOptions.defaultCenter.lng || 10.1815;
          if (position.value[0] !== newLat || position.value[1] !== newLng) {
            position.value = [newLat, newLng];
          }
        }
      },
      { deep: true }
    );

    // Lifecycle
    onMounted(async () => {
      await nextTick();
      // Map initialization is handled by @ready event
    });

    // onUnmounted(() => {
    //   if (globalMap.value) {
    //     globalMap.value.remove();
    //   }
    // });

    return {
      internalValue,
      localOptions,
      isDisabled,
      readOnly,
      isHidden,
      leafletMap,
      mapKey,
      position,
      selectedCoordinates,
      showSearch,
      searchQuery,
      errorState,
      t,
      onMapReady,
      clearMarkers,
      resetLocation,
      getCurrentLocation,
      toggleSearch,
      searchLocation,
      getValue,
      setValue,
      updateOptions,
      hideField,
      showField,
      disableField,
      enableField,
      setFieldError,
      clearFieldError,
    };
  },
};
</script>

<style scoped>
.neoMapField {
  width: 100%;

  .label {
    display: flex;
    flex-direction: row;

    .label-container {
      color: #165c77;
      min-width: 150px;
      align-items: center;
      padding-bottom: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
    }
  }

  .input-container {
    width: 100%;

    .map-wrapper {
      position: relative;

      .map-container {
        border: 1px solid #ccc;
        border-radius: 4px;
        background-color: #f5f5f5;
        z-index: 1;
      }

      /* Ensure Leaflet markers are visible */
      :deep(.leaflet-marker-icon) {
        z-index: 1000 !important;
      }

      :deep(.leaflet-marker-shadow) {
        z-index: 999 !important;
      }

      .map-controls {
        display: flex;
        gap: 8px;
        justify-content: center;
        align-items: center;
        white-space: nowrap;

        .control-btn {
          font-size: 12px;
          padding: 6px 12px;
          flex-shrink: 0;
        }
      }

      /* Center the bottom control */
      :deep(.leaflet-bottom.leaflet-left) {
        left: 50%;
        transform: translateX(-50%);
        width: auto;
      }

      .search-container {
        position: absolute;
        top: 16px;
        left: 50%;
        transform: translateX(-50%);
        display: flex;
        gap: 8px;
        background: rgba(255, 255, 255, 0.95);
        border-radius: 8px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
        padding: 8px 12px;
        z-index: 1200;
        align-items: center;

        .search-input {
          flex: 1;
          min-width: 180px;
        }

        .search-btn {
          padding: 6px 12px;
        }
      }
    }
  }

  .coordinates-display {
    margin-top: 8px;
    color: #666;
    font-size: 11px;
  }
}

.disabled-wrapper {
  pointer-events: none;
  opacity: 0.5;
}

.readOnly-wrapper {
  pointer-events: none;
}

.p-error {
  color: #e24c4c;
  font-size: 12px;
  margin-top: 4px;
}
</style>
