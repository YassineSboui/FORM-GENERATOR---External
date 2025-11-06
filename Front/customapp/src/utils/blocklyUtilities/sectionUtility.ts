/**
 * Section Utility Functions
 * 
 * Provides safe section manipulation functions for Blockly-generated code.
 * Handles section visibility and state management.
 * 
 * @module sectionUtility
 */

import { logger } from "@/api/api";

/**
 * Section Utility Class
 * Provides safe section operations
 */
export class SectionUtility {
  private app: any = null;
  private store: any = null;

  /**
   * Initialize section utility with app and store context
   * Must be called before using any section methods
   * @param app - Vue app instance
   * @param store - Pinia store instance
   */
  init(app: any, store: any): void {
    this.app = app;
    this.store = store;
  }

  /**
   * Validate section name format
   * @param sectionName - Section name to validate
   * @returns True if valid
   */
  private validateSectionName(sectionName: string): boolean {
    if (!sectionName || typeof sectionName !== 'string') {
      logger.error('Section name must be a non-empty string');
      return false;
    }
    
    // Section names should follow ZR_XXX ou ZS_XXX  pattern
    const pattern = /^(ZR|ZS)_[A-Z0-9_]+$/;
    if (!pattern.test(sectionName)) {
      logger.error(`Invalid section name format: ${sectionName}. Expected: ZR_XXX ou ZS_XXX`);
      return false;
    }
    
    return true;
  }

  /**
   * Get section reference
   * @param sectionName - Section name
   * @returns Section reference
   */
  private getSectionRef(sectionName: string): any {
    if (!this.app || !this.app.refs) {
      throw new Error('Section utility not initialized. Call initSectionUtility() first.');
    }

    if (!this.validateSectionName(sectionName)) {
      throw new Error(`Invalid section name: ${sectionName}`);
    }

    const sectionRef = this.app.refs[sectionName];
    if (!sectionRef) {
      throw new Error(`Section not found: ${sectionName}`);
    }

    // Sections are stored in arrays like fields
    if (Array.isArray(sectionRef) && sectionRef.length > 0) {
      return sectionRef[0];
    }

    return sectionRef;
  }

  /**
   * Show section
   * @param sectionName - Section name to show
   */
  showSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.showSection === 'function') {
        section.showSection();
        logger.debug(`Section shown: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support showSection method`);
      }
    } catch (error) {
      logger.error(`Error showing section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Hide section
   * @param sectionName - Section name to hide
   */
  hideSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.hideSection === 'function') {
        section.hideSection();
        logger.debug(`Section hidden: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support hideSection method`);
      }
    } catch (error) {
      logger.error(`Error hiding section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Toggle section visibility
   * @param sectionName - Section name to toggle
   */
  toggleSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.toggleSection === 'function') {
        section.toggleSection();
        logger.debug(`Section toggled: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support toggleSection method`);
      }
    } catch (error) {
      logger.error(`Error toggling section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Check if section is visible
   * @param sectionName - Section name to check
   * @returns True if visible
   */
  isVisible(sectionName: string): boolean {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.isVisible === 'function') {
        return section.isVisible();
      } else if (section.visible !== undefined) {
        return section.visible;
      } else {
        throw new Error(`Section ${sectionName} does not support visibility check`);
      }
    } catch (error) {
      logger.error(`Error checking section visibility ${sectionName}: ${error}`);
      return false;
    }
  }

  /**
   * Expand section (for collapsible sections)
   * @param sectionName - Section name to expand
   */
  expandSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.expand === 'function') {
        section.expand();
        logger.debug(`Section expanded: ${sectionName}`);
      } else if (typeof section.setExpanded === 'function') {
        section.setExpanded(true);
        logger.debug(`Section expanded: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support expand method`);
      }
    } catch (error) {
      logger.error(`Error expanding section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Collapse section (for collapsible sections)
   * @param sectionName - Section name to collapse
   */
  collapseSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.collapse === 'function') {
        section.collapse();
        logger.debug(`Section collapsed: ${sectionName}`);
      } else if (typeof section.setExpanded === 'function') {
        section.setExpanded(false);
        logger.debug(`Section collapsed: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support collapse method`);
      }
    } catch (error) {
      logger.error(`Error collapsing section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Check if section is expanded
   * @param sectionName - Section name to check
   * @returns True if expanded
   */
  isExpanded(sectionName: string): boolean {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.isExpanded === 'function') {
        return section.isExpanded();
      } else if (section.expanded !== undefined) {
        return section.expanded;
      } else {
        throw new Error(`Section ${sectionName} does not support expansion check`);
      }
    } catch (error) {
      logger.error(`Error checking section expansion ${sectionName}: ${error}`);
      return false;
    }
  }

  /**
   * Enable section
   * @param sectionName - Section name to enable
   */
  enableSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.enable === 'function') {
        section.enable();
        logger.debug(`Section enabled: ${sectionName}`);
      } else if (section.disabled !== undefined) {
        section.disabled = false;
        logger.debug(`Section enabled: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support enable method`);
      }
    } catch (error) {
      logger.error(`Error enabling section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Disable section
   * @param sectionName - Section name to disable
   */
  disableSection(sectionName: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.disable === 'function') {
        section.disable();
        logger.debug(`Section disabled: ${sectionName}`);
      } else if (section.disabled !== undefined) {
        section.disabled = true;
        logger.debug(`Section disabled: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support disable method`);
      }
    } catch (error) {
      logger.error(`Error disabling section ${sectionName}: ${error}`);
      throw error;
    }
  }

  /**
   * Check if section is enabled
   * @param sectionName - Section name to check
   * @returns True if enabled
   */
  isEnabled(sectionName: string): boolean {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (typeof section.isEnabled === 'function') {
        return section.isEnabled();
      } else if (section.disabled !== undefined) {
        return !section.disabled;
      } else {
        throw new Error(`Section ${sectionName} does not support enabled check`);
      }
    } catch (error) {
      logger.error(`Error checking section enabled state ${sectionName}: ${error}`);
      return false;
    }
  }

  /**
   * Get section title
   * @param sectionName - Section name
   * @returns Section title
   */
  getTitle(sectionName: string): string {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (section.title !== undefined) {
        return section.title;
      } else if (section.label !== undefined) {
        return section.label;
      } else {
        throw new Error(`Section ${sectionName} does not have a title property`);
      }
    } catch (error) {
      logger.error(`Error getting section title ${sectionName}: ${error}`);
      return '';
    }
  }

  /**
   * Set section title
   * @param sectionName - Section name
   * @param title - New title
   */
  setTitle(sectionName: string, title: string): void {
    try {
      const section = this.getSectionRef(sectionName);
      
      if (section.title !== undefined) {
        section.title = title;
        logger.debug(`Section title updated: ${sectionName}`);
      } else if (section.label !== undefined) {
        section.label = title;
        logger.debug(`Section label updated: ${sectionName}`);
      } else {
        throw new Error(`Section ${sectionName} does not support title update`);
      }
    } catch (error) {
      logger.error(`Error setting section title ${sectionName}: ${error}`);
      throw error;
    }
  }
}

/**
 * Global section utility instance
 * Available in Blockly-generated code as: sectionUtility.showSection("CF_SECTION_XXX")
 */
export const sectionUtility = new SectionUtility();

/**
 * Initialize section utility with app and store context
 * Should be called once during application initialization
 * @param app - Vue app instance
 * @param store - Pinia store instance
 */
export function initSectionUtility(app: any, store: any): void {
  sectionUtility.init(app, store);
}
