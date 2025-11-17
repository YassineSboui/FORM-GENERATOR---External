import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";
import type { Ref } from "vue";
import { isRef } from "vue";
import { logger, logBlockly } from "@/api/api";

/**
 * Context available to executed code
 */
export interface ExecutionContext {
  store?: ReturnType<typeof useAppStore>;
  Fields?: Ref<any>;
  [key: string]: any;
}

/**
 * Options for code execution
 */
export interface ExecutionOptions {
  /**
   * Context object to pass to the executed code
   */
  context?: ExecutionContext;

  /**
   * Whether to wrap code in async IIFE
   * @default true
   */
  asyncWrapper?: boolean;

  /**
   * Timeout in milliseconds
   * @default 5000
   */
  timeout?: number;

  /**
   * Whether to log execution
   * @default true
   */
  enableLogging?: boolean;

  /**
   * Whether to skip input validation (use with caution!)
   * @default false
   */
  skipValidation?: boolean;
}

/**
 * Result of code validation
 */
export interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
}

/**
 * Code Executor Helper
 *
 * Centralized helper to execute dynamic code with better security controls
 * This replaces direct eval() calls throughout the application
 *
 * @example
 * ```typescript
 * const executor = new CodeExecutor();
 * await executor.execute('console.log("Hello")', { store: useAppStore() });
 * ```
 */
export class CodeExecutor {
  private get defaultTimeout(): number {
    // Get timeout from httpRequest store (loaded from config.json)
    const httpRequest = useHttpRequest();
    return httpRequest.executionTimeout;
  }
  private executionCount = 0;

