import { useHttpRequest } from "../store/httpRequest.store";
import { useAppStore } from "@/store/app.store";
import axios from "axios";

// export type ObjectModel = {
//   id: number;
//   objectJson: string;
//   objectType: string;
//   objectName: string;
// };

const logToServer = async (level: any, message: any, BlocklyCall?: boolean) => {
  const httpRequest = useHttpRequest();
  try {
    if (!httpRequest.debugMode && !BlocklyCall) return;
    const apiUrl = `${httpRequest.apiUrl}/api/logs`; // Adjust endpoint as needed

    const logPayload = {
      level,
      message,
    };

    await axios.post(apiUrl, logPayload, {
      headers: { "Content-Type": "application/json" },
    });
  } catch (error) {
    console.error("[API] Failed to send log to server:", error);
    logger.error(error);
  }
};

export const uploadFile = async (base64: string, fileName?: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/File`;
    const payload = {
      bs64: base64 || "",
      fileName: fileName || "",
      courriersId: "",
      guid: "",
    };
    const response = await axios.post(apiUrl, payload);
    return response.data;
  } catch (error) {
    console.error("[API] uploadFile failed for file:", fileName, error);
    logger.error(error);
    throw error;
  }
};

export const getFileByGuid = async (guid: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}F/NeoForm/File/${guid}`;
    const response = await axios.get(apiUrl);
    return response.data;
  } catch (error) {
    console.error("[API] getFileByGuid failed for guid:", guid, error);
    logger.error(error);
    throw error;
  }
};

const log = (
  level: "info" | "warn" | "error" | "debug",
  message: any,
  BlocklyCall?: boolean
) => {
  const httpRequest = useHttpRequest();
  // make sure message is a string
  if (typeof message !== "string") {
    message = JSON.stringify(message);
  }

  // If server logging is enabled, send log to the server
  if (httpRequest.debugMode || BlocklyCall) {
    if (BlocklyCall) {
      console[level](`[Blockly] ${message}`);
    }
    logToServer(level, message, BlocklyCall);
  }
};

export const logger = {
  info: (message: any) => log("info", message),
  warn: (message: any) => log("warn", message),
  error: (message: any) => log("error", message),
  debug: (message: any) => log("debug", message),
};

export const logBlockly = {
  info: (message: any) => log("info", message, true),
  warn: (message: any) => log("warn", message, true),
  error: (message: any) => log("error", message, true),
  debug: (message: any) => log("debug", message, true),
};

export const getNeoFormVersion = async () => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Dashboard/version`;
    const { data }: { data: string } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] getNeoFormVersion failed:", error);
    logger.error(error);
    throw error;
  }
};
export const importObject = async (Object: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/import`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, Object);
    return data;
  } catch (error) {
    console.error("[API] importObject failed:", error);
    logger.error(error);
    throw error;
  }
};

