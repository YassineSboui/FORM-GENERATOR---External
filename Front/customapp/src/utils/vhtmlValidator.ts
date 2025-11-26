// src/utils/VhtmlValidator.ts

export interface VhtmlValidationResult {
  valid: boolean;
  errors: string[];
  warnings: string[];
}

/**
 * Validateur côté client pour analyser le contenu destiné à être rendu via v-html.
 *
 * Objectif : bloquer les charges vraiment dangereuses (XSS évidentes),
 * tout en laissant passer le style, les animations, etc.
 *
 * ⚠️ Ce contrôle ne remplace PAS une sanisation côté serveur.
 */
export function validateVHtml(content: string): VhtmlValidationResult {
  const errors: string[] = [];
  const warnings: string[] = [];

  if (!content || !content.trim()) {
    return {
      valid: true,
      errors,
      warnings,
    };
  }

  // 1) Balises clairement dangereuses (XSS / injections)
  const forbiddenTags = ["script", "iframe", "object", "embed", "link", "meta"];
  for (const tag of forbiddenTags) {
    const tagRegex = new RegExp(`<\\/?\\s*${tag}\\b`, "i");
    if (tagRegex.test(content)) {
      errors.push(`Balise interdite <${tag}> détectée.`);
    }
  }

  // 2) Gestionnaires d’événements inline : onclick=, onload=, onerror=, etc.
  const eventHandlerRegex = /\son[a-z]+\s*=/i;
  if (eventHandlerRegex.test(content)) {
    errors.push(
      "Les gestionnaires d'événements inline (onClick, onLoad, ...) sont interdits."
    );
  }

  // 3) URLs en javascript: (classique XSS)
  const javascriptUrlRegex = /javascript\s*:/i;
  if (javascriptUrlRegex.test(content)) {
    errors.push("L'utilisation de 'javascript:' dans une URL est interdite.");
  }

  // 4) URLs en data: dans src/href → juste un warning (suspect mais pas forcément interdit)
  const dataUrlRegex = /\s(?:src|href)\s*=\s*["']?\s*data:/i;
  if (dataUrlRegex.test(content)) {
    warnings.push(
      "Des URL en data: ont été détectées dans src/href. Vérifiez qu'elles sont sûres."
    );
  }

  // 5) (AUCUN contrôle sur style, animations, etc.)
  //    Tu peux écrire du CSS librement dans style= ou dans des <style>, keyframes, transitions, etc.
  //    Cependant: block global root selectors inside <style> because they affect the whole app.
  //    We consider selectors targeting `html`, `body` or `:root` in <style> blocks as unsafe.
  const styleTagRegex = /<style[^>]*>([\s\S]*?)<\/style>/gi;
  let styleMatch: RegExpExecArray | null;
  while ((styleMatch = styleTagRegex.exec(content))) {
    const styleContent = styleMatch[1] || "";
    // Detect use of html, body or :root selectors (as standalone selectors)
    const globalSelectorRegex =
      /(^|[^a-zA-Z0-9_-])(:root|html|body)([^a-zA-Z0-9_-]|$)/i;
    if (globalSelectorRegex.test(styleContent)) {
      errors.push(
        "Le contenu CSS contient des sélecteurs globaux (html/body/:root) dans une balise <style> — interdits."
      );
      break;
    }
    // Also detect selectors that start with these names followed by combinators or commas
    const globalSelectorStartRegex =
      /(?:^|\s)(html|body|:root)\s*[\>\+\~\,\{\.\[:]/i;
    if (globalSelectorStartRegex.test(styleContent)) {
      errors.push(
        "Le contenu CSS contient des sélecteurs globaux (html/body/:root) dans une balise <style> — interdits."
      );
      break;
    }
  }

  // 6) Optionnel : très gros contenu → juste un warning de perf
  const MAX_LENGTH = 20000;
  if (content.length > MAX_LENGTH) {
    warnings.push(
      `Le contenu est très volumineux (${content.length} caractères). Cela peut impacter les performances ou la stabilité de l’éditeur.`
    );
  }

  return {
    valid: errors.length === 0,
    errors,
    warnings,
  };
}
