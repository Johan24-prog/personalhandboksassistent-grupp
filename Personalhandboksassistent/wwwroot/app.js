import { getServiceStatus } from "./api.js";
import { initializeChat } from "./chat.js";
import { initializeDocuments } from "./documents.js";

const views = [
  ["navigation-mount", "/partials/navigation.html"],
  ["overview-view-mount", "/partials/overview-view.html"],
  ["assistant-view-mount", "/partials/assistant-view.html"],
  ["documents-view-mount", "/partials/documents-view.html"],
  ["sources-view-mount", "/partials/sources-view.html"],
  ["dialogs-mount", "/partials/delete-dialog.html"],
];

// Ladda vyerna innan modulerna söker efter element eller kopplar händelser.
await Promise.all(views.map(loadView));
initializeDocuments();
initializeChat();
await updateServiceStatus();
initializeNavigation();

async function loadView([mountId, path]) {
  const response = await fetch(path);
  if (!response.ok) throw new Error(`Kunde inte läsa vyn ${path}.`);
  document.querySelector(`#${mountId}`).innerHTML = await response.text();
}

async function updateServiceStatus() {
  const systemLabel = document.querySelector("#system-label");
  const systemLight = document.querySelector("#system-light");
  const overviewText = document.querySelector("#overview-connection-text");
  const overviewLight = document.querySelector("#overview-connection-light");

  try {
    const status = await getServiceStatus();
    setServiceState("storage-state", status.storageConfigured);
    setServiceState("search-state", status.searchConfigured);
    setServiceState("openai-state", status.openAiConfigured);
    document.querySelector("#openai-state").textContent =
      status.openAiConfigured
        ? status.chatDeployment || "Konfigurerad"
        : "Saknas";
    systemLabel.textContent = status.openAiConfigured
      ? "AI-anslutning konfigurerad"
      : "AI-anslutning saknas";
    systemLight.classList.toggle("is-offline", !status.openAiConfigured);
    overviewText.textContent = status.openAiConfigured
      ? "Konfigurerad"
      : "Saknas";
    overviewLight.classList.toggle("is-offline", !status.openAiConfigured);
  } catch {
    systemLabel.textContent = "API kan inte nås";
    systemLight.classList.add("is-offline");
    overviewText.textContent = "Ej tillgänglig";
    overviewLight.classList.add("is-offline");
  }
}

function setServiceState(id, configured) {
  const state = document.querySelector(`#${id}`);
  state.textContent = configured ? "Konfigurerad" : "Saknas";
  state.classList.toggle("is-configured", configured);
  state.classList.toggle("is-missing", !configured);
}

function initializeNavigation() {
  const sections = {
    overview: document.querySelector("#overview-view"),
    assistant: document.querySelector("#assistant-view"),
    documents: document.querySelector("#documents-view"),
    sources: document.querySelector("#sources-view"),
  };
  const sectionLabels = {
    overview: "Översikt",
    assistant: "Assistent",
    documents: "Dokument",
    sources: "Källor & status",
  };
  const currentSection = document.querySelector("#current-section");
  const links = [...document.querySelectorAll("[data-view]")];

  function showView(view) {
    if (!sections[view]) view = "overview";
    Object.entries(sections).forEach(([name, section]) => {
      section.classList.toggle("is-hidden", name !== view);
    });
    currentSection.textContent = sectionLabels[view];
    links.forEach((link) => {
      const active = link.dataset.view === view;
      link.classList.toggle("is-active", active);
      if (active) link.setAttribute("aria-current", "page");
      else link.removeAttribute("aria-current");
    });
    window.location.hash = view;
  }

  links.forEach((link) => {
    link.addEventListener("click", () => showView(link.dataset.view));
  });
  document.querySelectorAll("[data-go]").forEach((button) => {
    button.addEventListener("click", () => showView(button.dataset.go));
  });
  document.querySelector(".brand").addEventListener("click", (event) => {
    event.preventDefault();
    showView("overview");
  });

  showView(window.location.hash.replace("#", "") || "overview");
}