export const fetchObjects = async ({ objectType }: { objectType: string }) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}local/objects`;
  const { data }: { data: ObjectModel[] } = await axios.get(apiUrl, {
    params: { objectType },
  });
  return data;
};

export const createObject = async ({ objectJson }: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, {
      objectJson,
    });
    return data;
  } catch (error) {
    console.error("[API] createObject failed:", error);
    logger.error(error);
    throw error;
  }
};
export const updateObject = async ({ id, objectJson }: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/${id}`;
    const { data }: { data: ObjectModel } = await axios.put(apiUrl, {
      objectJson,
    });
    return data;
  } catch (error) {
    console.error("[API] updateObject failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};

export const deleteObject = async (id: number) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/${id}`;
    const response = await axios.delete(apiUrl);
    return response.data;
  } catch (error) {
    console.error("[API] deleteObject failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const saveNotice = async ({
  objectId,
  objectGuid,
  noticeJson,
  newDoc,
  disableNotice = false,
}: {
  objectId: number;
  objectGuid: string;
  noticeJson: any;
  newDoc: boolean;
  disableNotice?: boolean;
}) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Notice/ExtSave`;
    const { data }: { data: EliseDocument } = await axios.post(apiUrl, {
      objectId,
      objectGuid,
      noticeJson,
      newDoc,
      disableNotice,
    });
    return data;
  } catch (error) {
    httpRequest.setLoading(false);
    console.error(
      "[API] saveNotice failed for objectId:",
      objectId,
      "objectGuid:",
      objectGuid,
      error
    );
    logger.error(error);
    throw error;
  }
};
export const updateNotice = async ({
  objectId,
  noticeId,
  noticeJson,
  newDoc,
  disableNotice = false,
}: {
  objectId: number;
  noticeId: number;
  noticeJson: any;
  newDoc: boolean;
  disableNotice?: boolean;
}) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice/Update`;
    const { data }: { data: { url: string } } = await axios.post(apiUrl, {
      objectId,
      noticeId,
      noticeJson,
      newDoc,
      disableNotice,
    });
    return data;
  } catch (error) {
    httpRequest.setLoading(false);
    console.error(
      "[API] updateNotice failed for noticeId:",
      noticeId,
      "objectId:",
      objectId,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const fetchNotice = async (noticeType: NoticeType) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice`;
    const { data }: { data: any } = await axios.get(apiUrl, {
      params: { noticeType },
    });

    return data;
  } catch (error) {
    console.error(
      "[API] fetchNotice failed for noticeType:",
      noticeType,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const fetchMetadata = async (formGuid: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice/form/metadata/${formGuid}`;
    const { data }: { data: { eliseDocument: any; metadatas: any } } =
      await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] fetchMetadata failed for formGuid:", formGuid, error);
    logger.error(error);
    throw error;
  }
};

export const fetchOneObject = async (id: string) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/objects/guid/${id}`;
    const { data }: { data: ObjectModel } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    httpRequest.setLoading(false);
    console.error("[API] fetchOneObject failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const fetchOneTable = async (id: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/ref/${id}`;
    const { data }: { data: ObjectModel } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] fetchOneTable failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const getConnexion = async (connection: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/ExternalDatabase/test`;
    const response = await axios.post(apiUrl, connection);
    return response.data;
  } catch (error) {
    console.error("[API] getConnexion failed:", error);
    logger.error(error);
    throw error;
  }
};
export const fetchDataTest = async ({
  databaseConfigGuid,
  query,
  params,
  requestReturn,
}: {
  databaseConfigGuid: string;
  query: string;
  params: any;
  requestReturn: any;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/ExternalDatabase/executetest`;
    const response = await axios.post(apiUrl, {
      databaseConfigGuid,
      query,
      params,
      requestReturn,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] fetchDataTest failed for databaseConfigGuid:",
      databaseConfigGuid,
      error
    );
    logger.error(error);
    throw error;
  }
};
export const fetchData = async ({
  connectionString,
  request,
  params,
  requestReturn,
}: {
  connectionString: string;
  request: string;
  params: any;
  requestReturn: any;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/ExternalDatabase/execute`;
    const response = await axios.post(apiUrl, {
      connectionString,
      request,
      params,
      requestReturn,
    });
    return response.data;
  } catch (error) {
    console.error("[API] fetchData failed for request:", request, error);
    logger.error(error);
    throw error;
  }
};
export const getNoticesByCourriesId = async (id: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice/${id}`;
    // const { data }: { data: any } = await axios.get(apiUrl);
    const response = await axios.get(apiUrl);
    return response.data;
  } catch (error) {
    console.error("[API] getNoticesByCourriesId failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const getNoticesByFormId = async (id: number) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice/form/${id}`;
    // const { data }: { data: any } = await axios.get(apiUrl);
    const response = await axios.get(apiUrl);
    console.log(
      "[API] getNoticesByFormId response for formId:",
      id,
      response.data
    );
    return response.data;
  } catch (error) {
    console.error("[API] getNoticesByFormId failed for formId:", id, error);
    logger.error(error);
    throw error;
  }
};
export const getNoticesCountByFormId = async (id: number) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Notice/form/${id}/count`;
    // const { data }: { data: any } = await axios.get(apiUrl);
    const response = await axios.get(apiUrl);
    return response.data;
  } catch (error) {
    console.error(
      "[API] getNoticesCountByFormId failed for formId:",
      id,
      error
    );
    logger.error(error);
    throw error;
  }
};
export const createData = async ({ dataJson }: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, {
      dataJson,
    });
    return data;
  } catch (error) {
    console.error("[API] createData failed:", error);
    logger.error(error);
    throw error;
  }
};

export const importData = async ({
  dataJsonArray,
}: {
  dataJsonArray: any[];
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas/import`; // Updated endpoint
    const { data }: { data: ObjectModel[] } = await axios.post(
      apiUrl,
      dataJsonArray // Send the array in the request body
    );
    return data;
  } catch (error) {
    console.error(
      "[API] importData failed, array length:",
      dataJsonArray?.length,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const fetchDataByObjectID = async (id: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas/form/${id}`;
    const { data }: { data: ObjectModel } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] fetchDataByObjectID failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};

export const fetchDataByTableGuid = async (guid: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas/table/${guid}`;
    const { data }: { data: ObjectModel } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] fetchDataByTableGuid failed for guid:", guid, error);
    logger.error(error);
    throw error;
  }
};

export const fetchObjectByGuid = async (guid: string) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}local/objects/guid/${guid}`;
    const { data }: { data: ObjectModel } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] fetchObjectByGuid failed for guid:", guid, error);
    logger.error(error);
    throw error;
  }
};
export const updateData = async ({ id, dataJson }: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas/${id}`;
    const { data }: { data: ObjectModel } = await axios.put(apiUrl, {
      dataJson,
    });
    return data;
  } catch (error) {
    console.error("[API] updateData failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const deleteData = async (id: number) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Datas/${id}`;
    const response = await axios.delete(apiUrl);
    return response.data;
  } catch (error) {
    console.error("[API] deleteData failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const fileUpload = async (file: any) => {
  try {
    const formData = new FormData();
    formData.append("file", file);
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/Document/UploadFile`;
    const response = await axios.post(apiUrl, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  } catch (error) {
    console.error("[API] fileUpload failed for file:", file?.name, error);
    logger.error(error);
    throw error;
  }
};
export const AifileUpload = async (file: any) => {
  try {
    const formData = new FormData();
    formData.append("file", file);
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/api/Ecs/file`;
    const response = await axios.post(apiUrl, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  } catch (error) {
    console.error("[API] AifileUpload failed for file:", file?.name, error);
    logger.error(error);
    throw error;
  }
};
export const fetchActionConfigurations = async () => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ActionConfiguration`;
    const response = await axios.get(apiUrl);
    return response.data;
  } catch (error) {
    console.error("[API] fetchActionConfigurations failed:", error);
    logger.error(error);
    throw error;
  }
};
export const fetchActionConfigurationById = async (id: number) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ActionConfiguration/${id}`;
    const response = await axios.get(apiUrl);
    return response.data;
  } catch (error) {
    console.error(
      "[API] fetchActionConfigurationById failed for id:",
      id,
      error
    );
    logger.error(error);
    throw error;
  }
};
export const createActionConfiguration = async (obj: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ActionConfiguration`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, obj);
    return data;
  } catch (error) {
    console.error("[API] createActionConfiguration failed:", error);
    logger.error(error);
    throw error;
  }
};
export const updateActionConfiguration = async (id: any, obj: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ActionConfiguration/${id}`;
    const { data }: { data: ObjectModel } = await axios.put(apiUrl, obj);
    return data;
  } catch (error) {
    console.error("[API] updateActionConfiguration failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const deleteActionConfiguration = async (id: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ActionConfiguration/${id}`;
    const { data }: { data: ObjectModel } = await axios.delete(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] deleteActionConfiguration failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};
export const executeAPI = async (dataJson: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ExternalApi/Execute`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, dataJson);
    return data;
  } catch (error) {
    console.error("[API] executeAPI failed:", error);
    logger.error(error);
    throw error;
  }
};
export const executeAPIBeforeSave = async (dataJson: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/ExternalApi/ExecuteBeforeSave`;
    const { data }: { data: ObjectModel } = await axios.post(apiUrl, dataJson);
    const response = await axios.post(apiUrl, dataJson);
    console.log("[API] executeAPIBeforeSave response:", response);
    return data;
  } catch (error) {
    console.error("[API] executeAPIBeforeSave failed:", error);
    logger.error(error);
    throw error;
  }
};
export const executeApiCollection = async (objectName: string, params: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}local/ExternalSource/ExecuteApiByObjectName`;
    const { data }: { data: string } = await axios.post(apiUrl, {
      objectName,
      params,
    });
    return data;
  } catch (error) {
    console.error(
      "[API] executeApiCollection failed for objectName:",
      objectName,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const countObject = async (id: any) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Dashboard`;
    const { data }: { data: number } = await axios.get(apiUrl, {
      params: { id },
    });
    return data;
  } catch (error) {
    console.error("[API] countObject failed for id:", id, error);
    logger.error(error);
    throw error;
  }
};

