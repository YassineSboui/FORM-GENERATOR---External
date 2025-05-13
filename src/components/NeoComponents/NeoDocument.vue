<template>
  <div
    class="document"
    :id="chrono"
    @click="navigate"
    style="
      cursor: pointer;
      display: grid;
      grid-template-columns: auto 1fr auto;
      gap: 10px;
      align-items: center;
      width: 100%;
    "
  >
    <div
      style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis"
      class="docInfoItem"
    >
      <span class="input-container">{{ chrono }} -</span>
    </div>
    <div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis">
      <span>{{ title }}</span>
    </div>
    <div
      style="
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        justify-self: end;
        color: #2f87b2;
      "
      class="docInfoItem"
    >
      <span class="input-container">{{ prestation }}</span>
    </div>
  </div>
</template>

<script lang="ts">
import { useHttpRequest } from "@/store/httpRequest.store";
import { computed, defineComponent } from "vue";
export default defineComponent({
  props: {
    title: String,
    chrono: String,
    prestation: String,
    url: String,
  },
  setup(props) {
    const httpRequest = useHttpRequest();

    const navigate = () => {  
      const navUrl = `${httpRequest.eliseUrl}/GED/Elise/Home/document/overview?doc=${props.url}`
      window.open(navUrl, "_blank");
    };

    function convertXmlToJson(xml: string | undefined): any {
      if (!xml) return '';
      const parser = new DOMParser();
      const xmlDoc = parser.parseFromString(xml, "application/xml");
      const json = elementToJson(xmlDoc.documentElement);
      return json;
    }

    function elementToJson(element: Element): any {
      const obj: any = {};
      if (element.hasAttributes()) {
        for (let i = 0; i < element.attributes.length; i++) {
          const attr = element.attributes.item(i);
          obj[attr!.name] = attr!.value;
        }
      }

      element.childNodes.forEach((child) => {
        if (child.nodeType === Node.ELEMENT_NODE) {
          const childElement = child as Element;
          const childName = childElement.nodeName;
          if (!obj[childName]) {
            obj[childName] = elementToJson(childElement);
          } else {
            if (!Array.isArray(obj[childName])) {
              obj[childName] = [obj[childName]];
            }
            obj[childName].push(elementToJson(childElement));
          }
        } else if (
          child.nodeType === Node.TEXT_NODE &&
          child.nodeValue?.trim()
        ) {
          obj["value"] = child.nodeValue.trim();
        }
      });

      return obj;
    }

    const prestation = computed(() => {
      return convertXmlToJson(props.prestation)?.CP_GRC_PRESTATION?.TERME?.LABEL?.value;
    });

    return {
      navigate,
      prestation
    };
  },
});
</script>

<style class="scss">
.document {
  margin-bottom: 8px;
  border: 2px solid #e6e6e6;
  padding: 5px;
  height: 30px;
  border-radius: 4px;
  .docTitle {
    color: black;
    min-width: 150px;
    max-width: 50%;
    padding: 5px 0;
    display: flex;
    align-items: center;
    font-family: Trebuchet MS, sans-serif;
    font-size: 14px;
    overflow: hidden;
    width: 100%;
  }

  .docTitle span {
    /* Assuming the text is within a <span> inside .docTitle */
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    flex-grow: 1; /* Allows the span to fill the container for flexbox */
  }
  .docInfoItem {
    .input-container {
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
    }
    .material-icons {
      font-size: 17px;
    }
  }
}
</style>
