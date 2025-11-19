/**
 * System Utility Functions
 *
 * Provides safe, abstracted access to system/browser operations for Blockly-generated code.
 * Includes window operations, navigation, and other system-level functionality.
 *
 * @module systemUtility
 */

import { logger } from "@/api/api";

/**
 * System Utility Class
 * Provides safe system operation methods
 */
export class SystemUtility {
  constructor() {
    // No initialization needed for system operations
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
   * Reload the parent window after a specified delay
   * @param delay - Delay in milliseconds before reload (default: 500ms)
   */
  reloadWindow(delay: number = 500): void {
    try {
      if (typeof delay !== "number" || delay < 0) {
        logger.warn(
          `[SystemUtility] Invalid delay value: ${delay}, using default 500ms`
        );
        delay = 500;
      }

      logger.debug(`[SystemUtility] Scheduling window reload in ${delay}ms`);

      setTimeout(() => {
        try {
          if (window.parent && window.parent.location) {
            window.parent.location.reload();
          } else {
            // Fallback to current window if parent is not available
            window.location.reload();
          }
        } catch (error) {
          logger.error(
            `[SystemUtility] Error reloading window: ${this.stringifyForLog(
              error
            )}`
          );
          // Try fallback reload
          try {
            window.location.reload();
          } catch (fallbackError) {
            logger.error(
              `[SystemUtility] Fallback reload also failed: ${this.stringifyForLog(
                fallbackError
              )}`
            );
          }
        }
      }, delay);
    } catch (error) {
      logger.error(
        `[SystemUtility] Error scheduling window reload: ${this.stringifyForLog(
          error
        )}`
      );
    }
  }

  /**
   * Navigate to a new URL
   * @param url - The URL to navigate to
   * @param target - Target window ('_self', '_parent', '_top', '_blank')
   */
  navigate(url: string, target: string = "_self"): void {
    try {
      if (!url || typeof url !== "string") {
        logger.error(`[SystemUtility] Invalid URL provided: ${url}`);
        return;
      }

      logger.debug(`[SystemUtility] Navigating to: ${url} (target: ${target})`);

      switch (target) {
        case "_parent":
          if (window.parent) {
            window.parent.location.href = url;
          } else {
            window.location.href = url;
          }
          break;
        case "_top":
          if (window.top) {
            window.top.location.href = url;
          } else {
            window.location.href = url;
          }
          break;
        case "_blank":
          window.open(url, "_blank");
          break;
        case "_self":
        default:
          window.location.href = url;
          break;
      }
    } catch (error) {
      logger.error(
        `[SystemUtility] Error navigating to ${url}: ${this.stringifyForLog(
          error
        )}`
      );
    }
  }

  /**
   * Open URL in a new window/tab
   * @param url - The URL to open
   * @param windowName - Optional window name
   * @param features - Optional window features (size, position, etc.)
   */
  openWindow(
    url: string,
    windowName?: string,
    features?: string
  ): Window | null {
    try {
      if (!url || typeof url !== "string") {
        logger.error(`[SystemUtility] Invalid URL provided: ${url}`);
        return null;
      }

      logger.debug(`[SystemUtility] Opening window: ${url}`);
      return window.open(url, windowName, features);
    } catch (error) {
      logger.error(
        `[SystemUtility] Error opening window ${url}: ${this.stringifyForLog(
          error
        )}`
      );
      return null;
    }
  }

  /**
   * Get current window information
   * @returns Object with window properties
   */
  getWindowInfo(): {
    url: string;
    title: string;
    width: number;
    height: number;
    userAgent: string;
  } {
    try {
      return {
        url: window.location.href,
        title: document.title,
        width: window.innerWidth,
        height: window.innerHeight,
        userAgent: navigator.userAgent,
      };
    } catch (error) {
      logger.error(
        `[SystemUtility] Error getting window info: ${this.stringifyForLog(
          error
        )}`
      );
      return {
        url: "",
        title: "",
        width: 0,
        height: 0,
        userAgent: "",
      };
    }
  }

  /**
   * Show browser alert dialog
   * @param message - Alert message to display
   */
  alert(message: string): void {
    try {
      if (typeof message !== "string") {
        message = String(message);
      }
      logger.debug(`[SystemUtility] Showing alert: ${message}`);
      alert(message);
    } catch (error) {
      logger.error(
        `[SystemUtility] Error showing alert: ${this.stringifyForLog(error)}`
      );
    }
  }

  /**
   * Show browser confirm dialog
   * @param message - Confirm message to display
   * @returns Boolean result of user choice
   */
  confirm(message: string): boolean {
    try {
      if (typeof message !== "string") {
        message = String(message);
      }
      logger.debug(`[SystemUtility] Showing confirm: ${message}`);
      return confirm(message);
    } catch (error) {
      logger.error(
        `[SystemUtility] Error showing confirm: ${this.stringifyForLog(error)}`
      );
      return false;
    }
  }

  /**
   * Copy text to clipboard
   * @param text - Text to copy to clipboard
   * @returns Promise<boolean> - Success status
   */
  async copyToClipboard(text: string): Promise<boolean> {
    try {
      if (typeof text !== "string") {
        text = String(text);
      }

      if (navigator.clipboard && navigator.clipboard.writeText) {
        await navigator.clipboard.writeText(text);
        logger.debug(
          `[SystemUtility] Copied to clipboard: ${text.substring(0, 50)}...`
        );
        return true;
      } else {
        // Fallback for older browsers
        const textArea = document.createElement("textarea");
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        const success = document.execCommand("copy");
        document.body.removeChild(textArea);

        if (success) {
          logger.debug(
            `[SystemUtility] Copied to clipboard (fallback): ${text.substring(
              0,
              50
            )}...`
          );
        }
        return success;
      }
    } catch (error) {
      logger.error(
        `[SystemUtility] Error copying to clipboard: ${this.stringifyForLog(
          error
        )}`
      );
      return false;
    }
  }
}

/**
 * Global system utility instance
 * Available in Blockly-generated code as: systemUtility.reloadWindow(...)
 */
export const systemUtility = new SystemUtility();