export const countAllObjects = async () => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/NeoForm/Dashboard/All`;
    const { data }: { data: any } = await axios.get(apiUrl);
    return data;
  } catch (error) {
    console.error("[API] countAllObjects failed:", error);
    logger.error(error);
    throw error;
  }
};
export const executeDatabaseConnection = async (
  objectName: string,
  parameters: any
) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}local/ExternalSource/ExecuteDbqByObjectName`;
    // const { data }: { data: string } = await axios.post(apiUrl, { objectName, parameters });
    //  the objectName will be passed as a query parameter and the parameters will be passed as a request body
    const { data }: { data: string } = await axios.post(apiUrl, {
      objectName,
      params: parameters,
    });
    return data;
  } catch (error) {
    console.error(
      "[API] executeDatabaseConnection failed for objectName:",
      objectName,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const generateXMLModel = async (formJson: any) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}/DocumentModel/ConvertJsonToXml`;
    const { data }: { data: string } = await axios.post(apiUrl, formJson);
    httpRequest.setLoading(false);

    return data;
  } catch (error) {
    httpRequest.setLoading(false);
    console.error("[API] generateXMLModel failed:", error);
    logger.error(error);
    throw error;
  }
};

export const generateModel = async (formJson: any) => {
  const httpRequest = useHttpRequest();
  try {
    // const apiUrl = `${httpRequest.externalUrl}/DocumentModel/ConvertJsonToXml`;
    const apiUrl = `${httpRequest.externalUrl}/DocumentModel/GenerateModelFile`;
    const response = await axios.post(apiUrl, formJson, {
      responseType: "blob",
    });
    const href = URL.createObjectURL(response.data);
    // create "a" HTML element with href to file & click
    const link = document.createElement("a");
    link.href = href;
    link.setAttribute("download", "generated_doc.docx"); //or any other extension
    document.body.appendChild(link);
    link.click();

    // clean up "a" element & remove ObjectURL
    document.body.removeChild(link);
    URL.revokeObjectURL(href);
    httpRequest.setLoading(false);
    return null;
  } catch (error) {
    httpRequest.setLoading(false);

    console.error("[API] generateModel failed:", error);
    logger.error(error);
    throw error;
  }
};
export const uploadModelFile = (file: any) => {
  try {
    const formData = new FormData();
    formData.append("file", file);
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/DocumentModel/UploadModelFile`;
    const response = axios.post(apiUrl, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response;
  } catch (error) {
    console.error("[API] uploadModelFile failed for file:", file?.name, error);
    logger.error(error);
    throw error;
  }
};

