/**
 * Array Utility Functions
 *
 * Provides safe array manipulation functions for Blockly-generated code.
 * All functions are sandboxed and validated.
 *
 * @module arrayUtility
 */

import { logger } from "@/api/api";

/**
 * Array Utility Class
 * Provides safe array operations
 */
export class ArrayUtility {
  /**
   * Get array length
   * @param array - Input array
   * @returns Length of array
   */
  length(array: any[]): number {
    if (!Array.isArray(array)) {
      throw new Error("Parameter must be an array");
    }
    return array.length;
  }

  /**
   * Check if array is empty
   * @param array - Input array
   * @returns True if empty
   */
  isEmpty(array: any[]): boolean {
    if (!Array.isArray(array)) {
      throw new Error("Parameter must be an array");
    }
    return array.length === 0;
  }

  /**
   * Get element at index
   * @param array - Input array
   * @param index - Index to get
   * @returns Element at index
   */
  getAt(array: any[], index: number): any {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (index < 0 || index >= array.length) {
      throw new Error(
        `Index ${index} out of bounds for array of length ${array.length}`
      );
    }
    return array[index];
  }

  /**
   * Set element at index
   * @param array - Input array
   * @param index - Index to set
   * @param value - Value to set
   */
  setAt(array: any[], index: number, value: any): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (index < 0 || index >= array.length) {
      throw new Error(
        `Index ${index} out of bounds for array of length ${array.length}`
      );
    }
    array[index] = value;
  }

  /**
   * Add element to end of array
   * @param array - Input array
   * @param value - Value to add
   */
  push(array: any[], value: any): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    array.push(value);
  }

  /**
   * Remove and return last element
   * @param array - Input array
   * @returns Last element
   */
  pop(array: any[]): any {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (array.length === 0) {
      throw new Error("Cannot pop from empty array");
    }
    return array.pop();
  }

  /**
   * Add element to beginning of array
   * @param array - Input array
   * @param value - Value to add
   */
  unshift(array: any[], value: any): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    array.unshift(value);
  }

  /**
   * Remove and return first element
   * @param array - Input array
   * @returns First element
   */
  shift(array: any[]): any {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (array.length === 0) {
      throw new Error("Cannot shift from empty array");
    }
    return array.shift();
  }

  /**
   * Find index of element
   * @param array - Input array
   * @param value - Value to find
   * @returns Index or -1 if not found
   */
  indexOf(array: any[], value: any): number {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.indexOf(value);
  }

  /**
   * Check if array contains value
   * @param array - Input array
   * @param value - Value to check
   * @returns True if contains value
   */
  contains(array: any[], value: any): boolean {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.includes(value);
  }

  /**
   * Remove element at index
   * @param array - Input array
   * @param index - Index to remove
   */
  removeAt(array: any[], index: number): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (index < 0 || index >= array.length) {
      throw new Error(
        `Index ${index} out of bounds for array of length ${array.length}`
      );
    }
    array.splice(index, 1);
  }

  /**
   * Remove first occurrence of value
   * @param array - Input array
   * @param value - Value to remove
   * @returns True if removed, false if not found
   */
  remove(array: any[], value: any): boolean {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    const index = array.indexOf(value);
    if (index !== -1) {
      array.splice(index, 1);
      return true;
    }
    return false;
  }

  /**
   * Clear all elements from array
   * @param array - Input array
   */
  clear(array: any[]): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    array.length = 0;
  }

  /**
   * Reverse array in place
   * @param array - Input array
   */
  reverse(array: any[]): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    array.reverse();
  }

  /**
   * Sort array in place
   * @param array - Input array
   * @param ascending - Sort direction (default: true)
   */
  sort(array: any[], ascending: boolean = true): void {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    array.sort((a, b) => {
      if (ascending) {
        return a < b ? -1 : a > b ? 1 : 0;
      } else {
        return a > b ? -1 : a < b ? 1 : 0;
      }
    });
  }

  /**
   * Get slice of array
   * @param array - Input array
   * @param start - Start index
   * @param end - End index (optional)
   * @returns Sliced array
   */
  slice(array: any[], start: number, end?: number): any[] {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.slice(start, end);
  }

  /**
   * Concatenate arrays
   * @param arrays - Arrays to concatenate
   * @returns Concatenated array
   */
  concat(...arrays: any[][]): any[] {
    const result: any[] = [];
    for (const arr of arrays) {
      if (!Array.isArray(arr)) {
        throw new Error("All parameters must be arrays");
      }
      result.push(...arr);
    }
    return result;
  }

  /**
   * Filter array by condition
   * @param array - Input array
   * @param property - Property to check
   * @param value - Value to match
   * @returns Filtered array
   */
  filter(array: any[], property: string, value: any): any[] {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.filter((item) => {
      if (typeof item === "object" && item !== null) {
        return item[property] === value;
      }
      return item === value;
    });
  }

  /**
   * Find first element matching condition
   * @param array - Input array
   * @param property - Property to check
   * @param value - Value to match
   * @returns Found element or undefined
   */
  find(array: any[], property: string, value: any): any {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.find((item) => {
      if (typeof item === "object" && item !== null) {
        return item[property] === value;
      }
      return item === value;
    });
  }

  /**
   * Map array to new array
   * @param array - Input array
   * @param property - Property to extract
   * @returns Mapped array
   */
  map(array: any[], property: string): any[] {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.map((item) => {
      if (typeof item === "object" && item !== null) {
        return item[property];
      }
      return item;
    });
  }

  /**
   * Get unique values from array
   * @param array - Input array
   * @returns Array with unique values
   */
  unique(array: any[]): any[] {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return [...new Set(array)];
  }

  /**
   * Sum numeric array
   * @param array - Input array of numbers
   * @returns Sum of all elements
   */
  sum(array: number[]): number {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    return array.reduce((sum, val) => sum + (Number(val) || 0), 0);
  }

  /**
   * Get average of numeric array
   * @param array - Input array of numbers
   * @returns Average of all elements
   */
  average(array: number[]): number {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (array.length === 0) {
      return 0;
    }
    return this.sum(array) / array.length;
  }

  /**
   * Get minimum value from array
   * @param array - Input array of numbers
   * @returns Minimum value
   */
  min(array: number[]): number {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (array.length === 0) {
      throw new Error("Cannot get min of empty array");
    }
    return Math.min(...array);
  }

  /**
   * Get maximum value from array
   * @param array - Input array of numbers
   * @returns Maximum value
   */
  max(array: number[]): number {
    if (!Array.isArray(array)) {
      throw new Error("First parameter must be an array");
    }
    if (array.length === 0) {
      throw new Error("Cannot get max of empty array");
    }
    return Math.max(...array);
  }
}

/**
 * Global array utility instance
 * Available in Blockly-generated code as: arrayUtility.push(myArray, "item")
 */
export const arrayUtility = new ArrayUtility();
