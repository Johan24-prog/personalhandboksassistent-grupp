import { askAssistant } from "./api.js";

// Hanterar frågeformuläret, svaret och källornas namn i konversationen.
export function initializeChat() {
  const conversation = document.querySelector("#conversation");
  const form = document.querySelector("#chat-form");
  const input = document.querySelector("#question-input");
  let isSending = false;

  document.querySelectorAll(".suggestion").forEach((button) => {
    button.addEventListener("click", () => {
      input.value = button.dataset.question;
      form.requestSubmit();
    });
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const question = input.value.trim();
    if (!question || isSending) return;

    isSending = true;

    appendMessage(conversation, question, "user-message");
    input.value = "";
    input.style.height = "auto";
    const pending = appendPendingMessage(conversation);
    const sendButton = form.querySelector(".send-button");
    sendButton.disabled = true;

    try {
      const result = await askAssistant(question);
      pending.remove();
      appendMessage(
        conversation,
        result.answer || "Jag kunde inte formulera ett svar från underlaget.",
        "assistant-message",
        result.sources || [],
      );
    } catch (error) {
      pending.remove();
      appendMessage(conversation, error.message, "assistant-message");
    } finally {
    isSending = false;
    sendButton.disabled = false;
    input.focus();
    }
  });

  input.addEventListener("input", () => {
    input.style.height = "auto";
    input.style.height = `${Math.min(input.scrollHeight, 110)}px`;
  });
  input.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      form.requestSubmit();
    }
  });
}

function appendMessage(conversation, text, className, sources = []) {
  document.querySelector("#welcome-message")?.remove();
  document.querySelector("#suggestion-label")?.remove();
  document.querySelector("#suggestions")?.remove();

  const message = document.createElement("div");
  message.className = `chat-message ${className}`;
  const body = document.createElement("div");
  body.className = "message-body";
  body.textContent = text;

  if (sources.length) {
    const sourceList = document.createElement("div");
    sourceList.className = "message-sources";
    sources.forEach((source) => {
      const chip = document.createElement("span");
      chip.className = "message-source";
      chip.textContent = source;
      sourceList.append(chip);
    });
    body.append(sourceList);
  }

  message.append(body);
  conversation.append(message);
  message.scrollIntoView({ block: "nearest", behavior: "smooth" });
}

function appendPendingMessage(conversation) {
  const message = document.createElement("div");
  message.className = "chat-message assistant-message";
  const body = document.createElement("div");
  body.className = "message-body typing-indicator";
  body.textContent = "Letar i handboken…";
  message.append(body);
  conversation.append(message);
  message.scrollIntoView({ block: "nearest", behavior: "smooth" });
  return message;
}
