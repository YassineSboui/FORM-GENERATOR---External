/**
 * Field Utility Functions
 * 
 * Provides safe, abstracted access to form field operations.
 * Hides internal implementation details (stores, refs, etc.)
 * 
 * @module fieldUtility
 */

import { getCurrentInstance } from "vue";
import { useAppStore } from "@/store/app.store";
import { logger } from "@/api/api";

/**
 * Field Utility Class
 * Provides safe field manipulation methods
 */
export class FieldUtility {
  private app: any = null;
  private store: any = null;

  constructor() {
    // Don't initialize here - will be initialized lazily on first use
  }

  /**
   * Lazy initialization - get app instance on first use
   * Will use manually set app (via initFieldUtility) or getCurrentInstance()
   * @private
   */
  private getApp(): any {
    if (!this.app) {
      // Try to get current instance (works in component context)
      const instance = getCurrentInstance();
      if (instance) {
        this.app = instance;
      } else {
        throw new Error('[FieldUtility] App instance not available. Call initFieldUtility() before using field operations.');
      }
    }
    return this.app;
  }

  /**
   * Lazy initialization - get store instance on first use
   * Will use manually set store (via initFieldUtility) or useAppStore()
   * @private
   */
  private getStore(): any {
    if (!this.store) {
      try {
        this.store = useAppStore();
      } catch (error) {
        throw new Error('[FieldUtility] Store not available. Call initFieldUtility() with store instance before using field operations.');
      }
    }
    return this.store;
  }

  /**
   * Safely stringify values for logging (falls back to String on circular/unknown types)
   * @private
   */
  private stringifyForLog(value: any): string {
    try {
      if (typeof value === 'string') return value;
      return JSON.stringify(value);
    } catch {
      return String(value);
    }
  }

  /**
   * Validate field name format
   * @private
   */
  private validateFieldName(fieldName: string): boolean {
    if (!fieldName || typeof fieldName !== 'string') {
      throw new Error('Field name must be a non-empty string');
    }
    
    // Only allow CF_ prefix + alphanumeric + underscore
    if (!/^CF_[A-Z0-9_]+$/.test(fieldName)) {
      throw new Error(`Invalid field name format: ${fieldName}. Expected format: CF_FIELDNAME`);
    }
    
    return true;
  }

  /**
   * Get field reference safely
   * @private
   */
  private getFieldRef(fieldName: string): any {
    this.validateFieldName(fieldName);
    
    const app = this.getApp();
    const fieldRef = app?.refs?.[fieldName];
    if (!fieldRef || !fieldRef[0]) {
      throw new Error(`Field not found: ${fieldName}`);
    }
    
    return fieldRef[0];
  }

