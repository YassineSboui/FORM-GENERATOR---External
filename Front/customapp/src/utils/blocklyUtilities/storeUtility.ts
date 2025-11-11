/**
 * Store Utility Functions
 *
 * Provides safe, abstracted access to store operations for Blockly-generated code.
 * Hides internal implementation details (stores, APIs, etc.)
 *
 * @module storeUtility
 */

import { logger } from "@/api/api";

/**
 * Store Utility Class
 * Provides safe store operation methods
 */
export class StoreUtility {
  private store: any = null;

  constructor() {
    // Will be initialized via initStoreUtility
  }

  /**
   * Get store instance
   * @private
   */
  private getStore(): any {
    if (!this.store) {
      throw new Error(
        "[StoreUtility] Store not available. Call initStoreUtility() before using store operations."
      );
    }
    return this.store;
  }

  /**
   * Safely stringify values for logging (falls back to String on circular/unknown types)
   * @private
   */
  private stringifyForLog(value: any): string {
    try {
      if (typeof value === "string") return value;
      return JSON.stringify(value);
    } catch {
      return String(value);
    }
  }

  /**
   * Execute database connection with parameters
   * @param collectionName - Name of the database collection
   * @param parameters - Array of key-value parameters
   * @returns Promise with the result
   */
  async executeDatabaseConnection(
    collectionName: string,
    parameters?: Array<{ key: string; value: any }>
  ): Promise<any> {
    try {
      const store = this.getStore();
      const result = await store.executeDatabaseConnection(
        collectionName,
        parameters
      );
      logger.debug(
        `[StoreUtility] Executed database connection: ${collectionName}`
      );
      return result;
    } catch (error) {
      logger.error(
        `[StoreUtility] Error executing database connection: ${this.stringifyForLog(
          error
        )}`
      );
      throw error;
    }
  }

  /**
   * Execute API collection with parameters
   * @param collectionName - Name of the API collection
   * @param parameters - Array of key-value parameters
   * @returns Promise with the result
   */
  async executeApiCollection(
    collectionName: string,
    parameters?: Array<{ Key: string; Value: any }>
  ): Promise<any> {
    try {
      const store = this.getStore();
      const result = await store.executeApiCollection(
        collectionName,
        parameters
      );
      logger.debug(`[StoreUtility] Executed API collection: ${collectionName}`);
      return result;
    } catch (error) {
      logger.error(
        `[StoreUtility] Error executing API collection: ${this.stringifyForLog(
          error
        )}`
      );
      throw error;
    }
  }

  /**
   * Get object by lexicon
   * @param lexicon - The lexicon identifier
   * @returns Promise with the object data
   */
  async getObjectByLexicon(lexicon: string): Promise<any> {
    try {
      const store = this.getStore();
      const result = await store.getObjectByLexicon(lexicon);
      logger.debug(`[StoreUtility] Got object by lexicon: ${lexicon}`);
      return result;
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting object by lexicon: ${this.stringifyForLog(
          error
        )}`
      );
      throw error;
    }
  }

  /**
   * Generate final link for external form
   * @param formId - The form identifier
   * @param accessCode - The access code
   * @param parameters - Array of key-value parameters
   * @returns Promise with success status
   */
  async generateFinalLink(
    formId: string,
    accessCode: string,
    parameters?: Array<{ key: string; value: any }>
  ): Promise<boolean> {
    try {
      const store = this.getStore();
      const result = await store.generateFinalLink(
        formId,
        accessCode,
        parameters
      );
      logger.debug(`[StoreUtility] Generated final link for form: ${formId}`);
      return result;
    } catch (error) {
      logger.error(
        `[StoreUtility] Error generating final link: ${this.stringifyForLog(
          error
        )}`
      );
      return false;
    }
  }

  /**
   * Get external forms list
   * @returns Array of external forms
   */
  getExternalForms(): any[] {
    try {
      const store = this.getStore();
      return store.externalForms || [];
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting external forms: ${this.stringifyForLog(
          error
        )}`
      );
      return [];
    }
  }

  /**
   * Get databases collections list
   * @returns Array of database collections
   */
  getDatabasesCollections(): any[] {
    try {
      const store = this.getStore();
      return store.databasesCollections || [];
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting databases collections: ${this.stringifyForLog(
          error
        )}`
      );
      return [];
    }
  }

  /**
   * Get API collections list
   * @returns Array of API collections
   */
  getApiCollections(): any[] {
    try {
      const store = this.getStore();
      return store.apiCollections || [];
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting API collections: ${this.stringifyForLog(
          error
        )}`
      );
      return [];
    }
  }

  /**
   * Get store Fields object
   * @returns Fields object from store
   */
  getFields(): any {
    try {
      const store = this.getStore();
      return store.Fields || {};
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting Fields: ${this.stringifyForLog(error)}`
      );
      return {};
    }
  }

  /**
   * Get table variables
   * @returns Array of table variables
   */
  getTableVariables(): any[] {
    try {
      const store = this.getStore();
      return store.tableVariables || [];
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting table variables: ${this.stringifyForLog(
          error
        )}`
      );
      return [];
    }
  }

  /**
   * Get table variable value by table key and variable key
   * Returns a reactive reference object that can be used to both read and update the value
   *
   * @param tableKey - The table identifier key (e.g., 'TBL_USERS')
   * @param variableKey - The variable key within the table
   * @returns Object with 'value' property that references the store value, or { value: undefined } if not found
   *
   * @example
   * // Get value
   * const myVar = storeUtility.getTableVariableValue('TBL_USERS', 'USER_NAME');
   * console.log(myVar.value); // Read the value
   *
   * // Update value (updates the store directly)
   * myVar.value = 'New Value'; // This updates the store
   */
  getTableVariableValue(tableKey: string, variableKey: string): { value: any } {
    try {
      const store = this.getStore();
      const tableVar = store.tableVariables?.find(
        (variable: any) => variable.key === tableKey
      );
      if (!tableVar) {
        logger.warn(`[StoreUtility] Table variable not found: ${tableKey}`);
        return { value: undefined };
      }
      const varObj = tableVar.value?.find(
        (vari: any) => vari.key === variableKey
      );
      if (!varObj) {
        logger.warn(
          `[StoreUtility] Variable key not found in table: ${variableKey}`
        );
        return { value: undefined };
      }
      // Return a proxy object that only exposes 'value' but references the store object
      return {
        get value() {
          return varObj.value;
        },
        set value(newValue: any) {
          varObj.value = newValue;
        },
      };
    } catch (error) {
      logger.error(
        `[StoreUtility] Error getting table variable value: ${this.stringifyForLog(
          error
        )}`
      );
      return { value: undefined };
    }
  }
}

/**
 * Global store utility instance
 * Available in Blockly-generated code as: storeUtility.executeDatabaseConnection(...)
 */
export const storeUtility = new StoreUtility();

/**
 * Initialize store utility in execution context
 * Call this in ComponentForm.vue before executing Blockly code
 */
export function initStoreUtility(store: any) {
  (storeUtility as any).store = store;
}
