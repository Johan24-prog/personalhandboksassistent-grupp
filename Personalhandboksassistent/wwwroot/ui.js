// Små hjälpfunktioner som används av flera vyer.
export function showAlert(message, success = false) {
  const alert = document.querySelector("#inline-alert");
  clearTimeout(showAlert.timer);
  alert.textContent = message;
  alert.classList.toggle("success", success);
  alert.classList.remove("is-hidden");
  showAlert.timer = setTimeout(() => alert.classList.add("is-hidden"), 5000);
}

export function formatBytes(size) {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(0)} KB`;
  return `${(size / (1024 * 1024)).toFixed(1)} MB`;
}

export function formatDate(value) {
  return new Intl.DateTimeFormat("sv-SE", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(value));
}

export function escapeHtml(value) {
  const replacements = {
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;",
  };
  return String(value).replace(
    /[&<>"']/g,
    (character) => replacements[character],
  );
}