export const searchFlowChart = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/Flowchart/search`;
  const { data }: { data: ObjectModel[] } = await axios.post(apiUrl, payload);
  return data;
};

export const fetchFlowChart = async () => {
  const appStore = useAppStore();
  if (appStore.AllServices.length > 0) {
    return appStore.AllServices;
  } else {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/api/Flowchart/service`;
    const { data }: { data: ObjectModel[] } = await axios.get(apiUrl);
    // var localData = NodeService.getTreeNodesS() as any;
    appStore.setAllServices(data);
    // return localData;
    return data;
  }
};
export const fetchFlowChartUsers = async ({
  serviceId,
}: {
  serviceId: string;
}) => {
  const httpRequest = useHttpRequest();

  const apiUrl = `${httpRequest.externalUrl}/api/Flowchart/user`;
  const { data }: { data: ObjectModel[] } = await axios.get(apiUrl, {
    params: { serviceId },
  });
  return data;
  // if (serviceId == "LEXICON_00000000") {
  //   console.log("serviceId", serviceId);
  //   var localData = NodeService.getTreeNodesU() as any;
  //   return localData;
  // }
  // return [];
};

// Fetching the combined services and users from your backend
export const fetchFlowChartWithUsers = async () => {
  const appStore = useAppStore();
  if (appStore.AllEntities.length > 0) {
    return appStore.AllEntities;
  } else {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.externalUrl}/api/Flowchart/serviceAndUserList`; // Assuming you have this new endpoint
    const { data }: { data: ObjectModel[] } = await axios.get(apiUrl);
    appStore.setAllEntities(data);
    return data;
  }
};

export const getServiceOrUserById = async (
  id: string,
  returnLabel: boolean
) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/Flowchart/serviceAndUserById/${id}`;

  try {
    const { data } = await axios.get(apiUrl, {
      params: {
        returnLabel,
      },
    });
    if (data && data.message !== "Item not found") {
      console.log("[API] getServiceOrUserById found item for id:", id, data);
      return data;
    } else {
      return { message: "Item not found" };
    }
  } catch (error) {
    console.error("[API] getServiceOrUserById failed for id:", id, error);
    return { message: "Error fetching data" };
  }
};