  /**
   * Dangerous patterns that should be blocked
   * Note: Using 'i' flag only (case-insensitive), NOT 'g' flag
   * The 'g' flag causes .test() to maintain state between calls, leading to false negatives
   */
  private readonly dangerousPatterns = [
    // Direct code execution
    {
      pattern: /\beval\s*\(/i,
      severity: "critical",
      message: "Direct eval() usage is not allowed",
    },
    {
      pattern: /\bnew\s+Function\s*\(/i,
      severity: "critical",
      message: "Function constructor usage is not allowed",
    },
    {
      pattern: /\bsetTimeout\s*\(\s*['"`]/i,
      severity: "critical",
      message: "setTimeout with string is not allowed",
    },
    {
      pattern: /\bsetInterval\s*\(\s*['"`]/i,
      severity: "critical",
      message: "setInterval with string is not allowed",
    },

    // Prototype manipulation
    {
      pattern: /\.__proto__/i,
      severity: "critical",
      message: "__proto__ manipulation is not allowed",
    },
    {
      pattern: /\.prototype\s*=/i,
      severity: "critical",
      message: "Prototype assignment is not allowed",
    },
    {
      pattern: /Object\.setPrototypeOf/i,
      severity: "critical",
      message: "setPrototypeOf is not allowed",
    },

    // Constructor access (potential bypass)
    {
      pattern: /\.constructor\s*\(/i,
      severity: "warning",
      message: "Constructor access detected",
    },
    {
      pattern: /\['constructor'\]/i,
      severity: "warning",
      message: "String-based constructor access detected",
    },

    // Potential XSS vectors
    {
      pattern: /document\.write\s*\(/i,
      severity: "critical",
      message: "document.write is not allowed",
    },
    {
      pattern: /\.innerHTML\s*=/i,
      severity: "warning",
      message: "innerHTML assignment detected (potential XSS)",
    },
    {
      pattern: /\.outerHTML\s*=/i,
      severity: "warning",
      message: "outerHTML assignment detected (potential XSS)",
    },

    // Import/require (module loading)
    {
      pattern: /\bimport\s*\(/i,
      severity: "critical",
      message: "Dynamic import is not allowed",
    },
    {
      pattern: /\brequire\s*\(/i,
      severity: "critical",
      message: "require() is not allowed",
    },

    // Global object access (potential security issues)
    {
      pattern: /\bglobalThis\./i,
      severity: "warning",
      message: "globalThis access detected",
    },
    {
      pattern: /\bwindow\s*\[\s*['"`]/i,
      severity: "warning",
      message: "String-based window access detected",
    },
    {
      pattern: /fetch\s*\(/i,
      severity: "critical",
      message: "Network request detected",
    },
    {
      pattern: /XMLHttpRequest/i,
      severity: "critical",
      message: "XMLHttpRequest usage detected",
    },
    {
      pattern: /localStorage/i,
      severity: "critical",
      message: "localStorage access detected",
    },
    {
      pattern: /sessionStorage/i,
      severity: "critical",
      message: "sessionStorage access detected",
    },
    {
      pattern: /cookie/i,
      severity: "critical",
      message: "Cookie access detected",
    },
    {
      pattern: /location\s*=/i,
      severity: "critical",
      message: "Navigation attempt detected",
    },
    {
      pattern: /location\.href/i,
      severity: "critical",
      message: "URL manipulation detected",
    },
    {
      pattern: /location\.replace/i,
      severity: "critical",
      message: "Navigation attempt detected",
    },
    {
      pattern: /window\.open/i,
      severity: "critical",
      message: "Window.open detected",
    },
    {
      pattern: /\bwindow\.close\s*\(/i,
      severity: "critical",
      message: "window.close detected",
    },
    {
      pattern: /\bself\.close\s*\(/i,
      severity: "critical",
      message: "self.close detected",
    },
  ];

  /**
   * Additional patterns to block in custom code blocks only
   * These prevent users from accessing internal utility functions
   */
  private readonly customCodeBlockedPatterns = [
    // Utility access patterns
    {
      pattern: /\bfieldUtility\./i,
      severity: "critical",
      message: "fieldUtility access is not allowed in custom code",
    },
    {
      pattern: /\beliseUtility\./i,
      severity: "critical",
      message: "eliseUtility access is not allowed in custom code",
    },
    {
      pattern: /\bstringUtility\./i,
      severity: "critical",
      message: "stringUtility access is not allowed in custom code",
    },
    {
      pattern: /\bmathUtility\./i,
      severity: "critical",
      message: "mathUtility access is not allowed in custom code",
    },
    {
      pattern: /\barrayUtility\./i,
      severity: "critical",
      message: "arrayUtility access is not allowed in custom code",
    },
    {
      pattern: /\bsectionUtility\./i,
      severity: "critical",
      message: "sectionUtility access is not allowed in custom code",
    },

    // Store direct access
    {
      pattern: /\bstore\./i,
      severity: "critical",
      message: "Direct store access is not allowed in custom code",
    },
    {
      pattern: /\buseAppStore/i,
      severity: "critical",
      message: "useAppStore access is not allowed in custom code",
    },

    // App refs access (should use utilities instead)
    {
      pattern: /\bapp\.refs\./i,
      severity: "critical",
      message: "Direct app.refs access is not allowed in custom code",
    },

    // Internal function access attempts
    {
      pattern: /\binitFields/i,
      severity: "critical",
      message: "initFields access is not allowed in custom code",
    },
    {
      pattern: /\bvalidatePageFields/i,
      severity: "critical",
      message: "validatePageFields access is not allowed in custom code",
    },
    {
      pattern: /\bexecuteCodeAsync/i,
      severity: "critical",
      message: "executeCodeAsync access is not allowed in custom code",
    },
    {
      pattern: /\bexecuteCode\b/i,
      severity: "critical",
      message: "executeCode access is not allowed in custom code",
    },
  ];

  /**
   * Suspicious patterns that should trigger warnings
   */
  //   private readonly suspiciousPatterns = [
  //     { pattern: /fetch\s*\(/gi, message: 'Network request detected' },
  //     { pattern: /XMLHttpRequest/gi, message: 'XMLHttpRequest usage detected' },
  //     { pattern: /localStorage/gi, message: 'localStorage access detected' },
  //     { pattern: /sessionStorage/gi, message: 'sessionStorage access detected' },
  //     { pattern: /cookie/gi, message: 'Cookie access detected' },
  //     { pattern: /location\s*=/gi, message: 'Navigation attempt detected' },
  //     { pattern: /location\.href/gi, message: 'URL manipulation detected' },
  //     { pattern: /location\.replace/gi, message: 'Navigation attempt detected' },
  //     { pattern: /window\.open/gi, message: 'Window.open detected' },
  //   ];

  /**
   * Detect if code contains custom code block markers
   * @param code - The code to check
   * @returns True if code contains custom code markers
   */
  private hasCustomCodeMarkers(code: string): boolean {
    return (
      code.includes("/*__CUSTOM_CODE_START__*/") &&
      code.includes("/*__CUSTOM_CODE_END__*/")
    );
  }

  /**
   * Extract custom code sections from marked code
   * @param code - The code containing markers
   * @returns Array of custom code sections
   */
  private extractCustomCodeSections(code: string): string[] {
    const sections: string[] = [];
    const regex =
      /\/\*__CUSTOM_CODE_START__\*\/([\s\S]*?)\/\*__CUSTOM_CODE_END__\*\//g;
    let match;

    while ((match = regex.exec(code)) !== null) {
      sections.push(match[1]);
    }

    return sections;
  }

  /**
   * Validate code for dangerous patterns
   *
   * @param code - The code to validate
   * @returns Validation result with errors and warnings
   */
  validateCode(code: string): ValidationResult {
    const errors: string[] = [];
    const warnings: string[] = [];

    if (!code || typeof code !== "string") {
      errors.push("Code must be a non-empty string");
      return { isValid: false, errors, warnings };
    }

    // Check if code contains custom code blocks
    const hasCustomCode = this.hasCustomCodeMarkers(code);
    const customCodeSections = hasCustomCode
      ? this.extractCustomCodeSections(code)
      : [];

    // Validate each custom code section with stricter rules
    if (hasCustomCode && customCodeSections.length > 0) {
      for (const customCode of customCodeSections) {
        // Apply strict validation to custom code sections
        for (const { pattern, severity, message } of this
          .customCodeBlockedPatterns) {
          if (pattern.test(customCode)) {
            if (severity === "critical") {
              errors.push(`[Custom Code] ${message}`);
            } else {
              warnings.push(`[Custom Code] ${message}`);
            }
          }
        }
      }
    }

    // Check for dangerous patterns in all code
    for (const { pattern, severity, message } of this.dangerousPatterns) {
      if (pattern.test(code)) {
        if (severity === "critical") {
          errors.push(message);
        } else {
          warnings.push(message);
        }
      }
    }

    return {
      isValid: errors.length === 0,
      errors,
      warnings,
    };
  }

  /**
   * Execute dynamic code with controlled context
   *
   * @param code - The code string to execute
   * @param options - Execution options including context
   * @returns Promise resolving to the execution result
   */
  async execute(code: string, options: ExecutionOptions = {}): Promise<any> {
    const {
      context = {},
      asyncWrapper = true,
      timeout = this.defaultTimeout,
      enableLogging = true,
      skipValidation = false,
    } = options;

    // Validate input
    if (!code || typeof code !== "string") {
      throw new Error("Code must be a non-empty string");
    }
    // Validate code for dangerous patterns (unless explicitly skipped)
    if (!skipValidation) {
      const validation = this.validateCode(code);

      // Log warnings
      if (validation.warnings.length > 0 && enableLogging) {
        logger.warn(
          `[CodeExecutor] Security warnings:\n${validation.warnings.join("\n")}`
        );
      }

      // Throw error if validation failed
      if (!validation.isValid) {
        const errorMessage = `Code validation failed:\n${validation.errors.join(
          "\n"
        )}`;
        logger.error(`[CodeExecutor] ${errorMessage}`);
        throw new Error(errorMessage);
      }
    }

    // Log execution attempt
    this.executionCount++;
    if (enableLogging) {
      logger.debug(`[CodeExecutor] Execution #${this.executionCount}`);
    }

    try {
      // Create timeout promise
      const timeoutPromise = new Promise((_, reject) => {
        setTimeout(() => reject(new Error("Code execution timeout")), timeout);
      });

      // Execute code with timeout
      const executionPromise = this._executeCode(code, context, asyncWrapper);

      const result = await Promise.race([executionPromise, timeoutPromise]);

      if (enableLogging) {
        logger.debug(
          `[CodeExecutor] Execution #${this.executionCount} completed successfully`
        );
      }

      return result;
    } catch (error) {
      if (enableLogging) {
        logger.error(
          `[CodeExecutor] Execution #${this.executionCount} failed: ${error}`
        );
      }
      throw error;
    }
  }

  /**
   * Internal method to execute code
   * Uses Function constructor instead of eval for better security
   * Code is wrapped in strict mode for additional safety
   */
  private async _executeCode(
    code: string,
    context: ExecutionContext,
    asyncWrapper: boolean
  ): Promise<any> {
    // Make store available in scope
    const store = context.store || useAppStore();

    // Prepare execution context: wrap Vue refs with proxies so executed code
    // can access properties directly (e.g., User.displayName = 'x') and have
    // mutations reflected back into the original ref.
    const prepareContext = (ctx: ExecutionContext) => {
      const out: Record<string, any> = {};
      for (const [k, v] of Object.entries(ctx)) {
        if (isRef(v)) {
          // Create a proxy that maps property access to the inner ref value
          const refObj = v as Ref<any>;

          const proxy = new Proxy(
            {},
            {
              get(_t, prop: string | symbol) {
                if (prop === "__isRefProxy") return true;
                if (prop === "value") return refObj.value;
                // Forward function calls or nested objects as-is
                const val = refObj.value
                  ? (refObj.value as any)[prop as any]
                  : undefined;
                return val;
              },
              set(_t, prop: string | symbol, value) {
                if (prop === "value") {
                  refObj.value = value;
                  return true;
                }
                // Ensure the inner value is an object before setting properties
                if (refObj.value == null || typeof refObj.value !== "object") {
                  // replace primitive inner value with an object to hold properties
                  refObj.value = {} as any;
                }
                (refObj.value as any)[prop as any] = value;
                return true;
              },
              has(_t, prop: string | symbol) {
                if (prop === "value") return true;
                return refObj.value ? prop in (refObj.value as any) : false;
              },
              ownKeys() {
                return refObj.value ? Object.keys(refObj.value) : [];
              },
              getOwnPropertyDescriptor() {
                return {
                  configurable: true,
                  enumerable: true,
                } as PropertyDescriptor;
              },
            }
          );

          out[k] = proxy;
        } else {
          out[k] = v;
        }
      }
      // Helper to replace a ref entirely: setRef('User', { displayName: 'x' })
      out.__setRef = (refName: string, value: any) => {
        const orig = (ctx as any)[refName];
        if (isRef(orig)) {
          orig.value = value;
          return true;
        }
        // fallback: set on prepared object
        out[refName] = value;
        return false;
      };

      // Helper to set a single property on a ref's inner value
      out.__setRefProp = (refName: string, prop: string, value: any) => {
        const orig = (ctx as any)[refName];
        if (isRef(orig)) {
          if (orig.value == null || typeof orig.value !== "object")
            orig.value = {} as any;
          (orig.value as any)[prop] = value;
          return true;
        }
        // fallback
        if (!out[refName] || typeof out[refName] !== "object")
          out[refName] = {};
        (out[refName] as any)[prop] = value;
        return false;
      };
      return out;
    };

    const preparedContext = prepareContext(context || {});

    // Extract context keys and values
    const contextKeys = Object.keys(preparedContext);
    const contextValues = Object.values(preparedContext);

    // Wrap code in strict mode for additional security
    const strictCode = `'use strict';\n${code}`;

    if (asyncWrapper) {
      // Create async function with context parameters
      // Function constructor is safer than eval - it doesn't have access to local scope
      const AsyncFunction = async function () {}
        .constructor as FunctionConstructor;

      // Create function with context keys as parameters
      const fn = AsyncFunction(...contextKeys, strictCode) as (
        ...args: any[]
      ) => Promise<any>;

      // Execute with context values
      return await fn(...contextValues);
    } else {
      // Create synchronous function with context parameters
      const fn = Function(...contextKeys, strictCode) as (
        ...args: any[]
      ) => any;

      // Execute with context values
      return fn(...contextValues);
    }
  }

  /**
   * Execute code with store context (common use case)
   *
   * @param code - The code string to execute
   * @returns Promise resolving to the execution result
   */
  async executeWithStore(code: string): Promise<any> {
    const store = useAppStore();
    return this.execute(code, {
      context: { store },
      asyncWrapper: true,
    });
  }

  /**
   * Get execution statistics
   */
  getStats() {
    return {
      totalExecutions: this.executionCount,
    };
  }

  /**
   * Reset execution counter
   */
  resetStats() {
    this.executionCount = 0;
  }
}

/**
 * Global singleton instance
 */
export const codeExecutor = new CodeExecutor();

/**
 * Convenience function to execute code with store context
 * This is the most common use case in the application
 *
 * @param code - The code string to execute
 * @param additionalContext - Additional context to pass to the code
 * @returns Promise resolving to the execution result
 *
 * @example
 * ```typescript
 * await executeCode('console.log(store.Fields.value)');
 * ```
 */
export async function executeCode(
  code: string,
  additionalContext: Record<string, any> = {}
): Promise<any> {
  const store = useAppStore();
  return codeExecutor.execute(code, {
    context: { store, logBlockly, ...additionalContext },
    asyncWrapper: true,
  });
}

/**
 * Execute code wrapped in async IIFE with store
 * Mimics the current pattern: eval("(async () => { const store = useAppStore(); " + code + "})()")
 *
 * @param code - The code string to execute
 * @param additionalContext - Additional context variables (like app, Fields, etc.)
 * @returns Promise resolving to the execution result
 *
 * @example
 * ```typescript
 * await executeCodeAsync('Fields.value.total = 100');
 * await executeCodeAsync('app.refs.CF_DATE[0].hideField()', { app });
 * ```
 */
export async function executeCodeAsync(
  code: string,
  additionalContext: Record<string, any> = {}
): Promise<any> {
  return executeCode(code, additionalContext);
}
