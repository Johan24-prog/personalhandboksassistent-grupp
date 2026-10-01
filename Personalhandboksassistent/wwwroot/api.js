// Samlar HTTP-detaljer så att vyerna kan fokusera på sitt eget beteende.
async function requestJson(url, options) {
  const response = await fetch(url, options);
  if (response.status === 204) return null;

  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(data.error || "Ett fel uppstod i API-anropet.");
  }

  return data;
}

export function getServiceStatus() {
  return requestJson("/api/status");
}

export function listDocuments() {
  return requestJson("/api/documents");
}

export function saveDocument(file, documentId = null) {
  const body = new FormData();
  body.append("file", file);

  return requestJson(
    documentId
      ? `/api/documents/${encodeURIComponent(documentId)}`
      : "/api/documents",
    { method: documentId ? "PUT" : "POST", body },
  );
}

export function removeDocument(documentId) {
  return requestJson(`/api/documents/${encodeURIComponent(documentId)}`, {
    method: "DELETE",
  });
}

export function askAssistant(question) {
  return requestJson("/api/chat", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ question }),
  });
}
