import { listDocuments, removeDocument, saveDocument } from "./api.js";
import { escapeHtml, formatBytes, formatDate, showAlert } from "./ui.js";

const fileLabels = {
  PDF: "PDF",
  DOC: "DOC",
  DOCX: "DOC",
  MD: "MD",
  TXT: "TXT",
};

// Kopplar dokumentvyn till API:t och uppdaterar listor och räknare.
export function initializeDocuments() {
  const elements = {
    list: document.querySelector("#document-list"),
    total: document.querySelector("#document-total"),
    navCount: document.querySelector("#nav-document-count"),
    readableCount: document.querySelector("#readable-count"),
    overviewTotal: document.querySelector("#overview-document-count"),
    overviewReadable: document.querySelector("#overview-readable-count"),
    sources: document.querySelector("#assistant-source-list"),
    fileInput: document.querySelector("#file-input"),
    uploadPanel: document.querySelector("#upload-panel"),
    uploadProgress: document.querySelector("#upload-progress"),
    deleteDialog: document.querySelector("#delete-dialog"),
    deleteCopy: document.querySelector("#delete-copy"),
    confirmDelete: document.querySelector("#confirm-delete"),
  };
  let documents = [];
  let pendingDelete = null;

  async function refresh() {
    try {
      documents = await listDocuments();
      renderDocuments(elements, documents);
    } catch (error) {
      showAlert(error.message);
    }
  }

  async function upload(file, replaceId = null) {
    const extension = file.name.split(".").pop().toLowerCase();
    if (!["pdf", "doc", "docx", "md", "txt"].includes(extension)) {
      showAlert("Välj en PDF-, Word-, Markdown- eller textfil.");
      return;
    }
    if (file.size > 20 * 1024 * 1024) {
      showAlert("Filen får vara högst 20 MB.");
      return;
    }

    elements.uploadProgress.classList.remove("is-hidden");
    try {
      await saveDocument(file, replaceId);
      showAlert(
        replaceId ? "Dokumentet har ersatts." : "Dokumentet har lagts till.",
        true,
      );
      await refresh();
    } catch (error) {
      showAlert(error.message);
    } finally {
      elements.uploadProgress.classList.add("is-hidden");
    }
  }

  elements.uploadPanel.addEventListener("click", (event) => {
    if (!event.target.closest("button")) elements.fileInput.click();
  });
  document
    .querySelector("#browse-button")
    .addEventListener("click", (event) => {
      event.stopPropagation();
      elements.fileInput.click();
    });
  elements.fileInput.addEventListener("change", async () => {
    if (!elements.fileInput.files.length) return;
    const replaceId = elements.fileInput.dataset.replaceId || null;
    delete elements.fileInput.dataset.replaceId;
    await upload(elements.fileInput.files[0], replaceId);
    elements.fileInput.value = "";
  });

  elements.uploadPanel.addEventListener("dragover", (event) => {
    event.preventDefault();
    elements.uploadPanel.classList.add("is-dragging");
  });
  elements.uploadPanel.addEventListener("dragleave", (event) => {
    if (!elements.uploadPanel.contains(event.relatedTarget)) {
      elements.uploadPanel.classList.remove("is-dragging");
    }
  });
  elements.uploadPanel.addEventListener("drop", async (event) => {
    event.preventDefault();
    elements.uploadPanel.classList.remove("is-dragging");
    const [file] = event.dataTransfer.files;
    if (file) await upload(file);
  });

  elements.list.addEventListener("click", (event) => {
    const action = event.target.closest("[data-action]");
    if (!action) return;
    const selected = documents.find((item) => item.id === action.dataset.id);
    if (!selected) return;

    if (action.dataset.action === "replace") {
      elements.fileInput.dataset.replaceId = selected.id;
      elements.fileInput.click();
    } else if (action.dataset.action === "delete") {
      pendingDelete = selected;
      elements.deleteCopy.textContent = `${selected.name} tas bort från biblioteket.`;
      elements.deleteDialog.showModal();
    }
  });

  document.querySelector("#cancel-delete").addEventListener("click", () => {
    elements.deleteDialog.close();
  });
  document.querySelector(".dialog-close").addEventListener("click", () => {
    elements.deleteDialog.close();
  });
  elements.confirmDelete.addEventListener("click", async () => {
    if (!pendingDelete) return;
    elements.confirmDelete.disabled = true;
    try {
      await removeDocument(pendingDelete.id);
      elements.deleteDialog.close();
      showAlert("Dokumentet har tagits bort.", true);
      await refresh();
    } catch (error) {
      showAlert(error.message);
    } finally {
      elements.confirmDelete.disabled = false;
      pendingDelete = null;
    }
  });

  refresh();
}

function renderDocuments(elements, documents) {
  const ordered = [...documents].sort(
    (first, second) => new Date(second.updatedAt) - new Date(first.updatedAt),
  );
  const readable = ordered.filter((item) => item.searchable);
  elements.total.textContent = ordered.length;
  elements.navCount.textContent = ordered.length;
  elements.readableCount.textContent = readable.length;
  elements.overviewTotal.textContent = ordered.length;
  elements.overviewReadable.textContent = readable.length;

  elements.list.innerHTML = ordered.length
    ? ordered.map(renderDocument).join("")
    : '<div class="empty-library"><span class="empty-library-mark">▤</span><strong>Biblioteket är tomt</strong><p>Ladda upp ett dokument för att komma igång.</p></div>';
  elements.sources.innerHTML = readable.length
    ? readable.map(renderSource).join("")
    : '<div class="empty-source">Lägg till dokument för att ge assistenten underlag.</div>';
}

function renderDocument(item) {
  const type = escapeHtml(item.type.toLowerCase());
  const status = item.searchable
    ? '<span class="readable-tag">Läsbart underlag</span>'
    : '<span class="pending-tag">Kräver AI Search</span>';
  const name = escapeHtml(item.name);
  return `<article class="document-row">
    <div class="document-info"><span class="document-file-icon type-${type}">${fileLabels[item.type] || type.toUpperCase()}</span>
      <div class="document-text"><strong class="document-name" title="${name}">${name}</strong>
        <div class="document-meta"><span>${formatBytes(item.size)}</span><span>·</span><span>${formatDate(item.updatedAt)}</span><span>·</span>${status}</div>
      </div>
    </div>
    <div class="document-actions"><button class="icon-button" type="button" title="Ersätt ${name}" aria-label="Ersätt ${name}" data-action="replace" data-id="${escapeHtml(item.id)}">↻</button><button class="icon-button delete-button" type="button" title="Ta bort ${name}" aria-label="Ta bort ${name}" data-action="delete" data-id="${escapeHtml(item.id)}">×</button></div>
  </article>`;
}

function renderSource(item) {
  const type = escapeHtml(item.type.toLowerCase());
  const label = fileLabels[item.type] || escapeHtml(item.type);
  const name = escapeHtml(item.name);
  return `<div class="source-item"><span class="file-type type-${type}">${label}</span><span class="source-name" title="${name}">${name}</span></div>`;
}
