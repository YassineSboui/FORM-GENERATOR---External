<template>
  <div class="Home-container">
    <!-- <Message icon="pi pi-info-circle" severity="info" closable
      >Salut ! NeoLedge vous souhaite la bienvenue sur son générateur de
      formulaires !</Message
    > -->
    <Message icon="pi pi-info-circle" severity="info"
      >Tableau de board
    </Message>
    <div class="basic-stats flex flex-wrap">
      <div class="cards" v-for="item in cardsDatas" :key="item.title">
        <statistic-card-component
          class="m-1"
          :title="item.title"
          :icon="item.icon"
          :counter="item.counter"
          :comment="item.comment"
          :commentCounter="item.commentCounter"
          :iconStyle="item.iconStyle"
          @click="() => router.push(item.route)"
        ></statistic-card-component>
      </div>
    </div>
    <!-- <div class="grid flex justify-content-start">
        <Chart
          type="bar"
          :data="chartData"
          :options="chartOptions2"
          class="col-8 h-30rem"
        />
        <div class="col-4 flex align-items-center justify-content-center">
          <Chart
            type="polarArea"
            :data="chartData"
            :options="chartOptions"
            class=" w-full md:w-20rem"
          />
        </div>
    </div> -->
  </div>
</template>

<script setup lang="ts">
import { ref, onBeforeMount } from "vue";
import StatisticCardComponent from "@/components/Dashboard/StatisticCardComponent.vue";
import { countAllObjects } from "@/api/api";
import { useRouter } from "vue-router";
import { logger } from "@/api/api";
const cardsDatas = ref([] as any);
const router = useRouter();
const totalCount = ref(null as any);
const countAll = async () => {
  try {
    const response = await countAllObjects();
    totalCount.value = response;
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};
const getCount = (objectType: string) => {
  return (
    totalCount.value?.find((item: any) => item.objectType === objectType)
      ?.count || 0
  );
};
onBeforeMount(async () => {
  await countAll();
  cardsDatas.value = [
    {
      title: "Environments",
      icon: "web",
      counter: getCount("ENV"),
      comment: "Description d'environments",
      commentCounter: "",
      iconStyle: { backgroundColor: "#C6E0E7", color: "#0A6E89" },
      route: "/environment",
    },
    {
      title: "Applications",
      icon: "web_asset",
      counter: getCount("APP"),
      comment: "Description d'application",
      commentCounter: "",
      iconStyle: { backgroundColor: "#EDC9DA", color: "#B1286A" },
      route: "/application",
    },
    {
      title: "Référentiels",
      icon: "book",
      counter: getCount("REF"),
      comment: "Description d'application",
      commentCounter: "",
      iconStyle: { backgroundColor: "#F9C7CD", color: "#E71D36" },
      route: "/referentiels",
    },
    {
      title: "Collection DB",
      icon: "table_rows",
      counter: getCount("CDB"),
      comment: "Description de collection DB",
      commentCounter: "",
      iconStyle: { backgroundColor: "#FBE8C8", color: "#EF9D11" },
      route: "/collectionDB",
    },
    {
      title: "Collection API",
      icon: "api",
      counter: getCount("API"),
      comment: "Description de collection API",
      commentCounter: "52+",
      iconStyle: { backgroundColor: "#FDF1BD", color: "#F6C900" },
      route: "/collectionAPI",
    },
    {
      title: "Tableaux",
      icon: "table_chart",
      counter: getCount("TAB"),
      comment: "Description de tableaux",
      commentCounter: "520",
      iconStyle: { backgroundColor: "#E0EFC2", color: "#84B91C" },
      route: "/dataTables",
    },
    {
      title: "Formulaires",
      icon: "summarize",
      counter: getCount("FORM"),
      comment: "Description de formulaires",
      commentCounter: "",
      iconStyle: { backgroundColor: "#B7E8FA", color: "#0D96C8" },
      route: "/formulaire",
    },
    {
      title: "Modèles",
      icon: "insert_drive_file",
      counter: getCount("MODEL"),
      comment: "Description de modèles",
      commentCounter: "",
      iconStyle: { backgroundColor: "#D9E8F5", color: "#2E6CB8" },
      route: "/models",
    },
    {
      title: "Recherche XML",
      icon: "search",
      counter: getCount("XMLSEARCH"),
      comment: "Description de recherche XML",
      commentCounter: "",
      iconStyle: { backgroundColor: "#EDC9DA", color: "#B1286A" },
      route: "/xmlSearch",
    },
    {
      title: "Modèles Mail",
      icon: "email",
      counter: getCount("MailTemplate"),
      comment: "Description de modèles mail",
      commentCounter: "",
      iconStyle: { backgroundColor: "#F9C7CD", color: "#E71D36" },
      route: "/mailTemplate",
    },
  ];
});

// onMounted(() => {
//   setTimeout(() => {
//     chartData.value = setChartData();
//     chartOptions.value = setChartOptions();
//     chartOptions2.value = setChartOptions2();
//   }, 1000);
// });

// const chartData = ref();
// const chartOptions = ref();
// const chartOptions2 = ref();

// const setChartData = () => {
//   const documentStyle = getComputedStyle(document.documentElement);

//   return {
//     datasets: [
//       {
//         data: [
//           countEnvironment.value,
//           countApplication.value,
//           countCollectionDB.value,
//           countRéférentiel.value,
//           countApi.value,
//           countTable.value,
//           countForm.value,
//           countModel.value,
//         ],
//         backgroundColor: [
//           documentStyle.getPropertyValue("--red-500"),
//           documentStyle.getPropertyValue("--green-500"),
//           documentStyle.getPropertyValue("--bluegray-500"),
//           documentStyle.getPropertyValue("--blue-500"),
//           documentStyle.getPropertyValue("--black-500"),
//           documentStyle.getPropertyValue("--cyan-500"),
//           documentStyle.getPropertyValue("--orange-500"),
//           documentStyle.getPropertyValue("--yellow-500"),
//         ],
//         label: "",
//       },
//     ],
//     labels: [
//       "Environments",
//       "Applications",
//       "Collection DB",
//       "Référentiel",
//       "Collection API",
//       "Tableaux",
//       "Formulaires",
//       "Modèles",

//     ],
//   };
// };
// const setChartOptions = () => {
//   const documentStyle = getComputedStyle(document.documentElement);
//   const textColor = documentStyle.getPropertyValue("--text-color");
//   const surfaceBorder = documentStyle.getPropertyValue("--surface-border");
//   return {
//     plugins: {
//       legend: {
//         labels: {
//           color: textColor,
//         },
//       },
//     },
//     scales: {
//       r: {
//         grid: {
//           color: surfaceBorder,
//         },
//       },
//     },
//   };
// };
// const setChartOptions2 = () => {
//   const documentStyle = getComputedStyle(document.documentElement);
//   const textColor = documentStyle.getPropertyValue("--text-color");
//   const textColorSecondary = documentStyle.getPropertyValue(
//     "--text-color-secondary"
//   );
//   const surfaceBorder = documentStyle.getPropertyValue("--surface-border");

//   return {
//     maintainAspectRatio: false,
//     aspectRatio: 0.8,
//     plugins: {
//       legend: {
//         labels: {
//           color: textColor,
//         },
//       },
//     },
//     scales: {
//       x: {
//         ticks: {
//           color: textColorSecondary,
//           font: {
//             weight: 500,
//           },
//         },
//         grid: {
//           display: false,
//           drawBorder: false,
//         },
//       },
//       y: {
//         ticks: {
//           color: textColorSecondary,
//         },
//         grid: {
//           color: surfaceBorder,
//           drawBorder: false,
//         },
//       },
//     },
//   };
// };
</script>
<style lang="scss">
.Home-container {
  // margin: 40px;
  //height: 100%;
  //min-height: 88vh;
  //max-height: 100vh;
  padding: 2px;
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-width: none;
  // Firefox
  -ms-overflow-style: none;
  // Chrome
  &::-webkit-scrollbar {
    width: 0px;
    background: transparent; /* make scrollbar transparent */
  }

  .header {
    border-left: 1px solid #ebebeb;
  }
  .basic-stats {
    width: 100%;
  }
  .p-message {
    height: 3.5rem;
    border-radius: 20px;
    .p-message-content {
      height: 100%;
      padding: 1rem;
      border-radius: 10px;

      // .p-message-text{
      //   font-size: 16px;
      //   font-weight: 600;
      //   color: #6b7280;
      // }
    }
    .p-message-content {
      background: #fafbfb !important;
    }
  }
}
.cards {
  transition: transform 0.2s;
}

.cards:hover {
  transform: scale(1.05);
}
</style>