  /**
   * Show a field
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   */
  showField(fieldName: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.showField();
      logger.debug(`[FieldUtility] Showed field: ${fieldName}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error showing field ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Hide a field
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   */
  hideField(fieldName: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.hideField();
      logger.debug(`[FieldUtility] Hid field: ${fieldName}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error hiding field ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Enable a field
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   */
  enableField(fieldName: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.enableField();
      logger.debug(`[FieldUtility] Enabled field: ${fieldName}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error enabling field ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Disable a field
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   */
  disableField(fieldName: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.disableField();
      logger.debug(`[FieldUtility] Disabled field: ${fieldName}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error disabling field ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Get field value
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   * @returns Field value
   */
  getValue(fieldName: string): any {
    try {
      // Access via store Fields
      const store = this.getStore();
      const value = store.Fields[fieldName];
      logger.debug(`[FieldUtility] Got value for ${fieldName}: ${this.stringifyForLog(value)}`);
      return value;
    } catch (error) {
      logger.error(`[FieldUtility] Error getting value for ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Set field value
   * @param fieldName - Field identifier (e.g., "CF_NAME")
   * @param value - New value to set
   */
  setValue(fieldName: string, value: any): void {
    try {
      this.validateFieldName(fieldName);
      
      // Set via store Fields
      const store = this.getStore();
      store.Fields[fieldName] = value;
      logger.debug(`[FieldUtility] Set value for ${fieldName}: ${this.stringifyForLog(value)}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error setting value for ${fieldName}: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Copy value from one field to another
   * @param sourceField - Source field identifier
   * @param targetField - Target field identifier
   */
  copyFieldValue(sourceField: string, targetField: string): void {
    try {
      const value = this.getValue(sourceField);
      this.setValue(targetField, value);
      logger.debug(`[FieldUtility] Copied value from ${sourceField} to ${targetField}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error copying field value: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Set field error
   * @param fieldName - Field identifier
   * @param errorMessage - Error message to display
   */
  setFieldError(fieldName: string, errorMessage: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.setFieldError(errorMessage);
      logger.debug(`[FieldUtility] Set error for ${fieldName}: ${errorMessage}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error setting field error: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Clear field error
   * @param fieldName - Field identifier
   */
  clearFieldError(fieldName: string): void {
    try {
      const field = this.getFieldRef(fieldName);
      field.clearFieldError();
      logger.debug(`[FieldUtility] Cleared error for ${fieldName}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error clearing field error: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Set items for select/dropdown field
   * @param fieldName - Field identifier
   * @param items - Array of items
   */
  setItems(fieldName: string, items: any[]): void {
    try {
      const field = this.getFieldRef(fieldName);
      
      if (!Array.isArray(items)) {
        throw new Error('Items must be an array');
      }
      // Support multiple component APIs: prefer setElements, fallback to updateItems or setItems
      const setterCandidates = [
        (field as any).setElements,
        (field as any).updateItems,
        (field as any).setItems,
      ];

      const setter = setterCandidates.find((fn) => typeof fn === 'function');
      if (!setter) {
        const available = Object.keys(field).join(', ');
        throw new Error(
          `Field ${fieldName} does not support setting elements (no setElements/updateItems/setItems). Available: ${available}`
        );
      }

      // Call the discovered setter
      setter.call(field, items);
      logger.debug(`[FieldUtility] Set items for ${fieldName} using ${setter.name}: ${this.stringifyForLog(items)}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error setting items: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }

  /**
   * Check if field is valid
   * @param fieldName - Field identifier
   * @returns True if valid, false otherwise
   */
  isFieldValid(fieldName: string): boolean {
    try {
      const field = this.getFieldRef(fieldName);
      const isValid = !field.errorState?.errorMessage;
      logger.debug(`[FieldUtility] Field ${fieldName} valid: ${isValid}`);
      return isValid;
    } catch (error) {
      logger.error(`[FieldUtility] Error checking field validity: ${this.stringifyForLog(error)}`);
      return false;
    }
  }

  /**
   * Set documents for a file/document field
   * @param fieldName - Field identifier
   * @param documents - Array of documents or document list
   */
  setDocuments(fieldName: string, documents: any): void {
    try {
      const field = this.getFieldRef(fieldName);
      
      if (!field.setDocuments) {
        throw new Error(`Field ${fieldName} does not support document operations`);
      }
      
      field.setDocuments(documents);
      logger.debug(`[FieldUtility] Set documents for ${fieldName}: ${this.stringifyForLog(documents)}`);
    } catch (error) {
      logger.error(`[FieldUtility] Error setting documents: ${this.stringifyForLog(error)}`);
      throw error;
    }
  }
}

/**
 * Global field utility instance
 * Available in Blockly-generated code as: fieldUtility.showField("CF_NAME")
 */
export const fieldUtility = new FieldUtility();

/**
 * Initialize field utility in execution context
 * Call this in ComponentForm.vue before executing Blockly code
 */
export function initFieldUtility(app: any, store: any) {
  (fieldUtility as any).app = app;
  (fieldUtility as any).store = store;
}
