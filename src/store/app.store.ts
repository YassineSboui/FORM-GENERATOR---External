import { defineStore } from "pinia";
import { ref } from "vue";
import {
  fetchObjects,
  fetchData,
  executeDatabaseConnection,
  executeApiCollection,
  fetchDataTest,
  eliseGetContacts,
  eliseXmlSearch,
  eliseGetFullThesaurus,
  eliseSearchThesaurus,
  eliseLevelThesaurus,
  eliseSendEmail,
  eliseAddContactPerson,
  eliseUpdateContactPerson,
  eliseAddContactOrganization,
  eliseUpdateContactOrganization,
  eliseGetFlowchartItems,
  eliseApplyTrackingPath,
  eliseMailByTemplate,
  businessGenerateDocuments,
  logger,
  postAiChatMessage,
  getServiceOrUserById,
} from "@/api/api";
import { usePVToastService } from "@/composable/usePVToastService";
import { set } from "lodash";
export const useAppStore = defineStore("AppStore", {
  state: () => {
    return {
      centerItems: ref([{} as any] as any),
      selectionMode: ref(false),
      AllIDs: ref([] as any[]),
      availableIDs: ref([] as any[]),
      tables: ref([] as any[]),
      isMenuOpen: ref(false),
      local: ref(false),
      requestResponse: ref(),
      databasesCollections: ref([] as any[]),
      apiCollections: ref([] as any[]),
      xmlSearch: ref([] as XmlSearchConfig[]),
      forms: ref([] as any[]),
      models: ref([] as any[]),
      currentNotice: ref(null as any),
      currentEliseDocument: ref(null as any),
      Fields: ref({} as any),
      eliseCollections: ref([] as any[]),
      tableVariables: ref([] as TableVariables[]),
      mailTemplate: ref([] as mailTemplate[]),
      currentTable: ref("" as string),
      currentTableInsertionType: ref("DIALOG" as string),
      formHasError: ref([] as keyBooleanValue[]),
      language: ref("frensh" as string),
      functions: ref([] as StoreFunction[]),
      AllServices: ref([] as any[]),
      AllEntities: ref([] as any[]),
      guid: ref(""),
      code: ref(""),
    };
  },
  actions: {
    toggleMenu() {
      this.isMenuOpen = !this.isMenuOpen;
    },
    closeMenu() {
      this.isMenuOpen = false;
    },
    activateSelectionMode() {
      this.selectionMode = true;
    },
    deactivateSelectionMode() {
      this.selectionMode = false;
    },
    addID(id: any) {
      const newItem = { code: id, name: id };
      this.availableIDs.push(newItem);
      this.AllIDs.push(newItem);
    },
    addIdAfterDelete(id: any) {
      const index = this.AllIDs.findIndex((item) => item.code == id);
      const index1 = this.availableIDs.findIndex((item) => {
        return item.code == id;
      });
      if (index != -1 && index1 == -1) {
        const newItem = { code: id, name: id };
        this.availableIDs.push(newItem);
      }
    },
    importIds(ids: any) {
      for (const item of ids) {
        const column_name = item.column_name;
        const newItem = { code: column_name, name: column_name };
        this.AllIDs.push(newItem);
      }
    },
    updateID(oldID: any, newID: any) {
      if (newID == oldID) return;
      const index = this.availableIDs.findIndex((item) => item.code == oldID);
      if (index != -1) {
        this.availableIDs[index].code = newID;
        this.availableIDs[index].name = newID;
      }
      const index1 = this.AllIDs.findIndex((item) => item.code == oldID);

      if (index1 != -1) {
        const ToCheck = this.AllIDs[index1].code;
        this.AllIDs[index1].code = newID;
        this.AllIDs[index1].name = newID;
        if (index == -1 && ToCheck != newID) {
          const newItem = { code: newID, name: newID };
          this.availableIDs.push(newItem);
        }
      }
    },
    deleteID(id: any) {
      const index = this.availableIDs.findIndex((item) => item.code == id);
      if (index != -1) {
        this.availableIDs.splice(index, 1);
      }
      const index1 = this.AllIDs.findIndex((item) => item.code == id);
      if (index1 != -1) {
        this.AllIDs.splice(index1, 1);
      }
    },
    delIdAfterSelect(id: any) {
      const index = this.availableIDs.findIndex((item) => item.code == id);
      if (index != -1) {
        this.availableIDs.splice(index, 1);
      }
    },
    clearAllIds() {
      this.AllIDs = [];
    },
    ClearAvailableIDs() {
      this.availableIDs = [];
    },
    resetAvailableIDS() {
      this.availableIDs = [...this.AllIDs];
    },
    checkIfParamExistsInCenterItemsOnlyOneTime(param: string) {
      let exists = 0;
      const columnNames = ["column1", "column2", "column3", "column4"];
      const checkRows = (rows: any) => {
        if (!rows) return;
        columnNames.forEach((columnName: any) => {
          (rows[columnName] || []).forEach((row: any) => {
            if (row.options?.name === param) {
              exists++;
            }
            if (row.zone === "ZR") {
              columnNames.forEach((columnName: any) => {
                (row.rows[columnName] || []).forEach((subRow: any) => {
                  checkRows(subRow.rows);
                });
              });
            }
            checkRows(row.rows);
          });
        });
      };
      const processItems = (item: any) => {
        if (item.zone === "ZS") {
          columnNames.forEach((columnName: any) => {
            (item.rows[columnName] || []).forEach((subRow: any) => {
              checkRows(subRow.rows);
            });
          });
        } else {
          checkRows(item.rows);
        }
      };

      if (this.centerItems[0]?.pages) {
        Object.values(this.centerItems[0].pages).forEach((page: any) => {
          page.forEach((item: any) => processItems(item));
        });
      } else {
        this.centerItems.forEach((item: any) => processItems(item));
      }
      return exists > 1;
    },

    checkIfZoneCodeExistsInCenterItems(zoneCode: string, zoneHeader: string) {
      let exists = 0;
      let headerExists = 0;
      const columnNames = ["column1", "column2", "column3", "column4"];
      const checkRows = (item: any) => {
        if (item?.zone) {
          if (item.header === zoneHeader) {
            headerExists++;
          }
          if (item.code === zoneCode) {
            exists++;
          }
          if (item.zone === "ZR") {
            columnNames.forEach((columnName) => {
              item.rows[columnName].forEach((row: any) => {
                checkRows(row.rows);
              });
            });
          } else if (item.zone === "ZS") {
            columnNames.forEach((columnName) => {
              item.rows[columnName].forEach((row: any) => {
                if (row.zone === "ZR") {
                  checkRows(row);
                } else {
                  checkRows(row.rows);
                }
              });
            });
          } else {
            checkRows(item.rows);
          }
        }
      };

      const processItems = (item: any) => {
        checkRows(item);
      };
      if (this.centerItems[0]?.pages) {
        Object.values(this.centerItems[0].pages).forEach((page: any) => {
          page.forEach((item: any) => processItems(item));
        });
      } else {
        this.centerItems.forEach((item: any) => processItems(item));
      }
      return exists > 1 || headerExists > 1 ? true : false;
    },
    checkIfSplitterZoneCodeExistsInCenterItems(
      zoneCode: string,
      zoneHeader: string
    ) {
      let exists = 0;
      let headerExists = 0;
      const checkCodeAndHeader = (item: any) => {
        if (item.code === zoneCode) {
          exists++;
        }
        if (item.header === zoneHeader) {
          headerExists++;
        }
      };
      if (this.centerItems[0]?.pages) {
        Object.values(this.centerItems[0].pages).forEach((page: any) => {
          page.forEach((item: any) => checkCodeAndHeader(item));
        });
      } else {
        this.centerItems.forEach((item: any) => checkCodeAndHeader(item));
      }
      return exists > 1 || headerExists > 1 ? true : false;
    },
    checkItemsInCenterItems() {
      let exists = 0;
      const columnNames = ["column1", "column2", "column3", "column4"];
      const checkRows = (rows: any) => {
        columnNames.forEach((columnName) => {
          rows[columnName].forEach((row: any) => {
            if (
              this.checkIfParamExistsInCenterItemsOnlyOneTime(row.options.name)
            ) {
              exists++;
            }
          });
        });
      };
      const processRows = (rows: any) => {
        for (let row of rows) {
          checkRows(row.rows);
        }
      };
      const processZone = (zone: any, rows: any) => {
        if (zone === "ZR") {
          processRows(rows.column1);
        } else if (zone === "ZS") {
          columnNames.forEach((columnName) => {
            rows[columnName].forEach((row: any) => {
              if (row.zone === "ZR") {
                this.checkIfZoneCodeExistsInCenterItems(row.code, row.header) &&
                  exists++;
                processRows(row.rows.column1);
              } else {
                checkRows(row.rows);
              }
            });
          });
        }
      };
      const processItems = (item: any) => {
        if (item.zone === "ZR" || item.zone === "ZS") {
          this.checkIfZoneCodeExistsInCenterItems(item.code, item.header) &&
            exists++;
          processZone(item.zone, item.rows);
        } else {
          checkRows(item.rows);
        }
      };
      if (this.centerItems[0]?.pages) {
        Object.values(this.centerItems[0].pages).forEach((page: any) => {
          page.forEach((item: any) => processItems(item));
        });
      } else {
        this.centerItems.forEach((item: any) => processItems(item));
      }

      return exists > 1;
    },
    nameList(): string[] {
      let nameList: string[] = [];
      const columnNames = ["column1", "column2", "column3", "column4"];
      const columnSplitterNames = ["column1", "column2"];

      const extractNames = (rows: any) => {
        if (!rows) return;
        columnNames.forEach((columnName: any) => {
          (rows[columnName] || []).forEach((row: any) => {
            if (row.options && row.options.name) {
              nameList.push(row.options.name);
            }
            if (row.zone === "ZR") {
              columnNames.forEach((columnName: any) => {
                (row.rows[columnName] || []).forEach((subRow: any) => {
                  extractNames(subRow.rows);
                });
              });
            }
            extractNames(row.rows);
          });
        });
      };
      const processItems = (item: any) => {
        if (item.zone === "ZS") {
          columnSplitterNames.forEach((columnName: any) => {
            (item.rows[columnName] || []).forEach((subRow: any) => {
              extractNames(subRow.rows);
            });
          });
        } else {
          extractNames(item.rows);
        }
      };
      if (this.centerItems[0]?.pages) {
        Object.values(this.centerItems[0].pages).forEach((page: any) => {
          page.forEach((item: any) => processItems(item));
        });
      } else {
        this.centerItems.forEach((item: any) => processItems(item));
      }

      return nameList;
    },
    formSectionsNames(): string[] {
      const sectionsNamesList: string[] = [];

      const extractCodes = (item: any) => {
        if (item.zone === "ZR" || item.zone === "ZS") {
          sectionsNamesList.push(item.code);
          if (item.zone === "ZS" && typeof item.rows === "object") {
            for (const column in item.rows) {
              const rows = item.rows[column];
              if (Array.isArray(rows)) {
                for (const rowItem of rows) {
                  if (rowItem.code) {
                    sectionsNamesList.push(rowItem.code);
                  }
                }
              }
            }
          }
        }
      };

      const isStepper = this.centerItems[0]?.pages ? true : false;
      if (isStepper) {
        for (const page of Object.values(this.centerItems[0].pages) as any[]) {
          page.forEach(extractCodes);
        }
      } else {
        this.centerItems.forEach(extractCodes);
      }

      return sectionsNamesList;
    },
    async fetchTables() {
      this.tables = [];
      try {
        const dat = await fetchObjects({ objectType: "TAB" }).then((data) => {
          this.tables = data;
        });
        return this.tables;
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
    },
    checkIfNeoTableComponentExists() {
      var exists = false;
      const isStepper = this.centerItems[0]?.pages ? true : false;
      if (isStepper) {
        for (let page in this.centerItems[0].pages) {
          for (let item of this.centerItems[0].pages[page]) {
            if (item.zone != "Z1000") {
              for (let i in item.rows) {
                for (let j in item.rows[i]) {
                  if (item.rows[i][j].component === "NeoTableComponent") {
                    exists = true;
                  }
                }
              }
            }
          }
        }
      } else {
        this.centerItems.forEach((item: any) => {
          if (item.zone != "Z1000") {
            for (const key in item.rows) {
              const row = item.rows[key];
              for (const j in row) {
                if (row[j].component === "NeoTableComponent") {
                  exists = true;
                }
              }
            }
          }
        });
      }
      return exists;
    },
    removeNeoTableComponent() {
      this.centerItems.forEach((item: any) => {
        if (item.zone != "Z1000") {
          for (let i in item.rows) {
            for (let j in item.rows[i]) {
              if (item.rows[i][j].component === "NeoTableComponent") {
                item.rows[i].splice(j, 1);
              }
            }
          }
        }
      });
    },
    setCenterItems(centerItems: any) {
      this.centerItems = centerItems;
      // setTimeout(() => {
      //   if( this.checkIfNeoTableComponentExists() ){
      //     this.removeNeoTableComponent();
      //   }
      // }, 1000);
    },

    async fetchDataTestStore(
      databaseConfigGuid: string,
      query: string,
      params: any,
      requestReturn: any
    ) {
      const res = await fetchDataTest({
        databaseConfigGuid,
        query,
        params,
        requestReturn,
      });
      this.requestResponse = res;
      return res;
    },
    async fetchDataStore(
      connectionString: string,
      request: string,
      params: any,
      requestReturn: any
    ) {
      const res = await fetchData({
        connectionString,
        request,
        params,
        requestReturn,
      });
      this.requestResponse = res;
      return res;
    },
    async executeDatabaseConnection(objectName: string, parameters: any) {
      const res = await executeDatabaseConnection(objectName, parameters);
      return res;
    },
    setDatabaseCollections(name: string, parameters: any) {
      this.databasesCollections.push({
        name: name,
        params: parameters,
      });
    },
    updateDatabaseCollections(
      oldName: string,
      newName: string,
      parameters: any
    ) {
      const index = this.databasesCollections.findIndex(
        (item) => item.name == oldName
      );
      if (index != -1) {
        this.databasesCollections[index].name = newName;
        this.databasesCollections[index].params = parameters;
      }
    },
    async executeApiCollection(objectName: string, parameters: any) {
      const res = await executeApiCollection(objectName, parameters);
      return res;
    },
    setApiCollections(name: string, parameters: any) {
      this.apiCollections.push({
        name: name,
        params: parameters,
      });
    },
    setXmlSearch(xmlSearch: XmlSearchConfig) {
      this.xmlSearch.push(xmlSearch);
    },
    updateApiCollections(oldName: string, newName: string, parameters: any) {
      const index = this.apiCollections.findIndex(
        (item) => item.name == oldName
      );
      if (index != -1) {
        this.apiCollections[index].name = newName;
        this.apiCollections[index].params = parameters;
      }
    },

    async initLoader() {
      try {
        var objects = await fetchObjects({ objectType: "CDQ" });
        this.databasesCollections = [];
        objects.forEach((obj: any) => {
          // this.setDatabaseCollections(obj.objectName);
          this.databasesCollections.push({
            name: obj.objectName,
            params: JSON.parse(obj.objectJson).objectConfig
              .CollectionQueryConfig.Params,
          });
        });
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
      try {
        var value = await fetchObjects({ objectType: "API" });
        this.apiCollections = [];
        value.forEach((obj: any) => {
          // this.setApiCollections(obj.objectName);
          this.apiCollections.push({
            name: obj.objectName,
            params: JSON.parse(obj.objectJson).objectConfig.externalApiConfig
              .parameters,
          });
        });
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
      try {
        var forms = await fetchObjects({ objectType: "FORM" });
        this.forms = [];
        forms.forEach((obj: any) => {
          this.setForms({
            name:
              JSON.parse(obj.objectJson).objectConfig.formConfig.formID +
              " - " +
              JSON.parse(obj.objectJson).objectConfig.formConfig.formName,
            code: obj.guid,
          });
        });
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
      try {
        var functionsResp = await fetchObjects({ objectType: "FNC" });
        this.functions = [];
        functionsResp.forEach((obj: any) => {
          this.functions.push({
            name: obj.objectName,
            parameters: JSON.parse(obj.objectJson).objectConfig.FunctionConfig
              .params,
            code: JSON.parse(obj.objectJson).objectConfig.FunctionConfig
              .function,
            mode: JSON.parse(obj.objectJson).objectConfig.FunctionConfig.mode,
            category: JSON.parse(obj.objectJson).objectConfig.FunctionConfig
              .category,
            description: JSON.parse(obj.objectJson).objectConfig.FunctionConfig
              .description,
          });
        });
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
    },
    setMailsTemplates(mail: mailTemplate) {
      this.mailTemplate.push(mail);
    },
    setForms(form: any) {
      this.forms.push(form);
    },
    updateForms(oldName: string, newName: string) {
      const index = this.forms.findIndex((item) => item.name == oldName);
      if (index != -1) {
        this.forms[index].name = newName;
      }
    },
    setModels(models: any) {
      this.models = models;
    },
    updateModels(oldName: string, newName: string) {
      const index = this.models.findIndex((item) => item.name == oldName);
      if (index != -1) {
        this.models[index].name = newName;
      }
    },
    setModelsByOne(model: any) {
      this.models.push(model);
    },
    setNotice(notice: any) {
      this.currentNotice = notice;
    },
    setEliseDocument(document: any) {
      this.currentEliseDocument = document;
    },
    /// ELISE

    async eliseGetContacts(payload: any) {
      const res = await eliseGetContacts(payload);
      return res;
    },
    async eliseAddContactPerson(payload: any) {
      const res = await eliseAddContactPerson(payload);

      const toast = usePVToastService();
      toast.add({
        severity: "success",
        summary: "Succès...!",
        detail: "Contact ajouté avec succès.",
        life: 3000,
      });
      return res;
    },
    async eliseUpdateContactPerson(payload: any) {
      const res = await eliseUpdateContactPerson(payload);
      const toast = usePVToastService();
      toast.add({
        severity: "success",
        summary: "Success...!",
        detail: "Contact modifié avec succès",
        life: 3000,
      });

      return res;
    },
    async eliseAddContactOrganization(payload: any) {
      const res = await eliseAddContactOrganization(payload);

      const toast = usePVToastService();
      toast.add({
        severity: "success",
        summary: "Success...!",
        detail: "Contact ajouté avec succès.",
        life: 3000,
      });
      return res;
    },
    async eliseUpdateContactOrganization(payload: any) {
      const res = await eliseUpdateContactOrganization(payload);

      const toast = usePVToastService();
      toast.add({
        severity: "success",
        summary: "Success...!",
        detail: "Contact modifié avec succès",
        life: 3000,
      });
      return res;
    },

    async eliseXmlSearch(payload: any) {
      const res = await eliseXmlSearch(payload);
      return res;
    },

    async eliseGetFullThesaurus(
      thesaurusName: string,
      termName: string | null
    ) {
      return await eliseGetFullThesaurus(thesaurusName, termName);
    },
    async eliseSearchThesaurus(thesaurusId: string, searchTerm: string) {
      return await eliseSearchThesaurus(thesaurusId, searchTerm);
    },
    async eliseLevelThesaurus(
      thesaurusId: string,
      parentTermId: string | null
    ) {
      return await eliseLevelThesaurus(thesaurusId, parentTermId);
    },
    async eliseSendEmail(payload: any) {
      return await eliseSendEmail(payload);
    },
    async eliseGetFlowchartItems(payload: any) {
      return await eliseGetFlowchartItems(payload);
    },
    async eliseApplyTrackingPath(payload: any) {
      await eliseApplyTrackingPath(payload);
    },

    async businessGenerateDocuments(payload: any) {
      await businessGenerateDocuments(payload);
    },

    setTableVariables(tableVariables: TableVariables[]) {
      console.log("this.tableVariables", this.tableVariables);
      console.log("tableVariables", tableVariables);

      tableVariables.forEach((item) => {
        const exists = this.tableVariables.some(
          (table) => table.key == item.key
        );

        if (!exists) {
          this.tableVariables.push(item);
        }
      });
    },

    async sendEliseMailByTemplate(template: string) {
      await eliseMailByTemplate(template);
    },
    setCurrentTable(table: string) {
      this.currentTable = table;
    },
    setCurrentTableInsertionType(insertionType: string) {
      console.log("insertionType", insertionType);
      this.currentTableInsertionType = insertionType;
    },
    resetCurrentTableInsertionType() {
      this.currentTableInsertionType = "DIALOG";
    },
    addFormHasError(name: string) {
      let nameExist = false;
      this.formHasError.forEach((item) => {
        if (item.key === name) {
          nameExist = true;
        }
      });
      if (!nameExist) {
        this.formHasError.push({ key: name, value: true });
      }
    },
    removeFormHasError(name: string) {
      this.formHasError = this.formHasError.filter((item) => item.key !== name);
    },
    setFunctions(fun: StoreFunction) {
      this.functions.push({
        name: fun.name,
        parameters: fun.parameters,
        code: fun.code,
        mode: fun.mode,
        category: fun.category,
        description: fun.description,
      });
    },
    updateFunctions(oldName: string, func: StoreFunction) {
      const index = this.functions.findIndex((item) => item.name == oldName);
      if (index != -1) {
        this.functions[index] = func;
      }
    },
    async executeFunction(parameters: any, functionCode: any) {
      const paramValues = parameters.reduce((acc: any, param: any) => {
        acc[param.key] = param.value;
        return acc;
      }, {});
      const paramNames = parameters.map((param: any) => param.key);
      const functionToExecute = new Function(...paramNames, functionCode);
      const paramValuesToPass = paramNames.map(
        (name: any) => paramValues[name]
      );
      const result = await functionToExecute(...paramValuesToPass);
      return result;
    },
    async storePostAiChatMessage(payload: any) {
      return await postAiChatMessage(payload);
    },
    async getObjectByLexicon(identifier: string | Record<string, boolean>) {
      console.log("identifier", identifier);

      // Extract the key if identifier is an object
      const id =
        typeof identifier === "object"
          ? Object.keys(identifier)[0]
          : identifier;

      // Call the getServiceOrUserById function to fetch the data by ID
      const item = await getServiceOrUserById(id, false);

      if (item && item.message !== "Item not found") {
        console.log("item", item);
        return item;
      } else {
        return { message: "Item not found" };
      }
    },
    setAllServices(services: any) {
      this.AllServices = services;
    },
    setAllEntities(entities: any) {
      this.AllEntities = entities;
    },
    getAllServices() {
      return this.AllServices;
    },
    getAllEntities() {
      return this.AllEntities;
    },
    async executeFunctionByName(
      functionName: string,
      childFields: { key: string; value: any }[]
    ) {
      try {
        const functionToExecute = this.functions.find(
          (item: any) => item.name === functionName
        );
        if (functionToExecute) {
          const params = functionToExecute.parameters;
          if (params.length !== childFields.length) {
            throw new Error(
              `Function expects ${params.length} parameters but ${childFields.length} were provided`
            );
          } else {
            const paramsValues = params.map((param: any) => {
              const matchingField = childFields.find(
                (child) => child.key === param.key
              );
              if (!matchingField) {
                throw new Error(`Missing value for parameter: ${param.key}`);
              }
              return { key: param.key, value: matchingField.value };
            });
            const resp = await this.executeFunction(
              paramsValues,
              functionToExecute.code
            );
            return resp;
          }
        } else {
          throw new Error(`Function with name ${functionName} not found`);
        }
      } catch (error) {
        console.error("Execution error:", error);
        throw error;
      }
    },
    setExternalAuth(code: string, guid: string) {
      this.code = code;
      this.guid = guid;
    },
  },
});
