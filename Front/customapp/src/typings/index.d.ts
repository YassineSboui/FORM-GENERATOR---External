interface Label {
  key: string;
  value: string;
}

interface AttachmentFilter {
  hasAttachments: boolean;
}
interface Button {
  id: string;
  labels: Label[];
  actionEvent: string;
  position: number;
}
interface Action {
  id: number;
  data: {
    actionId: string;
    labels: Label[];
    roles: string[];
    icon: {
      name: string;
      type: string;
    };
    attachmentFilter: AttachmentFilter;
    applications: string[];
    displayByPriority: number;
    refreshData: boolean;
    eliseUi: [
      {
        scope: string;
        target: string;
        buttons: Button[];
        multiselect: boolean;
        frameConfiguration: {
          frameId: string;
          size: { height: number; width: number; unitMeasure: string };
        };
      },
    ];
    documentFilter: {
      types: string[]; //"COURRIERS_TYPE_XXX"
      taskStates: number[]; // 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
      states: number[]; // 1, 2, 3, 4, 5
      validationStates: number[]; // 0, 1, 2, 3, 4
    };
    attachmentFilter: {
      hasAttachments: boolean;
    };
    url: string;
    queryParameters: [{ key: string; value: string }];
  };
}
interface XmlSearchConfig {
  guid: string;
  code: string;
  description: string;
  titre: string;
  xml: string;
  parameters: Label[];
}
interface mailTemplate {
  guid: string;
  code: string;
  description: string;
  template: string;
  parameters: Label[];
  titre: string;
}

interface TableVariables {
  key: string;
  value: TableVariable[];
}
interface TableVariable {
  key: string;
  value: string;
}
/*
interface Term {
  children : Term[],
  entryId: string;
  label: string;
  extraProperties : any[]
}
*/
interface Term {
  thesaurusId: string;
  termId: string;
  label: string;
  isLeaf: boolean;
  isSyno: boolean;
  status: number;
  syno?: Term;
  extraProp: { key: string; value: string }[];
}

interface EliseDocument {
  chrono: string;
  mailId: string;
  url: string;
}

interface EliseContactSearch {
  address?: {
    city: string;
    complete: string;
    country: string;
    email: null;
    faxNumber: {
      areaCode: number;
      isPrivate: boolean;
      isProfessionnal: boolean;
      number: string;
      priority: number;
    }[];
    geographicalArea: null;
    lines: (null | string)[];
    name: null;
    phoneNumber: {
      areaCode: number;
      isPrivate: boolean;
      isProfessionnal: boolean;
      number: string;
      priority: number;
    }[];
    postalCode: string;
  };
  category: {
    organization: string;
    person: string;
  };
  contactType: number;
  externalTemporary: null;
  index: number;
  linkedMails: null;
  mission: {
    eid: null;
    function: null;
    missionId: null;
    missionType: number;
    organization?: {
      category: {
        code: string;
        description: string;
        id: string;
        label: string;
        priority: number;
        style: null;
      };
      name: string;
      nationalId: string;
      standardFaxNumber: string;
      standardPhoneNumber: string;
      addresses: any[];
      contactType: number;
      customFields: string[];
      email: string;
      faxNumber: {
        areaCode: number;
        isPrivate: boolean;
        isProfessionnal: boolean;
        number: null;
        priority: number;
      }[];
      id: string;
      mobilePhoneNumber: null;
      phoneNumber: null;
      professionnalEmail: null;
      quickText: null;
      referential: {
        acronym: null;
        description: null;
        id: string;
        name: string;
      };
    };
    person?: {
      birth: {
        country: string;
        date: null;
        location: string;
      };
      category: {
        code: null;
        description: null;
        id: string;
        label: null;
        priority: number;
        style: null;
      };
      firstName: string;
      lastName: string;
      name: string;
      title: number;
      addresses: {
        addressee: null;
        building: null;
        city: string;
        companyName: null;
        country: string;
        email: null;
        faxNumber: {
          areaCode: number;
          isPrivate: boolean;
          isProfessionnal: boolean;
          number: string;
          priority: number;
        }[];
        geographicalArea: null;
        id: number;
        locality: null;
        phoneNumber: {
          areaCode: number;
          isPrivate: boolean;
          isProfessionnal: boolean;
          number: string;
          priority: number;
        }[];
        postalCode: string;
        priority: number;
        street: string;
      }[];
      contactType: number;
      customFields: string[];
      email: string;
      faxNumber: {
        areaCode: number;
        isPrivate: boolean;
        isProfessionnal: boolean;
        number: string;
        priority: number;
      }[];
      id: string;
      mobilePhoneNumber: {
        areaCode: number;
        isPrivate: boolean;
        isProfessionnal: boolean;
        number: string;
        priority: number;
      }[];
      phoneNumber: {
        areaCode: number;
        isPrivate: boolean;
        isProfessionnal: boolean;
        number: string;
        priority: number;
      }[];
      professionnalEmail: null;
      quickText: null;
      referential: {
        acronym: null;
        description: null;
        id: string;
        name: string;
      };
    };
  };
  organizationEntity: null;
  personnalMobilePhoneNumber: string;
  standardEmail: string;
  standardFaxNumber: null;
  standardPhoneNumber: null;
  verification: null;
}

interface ObjectTreeNode {
  key: string;
  label: string;
  data: string;
  icon: string;
  children: ObjectTreeNode[];
}

interface ObjectModel {
  id: number;
  guid: string;
  application: string;
  objectName: string;
  objectType: string;
  objectJson: string;
}

interface keyBooleanValue {
  key: string;
  value: boolean;
}

interface StoreFunction {
  name: string;
  parameters: { key: string; value: string }[];
  code: string;
  mode: string;
  category: string;
  description: string;
}

enum NoticeType {
  All,
  File,
  Html,
  Simple,
}
