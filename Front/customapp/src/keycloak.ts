// src/keycloak.ts
import Keycloak from "keycloak-js";
const keycloak = new Keycloak({
  // url localhost 8080
  // url: "https://integration01.elisecloud.tn:8443/",
  // url :"https://demo-ecm-prep.elisedemo.com:8443/"
  url: "https://demo-ecm-prep.elisedemo.com:8443/",
  realm: "NeoFormExt",
  clientId: "NeoFormExt",
});

export default keycloak;
