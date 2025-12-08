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
        throw new Error(
          "[FieldUtility] App instance not available. Call initFieldUtility() before using field operations."
        );
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
        throw new Error(
          "[FieldUtility] Store not available. Call initFieldUtility() with store instance before using field operations."
        );
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
      if (typeof value === "string") return value;
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
    console.log("[FieldUtility] Validating field name:", fieldName);
    if (!fieldName || typeof fieldName !== "string") {
      throw new Error("Field name must be a non-empty string");
    }

    // System column names allowed as exception
    const systemColumns = [
      "id",
      "column_name",
      "description",
      "taille",
      "show",
      "unique",
      "sortable",
      "filters",
      "identifiant",
    ];
    if (systemColumns.includes(fieldName)) {
      return true;
    }

    // Only allow CF_, TBL_, or COL_ prefix + alphanumeric + underscore (case-insensitive)
    if (!/^(ZR_|CF_|CTF_|TBL_|COL_)[A-Z0-9_]+$/i.test(fieldName)) {
      throw new Error(
        `Invalid field name format: ${fieldName}. Expected format: CF_FIELDNAME, TBL_FIELDNAME, or COL_FIELDNAME, or a system column name.`
      );
    }

    return true;
  }

  /**
   * Get field reference safely
   * Searches in accumulated refs from all initialized components
   * @private
   */
  private getFieldRef(fieldName: string): any {
    this.validateFieldName(fieldName);

    const app = this.getApp();
    console.log("[FieldUtility] App refs:", app?.refs);
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
      logger.error(
        `[FieldUtility] Error showing field ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.error(
        `[FieldUtility] Error hiding field ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.error(
        `[FieldUtility] Error enabling field ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.error(
        `[FieldUtility] Error disabling field ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.debug(
        `[FieldUtility] Got value for ${fieldName}: ${this.stringifyForLog(
          value
        )}`
      );
      return value;
    } catch (error) {
      logger.error(
        `[FieldUtility] Error getting value for ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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

      // Process string values to convert literal \n to actual line breaks
      let processedValue = value;
      if (typeof value === "string") {
        processedValue = value.replace(/\\n/g, "\n");
      }

      // Special handling for ZR_ fields: extract row array if value is an object with row property
      if (
        fieldName.startsWith("ZR_") &&
        value &&
        typeof value === "object" &&
        value.row &&
        Array.isArray(value.row)
      ) {
        processedValue = value.row;
        logger.debug(
          `[FieldUtility] Extracted row array from ZR_ field ${fieldName}: ${this.stringifyForLog(
            processedValue
          )}`
        );
      }

      // Try to get field reference and use component's setValue if available
      try {
        const field = this.getFieldRef(fieldName);

        // Support multiple component APIs: prefer setValue, fallback to updateField or direct assignment
        const setterCandidates = [
          (field as any).setValue,
          (field as any).updateField,
        ];

        const setter = setterCandidates.find((fn) => typeof fn === "function");
        if (setter) {
          // Call the discovered setter on the component
          setter.call(field, processedValue);
          logger.debug(
            `[FieldUtility] Set value for ${fieldName} using ${
              setter.name
            }: ${this.stringifyForLog(processedValue)}`
          );
          return;
        }
      } catch (fieldError) {
        // Field not found or doesn't have setValue method, fallback to store
        logger.debug(
          `[FieldUtility] Field ${fieldName} not found or no setValue method, using store fallback`
        );
      }

      // Fallback: Set via store Fields (normal affectation)
      const store = this.getStore();
      store.Fields[fieldName] = processedValue;
      logger.debug(
        `[FieldUtility] Set value for ${fieldName} via store: ${this.stringifyForLog(
          processedValue
        )}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error setting value for ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.debug(
        `[FieldUtility] Copied value from ${sourceField} to ${targetField}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error copying field value: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.debug(
        `[FieldUtility] Set error for ${fieldName}: ${errorMessage}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error setting field error: ${this.stringifyForLog(
          error
        )}`
      );
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
      logger.error(
        `[FieldUtility] Error clearing field error: ${this.stringifyForLog(
          error
        )}`
      );
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
        throw new Error("Items must be an array");
      }
      // Support multiple component APIs: prefer setElements, fallback to updateItems or setItems
      const setterCandidates = [
        (field as any).setElements,
        (field as any).updateItems,
        (field as any).setItems,
      ];

      const setter = setterCandidates.find((fn) => typeof fn === "function");
      if (!setter) {
        const available = Object.keys(field).join(", ");
        throw new Error(
          `Field ${fieldName} does not support setting elements (no setElements/updateItems/setItems). Available: ${available}`
        );
      }

      // Call the discovered setter
      setter.call(field, items);
      logger.debug(
        `[FieldUtility] Set items for ${fieldName} using ${
          setter.name
        }: ${this.stringifyForLog(items)}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error setting items: ${this.stringifyForLog(error)}`
      );
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
      logger.error(
        `[FieldUtility] Error checking field validity: ${this.stringifyForLog(
          error
        )}`
      );
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
        throw new Error(
          `Field ${fieldName} does not support document operations`
        );
      }

      field.setDocuments(documents);
      logger.debug(
        `[FieldUtility] Set documents for ${fieldName}: ${this.stringifyForLog(
          documents
        )}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error setting documents: ${this.stringifyForLog(error)}`
      );
      throw error;
    }
  }

  /**
   * Replace variables in vhtml element template
   * @param fieldName - Field identifier containing the vhtml element
   * @param variables - Array of {key, value} pairs to replace in template
   */
  replaceVariablesInVhtml(
    fieldName: string,
    variables: Array<{ key: string; value: any }>
  ): void {
    try {
      this.validateFieldName(fieldName);
      const field = this.getFieldRef(fieldName);

      // Get current content from the component
      let currentContent = field.getValue();

      // If no current content, try to get from options
      if (!currentContent && field.options?.content) {
        currentContent = field.options.content;
      }

      // Store original template on first use (in component's options)
      if (!field.options.originalTemplate) {
        field.options.originalTemplate = currentContent || "";
      }

      // Replace all variables in template
      const updatedContent = variables.reduce(
        (content, variable) =>
          content.replace(
            new RegExp("\\{\\{" + variable.key + "\\}\\}", "g"),
            variable.value
          ),
        field.options.originalTemplate
      );

      // Update the component using its setValue method
      field.setValue(updatedContent);

      logger.debug(
        `[FieldUtility] Replaced variables in ${fieldName} vhtml template`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error replacing variables in vhtml: ${this.stringifyForLog(
          error
        )}`
      );
      throw error;
    }
  }

  /**
   * Get QR code image from QR code field
   * @param fieldName - Field identifier for the QR code component
   * @returns Promise<string | null> - Base64 image string or null
   */
  async getQRCodeImage(fieldName: string): Promise<string | null> {
    try {
      this.validateFieldName(fieldName);
      const app = this.getApp();
      const field = app.refs[fieldName];

      if (!field || !field[0]) {
        logger.warn(`[FieldUtility] QR code field ${fieldName} not found`);
        return null;
      }

      if (!field[0].getImage) {
        throw new Error(
          `Field ${fieldName} does not support getImage() method`
        );
      }

      const image = await field[0].getImage();
      logger.debug(`[FieldUtility] Got QR code image from ${fieldName}`);
      return image;
    } catch (error) {
      logger.error(
        `[FieldUtility] Error getting QR code image: ${this.stringifyForLog(
          error
        )}`
      );
      return null;
    }
  }

  /**
   * Change thesaurus ID for a NeoThesaurus field and launch search
   * @param fieldName - Field identifier for the NeoThesaurus component
   * @param newThesaurusId - New thesaurus ID to set
   */
  changeThesaurusId(fieldName: string, newThesaurusId: string): void {
    try {
      this.validateFieldName(fieldName);

      if (!newThesaurusId || typeof newThesaurusId !== "string") {
        throw new Error("Thesaurus ID must be a non-empty string");
      }

      const field = this.getFieldRef(fieldName);

      if (!field.changeThesaurusId) {
        throw new Error(
          `Field ${fieldName} does not support changeThesaurusId() method (not a NeoThesaurus component)`
        );
      }

      field.changeThesaurusId(newThesaurusId);
      logger.debug(
        `[FieldUtility] Changed thesaurus ID for ${fieldName} to: ${newThesaurusId}`
      );
    } catch (error) {
      logger.error(
        `[FieldUtility] Error changing thesaurus ID for ${fieldName}: ${this.stringifyForLog(
          error
        )}`
      );
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
 * Accumulates refs from multiple component initializations instead of overriding
 * @param app - Local app instance (current component)
 * @param store - Store instance
 */
export function initFieldUtility(app: any, store: any) {
  // ADD refs to existing ones instead of overriding
  if ((fieldUtility as any).app?.refs && app?.refs) {
    // Save existing refs (from parent components like ComponentForm)
    const existingRefs = { ...(fieldUtility as any).app.refs };
    // Set new app instance (from current component like NeoTable)
    (fieldUtility as any).app = app;
    // Merge: existing refs first, then current refs (current component refs take precedence)
    Object.assign((fieldUtility as any).app.refs, existingRefs, app.refs);
  } else {
    // First initialization
    (fieldUtility as any).app = app;
  }
  (fieldUtility as any).store = store;
}
