// src/keycloak.ts
import Keycloak from "keycloak-js";

// Function to get config and construct dynamic URL
const getKeycloakConfig = async () => {
  try {
    const response = await fetch("/config.json");

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const config = await response.json();

    // Extract base URL from API_URL by removing the API path
    const apiUrl = config.API_URL;
    let baseUrl;

    if (apiUrl.includes("/neoformext/neoformexternal/")) {
      baseUrl = apiUrl.replace("/neoformext/neoformexternal/", "");
    } else if (apiUrl.includes("/neoformexternal/")) {
      baseUrl = apiUrl.replace("/neoformexternal/", "");
    } else {
      // Fallback: remove everything after the domain
      const url = new URL(apiUrl);
      baseUrl = `${url.protocol}//${url.hostname}`;
    }

    // Remove trailing slash if exists
    const cleanBaseUrl = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;

    // Extract protocol and hostname, then add the Keycloak port
    const url = new URL(cleanBaseUrl);
    const keycloakUrl = `${url.protocol}//${url.hostname}:${config.KEYCLOAK_PORT}/`;

    return keycloakUrl;
  } catch (error) {
    console.error("Failed to load config, falling back to default URL:", error);
    // Fallback to production URL if config loading fails
    return "https://neoform01.elisecloud.tn:8443/";
  }
};

// Get the dynamic URL and create Keycloak instance
const keycloakUrl = await getKeycloakConfig();
console.log("Using Keycloak URL:", keycloakUrl);
const keycloak = new Keycloak({
  url: keycloakUrl,
  realm: "NeoFormExt",
  clientId: "NeoFormExt",
});

export default keycloak;
