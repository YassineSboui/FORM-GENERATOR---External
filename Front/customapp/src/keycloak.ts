// src/keycloak.ts
import Keycloak from "keycloak-js";

// Function to get config and construct dynamic URL
const getKeycloakConfig = async () => {
  try {
    const response = await fetch("/config.json");
    const config = await response.json();

    // Extract base URL from API_URL by removing /neoformexternal/
    const apiUrl = config.API_URL;
    const baseUrl = apiUrl.replace("/neoformexternal/", "");

    // Remove trailing slash if exists
    const cleanBaseUrl = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;

    // Extract protocol and hostname, then add the Keycloak port
    const url = new URL(cleanBaseUrl);
    const keycloakUrl = `${url.protocol}//${url.hostname}:${config.KEYCLOAK_PORT}/`;

    return keycloakUrl;
  } catch (error) {
    console.error("Failed to load config, falling back to default URL:", error);
    // Fallback to default URL if config loading fails
    return "https://demo-ecm-prep.elisedemo.com:8443/";
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
