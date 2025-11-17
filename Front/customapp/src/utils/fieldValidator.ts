export function validateByRule(
  value: any,
  rule: { code: string; expression: string },
  fieldLabel?: string
): { valid: boolean; msg: string } {
  if (!rule || typeof rule.code !== "string") return { valid: true, msg: "" };
  const isEmpty = (val: any) => {
    if (val === null || val === undefined) return true;
    if (typeof val === "string" && val.trim() === "") return true;
    if (Array.isArray(val) && val.length === 0) return true;
    if (
      typeof val === "object" &&
      !Array.isArray(val) &&
      Object.keys(val).length === 0
    )
      return true;
    return false;
  };
  const label = fieldLabel || "Ce champ";
  if (isEmpty(value)) {
    if (rule.code === "required") {
      return { valid: false, msg: `${label} est requis.` };
    }
  }
  switch (rule.code) {
    case "required":
      return {
        valid: !isEmpty(value),
        msg: !isEmpty(value) ? "" : `${label} est requis.`,
      };
    case "min": {
      const min = parseInt(
        rule.expression.split(":")[1] || rule.expression,
        10
      );
      if (typeof value === "string" || Array.isArray(value)) {
        return {
          valid: value.length >= min,
          msg:
            value.length >= min
              ? ""
              : `${label} doit contenir au moins ${min} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "max": {
      const max = parseInt(
        rule.expression.split(":")[1] || rule.expression,
        10
      );
      if (typeof value === "string" || Array.isArray(value)) {
        return {
          valid: value.length <= max,
          msg:
            value.length <= max
              ? ""
              : `${label} doit contenir au maximum ${max} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "between": {
      const match = rule.expression.match(/between:(.+) and (.+)/);
      if (match) {
        const min = parseFloat(match[1]);
        const max = parseFloat(match[2]);
        if (typeof value === "number") {
          return {
            valid: value >= min && value <= max,
            msg:
              value >= min && value <= max
                ? ""
                : `${label} doit être entre ${min} et ${max}.`,
          };
        }
        if (typeof value === "string" || Array.isArray(value)) {
          return {
            valid: value.length >= min && value.length <= max,
            msg:
              value.length >= min && value.length <= max
                ? ""
                : `${label} doit contenir entre ${min} et ${max} caractères.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "timeBetween":
    case "dateBetween": {
      const match = rule.expression.match(/between:([\d/: ]+) and ([\d/: ]+)/);
      if (match) {
        const from = match[1].trim();
        const to = match[2].trim();
        let valStr = value;
        if (value instanceof Date) {
          valStr = value.getHours() + ":" + value.getMinutes();
        }
        if (typeof valStr === "string") {
          return {
            valid: valStr >= from && valStr <= to,
            msg:
              valStr >= from && valStr <= to
                ? ""
                : `${label} doit être entre ${from} et ${to}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "confirmed": {
      return { valid: true, msg: "" };
    }
    case "digits": {
      const match = rule.expression.match(/digits:(\d+)/);
      if (match) {
        const len = parseInt(match[1], 10);
        return {
          valid:
            typeof value === "string" &&
            value.length === len &&
            /^\d+$/.test(value),
          msg:
            typeof value === "string" &&
            value.length === len &&
            /^\d+$/.test(value)
              ? ""
              : `${label} doit contenir exactement ${len} chiffres.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "dimensions": {
      return { valid: true, msg: "" };
    }
    case "ext": {
      const match = rule.expression.match(/ext:([\w,]+)/);
      if (match) {
        const allowed = match[1].split(",");
        if (typeof value === "string") {
          const ext = value.split(".").pop();
          return {
            valid: allowed.includes(ext as any),
            msg: allowed.includes(ext as any)
              ? ""
              : `${label} doit avoir l'une des extensions suivantes : ${allowed.join(
                  ", "
                )}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "image": {
      return { valid: true, msg: "" };
    }
    case "integer": {
      return {
        valid: /^-?\d+$/.test(String(value)),
        msg: /^-?\d+$/.test(String(value))
          ? ""
          : `${label} doit être un entier.`,
      };
    }
    case "is": {
      const match = rule.expression.match(/is:(.+)/);
      if (match) {
        return {
          valid: String(value) === match[1],
          msg:
            String(value) === match[1] ? "" : `${label} doit être ${match[1]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "is_not": {
      const match = rule.expression.match(/is_not:(.+)/);
      if (match) {
        return {
          valid: String(value) !== match[1],
          msg:
            String(value) !== match[1]
              ? ""
              : `${label} ne doit pas être ${match[1]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "length": {
      const match = rule.expression.match(/length:(\d+)/);
      if (match) {
        const len = parseInt(match[1], 10);
        return {
          valid:
            (typeof value === "string" || Array.isArray(value)) &&
            value.length === len,
          msg:
            (typeof value === "string" || Array.isArray(value)) &&
            value.length === len
              ? ""
              : `${label} doit contenir exactement ${len} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "max_value": {
      const match = rule.expression.match(/max_value:(\d+)/);
      if (match) {
        const max = parseInt(match[1], 10);
        return {
          valid: typeof value === "number" && value <= max,
          msg:
            typeof value === "number" && value <= max
              ? ""
              : `${label} doit être inférieur ou égal à ${max}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "min_value": {
      const match = rule.expression.match(/min_value:(\d+)/);
      if (match) {
        const min = parseInt(match[1], 10);
        return {
          valid: typeof value === "number" && value >= min,
          msg:
            typeof value === "number" && value >= min
              ? ""
              : `${label} doit être supérieur ou égal à ${min}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "mimes": {
      const match = rule.expression.match(/mimes:([\w,]+)/);
      if (match) {
        const allowed = match[1].split(",");
        if (typeof value === "string") {
          const ext = value.split(".").pop();
          return {
            valid: allowed.includes(ext as any),
            msg: allowed.includes(ext as any)
              ? ""
              : `${label} doit être de l'un des types suivants : ${allowed.join(
                  ", "
                )}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "not_one_of": {
      const match = rule.expression.match(/not_one_of:([^,]+),([^,]+)/);
      if (match) {
        return {
          valid: value !== match[1] && value !== match[2],
          msg:
            value !== match[1] && value !== match[2]
              ? ""
              : `${label} ne doit pas être ${match[1]} ou ${match[2]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "one_of": {
      const match = rule.expression.match(/one_of:([^,]+),([^,]+)/);
      if (match) {
        return {
          valid: value === match[1] || value === match[2],
          msg:
            value === match[1] || value === match[2]
              ? ""
              : `${label} doit être ${match[1]} ou ${match[2]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "regex": {
      let pattern: any = rule.expression;
      let flags = "";

      // If expression is an object with a regex property, extract it
      if (typeof pattern === "object" && pattern !== null && pattern.regex) {
        pattern = pattern.regex;

        // If the extracted regex is a RegExp object, get its source and flags
        if (pattern instanceof RegExp) {
          flags = pattern.flags;
          pattern = pattern.source;
        }
      }

      // Convert to string if needed
      if (typeof pattern !== "string") {
        pattern = String(pattern);
      }

      // Handle string representation of object: "{ regex: /pattern/ }"
      const objectMatch = pattern.match(
        /\{\s*regex:\s*(\/.*?\/[gimsuvy]*)\s*\}/
      );
      if (objectMatch) {
        pattern = objectMatch[1];
      }

      // Strip "regex:" prefix if present and trim whitespace
      if (pattern.startsWith("regex:")) {
        pattern = pattern.substring(6).trim();
      }

      // Handle /pattern/flags format (only if we didn't already extract from RegExp)
      if (!flags) {
        const regexParts = pattern.match(/^\/([^/]+)\/(\w*)$/);
        if (regexParts) {
          pattern = regexParts[1];
          flags = regexParts[2];
        } else if (pattern.startsWith("/") && pattern.endsWith("/")) {
          pattern = pattern.slice(1, -1);
        }
      }

      try {
        const regex = new RegExp(pattern, flags);
        const valid = regex.test(String(value));
        return {
          valid: valid,
          msg: valid ? "" : `${label} a un format invalide.`,
        };
      } catch (e) {
        console.error("Regex validation error:", e, "Pattern:", pattern);
        return { valid: false, msg: `${label} a un format invalide.` };
      }
    }
    case "size": {
      return { valid: true, msg: "" };
    }
    case "url": {
      const valid =
        /^(https?:\/\/)?([\w\-]+\.)+[\w\-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=]*)?$/.test(
          String(value)
        );
      return {
        valid,
        msg: valid ? "" : `${label} doit être une URL valide.`,
      };
    }
    case "timeAfter":
    case "timeBefore": {
      const match = rule.expression.match(/(after|before):(\d{1,2}:\d{1,2})/);
      if (match) {
        const ref = match[2];
        let valStr = value;
        if (value instanceof Date) {
          valStr =
            value.getHours().toString().padStart(2, "0") +
            ":" +
            value.getMinutes().toString().padStart(2, "0");
        }
        if (typeof valStr === "string") {
          if (rule.code.includes("After")) {
            return {
              valid: valStr > ref,
              msg: valStr > ref ? "" : `${label} doit être après ${ref}.`,
            };
          }
          if (rule.code.includes("Before")) {
            return {
              valid: valStr < ref,
              msg: valStr < ref ? "" : `${label} doit être avant ${ref}.`,
            };
          }
        }
      }
      return { valid: true, msg: "" };
    }
    case "dateAfter":
    case "dateAfterToday":
    case "dateBefore":
    case "dateBeforeToday": {
      const match = rule.expression.match(
        /(after|before):(\d{1,2})\/(\d{1,2})\/(\d{4})/
      );
      if (match) {
        const refDay = parseInt(match[2], 10);
        const refMonth = parseInt(match[3], 10) - 1;
        const refYear = parseInt(match[4], 10);
        const refDate = new Date(refYear, refMonth, refDay);
        let valDate;
        if (value instanceof Date) {
          valDate = value;
        } else if (
          typeof value === "string" &&
          /^\d{4}-\d{2}-\d{2}/.test(value)
        ) {
          valDate = new Date(value);
        } else if (
          typeof value === "string" &&
          /\d{1,2}\/\d{1,2}\/\d{4}/.test(value)
        ) {
          const [d, m, y] = value.split("/").map(Number);
          valDate = new Date(y, m - 1, d);
        }
        if (valDate instanceof Date && !isNaN(valDate.getTime())) {
          if (rule.code.includes("After")) {
            return {
              valid: valDate > refDate,
              msg:
                valDate > refDate
                  ? ""
                  : `${label} doit être après ${match[0].split(":")[1]}.`,
            };
          }
          if (rule.code.includes("Before")) {
            return {
              valid: valDate < refDate,
              msg:
                valDate < refDate
                  ? ""
                  : `${label} doit être avant ${match[0].split(":")[1]}.`,
            };
          }
        }
      }
      return { valid: true, msg: "" };
    }
    case "dateIsNot": {
      return { valid: true, msg: "" };
    }
    case "disabledDateRange":
    case "disabledMonthDays":
    case "disabledWeekDays": {
      return { valid: true, msg: "" };
    }
    case "email": {
      const valid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit être une adresse e-mail valide.`,
      };
    }
    case "numeric": {
      const valid = /^-?\d*(\.\d+)?$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit être un nombre.`,
      };
    }
    case "alpha": {
      const valid = /^[A-Za-z]+$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit contenir uniquement des lettres.`,
      };
    }
    case "alpha_num": {
      const valid = /^[A-Za-z0-9]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres et des chiffres.`,
      };
    }
    case "alpha_dash": {
      const valid = /^[A-Za-z0-9_-]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres, des chiffres, des tirets ou des underscores.`,
      };
    }
    case "alpha_spaces": {
      const valid = /^[A-Za-z\s]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres et des espaces.`,
      };
    }
    default:
      return { valid: true, msg: "" };
  }
}
