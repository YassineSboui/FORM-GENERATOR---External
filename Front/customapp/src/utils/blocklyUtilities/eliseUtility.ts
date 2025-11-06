/**
 * Elise Utility Functions
 * 
 * Provides safe Elise-specific operations for Blockly-generated code.
 * Handles workflows, contacts, documents, and other Elise system operations.
 * 
 * All API calls are made directly through this utility - no need to pass functions from ComponentForm.
 * 
 * @module eliseUtility
 */

import { 
  logger, 
  callEliseWebService, 
  executeWorkflow as apiExecuteWorkflow, 
  executeStandalone as apiExecuteStandalone, 
  executeAsyncWorkflow as apiExecuteAsyncWorkflow, 
  executeAsyncStandalone as apiExecuteAsyncStandalone, 
  generateModelWithoutNotice,
  publishFiles as apiPublishFiles
} from "@/api/api";

/**
 * Elise Utility Class
 * Provides safe Elise system operations
 */
export class EliseUtility {
  private store: any = null;

  /**
   * Initialize elise utility with store
   * Must be called before using any elise methods that require store
   * @param store - Pinia store instance
   */
  init(store: any): void {
    this.store = store;
  }

  /**
   * Execute workflow - wrapper for API call
   * Accepts parameters object from Blockly blocks
   * @param parameters - Workflow parameters from Blockly
   * @returns Workflow execution result
   */
  async executeWorkflow(parameters: any): Promise<any> {
    try {
      logger.debug(`Executing workflow`);
      return await apiExecuteWorkflow(parameters);
    } catch (error) {
      logger.error(`Error executing workflow: ${error}`);
      throw error;
    }
  }

  /**
   * Execute async workflow - wrapper for API call
   * Accepts parameters object from Blockly blocks
   * @param parameters - Workflow parameters from Blockly
   */
  executeWorkflowAsync(parameters: any): void {
    try {
      logger.debug(`Executing async workflow`);
      apiExecuteAsyncWorkflow(parameters);
    } catch (error) {
      logger.error(`Error executing async workflow: ${error}`);
      throw error;
    }
  }

  /**
   * Execute standalone workflow - wrapper for API call
   * Accepts parameters object from Blockly blocks
   * @param parameters - Workflow parameters from Blockly
   * @returns Workflow execution result
   */
  async executeStandaloneWorkflow(parameters: any): Promise<any> {
    try {
      logger.debug(`Executing standalone workflow`);
      return await apiExecuteStandalone(parameters);
    } catch (error) {
      logger.error(`Error executing standalone workflow: ${error}`);
      throw error;
    }
  }

  /**
   * Execute async standalone workflow - wrapper for API call
   * Accepts parameters object from Blockly blocks
   * @param parameters - Workflow parameters from Blockly
   */
  executeStandaloneWorkflowAsync(parameters: any): void {
    try {
      logger.debug(`Executing async standalone workflow`);
      apiExecuteAsyncStandalone(parameters);
    } catch (error) {
      logger.error(`Error executing async standalone workflow: ${error}`);
      throw error;
    }
  }

  /**
   * Generate model without notice
   * @param parameters - Model generation parameters
   * @returns Generated model result
   */
  async generateModel(parameters: any): Promise<any> {
    try {
      logger.debug('Generating model without notice');
      return await generateModelWithoutNotice(parameters);
    } catch (error) {
      logger.error(`Error generating model: ${error}`);
      throw error;
    }
  }

  /**
   * Get current document ID
   * @returns Document ID
   */
  getDocumentId(): number | null {
    try {
      if (this.store && this.store.document && this.store.document.id) {
        return this.store.document.id;
      }
      return null;
    } catch (error) {
      logger.error(`Error getting document ID: ${error}`);
      return null;
    }
  }

  /**
   * Get current form ID
   * @returns Form ID
   */
  getFormId(): string | null {
    try {
      if (this.store && this.store.formId) {
        return this.store.formId;
      }
      return null;
    } catch (error) {
      logger.error(`Error getting form ID: ${error}`);
      return null;
    }
  }

