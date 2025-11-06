/**
 * Blockly Utilities Index
 *
 * Central export point for all Blockly utility functions.
 * These utilities provide a safe, abstracted API for Blockly-generated code.
 *
 * @module blocklyUtilities
 */

// Import utilities
import { fieldUtility, initFieldUtility } from "./fieldUtility";
import { stringUtility } from "./stringUtility";
import { mathUtility } from "./mathUtility";
import { arrayUtility } from "./arrayUtility";
import { sectionUtility, initSectionUtility } from "./sectionUtility";
import { eliseUtility, initEliseUtility } from "./eliseUtility";

// Export all utilities
export * from "./fieldUtility";
export * from "./stringUtility";
export * from "./mathUtility";
export * from "./arrayUtility";
export * from "./sectionUtility";
export * from "./eliseUtility";

// Re-export utility instances
export {
  fieldUtility,
  initFieldUtility,
  stringUtility,
  mathUtility,
  arrayUtility,
  sectionUtility,
  initSectionUtility,
  eliseUtility,
  initEliseUtility,
};

/**
 * Initialize all utilities that require context
 *
 * This function should be called once during application initialization
 * to provide all utilities with necessary context (app, store, etc.)
 *
 * @param context - Context object containing app, store, and other dependencies
 * @param context.app - Vue app instance
 * @param context.store - Pinia store instance
 */
export function initializeBlocklyUtilities(context: {
  app: any;
  store: any;
}): void {
  const { app, store } = context;

  // Initialize utilities that need context
  initFieldUtility(app, store);
  initSectionUtility(app, store);
  initEliseUtility(store);
}

/**
 * Get all utility instances as an object
 *
 * This is useful for passing utilities to code execution contexts
 *
 * @returns Object containing all utility instances
 */
export function getAllUtilities() {
  return {
    fieldUtility,
    stringUtility,
    mathUtility,
    arrayUtility,
    sectionUtility,
    eliseUtility,
  };
}
