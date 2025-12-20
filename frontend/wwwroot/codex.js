// ================================================================================
// codex.js
// A simple web interface for rendering story sections and options.
// This is the JS side for WebRunner.cs
// ================================================================================

const sectionTitle = document.querySelector("header");
const sectionMain = document.querySelector("main");
const optionsList = document.querySelector("nav");
const dialog = document.querySelector("dialog");
const menu = document.querySelector("menu");

// Renders a story section with title, text, and options.
// This is imported and called from the WebAssembly module.
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

  const storyGlobalsJson = globalThis.wasmExports.WebRunner.GetGlobalsWrapper();
  const storyGlobals = JSON.parse(storyGlobalsJson);
  menu.innerHTML = `<li><b>Name:</b> ${storyGlobals.player_name}</li>`;
  menu.innerHTML += `<li><b>Stamina:</b> ${storyGlobals.stamina}</li>`;
  menu.innerHTML += `<li><b>Skill:</b> ${storyGlobals.skill}</li>`;
  menu.innerHTML += `<li><b>Luck:</b> ${storyGlobals.luck}</li>`;
  menu.innerHTML += `<li><b>Gold:</b> ${storyGlobals.gold}</li>`;
  menu.innerHTML += `<li><b>Carrying:</b> ${storyGlobals.bag.join(", ")}</li>`;
}

// Displays a notification dialog with the given message.
// This is imported and called from the WebAssembly module.
export function notify(message) {
  dialog.querySelector("p").innerText = message;
  dialog.showModal();
}

// Utility function to create a readable title from a section ID.
function makeTitleFromId(id) {
  return id
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(" ");
}
