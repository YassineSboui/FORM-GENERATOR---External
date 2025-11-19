export function validateByRule(
  value: any,
  rule: { code: string; expression: string },
  fieldLabel?: string,
  lang: "fr" | "ar" = "fr"
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

  const t = (fr: string, ar: string) => (lang === "ar" ? ar : fr);

  const label = fieldLabel || (lang === "ar" ? "هذا الحقل" : "Ce champ");

  if (isEmpty(value)) {
    if (rule.code === "required") {
      return {
        valid: false,
        msg: t(`${label} est requis.`, `${label} حقل إجباري.`),
      };
    }
  }

  switch (rule.code) {
    case "required":
      return {
        valid: !isEmpty(value),
        msg: !isEmpty(value)
          ? ""
          : t(`${label} est requis.`, `${label} حقل إجباري.`),
      };

    case "min": {
      const min = parseInt(
        rule.expression.split(":")[1] || rule.expression,
        10
      );
      if (typeof value === "string" || Array.isArray(value)) {
        const ok = value.length >= min;
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit contenir au moins ${min} caractères.`,
                `${label} يجب أن يحتوي على الأقل على ${min} حروف.`
              ),
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
        const ok = value.length <= max;
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit contenir au maximum ${max} caractères.`,
                `${label} يجب أن يحتوي على الأكثر على ${max} حروف.`
              ),
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
          const ok = value >= min && value <= max;
          return {
            valid: ok,
            msg: ok
              ? ""
              : t(
                  `${label} doit être entre ${min} et ${max}.`,
                  `${label} يجب أن يكون بين ${min} و ${max}.`
                ),
          };
        }
        if (typeof value === "string" || Array.isArray(value)) {
          const ok = value.length >= min && value.length <= max;
          return {
            valid: ok,
            msg: ok
              ? ""
              : t(
                  `${label} doit contenir entre ${min} et ${max} caractères.`,
                  `${label} يجب أن يحتوي على ما بين ${min} و ${max} حروف.`
                ),
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
          const ok = valStr >= from && valStr <= to;
          return {
            valid: ok,
            msg: ok
              ? ""
              : t(
                  `${label} doit être entre ${from} et ${to}.`,
                  `${label} يجب أن يكون بين ${from} و ${to}.`
                ),
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
        const ok =
          typeof value === "string" &&
          value.length === len &&
          /^\d+$/.test(value);
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit contenir exactement ${len} chiffres.`,
                `${label} يجب أن يحتوي على ${len} أرقام بالضبط.`
              ),
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
          const ok = allowed.includes(ext as any);
          return {
            valid: ok,
            msg: ok
              ? ""
              : t(
                  `${label} doit avoir l'une des extensions suivantes : ${allowed.join(
                    ", "
                  )}.`,
                  `${label} يجب أن يكون امتداده واحدًا من الامتدادات التالية: ${allowed.join(
                    ", "
                  )}.`
                ),
          };
        }
      }
      return { valid: true, msg: "" };
    }

    case "image": {
      return { valid: true, msg: "" };
    }

    case "integer": {
      const ok = /^-?\d+$/.test(String(value));
      return {
        valid: ok,
        msg: ok
          ? ""
          : t(
              `${label} doit être un entier.`,
              `${label} يجب أن يكون عددًا صحيحًا.`
            ),
      };
    }

    case "is": {
      const match = rule.expression.match(/is:(.+)/);
      if (match) {
        const ok = String(value) === match[1];
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit être ${match[1]}.`,
                `${label} يجب أن يكون ${match[1]}.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "is_not": {
      const match = rule.expression.match(/is_not:(.+)/);
      if (match) {
        const ok = String(value) !== match[1];
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} ne doit pas être ${match[1]}.`,
                `${label} لا يجب أن يكون ${match[1]}.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "length": {
      const match = rule.expression.match(/length:(\d+)/);
      if (match) {
        const len = parseInt(match[1], 10);
        const ok =
          (typeof value === "string" || Array.isArray(value)) &&
          value.length === len;
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit contenir exactement ${len} caractères.`,
                `${label} يجب أن يحتوي على ${len} حروف بالضبط.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "max_value": {
      const match = rule.expression.match(/max_value:(\d+)/);
      if (match) {
        const max = parseInt(match[1], 10);
        const ok = typeof value === "number" && value <= max;
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit être inférieur ou égal à ${max}.`,
                `${label} يجب أن يكون أقل من أو يساوي ${max}.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "min_value": {
      const match = rule.expression.match(/min_value:(\d+)/);
      if (match) {
        const min = parseInt(match[1], 10);
        const ok = typeof value === "number" && value >= min;
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit être supérieur ou égal à ${min}.`,
                `${label} يجب أن يكون أكبر من أو يساوي ${min}.`
              ),
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
          const ok = allowed.includes(ext as any);
          return {
            valid: ok,
            msg: ok
              ? ""
              : t(
                  `${label} doit être de l'un des types suivants : ${allowed.join(
                    ", "
                  )}.`,
                  `${label} يجب أن يكون من الأنواع التالية: ${allowed.join(
                    ", "
                  )}.`
                ),
          };
        }
      }
      return { valid: true, msg: "" };
    }

    case "not_one_of": {
      const match = rule.expression.match(/not_one_of:([^,]+),([^,]+)/);
      if (match) {
        const ok = value !== match[1] && value !== match[2];
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} ne doit pas être ${match[1]} ou ${match[2]}.`,
                `${label} لا يجب أن يكون ${match[1]} أو ${match[2]}.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "one_of": {
      const match = rule.expression.match(/one_of:([^,]+),([^,]+)/);
      if (match) {
        const ok = value === match[1] || value === match[2];
        return {
          valid: ok,
          msg: ok
            ? ""
            : t(
                `${label} doit être ${match[1]} ou ${match[2]}.`,
                `${label} يجب أن يكون ${match[1]} أو ${match[2]}.`
              ),
        };
      }
      return { valid: true, msg: "" };
    }

    case "regex": {
      let pattern: any = rule.expression;
      let flags = "";

      if (typeof pattern === "object" && pattern !== null && pattern.regex) {
        pattern = pattern.regex;

        if (pattern instanceof RegExp) {
          flags = pattern.flags;
          pattern = pattern.source;
        }
      }

      if (typeof pattern !== "string") {
        pattern = String(pattern);
      }

      const objectMatch = pattern.match(
        /\{\s*regex:\s*(\/.*?\/[gimsuvy]*)\s*\}/
      );
      if (objectMatch) {
        pattern = objectMatch[1];
      }

      if (pattern.startsWith("regex:")) {
        pattern = pattern.substring(6).trim();
      }

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
          valid,
          msg: valid
            ? ""
            : t(`${label} a un format invalide.`, `${label} تنسيقه غير صحيح.`),
        };
      } catch (e) {
        console.error("Regex validation error:", e, "Pattern:", pattern);
        return {
          valid: false,
          msg: t(`${label} a un format invalide.`, `${label} تنسيقه غير صحيح.`),
        };
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
        msg: valid
          ? ""
          : t(
              `${label} doit être une URL valide.`,
              `${label} يجب أن يكون رابطًا (URL) صالحًا.`
            ),
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
            const ok = valStr > ref;
            return {
              valid: ok,
              msg: ok
                ? ""
                : t(
                    `${label} doit être après ${ref}.`,
                    `${label} يجب أن يكون بعد ${ref}.`
                  ),
            };
          }
          if (rule.code.includes("Before")) {
            const ok = valStr < ref;
            return {
              valid: ok,
              msg: ok
                ? ""
                : t(
                    `${label} doit être avant ${ref}.`,
                    `${label} يجب أن يكون قبل ${ref}.`
                  ),
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
        let valDate: Date | undefined;

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
          const refStr = match[0].split(":")[1];
          if (rule.code.includes("After")) {
            const ok = valDate > refDate;
            return {
              valid: ok,
              msg: ok
                ? ""
                : t(
                    `${label} doit être après ${refStr}.`,
                    `${label} يجب أن يكون بعد ${refStr}.`
                  ),
            };
          }
          if (rule.code.includes("Before")) {
            const ok = valDate < refDate;
            return {
              valid: ok,
              msg: ok
                ? ""
                : t(
                    `${label} doit être avant ${refStr}.`,
                    `${label} يجب أن يكون قبل ${refStr}.`
                  ),
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
        msg: valid
          ? ""
          : t(
              `${label} doit être une adresse e-mail valide.`,
              `${label} يجب أن يكون بريدًا إلكترونيًا صالحًا.`
            ),
      };
    }

    case "numeric": {
      const valid = /^-?\d*(\.\d+)?$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : t(`${label} doit être un nombre.`, `${label} يجب أن يكون رقمًا.`),
      };
    }

    case "alpha": {
      const valid = /^[A-Za-z]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : t(
              `${label} doit contenir uniquement des lettres.`,
              `${label} يجب أن يحتوي على حروف فقط.`
            ),
      };
    }

    case "alpha_num": {
      const valid = /^[A-Za-z0-9]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : t(
              `${label} doit contenir uniquement des lettres et des chiffres.`,
              `${label} يجب أن يحتوي على حروف وأرقام فقط.`
            ),
      };
    }

    case "alpha_dash": {
      const valid = /^[A-Za-z0-9_-]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : t(
              `${label} doit contenir uniquement des lettres, des chiffres, des tirets ou des underscores.`,
              `${label} يجب أن يحتوي فقط على حروف، أرقام، شرطات (-)، أو شرطات سفلية (_).`
            ),
      };
    }

    case "alpha_spaces": {
      const valid = /^[A-Za-z\s]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : t(
              `${label} doit contenir uniquement des lettres et des espaces.`,
              `${label} يجب أن يحتوي فقط على حروف ومسافات.`
            ),
      };
    }

    default:
      return { valid: true, msg: "" };
  }
}
