/**
 * String Utility Functions
 *
 * Provides safe string manipulation functions for Blockly-generated code.
 * All functions are sandboxed and validated.
 *
 * @module stringUtility
 */

import { logger } from "@/api/api";

/**
 * String Utility Class
 * Provides safe string operations
 */
export class StringUtility {
  /**
   * Convert string to uppercase
   * @param text - Input string
   * @returns Uppercase string
   */
  toUpperCase(text: string): string {
    if (typeof text !== "string") {
      return String(text).toUpperCase();
    }
    return text.toUpperCase();
  }

  /**
   * Convert string to lowercase
   * @param text - Input string
   * @returns Lowercase string
   */
  toLowerCase(text: string): string {
    if (typeof text !== "string") {
      return String(text).toLowerCase();
    }
    return text.toLowerCase();
  }

  /**
   * Capitalize first letter
   * @param text - Input string
   * @returns Capitalized string
   */
  capitalize(text: string): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    if (text.length === 0) return text;
    return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
  }

  /**
   * Trim whitespace
   * @param text - Input string
   * @returns Trimmed string
   */
  trim(text: string): string {
    if (typeof text !== "string") {
      return String(text).trim();
    }
    return text.trim();
  }

  /**
   * Get string length
   * @param text - Input string
   * @returns Length of string
   */
  length(text: string): number {
    if (typeof text !== "string") {
      return String(text).length;
    }
    return text.length;
  }

  /**
   * Check if string contains substring
   * @param text - Input string
   * @param searchText - Text to search for
   * @returns True if contains, false otherwise
   */
  contains(text: string, searchText: string): boolean {
    if (typeof text !== "string") {
      text = String(text);
    }
    if (typeof searchText !== "string") {
      searchText = String(searchText);
    }
    return text.includes(searchText);
  }

  /**
   * Replace text in string
   * @param text - Input string
   * @param searchText - Text to search for
   * @param replaceText - Text to replace with
   * @returns Modified string
   */
  replace(text: string, searchText: string, replaceText: string): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.replace(searchText, replaceText);
  }

  /**
   * Replace all occurrences
   * @param text - Input string
   * @param searchText - Text to search for
   * @param replaceText - Text to replace with
   * @returns Modified string
   */
  replaceAll(text: string, searchText: string, replaceText: string): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.split(searchText).join(replaceText);
  }

  /**
   * Split string by delimiter
   * @param text - Input string
   * @param delimiter - Delimiter to split by
   * @returns Array of strings
   */
  split(text: string, delimiter: string): string[] {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.split(delimiter);
  }

  /**
   * Get substring
   * @param text - Input string
   * @param start - Start index
   * @param length - Length of substring (optional)
   * @returns Substring
   */
  substring(text: string, start: number, length?: number): string {
    if (typeof text !== "string") {
      text = String(text);
    }

    if (length === undefined) {
      return text.substring(start);
    }
    return text.substring(start, start + length);
  }

  /**
   * Concatenate strings
   * @param strings - Strings to concatenate
   * @returns Concatenated string
   */
  concat(...strings: any[]): string {
    return strings.map((s) => String(s)).join("");
  }

  /**
   * Join array with delimiter
   * @param array - Array of items
   * @param delimiter - Delimiter to join with
   * @returns Joined string
   */
  join(array: any[], delimiter: string = ""): string {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.map((item) => String(item)).join(delimiter);
  }

  /**
   * Check if string starts with prefix
   * @param text - Input string
   * @param prefix - Prefix to check
   * @returns True if starts with prefix
   */
  startsWith(text: string, prefix: string): boolean {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.startsWith(prefix);
  }

  /**
   * Check if string ends with suffix
   * @param text - Input string
   * @param suffix - Suffix to check
   * @returns True if ends with suffix
   */
  endsWith(text: string, suffix: string): boolean {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.endsWith(suffix);
  }

  /**
   * Pad string to length
   * @param text - Input string
   * @param length - Target length
   * @param padChar - Character to pad with (default: space)
   * @returns Padded string
   */
  padStart(text: string, length: number, padChar: string = " "): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.padStart(length, padChar);
  }

  /**
   * Pad string to length (end)
   * @param text - Input string
   * @param length - Target length
   * @param padChar - Character to pad with (default: space)
   * @returns Padded string
   */
  padEnd(text: string, length: number, padChar: string = " "): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.padEnd(length, padChar);
  }

  /**
   * Repeat string n times
   * @param text - Input string
   * @param count - Number of repetitions
   * @returns Repeated string
   */
  repeat(text: string, count: number): string {
    if (typeof text !== "string") {
      text = String(text);
    }
    if (count < 0 || count > 1000) {
      throw new Error("Repeat count must be between 0 and 1000");
    }
    return text.repeat(count);
  }

  /**
   * Check if string is empty or whitespace
   * @param text - Input string
   * @returns True if empty or whitespace
   */
  isEmpty(text: string): boolean {
    if (typeof text !== "string") {
      text = String(text);
    }
    return text.trim().length === 0;
  }

  /**
   * Format string with placeholders
   * @param template - Template string with {0}, {1}, etc.
   * @param values - Values to insert
   * @returns Formatted string
   */
  format(template: string, ...values: any[]): string {
    if (typeof template !== "string") {
      template = String(template);
    }

    return template.replace(/\{(\d+)\}/g, (match, index) => {
      const idx = parseInt(index);
      return idx < values.length ? String(values[idx]) : match;
    });
  }
}

/**
 * Global string utility instance
 * Available in Blockly-generated code as: stringUtility.toUpperCase("hello")
 */
export const stringUtility = new StringUtility();