  /**
   * Get Elise URL
   * @returns Elise base URL
   */
  getEliseUrl(): string {
    try {
      if (this.store && this.store.eliseUrl) {
        return this.store.eliseUrl;
      }
      return '';
    } catch (error) {
      logger.error(`Error getting Elise URL: ${error}`);
      return '';
    }
  }

  /**
   * Get Elise instance name
   * @returns Instance name
   */
  getInstanceName(): string {
    try {
      if (this.store && this.store.instance) {
        return this.store.instance;
      }
      return '';
    } catch (error) {
      logger.error(`Error getting instance name: ${error}`);
      return '';
    }
  }

  /**
   * Get current user info
   * @returns User information
   */
  getCurrentUser(): any {
    try {
      if (this.store && this.store.user) {
        return this.store.user;
      }
      return null;
    } catch (error) {
      logger.error(`Error getting current user: ${error}`);
      return null;
    }
  }

  /**
   * Get document information
   * @returns Document object
   */
  getDocument(): any {
    try {
      if (this.store && this.store.document) {
        return this.store.document;
      }
      return null;
    } catch (error) {
      logger.error(`Error getting document: ${error}`);
      return null;
    }
  }

  /**
   * Check if document exists
   * @returns True if document exists
   */
  hasDocument(): boolean {
    try {
      return this.store && this.store.document && this.store.document.id != null;
    } catch (error) {
      logger.error(`Error checking document existence: ${error}`);
      return false;
    }
  }

  /**
   * Get document status
   * @returns Document status
   */
  getDocumentStatus(): string {
    try {
      if (this.store && this.store.document && this.store.document.status) {
        return this.store.document.status;
      }
      return '';
    } catch (error) {
      logger.error(`Error getting document status: ${error}`);
      return '';
    }
  }

  /**
   * Get contact by ID
   * @param contactId - Contact ID
   * @returns Contact information
   */
  getContact(contactId: number): any {
    try {
      // This would typically call an API to fetch contact details
      // For now, return placeholder
      logger.debug(`Getting contact: ${contactId}`);
      return { id: contactId };
    } catch (error) {
      logger.error(`Error getting contact ${contactId}: ${error}`);
      return null;
    }
  }

  /**
   * Format date for Elise
   * @param date - Date object or string
   * @returns Formatted date string
   */
  formatDate(date: Date | string): string {
    try {
      const dateObj = typeof date === 'string' ? new Date(date) : date;
      if (isNaN(dateObj.getTime())) {
        throw new Error('Invalid date');
      }
      return dateObj.toISOString();
    } catch (error) {
      logger.error(`Error formatting date: ${error}`);
      return '';
    }
  }

  /**
   * Log debug message
   * @param message - Message to log
   */
  logDebug(message: string): void {
    logger.debug(message);
  }

  /**
   * Log error message
   * @param message - Message to log
   */
  logError(message: string): void {
    logger.error(message);
  }

  /**
   * Get Elise contacts
   * @param payload - Search parameters
   * @returns List of contacts
   */
  async getContacts(payload: {
    isPerson: string;
    isOrganization: string;
    options: string;
    searchFilter: string;
    limit: number;
    addressBook: string;
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Getting Elise contacts');
      return await this.store.eliseGetContacts(payload);
    } catch (error) {
      logger.error(`Error getting contacts: ${error}`);
      throw error;
    }
  }