export const getMetaData = async (formGuid: string) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/metaData/${formGuid}`;
  const response = await axios.get(apiUrl);
  return response.data;
};
// SETUP CONFIG

export const getAllObjectTree = async () => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/NeoForm/Objects/AllObjectTree`;
  const { data }: { data: ObjectTreeNode[] } = await axios.get(apiUrl);
  return data;
};

// ELISE
export const eliseGetContacts = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/GetContacts`;
  const { data }: { data: EliseContactSearch[] } = await axios.post(
    apiUrl,
    payload
  );
  return data;
};
export const eliseAddContactPerson = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/AddContactPerson`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseUpdateContactPerson = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/UpdateContactPerson`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseAddContactOrganization = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/AddContactOrganization`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseUpdateContactOrganization = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/UpdateContactOrganization`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseGetContactMails = async (
  starts: any,
  limit: any,
  payload: any
) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/GetContactMails?start=0&limit=20`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};
export const eliseXmlSearch = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/XmlSearch`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseGetFullThesaurus = async (
  thesaurusName: string,
  termName?: string | null
) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/GetFullThesaurus`;
  const response = await axios.get(apiUrl, {
    params: { thesaurusName, termName },
  });
  return response.data;
};

export const eliseSearchThesaurus = async (
  thesaurusId: string,
  searchTerm: string
) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/SearchThesaurus`;
  const response = await axios.get(apiUrl, {
    params: { thesaurusId, searchTerm },
  });
  return response.data;
};

export const eliseLevelThesaurus = async (
  thesaurusId: string,
  parentTermId?: string | null
) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/LevelThesaurus`;
  const response = await axios.get(apiUrl, {
    params: { thesaurusId, parentTermId },
  });
  return response.data;
};

export const eliseSendEmail = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/SendEmail`;
  await axios.post(apiUrl, payload);
};

export const eliseGetFlowchartItems = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/GetFlowchartItems`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

export const eliseApplyTrackingPath = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/ApplyTrackingPath`;
  await axios.post(apiUrl, payload);
};

export const eliseMailByTemplate = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/SendEmailByTemplate`;
  await axios.post(apiUrl, payload);
};

export const businessGenerateDocuments = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/Project/CreateDocumentsFromThesaurus`;
  await axios.post(apiUrl, payload);
};
export const eliseCustomFields = async () => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/AdminConfiguration/customFields`;
  const response = await axios.get(apiUrl);
  return response.data;
};
export const eliseEnumeration = async (cp: string) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/AdminConfiguration/enumeration`;
  const response = await axios.get(apiUrl, { params: { cp } });
  return response.data;
};

export const askChatbot = async (userInput: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/Chat`;
  const response = await axios.get(apiUrl, {
    params: { userInput },
  });
  return response.data;
};
export const callEliseWebService = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/Elise/CallEliseWS`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};
export const postAiChatMessage = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}/api/Ecs/message`;
  const response = await axios.post(apiUrl, payload);
  return response.data;
};

// MANAGE CLIENTS

