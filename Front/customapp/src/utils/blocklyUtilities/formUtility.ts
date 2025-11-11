/**
 * Form Utility Functions
 *
 * Provides safe, abstracted access to form-level operations.
 * Hides internal implementation details (formID, currentFormId, initForm, etc.)
 *
 * @module formUtility
 */

import { logger } from "@/api/api";

/**
 * Form Utility Class
 * Provides safe form operation methods
 */
export class FormUtility {
  private formID: any = null;
  private currentFormId: any = null;
  private initForm: any = null;

  constructor() {
    // Will be initialized via initFormUtility
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
   * Get form ID reference
   * @private
   */
  private getFormID(): any {
    if (!this.formID) {
      throw new Error(
        "[FormUtility] Form ID not available. Call initFormUtility() before using form operations."
      );
    }
    return this.formID;
  }

  /**
   * Get current form ID reference
   * @private
   */
  private getCurrentFormId(): any {
    if (!this.currentFormId) {
      throw new Error(
        "[FormUtility] Current Form ID not available. Call initFormUtility() before using form operations."
      );
    }
    return this.currentFormId;
  }

  /**
   * Get initForm function
   * @private
   */
  private getInitForm(): any {
    if (!this.initForm) {
      throw new Error(
        "[FormUtility] InitForm function not available. Call initFormUtility() before using form operations."
      );
    }
    return this.initForm;
  }

  /**
   * Set form ID and initialize form if ID changed
   * @param newFormId - New form identifier
   * @returns Promise<void>
   */
  async setFormIdAndInit(newFormId: string): Promise<void> {
    try {
      if (!newFormId || newFormId === null || newFormId === undefined) {
        logger.warn("[FormUtility] Invalid form ID provided, skipping init");
        return;
      }

      const formID = this.getFormID();
      const currentFormId = this.getCurrentFormId();

      if (newFormId !== currentFormId.value) {
        formID.value = newFormId;
        const initForm = this.getInitForm();
        await initForm();
        logger.info(
          `[FormUtility] Form ID changed to ${newFormId} and initialized`
        );
      } else {
        logger.debug("[FormUtility] Form ID unchanged, skipping initForm");
      }
    } catch (error) {
      logger.error(
        `[FormUtility] Error setting form ID and initializing: ${this.stringifyForLog(
          error
        )}`
      );
      throw error;
    }
  }
}

/**
 * Global form utility instance
 * Available in Blockly-generated code as: formUtility.setFormIdAndInit(...)
 */
export const formUtility = new FormUtility();

/**
 * Initialize form utility in execution context
 * Call this in ComponentForm.vue before executing Blockly code
 */
export function initFormUtility(context: {
  formID: any;
  currentFormId: any;
  initForm: any;
}) {
  (formUtility as any).formID = context.formID;
  (formUtility as any).currentFormId = context.currentFormId;
  (formUtility as any).initForm = context.initForm;
}
