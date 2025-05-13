<template>
  <div v-if="index != -1">
    <div v-if="propsValue?.zone === 'Z1000'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid">
          <div :class="{ col: true, 'pr-2': isRTL }">
            <!-- &&
            propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="propsValue.rows.column1[j - 1]"
              :style="{
                maxWidth:
                  propsValue?.rows.column1[j - 1]?.component ==
                  'NeoTableComponent'
                    ? `${win.innerWidth * 0.948}px`
                    : 'none',
              }"
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
              }"
              @update:options="
                      (e : any) => {
                        propsValue.rows.column1[j - 1].options = e;
                      }
                    "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1100'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ col: true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
            (e : any) => {
            propsValue.rows.column1[j - 1].options = e;
            }
        "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column2[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column2[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1111'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ col: true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
            (e : any) => {
            propsValue.rows.column1[j - 1].options = e;
            }
        "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column2[j - 1].component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1].component != 'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1].component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column2[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column2[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column3[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column3[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pr-2': isRTL }">
            <!-- &&
          propsValue?.rows.column4[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column4[j - 1] &&
                propsValue?.rows.column4[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column4[j - 1].options.name"
              :is="propsValue.rows.column4[j - 1]?.component"
              :label="propsValue.rows.column4[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column4[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column4[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column4[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column4[j - 1].options.events,
                  propsValue.rows.column4[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column4[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column4[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column4[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column4[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column4[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column4[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column4[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column4[j - 1].options.content"
              :ref="propsValue.rows.column4[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1011'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-6': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
            (e : any) => {
            propsValue.rows.column1[j - 1].options = e;
            }
        "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-3': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column2[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column2[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-3">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column3[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column3[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1110'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-3': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
            (e : any) => {
            propsValue.rows.column1[j - 1].options = e;
            }
        "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-3': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column2[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column2[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-6">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column3[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column3[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z0111'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-4': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column1[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
            (e : any) => {
            propsValue.rows.column1[j - 1].options = e;
            }
        "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-4': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column2[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column2[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-4">
            <!-- &&
         propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="
                fields[sectionCode][index][
                  propsValue.rows.column3[j - 1].options.name
                ]
              "
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
            (e : any) => {
                propsValue.rows.column3[j - 1].options = e;
            }
            "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else>
    <div v-if="propsValue?.zone === 'Z1000'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid">
          <div
            :class="{ col: true, 'pr-2': isRTL }"
            :style="{
              maxWidth:
                propsValue?.rows.column1[j - 1]?.component ==
                'NeoTableComponent'
                  ? `${win.innerWidth * 0.948}px`
                  : 'none',
            }"
          >
            <component
              v-if="propsValue.rows.column1[j - 1]"
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
              }"
              @update:options="
                        (e : any) => {
                          propsValue.rows.column1[j - 1].options = e;
                        }
                      "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              :isRTL="isRTL"
              :showLabel="false"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
            ></component>
            <!-- @focus="handlefocus" -->
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1100'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ col: true, 'pl-2 pr-2 ': isRTL, 'pr-2': !isRTL }">
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1]?.options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
              (e : any) => {
              propsValue.rows.column1[j - 1].options = e;
              }
          "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="fields[propsValue.rows.column2[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column2[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1111'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ col: true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
              (e : any) => {
              propsValue.rows.column1[j - 1].options = e;
              }
          "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column2[j - 1].component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1].component != 'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1].component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="fields[propsValue.rows.column2[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column2[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="fields[propsValue.rows.column3[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column3[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ col: true, 'pr-2': isRTL }">
            <!-- &&
          propsValue?.rows.column4[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column4[j - 1] &&
                propsValue?.rows.column4[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column4[j - 1].options.name"
              :is="propsValue.rows.column4[j - 1]?.component"
              :label="propsValue.rows.column4[j - 1].options.label"
              v-model="fields[propsValue.rows.column4[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column4[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column4[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column4[j - 1].options.events,
                  propsValue.rows.column4[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column4[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column4[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column4[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column4[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column4[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column4[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column4[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column4[j - 1].options.content"
              :ref="propsValue.rows.column4[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1011'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-6': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
              (e : any) => {
              propsValue.rows.column1[j - 1].options = e;
              }
          "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-3': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="fields[propsValue.rows.column2[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column2[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-3">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="fields[propsValue.rows.column3[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column3[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z1110'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-3': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
              (e : any) => {
              propsValue.rows.column1[j - 1].options = e;
              }
          "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-3': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="fields[propsValue.rows.column2[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column2[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-6">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="fields[propsValue.rows.column3[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column3[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
    <div v-else-if="propsValue?.zone === 'Z0111'">
      <div
        v-for="j in Math.max(
          propsValue.rows.column1.length || 0,
          propsValue.rows.column2.length || 0,
          propsValue.rows.column3.length || 0,
          propsValue.rows.column4.length || 0
        )"
        :key="j"
      >
        <div class="grid mt-2">
          <div :class="{ 'col-4': true, 'pl-2 pr-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
         propsValue?.rows.column1[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column1[j - 1] &&
                propsValue?.rows.column1[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column1[j - 1].options.name"
              :is="propsValue.rows.column1[j - 1]?.component"
              :label="propsValue.rows.column1[j - 1].options.label"
              v-model="fields[propsValue.rows.column1[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column1[j - 1].options,
                id: 'EditoIdrZ1100C1' + (j + 1),
              }"
              @update:options="
              (e : any) => {
              propsValue.rows.column1[j - 1].options = e;
              }
          "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column1[j - 1].options.events,
                  propsValue.rows.column1[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column1[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column1[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column1[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column1[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column1[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column1[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column1[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column1[j - 1].options.content"
              :ref="propsValue.rows.column1[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div :class="{ 'col-4': true, 'pl-2': isRTL, 'pr-2': !isRTL }">
            <!-- &&
          propsValue?.rows.column2[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column2[j - 1] &&
                propsValue?.rows.column2[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column2[j - 1].options.name"
              :is="propsValue.rows.column2[j - 1]?.component"
              :label="propsValue.rows.column2[j - 1].options.label"
              v-model="fields[propsValue.rows.column2[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column2[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column2[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column2[j - 1].options.events,
                  propsValue.rows.column2[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column2[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column2[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column2[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column2[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column2[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column2[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column2[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column2[j - 1].options.content"
              :ref="propsValue.rows.column2[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
          <div class="col-4">
            <!-- &&
          propsValue?.rows.column3[j - 1]?.component != 'NeoVHtml' -->
            <component
              v-if="
                propsValue.rows.column3[j - 1] &&
                propsValue?.rows.column3[j - 1]?.component !=
                  'NeoTableComponent'
              "
              :ref="propsValue.rows.column3[j - 1].options.name"
              :is="propsValue.rows.column3[j - 1]?.component"
              :label="propsValue.rows.column3[j - 1].options.label"
              v-model="fields[propsValue.rows.column3[j - 1].options.name]"
              :options="{
                ...propsValue.rows.column3[j - 1].options,
                id: 'EditoIdrZ1100C2' + (j + 1),
              }"
              @update:options="
              (e : any) => {
                  propsValue.rows.column3[j - 1].options = e;
              }
              "
              @update:modelValue="
                handleInputChange(
                  propsValue.rows.column3[j - 1].options.events,
                  propsValue.rows.column3[j - 1].options.name
                )
              "
              @click="
                propsValue.rows.column3[j - 1].options.action
                  ? executeFun(
                      propsValue.rows.column3[j - 1].options.action[0].code
                    )
                  : null
              "
              @itemSelected="itemSelectedFunc"
              @searchItem="searchItemFunc"
              @focus="
                handleFocus(propsValue.rows.column3[j - 1].options.events)
              "
              @blur="handleBlur(propsValue.rows.column3[j - 1].options.events)"
              @mouseenter="
                handleMouseenter(propsValue.rows.column3[j - 1].options.events)
              "
              @mouseleave="
                handleMouseleave(propsValue.rows.column3[j - 1].options.events)
              "
              :isRTL="isRTL"
              :showLabel="false"
            ></component>
            <!-- <div
              v-if="propsValue?.rows.column3[j - 1]?.component == 'NeoVHtml'"
              v-html="propsValue.rows.column3[j - 1].options.content"
              :ref="propsValue.rows.column3[j - 1].options.name"
              class="vhtml"
            ></div> -->
            <div v-else></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, getCurrentInstance, computed } from "vue";
const win = window;

const props = defineProps({
  modelValue: {
    type: Object,
    required: true,
  },
  // fields: {
  //   type: Object,
  //   required: true,
  // },
  index: {
    type: Number,
    required: false,
    default: -1,
  },
  sectionCode: {
    type: String,
    required: false,
    default: "",
  },
  isRTL: {
    type: Boolean,
    required: false,
    default: false,
  },
});
const propsValue = computed(() => {
  return props.modelValue as any;
});

const fields = defineModel("fields") as any;

const emit = defineEmits([
  "update:modelValue",
  "handleInputChange",
  "executeFun",
  "AppRefs",
  "itemSelected",
  "searchItem",
  "focus",
  "blur",
  "mouseleave",
  "mouseenter",
]);

const app = getCurrentInstance() as any;
onMounted(() => {
  emit("AppRefs", app.refs);
});
const handleInputChange = (events: any, name: any) => {
  emit("handleInputChange", events);
};
const executeFun = (code: any) => {
  emit("executeFun", code);
};
const index = computed(() => {
  return props.index;
});
const sectionCode = computed(() => {
  return props.sectionCode;
});
const isRTL = computed(() => {
  return props.isRTL;
});
const itemSelectedFunc = (e: any) => {
  emit("itemSelected", e);
};
const searchItemFunc = (e: any) => {
  emit("searchItem", e);
};
const handleFocus = (e: any) => {
  emit("focus", e);
};
const handleBlur = (e: any) => {
  emit("blur", e);
};
const handleMouseenter = (e: any) => {
  emit("mouseenter", e);
};
const handleMouseleave = (e: any) => {
  emit("mouseleave", e);
};
</script>

<style lang="scss" scoped>
.grid {
  margin-bottom: 10px;
  margin-left: 0 !important;
  margin-right: 0 !important;
  margin-top: 10px !important;
}
.vhtml {
  max-width: 100%;
  word-break: break-word;
  overflow-wrap: break-word;
}
</style>