export const fetchClients = async () => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}Clients`;
  const token = localStorage.getItem("authToken");
  const { data } = await axios.get(apiUrl, {
    headers: {
      Authorization: token ? `Bearer ${token}` : "",
    },
  });
  return data; // returns Dictionary: { clientId: url }
};

export const addClient = async (payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}Clients`;
  const token = localStorage.getItem("authToken");
  const response = await axios.post(apiUrl, payload, {
    headers: {
      "Content-Type": "application/json",
      Authorization: token ? `Bearer ${token}` : "",
    },
  });
  return response.data;
};

export const updateClient = async (clientId: string, payload: any) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}Clients/${clientId}`;
  const token = localStorage.getItem("authToken");
  const response = await axios.put(apiUrl, payload, {
    headers: {
      "Content-Type": "application/json",
      Authorization: token ? `Bearer ${token}` : "",
    },
  });
  return response.data;
};

export const deleteClientById = async (clientId: string) => {
  const httpRequest = useHttpRequest();
  const apiUrl = `${httpRequest.externalUrl}Clients/${clientId}`;
  const token = localStorage.getItem("authToken");
  const response = await axios.delete(apiUrl, {
    headers: {
      Authorization: token ? `Bearer ${token}` : "",
    },
  });
  return response.data;
};
export const GetAuthInfo = async ({
  code,
  guid,
}: {
  code: string;
  guid: string;
}) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.apiUrl}/api/external/auth-type`;
    const response = await axios.get(apiUrl, {
      params: { code, guid },
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] getAuthTypeFromAccessConfig failed for code:",
      code,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};
export const validateOidcCode = async (
  code: string,
  state: string,
  guid: string,
  client: string,
  personalCode: string
) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/auth/validate-oidc`;
    const configUrl = `${
      httpRequest.externalUrl + client
    }/api/external/auth-type`;
    console.log(
      "[API] validateOidcCode - URL:",
      httpRequest.externalUrl,
      "client:",
      client,
      "personalCode:",
      personalCode
    );
    const response = await axios.post(apiUrl, {
      code,
      state,
      guid,
      configUrl,
      personalCode,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] validateOidcCode failed for code:",
      code,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};

// Email Authentication API functions
export const validateEmailInvitation = async (
  email: string,
  guid: string,
  personalCode: string,
  client: string
) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/auth/validate-email`;
    const configUrl = `${
      httpRequest.externalUrl + client
    }/api/external/auth-type`;
    console.log(
      "[API] validateEmailInvitation for email:",
      email,
      "guid:",
      guid,
      "client:",
      client
    );

    const response = await axios.post(apiUrl, {
      email,
      guid,
      personalCode,
      configUrl,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] validateEmailInvitation failed for email:",
      email,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const sendEmailOTP = async (
  email: string,
  guid: string,
  personalCode: string,
  client: string
) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/auth/send-otp`;
    const configUrl = `${
      httpRequest.externalUrl + client
    }/api/external/auth-type`;
    console.log(
      "[API] sendEmailOTP to email:",
      email,
      "guid:",
      guid,
      "client:",
      client
    );

    const response = await axios.post(apiUrl, {
      email,
      guid,
      personalCode,
      configUrl,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] sendEmailOTP failed for email:",
      email,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const verifyEmailOTP = async (
  email: string,
  otp: string,
  guid: string,
  personalCode: string,
  client: string
) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/auth/verify-otp`;
    const configUrl = `${
      httpRequest.externalUrl + client
    }/api/external/auth-type`;
    console.log(
      "[API] verifyEmailOTP for email:",
      email,
      "guid:",
      guid,
      "client:",
      client
    );

    const response = await axios.post(apiUrl, {
      email,
      otp,
      guid,
      personalCode,
      configUrl,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] verifyEmailOTP failed for email:",
      email,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};

// Validate authentication token to prevent session storage manipulation
export const validateAuthToken = async (
  token: string,
  email: string,
  guid: string,
  personalCode: string,
  client: string
) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/auth/validate-token`;
    const configUrl = `${
      httpRequest.externalUrl + client
    }/api/external/auth-type`;
    console.log(
      "[API] validateAuthToken for email:",
      email,
      "guid:",
      guid,
      "client:",
      client
    );

    const response = await axios.post(apiUrl, {
      token,
      email,
      guid,
      personalCode,
      configUrl,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] validateAuthToken failed for email:",
      email,
      "guid:",
      guid,
      error
    );
    logger.error(error);
    throw error;
  }
};
// Create a unified session token for any authentication type
export const createSessionToken = async ({
  guid,
  code,
  authType,
  clientId,
  email,
  oidcUserId,
}: {
  guid: string;
  code: string;
  authType: string;
  clientId: string;
  email?: string;
  oidcUserId?: string;
}) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}local/api/Session/create`;
    console.log(
      "[API] createSessionToken for authType:",
      authType,
      "guid:",
      guid,
      "clientId:",
      clientId
    );

    const response = await axios.post(apiUrl, {
      guid,
      code,
      authType,
      clientId,
      email,
      oidcUserId,
    });
    return response.data;
  } catch (error) {
    console.error(
      "[API] createSessionToken failed for authType:",
      authType,
      "clientId:",
      clientId,
      error
    );
    logger.error(error);
    throw error;
  }
};