  /**
   * Add contact person
   * @param payload - Person data
   * @returns Created person
   */
  async addContactPerson(payload: any): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Adding contact person');
      return await this.store.eliseAddContactPerson(payload);
    } catch (error) {
      logger.error(`Error adding contact person: ${error}`);
      throw error;
    }
  }

  /**
   * Update contact person
   * @param payload - Updated person data
   * @returns Updated person
   */
  async updateContactPerson(payload: any): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Updating contact person');
      return await this.store.eliseUpdateContactPerson(payload);
    } catch (error) {
      logger.error(`Error updating contact person: ${error}`);
      throw error;
    }
  }

  /**
   * Add contact organization
   * @param payload - Organization data
   * @returns Created organization
   */
  async addContactOrganization(payload: any): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Adding contact organization');
      return await this.store.eliseAddContactOrganization(payload);
    } catch (error) {
      logger.error(`Error adding contact organization: ${error}`);
      throw error;
    }
  }

  /**
   * Update contact organization
   * @param payload - Updated organization data
   * @returns Updated organization
   */
  async updateContactOrganization(payload: any): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Updating contact organization');
      return await this.store.eliseUpdateContactOrganization(payload);
    } catch (error) {
      logger.error(`Error updating contact organization: ${error}`);
      throw error;
    }
  }

  /**
   * Execute XML search
   * @param payload - Search parameters
   * @returns Search results
   */
  async xmlSearch(payload: { limit: string; guid: string; parameters: any[] }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug(`Executing XML search: ${payload.guid}`);
      return await this.store.eliseXmlSearch(payload);
    } catch (error) {
      logger.error(`Error in XML search: ${error}`);
      throw error;
    }
  }

  /**
   * Get full thesaurus
   * @param thesaurusName - Thesaurus name
   * @param termName - Optional term name
   * @returns Thesaurus data
   */
  async getFullThesaurus(thesaurusName: string, termName?: string): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug(`Getting full thesaurus: ${thesaurusName}`);
      return await this.store.eliseGetFullThesaurus(thesaurusName, termName);
    } catch (error) {
      logger.error(`Error getting thesaurus: ${error}`);
      throw error;
    }
  }

  /**
   * Send email by template
   * @param template - Template configuration
   * @returns Send result
   */
  async sendEmailByTemplate(template: {
    Guid: string;
    SmtpConfig: string;
    Parameters: any[];
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug(`Sending email by template: ${template.Guid}`);
      return await this.store.sendEliseMailByTemplate(template);
    } catch (error) {
      logger.error(`Error sending email by template: ${error}`);
      throw error;
    }
  }

  /**
   * Send email
   * @param emailPayload - Email data
   * @returns Send result
   */
  async sendEmail(emailPayload: {
    smtpConfig: string;
    fromEmail: any;
    toEmail: any[];
    subject: string;
    message: string;
    ccEmail?: any[];
    bccEmail?: any[];
    attachments?: any[];
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Sending email');
      return await this.store.eliseSendEmail(emailPayload);
    } catch (error) {
      logger.error(`Error sending email: ${error}`);
      throw error;
    }
  }

  /**
   * Get flowchart items
   * @param payload - Search parameters
   * @returns Flowchart items
   */
  async getFlowchartItems(payload: {
    fullService: string;
    itemType: string;
    ldapAttribute: string;
    searchTerm: string;
    searchType: string;
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Getting flowchart items');
      return await this.store.eliseGetFlowchartItems(payload);
    } catch (error) {
      logger.error(`Error getting flowchart items: ${error}`);
      throw error;
    }
  }

  /**
   * Apply tracking path
   * @param payload - Tracking path data
   * @returns Result
   */
  async applyTrackingPath(payload: {
    documentId: string;
    trackingPathId: string;
    trackingPathMotherId: string;
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Applying tracking path');
      return await this.store.eliseApplyTrackingPath(payload);
    } catch (error) {
      logger.error(`Error applying tracking path: ${error}`);
      throw error;
    }
  }

  /**
   * Get current notice
   * @returns Notice object
   */
  getCurrentNotice(): any {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      return this.store.currentNotice;
    } catch (error) {
      logger.error(`Error getting current notice: ${error}`);
      return null;
    }
  }

  /**
   * Get current Elise document
   * @returns Elise document object
   */
  getCurrentEliseDocument(): any {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      return this.store.currentEliseDocument;
    } catch (error) {
      logger.error(`Error getting current Elise document: ${error}`);
      return null;
    }
  }

  /**
   * Generate documents by thesaurus
   * @param payload - Generation parameters
   * @returns Result
   */
  async generateDocuments(payload: {
    thesaurusID: string;
    parentID: string;
    reference: string;
  }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Generating documents');
      return await this.store.businessGenerateDocuments(payload);
    } catch (error) {
      logger.error(`Error generating documents: ${error}`);
      throw error;
    }
  }

  /**
   * Post AI chat message
   * @param payload - Message payload
   * @returns AI response
   */
  async postAiChatMessage(payload: { Prompt: string; Files: any[] }): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug('Posting AI chat message');
      return await this.store.storePostAiChatMessage(payload);
    } catch (error) {
      logger.error(`Error posting AI message: ${error}`);
      throw error;
    }
  }

  /**
   * Get object by lexicon
   * @param lexiconId - Lexicon ID
   * @returns Object data
   */
  async getObjectByLexicon(lexiconId: string): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug(`Getting object by lexicon: ${lexiconId}`);
      return await this.store.getObjectByLexicon(lexiconId);
    } catch (error) {
      logger.error(`Error getting object by lexicon: ${error}`);
      throw error;
    }
  }

  /**
   * Execute Elise web service
   * @param webServiceName - Web service name/type
   * @param parameters - Web service parameters
   * @returns Web service result
   */
  async executeWebService(webServiceName: string, parameters: any): Promise<any> {
    try {
      const payload = {
        eliseWsInputType: webServiceName,
        objet: parameters,
      };
      
      logger.debug(`Executing web service: ${webServiceName}`);
      const result = await callEliseWebService(payload);
      return result;
    } catch (error) {
      logger.error(`Error executing web service ${webServiceName}: ${error}`);
      throw error;
    }
  }

  /**
   * Publish files
   * @param parameters - Publish parameters
   * @returns Publish result
   */
  async publishFiles(parameters: any): Promise<any> {
    try {
      logger.debug('Publishing files');
      return await apiPublishFiles(parameters);
    } catch (error) {
      logger.error(`Error publishing files: ${error}`);
      throw error;
    }
  }

  /**
   * Safely access nested properties of an object
   * Handles array notation like datas[0].COL_VALEUR and null/undefined checks
   * @param object - The object to access
   * @param path - The property path (e.g., "datas[0].COL_VALEUR" or "user.name")
   * @returns The value at the path, or undefined if not accessible
   */
  getNestedProperty(object: any, path: string): any {
    try {
      // Check if object is null or undefined
      if (object == null) {
        console.warn("Object is null or undefined:", object);
        return undefined;
      }
      
      // Handle array notation like datas[0].COL_VALEUR
      const normalizedPath = path.replace(/\[(\d+)\]/g, '.$1');
      const keys = normalizedPath.split('.');
      let current = object;
      
      for (const key of keys) {
        if (current == null) {
          console.warn("Cannot access property '" + key + "' of null or undefined");
          return undefined;
        }
        
        // Check if key is numeric (array index)
        const numericKey = parseInt(key, 10);
        if (!isNaN(numericKey) && Array.isArray(current)) {
          if (numericKey >= current.length || numericKey < 0) {
            console.warn("Array index '" + numericKey + "' is out of bounds");
            return undefined;
          }
          current = current[numericKey];
        } else {
          if (typeof current !== 'object' || !(key in current)) {
            console.warn("Property '" + key + "' does not exist in object:", current);
            return undefined;
          }
          current = current[key];
        }
      }
      
      return current;
    } catch (error) {
      console.error("Error accessing nested property:", error);
      return undefined;
    }
  }

  /**
   * Execute a custom function by name
   * @param functionName - Name of the function to execute
   * @param parameters - Array of parameters with key/value pairs
   * @returns Function execution result
   */
  async executeFunctionByName(functionName: string, parameters: Array<{ key: string; value: any }>): Promise<any> {
    if (!this.store) {
      throw new Error('Elise utility not initialized');
    }
    
    try {
      logger.debug(`Executing function by name: ${functionName}`);
      return await this.store.executeFunctionByName(functionName, parameters);
    } catch (error) {
      logger.error(`Error executing function ${functionName}: ${error}`);
      throw error;
    }
  }
}

/**
 * Global elise utility instance
 * Available in Blockly-generated code as: eliseUtility.getContacts({...})
 */
export const eliseUtility = new EliseUtility();

/**
 * Initialize elise utility with store
 * Should be called once during application initialization
 * @param store - Pinia store instance
 */
export function initEliseUtility(store: any): void {
  eliseUtility.init(store);
}
