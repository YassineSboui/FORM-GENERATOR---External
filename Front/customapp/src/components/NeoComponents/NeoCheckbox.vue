<template>
  <div class="neocheckbox">
    <div class="flex align-items-center input-container">
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
        ><div class="flex gap-2">
          <div>
            <Checkbox
              v-bind="field"
              :inputId="label"
              v-model="internalValue"
              :binary="true"
              @input="emitValue(internalValue)"
              :class="{
                'p-invalid': errorMessage,
              }"
              :disabled="isDisabled"
            ></Checkbox>
          </div>
          <div>
            <label class="label" v-if="label" :for="label">{{ label }}</label>
          </div>
        </div>
        <small class="p-error" id="text-error" v-if="errorMessage">{{
          errorMessage || "&nbsp;"
        }}</small>
      </Field>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, watch } from "vue";
import { ref } from "vue";

export default {
  props: {
    label: String,
    modelValue: {
      type: Boolean,
      default: "",
    },
    options: {
      type: Object,
      default: () => ({
        name: "",
        label: "",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        elements: [],
        rules: [],
        events: [],
      }),
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
  },
  setup(props, { emit }) {
    const internalValue = ref(props.modelValue);
    watch(
      () => props.modelValue,
      (value: boolean) => {
        internalValue.value = value;
      }
    );
    watch(
      () => internalValue.value,
      (value: boolean) => {
        emit("update:modelValue", value);
      }
    );
    const isDisabled = computed({
      get(): boolean {
        return props.options.disabled;
      },
      set(value: boolean) {
        emit("update:options", (props.options.disabled = value));
      },
    });
    function deactivateField() {
      isDisabled.value = true;
    }
    function emitValue(value: any) {
      emit("update:modelValue", value);
    }
    //Custom code
    // Compute the rules dynamically based on options.rules
    const computedRules = computed(() => {
      if (Array.isArray(props.options.rules)) {
        var expression = props.options.rules
          .map((item) => item.expression)
          .join("|");
        if (props.options.required) {
          expression = expression + "|required";
        }
        return expression;
      } else return "";
    });
    return {
      isDisabled,
      internalValue,
      deactivateField,
      emitValue,
      computedRules,
    };
  },
};
</script>