export const getClientApiKey = async (clientId: string) => {
  const httpRequest = useHttpRequest();
  try {
    const apiUrl = `${httpRequest.externalUrl}Clients/${clientId}/apikey`;
    const response = await axios.get(apiUrl);
    return response.data.apiKey; // Returns the API key for the specified client
  } catch (error) {
    console.error("Error fetching client API key:", error);
    logger.error(error);
    throw error;
  }
};

export const executeWorkflow = async (payload: {
  identifier: string;
  documentId: string;
  mappingName: string;
  rackCode: string;
  doNotUpdateDocument: boolean;
  parameters: Record<string, string>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/api/automate/executeWorkflow`;
    const { data } = await axios.post(apiUrl, payload);
    return { success: true, ...data };
  } catch (error: any) {
    console.error("Workflow execution failed:", error);
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "Workflow execution failed",
    };
  }
};

export const executeStandalone = async (payload: {
  identifier: string;
  batchId: string;
  version: number;
  parameters: Record<string, string>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/api/automate/executeStandalone`;
    const { data } = await axios.post(apiUrl, payload);
    return { success: true, ...data };
  } catch (error: any) {
    console.error(
      "[API] executeStandalone failed for identifier:",
      payload.identifier,
      error
    );
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "Standard Workflow execution failed",
    };
  }
};
export const executeAsyncWorkflow = (payload: {
  identifier: string;
  documentId: string;
  mappingName: string;
  rackCode: string;
  doNotUpdateDocument: boolean;
  parameters: Record<string, string>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/api/automate/QueueWorkflow`;
    axios.post(apiUrl, payload);
    return { success: true, message: "Workflow execution started" };
  } catch (error: any) {
    console.error(
      "[API] executeAsyncWorkflow failed for identifier:",
      payload.identifier,
      error
    );
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "Workflow execution failed",
    };
  }
};

export const executeAsyncStandalone = (payload: {
  identifier: string;
  batchId: string;
  version: number;
  parameters: Record<string, string>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/api/automate/QueueStandalone`;
    axios.post(apiUrl, payload);
    return { success: true, message: "Script execution started" };
  } catch (error: any) {
    console.error(
      "[API] executeAsyncStandalone failed for identifier:",
      payload.identifier,
      error
    );
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "Standard Workflow execution failed",
    };
  }
};

export const generateModelWithoutNotice = async (payload: {
  courrierId: string;
  modelGuid: string;
  chrono: number;
  data: Record<string, string>;
  htmls: Record<string, string>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/NeoForm/Model`;
    const { data } = await axios.post(apiUrl, payload);
    return { success: true, ...data };
  } catch (error: any) {
    console.error(
      "[API] generateModelWithoutNotice failed for courrierId:",
      payload.courrierId,
      error
    );
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "Document generation failed",
    };
  }
};
export const publishFiles = async (payload: {
  attachments: Array<{ name: string; content: string }>;
}) => {
  try {
    const httpRequest = useHttpRequest();
    const apiUrl = `${httpRequest.apiUrl}/api/project/PublishFiles`;
    const { data } = await axios.post(apiUrl, payload);
    return { success: true, ...data };
  } catch (error: any) {
    console.error(
      "[API] publishFiles failed, attachments count:",
      payload.attachments?.length,
      error
    );
    logger.error(error);
    return {
      success: false,
      error:
        error.response?.data?.message ||
        error.message ||
        "File publishing failed",
    };
  }
};
