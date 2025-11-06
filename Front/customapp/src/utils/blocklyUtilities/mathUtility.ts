/**
 * Math Utility Functions
 * 
 * Provides safe mathematical operations for Blockly-generated code.
 * All functions are validated and bounded.
 * 
 * @module mathUtility
 */

import { logger } from "@/api/api";

/**
 * Math Utility Class
 * Provides safe mathematical operations
 */
export class MathUtility {
  
  add(a: number, b: number): number {
    return Number(a) + Number(b);
  }

  subtract(a: number, b: number): number {
    return Number(a) - Number(b);
  }

  multiply(a: number, b: number): number {
    return Number(a) * Number(b);
  }

  divide(a: number, b: number): number {
    const divisor = Number(b);
    if (divisor === 0) {
      throw new Error('Division by zero');
    }
    return Number(a) / divisor;
  }

  modulo(a: number, b: number): number {
    const divisor = Number(b);
    if (divisor === 0) {
      throw new Error('Modulo by zero');
    }
    return Number(a) % divisor;
  }

  power(base: number, exponent: number): number {
    const result = Math.pow(Number(base), Number(exponent));
    if (!isFinite(result)) {
      throw new Error('Power operation resulted in infinity');
    }
    return result;
  }

  sqrt(value: number): number {
    const num = Number(value);
    if (num < 0) {
      throw new Error('Cannot get square root of negative number');
    }
    return Math.sqrt(num);
  }

  abs(value: number): number {
    return Math.abs(Number(value));
  }

  round(value: number): number {
    return Math.round(Number(value));
  }

  floor(value: number): number {
    return Math.floor(Number(value));
  }

  ceil(value: number): number {
    return Math.ceil(Number(value));
  }

  roundToDecimals(value: number, decimals: number): number {
    const places = Math.min(Number(decimals), 10);
    const multiplier = Math.pow(10, places);
    return Math.round(Number(value) * multiplier) / multiplier;
  }

  min(a: number, b: number): number {
    return Math.min(Number(a), Number(b));
  }

  max(a: number, b: number): number {
    return Math.max(Number(a), Number(b));
  }

  constrain(value: number, min: number, max: number): number {
    return Math.max(Number(min), Math.min(Number(max), Number(value)));
  }

  randomInt(min: number, max: number): number {
    const minVal = Math.ceil(Number(min));
    const maxVal = Math.floor(Number(max));
    return Math.floor(Math.random() * (maxVal - minVal + 1)) + minVal;
  }

  random(min: number, max: number): number {
    const minVal = Number(min);
    const maxVal = Number(max);
    return Math.random() * (maxVal - minVal) + minVal;
  }

  isEven(value: number): boolean {
    return Number(value) % 2 === 0;
  }

  isOdd(value: number): boolean {
    return Number(value) % 2 !== 0;
  }

  isPositive(value: number): boolean {
    return Number(value) > 0;
  }

  isNegative(value: number): boolean {
    return Number(value) < 0;
  }

  isZero(value: number): boolean {
    return Number(value) === 0;
  }

  isNumber(value: any): boolean {
    return !isNaN(Number(value)) && isFinite(Number(value));
  }

  toInt(value: number): number {
    return parseInt(String(value), 10);
  }

  toFloat(value: number): number {
    return parseFloat(String(value));
  }

  sign(value: number): number {
    return Math.sign(Number(value));
  }

  percentage(value: number, total: number): number {
    const totalNum = Number(total);
    if (totalNum === 0) {
      return 0;
    }
    return (Number(value) / totalNum) * 100;
  }

  fromPercentage(percentage: number, total: number): number {
    return (Number(percentage) / 100) * Number(total);
  }

  sin(degrees: number): number {
    return Math.sin((Number(degrees) * Math.PI) / 180);
  }

  cos(degrees: number): number {
    return Math.cos((Number(degrees) * Math.PI) / 180);
  }

  tan(degrees: number): number {
    return Math.tan((Number(degrees) * Math.PI) / 180);
  }

  ln(value: number): number {
    const num = Number(value);
    if (num <= 0) {
      throw new Error('Cannot get logarithm of non-positive number');
    }
    return Math.log(num);
  }

  log10(value: number): number {
    const num = Number(value);
    if (num <= 0) {
      throw new Error('Cannot get logarithm of non-positive number');
    }
    return Math.log10(num);
  }

  exp(exponent: number): number {
    const result = Math.exp(Number(exponent));
    if (!isFinite(result)) {
      throw new Error('Exponential operation resulted in infinity');
    }
    return result;
  }

  toRadians(degrees: number): number {
    return (Number(degrees) * Math.PI) / 180;
  }

  toDegrees(radians: number): number {
    return (Number(radians) * 180) / Math.PI;
  }

  pi(): number {
    return Math.PI;
  }

  e(): number {
    return Math.E;
  }
}

export const mathUtility = new MathUtility();
