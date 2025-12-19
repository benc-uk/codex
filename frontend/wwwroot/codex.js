const sectionTitle = document.querySelector("header");
const sectionMain = document.querySelector("main");
const optionsList = document.querySelector("nav");
const dialog = document.querySelector("dialog");

export function renderSection(id, text, title, optionIds, optionTexts) {
  sectionTitle.innerText = title ? title : makeTitleFromId(id);
  sectionMain.innerText = text;

  optionsList.innerHTML = ""; // Clear existing options

  for (let i = 0; i < optionIds.length; i++) {
    const div = document.createElement("div");
    div.onclick = () =>
      globalThis.wasmExports.WebRunner.TakeOption(optionIds[i]);
    div.innerText = optionTexts[i];

    optionsList.appendChild(div);
  }
}

export function notify(message) {
  dialog.querySelector("p").innerText = message;
  dialog.showModal();
}

function makeTitleFromId(id) {
  return id
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(" ");
}
